#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests;

public class PublicationSaveRequestTests
{
    [Fact]
    public void ManualSlugOverride()
    {
        Assert.Equal("slug", new PublicationSaveRequest {Title = "title", Slug = "slug"}.Slug);
    }
}
