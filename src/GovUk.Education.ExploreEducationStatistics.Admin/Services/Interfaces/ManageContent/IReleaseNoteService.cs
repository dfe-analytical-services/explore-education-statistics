using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    public interface IReleaseNoteService
    {
        Task<Either<ActionResult, List<ReleaseNoteViewModel>>> AddReleaseNoteAsync(Guid releaseId,
            CreateOrUpdateReleaseNoteRequest request);

        Task<Either<ActionResult, List<ReleaseNoteViewModel>>> UpdateReleaseNoteAsync(Guid releaseId,
            Guid releaseNoteId, CreateOrUpdateReleaseNoteRequest request);

        Task<Either<ActionResult, List<ReleaseNoteViewModel>>> DeleteReleaseNoteAsync(Guid releaseId,
            Guid releaseNoteId);
    }
}