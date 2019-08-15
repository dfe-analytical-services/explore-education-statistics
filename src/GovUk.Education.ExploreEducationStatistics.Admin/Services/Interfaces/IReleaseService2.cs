using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using ReleaseId = System.Guid;
using PublicationId  = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleaseService2
    {
        Task<Release> GetAsync(ReleaseId id);
        
        Task<Either<ValidationResult, ReleaseViewModel>> CreateReleaseAsync(CreateReleaseViewModel release);
        
        Task<ReleaseViewModel> GetReleaseForIdAsync(ReleaseId id);
        
        Task<EditReleaseSummaryViewModel> GetReleaseSummaryAsync(ReleaseId releaseId);

        Task<Either<ValidationResult, ReleaseViewModel>> EditReleaseSummaryAsync(EditReleaseSummaryViewModel model);

        Task<List<ReleaseViewModel>> GetReleasesForPublicationAsync(PublicationId publicationId);
    }
}
