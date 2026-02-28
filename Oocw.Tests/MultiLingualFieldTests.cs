using Xunit;
using Oocw.Database.Models.Technical;

namespace Oocw.Tests;

public class MultiLingualFieldTests
{
    [Fact]
    public void GetTranslation_ExactMatch_Returns()
    {
        var field = new MultiLingualField();
        field.SetTranslation("en", "Hello");
        field.SetTranslation("ja", "こんにちは");

        Assert.Equal("Hello", field.GetTranslation("en"));
        Assert.Equal("こんにちは", field.GetTranslation("ja"));
    }

    [Fact]
    public void GetTranslation_FallsBackToEnglish()
    {
        var field = new MultiLingualField();
        field.SetTranslation("en", "Hello");

        // Requesting a language not present falls back to English
        Assert.Equal("Hello", field.GetTranslation("fr"));
    }

    [Fact]
    public void GetTranslation_FallsBackToFirst()
    {
        var field = new MultiLingualField();
        field.SetTranslation("ja", "こんにちは");

        // No English, returns first available
        Assert.Equal("こんにちは", field.GetTranslation("fr"));
    }

    [Fact]
    public void SetTranslation_NullValue_Removes()
    {
        var field = new MultiLingualField();
        field.SetTranslation("en", "Hello");
        field.SetTranslation("en", null);  // Remove by setting null

        Assert.False(field.ContainsKey("en"));
    }

    [Fact]
    public void GetTranslation_PrefixMatch_Returns()
    {
        var field = new MultiLingualField();
        field.SetTranslation("en-US", "Hello");

        // "en-US".StartsWith("en") is true, so prefix match should find it
        Assert.Equal("Hello", field.GetTranslation("en"));
    }
}
