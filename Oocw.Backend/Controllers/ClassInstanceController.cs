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

[ApiController]
[Route("api/class-instance")]
public class ClassInstanceController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    [HttpGet("list")]
    public async Task<ListResult<ClassInstanceSchema>> ListInstances([FromQuery] string classId)
    {
        if (string.IsNullOrWhiteSpace(classId))
            throw new ApiException((int)HttpStatusCode.BadRequest);

        var list = await DbService.Wrapper.ClassInstances
            .Find(x => x.ClassId == classId && !x.Deleted)
            .ToListAsync();

        var result = list.ConvertAll(i => new ClassInstanceSchema
        {
            Id = i.Id,
            ClassId = i.ClassId,
            Lecturers = i.Lecturers,
        });

        return new ListResult<ClassInstanceSchema>(result) { TotalCount = result.Count };
    }

    [HttpGet("info/{id}")]
    public async Task<object> GetInstanceInfo(string id)
    {
        var instance = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        return new
        {
            instance.Id,
            instance.ClassId,
            instance.Lecturers,
            instance.Address,
            instance.Contents,
        };
    }

    [RequireAuth]
    [HttpPost("create")]
    public async Task<object> CreateInstance([FromBody] ClassInstanceSchema schema)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        if (string.IsNullOrWhiteSpace(schema.ClassId))
            throw new ApiException((int)HttpStatusCode.BadRequest);

        _ = await DbService.Wrapper.Classes.FindByIdAsync(null, schema.ClassId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        var instance = new ClassInstance
        {
            ClassId = schema.ClassId,
            Lecturers = schema.Lecturers,
        };
        instance.SetCreateTime();

        var id = await DbService.Wrapper.ClassInstances.InsertAsync(null, instance);
        return new { id };
    }

    [RequireAuth]
    [HttpPut("edit/{id}")]
    public async Task EditInstance(string id, [FromBody] ClassInstanceSchema schema)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var instance = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        instance.Lecturers = schema.Lecturers;
        instance.SetUpdateTime();

        await DbService.Wrapper.ClassInstances.ReplaceOneAsync(x => x.Id == instance.Id, instance);
    }

    [RequireAuth]
    [HttpPost("{id}/content/add")]
    public async Task<object> AddContent(string id, [FromBody] ClassInstance.Content content)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var instance = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        content.Id = DataModelUtils.GenerateRandomId(8);
        instance.Contents.Add(content);
        instance.SetUpdateTime();

        await DbService.Wrapper.ClassInstances.ReplaceOneAsync(x => x.Id == instance.Id, instance);
        return new { contentId = content.Id };
    }

    [RequireAuth]
    [HttpDelete("{id}/content/{contentId}")]
    public async Task RemoveContent(string id, string contentId)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var instance = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        var removed = instance.Contents.RemoveAll(c => c.Id == contentId);
        if (removed == 0)
            throw new ApiException((int)HttpStatusCode.NotFound);

        instance.SetUpdateTime();
        await DbService.Wrapper.ClassInstances.ReplaceOneAsync(x => x.Id == instance.Id, instance);
    }

    [RequireAuth]
    [HttpPost("{id}/copy/{sourceId}")]
    public async Task CopyContent(string id, string sourceId)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);

        var target = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, id)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        var source = await DbService.Wrapper.ClassInstances.FindByIdAsync(null, sourceId)
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        foreach (var content in source.Contents)
        {
            var copy = new ClassInstance.Content
            {
                Id = DataModelUtils.GenerateRandomId(8),
                LectureNumber = content.LectureNumber,
                LectureDate = content.LectureDate,
                Type = content.Type,
                Text = content.Text,
                IsPublic = content.IsPublic,
            };
            target.Contents.Add(copy);
        }

        target.SetUpdateTime();
        await DbService.Wrapper.ClassInstances.ReplaceOneAsync(x => x.Id == target.Id, target);
    }
}
