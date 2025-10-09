using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Services;

public abstract class SqlIndexHelperTests
{
    public class CreateRandomIndexNameTests : SqlIndexHelperTests
    {
        private readonly SqlStatementsHelper _sqlStatementsHelper = new();

        [Fact]
        public void SingleColumn()
        {
            var indexName = _sqlStatementsHelper.CreateRandomIndexName(tableName: "MyTable", columnName: "Col1");
            Assert.Matches("IX_MyTable_Col1_[a-z0-9]{29}", indexName);
        }

        [Fact]
        public void MultipleColumns()
        {
            var indexName = _sqlStatementsHelper.CreateRandomIndexName(
                tableName: "MyTable",
                columnNames: ["Col1", "Col2"]
            );
            Assert.Matches("IX_MyTable_Col1_Col2_[a-z0-9]{29}", indexName);
        }

        [Fact]
        public void UniqueNamesGenerated()
        {
            var indexName1 = _sqlStatementsHelper.CreateRandomIndexName(tableName: "MyTable", columnName: "Col1");
            var indexName2 = _sqlStatementsHelper.CreateRandomIndexName(tableName: "MyTable", columnName: "Col1");
            Assert.NotEqual(indexName1, indexName2);
        }
    }
}
