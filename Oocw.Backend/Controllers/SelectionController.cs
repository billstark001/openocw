using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Oocw.Backend.Api;
using Oocw.Backend.Auth;
using Oocw.Backend.Models;
using Oocw.Backend.Services;
using Oocw.Backend.Utils;
using Oocw.Database.Models;
using DbUser = Oocw.Database.Models.User;
using Oocw.Database.Models.Technical;

namespace Oocw.Backend.Controllers;

public class ApplyBody { public string ClassInstanceId { get; set; } = ""; }
public class SelectionActionBody { public string SelectionId { get; set; } = ""; }

[ApiController]
[Route("api/selection")]
public class SelectionController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [RequireAuth]
    [HttpGet("available")]
    public async Task<ListResult<object>> GetAvailable([FromQuery] PaginationParams pagination)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Student);
        pagination ??= new();
        pagination.Sanitize();

        var settings = await DbService.Wrapper.GlobalSettings
            .Find(x => x.Key == "global")
            .FirstOrDefaultAsync();

        var filter = settings != null
            ? Builders<ClassInstance>.Filter.And(
                Builders<ClassInstance>.Filter.Eq(x => x.Deleted, false),
                Builders<ClassInstance>.Filter.Eq(x => x.Address.Year, settings.CurrentYear),
                Builders<ClassInstance>.Filter.BitsAnySet(x => x.Address.Term, settings.CurrentTerm))
            : Builders<ClassInstance>.Filter.Eq(x => x.Deleted, false);

        var total = await DbService.Wrapper.ClassInstances.CountDocumentsAsync(filter);
        var list = await DbService.Wrapper.ClassInstances
            .Find(filter)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        var result = list.Cast<object>().ToList();
        int pageSize = pagination.PageSize > 0 ? pagination.PageSize : 1;
        return new ListResult<object>(result)
        {
            TotalCount = (int)total,
            TotalPage = (int)Math.Ceiling((double)total / pageSize),
        };
    }

    [RequireAuth]
    [HttpGet("my")]
    public async Task<ListResult<object>> GetMySelections()
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        var list = await DbService.Wrapper.CourseSelections
            .Find(x => x.StudentId == user.Id && !x.Deleted)
            .ToListAsync();

        var result = list.Cast<object>().ToList();
        return new ListResult<object>(result) { TotalCount = result.Count };
    }

    [RequireAuth]
    [HttpPost("apply")]
    public async Task<object> Apply([FromBody] ApplyBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        _ = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, body.ClassInstanceId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        var existing = await DbService.Wrapper.CourseSelections
            .Find(x => x.StudentId == user.Id && x.ClassInstanceId == body.ClassInstanceId && !x.Deleted)
            .FirstOrDefaultAsync();

        if (existing != null)
            throw new ApiException((int)HttpStatusCode.Conflict);

        var selection = new CourseSelection
        {
            StudentId = user.Id,
            ClassInstanceId = body.ClassInstanceId,
            CurrentStatus = CourseSelection.Status.Application,
        };
        selection.SetCreateTime();

        var id = await DbService.Wrapper.CourseSelections.InsertAsync(null, selection);
        return new { id };
    }

    [RequireAuth]
    [HttpPost("approve")]
    public async Task Approve([FromBody] SelectionActionBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var selection = await DbService.Wrapper.CourseSelections.FindByIdAsync(null, body.SelectionId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        await DbService.Wrapper.CourseSelections.UpdateOneAsync(
            x => x.Id == selection.Id,
            Builders<CourseSelection>.Update
                .Set(x => x.CurrentStatus, CourseSelection.Status.Approval)
                .Set(x => x.UpdateTime, DateTime.UtcNow));

        var notification = new Notification(
            selection.StudentId,
            new MultiLingualField { ["en"] = "Your enrolment application has been approved." });
        await DbService.Wrapper.Notifications.InsertOneAsync(notification);
    }

    [RequireAuth]
    [HttpPost("reject")]
    public async Task Reject([FromBody] SelectionActionBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var selection = await DbService.Wrapper.CourseSelections.FindByIdAsync(null, body.SelectionId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        await DbService.Wrapper.CourseSelections.UpdateOneAsync(
            x => x.Id == selection.Id,
            Builders<CourseSelection>.Update
                .Set(x => x.CurrentStatus, CourseSelection.Status.Dismissal)
                .Set(x => x.UpdateTime, DateTime.UtcNow));

        var notification = new Notification(
            selection.StudentId,
            new MultiLingualField { ["en"] = "Your enrolment application has been rejected." });
        await DbService.Wrapper.Notifications.InsertOneAsync(notification);
    }

    [RequireAuth]
    [HttpPost("withdraw")]
    public async Task Withdraw([FromBody] SelectionActionBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        var selection = await DbService.Wrapper.CourseSelections.FindByIdAsync(null, body.SelectionId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        if (selection.StudentId != user.Id)
            throw new ApiException((int)HttpStatusCode.Forbidden);

        await DbService.Wrapper.CourseSelections.UpdateOneAsync(
            x => x.Id == selection.Id,
            Builders<CourseSelection>.Update
                .Set(x => x.CurrentStatus, CourseSelection.Status.Closure)
                .Set(x => x.UpdateTime, DateTime.UtcNow));
    }
}
