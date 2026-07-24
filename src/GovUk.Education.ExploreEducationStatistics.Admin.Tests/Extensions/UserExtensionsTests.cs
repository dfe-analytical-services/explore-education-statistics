using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Extensions;

public class UserExtensionsTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [InlineData(GlobalRoles.Role.StandardUser, false)]
    [InlineData(GlobalRoles.Role.BauUser, true)]
    public void IsBau_ReturnsExpectedResult(GlobalRoles.Role role, bool expected)
    {
        User user = _dataFixture.DefaultUser().WithRoleId(role.GetEnumValue());

        var result = user.IsBau();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(GlobalRoles.Role.StandardUser)]
    [InlineData(GlobalRoles.Role.BauUser)]
    public void GetGlobalRole_ReturnsExpectedResult(GlobalRoles.Role role)
    {
        User user = _dataFixture.DefaultUser().WithRoleId(role.GetEnumValue());

        var result = user.GetGlobalRole();

        Assert.Equal(role, result);
    }
}
