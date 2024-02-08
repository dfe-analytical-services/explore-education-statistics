using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Utils;
using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Utils;

public partial class TimePeriodFormatterTests
{
    [GeneratedRegex(@"^[0-9]{4}([0-9]{2})?$")]
    private static partial Regex YearRegex();

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
    [InlineData(199899)]
    [InlineData(199900)]
    [InlineData(200001)]
    [InlineData(200102)]
    public void SixDigitYear_Success(int year)
    {
        var formattedTimePeriod = TimePeriodFormatter.Format(year, TimeIdentifier.AcademicYearQ1);

        var match = YearRegex().Match(year.ToString());
        var firstYear = int.Parse(match.Groups[0].Value.Substring(0, 4));

        Assert.Equal($"{firstYear}/{(firstYear + 1) % 100:D2} Q1", formattedTimePeriod);
    }
}
