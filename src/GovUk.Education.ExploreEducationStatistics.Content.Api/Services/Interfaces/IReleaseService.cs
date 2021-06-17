using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IReleaseService
    {
        public Task<Either<ActionResult, ReleaseViewModel>> Get(string publicationPath, string releasePath);
        
        public Task<Either<ActionResult, ReleaseSummaryViewModel>> GetSummary(string publicationPath, string releasePath);
    }
}
