#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public static class GlobalRoles
{
    // TODO EES-2462 Consider making this a class with Name and Id properties
    public enum Role
    {
        [EnumLabelValue(RoleNames.StandardUser, "f9ddb43e-aa9e-41ed-837d-3062e130c425")]
        StandardUser,

        [EnumLabelValue(RoleNames.BauUser, "cf67b697-bddd-41bd-86e0-11b7e11d99b3")]
        BauUser,
    }

    public static class RoleNames
    {
        public const string StandardUser = "Standard User";
        public const string BauUser = "BAU User";
    }

    public static List<string> GlobalRolePrecedenceOrder = new() { RoleNames.StandardUser, RoleNames.BauUser };

    public static List<string> GetHigherRoles(string roleName)
    {
        return [.. GlobalRolePrecedenceOrder.Skip(GlobalRolePrecedenceOrder.IndexOf(roleName) + 1)];
    }

    public static List<string> GetLowerRoles(string role)
    {
        return GlobalRolePrecedenceOrder.GetRange(0, GlobalRolePrecedenceOrder.IndexOf(role));
    }
}
