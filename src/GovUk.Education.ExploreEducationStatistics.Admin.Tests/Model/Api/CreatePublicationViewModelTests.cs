using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Model.Api
{
    public class CreatePublicationViewModelTests
    {
        [Fact]
        public void ManualSlugOverride()
        {
            Assert.Equal("slug", new SavePublicationViewModel {Title = "title", Slug = "slug"}.Slug);
        }
    }
}