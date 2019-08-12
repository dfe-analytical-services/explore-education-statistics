using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class NamingUtilTests
    {
        
        [Fact]
        public void ReleaseSlugFromTitle_CalendarYear()
        {
            var slug = SlugFromTitle("calendar year 2019");
            Assert.Equal("calendar-year-2019", slug);
        }
        
        [Fact]
        public void ReleaseSlugFromTitle_NonCalendarYear()
        {
            var slug = SlugFromTitle("tax year 2019/20");
            Assert.Equal("tax-year-2019-20", slug);
        }
        
        
        [Fact]
        public void GenerateSlugFromTitle()
        {
            Assert.Equal("title", SlugFromTitle("title"));
            Assert.Equal("title", SlugFromTitle("TITLE"));
            Assert.Equal("a-sentence-with-spaces",SlugFromTitle("A sentence with spaces"));
            Assert.Equal("a-sentence-with-non-alpha-numeric-characters",SlugFromTitle("A sentence with !@£('\\) non alpha numeric characters"));
            Assert.Equal("a-sentence-with-non-alpha-numeric-characters-at-the-end", SlugFromTitle("A sentence with non alpha numeric characters at the end !@£('\\)"));
            Assert.Equal("a-sentence-with-big-spaces", SlugFromTitle("a sentence with      big     spaces   "));
            Assert.Equal("a-sentence-with-numbers-1-2-3-and-4", SlugFromTitle("a sentence with numbers 1 2 3 and 4"));
        }
        
    }
}