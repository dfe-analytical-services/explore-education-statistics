using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Query;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures.Parquet;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;

public abstract class DataSetQueryParserTests
{
    private readonly DataFixture _dataFixture = new();

    private readonly Mock<IParquetFilterRepository> _filterRepository = new(MockBehavior.Strict);
    private readonly Mock<IParquetLocationRepository> _locationRepository = new(MockBehavior.Strict);
    private readonly Mock<IParquetTimePeriodRepository> _timePeriodRepository = new(MockBehavior.Strict);

    public class ParseCriteriaFiltersTests : DataSetQueryParserTests
    {
        private readonly DataSetVersion _dataSetVersion;

        public ParseCriteriaFiltersTests()
        {
            _dataSetVersion = DefaultDataSetVersion();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<DataSetQueryLocation>(), default))
                .ReturnsAsync([]);

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, Array.Empty<DataSetQueryTimePeriod>(), default))
                .ReturnsAsync([]);
        }

        [Theory]
        [InlineData("Eq")]
        [InlineData("NotEq")]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task AllComparators_Empty(string comparator)
        {
            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<string>(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters(comparator, [])
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal("", parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("Eq", """data."field_a" = ?""")]
        [InlineData("NotEq", """data."field_a" != ?""")]
        [InlineData("In", """data."field_a" IN (?)""")]
        [InlineData("NotIn", """data."field_a" NOT IN (?)""")]
        public async Task AllComparators_SingleOptionExists(string comparator, string expectedSql)
        {
            var filterOptions = _dataFixture
                .DefaultParquetFilterOption()
                .WithFilterId("field_a")
                .GenerateList(1);

            var queryFilterOptionIds = filterOptions
                .Select(o => o.PublicId)
                .ToList();

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryFilterOptionIds.ToHashSet(), default))
                .ReturnsAsync(filterOptions);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters(comparator, queryFilterOptionIds)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Single(parsed.SqlParameters);
            var sqlParameter = Assert.Single(parsed.SqlParameters);
            Assert.Equal(filterOptions[0].Id, sqlParameter.Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("Eq", "false")]
        [InlineData("NotEq", "true")]
        [InlineData("In", "false")]
        [InlineData("NotIn", "true")]
        public async Task AllComparators_SingleOptionNotFound(string comparator, string expectedSql)
        {
            List<string> queryFilterOptionIds = ["testId1"];

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryFilterOptionIds.ToHashSet(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters(comparator, queryFilterOptionIds)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.FiltersNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.FiltersNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<string>>(warning.Detail);

            Assert.Equal(queryFilterOptionIds, warningDetail.Items);
        }

        [Theory]
        [InlineData("In", """data."field_a" IN (?, ?, ?)""")]
        [InlineData("NotIn", """data."field_a" NOT IN (?, ?, ?)""")]
        public async Task InComparators_MultipleOptionsForSingleFilter(string comparator, string expectedSql)
        {
            var filterOptions = _dataFixture
                .DefaultParquetFilterOption()
                .WithFilterId("field_a")
                .GenerateList(3);

            var queryFilterOptionIds = filterOptions
                .Select(o => o.PublicId)
                .ToList();

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryFilterOptionIds.ToHashSet(), default))
                .ReturnsAsync(filterOptions);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters(comparator, queryFilterOptionIds)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(3, parsed.SqlParameters.Count);
            Assert.Equal(filterOptions[0].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(filterOptions[1].Id, parsed.SqlParameters[1].Argument);
            Assert.Equal(filterOptions[2].Id, parsed.SqlParameters[2].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData(
            "In",
            """
            (data."field_a" IN (?, ?)
             OR data."field_b" IN (?, ?))
            """
        )]
        [InlineData(
            "NotIn",
            """
            (data."field_a" NOT IN (?, ?)
             AND data."field_b" NOT IN (?, ?))
            """
        )]
        public async Task InComparators_MultipleOptionsForMultipleFilters(string comparator, string expectedSql)
        {
            var filterOptions = _dataFixture
                .DefaultParquetFilterOption()
                .ForRange(..2, s => s.SetFilterId("field_a"))
                .ForRange(2..4, s => s.SetFilterId("field_b"))
                .GenerateList();

            var queryFilterOptionIds = filterOptions
                .Select(o => o.PublicId)
                .ToList();

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryFilterOptionIds.ToHashSet(), default))
                .ReturnsAsync(filterOptions);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters(comparator, queryFilterOptionIds)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(4, parsed.SqlParameters.Count);
            Assert.Equal(filterOptions[0].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(filterOptions[1].Id, parsed.SqlParameters[1].Argument);
            Assert.Equal(filterOptions[2].Id, parsed.SqlParameters[2].Argument);
            Assert.Equal(filterOptions[3].Id, parsed.SqlParameters[3].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData(
            "In",
            """
            (data."field_a" IN (?)
             OR data."field_b" IN (?))
            """
        )]
        [InlineData(
            "NotIn",
            """
            (data."field_a" NOT IN (?)
             AND data."field_b" NOT IN (?))
            """
        )]
        public async Task InComparators_MultipleOptionsSomeNotFound(string comparator, string expectedSql)
        {
            var filterOptions = _dataFixture
                .DefaultParquetFilterOption()
                .ForRange(..2, s => s.SetFilterId("field_a"))
                .ForRange(2..4, s => s.SetFilterId("field_b"))
                .GenerateList();

            var queryFilterOptionIds = filterOptions
                .Select(o => o.PublicId)
                .ToList();

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryFilterOptionIds.ToHashSet(), default))
                .ReturnsAsync([filterOptions[1], filterOptions[3]]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters(comparator, queryFilterOptionIds)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(2, parsed.SqlParameters.Count);
            Assert.Equal(filterOptions[1].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(filterOptions[3].Id, parsed.SqlParameters[1].Argument);

            Assert.Empty(queryState.Errors);

            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.FiltersNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.FiltersNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<string>>(warning.Detail);

            Assert.Equal([queryFilterOptionIds[0], queryFilterOptionIds[2]], warningDetail.Items);
        }

        [Theory]
        [InlineData("In", "false")]
        [InlineData("NotIn", "true")]
        public async Task InComparators_MultipleOptionsNoneFound(string comparator, string expectedSql)
        {
            List<string> queryFilterOptionIds = ["missing1", "missing2"];

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryFilterOptionIds.ToHashSet(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters(comparator, queryFilterOptionIds)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);

            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.FiltersNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.FiltersNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<string>>(warning.Detail);

            Assert.Equal(queryFilterOptionIds, warningDetail.Items);
        }
    }

    public class ParseCriteriaGeographicLevels : DataSetQueryParserTests
    {
        private readonly DataSetVersion _dataSetVersion;

        public ParseCriteriaGeographicLevels()
        {
            _dataSetVersion = _dataFixture
                .DefaultDataSetVersion()
                .WithMetaSummary(_dataFixture.DefaultDataSetVersionMetaSummary()
                    .WithGeographicLevels([]));

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<string>(), default))
                .ReturnsAsync([]);

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<DataSetQueryLocation>(), default))
                .ReturnsAsync([]);

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, Array.Empty<DataSetQueryTimePeriod>(), default))
                .ReturnsAsync([]);
        }

        [Theory]
        [InlineData("Eq")]
        [InlineData("NotEq")]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task AllComparators_Empty(string comparator)
        {
            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = CreateCriteriaGeographicLevels(comparator, [])
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal("", parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("Eq", "data.geographic_level = ?")]
        [InlineData("NotEq", "data.geographic_level != ?")]
        [InlineData("In", "data.geographic_level IN (?)")]
        [InlineData("NotIn", "data.geographic_level NOT IN (?)")]
        public async Task AllComparators_SingleOptionExists(string comparator, string expectedSql)
        {
            List<GeographicLevel> geographicLevels = [GeographicLevel.LocalAuthority];

            _dataSetVersion.MetaSummary.GeographicLevels = geographicLevels;

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = CreateCriteriaGeographicLevels(comparator, geographicLevels)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Single(parsed.SqlParameters);
            Assert.Equal(geographicLevels[0].GetEnumLabel(), parsed.SqlParameters[0].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("Eq", "false")]
        [InlineData("NotEq", "true")]
        [InlineData("In", "false")]
        [InlineData("NotIn", "true")]
        public async Task AllComparators_SingleOptionNotFound(string comparator, string expectedSql)
        {
            List<GeographicLevel> geographicLevels = [GeographicLevel.LocalAuthority];

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = CreateCriteriaGeographicLevels(comparator, geographicLevels)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.GeographicLevelsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.GeographicLevelsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<string>>(warning.Detail);

            Assert.Equal(geographicLevels.Select(l => l.GetEnumValue()).ToList(), warningDetail.Items);
        }

        [Theory]
        [InlineData("In", "data.geographic_level IN (?, ?, ?)")]
        [InlineData("NotIn", "data.geographic_level NOT IN (?, ?, ?)")]
        public async Task InComparators_MultipleOptions(string comparator, string expectedSql)
        {
            List<GeographicLevel> geographicLevels =
            [
                GeographicLevel.Region,
                GeographicLevel.LocalAuthority,
                GeographicLevel.School,
            ];

            _dataSetVersion.MetaSummary.GeographicLevels = geographicLevels;

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = CreateCriteriaGeographicLevels(comparator, geographicLevels)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(3, parsed.SqlParameters.Count);
            Assert.Equal(geographicLevels[0].GetEnumLabel(), parsed.SqlParameters[0].Argument);
            Assert.Equal(geographicLevels[1].GetEnumLabel(), parsed.SqlParameters[1].Argument);
            Assert.Equal(geographicLevels[2].GetEnumLabel(), parsed.SqlParameters[2].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("In", "data.geographic_level IN (?, ?)")]
        [InlineData("NotIn", "data.geographic_level NOT IN (?, ?)")]
        public async Task InComparators_MultipleOptionsSomeNotFound(string comparator, string expectedSql)
        {
            List<GeographicLevel> geographicLevels =
            [
                GeographicLevel.Country,
                GeographicLevel.Region,
                GeographicLevel.LocalAuthority,
                GeographicLevel.School,
            ];

            _dataSetVersion.MetaSummary.GeographicLevels = [geographicLevels[1], geographicLevels[3]];

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = CreateCriteriaGeographicLevels(comparator, geographicLevels)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(2, parsed.SqlParameters.Count);
            Assert.Equal(geographicLevels[1].GetEnumLabel(), parsed.SqlParameters[0].Argument);
            Assert.Equal(geographicLevels[3].GetEnumLabel(), parsed.SqlParameters[1].Argument);

            Assert.Empty(queryState.Errors);

            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.GeographicLevelsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.GeographicLevelsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<string>>(warning.Detail);

            List<string> notFoundLevels = [geographicLevels[0].GetEnumValue(), geographicLevels[2].GetEnumValue()];

            Assert.Equal(notFoundLevels, warningDetail.Items);
        }

        [Theory]
        [InlineData("In", "false")]
        [InlineData("NotIn", "true")]
        public async Task InComparators_MultipleOptionsNoneFound(string comparator, string expectedSql)
        {
            List<GeographicLevel> geographicLevels =
            [
                GeographicLevel.Country,
                GeographicLevel.Region,
            ];

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                GeographicLevels = CreateCriteriaGeographicLevels(comparator, geographicLevels)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);

            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.GeographicLevelsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.GeographicLevelsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<string>>(warning.Detail);

            Assert.Equal(geographicLevels.Select(l => l.GetEnumValue()), warningDetail.Items);
        }
    }

    public class ParseCriteriaLocationsTests : DataSetQueryParserTests
    {
        private readonly DataSetVersion _dataSetVersion;

        public ParseCriteriaLocationsTests()
        {
            _dataSetVersion = DefaultDataSetVersion();

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<string>(), default))
                .ReturnsAsync([]);

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, Array.Empty<DataSetQueryTimePeriod>(), default))
                .ReturnsAsync([]);
        }

        [Theory]
        [InlineData("Eq")]
        [InlineData("NotEq")]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task AllComparators_Empty(string comparator)
        {
            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<DataSetQueryLocation>(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Locations = CreateCriteriaLocations(comparator, [])
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal("", parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("Eq", GeographicLevel.Country, "data.locations_nat_id = ?")]
        [InlineData("Eq", GeographicLevel.LocalAuthority, "data.locations_la_id = ?")]
        [InlineData("Eq", GeographicLevel.Provider, "data.locations_prov_id = ?")]
        [InlineData("Eq", GeographicLevel.RscRegion, "data.locations_rsc_id = ?")]
        [InlineData("Eq", GeographicLevel.School, "data.locations_sch_id = ?")]
        [InlineData("NotEq", GeographicLevel.Country, "data.locations_nat_id != ?")]
        [InlineData("NotEq", GeographicLevel.LocalAuthority, "data.locations_la_id != ?")]
        [InlineData("NotEq", GeographicLevel.Provider, "data.locations_prov_id != ?")]
        [InlineData("NotEq", GeographicLevel.RscRegion, "data.locations_rsc_id != ?")]
        [InlineData("NotEq", GeographicLevel.School, "data.locations_sch_id != ?")]
        [InlineData("In", GeographicLevel.Country, "data.locations_nat_id IN (?)")]
        [InlineData("In", GeographicLevel.LocalAuthority, "data.locations_la_id IN (?)")]
        [InlineData("In", GeographicLevel.Provider, "data.locations_prov_id IN (?)")]
        [InlineData("In", GeographicLevel.RscRegion, "data.locations_rsc_id IN (?)")]
        [InlineData("In", GeographicLevel.School, "data.locations_sch_id IN (?)")]
        [InlineData("NotIn", GeographicLevel.Country, "data.locations_nat_id NOT IN (?)")]
        [InlineData("NotIn", GeographicLevel.LocalAuthority, "data.locations_la_id NOT IN (?)")]
        [InlineData("NotIn", GeographicLevel.Provider, "data.locations_prov_id NOT IN (?)")]
        [InlineData("NotIn", GeographicLevel.RscRegion, "data.locations_rsc_id NOT IN (?)")]
        [InlineData("NotIn", GeographicLevel.School, "data.locations_sch_id NOT IN (?)")]
        public async Task AllComparators_SingleOptionExists(
            string comparator,
            GeographicLevel level,
            string expectedSql)
        {
            var locationOptions = _dataFixture
                .DefaultParquetLocationOption(level)
                .GenerateList(1);

            var queryLocation = locationOptions
                .Select(o => MapOptionToQueryLocation(o, level))
                .ToList();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryLocation.ToHashSet(), default))
                .ReturnsAsync(locationOptions);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Locations = CreateCriteriaLocations(comparator, queryLocation)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Single(parsed.SqlParameters);
            Assert.Equal(locationOptions[0].Id, parsed.SqlParameters[0].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("Eq", "false")]
        [InlineData("NotEq", "true")]
        [InlineData("In", "false")]
        [InlineData("NotIn", "true")]
        public async Task AllComparators_SingleOptionNotFound(string comparator, string expectedSql)
        {
            List<DataSetQueryLocation> queryLocation =
            [
                new DataSetQueryLocationId
                {
                    Id = "Invalid ID",
                    Level = GeographicLevel.Country.GetEnumValue()
                }
            ];

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryLocation.ToHashSet(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Locations = CreateCriteriaLocations(comparator, queryLocation),
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.LocationsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.LocationsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<DataSetQueryLocation>>(warning.Detail);

            Assert.Equal(queryLocation, warningDetail.Items);
        }

        [Theory]
        [InlineData("In", GeographicLevel.Country, "data.locations_nat_id IN (?, ?, ?)")]
        [InlineData("In", GeographicLevel.LocalAuthority, "data.locations_la_id IN (?, ?, ?)")]
        [InlineData("In", GeographicLevel.Provider, "data.locations_prov_id IN (?, ?, ?)")]
        [InlineData("In", GeographicLevel.RscRegion, "data.locations_rsc_id IN (?, ?, ?)")]
        [InlineData("In", GeographicLevel.School, "data.locations_sch_id IN (?, ?, ?)")]
        [InlineData("NotIn", GeographicLevel.Country, "data.locations_nat_id NOT IN (?, ?, ?)")]
        [InlineData("NotIn", GeographicLevel.LocalAuthority, "data.locations_la_id NOT IN (?, ?, ?)")]
        [InlineData("NotIn", GeographicLevel.Provider, "data.locations_prov_id NOT IN (?, ?, ?)")]
        [InlineData("NotIn", GeographicLevel.RscRegion, "data.locations_rsc_id NOT IN (?, ?, ?)")]
        [InlineData("NotIn", GeographicLevel.School, "data.locations_sch_id NOT IN (?, ?, ?)")]
        public async Task InComparators_MultipleOptionsForSingleLevel(
            string comparator,
            GeographicLevel level,
            string expectedSql)
        {
            var locationOptions = _dataFixture
                .DefaultParquetLocationOption(level)
                .GenerateList(3);

            var queryLocations = locationOptions
                .Select(o => MapOptionToQueryLocation(o, level))
                .ToList();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryLocations.ToHashSet(), default))
                .ReturnsAsync(locationOptions);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Locations = CreateCriteriaLocations(comparator, queryLocations)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(3, parsed.SqlParameters.Count);
            Assert.Equal(locationOptions[0].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(locationOptions[1].Id, parsed.SqlParameters[1].Argument);
            Assert.Equal(locationOptions[2].Id, parsed.SqlParameters[2].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData(
            "In",
            """
            (data.locations_nat_id IN (?)
             OR data.locations_la_id IN (?, ?)
             OR data.locations_prov_id IN (?, ?))
            """
        )]
        [InlineData(
            "NotIn",
            """
            (data.locations_nat_id NOT IN (?)
             AND data.locations_la_id NOT IN (?, ?)
             AND data.locations_prov_id NOT IN (?, ?))
            """
        )]
        public async Task InComparators_MultipleOptionsForMultipleLevels(string comparator, string expectedSql)
        {
            var locationOptions = _dataFixture
                .DefaultParquetLocationOption()
                .ForRange(..1, s => s.SetDefaults())
                .ForRange(1..3, s => s.SetDefaults(GeographicLevel.LocalAuthority))
                .ForRange(3..5, s => s.SetDefaults(GeographicLevel.Provider))
                .GenerateList();

            var queryLocations = locationOptions
                .Select(o => MapOptionToQueryLocation(o, EnumUtil.GetFromEnumValue<GeographicLevel>(o.Level)))
                .ToList();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryLocations.ToHashSet(), default))
                .ReturnsAsync(locationOptions);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Locations = CreateCriteriaLocations(comparator, queryLocations)
            };
            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(5, parsed.SqlParameters.Count);
            Assert.Equal(locationOptions[0].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(locationOptions[1].Id, parsed.SqlParameters[1].Argument);
            Assert.Equal(locationOptions[2].Id, parsed.SqlParameters[2].Argument);
            Assert.Equal(locationOptions[3].Id, parsed.SqlParameters[3].Argument);
            Assert.Equal(locationOptions[4].Id, parsed.SqlParameters[4].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData(
            "In",
            """
            (data.locations_la_id IN (?)
             OR data.locations_prov_id IN (?))
            """
        )]
        [InlineData(
            "NotIn",
            """
            (data.locations_la_id NOT IN (?)
             AND data.locations_prov_id NOT IN (?))
            """
        )]
        public async Task InComparators_MultipleOptionsSomeNotFound(string comparator, string expectedSql)
        {
            var locationOptions = _dataFixture
                .DefaultParquetLocationOption()
                .ForRange(..1, s => s.SetDefaults())
                .ForRange(1..3, s => s.SetDefaults(GeographicLevel.LocalAuthority))
                .ForRange(3..5, s => s.SetDefaults(GeographicLevel.Provider))
                .GenerateList();

            var queryLocations = locationOptions
                .Select(o => MapOptionToQueryLocation(o, EnumUtil.GetFromEnumValue<GeographicLevel>(o.Level)))
                .ToList();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryLocations.ToHashSet(), default))
                .ReturnsAsync([locationOptions[1], locationOptions[4]]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Locations = CreateCriteriaLocations(comparator, queryLocations)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(2, parsed.SqlParameters.Count);
            Assert.Equal(locationOptions[1].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(locationOptions[4].Id, parsed.SqlParameters[1].Argument);

            Assert.Empty(queryState.Errors);

            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.LocationsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.LocationsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<DataSetQueryLocation>>(warning.Detail);

            Assert.Equal([queryLocations[0], queryLocations[2], queryLocations[3]], warningDetail.Items);
        }

        [Theory]
        [InlineData("In", "false")]
        [InlineData("NotIn", "true")]
        public async Task InComparators_MultipleOptionsNoneFound(string comparator, string expectedSql)
        {
            var locationOptions = _dataFixture
                .DefaultParquetLocationOption()
                .ForIndex(0, s => s.SetDefaults())
                .ForIndex(1, s => s.SetDefaults(GeographicLevel.LocalAuthority))
                .ForIndex(2, s => s.SetDefaults(GeographicLevel.Provider))
                .GenerateList();

            var queryLocations = locationOptions
                .Select(o => MapOptionToQueryLocation(o, EnumUtil.GetFromEnumValue<GeographicLevel>(o.Level)))
                .ToList();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryLocations.ToHashSet(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Locations = CreateCriteriaLocations(comparator, queryLocations)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);

            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.LocationsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.LocationsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<DataSetQueryLocation>>(warning.Detail);

            Assert.Equal(queryLocations, warningDetail.Items);
        }
    }

    public class ParseCriteriaTimePeriodsTests : DataSetQueryParserTests
    {
        private readonly DataSetVersion _dataSetVersion;

        public ParseCriteriaTimePeriodsTests()
        {
            _dataSetVersion = DefaultDataSetVersion();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<DataSetQueryLocation>(), default))
                .ReturnsAsync([]);

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<string>(), default))
                .ReturnsAsync([]);
        }

        [Theory]
        [InlineData("Eq")]
        [InlineData("NotEq")]
        [InlineData("In")]
        [InlineData("NotIn")]
        [InlineData("Gt")]
        [InlineData("Gte")]
        [InlineData("Lt")]
        [InlineData("Lte")]
        public async Task AllComparators_Empty(string comparator)
        {
            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, Array.Empty<DataSetQueryTimePeriod>(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                TimePeriods = CreateCriteriaTimePeriods(comparator, [])
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal("", parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("Eq", "data.time_period_id = ?")]
        [InlineData("NotEq", "data.time_period_id != ?")]
        [InlineData("In", "data.time_period_id IN (?)")]
        [InlineData("NotIn", "data.time_period_id NOT IN (?)")]
        public async Task AllComparators_SingleTimePeriodExists(string comparator, string expectedSql)
        {
            var timePeriods = _dataFixture
                .DefaultParquetTimePeriod()
                .WithPeriod("202324")
                .WithIdentifier(TimeIdentifier.AcademicYear.GetEnumLabel())
                .GenerateList(1);

            var queryTimePeriods = timePeriods
                .Select(o => new DataSetQueryTimePeriod
                {
                    Code = EnumUtil.GetFromEnumLabel<TimeIdentifier>(o.Identifier).GetEnumValue(),
                    Period = TimePeriodFormatter.FormatFromCsv(o.Period)
                })
                .ToList();

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, queryTimePeriods, default))
                .ReturnsAsync(timePeriods);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                TimePeriods = CreateCriteriaTimePeriods(comparator, queryTimePeriods)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Single(parsed.SqlParameters);
            Assert.Equal(timePeriods[0].Id, parsed.SqlParameters[0].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("Eq", "false")]
        [InlineData("NotEq", "true")]
        [InlineData("In", "false")]
        [InlineData("NotIn", "true")]
        public async Task AllComparators_SingleTimePeriodNotFound(string comparator, string expectedSql)
        {
            List<DataSetQueryTimePeriod> queryTimePeriods =
            [
                new DataSetQueryTimePeriod
                {
                    Code = TimeIdentifier.AcademicYear.GetEnumValue(),
                    Period = "2023/2024"
                }
            ];

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, queryTimePeriods, default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                TimePeriods = CreateCriteriaTimePeriods(comparator, queryTimePeriods)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.TimePeriodsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.TimePeriodsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<DataSetQueryTimePeriod>>(warning.Detail);

            Assert.Equal(queryTimePeriods, warningDetail.Items);
        }

        [Theory]
        [InlineData("In", "data.time_period_id IN (?, ?, ?)")]
        [InlineData("NotIn", "data.time_period_id NOT IN (?, ?, ?)")]
        public async Task InComparators_MultipleTimePeriods(string comparator, string expectedSql)
        {
            var timePeriods = _dataFixture
                .DefaultParquetTimePeriod()
                .WithIdentifier(TimeIdentifier.AcademicYear.GetEnumLabel())
                .ForIndex(0, s => s.SetPeriod("202021"))
                .ForIndex(1, s => s.SetPeriod("202122"))
                .ForIndex(2, s => s.SetPeriod("202223"))
                .GenerateList();

            var queryTimePeriods = timePeriods
                .Select(o => new DataSetQueryTimePeriod
                {
                    Code = EnumUtil.GetFromEnumLabel<TimeIdentifier>(o.Identifier).GetEnumValue(),
                    Period = TimePeriodFormatter.FormatFromCsv(o.Period)
                })
                .ToList();

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, queryTimePeriods, default))
                .ReturnsAsync(timePeriods);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                TimePeriods = CreateCriteriaTimePeriods(comparator, queryTimePeriods)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(3, parsed.SqlParameters.Count);
            Assert.Equal(timePeriods[0].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(timePeriods[1].Id, parsed.SqlParameters[1].Argument);
            Assert.Equal(timePeriods[2].Id, parsed.SqlParameters[2].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Theory]
        [InlineData("In", "data.time_period_id IN (?, ?)")]
        [InlineData("NotIn", "data.time_period_id NOT IN (?, ?)")]
        public async Task InComparators_MultipleTimePeriodsSomeNotFound(string comparator, string expectedSql)
        {
            var timePeriods = _dataFixture
                .DefaultParquetTimePeriod()
                .WithIdentifier(TimeIdentifier.AcademicYear.GetEnumLabel())
                .ForIndex(0, s => s.SetPeriod("202021"))
                .ForIndex(1, s => s.SetPeriod("202122"))
                .ForIndex(2, s => s.SetPeriod("202223"))
                .ForIndex(3, s => s.SetPeriod("202324"))
                .GenerateList();

            var queryTimePeriods = timePeriods
                .Select(o => new DataSetQueryTimePeriod
                {
                    Code = EnumUtil.GetFromEnumLabel<TimeIdentifier>(o.Identifier).GetEnumValue(),
                    Period = TimePeriodFormatter.FormatFromCsv(o.Period)
                })
                .ToList();

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, queryTimePeriods, default))
                .ReturnsAsync([timePeriods[0], timePeriods[2]]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                TimePeriods = CreateCriteriaTimePeriods(comparator, queryTimePeriods)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(2, parsed.SqlParameters.Count);
            Assert.Equal(timePeriods[0].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(timePeriods[2].Id, parsed.SqlParameters[1].Argument);

            Assert.Empty(queryState.Errors);

            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.TimePeriodsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.TimePeriodsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<DataSetQueryTimePeriod>>(warning.Detail);

            Assert.Equal([queryTimePeriods[1], queryTimePeriods[3]], warningDetail.Items);
        }

        [Theory]
        [InlineData("In", "false")]
        [InlineData("NotIn", "true")]
        public async Task InComparators_MultipleTimePeriodsNoneFound(string comparator, string expectedSql)
        {
            List<DataSetQueryTimePeriod> queryTimePeriods =
            [
                new DataSetQueryTimePeriod
                {
                    Code = TimeIdentifier.AcademicYear.GetEnumValue(),
                    Period = "2020/2021"
                },
                new DataSetQueryTimePeriod
                {
                    Code = TimeIdentifier.CalendarYear.GetEnumValue(),
                    Period = "2024"
                }
            ];

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, queryTimePeriods, default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                TimePeriods = CreateCriteriaTimePeriods(comparator, queryTimePeriods)
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);

            var warning = Assert.Single(queryState.Warnings);

            Assert.Equal(ValidationMessages.TimePeriodsNotFound.Message, warning.Message);
            Assert.Equal(ValidationMessages.TimePeriodsNotFound.Code, warning.Code);

            var warningDetail = Assert.IsType<NotFoundItemsErrorDetail<DataSetQueryTimePeriod>>(warning.Detail);

            Assert.Equal(queryTimePeriods, warningDetail.Items);
        }
    }

    public class ParseCriteriaMixedTest : DataSetQueryParserTests
    {
        private readonly DataSetVersion _dataSetVersion;

        public ParseCriteriaMixedTest()
        {
            _dataSetVersion = DefaultDataSetVersion();
        }

        [Fact]
        public async Task NoFacets()
        {
            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<string>(), default))
                .ReturnsAsync([]);

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<DataSetQueryLocation>(), default))
                .ReturnsAsync([]);

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, Array.Empty<DataSetQueryTimePeriod>(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets();

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal("", parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Fact]
        public async Task AllFacetsWithMixedComparatorsAndNoOptions()
        {
            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<string>(), default))
                .ReturnsAsync([]);

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, Array.Empty<DataSetQueryLocation>(), default))
                .ReturnsAsync([]);

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, Array.Empty<DataSetQueryTimePeriod>(), default))
                .ReturnsAsync([]);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters("Eq", []),
                GeographicLevels = CreateCriteriaGeographicLevels("In", []),
                Locations = CreateCriteriaLocations("NotEq", []),
                TimePeriods = CreateCriteriaTimePeriods("Gt", []),
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            Assert.Equal("", parsed.Sql);
            Assert.Empty(parsed.SqlParameters);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Fact]
        public async Task AllFacetsWithMixedComparatorsAndSingleOption()
        {
            // Filter options

            var filterOptions = _dataFixture
                .DefaultParquetFilterOption()
                .WithFilterId("field_a")
                .GenerateList(1);

            var queryFilterOptionIds = filterOptions
                .Select(o => o.PublicId)
                .ToList();

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryFilterOptionIds.ToHashSet(), default))
                .ReturnsAsync(filterOptions);

            // Geographic levels

            List<GeographicLevel> geographicLevels = [GeographicLevel.LocalAuthority];

            _dataSetVersion.MetaSummary.GeographicLevels = geographicLevels;

            // Location options

            var locationOptions = _dataFixture
                .DefaultParquetLocationOption(GeographicLevel.Region)
                .GenerateList(1);

            var queryLocations = locationOptions
                .Select(o => MapOptionToQueryLocation(o, GeographicLevel.Region))
                .ToList();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryLocations.ToHashSet(), default))
                .ReturnsAsync(locationOptions);

            // Time periods

            var timePeriods = _dataFixture
                .DefaultParquetTimePeriod()
                .WithPeriod("2020")
                .WithIdentifier(TimeIdentifier.CalendarYear.GetEnumLabel())
                .GenerateList(1);

            var queryTimePeriods = timePeriods
                .Select(o => new DataSetQueryTimePeriod
                {
                    Code = EnumUtil.GetFromEnumLabel<TimeIdentifier>(o.Identifier).GetEnumValue(),
                    Period = TimePeriodFormatter.FormatFromCsv(o.Period)
                })
                .ToList();

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, queryTimePeriods, default))
                .ReturnsAsync(timePeriods);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters("Eq", queryFilterOptionIds),
                GeographicLevels = CreateCriteriaGeographicLevels("In", geographicLevels),
                Locations = CreateCriteriaLocations("NotEq", queryLocations),
                TimePeriods = CreateCriteriaTimePeriods("Gt", queryTimePeriods),
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            const string expectedSql = """
                                       data."field_a" = ?
                                       AND data.geographic_level IN (?)
                                       AND data.locations_reg_id != ?
                                       AND data.time_period_id > ?
                                       """;

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(4, parsed.SqlParameters.Count);
            Assert.Equal(filterOptions[0].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(geographicLevels[0].GetEnumLabel(), parsed.SqlParameters[1].Argument);
            Assert.Equal(locationOptions[0].Id, parsed.SqlParameters[2].Argument);
            Assert.Equal(timePeriods[0].Id, parsed.SqlParameters[3].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }

        [Fact]
        public async Task AllFacetsWithMixedComparatorsAndMultipleOptions()
        {
            // Filter options

            var filterOptions = _dataFixture
                .DefaultParquetFilterOption()
                .ForIndex(0, s => s.SetFilterId("field_a"))
                .ForIndex(1, s => s.SetFilterId("field_b"))
                .GenerateList();

            var queryFilterOptionIds = filterOptions
                .Select(o => o.PublicId)
                .ToList();

            _filterRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryFilterOptionIds.ToHashSet(), default))
                .ReturnsAsync(filterOptions);

            // Geographic levels

            List<GeographicLevel> geographicLevels = [GeographicLevel.Country, GeographicLevel.Region];

            _dataSetVersion.MetaSummary.GeographicLevels = geographicLevels;

            // Location options

            var locationOptions = _dataFixture
                .DefaultParquetLocationOption()
                .ForIndex(0, s => s.SetDefaults(GeographicLevel.Region))
                .ForIndex(1, s => s.SetDefaults(GeographicLevel.LocalAuthority))
                .ForIndex(2, s => s.SetDefaults(GeographicLevel.LocalAuthority))
                .GenerateList();

            var queryLocations = locationOptions
                .Select(o => MapOptionToQueryLocation(o, EnumUtil.GetFromEnumValue<GeographicLevel>(o.Level)))
                .ToList();

            _locationRepository
                .Setup(r => r.ListOptions(_dataSetVersion, queryLocations.ToHashSet(), default))
                .ReturnsAsync(locationOptions);

            // Time periods

            var timePeriods = _dataFixture
                .DefaultParquetTimePeriod()
                .WithIdentifier(TimeIdentifier.CalendarYear.GetEnumLabel())
                .ForIndex(0, s => s.SetPeriod("2020"))
                .ForIndex(1, s => s.SetPeriod("2021"))
                .GenerateList();

            var queryTimePeriods = timePeriods
                .Select(o => new DataSetQueryTimePeriod
                {
                    Code = EnumUtil.GetFromEnumLabel<TimeIdentifier>(o.Identifier).GetEnumValue(),
                    Period = TimePeriodFormatter.FormatFromCsv(o.Period)
                })
                .ToList();

            _timePeriodRepository
                .Setup(r => r.List(_dataSetVersion, queryTimePeriods, default))
                .ReturnsAsync(timePeriods);

            var service = BuildService();

            var queryState = new QueryState();
            var criteria = new DataSetQueryCriteriaFacets
            {
                Filters = CreateCriteriaFilters("NotIn", queryFilterOptionIds),
                GeographicLevels = CreateCriteriaGeographicLevels("NotIn", geographicLevels),
                Locations = CreateCriteriaLocations("In", queryLocations),
                TimePeriods = CreateCriteriaTimePeriods("In", queryTimePeriods),
            };

            var parsed = await service.ParseCriteria(criteria, _dataSetVersion, queryState);

            const string expectedSql = """
                                       (data."field_a" NOT IN (?)
                                        AND data."field_b" NOT IN (?))
                                       AND data.geographic_level NOT IN (?, ?)
                                       AND (data.locations_reg_id IN (?)
                                        OR data.locations_la_id IN (?, ?))
                                       AND data.time_period_id IN (?, ?)
                                       """;

            Assert.Equal(expectedSql, parsed.Sql);
            Assert.Equal(9, parsed.SqlParameters.Count);
            Assert.Equal(filterOptions[0].Id, parsed.SqlParameters[0].Argument);
            Assert.Equal(filterOptions[1].Id, parsed.SqlParameters[1].Argument);
            Assert.Equal(geographicLevels[0].GetEnumLabel(), parsed.SqlParameters[2].Argument);
            Assert.Equal(geographicLevels[1].GetEnumLabel(), parsed.SqlParameters[3].Argument);
            Assert.Equal(locationOptions[0].Id, parsed.SqlParameters[4].Argument);
            Assert.Equal(locationOptions[1].Id, parsed.SqlParameters[5].Argument);
            Assert.Equal(locationOptions[2].Id, parsed.SqlParameters[6].Argument);
            Assert.Equal(timePeriods[0].Id, parsed.SqlParameters[7].Argument);
            Assert.Equal(timePeriods[1].Id, parsed.SqlParameters[8].Argument);

            Assert.Empty(queryState.Errors);
            Assert.Empty(queryState.Warnings);
        }
    }

    private DataSetVersion DefaultDataSetVersion() => _dataFixture
        .DefaultDataSetVersion()
        .WithMetaSummary(
            _dataFixture.DefaultDataSetVersionMetaSummary()
                .WithGeographicLevels([GeographicLevel.Country, GeographicLevel.Region])
        );

    private static DataSetQueryCriteriaFilters CreateCriteriaFilters(
        string comparator,
        IReadOnlyList<string> filterOptionIds)
    {
        return comparator switch
        {
            nameof(DataSetQueryCriteriaFilters.Eq) =>
                new DataSetQueryCriteriaFilters { Eq = filterOptionIds.Count > 0 ? filterOptionIds[0] : null },
            nameof(DataSetQueryCriteriaFilters.NotEq) =>
                new DataSetQueryCriteriaFilters { NotEq = filterOptionIds.Count > 0 ? filterOptionIds[0] : null },
            nameof(DataSetQueryCriteriaFilters.In) =>
                new DataSetQueryCriteriaFilters { In = filterOptionIds },
            nameof(DataSetQueryCriteriaFilters.NotIn) =>
                new DataSetQueryCriteriaFilters { NotIn = filterOptionIds },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null)
        };
    }

    private static DataSetQueryCriteriaGeographicLevels CreateCriteriaGeographicLevels(
        string comparator,
        IReadOnlyList<GeographicLevel> geographicLevels)
    {
        return comparator switch
        {
            nameof(DataSetQueryCriteriaGeographicLevels.Eq) => new DataSetQueryCriteriaGeographicLevels
            {
                Eq = geographicLevels.Count > 0 ? geographicLevels[0].GetEnumValue() : null
            },
            nameof(DataSetQueryCriteriaGeographicLevels.NotEq) => new DataSetQueryCriteriaGeographicLevels
            {
                NotEq = geographicLevels.Count > 0 ? geographicLevels[0].GetEnumValue() : null
            },
            nameof(DataSetQueryCriteriaGeographicLevels.In) => new DataSetQueryCriteriaGeographicLevels
            {
                In = geographicLevels.Select(l => l.GetEnumValue()).ToList()
            },
            nameof(DataSetQueryCriteriaGeographicLevels.NotIn) => new DataSetQueryCriteriaGeographicLevels
            {
                NotIn = geographicLevels.Select(l => l.GetEnumValue()).ToList()
            },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null)
        };
    }

    private static DataSetQueryCriteriaLocations CreateCriteriaLocations(
        string comparator,
        IReadOnlyList<DataSetQueryLocation> locations)
    {
        return comparator switch
        {
            nameof(DataSetQueryCriteriaLocations.Eq) =>
                new DataSetQueryCriteriaLocations { Eq = locations.Count > 0 ? locations[0] : null },
            nameof(DataSetQueryCriteriaLocations.NotEq) =>
                new DataSetQueryCriteriaLocations { NotEq = locations.Count > 0 ? locations[0] : null },
            nameof(DataSetQueryCriteriaLocations.In) =>
                new DataSetQueryCriteriaLocations { In = locations },
            nameof(DataSetQueryCriteriaLocations.NotIn) =>
                new DataSetQueryCriteriaLocations { NotIn = locations },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null)
        };
    }

    private static DataSetQueryCriteriaTimePeriods CreateCriteriaTimePeriods(
        string comparator,
        IReadOnlyList<DataSetQueryTimePeriod> timePeriods)
    {
        return comparator switch
        {
            nameof(DataSetQueryCriteriaTimePeriods.Eq) =>
                new DataSetQueryCriteriaTimePeriods { Eq = timePeriods.Count > 0 ? timePeriods[0] : null },
            nameof(DataSetQueryCriteriaTimePeriods.NotEq) =>
                new DataSetQueryCriteriaTimePeriods { NotEq = timePeriods.Count > 0 ? timePeriods[0] : null },
            nameof(DataSetQueryCriteriaTimePeriods.In) =>
                new DataSetQueryCriteriaTimePeriods { In = timePeriods },
            nameof(DataSetQueryCriteriaTimePeriods.NotIn) =>
                new DataSetQueryCriteriaTimePeriods { NotIn = timePeriods },
            nameof(DataSetQueryCriteriaTimePeriods.Gt) =>
                new DataSetQueryCriteriaTimePeriods { Gt = timePeriods.Count > 0 ? timePeriods[0] : null },
            nameof(DataSetQueryCriteriaTimePeriods.Gte) =>
                new DataSetQueryCriteriaTimePeriods { Gte = timePeriods.Count > 0 ? timePeriods[0] : null },
            nameof(DataSetQueryCriteriaTimePeriods.Lt) =>
                new DataSetQueryCriteriaTimePeriods { Lt = timePeriods.Count > 0 ? timePeriods[0] : null },
            nameof(DataSetQueryCriteriaTimePeriods.Lte) =>
                new DataSetQueryCriteriaTimePeriods { Lte = timePeriods.Count > 0 ? timePeriods[0] : null },
            _ => throw new ArgumentOutOfRangeException(nameof(comparator), comparator, null)
        };
    }

    private static DataSetQueryLocation MapOptionToQueryLocation(ParquetLocationOption option, GeographicLevel level)
        => level switch
        {
            GeographicLevel.LocalAuthority => option switch
            {
                { Code: not null } =>
                    new DataSetQueryLocationLocalAuthorityCode { Level = option.Level, Code = option.Code },
                { OldCode: not null } =>
                    new DataSetQueryLocationLocalAuthorityOldCode { Level = option.Level, OldCode = option.OldCode },
                _ => throw new NullReferenceException(
                    $"{nameof(option.Code)} and {nameof(option.OldCode)} cannot both be null")
            },
            GeographicLevel.Provider => option switch
            {
                { Ukprn: not null } =>
                    new DataSetQueryLocationProviderUkprn { Level = option.Level, Ukprn = option.Ukprn! },
                _ => throw new NullReferenceException($"{nameof(option.Ukprn)} cannot both be null")
            },
            GeographicLevel.RscRegion => new DataSetQueryLocationId
            {
                Level = option.Level,
                Id  = option.PublicId,
            },
            GeographicLevel.School => option switch
            {
                { Urn: not null } =>
                    new DataSetQueryLocationSchoolUrn { Level = option.Level, Urn = option.Urn },
                { LaEstab: not null } =>
                    new DataSetQueryLocationSchoolLaEstab { Level = option.Level, LaEstab = option.LaEstab },
                _ => throw new NullReferenceException(
                    $"{nameof(option.Urn)} and {nameof(option.LaEstab)} cannot both be null")
            },
            _ => option switch
            {
                { Code: not null } =>
                    new DataSetQueryLocationCode { Level = option.Level, Code = option.Code },
                _ => throw new NullReferenceException($"{nameof(option.Code)} cannot be null")
            }
        };

    private DataSetQueryParser BuildService(
        IParquetFilterRepository? filterRepository = null,
        IParquetLocationRepository? locationRepository = null,
        IParquetTimePeriodRepository? timePeriodRepository = null)
    {
        return new DataSetQueryParser(
            filterRepository ?? _filterRepository.Object,
            locationRepository ?? _locationRepository.Object,
            timePeriodRepository ?? _timePeriodRepository.Object
        );
    }
}
