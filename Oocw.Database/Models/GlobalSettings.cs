
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Oocw.Database.Models;

/// <summary>
/// Singleton document that stores system-wide settings such as the current
/// academic year and term.
/// </summary>
public class GlobalSettings
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? SystemId { get; set; }

    /// <summary>
    /// Logical singleton key – always <c>"global"</c>.
    /// An index with <c>unique = true</c> ensures only one document exists.
    /// </summary>
    public string Key { get; set; } = "global";

    /// <summary>Current academic year (e.g. 2024).</summary>
    public int CurrentYear { get; set; }

    /// <summary>
    /// Current academic term encoded as a bit-flag
    /// (e.g. 1 = spring, 2 = autumn, 3 = both, 4 = summer, …).
    /// </summary>
    public int CurrentTerm { get; set; }
}
