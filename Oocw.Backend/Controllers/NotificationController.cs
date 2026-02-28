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

namespace Oocw.Backend.Controllers;

[ApiController]
[Route("api/notification")]
public class NotificationController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [RequireAuth]
    [HttpGet("list")]
    public async Task<ListResult<object>> ListNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var user = HttpContext.GetAuthenticatedUser();

        pageSize = Math.Clamp(pageSize, 5, 100);
        page = Math.Max(1, page);

        var filter = Builders<Notification>.Filter.Eq(x => x.UserId, user.Id);
        if (unreadOnly)
            filter = Builders<Notification>.Filter.And(filter,
                Builders<Notification>.Filter.Eq(x => x.Read, false));

        var total = await DbService.Wrapper.Notifications.CountDocumentsAsync(filter);
        var list = await DbService.Wrapper.Notifications
            .Find(filter)
            .SortByDescending(x => x.CreateTime)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();

        int ps = pageSize > 0 ? pageSize : 1;
        return new ListResult<object>(list.Cast<object>().ToList())
        {
            TotalCount = (int)total,
            TotalPage = (int)Math.Ceiling((double)total / ps),
        };
    }

    [RequireAuth]
    [HttpPost("read/{id}")]
    public async Task MarkRead(string id)
    {
        var user = HttpContext.GetAuthenticatedUser();

        var notification = await DbService.Wrapper.Notifications
            .Find(x => x.SystemId == id && x.UserId == user.Id)
            .FirstOrDefaultAsync()
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        await DbService.Wrapper.Notifications.UpdateOneAsync(
            x => x.SystemId == id,
            Builders<Notification>.Update.Set(x => x.Read, true));
    }

    [RequireAuth]
    [HttpPost("read-all")]
    public async Task MarkAllRead()
    {
        var user = HttpContext.GetAuthenticatedUser();

        await DbService.Wrapper.Notifications.UpdateManyAsync(
            x => x.UserId == user.Id && !x.Read,
            Builders<Notification>.Update.Set(x => x.Read, true));
    }
}
