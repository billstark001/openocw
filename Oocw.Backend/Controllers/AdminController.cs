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
using Oocw.Backend.Schemas;
using Oocw.Backend.Services;
using Oocw.Backend.Utils;
using Oocw.Database.Models;
using DbUser = Oocw.Database.Models.User;
using Oocw.Database.Models.Technical;

namespace Oocw.Backend.Controllers;

public class UpdateSettingsBody
{
    public int CurrentYear { get; set; }
    public int CurrentTerm { get; set; }
}

public class UpdateRoleBody
{
    public string Role { get; set; } = "";
}

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [RequireAuth]
    [HttpGet("settings")]
    public async Task<object> GetSettings()
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        var settings = await DbService.Wrapper.GlobalSettings
            .Find(x => x.Key == "global")
            .FirstOrDefaultAsync()
            ?? new GlobalSettings();

        return settings;
    }

    [RequireAuth]
    [HttpPut("settings")]
    public async Task UpdateSettings([FromBody] UpdateSettingsBody body)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        var settings = new GlobalSettings
        {
            Key = "global",
            CurrentYear = body.CurrentYear,
            CurrentTerm = body.CurrentTerm,
        };

        await DbService.Wrapper.GlobalSettings.ReplaceOneAsync(
            x => x.Key == "global",
            settings,
            new ReplaceOptions { IsUpsert = true });
    }

    [RequireAuth]
    [HttpGet("users")]
    public async Task<ListResult<object>> ListUsers([FromQuery] PaginationParams pagination)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);
        pagination ??= new();
        pagination.Sanitize();

        var filter = Builders<User>.Filter.Eq(x => x.Deleted, false);
        var total = await DbService.Wrapper.Users.CountDocumentsAsync(filter);
        var list = await DbService.Wrapper.Users
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
    [HttpPut("user/{id}/role")]
    public async Task UpdateUserRole(string id, [FromBody] UpdateRoleBody body)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        if (!Enum.TryParse<DbUser.Role>(body.Role, ignoreCase: true, out var role))
            throw new ApiException((int)HttpStatusCode.BadRequest);

        var user = await DbService.Wrapper.Users.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        await DbService.Wrapper.Users.UpdateOneAsync(
            x => x.Id == id,
            Builders<User>.Update
                .Set(x => x.Group, role)
                .Set(x => x.UpdateTime, DateTime.UtcNow));
    }

    [RequireAuth]
    [HttpGet("organizations")]
    public async Task<ListResult<object>> ListOrganizations()
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        var list = await DbService.Wrapper.Organizations
            .Find(x => !x.Deleted)
            .ToListAsync();

        return new ListResult<object>(list.Cast<object>().ToList()) { TotalCount = list.Count };
    }

    [RequireAuth]
    [HttpPost("organization")]
    public async Task<object> CreateOrganization([FromBody] OrganizationSchema schema)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        var org = new Organization
        {
            Code = schema.Code,
            Aliases = schema.Aliases,
            ParentId = schema.ParentId,
            Type = schema.Type,
        };
        org.Name[schema.Lang] = schema.Name;
        org.SetCreateTime();

        var id = await DbService.Wrapper.Organizations.InsertAsync(null, org);
        return new { id };
    }

    [RequireAuth]
    [HttpPut("organization/{id}")]
    public async Task EditOrganization(string id, [FromBody] OrganizationSchema schema)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        var org = await DbService.Wrapper.Organizations.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        org.Code = schema.Code;
        org.Name[schema.Lang] = schema.Name;
        org.Aliases = schema.Aliases;
        org.ParentId = schema.ParentId;
        org.Type = schema.Type;
        org.SetUpdateTime();

        await DbService.Wrapper.Organizations.ReplaceOneAsync(x => x.Id == id, org);
    }

    [RequireAuth]
    [HttpDelete("organization/{id}")]
    public async Task DeleteOrganization(string id)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        var org = await DbService.Wrapper.Organizations.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        using var session = await DbService.Wrapper.Client.StartSessionAsync();
        await DbService.Wrapper.Organizations.DeleteAsync(session, org);
    }

    [RequireAuth]
    [HttpGet("feedback")]
    public async Task<ListResult<object>> ListFeedback(
        [FromQuery] string? classInstanceId,
        [FromQuery] PaginationParams pagination)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);
        pagination ??= new();
        pagination.Sanitize();

        var filter = Builders<Feedback>.Filter.Eq(x => x.Deleted, false);
        if (!string.IsNullOrWhiteSpace(classInstanceId))
            filter = Builders<Feedback>.Filter.And(filter,
                Builders<Feedback>.Filter.Eq(x => x.ClassInstanceId, classInstanceId));

        var total = await DbService.Wrapper.Feedbacks.CountDocumentsAsync(filter);
        var list = await DbService.Wrapper.Feedbacks
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
    [HttpGet("feedback/export")]
    public async Task<List<object>> ExportFeedback([FromQuery] string? classInstanceId)
    {
        HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        var filter = Builders<Feedback>.Filter.Eq(x => x.Deleted, false);
        if (!string.IsNullOrWhiteSpace(classInstanceId))
            filter = Builders<Feedback>.Filter.And(filter,
                Builders<Feedback>.Filter.Eq(x => x.ClassInstanceId, classInstanceId));

        var list = await DbService.Wrapper.Feedbacks.Find(filter).ToListAsync();
        return list.Cast<object>().ToList();
    }
}
