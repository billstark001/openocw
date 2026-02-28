using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Oocw.Backend.Api;
using Oocw.Backend.Auth;
using Oocw.Backend.Services;
using Oocw.Backend.Utils;
using Oocw.Database.Models;
using DbUser = Oocw.Database.Models.User;
using Oocw.Database.Models.Technical;

namespace Oocw.Backend.Controllers;

public class SubmitFeedbackBody
{
    public string ClassInstanceId { get; set; } = "";
    public int Rating { get; set; } = 3;
    public string Comment { get; set; } = "";
}

[ApiController]
[Route("api/feedback")]
public class FeedbackController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [RequireAuth]
    [HttpPost("submit")]
    public async Task<object> SubmitFeedback([FromBody] SubmitFeedbackBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Student);

        if (body.Rating < 1 || body.Rating > 5)
            throw new ApiException((int)HttpStatusCode.BadRequest);

        _ = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, body.ClassInstanceId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        var existing = await DbService.Wrapper.Feedbacks
            .Find(x => x.StudentId == user.Id && x.ClassInstanceId == body.ClassInstanceId && !x.Deleted)
            .FirstOrDefaultAsync();

        if (existing != null)
        {
            await DbService.Wrapper.Feedbacks.UpdateOneAsync(
                x => x.Id == existing.Id,
                Builders<Feedback>.Update
                    .Set(x => x.Rating, body.Rating)
                    .Set(x => x.Comment, body.Comment)
                    .Set(x => x.UpdateTime, DateTime.UtcNow));
            return new { id = existing.Id };
        }

        var feedback = new Feedback
        {
            StudentId = user.Id,
            ClassInstanceId = body.ClassInstanceId,
            Rating = body.Rating,
            Comment = body.Comment,
        };
        feedback.SetCreateTime();

        var id = await DbService.Wrapper.Feedbacks.InsertAsync(null, feedback);
        return new { id };
    }

    [RequireAuth]
    [HttpGet("summary")]
    public async Task<object> GetSummary([FromQuery] string classInstanceId)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        if (string.IsNullOrWhiteSpace(classInstanceId))
            throw new ApiException((int)HttpStatusCode.BadRequest);

        var list = await DbService.Wrapper.Feedbacks
            .Find(x => x.ClassInstanceId == classInstanceId && !x.Deleted)
            .ToListAsync();

        if (list.Count == 0)
            return new { count = 0, averageRating = 0.0 };

        var avg = list.Average(f => f.Rating);
        var dist = list.GroupBy(f => f.Rating)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.Count());

        return new
        {
            count = list.Count,
            averageRating = Math.Round(avg, 2),
            distribution = dist,
        };
    }
}
