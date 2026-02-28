using System.Linq;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Oocw.Database;
using Oocw.Backend.Schemas;
using Oocw.Backend.Utils;
using Oocw.Backend.Services;
using Oocw.Utils;
using Oocw.Database.Models;
using DbUser = Oocw.Database.Models.User;
using System;
using System.Threading.Tasks;
using Oocw.Backend.Models;
using Oocw.Backend.Api;
using Oocw.Backend.Auth;
using System.Net;
using Oocw.Database.Models.Technical;

namespace Oocw.Backend.Controllers;


[ApiController]
[Route("api/course")]
public class CourseController : ControllerBase
{
    // var defs
    [FromServices] public DatabaseService DbService { get; set; } = null!;
    [FromServices] public SearchService SearchService { get; set; } = null!;
    // api


    [HttpGet("search")]
    public async Task<ListResult<CourseBrief>> SearchCourse(
        [FromQuery] CourseFilter filter,
        [FromQuery] PaginationParams pagination,
        string? lang)
    {
        filter ??= new();
        pagination ??= new();
        pagination.Sanitize();
        lang ??= this.TryGetLanguage();

        var records = await SearchService.SearchCourse(filter, pagination, lang);

        if (records.Count == 0)
            return new ListResult<CourseBrief>(null) { TotalCount = 0, TotalPage = 0 };

        var courseIds = records.Select(r => r.CourseId).Distinct().ToList();
        var courses = await DbService.Wrapper.Courses
            .Find(Builders<Course>.Filter.In(x => x.Id, courseIds))
            .ToListAsync();

        var courseMap = courses.ToDictionary(c => c.Id);

        var briefs = records
            .Where(r => courseMap.ContainsKey(r.CourseId))
            .Select(r => BuildCourseBrief(courseMap[r.CourseId], lang))
            .ToList();

        return new ListResult<CourseBrief>(briefs) { TotalCount = briefs.Count };
    }

    [HttpGet("list/{id}")]
    public Task<ListResult<CourseBrief>> ListCourseByDepartment(
        string? id,
        [FromQuery] CourseFilter filter,
        [FromQuery] PaginationParams pagination,
        string? lang)
    {
        filter ??= new();
        filter.DepartmentExact = [id ?? ""];
        return ListCourse(filter, pagination, lang);
    }

    [HttpGet("list")]
    public async Task<ListResult<CourseBrief>> ListCourse(
        [FromQuery] CourseFilter filter,
        [FromQuery] PaginationParams pagination,
        string? lang)
    {
        filter ??= new();
        pagination ??= new();
        pagination.Sanitize();
        lang ??= this.TryGetLanguage();

        var filterDef = filter.GetCourseFilterDefinition()
            ?? FilterDefinition<Course>.Empty;

        var total = await DbService.Wrapper.Courses.CountDocumentsAsync(filterDef);

        var list = await DbService.Wrapper.Courses
            .Find(filterDef)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Limit(pagination.PageSize)
            .ToListAsync();

        var briefs = list.Select(c => BuildCourseBrief(c, lang)).ToList();

        int pageSize = pagination.PageSize > 0 ? pagination.PageSize : 1;

        return new ListResult<CourseBrief>(briefs)
        {
            TotalCount = (int)total,
            TotalPage = (int)Math.Ceiling((double)total / pageSize),
        };
    }

    [HttpGet("info/{code}")]
    public async Task<CourseSchema> GetCourseInfo(string code, string? classId, string? lang)
    {
        lang ??= this.TryGetLanguage();

        var crs = await DbService.Wrapper.Courses
            .Find(x => x.CourseCode == code && !x.Deleted)
            .FirstOrDefaultAsync();

        var cls = !string.IsNullOrWhiteSpace(classId) && crs != null
            ? await DbService.Wrapper.Classes
                .Find(x => x.CourseId == crs.Id && x.Id == classId && !x.Deleted)
                .FirstOrDefaultAsync()
            : null;

        if (crs == null || (!string.IsNullOrWhiteSpace(classId) && cls == null))
        {
            throw new ApiException((int)HttpStatusCode.NotFound);
        }

        return BuildCourseSchema(crs, cls, lang);
    }


    [RequireAuth]
    [HttpPost("create")]
    public async Task<object> CreateCourse(string? lang, [FromBody] CourseSchema course)
    {
        var user = HttpContext.GetAuthenticatedUser(DbUser.Role.Faculty);
        lang ??= this.TryGetLanguage();

        var newCourse = new Course
        {
            CourseCode = course.CourseCode,
            Credit = course.Credit,
            Departments = course.Departments,
            Lecturers = course.Lecturers,
            Tags = course.Tags,
            Image = course.ImageLink,
        };
        newCourse.Name[lang] = course.Name;
        newCourse.Content[lang] = course.Content;
        newCourse.SetCreateTime();

        var id = await DbService.Wrapper.Courses.InsertAsync(null, newCourse);
        await SearchService.MarkCourseRecordDirty(id);

        return new { id };
    }

    [RequireAuth]
    [HttpPost("edit")]
    public async Task Edit(string? lang, [FromBody] CourseSchema course)
    {
        lang ??= this.TryGetLanguage();
        if (string.IsNullOrWhiteSpace(course.Id))
        {
            throw new ApiException((int)HttpStatusCode.NotFound);
        }

        var existing = await DbService.Wrapper.Courses
            .Find(x => x.Id == course.Id && !x.Deleted)
            .FirstOrDefaultAsync()
            ?? throw new ApiException((int)HttpStatusCode.NotFound);

        existing.Name[lang] = course.Name;
        existing.Content[lang] = course.Content;
        existing.CourseCode = course.CourseCode;
        existing.Credit = course.Credit;
        existing.Departments = course.Departments;
        existing.Lecturers = course.Lecturers;
        existing.Tags = course.Tags;
        existing.Image = course.ImageLink;
        existing.SetUpdateTime();

        await DbService.Wrapper.Courses.ReplaceOneAsync(x => x.Id == existing.Id, existing);

        await SearchService.MarkCourseRecordDirty(existing.Id!);
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static CourseBrief BuildCourseBrief(Course c, string lang) => new()
    {
        Id = c.Id,
        Name = c.Name[lang] ?? c.Name.GetTranslation(lang) ?? "",
        Tags = c.Tags,
        Lecturers = c.Lecturers.Select(l => new EntityReference { Id = l }).ToList(),
        Description = c.Content[lang] ?? "",
        Image = c.Image,
    };

    private static CourseSchema BuildCourseSchema(Course c, Class? cls, string lang) => new()
    {
        Id = c.Id,
        Name = c.Name[lang] ?? c.Name.GetTranslation(lang) ?? "",
        CourseCode = c.CourseCode,
        Credit = c.Credit,
        Departments = c.Departments,
        Lecturers = c.Lecturers,
        Tags = c.Tags,
        Content = c.Content[lang] ?? "",
        ImageLink = c.Image,
        Classes = cls != null
            ? [new EntityReference { Id = cls.Id, Name = cls.Name }]
            : [],
    };
}
