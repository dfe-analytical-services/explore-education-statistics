using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Model.Api
{
    public class CreatePublicationViewModelTests
    {
        [Fact]
        public void ManualSlugOverride()
        {
            Assert.Equal("slug", new CreatePublicationViewModel {Title = "title", Slug = "slug"}.Slug);
        }
    }
}