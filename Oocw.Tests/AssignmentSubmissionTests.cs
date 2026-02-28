using Xunit;
using Oocw.Database.Models;

namespace Oocw.Tests;

public class AssignmentSubmissionTests
{
    [Fact]
    public void NewSubmission_GradeIsMinusOne()
    {
        var sub = new AssignmentSubmission();
        Assert.Equal(-1, sub.Grade);
    }

    [Fact]
    public void NewSubmission_HistoryIsEmpty()
    {
        var sub = new AssignmentSubmission();
        Assert.Empty(sub.History);
    }

    [Fact]
    public void NewSubmission_ContentsIsEmpty()
    {
        var sub = new AssignmentSubmission();
        Assert.Empty(sub.Contents);
    }
}
