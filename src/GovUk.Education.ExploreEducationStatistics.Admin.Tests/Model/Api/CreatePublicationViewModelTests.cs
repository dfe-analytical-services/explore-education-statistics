using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Model.Api
{
    public class CreatePublicationViewModelTests
    {
        [Fact]
        public void GenerateSlugFromTitle()
        {
            Assert.Equal("title", new CreatePublicationViewModel {Title = "title"}.Slug);
            Assert.Equal("title", new CreatePublicationViewModel {Title = "TITLE"}.Slug);
            Assert.Equal("a-sentence-with-spaces",
                new CreatePublicationViewModel {Title = "A sentence with spaces"}.Slug);
            Assert.Equal("a-sentence-with-non-alpha-numeric-characters",
                new CreatePublicationViewModel {Title = "A sentence with !@£('\\) non alpha numeric characters"}.Slug);
            Assert.Equal("a-sentence-with-non-alpha-numeric-characters-at-the-end",
                new CreatePublicationViewModel {Title = "A sentence with non alpha numeric characters at the end !@£('\\)"}.Slug);
            Assert.Equal("a-sentence-with-big-spaces",
                new CreatePublicationViewModel {Title = "a sentence with      big     spaces   "}.Slug);
            Assert.Equal("a-sentence-with-big-spaces",
                new CreatePublicationViewModel {Title = "a sentence with      big     spaces   "}.Slug);
            Assert.Equal("a-sentence-with-numbers-1-2-3-and-4",
                new CreatePublicationViewModel {Title = "a sentence with numbers 1 2 3 and 4"}.Slug);
        }
        
        [Fact]
        public void ManualSlugOverride()
        {
            Assert.Equal("slug", new CreatePublicationViewModel {Title = "title", Slug = "slug"}.Slug);
        }
    }
}