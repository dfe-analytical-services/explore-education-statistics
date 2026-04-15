using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.LinkChecker;

public interface ILinksChecker
{
    public Task<List<ContentLink>> ExtractReleaseLinksAsync(
        ContentDbContext context,
        CancellationToken cancellationToken
    );

    public Task<List<LinksCsvItem>> TestReleaseLinksAsync(
        List<ContentLink> contentDetails,
        CancellationToken cancellationToken,
        CurrentEnvironment environment = CurrentEnvironment.Local
    );
}
