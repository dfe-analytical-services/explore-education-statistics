using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.LinkChecker;

public interface ILinksChecker
{
    public Task<List<LinkDetails>> ExtractReleaseLinksAsync(
        ContentDbContext context,
        CancellationToken cancellationToken
    );

    public Task<List<LinksCsvItem>> TestReleaseLinksAsync(
        List<LinkDetails> contentDetails,
        CancellationToken cancellationToken
    );
}
