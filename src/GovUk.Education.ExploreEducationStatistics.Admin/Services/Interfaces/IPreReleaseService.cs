#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPreReleaseService
{
    PreReleaseWindow GetPreReleaseWindow(ReleaseVersion releaseVersion);

    PreReleaseWindowStatus GetPreReleaseWindowStatus(ReleaseVersion releaseVersion, DateTimeOffset referenceTime);
}
