using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Utils.Tests;

public class TimePeriodFormatterTests
{
    public class FormatLabelTests : TimePeriodFormatterTests
    {
        [Theory]
        [InlineData("1")]
        [InlineData("22")]
        [InlineData("333")]
        [InlineData("55555")]
        [InlineData("666666")]
        [InlineData("7777777")]
        [InlineData("20202021")]
        [InlineData("88888888")]
        [InlineData("999999999")]
        [InlineData("1000000000")]
        public void SingleYearHasInvalidNumberOfDigits_Throws(string year)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodFormatter.FormatLabel(year, TimeIdentifier.AcademicYear)
            );
        }

        [Theory]
        [InlineData("1997/1999")]
        [InlineData("1998/2000")]
        [InlineData("1999/2001")]
        [InlineData("2000/2002")]
        [InlineData("2003/2001")]
        [InlineData("2003/20045")]
        [InlineData("203/2004")]
        [InlineData("20030/2004")]
        public void InvalidRange_Throws(string year)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodFormatter.FormatLabel(year, TimeIdentifier.AcademicYear)
            );
        }

        [Fact]
        public void SingleYear_Success()
        {
            var formattedTimePeriod = TimePeriodFormatter.FormatLabel(
                "2024",
                TimeIdentifier.AcademicYearQ1
            );

            Assert.Equal("2024/25 Q1", formattedTimePeriod);
        }

        [Theory]
        [InlineData("1998/1999", "1998/99 Q1")]
        [InlineData("1999/2000", "1999/00 Q1")]
        [InlineData("2000/2001", "2000/01 Q1")]
        [InlineData("2001/2002", "2001/02 Q1")]
        public void Range_Success(string year, string expected)
        {
            var formattedTimePeriod = TimePeriodFormatter.FormatLabel(
                year,
                TimeIdentifier.AcademicYearQ1
            );

            Assert.Equal(expected, formattedTimePeriod);
        }
    }

    public class FormatFromCsvTests : TimePeriodFormatterTests
    {
        [Theory]
        [InlineData("1")]
        [InlineData("22")]
        [InlineData("333")]
        [InlineData("55555")]
        [InlineData("666666")]
        [InlineData("7777777")]
        [InlineData("20202021")]
        [InlineData("88888888")]
        [InlineData("999999999")]
        [InlineData("1000000000")]
        public void InvalidSingleYear_Throws(string period)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodFormatter.FormatFromCsv(period)
            );
        }

        [Theory]
        [InlineData("199799")]
        [InlineData("199800")]
        [InlineData("199901")]
        [InlineData("200002")]
        [InlineData("200301")]
        public void InvalidSecondYear_Throws(string period)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodFormatter.FormatFromCsv(period)
            );
        }

        [Fact]
        public void SingleYear_Success()
        {
            Assert.Equal("2024", TimePeriodFormatter.FormatFromCsv("2024"));
        }

        [Theory]
        [InlineData("199899", "1998/1999")]
        [InlineData("199900", "1999/2000")]
        [InlineData("200001", "2000/2001")]
        [InlineData("200102", "2001/2002")]
        public void Range_Success(string period, string expected)
        {
            Assert.Equal(expected, TimePeriodFormatter.FormatFromCsv(period));
        }
    }

    public class FormatToCsvTests : TimePeriodFormatterTests
    {
        [Theory]
        [InlineData("1")]
        [InlineData("22")]
        [InlineData("333")]
        [InlineData("55555")]
        [InlineData("666666")]
        [InlineData("7777777")]
        [InlineData("20202021")]
        [InlineData("88888888")]
        [InlineData("999999999")]
        [InlineData("1000000000")]
        public void InvalidSingleYear_Throws(string period)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodFormatter.FormatToCsv(period)
            );
        }

        [Theory]
        [InlineData("1997/1999")]
        [InlineData("1998/2000")]
        [InlineData("1999/2001")]
        [InlineData("2000/2002")]
        [InlineData("2003/2001")]
        [InlineData("2003/20045")]
        [InlineData("203/2004")]
        [InlineData("20030/2004")]
        public void InvalidRange_Throws(string period)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                TimePeriodFormatter.FormatToCsv(period)
            );
        }

        [Fact]
        public void SingleYear_Success()
        {
            Assert.Equal("2024", TimePeriodFormatter.FormatToCsv("2024"));
        }

        [Theory]
        [InlineData("1998/1999", "199899")]
        [InlineData("1999/2000", "199900")]
        [InlineData("2000/2001", "200001")]
        [InlineData("2001/2002", "200102")]
        public void Range_Success(string period, string expected)
        {
            Assert.Equal(expected, TimePeriodFormatter.FormatToCsv(period));
        }
    }
}
