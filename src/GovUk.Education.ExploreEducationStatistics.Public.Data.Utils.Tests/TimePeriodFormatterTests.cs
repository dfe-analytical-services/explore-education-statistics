using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Tests;

public class TimePeriodFormatterTests
{
    public class FormatTests : TimePeriodFormatterTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(22)]
        [InlineData(333)]
        [InlineData(55555)]
        [InlineData(7777777)]
        [InlineData(88888888)]
        public void YearHasInvalidNumberOfDigits_Throws(int year)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimePeriodFormatter.Format(year, TimeIdentifier.AcademicYear));
        }

        [Theory]
        [InlineData(199799)]
        [InlineData(199800)]
        [InlineData(199901)]
        [InlineData(200002)]
        [InlineData(200301)]
        public void InvalidSixDigitYear_Throws(int year)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimePeriodFormatter.Format(year, TimeIdentifier.AcademicYear));
        }

        [Fact]
        public void FourDigitYear_Success()
        {
            var formattedTimePeriod = TimePeriodFormatter.Format(2024, TimeIdentifier.AcademicYearQ1);

            Assert.Equal("2024/25 Q1", formattedTimePeriod);
        }

        [Theory]
        [InlineData(199899, "1998/99 Q1")]
        [InlineData(199900, "1999/00 Q1")]
        [InlineData(200001, "2000/01 Q1")]
        [InlineData(200102, "2001/02 Q1")]
        public void SixDigitYear_Success(int year, string expected)
        {
            var formattedTimePeriod = TimePeriodFormatter.Format(year, TimeIdentifier.AcademicYearQ1);

            Assert.Equal(expected, formattedTimePeriod);
        }
    }
}
