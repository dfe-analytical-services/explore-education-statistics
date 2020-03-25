using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPreReleaseContactsService
    {
        Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>> GetPreReleaseContactsForReleaseAsync(Guid releaseId);

        Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>> AddPreReleaseContactToReleaseAsync(Guid releaseId, string email);

        Task<Either<ActionResult, List<PrereleaseCandidateViewModel>>> RemovePreReleaseContactFromReleaseAsync(Guid releaseId, string email);
    }
}
