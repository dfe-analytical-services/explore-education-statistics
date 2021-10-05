#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPreReleaseUserService
    {
        Task<Either<ActionResult, List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseId);

        Task<Either<ActionResult, PreReleaseInvitePlan>> GetPreReleaseUsersInvitePlan(Guid releaseId, string emails);

        Task<Either<ActionResult, List<PreReleaseUserViewModel>>> InvitePreReleaseUsers(Guid releaseId, string emails);

        Task<Either<ActionResult, Unit>> RemovePreReleaseUser(Guid releaseId, string email);

        Task<Either<ActionResult, Unit>> SendPreReleaseUserInviteEmails(Guid releaseId);
    }
}
