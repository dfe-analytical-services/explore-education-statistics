#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IGlobalRoleService
{
    Task<Either<ActionResult, Unit>> UpdateGlobalRoleForUser(Guid userId, Role newRole);
}
