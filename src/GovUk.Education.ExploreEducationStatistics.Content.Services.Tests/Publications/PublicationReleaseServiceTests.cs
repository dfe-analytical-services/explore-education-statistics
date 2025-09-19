using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications;

public abstract class PublicationReleaseServiceTests
{
    public class GetPublicationReleasesTests : PublicationReleaseServiceTests
    {
    }

    private static PublicationReleasesService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
