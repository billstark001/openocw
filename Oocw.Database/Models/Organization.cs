
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Oocw.Database.Models.Technical;

namespace Oocw.Database.Models;

/// <summary>
/// Represents an entry in the institutional organisation hierarchy
/// (e.g. a school, faculty, department, or programme).
/// The hierarchy is encoded via <see cref="ParentId"/>.
/// </summary>
public class Organization : DataModel
{
    public enum OrganizationType
    {
        School,
        Faculty,
        Department,
        Programme,
        Other,
    }

    /// <summary>Short machine-readable code used in <see cref="Course.Departments"/>.</summary>
    public string Code { get; set; } = "";

    /// <summary>Human-readable names in multiple languages.</summary>
    public MultiLingualField Name { get; set; } = new();

    /// <summary>
    /// Alternative names / aliases (in any language) that can be matched when
    /// importing data from external sources.  Used by
    /// <see cref="Utils.OrganizationUtils.RefreshCourseOrganizationsAsync"/>.
    /// </summary>
    public List<string> Aliases { get; set; } = [];

    /// <summary>
    /// Id of the parent <see cref="Organization"/>, or <c>null</c> for a
    /// top-level entry.
    /// </summary>
    public string? ParentId { get; set; }

    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public OrganizationType Type { get; set; } = OrganizationType.Department;
}
