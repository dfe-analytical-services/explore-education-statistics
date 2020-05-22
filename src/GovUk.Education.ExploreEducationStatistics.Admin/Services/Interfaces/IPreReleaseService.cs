using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPreReleaseService
    {
        PreReleaseWindow GetPreReleaseWindow(Release release);
        PreReleaseWindowStatus GetPreReleaseWindowStatus(Release release, DateTime referenceTime);
    }
}