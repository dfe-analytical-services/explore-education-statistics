using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Extensions;

public static class UserExtensions
{
    public static GlobalRoles.Role GetGlobalRole(this User user) =>
        user.RoleId switch
        {
            var roleId when roleId == GlobalRoles.Role.StandardUser.GetEnumValue() => GlobalRoles.Role.StandardUser,
            var roleId when roleId == GlobalRoles.Role.BauUser.GetEnumValue() => GlobalRoles.Role.BauUser,
            _ => throw new ArgumentOutOfRangeException($"Unknown role id: {user.RoleId}"),
        };
}
