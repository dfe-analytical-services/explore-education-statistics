using System.Net.Http.Json;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Tests;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class DataSetsControllerPostQueryTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/v1/data-sets";

    private readonly TestDataSetVersionPathResolver _dataSetVersionPathResolver = new()
    {
        Directory = "AbsenceSchool"
    };

    public class AccessTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData(DataSetVersionStatus.Processing)]
        [InlineData(DataSetVersionStatus.Failed)]
        [InlineData(DataSetVersionStatus.Draft)]
        [InlineData(DataSetVersionStatus.Mapping)]
        [InlineData(DataSetVersionStatus.Withdrawn)]
        [InlineData(DataSetVersionStatus.Cancelled)]
        public async Task VersionNotAvailable_Returns403(DataSetVersionStatus versionStatus)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion(versionStatus);

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"]
                }
            );

            response.AssertForbidden();
        }

        [Theory]
        [InlineData(DataSetVersionStatus.Published)]
        [InlineData(DataSetVersionStatus.Deprecated)]
        public async Task VersionAvailable_Returns200(DataSetVersionStatus versionStatus)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion(versionStatus);

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"]
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(216, viewModel.Paging.TotalResults);

            Assert.Empty(viewModel.Warnings);

            Assert.Equal(216, viewModel.Results.Count);
        }

        [Fact]
        public async Task DataSetDoesNotExist_Returns404()
        {
            var response = await QueryDataSet(
                dataSetId: Guid.NewGuid(),
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"]
                }
            );

            response.AssertNotFound();
        }

        [Fact]
        public async Task VersionDoesNotExist_Returns404()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                dataSetVersion: "2.0",
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"]
                }
            );

            response.AssertNotFound();
        }
    }

    public class IndicatorValidationTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Fact]
        public async Task Empty_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = []
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError("indicators");
        }

        [Fact]
        public async Task Blank_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["", " ", "  "]
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(3, validationProblem.Errors.Count);

            validationProblem.AssertHasNotEmptyError("indicators[0]");
            validationProblem.AssertHasNotEmptyError("indicators[1]");
            validationProblem.AssertHasNotEmptyError("indicators[2]");
        }

        [Fact]
        public async Task TooLong_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [new string('a', 41), new string('a', 42)]
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(2, validationProblem.Errors.Count);

            validationProblem.AssertHasMaximumLengthError("indicators[0]", maxLength: 40);
            validationProblem.AssertHasMaximumLengthError("indicators[1]", maxLength: 40);
        }

        [Fact]
        public async Task MissingParam_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var client = BuildApp().CreateClient();

            var response = await client.PostAsJsonAsync(
                $"{BaseUrl}/{dataSetVersion.DataSetId}/query",
                new {}
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError("indicators");
        }

        [Fact]
        public async Task NotFound_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] notFoundIndicators = ["invalid1", "invalid2", "invalid3"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = notFoundIndicators
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasIndicatorsNotFoundError("indicators", notFoundIndicators);
        }
    }

    public class FiltersValidationTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task Empty_Returns400(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = DataSetQueryCriteriaFilters.Create(comparator, [])
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError($"criteria.filters.{comparator.ToLowerFirst()}");
        }

        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task InvalidMix_Returns400(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] invalidFilters =
            [
                "",
                " ",
                "  ",
                new string('a', 11),
                new string('a', 12),
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = DataSetQueryCriteriaFilters.Create(comparator, invalidFilters)
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(5, validationProblem.Errors.Count);

            var basePath = $"criteria.filters.{comparator.ToLowerFirst()}";

            validationProblem.AssertHasNotEmptyError($"{basePath}[0]");
            validationProblem.AssertHasNotEmptyError($"{basePath}[1]");
            validationProblem.AssertHasNotEmptyError($"{basePath}[2]");
            validationProblem.AssertHasMaximumLengthError($"{basePath}[3]", maxLength: 10);
            validationProblem.AssertHasMaximumLengthError($"{basePath}[4]", maxLength: 10);
        }

        [Fact]
        public async Task AllComparatorsInvalid_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] invalidFilters =
            [
                new string('a', 11),
                ""
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = new string('a', 11),
                            NotEq = new string('a', 12),
                            In = [],
                            NotIn = invalidFilters
                        }
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(5, validationProblem.Errors.Count);

            validationProblem.AssertHasMaximumLengthError("criteria.filters.eq", maxLength: 10);
            validationProblem.AssertHasMaximumLengthError("criteria.filters.notEq", maxLength: 10);
            validationProblem.AssertHasNotEmptyError("criteria.filters.in");
            validationProblem.AssertHasMaximumLengthError("criteria.filters.notIn[0]", maxLength: 10);
            validationProblem.AssertHasNotEmptyError("criteria.filters.notIn[1]");
        }

        [Fact]
        public async Task NotFound_Returns200_HasWarning()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] notFoundFilters =
            [
                "invalid",
                "9999999"
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = notFoundFilters[0],
                            NotEq = notFoundFilters[1],
                            In = ["IzBzg", ..notFoundFilters],
                            NotIn = ["IzBzg", ..notFoundFilters]
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(5, viewModel.Warnings.Count);

            viewModel.AssertHasFiltersNotFoundWarning("criteria.filters.eq", [notFoundFilters[0]]);
            viewModel.AssertHasFiltersNotFoundWarning("criteria.filters.notEq", [notFoundFilters[1]]);
            viewModel.AssertHasFiltersNotFoundWarning("criteria.filters.in", notFoundFilters);
            viewModel.AssertHasFiltersNotFoundWarning("criteria.filters.notIn", notFoundFilters);
        }
    }

    public class GeographicLevelsValidationTests(TestApplicationFactory testApp)
        : DataSetsControllerPostQueryTests(testApp)
    {
        [Fact]
        public async Task Empty_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            In = [],
                            NotIn = []
                        }
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(2, validationProblem.Errors.Count);

            validationProblem.AssertHasNotEmptyError("criteria.geographicLevels.in");
            validationProblem.AssertHasNotEmptyError("criteria.geographicLevels.in");
        }

        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task InvalidMix_Returns400(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] invalidLevels =
            [
                "",
                " ",
                "LADD",
                "NATT",
                "National",
                "Local authority",
                "LocalAuthority"
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = DataSetQueryCriteriaGeographicLevels.Create(comparator, invalidLevels)
                    }
                }
            );

            var allowed = GeographicLevelUtils.OrderedCodes;

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(7, validationProblem.Errors.Count);

            var path = $"criteria.geographicLevels.{comparator.ToLowerFirst()}";

            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[0]", value: invalidLevels[0], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[1]", value: invalidLevels[1], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[2]", value: invalidLevels[2], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[3]", value: invalidLevels[3], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[4]", value: invalidLevels[4], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[5]", value: invalidLevels[5], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[6]", value: invalidLevels[6], allowed);
        }

        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task NotFound_Returns200_HasWarning(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            GeographicLevel[] notFoundGeographicLevels =
            [
                GeographicLevel.Ward,
                GeographicLevel.OpportunityArea,
                GeographicLevel.PlanningArea,
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = DataSetQueryCriteriaGeographicLevels.Create(comparator,  [
                            GeographicLevel.LocalAuthority,
                            ..notFoundGeographicLevels
                        ])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Single(viewModel.Warnings);

            viewModel.AssertHasGeographicLevelsNotFoundWarning(
                $"criteria.geographicLevels.{comparator.ToLowerFirst()}",
                notFoundGeographicLevels.Select(l => l.GetEnumValue()));
        }

        [Fact]
        public async Task AllComparatorsInvalid_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] invalidLevels =
            [
                "  ",
                "National",
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                        {
                            Eq = "NATT",
                            NotEq = "LADD",
                            In = invalidLevels,
                            NotIn = []
                        }
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            var allowed = GeographicLevelUtils.OrderedCodes;

            Assert.Equal(5, validationProblem.Errors.Count);

            validationProblem.AssertHasAllowedValueError(
                expectedPath: "criteria.geographicLevels.eq",
                value: "NATT",
                allowed: allowed
            );
            validationProblem.AssertHasAllowedValueError(
                expectedPath: "criteria.geographicLevels.notEq",
                value: "LADD",
                allowed: allowed
            );
            validationProblem.AssertHasAllowedValueError(
                expectedPath: "criteria.geographicLevels.in[0]",
                value: invalidLevels[0],
                allowed: allowed
            );
            validationProblem.AssertHasAllowedValueError(
                expectedPath: "criteria.geographicLevels.in[1]",
                value: invalidLevels[1],
                allowed: allowed
            );
            validationProblem.AssertHasNotEmptyError("criteria.geographicLevels.notIn");
        }
    }

    public class LocationsValidationTests(TestApplicationFactory testApp)
        : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task Empty_Returns400(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = DataSetQueryCriteriaLocations.Create(comparator, [])
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError($"criteria.locations.{comparator.ToLowerFirst()}");
        }

        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task InvalidMix_Returns400(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = DataSetQueryCriteriaLocations.Create(comparator, [
                            new DataSetQueryLocationCode { Level = "LADD", Code = "12345" },
                            new DataSetQueryLocationCode { Level = "NATT", Code = "12345" },
                            new DataSetQueryLocationId { Level = "NAT", Id = "" },
                            new DataSetQueryLocationId { Level = "NAT", Id = " " },
                            new DataSetQueryLocationCode { Level = "REG", Code = "" },
                            new DataSetQueryLocationLocalAuthorityCode { Code = "" },
                            new DataSetQueryLocationId { Level = "NAT", Id = new string('a', 11) },
                            new DataSetQueryLocationLocalAuthorityCode { Code = new string('a', 26) },
                            new DataSetQueryLocationLocalAuthorityOldCode { OldCode = new string('a', 11) },
                            new DataSetQueryLocationProviderUkprn { Ukprn = new string('a', 9) },
                            new DataSetQueryLocationSchoolUrn { Urn = new string('a', 7) },
                            new DataSetQueryLocationSchoolLaEstab { LaEstab = new string('a', 8) }
                        ])
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(12, validationProblem.Errors.Count);

            var path = $"criteria.locations.{comparator.ToLowerFirst()}";

            validationProblem.AssertHasAllowedValueError(
                expectedPath: $"{path}[0].level",
                value: "LADD",
                allowed: GeographicLevelUtils.OrderedCodes);
            validationProblem.AssertHasAllowedValueError(
                expectedPath: $"{path}[1].level",
                value: "NATT",
                allowed: GeographicLevelUtils.OrderedCodes);

            validationProblem.AssertHasNotEmptyError($"{path}[2].id");
            validationProblem.AssertHasNotEmptyError($"{path}[3].id");
            validationProblem.AssertHasNotEmptyError($"{path}[4].code");
            validationProblem.AssertHasNotEmptyError($"{path}[5].code");

            validationProblem.AssertHasMaximumLengthError($"{path}[6].id", maxLength: 10);
            validationProblem.AssertHasMaximumLengthError($"{path}[7].code", maxLength: 25);
            validationProblem.AssertHasMaximumLengthError($"{path}[8].oldCode", maxLength: 10);
            validationProblem.AssertHasMaximumLengthError($"{path}[9].ukprn", maxLength: 8);
            validationProblem.AssertHasMaximumLengthError($"{path}[10].urn", maxLength: 6);
            validationProblem.AssertHasMaximumLengthError($"{path}[11].laEstab", maxLength: 7);
        }

        [Fact]
        public async Task AllComparatorsInvalid_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            DataSetQueryLocation[] invalidLocations =
            [
                new DataSetQueryLocationId { Level = "", Id = "" },
                new DataSetQueryLocationId { Level = "NAT", Id = " " },
                new DataSetQueryLocationId { Level = "NAT", Id = new string('a', 11) },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            Eq = new DataSetQueryLocationCode { Level = "LADD", Code = "12345" },
                            NotEq = new DataSetQueryLocationId { Level = "NAT", Id = " " },
                            In = invalidLocations,
                            NotIn = []
                        }
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(8, validationProblem.Errors.Count);

            validationProblem.AssertHasAllowedValueError(
                expectedPath: "criteria.locations.eq.level",
                value: "LADD",
                allowed: GeographicLevelUtils.OrderedCodes
            );
            validationProblem.AssertHasNotEmptyError(expectedPath: "criteria.locations.notEq.id");
            validationProblem.AssertHasNotEmptyError(expectedPath: "criteria.locations.in[0].id");
            validationProblem.AssertHasNotEmptyError(expectedPath: "criteria.locations.in[0].level");
            validationProblem.AssertHasNotEmptyError(expectedPath: "criteria.locations.in[1].id");
            validationProblem.AssertHasMaximumLengthError(expectedPath: "criteria.locations.in[2].id", maxLength: 10);
            validationProblem.AssertHasNotEmptyError("criteria.locations.notIn");
        }

        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task NotFound_Returns200_HasWarning(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            DataSetQueryLocation[] notFoundLocations =
            [
                new DataSetQueryLocationId { Level = "NAT", Id = "11111111" },
                new DataSetQueryLocationCode { Level = "NAT", Code = "11111111" },
                new DataSetQueryLocationId { Level = "REG", Id = "22222222" },
                new DataSetQueryLocationLocalAuthorityCode { Code = "4444444" },
                new DataSetQueryLocationLocalAuthorityOldCode { OldCode= "333" },
                new DataSetQueryLocationProviderUkprn { Ukprn = "88888888" },
                new DataSetQueryLocationSchoolUrn { Urn = "666666" },
                new DataSetQueryLocationSchoolLaEstab { LaEstab = "7777777" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = DataSetQueryCriteriaLocations.Create(comparator,
                        [
                            new DataSetQueryLocationLocalAuthorityCode { Code = "E08000016" },
                            ..notFoundLocations
                        ])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Single(viewModel.Warnings);

            viewModel.AssertHasLocationsNotFoundWarning(
                $"criteria.locations.{comparator.ToLowerFirst()}",
                notFoundLocations);
        }
    }

    public class TimePeriodsValidationTests(TestApplicationFactory testApp)
        : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task Empty_Returns400(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = DataSetQueryCriteriaTimePeriods.Create(comparator, [])
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError($"criteria.timePeriods.{comparator.ToLowerFirst()}");
        }

        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task InvalidMix_Returns400(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            DataSetQueryTimePeriod[] invalidTimePeriods =
            [
                new DataSetQueryTimePeriod { Code = "", Period = "" },
                new DataSetQueryTimePeriod { Code = "AY", Period = "2020/2019" },
                new DataSetQueryTimePeriod { Code = "AY", Period = "2020/2022" },
                new DataSetQueryTimePeriod { Code = "INVALID", Period = "2020" },
                new DataSetQueryTimePeriod { Code = "CY", Period = "2020/2021" },
                new DataSetQueryTimePeriod { Code = "CYQ2", Period = "2020/2021" },
                new DataSetQueryTimePeriod { Code = "RY", Period = "2020/2021" },
                new DataSetQueryTimePeriod { Code = "W10", Period = "2020/2021" },
                new DataSetQueryTimePeriod { Code = "M5", Period = "2020/2021" }
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = DataSetQueryCriteriaTimePeriods.Create(comparator, invalidTimePeriods)
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(9, validationProblem.Errors.Count);

            var path = $"criteria.timePeriods.{comparator.ToLowerFirst()}";

            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[0].code", code: "");

            validationProblem.AssertHasTimePeriodYearRangeError(expectedPath: $"{path}[1].period", period: "2020/2019");
            validationProblem.AssertHasTimePeriodYearRangeError(expectedPath: $"{path}[2].period", period: "2020/2022");

            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[3].code", code: "INVALID");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[4].code", code: "CY");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[5].code", code: "CYQ2");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[6].code", code: "RY");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[7].code", code: "W10");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[8].code", code: "M5");
        }

        [Fact]
        public async Task AllComparatorsInvalid_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            DataSetQueryTimePeriod[] invalidTimePeriods =
            [
                new DataSetQueryTimePeriod { Code = "INVALID", Period = "2020" },
                new DataSetQueryTimePeriod { Code = "CY", Period = "" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020/2019" },
                            NotEq = new DataSetQueryTimePeriod { Code = "W10", Period = "2020/2021" },
                            In = invalidTimePeriods,
                            NotIn = [],
                            Gt = new DataSetQueryTimePeriod { Code = "CY", Period = "2020/2021" },
                            Gte = new DataSetQueryTimePeriod { Code = "AY", Period = "2020/" },
                            Lt = new DataSetQueryTimePeriod { Code = "", Period = "2020" },
                            Lte = new DataSetQueryTimePeriod { Code = "", Period = "2020/2021" }
                        }
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(9, validationProblem.Errors.Count);

            const string basePath = "criteria.timePeriods";

            validationProblem.AssertHasTimePeriodYearRangeError($"{basePath}.eq.period", period: "2020/2019");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{basePath}.notEq.code", code: "W10");
            validationProblem.AssertHasTimePeriodAllowedCodeError(
                expectedPath: $"{basePath}.in[0].code",
                code: "INVALID"
            );
            validationProblem.AssertHasTimePeriodInvalidYearError(
                expectedPath: $"{basePath}.in[1].period",
                period: ""
            );
            validationProblem.AssertHasNotEmptyError(expectedPath: $"{basePath}.notIn");

            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{basePath}.gt.code", code: "CY");
            validationProblem.AssertHasTimePeriodYearRangeError(expectedPath: $"{basePath}.gte.period", period: "2020/");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{basePath}.lt.code", code: "");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{basePath}.lte.code", code: "");
        }

        [Theory]
        [InlineData("In")]
        [InlineData("NotIn")]
        public async Task NotFound_Returns200_HasWarning(string comparator)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            DataSetQueryTimePeriod[] notFoundTimePeriods =
            [
                new DataSetQueryTimePeriod { Code = "CY", Period = "2021" },
                new DataSetQueryTimePeriod { Code = "CY", Period = "2022" },
                new DataSetQueryTimePeriod { Code = "CY", Period = "2030" },
                new DataSetQueryTimePeriod { Code = "AY", Period = "2023/2024" },
                new DataSetQueryTimePeriod { Code = "AY", Period = "2018/2019" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = DataSetQueryCriteriaTimePeriods.Create(comparator, [
                            new DataSetQueryTimePeriod
                            {
                                Code = "AY",
                                Period = "2020/2021"
                            },
                            ..notFoundTimePeriods
                        ])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Single(viewModel.Warnings);

            viewModel.AssertHasTimePeriodsNotFoundWarning(
                $"criteria.timePeriods.{comparator.ToLowerFirst()}",
                notFoundTimePeriods);
        }
    }

    public class SortsValidationTests(TestApplicationFactory testApp)
        : DataSetsControllerPostQueryTests(testApp)
    {
        [Fact]
        public async Task Empty_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Sorts = []
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError("sorts");
        }

        [Fact]
        public async Task InvalidMix_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Sorts =
                    [
                        new DataSetQuerySort { Field = "", Direction = "asc" },
                        new DataSetQuerySort { Field = "test", Direction = "" },
                        new DataSetQuerySort { Field = "test", Direction = "invalid" },
                        new DataSetQuerySort { Field = "test", Direction = "asc" },
                        new DataSetQuerySort { Field = "test", Direction = "desc" },
                        new DataSetQuerySort { Field = $"{new string('a', 41)}", Direction = "Asc" },
                        new DataSetQuerySort { Field = $"{new string('b', 41)}", Direction = "Desc" },
                    ]
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(8, validationProblem.Errors.Count);

            validationProblem.AssertHasNotEmptyError(expectedPath: "sorts[0].field");
            validationProblem.AssertHasAllowedValueError(expectedPath: "sorts[0].direction", value: "asc");

            validationProblem.AssertHasAllowedValueError(expectedPath: "sorts[1].direction", value: "");
            validationProblem.AssertHasAllowedValueError(expectedPath: "sorts[2].direction", value: "invalid");
            validationProblem.AssertHasAllowedValueError(expectedPath: "sorts[3].direction", value: "asc");
            validationProblem.AssertHasAllowedValueError(expectedPath: "sorts[4].direction", value: "desc");

            validationProblem.AssertHasMaximumLengthError(
                expectedPath: "sorts[5].field",
                maxLength: 40
            );
            validationProblem.AssertHasMaximumLengthError(
                expectedPath: "sorts[6].field",
                maxLength: 40
            );
        }

        [Fact]
        public async Task FieldsNotFound_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            DataSetQuerySort[] notFoundSorts =
            [
                new DataSetQuerySort { Field = "invalid1", Direction = "Asc" },
                new DataSetQuerySort { Field = "invalid2", Direction = "Desc" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Sorts =
                    [
                        new() { Field = "timePeriod", Direction = "Asc" },
                        ..notFoundSorts,
                    ]
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasSortFieldsNotFoundError("sorts", notFoundSorts);
        }
    }

    public class PaginationTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task PageTooSmall_Returns400(int page)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Page = page
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasGreaterThanOrEqualError("page", comparisonValue: 1);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(10001)]
        public async Task PageSizeOutOfBounds_Returns400(int pageSize)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    PageSize = pageSize
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasInclusiveBetweenError("pageSize", from: 1, to: 10000);
        }

        [Theory]
        [InlineData(1, 50, 5, 50)]
        [InlineData(2, 50, 5, 50)]
        [InlineData(3, 50, 5, 50)]
        [InlineData(1, 150, 2, 150)]
        [InlineData(2, 150, 2, 66)]
        [InlineData(1, 216, 1, 216)]
        public async Task MultiplePages_Returns200_PaginatedCorrectly(
            int page,
            int pageSize,
            int totalPages,
            int pageResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Page = page,
                    PageSize = pageSize
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(pageResults, viewModel.Results.Count);
            Assert.Equal(page, viewModel.Paging.Page);
            Assert.Equal(pageSize, viewModel.Paging.PageSize);
            Assert.Equal(totalPages, viewModel.Paging.TotalPages);
            Assert.Equal(216, viewModel.Paging.TotalResults);
        }
    }

    public class FiltersQueryTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData("Eq", 54)]
        [InlineData("NotEq", 162)]
        [InlineData("In", 54)]
        [InlineData("NotIn", 162)]
        public async Task SingleOption_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Year 4
            const string filterOptionId = "IzBzg";

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = DataSetQueryCriteriaFilters.Create(comparator, [filterOptionId])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (comparator)
            {
                case "Eq":
                case "In":
                    Assert.Single(meta.Filters["ncyear"]);
                    Assert.Contains(filterOptionId, meta.Filters["ncyear"]);
                    break;
                case "NotEq":
                case "NotIn":
                    Assert.Equal(3, meta.Filters["ncyear"].Count);
                    Assert.DoesNotContain(filterOptionId, meta.Filters["ncyear"]);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 108)]
        [InlineData("NotIn", 108)]
        public async Task MultipleOptionsInSameFilter_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Year 4 and 8
            string[] filterOptionIds = ["IzBzg", "7zXob"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = DataSetQueryCriteriaFilters.Create(comparator, filterOptionIds)
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(2, meta.Indicators.Count);
            Assert.Contains("enrolments", meta.Indicators);
            Assert.Contains("sess_authorised", meta.Indicators);

            switch (comparator)
            {
                case "In":
                    Assert.Equal(2, meta.Filters["ncyear"].Count);
                    Assert.Contains(filterOptionIds[0], meta.Filters["ncyear"]);
                    Assert.Contains(filterOptionIds[1], meta.Filters["ncyear"]);
                    break;
                case "NotIn":
                    Assert.Equal(2, meta.Filters["ncyear"].Count);
                    Assert.DoesNotContain(filterOptionIds[0], meta.Filters["ncyear"]);
                    Assert.DoesNotContain(filterOptionIds[1], meta.Filters["ncyear"]);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 150)]
        [InlineData("NotIn", 66)]
        public async Task MultipleOptionsInDifferentFilters_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Total and secondary school type
            // Secondary free school and secondary sponsor led academy types
            string[] filterOptionIds = ["0kT5D", "6jrfe", "9U4vZ", "O7CLF"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = DataSetQueryCriteriaFilters.Create(comparator, filterOptionIds)
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (comparator)
            {
                case "In":
                    Assert.Equal(2, meta.Filters["school_type"].Count);
                    Assert.Contains(filterOptionIds[0], meta.Filters["school_type"]);
                    Assert.Contains(filterOptionIds[1], meta.Filters["school_type"]);

                    Assert.Equal(2, meta.Filters["academy_type"].Count);
                    Assert.Contains(filterOptionIds[2], meta.Filters["academy_type"]);
                    Assert.Contains(filterOptionIds[3], meta.Filters["academy_type"]);
                    break;
                case "NotIn":
                    Assert.Single(meta.Filters["school_type"]);
                    Assert.DoesNotContain(filterOptionIds[0], meta.Filters["school_type"]);
                    Assert.DoesNotContain(filterOptionIds[1], meta.Filters["school_type"]);

                    Assert.Single(meta.Filters["academy_type"]);
                    Assert.DoesNotContain(filterOptionIds[2], meta.Filters["academy_type"]);
                    Assert.DoesNotContain(filterOptionIds[3], meta.Filters["academy_type"]);
                    break;
            }
        }
    }

    public class GeographicLevelsQueryTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData("Eq", 132)]
        [InlineData("NotEq", 84)]
        [InlineData("In", 132)]
        [InlineData("NotIn", 84)]
        public async Task SingleOption_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            const GeographicLevel geographicLevel = GeographicLevel.LocalAuthority;

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = DataSetQueryCriteriaGeographicLevels.Create(comparator, [geographicLevel])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (comparator)
            {
                case "Eq":
                case "In":
                    Assert.Single(meta.GeographicLevels);
                    Assert.Contains(geographicLevel, meta.GeographicLevels);
                    break;
                case "NotEq":
                case "NotIn":
                    Assert.Equal(3, meta.GeographicLevels.Count);
                    Assert.DoesNotContain(geographicLevel, meta.GeographicLevels);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 180)]
        [InlineData("NotIn", 36)]
        public async Task MultipleOptions_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            GeographicLevel[] geographicLevels = [GeographicLevel.Region, GeographicLevel.LocalAuthority];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = DataSetQueryCriteriaGeographicLevels.Create(comparator, geographicLevels)
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (comparator)
            {
                case "In":
                    Assert.Equal(2, meta.GeographicLevels.Count);
                    Assert.Contains(geographicLevels[0], meta.GeographicLevels);
                    Assert.Contains(geographicLevels[1], meta.GeographicLevels);
                    break;
                case "NotIn":
                    Assert.Equal(2, meta.GeographicLevels.Count);
                    Assert.DoesNotContain(geographicLevels[0], meta.GeographicLevels);
                    Assert.DoesNotContain(geographicLevels[1], meta.GeographicLevels);
                    break;
            }
        }
    }

    public class LocationsQueryTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData("Eq", 36)]
        [InlineData("NotEq", 180)]
        [InlineData("In", 36)]
        [InlineData("NotIn", 180)]
        public async Task SingleOption_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Sheffield
            var location = new DataSetQueryLocationLocalAuthorityCode { Code = "E08000019" };
            const string locationId = "7zXob";

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = DataSetQueryCriteriaLocations.Create(comparator, [location])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (comparator)
            {
                case "Eq":
                case "In":
                    Assert.Single(meta.Locations["LA"]);
                    Assert.Contains(locationId, meta.Locations["LA"]);
                    break;
                case "NotEq":
                case "NotIn":
                    Assert.Equal(3, meta.Locations["LA"].Count);
                    Assert.DoesNotContain(locationId, meta.Locations["LA"]);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 72)]
        [InlineData("NotIn", 144)]
        public async Task MultipleOptionsInSameLevel_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Sheffield and Barnsley
            DataSetQueryLocation[] locations =
            [
                new DataSetQueryLocationLocalAuthorityCode { Code = "E08000019" },
                new DataSetQueryLocationId { Level = "LA", Id = "O7CLF" }
            ];
            string[] locationIds = ["7zXob", "O7CLF"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = DataSetQueryCriteriaLocations.Create(comparator, locations)
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (comparator)
            {
                case "In":
                    Assert.Equal(2, meta.Locations["LA"].Count);
                    Assert.Contains(locationIds[0], meta.Locations["LA"]);
                    Assert.Contains(locationIds[1], meta.Locations["LA"]);
                    break;
                case "NotIn":
                    Assert.Equal(2, meta.Locations["LA"].Count);
                    Assert.DoesNotContain(locationIds[0], meta.Locations["LA"]);
                    Assert.DoesNotContain(locationIds[1], meta.Locations["LA"]);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 84)]
        [InlineData("NotIn", 132)]
        public async Task MultipleOptionsInDifferentLevels_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Sheffield and Barnsley
            // THe Kingston Academy and King Athelstan Primary School
            DataSetQueryLocation[] locations =
            [
                new DataSetQueryLocationLocalAuthorityCode { Code = "E08000019" },
                new DataSetQueryLocationId { Level = "LA", Id = "O7CLF" },
                new DataSetQueryLocationSchoolLaEstab { LaEstab = "3144001" },
                new DataSetQueryLocationSchoolUrn { Urn = "102579" }
            ];

            string[] locationIds = ["7zXob", "O7CLF", "0kT5D", "arLPb"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = DataSetQueryCriteriaLocations.Create(comparator, locations)
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (comparator)
            {
                case "In":
                    Assert.Equal(3, meta.Locations["LA"].Count);
                    Assert.Contains(locationIds[0], meta.Locations["LA"]);
                    Assert.Contains(locationIds[1], meta.Locations["LA"]);

                    Assert.Equal(6, meta.Locations["SCH"].Count);
                    Assert.Contains(locationIds[2], meta.Locations["SCH"]);
                    Assert.Contains(locationIds[3], meta.Locations["SCH"]);
                    break;
                case "NotIn":
                    Assert.Equal(2, meta.Locations["LA"].Count);
                    Assert.DoesNotContain(locationIds[0], meta.Locations["LA"]);
                    Assert.DoesNotContain(locationIds[1], meta.Locations["LA"]);

                    Assert.Equal(2, meta.Locations["SCH"].Count);
                    Assert.DoesNotContain(locationIds[2], meta.Locations["SCH"]);
                    Assert.DoesNotContain(locationIds[3], meta.Locations["SCH"]);
                    break;
            }
        }
    }

    public class TimePeriodsQueryTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [InlineData("Eq", 72)]
        [InlineData("NotEq", 144)]
        [InlineData("In", 72)]
        [InlineData("NotIn", 144)]
        [InlineData("Gt", 72)]
        [InlineData("Gte", 144)]
        [InlineData("Lt", 72)]
        [InlineData("Lte", 144)]
        public async Task SingleOption_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var queryTimePeriod = new DataSetQueryTimePeriod { Code = "AY", Period = "2021/2022" };

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = DataSetQueryCriteriaTimePeriods.Create(comparator, [queryTimePeriod])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            var timePeriod = new TimePeriodViewModel
            {
                Code = TimeIdentifier.AcademicYear,
                Period = "2021/2022"
            };

            switch (comparator)
            {
                case "Eq":
                case "In":
                    Assert.Single(meta.TimePeriods);
                    Assert.Contains(timePeriod, meta.TimePeriods);
                    break;
                case "NotEq":
                case "NotIn":
                    Assert.Equal(2, meta.TimePeriods.Count);
                    Assert.DoesNotContain(timePeriod, meta.TimePeriods);
                    break;
                case "Lt":
                case "Gt":
                    Assert.Single(meta.TimePeriods);
                    Assert.DoesNotContain(timePeriod, meta.TimePeriods);
                    break;
                case "Lte":
                case "Gte":
                    Assert.Equal(2, meta.TimePeriods.Count);
                    Assert.Contains(timePeriod, meta.TimePeriods);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 144)]
        [InlineData("NotIn", 72)]
        public async Task MultipleOptions_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            DataSetQueryTimePeriod[] queryTimePeriods =
            [
                new DataSetQueryTimePeriod { Code = "AY", Period = "2021" },
                new DataSetQueryTimePeriod { Code = "AY", Period = "2022/2023" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = DataSetQueryCriteriaTimePeriods.Create(comparator, queryTimePeriods)
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            TimePeriodViewModel[] timePeriods =
            [
                new TimePeriodViewModel { Code = TimeIdentifier.AcademicYear, Period = "2021/2022" },
                new TimePeriodViewModel { Code = TimeIdentifier.AcademicYear, Period = "2022/2023" }
            ];

            switch (comparator)
            {
                case "In":
                    Assert.Equal(2, meta.TimePeriods.Count);
                    Assert.Contains(timePeriods[0], meta.TimePeriods);
                    Assert.Contains(timePeriods[1], meta.TimePeriods);
                    break;
                case "NotIn":
                    Assert.Single(meta.TimePeriods);
                    Assert.DoesNotContain(timePeriods[0], meta.TimePeriods);
                    Assert.DoesNotContain(timePeriods[1], meta.TimePeriods);
                    break;
            }
        }
    }

    public class FacetsOnlyResultsTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Fact]
        public async Task NoResults_Returns200_HasWarning()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            Eq = new DataSetQueryLocationId { Level = "LA", Id = "9U4vZ" }
                        },
                        GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                        {
                            Eq = "NAT"
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1000, viewModel.Paging.PageSize);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(0, viewModel.Paging.TotalResults);

            var warning = Assert.Single(viewModel.Warnings);

            Assert.Equal(ValidationMessages.QueryNoResults.Code, warning.Code);
            Assert.Equal(ValidationMessages.QueryNoResults.Message, warning.Message);
        }

        [Fact]
        public async Task DebugEnabled_Returns200_HasWarning()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"],
                    Debug = true
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            var warning = Assert.Single(viewModel.Warnings);

            Assert.Equal(ValidationMessages.DebugEnabled.Code, warning.Code);
            Assert.Equal(ValidationMessages.DebugEnabled.Message, warning.Message);
        }

        [Fact]
        public async Task SingleIndicator_Returns200_CorrectViewModel()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["sess_authorised"]
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(216, viewModel.Paging.TotalResults);

            Assert.Empty(viewModel.Warnings);

            Assert.Equal(216, viewModel.Results.Count);

            var result = viewModel.Results[0];

            Assert.Equal(2, result.Filters.Count);
            Assert.Equal("pTSoj", result.Filters["ncyear"]);
            Assert.Equal("0kT5D", result.Filters["school_type"]);

            Assert.Equal(GeographicLevel.LocalAuthority, result.GeographicLevel);

            Assert.Equal(3, result.Locations.Count);
            Assert.Equal("dP0Zw", result.Locations["LA"]);
            Assert.Equal("pTSoj", result.Locations["NAT"]);
            Assert.Equal("it6Xr", result.Locations["REG"]);

            Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriod.Code);
            Assert.Equal("2022/2023", result.TimePeriod.Period);

            Assert.Single(result.Values);
            Assert.Equal("4064499", result.Values["sess_authorised"]);
        }

        [Fact]
        public async Task AllIndicators_Returns200_ResultValuesInAllowedRanges()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators =
                    [
                        "enrolments",
                        "sess_authorised",
                        "sess_possible",
                        "sess_unauthorised",
                        "sess_unauthorised_percent",
                    ]
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(216, viewModel.Results.Count);

            var values = viewModel.Results
                .SelectMany(result => result.Values)
                .GroupBy(kv => kv.Key, kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.ToList());

            var enrolments = values["enrolments"].Select(int.Parse).ToList();

            Assert.Equal(216, enrolments.Count);
            Assert.Equal(999598, enrolments.Max());
            Assert.Equal(1072, enrolments.Min());

            var sessAuthorised = values["sess_authorised"].Select(int.Parse).ToList();

            Assert.Equal(216, sessAuthorised.Count);
            Assert.Equal(4967515, sessAuthorised.Max());
            Assert.Equal(22441, sessAuthorised.Min());

            var sessPossible = values["sess_possible"].Select(int.Parse).ToList();

            Assert.Equal(216, sessPossible.Count);
            Assert.Equal(9934276, sessPossible.Max());
            Assert.Equal(18306, sessPossible.Min());

            var sessUnauthorised = values["sess_unauthorised"].Select(int.Parse).ToList();

            Assert.Equal(216, sessUnauthorised.Count);
            Assert.Equal(494993, sessUnauthorised.Max());
            Assert.Equal(2883, sessUnauthorised.Min());

            var sessUnauthorisedPercent = values["sess_unauthorised_percent"].Select(float.Parse).ToList();

            Assert.Equal(216, sessUnauthorisedPercent.Count);
            Assert.Equal(14.8837004f, sessUnauthorisedPercent.Max());
            Assert.Equal(0.241600007f, sessUnauthorisedPercent.Min());
        }

        [Fact]
        public async Task AllIndicators_Returns200_CorrectResultIds()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators =
                    [
                        "enrolments",
                        "sess_authorised",
                        "sess_possible",
                        "sess_unauthorised",
                        "sess_unauthorised_percent",
                    ]
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(216, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(3, meta.Filters.Count);

            Assert.Equal(3, meta.Filters["academy_type"].Count);

            Assert.Contains("dP0Zw", meta.Filters["academy_type"]);
            Assert.Contains("9U4vZ", meta.Filters["academy_type"]);
            Assert.Contains("O7CLF", meta.Filters["academy_type"]);

            Assert.Equal(4, meta.Filters["ncyear"].Count);
            Assert.Contains("IzBzg", meta.Filters["ncyear"]);
            Assert.Contains("it6Xr", meta.Filters["ncyear"]);
            Assert.Contains("7zXob", meta.Filters["ncyear"]);
            Assert.Contains("pTSoj", meta.Filters["ncyear"]);

            Assert.Equal(3, meta.Filters["school_type"].Count);
            Assert.Contains("LxWjE", meta.Filters["school_type"]);
            Assert.Contains("6jrfe", meta.Filters["school_type"]);
            Assert.Contains("0kT5D", meta.Filters["school_type"]);

            Assert.Equal(4, meta.Locations.Count);

            Assert.Single(meta.Locations["NAT"]);
            Assert.Contains("pTSoj", meta.Locations["NAT"]);

            Assert.Equal(2, meta.Locations["REG"].Count);
            Assert.Contains("it6Xr", meta.Locations["REG"]);
            Assert.Contains("IzBzg", meta.Locations["REG"]);

            Assert.Equal(4, meta.Locations["LA"].Count);
            Assert.Contains("9U4vZ", meta.Locations["LA"]);
            Assert.Contains("O7CLF", meta.Locations["LA"]);
            Assert.Contains("dP0Zw", meta.Locations["LA"]);
            Assert.Contains("7zXob", meta.Locations["LA"]);

            Assert.Equal(8, meta.Locations["SCH"].Count);
            Assert.Contains("qFjG7", meta.Locations["SCH"]);
            Assert.Contains("0kT5D", meta.Locations["SCH"]);
            Assert.Contains("arLPb", meta.Locations["SCH"]);
            Assert.Contains("6jrfe", meta.Locations["SCH"]);
            Assert.Contains("HTzLj", meta.Locations["SCH"]);
            Assert.Contains("LxWjE", meta.Locations["SCH"]);
            Assert.Contains("CpId1", meta.Locations["SCH"]);
            Assert.Contains("YPHKM", meta.Locations["SCH"]);

            Assert.Equal(4, meta.GeographicLevels.Count);
            Assert.Contains(GeographicLevel.Country, meta.GeographicLevels);
            Assert.Contains(GeographicLevel.Region, meta.GeographicLevels);
            Assert.Contains(GeographicLevel.LocalAuthority, meta.GeographicLevels);
            Assert.Contains(GeographicLevel.School, meta.GeographicLevels);

            Assert.Equal(3, meta.TimePeriods.Count);
            Assert.Contains(
                new TimePeriodViewModel { Code = TimeIdentifier.AcademicYear, Period = "2020/2021" },
                meta.TimePeriods
            );
            Assert.Contains(
                new TimePeriodViewModel { Code = TimeIdentifier.AcademicYear, Period = "2021/2022" },
                meta.TimePeriods
            );
            Assert.Contains(
                new TimePeriodViewModel { Code = TimeIdentifier.AcademicYear, Period = "2022/2023" },
                meta.TimePeriods
            );

            Assert.Equal(5, meta.Indicators.Count);
            Assert.Contains("enrolments", meta.Indicators);
            Assert.Contains("sess_authorised", meta.Indicators);
            Assert.Contains("sess_possible", meta.Indicators);
            Assert.Contains("sess_unauthorised", meta.Indicators);
            Assert.Contains("sess_unauthorised_percent", meta.Indicators);
        }

        [Fact]
        public async Task AllIndicators_Returns200_CorrectDebuggedResultLabels()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators =
                    [
                        "enrolments",
                        "sess_authorised",
                        "sess_possible",
                        "sess_unauthorised",
                        "sess_unauthorised_percent",
                    ],
                    Debug = true
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(216, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(3, meta.Filters.Count);

            Assert.Equal(3, meta.Filters["academy_type"].Count);

            Assert.Contains("dP0Zw :: Primary sponsor led academy", meta.Filters["academy_type"]);
            Assert.Contains("9U4vZ :: Secondary free school", meta.Filters["academy_type"]);
            Assert.Contains("O7CLF :: Secondary sponsor led academy", meta.Filters["academy_type"]);

            Assert.Equal(4, meta.Filters["ncyear"].Count);
            Assert.Contains("IzBzg :: Year 4", meta.Filters["ncyear"]);
            Assert.Contains("it6Xr :: Year 6", meta.Filters["ncyear"]);
            Assert.Contains("7zXob :: Year 8", meta.Filters["ncyear"]);
            Assert.Contains("pTSoj :: Year 10", meta.Filters["ncyear"]);

            Assert.Equal(3, meta.Filters["school_type"].Count);
            Assert.Contains("LxWjE :: State-funded primary", meta.Filters["school_type"]);
            Assert.Contains("6jrfe :: State-funded secondary", meta.Filters["school_type"]);
            Assert.Contains("0kT5D :: Total", meta.Filters["school_type"]);

            Assert.Equal(4, meta.Locations.Count);

            Assert.Single(meta.Locations["NAT"]);
            Assert.Contains("pTSoj :: England (code = E92000001)", meta.Locations["NAT"]);

            Assert.Equal(2, meta.Locations["REG"].Count);
            Assert.Contains("it6Xr :: Outer London (code = E13000002)", meta.Locations["REG"]);
            Assert.Contains("IzBzg :: Yorkshire and The Humber (code = E12000003)", meta.Locations["REG"]);

            Assert.Equal(4, meta.Locations["LA"].Count);
            Assert.Contains("9U4vZ :: Barnet (code = E09000003, oldCode = 302)", meta.Locations["LA"]);
            Assert.Contains("O7CLF :: Barnsley (code = E08000016, oldCode = 370)", meta.Locations["LA"]);
            Assert.Contains(
                "dP0Zw :: Kingston upon Thames / Richmond upon Thames (code = E09000021 / E09000027, oldCode = 314)",
                meta.Locations["LA"]
            );
            Assert.Contains("7zXob :: Sheffield (code = E08000019, oldCode = 373)", meta.Locations["LA"]);

            Assert.Equal(8, meta.Locations["SCH"].Count);
            Assert.Contains("qFjG7 :: Colindale Primary School (urn = 101269, laEstab = 3022014)", meta.Locations["SCH"]);
            Assert.Contains("0kT5D :: Greenhill Primary School (urn = 145374, laEstab = 3732341)", meta.Locations["SCH"]);
            Assert.Contains(
                "arLPb :: Hoyland Springwood Primary School (urn = 141973, laEstab = 3702039)",
                meta.Locations["SCH"]
            );
            Assert.Contains(
                "6jrfe :: King Athelstan Primary School (urn = 102579, laEstab = 3142032)",
                meta.Locations["SCH"]
            );
            Assert.Contains("HTzLj :: Newfield Secondary School (urn = 140821, laEstab = 3734008)", meta.Locations["SCH"]);
            Assert.Contains("LxWjE :: Penistone Grammar School (urn = 106653, laEstab = 3704027)", meta.Locations["SCH"]);
            Assert.Contains("CpId1 :: The Kingston Academy (urn = 141862, laEstab = 3144001)", meta.Locations["SCH"]);
            Assert.Contains("YPHKM :: Wren Academy Finchley (urn = 135507, laEstab = 3026906)", meta.Locations["SCH"]);

            Assert.Equal(4, meta.GeographicLevels.Count);
            Assert.Contains(GeographicLevel.Country, meta.GeographicLevels);
            Assert.Contains(GeographicLevel.Region, meta.GeographicLevels);
            Assert.Contains(GeographicLevel.LocalAuthority, meta.GeographicLevels);
            Assert.Contains(GeographicLevel.School, meta.GeographicLevels);

            Assert.Equal(3, meta.TimePeriods.Count);
            Assert.Contains(
                new TimePeriodViewModel { Code = TimeIdentifier.AcademicYear, Period = "2020/2021" },
                meta.TimePeriods
            );
            Assert.Contains(
                new TimePeriodViewModel { Code = TimeIdentifier.AcademicYear, Period = "2021/2022" },
                meta.TimePeriods
            );
            Assert.Contains(
                new TimePeriodViewModel { Code = TimeIdentifier.AcademicYear, Period = "2022/2023" },
                meta.TimePeriods
            );

            Assert.Equal(5, meta.Indicators.Count);
            Assert.Contains("enrolments", meta.Indicators);
            Assert.Contains("sess_authorised", meta.Indicators);
            Assert.Contains("sess_possible", meta.Indicators);
            Assert.Contains("sess_unauthorised", meta.Indicators);
            Assert.Contains("sess_unauthorised_percent", meta.Indicators);
        }

        [Fact]
        public async Task AllFacetsMixture_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Debug = true,
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            NotEq = "7zXob",
                            In = ["9U4vZ", "O7CLF"]
                        },
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            NotEq = "NAT"
                        },
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            Eq = new DataSetQueryLocationId { Level = "NAT", Id = "pTSoj" },
                            NotIn =
                            [
                                new DataSetQueryLocationCode { Level = "REG", Code = "E13000002" },
                                new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "370" },
                            ]
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Gt = new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" },
                            Lt = new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            var result = Assert.Single(viewModel.Results);

            Assert.Equal(3, result.Filters.Count);
            Assert.Equal("pTSoj :: Year 10", result.Filters["ncyear"]);
            Assert.Equal("6jrfe :: State-funded secondary", result.Filters["school_type"]);
            Assert.Equal("O7CLF :: Secondary sponsor led academy", result.Filters["academy_type"]);

            Assert.Equal(GeographicLevel.School, result.GeographicLevel);

            Assert.Equal(4, result.Locations.Count);
            Assert.Equal("pTSoj :: England (code = E92000001)", result.Locations["NAT"]);
            Assert.Equal("IzBzg :: Yorkshire and The Humber (code = E12000003)", result.Locations["REG"]);
            Assert.Equal("7zXob :: Sheffield (code = E08000019, oldCode = 373)", result.Locations["LA"]);
            Assert.Equal(
                "HTzLj :: Newfield Secondary School (urn = 140821, laEstab = 3734008)",
                result.Locations["SCH"]
            );

            Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriod.Code);
            Assert.Equal("2021/2022", result.TimePeriod.Period);

            Assert.Equal(2, result.Values.Count);
            Assert.Equal("752009", result.Values["enrolments"]);
            Assert.Equal("262396", result.Values["sess_authorised"]);
        }
    }

    public class AndConditionTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Fact]
        public async Task Empty_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Debug = true,
                    Criteria = new DataSetQueryCriteriaAnd
                    {
                        And = []
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError("criteria.and");
        }

        private static readonly DataSetQueryCriteriaFacets BaseFacets = new()
        {
            Filters = new DataSetQueryCriteriaFilters
            {
                NotEq = "7zXob",
                In = ["9U4vZ", "O7CLF"]
            },
            GeographicLevels = new DataSetGetQueryGeographicLevels
            {
                NotEq = "NAT"
            },
            Locations = new DataSetQueryCriteriaLocations
            {
                NotIn =
                [
                    new DataSetQueryLocationCode { Level = "REG", Code = "E13000002" },
                    new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "370" },
                ]
            },
            TimePeriods = new DataSetQueryCriteriaTimePeriods
            {
                Eq = new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
            }
        };

        public static readonly TheoryData<DataSetQueryCriteria> EquivalentCriteria = new()
        {
            new DataSetQueryCriteriaAnd
            {
                And = [BaseFacets]
            },
            new DataSetQueryCriteriaAnd
            {
                And =
                [
                    new DataSetQueryCriteriaFacets
                    {
                        Filters = BaseFacets.Filters
                    },
                    new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = BaseFacets.GeographicLevels
                    },
                    new DataSetQueryCriteriaFacets
                    {
                        Locations = BaseFacets.Locations
                    },
                    new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = BaseFacets.TimePeriods
                    },
                ]
            },
            new DataSetQueryCriteriaAnd
            {
                And =
                [
                    new DataSetQueryCriteriaAnd
                    {
                        And = [BaseFacets]
                    }
                ]
            },
            new DataSetQueryCriteriaAnd
            {
                And =
                [
                    new DataSetQueryCriteriaAnd
                    {
                        And =
                        [
                            new DataSetQueryCriteriaFacets
                            {
                                Filters = BaseFacets.Filters
                            },
                            new DataSetQueryCriteriaFacets
                            {
                                GeographicLevels = BaseFacets.GeographicLevels
                            },
                            new DataSetQueryCriteriaFacets
                            {
                                Locations = BaseFacets.Locations
                            },
                            new DataSetQueryCriteriaFacets
                            {
                                TimePeriods = BaseFacets.TimePeriods
                            },
                        ]
                    }
                ]
            },
            new DataSetQueryCriteriaAnd
            {
                And =
                [
                    new DataSetQueryCriteriaAnd
                    {
                        And =
                        [
                            new DataSetQueryCriteriaFacets
                            {
                                Filters = new DataSetQueryCriteriaFilters
                                {
                                    NotEq = BaseFacets.Filters!.NotEq,
                                },
                            },
                            new DataSetQueryCriteriaFacets
                            {
                                Filters = new DataSetQueryCriteriaFilters
                                {
                                    In = BaseFacets.Filters!.In
                                },
                                Locations = BaseFacets.Locations
                            },
                        ]
                    },
                    new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = BaseFacets.GeographicLevels,
                    },
                    new DataSetQueryCriteriaFacets
                    {
                        TimePeriods = BaseFacets.TimePeriods
                    },
                ]
            },
            new DataSetQueryCriteriaAnd
            {
                And =
                [
                    new DataSetQueryCriteriaAnd
                    {
                        And =
                        [
                            new DataSetQueryCriteriaFacets
                            {
                                Filters = new DataSetQueryCriteriaFilters
                                {
                                    NotEq = BaseFacets.Filters.NotEq,
                                },
                            },
                            new DataSetQueryCriteriaFacets
                            {
                                Locations = BaseFacets.Locations,
                                TimePeriods = BaseFacets.TimePeriods
                            },
                        ]
                    },
                    new DataSetQueryCriteriaOr
                    {
                        Or =
                        [
                            new DataSetQueryCriteriaFacets
                            {
                                Filters = new DataSetQueryCriteriaFilters
                                {
                                    Eq = "9U4vZ",
                                },
                            },
                            new DataSetQueryCriteriaFacets
                            {
                                Filters = new DataSetQueryCriteriaFilters
                                {
                                    Eq = "O7CLF",
                                },
                            },
                        ]
                    },
                    new DataSetQueryCriteriaNot
                    {
                        Not = new DataSetQueryCriteriaFacets
                        {
                            GeographicLevels = new DataSetGetQueryGeographicLevels
                            {
                                Eq = "NAT"
                            },
                        }
                    }
                ]
            }
        };

        [Theory]
        [MemberData(nameof(EquivalentCriteria))]
        public async Task EquivalentCriteria_Returns200(DataSetQueryCriteria criteria)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Debug = true,
                    Criteria = criteria
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            var result = Assert.Single(viewModel.Results);

            Assert.Equal(3, result.Filters.Count);
            Assert.Equal("pTSoj :: Year 10", result.Filters["ncyear"]);
            Assert.Equal("6jrfe :: State-funded secondary", result.Filters["school_type"]);
            Assert.Equal("O7CLF :: Secondary sponsor led academy", result.Filters["academy_type"]);

            Assert.Equal(GeographicLevel.School, result.GeographicLevel);

            Assert.Equal(4, result.Locations.Count);
            Assert.Equal("pTSoj :: England (code = E92000001)", result.Locations["NAT"]);
            Assert.Equal("IzBzg :: Yorkshire and The Humber (code = E12000003)", result.Locations["REG"]);
            Assert.Equal("7zXob :: Sheffield (code = E08000019, oldCode = 373)", result.Locations["LA"]);
            Assert.Equal(
                "HTzLj :: Newfield Secondary School (urn = 140821, laEstab = 3734008)",
                result.Locations["SCH"]
            );

            Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriod.Code);
            Assert.Equal("2021/2022", result.TimePeriod.Period);

            Assert.Equal(2, result.Values.Count);
            Assert.Equal("752009", result.Values["enrolments"]);
            Assert.Equal("262396", result.Values["sess_authorised"]);
        }
    }

    public class OrConditionTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Fact]
        public async Task Empty_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Debug = true,
                    Criteria = new DataSetQueryCriteriaOr
                    {
                        Or = []
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError("criteria.or");
        }

        private static readonly DataSetQueryCriteriaFacets BaseFacets = new()
        {
            Filters = new DataSetQueryCriteriaFilters
            {
                NotEq = "7zXob",
                In = ["9U4vZ", "O7CLF"]
            },
            GeographicLevels = new DataSetGetQueryGeographicLevels
            {
                NotEq = "NAT"
            },
            Locations = new DataSetQueryCriteriaLocations
            {
                Eq = new DataSetQueryLocationId { Level = "NAT", Id = "pTSoj" },
                NotIn =
                [
                    new DataSetQueryLocationCode { Level = "REG", Code = "E13000002" },
                    new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "370" },
                ]
            }
        };

        public static readonly TheoryData<DataSetQueryCriteria> EquivalentCriteria = new()
        {
            new DataSetQueryCriteriaOr
            {
                Or =
                [
                    BaseFacets with
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            In =
                            [
                                new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
                                new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" }
                            ],
                        }
                    },
                ],
            },
            new DataSetQueryCriteriaOr
            {
                Or =
                [
                    BaseFacets with
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
                        }
                    },
                    BaseFacets with
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" },
                        }
                    }
                ]
            },
            new DataSetQueryCriteriaOr
            {
                Or =
                [
                    BaseFacets with
                    {
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
                        }
                    },
                    new DataSetQueryCriteriaOr
                    {
                        Or =
                        [
                            BaseFacets with
                            {
                                TimePeriods = new DataSetQueryCriteriaTimePeriods
                                {
                                    Eq = new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" },
                                }
                            },
                        ]
                    }
                ]
            },
            new DataSetQueryCriteriaOr
            {
                Or =
                [
                    new DataSetQueryCriteriaOr
                    {
                        Or =
                        [
                            BaseFacets with
                            {
                                TimePeriods = new DataSetQueryCriteriaTimePeriods
                                {
                                    In =
                                    [
                                        new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
                                        new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" },
                                    ]
                                }
                            },
                        ]
                    }
                ]
            },
            new DataSetQueryCriteriaOr
            {
                Or =
                [
                    new DataSetQueryCriteriaOr
                    {
                        Or =
                        [
                            BaseFacets with
                            {
                                TimePeriods = new DataSetQueryCriteriaTimePeriods
                                {
                                    Eq = new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
                                }
                            },
                            BaseFacets with
                            {
                                TimePeriods = new DataSetQueryCriteriaTimePeriods
                                {
                                    Eq = new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" },
                                }
                            },
                        ]
                    }
                ]
            },
            new DataSetQueryCriteriaOr
            {
                Or =
                [
                    new DataSetQueryCriteriaAnd
                    {
                        And =
                        [
                            new DataSetQueryCriteriaFacets { Filters = BaseFacets.Filters },
                            new DataSetQueryCriteriaFacets { GeographicLevels = BaseFacets.GeographicLevels },
                            new DataSetQueryCriteriaFacets { Locations = BaseFacets.Locations },
                            new DataSetQueryCriteriaFacets
                            {
                                TimePeriods = new DataSetQueryCriteriaTimePeriods
                                {
                                    In =
                                    [
                                        new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
                                        new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" },
                                    ]
                                }
                            },
                        ]
                    },
                    new DataSetQueryCriteriaOr
                    {
                        Or =
                        [
                            BaseFacets with
                            {
                                TimePeriods = new DataSetQueryCriteriaTimePeriods
                                {
                                    Eq = new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
                                }
                            },
                            BaseFacets with
                            {
                                TimePeriods = new DataSetQueryCriteriaTimePeriods
                                {
                                    Eq = new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" },
                                }
                            },
                        ]
                    }
                ]
            },
        };

        [Theory]
        [MemberData(nameof(EquivalentCriteria))]
        public async Task EquivalentCriteria_Returns200(DataSetQueryCriteria criteria)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Debug = true,
                    Criteria = criteria
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            var results = viewModel.Results;

            Assert.Equal(2, results.Count);

            // Result 1

            Assert.Equal(3, results[0].Filters.Count);
            Assert.Equal("pTSoj :: Year 10", results[0].Filters["ncyear"]);
            Assert.Equal("6jrfe :: State-funded secondary", results[0].Filters["school_type"]);
            Assert.Equal("O7CLF :: Secondary sponsor led academy", results[0].Filters["academy_type"]);

            Assert.Equal(GeographicLevel.School, results[0].GeographicLevel);

            Assert.Equal(4, results[0].Locations.Count);
            Assert.Equal("pTSoj :: England (code = E92000001)", results[0].Locations["NAT"]);
            Assert.Equal("IzBzg :: Yorkshire and The Humber (code = E12000003)", results[0].Locations["REG"]);
            Assert.Equal("7zXob :: Sheffield (code = E08000019, oldCode = 373)", results[0].Locations["LA"]);
            Assert.Equal(
                "HTzLj :: Newfield Secondary School (urn = 140821, laEstab = 3734008)",
                results[0].Locations["SCH"]
            );

            Assert.Equal(TimeIdentifier.AcademicYear, results[0].TimePeriod.Code);
            Assert.Equal("2022/2023", results[0].TimePeriod.Period);

            Assert.Equal(2, results[0].Values.Count);
            Assert.Equal("751028", results[0].Values["enrolments"]);
            Assert.Equal("175843", results[0].Values["sess_authorised"]);

            // Result 2

            Assert.Equal(3, results[1].Filters.Count);
            Assert.Equal("pTSoj :: Year 10", results[1].Filters["ncyear"]);
            Assert.Equal("6jrfe :: State-funded secondary", results[1].Filters["school_type"]);
            Assert.Equal("O7CLF :: Secondary sponsor led academy", results[1].Filters["academy_type"]);

            Assert.Equal(GeographicLevel.School, results[1].GeographicLevel);

            Assert.Equal(4, results[1].Locations.Count);
            Assert.Equal("pTSoj :: England (code = E92000001)", results[1].Locations["NAT"]);
            Assert.Equal("IzBzg :: Yorkshire and The Humber (code = E12000003)", results[1].Locations["REG"]);
            Assert.Equal("7zXob :: Sheffield (code = E08000019, oldCode = 373)", results[1].Locations["LA"]);
            Assert.Equal(
                "HTzLj :: Newfield Secondary School (urn = 140821, laEstab = 3734008)",
                results[1].Locations["SCH"]
            );

            Assert.Equal(TimeIdentifier.AcademicYear, results[1].TimePeriod.Code);
            Assert.Equal("2021/2022", results[1].TimePeriod.Period);

            Assert.Equal(2, results[1].Values.Count);
            Assert.Equal("752009", results[1].Values["enrolments"]);
            Assert.Equal("262396", results[1].Values["sess_authorised"]);
        }
    }

    public class NotConditionTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        private static readonly DataSetQueryCriteriaFacets BaseFacets = new()
        {
            Filters = new DataSetQueryCriteriaFilters
            {
                In = ["LxWjE", "7zXob"],
            },
            GeographicLevels = new DataSetGetQueryGeographicLevels
            {
                In = ["NAT", "REG", "LA"]
            },
            Locations = new DataSetQueryCriteriaLocations
            {
                In =
                [
                    new DataSetQueryLocationLocalAuthorityCode { Code = "E09000003" },
                    new DataSetQueryLocationLocalAuthorityCode { Code = "E08000016" },
                    new DataSetQueryLocationLocalAuthorityCode { Code = "E09000021 / E09000027" },
                ]
            },
            TimePeriods = new DataSetQueryCriteriaTimePeriods
            {
                In =
                [
                    new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" },
                    new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" },
                ]
            }
        };

        public static readonly TheoryData<DataSetQueryCriteria> EquivalentCriteria = new()
        {
            new DataSetQueryCriteriaNot
            {
                Not = BaseFacets
            },
            new DataSetQueryCriteriaNot
            {
                Not = new DataSetQueryCriteriaNot
                {
                    Not = new DataSetQueryCriteriaNot
                    {
                        Not = BaseFacets
                    }
                }
            },
            new DataSetQueryCriteriaNot
            {
                Not = new DataSetQueryCriteriaNot
                {
                    Not = new DataSetQueryCriteriaAnd
                    {
                        And = [
                            new DataSetQueryCriteriaFacets
                            {
                                Filters = new DataSetQueryCriteriaFilters
                                {
                                    NotEq = "7zXob",
                                    In = ["9U4vZ", "O7CLF"]
                                },
                                GeographicLevels = new DataSetGetQueryGeographicLevels
                                {
                                    NotEq = "NAT"
                                },
                                Locations = new DataSetQueryCriteriaLocations
                                {
                                    Eq = new DataSetQueryLocationId { Level = "NAT", Id = "pTSoj" },
                                    NotIn =
                                    [
                                        new DataSetQueryLocationCode { Level = "REG", Code = "E13000002" },
                                        new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "370" },
                                    ]
                                },
                                TimePeriods = new DataSetQueryCriteriaTimePeriods
                                {
                                    Eq = new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" }
                                }
                            }
                        ]
                    }
                }
            },
            new DataSetQueryCriteriaNot
            {
                Not = new DataSetQueryCriteriaOr
                {
                    Or =
                    [
                        new DataSetQueryCriteriaFacets
                        {
                            Filters = new DataSetQueryCriteriaFilters
                            {
                                In = ["LxWjE", "7zXob"],
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            GeographicLevels = new DataSetGetQueryGeographicLevels
                            {
                                In = ["NAT", "REG", "LA"]
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            Locations = new DataSetQueryCriteriaLocations
                            {
                                In =
                                [
                                    new DataSetQueryLocationLocalAuthorityCode { Code = "E09000003" },
                                    new DataSetQueryLocationLocalAuthorityCode { Code = "E08000016" },
                                    new DataSetQueryLocationLocalAuthorityCode { Code = "E09000021 / E09000027" },
                                ]
                            },
                        },
                        new DataSetQueryCriteriaFacets
                        {
                            TimePeriods = new DataSetQueryCriteriaTimePeriods
                            {
                                In =
                                [
                                    new DataSetQueryTimePeriod { Period = "2020/2021", Code = "AY" },
                                    new DataSetQueryTimePeriod { Period = "2022/2023", Code = "AY" },
                                ]
                            },
                        }
                    ]
                }
            },
        };

        [Theory]
        [MemberData(nameof(EquivalentCriteria))]
        public async Task EquivalentCriteria_Returns200(DataSetQueryCriteria criteria)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = ["enrolments", "sess_authorised"],
                    Debug = true,
                    Criteria = criteria
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            var result = Assert.Single(viewModel.Results);

            Assert.Equal(3, result.Filters.Count);
            Assert.Equal("pTSoj :: Year 10", result.Filters["ncyear"]);
            Assert.Equal("6jrfe :: State-funded secondary", result.Filters["school_type"]);
            Assert.Equal("O7CLF :: Secondary sponsor led academy", result.Filters["academy_type"]);

            Assert.Equal(GeographicLevel.School, result.GeographicLevel);

            Assert.Equal(4, result.Locations.Count);
            Assert.Equal("pTSoj :: England (code = E92000001)", result.Locations["NAT"]);
            Assert.Equal("IzBzg :: Yorkshire and The Humber (code = E12000003)", result.Locations["REG"]);
            Assert.Equal("7zXob :: Sheffield (code = E08000019, oldCode = 373)", result.Locations["LA"]);
            Assert.Equal(
                "HTzLj :: Newfield Secondary School (urn = 140821, laEstab = 3734008)",
                result.Locations["SCH"]
            );

            Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriod.Code);
            Assert.Equal("2021/2022", result.TimePeriod.Period);

            Assert.Equal(2, result.Values.Count);
            Assert.Equal("752009", result.Values["enrolments"]);
            Assert.Equal("262396", result.Values["sess_authorised"]);
        }
    }

    private async Task<HttpResponseMessage> QueryDataSet(
        Guid dataSetId,
        DataSetQueryRequest request,
        string? dataSetVersion = null)
    {
        var query = new Dictionary<string, StringValues>();

        if (dataSetVersion is not null)
        {
            query["dataSetVersion"] = dataSetVersion;
        }

        var client = BuildApp().CreateClient();

        var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/query", query);

        return await client.PostAsJsonAsync(uri, request);
    }

    private async Task<DataSetVersion> SetupDefaultDataSetVersion(
        DataSetVersionStatus versionStatus = DataSetVersionStatus.Published)
    {
        DataSet dataSet = DataFixture
            .DefaultDataSet()
            .WithStatusPublished();

        await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

        DataSetVersion dataSetVersion = DataFixture
            .DefaultDataSetVersion()
            .WithDataSet(dataSet)
            .WithMetaSummary(
                DataFixture.DefaultDataSetVersionMetaSummary()
                    .WithGeographicLevels(
                        [
                            GeographicLevel.Country,
                            GeographicLevel.LocalAuthority,
                            GeographicLevel.Region,
                            GeographicLevel.School
                        ]
                    )
            )
            .WithStatus(versionStatus);

        dataSet.LatestLiveVersion = dataSetVersion;

        await TestApp.AddTestData<PublicDataDbContext>(
            context =>
            {
                context.DataSetVersions.Add(dataSetVersion);
                context.DataSets.Update(dataSet);
            }
        );

        return dataSetVersion;
    }

    private WebApplicationFactory<Startup> BuildApp()
    {
        return TestApp.ConfigureServices(services =>
            services.ReplaceService<IDataSetVersionPathResolver>(_dataSetVersionPathResolver));
    }

    private static QueryResultsMeta GatherQueryResultsMeta(DataSetQueryPaginatedResultsViewModel viewModel)
    {
        var filters = new Dictionary<string, HashSet<string>>();
        var locations = new Dictionary<string, HashSet<string>>();
        var geographicLevels = new HashSet<GeographicLevel>();
        var timePeriods = new HashSet<TimePeriodViewModel>();
        var indicators = new HashSet<string>();

        foreach (var result in viewModel.Results)
        {
            foreach (var filter in result.Filters)
            {
                if (!filters.ContainsKey(filter.Key))
                {
                    filters[filter.Key] = [filter.Value];
                }
                else
                {
                    filters[filter.Key].Add(filter.Value);
                }
            }

            foreach (var location in result.Locations)
            {
                if (!locations.ContainsKey(location.Key))
                {
                    locations[location.Key] = [location.Value];
                }
                else
                {
                    locations[location.Key].Add(location.Value);
                }
            }

            geographicLevels.Add(result.GeographicLevel);
            timePeriods.Add(result.TimePeriod);
            indicators.AddRange(result.Values.Keys);
        }

        return new QueryResultsMeta
        {
            Filters = filters,
            Indicators = indicators,
            Locations = locations,
            GeographicLevels = geographicLevels,
            TimePeriods = timePeriods,
        };
    }

    private record QueryResultsMeta
    {
        public required Dictionary<string, HashSet<string>> Filters { get; init; } = [];
        public required Dictionary<string, HashSet<string>> Locations { get; init; } = [];
        public required HashSet<GeographicLevel> GeographicLevels { get; init; } = [];
        public required HashSet<TimePeriodViewModel> TimePeriods { get; init; } = [];
        public required HashSet<string> Indicators { get; init; } = [];
    }
}