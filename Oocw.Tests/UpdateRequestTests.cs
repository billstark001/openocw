using Xunit;
using Oocw.Database.Models.Technical;

namespace Oocw.Tests;

public class UpdateRequestTests
{
    [Fact]
    public void NewUpdateRequest_HasPendingStatus()
    {
        var req = new UpdateRequest();
        Assert.Equal(UpdateRequest.RequestStatus.Pending, req.Status);
    }

    [Fact]
    public void NewUpdateRequest_ReviewerFieldsAreNull()
    {
        var req = new UpdateRequest();
        Assert.Null(req.ReviewerId);
        Assert.Null(req.ReviewTime);
        Assert.Null(req.ReviewComment);
    }
}
