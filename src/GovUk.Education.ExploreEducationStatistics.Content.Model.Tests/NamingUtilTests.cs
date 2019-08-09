using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests
{
    public class NamingUtilTests
    {
        
        [Fact]
        public void Title_CalendarYear()
        {
            var title = ReleaseTitle("2019", TimeIdentifier.CalendarYear);
            Assert.Equal("Calendar Year 2019", title);
        }
        
        [Fact]
        public void Title_NonCalendarYear()
        {
            var title1 = ReleaseTitle("2019", TimeIdentifier.TaxYear);
            Assert.Equal("Tax Year 2019/20", title1);
            
            var title2 = ReleaseTitle("2010", TimeIdentifier.TaxYear);
            Assert.Equal("Tax Year 2010/11", title2);
        }

        [Fact]
        public void Title_NoYear()
        {
            var title1 = ReleaseTitle("", TimeIdentifier.TaxYear);
            Assert.Equal("Tax Year", title1);
            
            var title2 = ReleaseTitle(null, TimeIdentifier.TaxYear);
            Assert.Equal("Tax Year", title2);
        }
        
        [Fact]
        public void CoverageTitle()
        {
            var coverageTitle = ReleaseCoverageTitle(TimeIdentifier.TaxYear);
            Assert.Equal("Tax Year", coverageTitle);
        }
        
        [Fact]
        public void ReleaseYearTitle_CalendarYear()
        {
            var coverageTitle = ReleaseYearTitle("2019", TimeIdentifier.CalendarYear);
            Assert.Equal("2019", coverageTitle);
        }
        
        [Fact]
        public void ReleaseYearTitle_NonCalendarYear()
        {
            var coverageTitle = ReleaseYearTitle("2019", TimeIdentifier.TaxYear);
            Assert.Equal("2019/20", coverageTitle);
        }

        [Fact]
        public void ReleaseSlugFromTitle_CalendarYear()
        {
            var slug = SlugFromTitle(ReleaseTitle("2019", TimeIdentifier.CalendarYear));
            Assert.Equal("calendar-year-2019", slug);
        }
        
        [Fact]
        public void ReleaseSlugFromTitle_NonCalendarYear()
        {
            var slug = SlugFromTitle(ReleaseTitle("2019", TimeIdentifier.TaxYear));
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