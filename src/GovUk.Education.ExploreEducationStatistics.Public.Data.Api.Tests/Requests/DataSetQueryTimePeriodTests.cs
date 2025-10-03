using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetQueryTimePeriodTests
{
    public class ParseTests
    {
        [Theory]
        [InlineData("2020", "AY")]
        [InlineData("2020/2021", "AY")]
        [InlineData("2020", "CY")]
        public void Success(string period, string code)
        {
            var timePeriod = DataSetQueryTimePeriod.Parse($"{period}|{code}");

            Assert.Equal(period, timePeriod.Period);
            Assert.Equal(code, timePeriod.Code);
        }
    }

    public class ParsedPeriodTests
    {
        [Theory]
        [InlineData("2021", "CY")]
        [InlineData("2021", "CYQ1")]
        [InlineData("2021", "RY")]
        [InlineData("2021", "W5")]
        [InlineData("2021", "M3")]
        public void YearPeriod_DefaultYearFormatIdentifier(string period, string code)
        {
            var timePeriod = new DataSetQueryTimePeriod { Period = period, Code = code };

            Assert.Equal(period, timePeriod.ParsedPeriod());
        }

        [Theory]
        [InlineData("AY")]
        [InlineData("AYQ1")]
        [InlineData("T1")]
        [InlineData("T1T2")]
        [InlineData("FY")]
        [InlineData("FYQ1")]
        [InlineData("P1")]
        [InlineData("TY")]
        [InlineData("TYQ1")]
        public void InvalidYearPeriod_RangeIdentifier_Throws(string code)
        {
            var timePeriod = new DataSetQueryTimePeriod { Period = "invalid", Code = code };

            Assert.Throws<ArgumentOutOfRangeException>(timePeriod.ParsedPeriod);
        }

        [Theory]
        [InlineData("2021/2022", "CY")]
        [InlineData("2021/2022", "CYQ1")]
        [InlineData("2021/2022", "RY")]
        [InlineData("2021/2022", "W5")]
        [InlineData("2021/2022", "M3")]
        public void RangePeriod_DefaultYearIdentifier_Throws(string period, string code)
        {
            var timePeriod = new DataSetQueryTimePeriod { Period = period, Code = code };

            Assert.Throws<ArgumentOutOfRangeException>(timePeriod.ParsedPeriod);
        }

        [Theory]
        [InlineData("2021/2022", "AY", "2021/2022")]
        [InlineData("2021/2022", "AYQ1", "2021/2022")]
        [InlineData("2021/2022", "T1", "2021/2022")]
        [InlineData("2021/2022", "T1T2", "2021/2022")]
        [InlineData("2021", "AY", "2021/2022")]
        [InlineData("2021", "AYQ1", "2021/2022")]
        [InlineData("2021", "T1", "2021/2022")]
        [InlineData("2021", "T1T2", "2021/2022")]
        public void YearOrRangePeriod_AcademicYearIdentifier_ReturnsRangedPeriod(
            string period,
            string code,
            string parsedPeriod
        )
        {
            var timePeriod = new DataSetQueryTimePeriod { Period = period, Code = code };

            Assert.Equal(parsedPeriod, timePeriod.ParsedPeriod());
        }

        [Theory]
        [InlineData("2021/2022", "FY", "2021/2022")]
        [InlineData("2021/2022", "FYQ1", "2021/2022")]
        [InlineData("2021/2022", "P1", "2021/2022")]
        [InlineData("2021/2022", "TY", "2021/2022")]
        [InlineData("2021/2022", "TYQ1", "2021/2022")]
        [InlineData("2021", "FY", "2021/2022")]
        [InlineData("2021", "FYQ1", "2021/2022")]
        [InlineData("2021", "P1", "2021/2022")]
        [InlineData("2021", "TY", "2021/2022")]
        [InlineData("2021", "TYQ1", "2021/2022")]
        public void YearOrRangePeriod_FiscalYearIdentifier_ReturnsRangedPeriod(
            string period,
            string code,
            string parsedPeriod
        )
        {
            var timePeriod = new DataSetQueryTimePeriod { Period = period, Code = code };

            Assert.Equal(parsedPeriod, timePeriod.ParsedPeriod());
        }
    }
}
