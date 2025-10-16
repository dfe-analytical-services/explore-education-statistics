using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests;

public class UserTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [InlineData("", "", "")]
    [InlineData(" ", " ", "")]
    [InlineData("Joe", "", "Joe")]
    [InlineData("Joe", " ", "Joe")]
    [InlineData(" Joe ", "", "Joe")]
    [InlineData("", "Smith", "Smith")]
    [InlineData(" ", "Smith", "Smith")]
    [InlineData("", " Smith ", "Smith")]
    [InlineData("Joe", "Smith", "Joe Smith")]
    [InlineData(" Joe ", " Smith ", "Joe Smith")]
    public void DisplayName(
        string? firstName,
        string? lastName,
        string expectedDisplayName)
    {
        var user = _dataFixture.DefaultUser()
            .WithFirstName(firstName)
            .WithLastName(lastName)
            .Generate();

        Assert.Equal(expectedDisplayName, user.DisplayName);
    }
}
