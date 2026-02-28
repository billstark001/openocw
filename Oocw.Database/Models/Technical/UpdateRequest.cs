

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Oocw.Database.Models.Technical;

public class UpdateRequest
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected,
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string SystemId { get; set; } = "";

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;

    public string SenderId { get; set; } = "";

    public string TargetCollection { get; set; } = "";

    public string TargetObjectId { get; set; } = "";

    public BsonDocument Patch { get; set; } = [];

    [BsonRepresentation(BsonType.String)]
    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public string? ReviewerId { get; set; }

    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ReviewTime { get; set; }

    public string? ReviewComment { get; set; }
}