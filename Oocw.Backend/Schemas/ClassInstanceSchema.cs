using System.Collections.Generic;

namespace Oocw.Backend.Schemas;

public class ClassInstanceSchema
{
    public string Id { get; set; } = "";
    public string ClassId { get; set; } = "";
    public List<string> Lecturers { get; set; } = [];
}
