#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

public static class GeographicLevelUtilTests
{
    public class CsvColumnTests
    {
        [Fact]
        public void CsvColumns()
        {
            var columns = GeographicLevel.Country.CsvColumns();

            Assert.Equal(new[] { "country_code" }, columns.Codes);
            Assert.Equal("country_name", columns.Name);
        }

        [Fact]
        public void CsvNameColumn()
        {
            Assert.Equal("country_name", GeographicLevel.Country.CsvNameColumn());
        }

        [Fact]
        public void CsvCodeColumn()
        {
            Assert.Equal(new[] { "country_code" }, GeographicLevel.Country.CsvCodeColumns());
        }

        [Theory]
        [MemberData(nameof(GeographicLevelColumns))]
        public void CsvColumnsToGeographicLevel(string column, GeographicLevel level)
        {
            Assert.Equal(level, GeographicLevelUtils.CsvColumnsToGeographicLevel[column]);
        }

        public static IEnumerable<object[]> GeographicLevelColumns()
        {
            return EnumUtil
                .GetEnums<GeographicLevel>()
                .SelectMany(level =>
                {
                    var columns = new List<string>(level.CsvCodeColumns()) { level.CsvNameColumn() };

                    return columns.Select(column => new object[] { column, level });
                });
        }
    }
}
