#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
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

            Assert.Equal("country_code", columns.Code);
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
            Assert.Equal("country_code", GeographicLevel.Country.CsvCodeColumn());
        }

        [Fact]
        public void CsvOtherColumns()
        {
            var columns = GeographicLevel.LocalAuthority.CsvOtherColumns();

            Assert.Single(columns);
            Assert.Equal("old_la_code", columns[0]);
        }

        [Theory]
        [MemberData(nameof(GeographicLevelColumns))]
        public void CsvColumnsToGeographicLevel(string column, GeographicLevel level)
        {
            Assert.Equal(level, GeographicLevelUtils.CsvColumnsToGeographicLevel[column]);
        }

        public static IEnumerable<object[]> GeographicLevelColumns()
        {
            return EnumUtil.GetEnumValues<GeographicLevel>()
                .SelectMany(
                    level =>
                    {
                        var columns = new List<string> { level.CsvCodeColumn(), level.CsvNameColumn() };

                        columns.AddRange(level.CsvOtherColumns());

                        return columns.Select(column => new object[] { column, level });
                    }
                );
        }
    }
}
