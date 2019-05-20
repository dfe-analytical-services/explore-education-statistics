using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces
{
    public interface IReleaseService
    {
        ActionResult<Release> GetRelease(string id);
        
        ActionResult<Release> GetLatestRelease(string id);
    }
}