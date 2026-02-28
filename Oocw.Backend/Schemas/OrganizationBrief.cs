namespace Oocw.Backend.Schemas;

/// <summary>Lightweight organisation summary returned by the public listing endpoint.</summary>
public class OrganizationBrief
{
    public string? Id { get; set; }

    /// <summary>Machine-readable code stored in <see cref="Oocw.Database.Models.Organization.Code"/>.</summary>
    public string Code { get; set; } = "";

    /// <summary>Display name resolved for the requested language.</summary>
    public string Name { get; set; } = "";

    /// <summary>String representation of <see cref="Oocw.Database.Models.Organization.OrganizationType"/>.</summary>
    public string Type { get; set; } = "";

    /// <summary><see cref="Oocw.Database.Models.Technical.DataModel.Id"/> of the parent organisation, or <c>null</c> for root entries.</summary>
    public string? ParentId { get; set; }
}
