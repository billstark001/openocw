

using System.Collections.Generic;
using Oocw.Database.Models.Technical;

namespace Oocw.Database.Models;

public class AssignmentSubmission : DataModel
{
    public enum ContentType
    {
        Text,
        File,
        Media,
    }

    public class Content
    {
        public ContentType Type { get; set; } = ContentType.Text;
        public string Text { get; set; } = "";
    }

    public string ClassInstanceId { get; set; } = "";
    public string ContentId { get; set; } = "";
    public string StudentId { get; set; } = "";

    public List<Content> Contents { get; set; } = [];

    /// <summary>Revision history; the last entry is the current submission.</summary>
    public List<List<Content>> History { get; set; } = [];

    /// <summary>Grade 0–100, or -1 if not yet graded.</summary>
    public int Grade { get; set; } = -1;

    public string GraderComment { get; set; } = "";

    public string GradedBy { get; set; } = "";
}