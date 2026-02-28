using Xunit;
using Oocw.Database.Utils;

namespace Oocw.Tests;

public class UserUtilsTests
{
    [Theory]
    [InlineData("validUser", true)]
    [InlineData("short", false)]        // too short (5 chars, min is 6)
    [InlineData("validUser123", true)]
    [InlineData("", false)]
    public void IsValidUsername_ReturnsExpected(string username, bool expected)
    {
        Assert.Equal(expected, UserUtils.IsValidUsername(username));
    }

    [Theory]
    [InlineData("Password1", true)]
    [InlineData("pwd", false)]          // too short (3 chars, min is 6)
    [InlineData("", false)]
    [InlineData("validpass", true)]
    public void IsValidPassword_ReturnsExpected(string password, bool expected)
    {
        Assert.Equal(expected, UserUtils.IsValidPassword(password));
    }

    [Fact]
    public void HashPassword_ThenVerify_ReturnsTrue()
    {
        var password = "TestPassword123";
        var hash = UserUtils.HashPassword(password);
        Assert.True(UserUtils.VerifyPassword(password, hash));
    }

    [Fact]
    public void VerifyPassword_WrongPassword_ReturnsFalse()
    {
        var hash = UserUtils.HashPassword("correctPassword");
        Assert.False(UserUtils.VerifyPassword("wrongPassword", hash));
    }

    [Fact]
    public void HashPassword_SameInput_DifferentHashes()
    {
        var password = "TestPassword";
        var hash1 = UserUtils.HashPassword(password);
        var hash2 = UserUtils.HashPassword(password);
        // Different salts should produce different hashes
        Assert.NotEqual(hash1, hash2);
    }
}
