using System;
using System.Collections.Generic;
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

[ApiController]
[Route("api/class")]
public class ClassController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [HttpGet("list")]
    public async Task<ListResult<ClassSchema>> ListClasses([FromQuery] string courseId)
    {
        if (string.IsNullOrWhiteSpace(courseId))
            throw new ApiException((int)HttpStatusCode.BadRequest);

        var list = await DbService.Wrapper.Classes
            .Find(x => x.CourseId == courseId && !x.Deleted)
            .ToListAsync();

        var result = list.ConvertAll(c => new ClassSchema
        {
            Id = c.Id,
            CourseId = c.CourseId,
            ClassName = c.Name,
            Lecturers = c.Lecturers,
            Language = c.Language,
            Content = c.Content["en"] ?? "",
        });

        return new ListResult<ClassSchema>(result) { TotalCount = result.Count };
    }

    [HttpGet("info/{id}")]
    public async Task<ClassSchema> GetClassInfo(string id)
    {
        var cls = await DbService.Wrapper.Classes.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        return new ClassSchema
        {
            Id = cls.Id,
            CourseId = cls.CourseId,
            ClassName = cls.Name,
            Lecturers = cls.Lecturers,
            Language = cls.Language,
            Content = cls.Content["en"] ?? "",
        };
    }

    [RequireAuth]
    [HttpPost("create")]
    public async Task<object> CreateClass([FromBody] ClassSchema schema)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        if (string.IsNullOrWhiteSpace(schema.CourseId))
            throw new ApiException((int)HttpStatusCode.BadRequest);

        var course = await DbService.Wrapper.Courses.FindByIdAsync(null, schema.CourseId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        var cls = new Class
        {
            CourseId = schema.CourseId,
            Name = schema.ClassName,
            Lecturers = schema.Lecturers,
            Language = schema.Language,
        };
        cls.Content["en"] = schema.Content;
        cls.SetCreateTime();

        var id = await DbService.Wrapper.Classes.InsertAsync(null, cls);
        return new { id };
    }

    [RequireAuth]
    [HttpPut("edit/{id}")]
    public async Task EditClass(string id, [FromBody] ClassSchema schema)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var cls = await DbService.Wrapper.Classes.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        // Faculty must be a lecturer or Admin
        if (user.Group != DbUser.Role.Admin && !cls.Lecturers.Contains(user.Id))
            throw new ApiException((int)HttpStatusCode.Forbidden);

        cls.Name = schema.ClassName;
        cls.Lecturers = schema.Lecturers;
        cls.Language = schema.Language;
        cls.Content["en"] = schema.Content;
        cls.SetUpdateTime();

        await DbService.Wrapper.Classes.ReplaceOneAsync(x => x.Id == cls.Id, cls);
    }

    [RequireAuth]
    [HttpDelete("delete/{id}")]
    public async Task DeleteClass(string id)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Admin);

        var cls = await DbService.Wrapper.Classes.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        using var session = await DbService.Wrapper.Client.StartSessionAsync();
        await DbService.Wrapper.Classes.DeleteAsync(session, cls);
    }
}
