using Xunit;
using Oocw.Database.Models.Technical;
using System;

namespace Oocw.Tests;

public class DataModelTests
{
    [Fact]
    public void SetCreateTime_SetsUtcNow()
    {
        var before = DateTime.UtcNow;
        var model = new TestDataModel();
        model.SetCreateTime();
        var after = DateTime.UtcNow;

        Assert.InRange(model.CreateTime, before, after);
    }

    [Fact]
    public void SetUpdateTime_SetsUtcNow()
    {
        var model = new TestDataModel();
        model.SetUpdateTime();

        Assert.NotNull(model.UpdateTime);
        Assert.True(model.UpdateTime!.Value <= DateTime.UtcNow);
    }

    [Fact]
    public void MarkDeleted_SetsDeletedTrue()
    {
        var model = new TestDataModel();
        Assert.False(model.Deleted);
        model.MarkDeleted();
        Assert.True(model.Deleted);
    }

    [Fact]
    public void GenerateRandomId_ReturnsCorrectLength()
    {
        var id = DataModelUtils.GenerateRandomId(12);
        Assert.Equal(12, id.Length);
    }

    [Fact]
    public void GenerateRandomId_ReturnsDistinctValues()
    {
        var id1 = DataModelUtils.GenerateRandomId();
        var id2 = DataModelUtils.GenerateRandomId();
        Assert.NotEqual(id1, id2);
    }

    private class TestDataModel : DataModel { }
}
