using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Oocw.Database.Models.Technical;

namespace Oocw.Database.Models;

/// <summary>Student feedback for a class instance.</summary>
public class Feedback : DataModel
{
    public string StudentId { get; set; } = "";
    public string ClassInstanceId { get; set; } = "";

    /// <summary>Rating 1–5.</summary>
    public int Rating { get; set; } = 3;

    public string Comment { get; set; } = "";
}
