#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPreReleaseUserService
    {
        Task<Either<ActionResult, List<PreReleaseUserViewModel>>> GetPreReleaseUsers(Guid releaseId);

        Task<Either<ActionResult, PreReleaseUserViewModel>> AddPreReleaseUser(Guid releaseId, string email);

        Task<Either<ActionResult, Unit>> RemovePreReleaseUser(Guid releaseId, string email);

        Task<Either<ActionResult, Unit>> SendPreReleaseUserInviteEmails(Release release);
    }
}
