using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Oocw.Backend.Models;
using Oocw.Backend.Schemas;
using Oocw.Backend.Services;
using Oocw.Backend.Utils;

namespace Oocw.Backend.Controllers;

[ApiController]
[Route("api/organization")]
public class OrganizationController : ControllerBase
{
    [FromServices] public DatabaseService DbService { get; set; } = null!;

    /// <summary>
    /// Returns all non-deleted organisations as a flat list.
    /// Clients build the hierarchy using the <c>parentId</c> field.
    /// </summary>
    [HttpGet("list")]
    public async Task<ListResult<OrganizationBrief>> List(string? lang)
    {
        lang ??= this.TryGetLanguage();

        var list = await DbService.Wrapper.Organizations
            .Find(x => !x.Deleted)
            .ToListAsync();

        var briefs = list.Select(o => new OrganizationBrief
        {
            Id = o.Id,
            Code = o.Code,
            Name = o.Name[lang] ?? o.Code,
            Type = o.Type.ToString(),
            ParentId = o.ParentId,
        }).ToList();

        return new ListResult<OrganizationBrief>(briefs) { TotalCount = briefs.Count };
    }
}
