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

public class ReviewActionBody
{
    public string RequestId { get; set; } = "";
    public string? Comment { get; set; }
}

[ApiController]
[Route("api/review")]
public class ReviewController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [RequireAuth]
    [HttpGet("queue")]
    public async Task<ListResult<object>> GetQueue([FromQuery] PaginationParams pagination)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Viewer);
        pagination ??= new();
        pagination.Sanitize();

        var filter = Builders<UpdateRequest>.Filter.Eq(x => x.Status, UpdateRequest.RequestStatus.Pending);
        var total = await DbService.Wrapper.UpdateRequests.CountDocumentsAsync(filter);
        var list = await DbService.Wrapper.UpdateRequests
            .Find(filter)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        int ps = pagination.PageSize > 0 ? pagination.PageSize : 1;
        return new ListResult<object>(list.Cast<object>().ToList())
        {
            TotalCount = (int)total,
            TotalPage = (int)Math.Ceiling((double)total / ps),
        };
    }

    [RequireAuth]
    [HttpPost("approve")]
    public async Task Approve([FromBody] ReviewActionBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Viewer);

        var request = await DbService.Wrapper.UpdateRequests
            .Find(x => x.SystemId == body.RequestId)
            .FirstOrDefaultAsync()
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        await DbService.Wrapper.UpdateRequests.UpdateOneAsync(
            x => x.SystemId == body.RequestId,
            Builders<UpdateRequest>.Update
                .Set(x => x.Status, UpdateRequest.RequestStatus.Approved)
                .Set(x => x.ReviewerId, user.Id)
                .Set(x => x.ReviewTime, DateTime.UtcNow)
                .Set(x => x.ReviewComment, body.Comment));

        // TODO: apply the patch in request.Patch to the target collection (request.TargetCollection / request.TargetObjectId)
    }

    [RequireAuth]
    [HttpPost("reject")]
    public async Task Reject([FromBody] ReviewActionBody body)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Viewer);

        _ = await DbService.Wrapper.UpdateRequests
            .Find(x => x.SystemId == body.RequestId)
            .FirstOrDefaultAsync()
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        await DbService.Wrapper.UpdateRequests.UpdateOneAsync(
            x => x.SystemId == body.RequestId,
            Builders<UpdateRequest>.Update
                .Set(x => x.Status, UpdateRequest.RequestStatus.Rejected)
                .Set(x => x.ReviewerId, user.Id)
                .Set(x => x.ReviewTime, DateTime.UtcNow)
                .Set(x => x.ReviewComment, body.Comment));
    }
}
