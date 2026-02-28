using System.Collections.Generic;
using Oocw.Database.Models;

namespace Oocw.Backend.Schemas;

public class OrganizationSchema
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string Lang { get; set; } = "en";
    public List<string> Aliases { get; set; } = [];
    public string? ParentId { get; set; }
    public Organization.OrganizationType Type { get; set; } = Organization.OrganizationType.Department;
}
