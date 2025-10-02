using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.NamingUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests;

public abstract class NamingUtilTests
{
    public class SlugFromTitleTests
    {
        [Fact]
        public void CalendarYearTitle()
        {
            Assert.Equal("calendar-year-2019", SlugFromTitle("calendar year 2019"));
        }

        [Fact]
        public void NonCalendarYearTitle()
        {
            Assert.Equal("tax-year-2019-20", SlugFromTitle("tax year 2019/20"));
        }

        [Theory]
        [InlineData("title", "title")]
        [InlineData("TITLE", "title")]
        [InlineData("A sentence with spaces", "a-sentence-with-spaces")]
        [InlineData("A - sentence -  with - - dashes  -  and -- spaces", "a-sentence-with-dashes-and-spaces")]
        [InlineData(
            "A sentence with !@£('\\) non alpha numeric characters",
            "a-sentence-with-non-alpha-numeric-characters"
        )]
        [InlineData(
            "A sentence with non alpha numeric characters at the end !@£('\\)",
            "a-sentence-with-non-alpha-numeric-characters-at-the-end"
        )]
        [InlineData("a sentence with      big     spaces   ", "a-sentence-with-big-spaces")]
        [InlineData("a sentence with numbers 1 2 3 and 4", "a-sentence-with-numbers-1-2-3-and-4")]
        public void EdgeCaseTitles(string title, string expectedSlug)
        {
            Assert.Equal(expectedSlug, SlugFromTitle(title));
        }
    }
}
