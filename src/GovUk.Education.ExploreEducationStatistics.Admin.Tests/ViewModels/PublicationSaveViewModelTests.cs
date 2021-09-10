#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.ViewModels
{
    public class PublicationSaveViewModelTests
    {
        [Fact]
        public void ManualSlugOverride()
        {
            Assert.Equal("slug", new PublicationSaveViewModel {Title = "title", Slug = "slug"}.Slug);
        }
    }
}
