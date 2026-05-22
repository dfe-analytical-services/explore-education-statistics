#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IGlobalRoleService
{
    Task<Either<ActionResult, List<RoleViewModel>>> GetAllGlobalRoles();

    Task<Either<ActionResult, List<RoleViewModel>>> GetGlobalRolesForUser(string userId);

    Task<Either<ActionResult, Unit>> SetGlobalRoleForUser(string userId, string roleId);

    Task UpgradeToGlobalRoleIfRequired(ApplicationUser user, string globalRoleNameToSet);

    Task DowngradeFromGlobalRoleIfRequired(ApplicationUser user, string globalRoleNameToDowngradeFrom);
}
