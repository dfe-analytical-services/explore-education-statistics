using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public abstract class DataSetQuerySortTests
{
    public class ParseTests
    {
        [Theory]
        [InlineData("time_period", "Asc")]
        [InlineData("time_period", "Desc")]
        [InlineData("location|NAT", "Asc")]
        public void Success(string field, string direction)
        {
            var sort = DataSetQuerySort.Parse($"{field}|{direction}");

            Assert.Equal(field, sort.Field);
            Assert.Equal(direction, sort.Direction);
        }
    }
}
