using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures.Parquet;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

public abstract class DataSetQueryParserTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly Mock<IParquetFilterOptionRepository> _filterOptionRepository = new(MockBehavior.Strict);
    private readonly Mock<IParquetLocationOptionRepository> _locationOptionRepository = new(MockBehavior.Strict);
    private readonly Mock<IParquetTimePeriodRepository> _timePeriodRepository = new(MockBehavior.Strict);

    public class ParseCriteriaFiltersTests : DataSetQueryParserTests
    {
        public ParseCriteriaFiltersTests()
        {
            _locationOptionRepository
                .Setup(
                    r => r
                        .List(It.IsAny<DataSetVersion>(), Array.Empty<DataSetQueryLocation>(), default)
                )
                .ReturnsAsync([]);

            _timePeriodRepository
                .Setup(
                    r => r
                        .List(It.IsAny<DataSetVersion>(), Array.Empty<DataSetQueryTimePeriod>(), default)
                )
                .ReturnsAsync([]);
        }

        [Theory]
        [MemberData(nameof(SuccessData))]
        public async Task Success(
            DataSetQueryCriteria criteria,
            HashSet<string> expectedOptionIds,
            string expectedSql)
        {
            DataSetVersion dataSetVersion = _dataFixture.DefaultDataSetVersion()
                .WithMetaSummary(
                    _dataFixture.DefaultDataSetVersionMetaSummary()
                        .WithGeographicLevels([GeographicLevel.Country])
                );

            var queryState = new QueryState();

            _filterOptionRepository
                .Setup(
                    r =>
                        r.List(dataSetVersion, expectedOptionIds, It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(
                    _dataFixture.DefaultParquetFilterOption()
                        .WithPublicId("test")
                        .WithColumnName("column_name")
                        .GenerateList(1)
                );

            var service = BuildService();

            var criteria = CreateFiltersFacets(comparator, publicIds);

            var parsed = await service.ParseCriteria(criteria, dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(expectedSql, parsed.SqlParameters);
        }

        public static TheoryData<SuccessTestCase> SuccessData =>
        [
            new SuccessTestCase
            {
                Comparator = nameof(DataSetQueryCriteriaFilters.Eq),
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters { Eq = "test" }
                },
                ExpectedOptionIds = ["test"],
                ExpectedSql = ""
            }
        ];

        private record SuccessTestCase
        {
            public required string Comparator { get; init; }

            public required HashSet<string> ExpectedOptionIds { get; init; }

            public required string ExpectedSql { get; init; }
        }
    }

    private static DataSetQueryCriteriaFacets CreateFiltersFacets(string comparator, IReadOnlyList<string> optionIds)
    {
        var filters = comparator switch
        {
            nameof(DataSetQueryCriteriaFilters.Eq) => new DataSetQueryCriteriaFilters { Eq = optionIds[0] },
            nameof(DataSetQueryCriteriaFilters.NotEq) => new DataSetQueryCriteriaFilters { NotEq = optionIds[0] },
            nameof(DataSetQueryCriteriaFilters.In) => new DataSetQueryCriteriaFilters { In = optionIds },
            nameof(DataSetQueryCriteriaFilters.NotIn) => new DataSetQueryCriteriaFilters { NotIn = optionIds },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null)
        };

        return new DataSetQueryCriteriaFacets { Filters = filters };
    }

    private DataSetQueryParser BuildService(
        IParquetFilterOptionRepository? filterOptionRepository = null,
        IParquetLocationOptionRepository? locationOptionRepository = null,
        IParquetTimePeriodRepository? timePeriodRepository = null)
    {
        return new DataSetQueryParser(
            filterOptionRepository ?? _filterOptionRepository.Object,
            locationOptionRepository ?? _locationOptionRepository.Object,
            timePeriodRepository ?? _timePeriodRepository.Object
        );
    }
}
