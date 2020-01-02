using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent
{
    // TODO EES-919 - return ActionResults rather than ValidationResults
    public interface IReleaseNoteService
    {
        Task<Either<ValidationResult, List<ReleaseNoteViewModel>>> AddReleaseNoteAsync(Guid releaseId,
            CreateOrUpdateReleaseNoteRequest request);

        Task<Either<ValidationResult, List<ReleaseNoteViewModel>>> UpdateReleaseNoteAsync(Guid releaseId,
            Guid releaseNoteId, CreateOrUpdateReleaseNoteRequest request);

        Task<Either<ValidationResult, List<ReleaseNoteViewModel>>> DeleteReleaseNoteAsync(Guid releaseId,
            Guid releaseNoteId);
    }
}