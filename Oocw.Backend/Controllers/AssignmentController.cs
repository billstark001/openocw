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

public class SubmitAssignmentBody
{
    public string ClassInstanceId { get; set; } = "";
    public string ContentId { get; set; } = "";
    public List<AssignmentSubmission.Content> Contents { get; set; } = [];
}

public class GradeBody
{
    public string SubmissionId { get; set; } = "";
    public int Grade { get; set; }
    public string Comment { get; set; } = "";
}

[ApiController]
[Route("api/assignment")]
public class AssignmentController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [RequireAuth]
    [HttpGet("list")]
    public async Task<ListResult<object>> ListAssignments([FromQuery] string classInstanceId)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        var instance = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, classInstanceId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        // Check access: enrolled student or faculty
        if (user.Group < DbUser.Role.Faculty)
        {
            var enrolled = await DbService.Wrapper.CourseSelections
                .Find(x => x.StudentId == user.Id
                    && x.ClassInstanceId == classInstanceId
                    && x.CurrentStatus == CourseSelection.Status.Approval
                    && !x.Deleted)
                .FirstOrDefaultAsync();
            if (enrolled == null)
                throw new ApiException((int)HttpStatusCode.Forbidden);
        }

        var assignments = instance.Contents
            .Where(c => c.Type == ClassInstance.ContentType.Assignment)
            .Cast<object>()
            .ToList();

        return new ListResult<object>(assignments) { TotalCount = assignments.Count };
    }

    [RequireAuth]
    [HttpPost("submit")]
    public async Task<object> Submit([FromBody] SubmitAssignmentBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        var existing = await DbService.Wrapper.AssignmentSubmissions
            .Find(x => x.StudentId == user.Id
                && x.ClassInstanceId == body.ClassInstanceId
                && x.ContentId == body.ContentId
                && !x.Deleted)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            existing.History.Add(existing.Contents);
            existing.Contents = body.Contents;
            existing.SetUpdateTime();
            await DbService.Wrapper.AssignmentSubmissions.ReplaceOneAsync(x => x.Id == existing.Id, existing);
            return new { id = existing.Id };
        }

        var submission = new AssignmentSubmission
        {
            ClassInstanceId = body.ClassInstanceId,
            ContentId = body.ContentId,
            StudentId = user.Id,
            Contents = body.Contents,
        };
        submission.SetCreateTime();

        var id = await DbService.Wrapper.AssignmentSubmissions.InsertAsync(null, submission);
        return new { id };
    }

    [RequireAuth]
    [HttpGet("submissions")]
    public async Task<ListResult<object>> ListSubmissions(
        [FromQuery] string classInstanceId,
        [FromQuery] string contentId)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var filter = Builders<AssignmentSubmission>.Filter.And(
            Builders<AssignmentSubmission>.Filter.Eq(x => x.ClassInstanceId, classInstanceId),
            Builders<AssignmentSubmission>.Filter.Eq(x => x.ContentId, contentId),
            Builders<AssignmentSubmission>.Filter.Eq(x => x.Deleted, false));

        var list = await DbService.Wrapper.AssignmentSubmissions
            .Find(filter)
            .ToListAsync();

        return new ListResult<object>(list.Cast<object>().ToList()) { TotalCount = list.Count };
    }

    [RequireAuth]
    [HttpGet("my-submission")]
    public async Task<object> GetMySubmission(
        [FromQuery] string classInstanceId,
        [FromQuery] string contentId)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        var submission = await DbService.Wrapper.AssignmentSubmissions
            .Find(x => x.StudentId == user.Id
                && x.ClassInstanceId == classInstanceId
                && x.ContentId == contentId
                && !x.Deleted)
            .FirstOrDefaultAsync()
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        return submission;
    }

    [RequireAuth]
    [HttpPost("grade")]
    public async Task Grade([FromBody] GradeBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var submission = await DbService.Wrapper.AssignmentSubmissions.FindByIdAsync(null, body.SubmissionId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        await DbService.Wrapper.AssignmentSubmissions.UpdateOneAsync(
            x => x.Id == submission.Id,
            Builders<AssignmentSubmission>.Update
                .Set(x => x.Grade, body.Grade)
                .Set(x => x.GraderComment, body.Comment)
                .Set(x => x.GradedBy, user.Id)
                .Set(x => x.UpdateTime, DateTime.UtcNow));

        var notification = new Notification(
            submission.StudentId,
            new MultiLingualField { ["en"] = $"Your assignment has been graded: {body.Grade}/100." });
        await DbService.Wrapper.Notifications.InsertOneAsync(notification);
    }
}
