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

public class DiscussionPostBody
{
    public string CourseId { get; set; } = "";
    public string? ClassInstanceId { get; set; }
    public string? ReplyId { get; set; }
    public bool IsPublic { get; set; } = true;
    public string Content { get; set; } = "";
    public string Lang { get; set; } = "en";
}

[ApiController]
[Route("api/discussion")]
public class DiscussionController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [RequireAuth]
    [HttpGet("list")]
    public async Task<ListResult<object>> ListDiscussions(
        [FromQuery] string? courseId,
        [FromQuery] string? classInstanceId)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        var filterBuilder = Builders<CourseDiscussion>.Filter;
        var filter = filterBuilder.Eq(x => x.Deleted, false);

        if (!string.IsNullOrWhiteSpace(courseId))
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.CourseId, courseId));

        if (!string.IsNullOrWhiteSpace(classInstanceId))
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.ClassInstanceId, classInstanceId));

        // Non-faculty users can only see public discussions
        if (user.Group < DbUser.Role.Faculty)
            filter = filterBuilder.And(filter, filterBuilder.Eq(x => x.IsPublic, true));

        var list = await DbService.Wrapper.CourseDiscussions
            .Find(filter)
            .SortByDescending(x => x.CreateTime)
            .ToListAsync();

        return new ListResult<object>(list.Cast<object>().ToList()) { TotalCount = list.Count };
    }

    [RequireAuth]
    [HttpPost("post")]
    public async Task<object> PostDiscussion([FromBody] DiscussionPostBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        var discussion = new CourseDiscussion
        {
            CourseId = body.CourseId,
            SenderId = user.Id,
            ClassInstanceId = body.ClassInstanceId,
            ReplyId = body.ReplyId,
            IsPublic = body.IsPublic,
        };
        discussion.Content[body.Lang] = body.Content;
        discussion.SetCreateTime();

        var id = await DbService.Wrapper.CourseDiscussions.InsertAsync(null, discussion);
        return new { id };
    }

    [RequireAuth]
    [HttpDelete("delete/{id}")]
    public async Task DeleteDiscussion(string id)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        var discussion = await DbService.Wrapper.CourseDiscussions.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        // Only own post or Admin can delete
        if (discussion.SenderId != user.Id && user.Group != DbUser.Role.Admin)
            throw new ApiException((int)HttpStatusCode.Forbidden);

        using var session = await DbService.Wrapper.Client.StartSessionAsync();
        await DbService.Wrapper.CourseDiscussions.DeleteAsync(session, discussion);
    }
}
