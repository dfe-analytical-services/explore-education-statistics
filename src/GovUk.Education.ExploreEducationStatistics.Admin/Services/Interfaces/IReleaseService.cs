using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseService
    {
        List<Release> List();

        Release Get(Guid id);
        
        Task<Release> GetAsync(Guid id);

        Release Get(string slug);

        Task<Either<ValidationResult, ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release);

        Task<ReleaseViewModel> GetReleaseForIdAsync(Guid id);
        
        Task<ReleaseSummaryViewModel> GetReleaseSummaryAsync(Guid releaseId);
        
        Task<Either<ValidationResult, ReleaseViewModel>> EditReleaseSummaryAsync(Guid releaseId, UpdateReleaseSummaryRequest request);

        Task<List<ReleaseViewModel>> GetReleasesForPublicationAsync(Guid publicationId);

        Task<List<ReleaseViewModel>> GetMyReleasesForReleaseStatusesAsync(params ReleaseStatus[] releaseStatuses);

        Task<Either<ValidationResult, ReleaseSummaryViewModel>> UpdateReleaseStatusAsync(Guid releaseId, ReleaseStatus status, string internalReleaseNote);
    }
}
