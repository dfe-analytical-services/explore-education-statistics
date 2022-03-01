#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Model.Data
{
    public class TableBuilderConfigurationTests
    {
        [Fact]
        public void Clone_CreatesDeepCopy()
        {
            var original = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    Columns = new List<TableHeader>
                    {
                        new("original", TableHeaderType.Filter)
                    },
                    Rows = new List<TableHeader>
                    {
                        new("original", TableHeaderType.Indicator)
                    },
                    ColumnGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            new TableHeader("original", TableHeaderType.Filter)
                        }
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            new TableHeader("original", TableHeaderType.TimePeriod)
                        }
                    }
                }
            };

            var clone = original.Clone();

            clone.AssertDeepEqualTo(original);

            clone.TableHeaders.Columns[0].Value = "updated";
            clone.TableHeaders.Rows[0].Value = "updated";
            clone.TableHeaders.ColumnGroups[0][0].Value = "updated";
            clone.TableHeaders.RowGroups[0][0].Value = "updated";

            Assert.Equal("original", original.TableHeaders.Columns[0].Value);
            Assert.Equal("original", original.TableHeaders.Rows[0].Value);
            Assert.Equal("original", original.TableHeaders.ColumnGroups[0][0].Value);
            Assert.Equal("original", original.TableHeaders.RowGroups[0][0].Value);
        }
    }
}
