using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Resources.DataFiles.AbsenceSchool;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.ViewModels;
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
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class DataSetsControllerPostQueryTests(TestApplicationFactory testApp) : IntegrationTestFixtureWithCommonTestDataSetup(testApp)
{
    private const string BaseUrl = "v1/data-sets";

    private readonly TestDataSetVersionPathResolver _dataSetVersionPathResolver = new()
    {
        Directory = "AbsenceSchool"
    };

    private readonly TestAnalyticsPathResolver _analyticsPathResolver = new();

    public class AccessTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [MemberData(nameof(DataSetVersionStatusQueryTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
        public async Task VersionNotAvailable_Returns403(DataSetVersionStatus versionStatus)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion(versionStatus);

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised]
                }
            );

            response.AssertForbidden();
        }

        [Theory]
        [MemberData(nameof(DataSetVersionStatusQueryTheoryData.AvailableStatuses),
            MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
        public async Task VersionAvailable_Returns200(DataSetVersionStatus versionStatus)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion(versionStatus);

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised]
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised]
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised]
                }
            );

            response.AssertNotFound();
        }
        [Theory]
        [MemberData(nameof(DataSetVersionStatusQueryTheoryData.NonPublishedStatus),
            MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
        public async Task WildCardSpecified_RequestsNonPublishedVersion_Returns404(DataSetVersionStatus versionStatus)
        {
            var (dataSet, versions) = await SetupDataSetWithSpecifiedVersionStatuses(versionStatus);
            
            var response = await QueryDataSet(
                dataSetId: dataSet.Id,
                dataSetVersion: "2.*",
                previewTokenId: versions.Last().PreviewTokens[0].Id,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised]
                });

            response.AssertNotFound();
        }
        
        [Fact]
        public async Task WildCardSpecified_RequestPublishedVersion_Returns200()
        {
            var (dataSet, versions) = await SetupDataSetWithSpecifiedVersionStatuses(DataSetVersionStatus.Published);
            
            var response = await QueryDataSet(
                dataSetId: dataSet.Id,
                dataSetVersion: "2.*",
                previewTokenId: versions.Last().PreviewTokens[0].Id,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised]
                });

            response.AssertOk();
            
            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(216, viewModel.Paging.TotalResults);

            Assert.Empty(viewModel.Warnings);

            Assert.Equal(216, viewModel.Results.Count);
        }
    }

    public class PreviewTokenTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Theory]
        [MemberData(nameof(DataSetVersionStatusQueryTheoryData.AvailableStatusesIncludingDraft),
            MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
        public async Task PreviewTokenIsActive_Returns200(DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()]);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await QueryDataSet(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                previewTokenId: dataSetVersion.PreviewTokens[0].Id,
                request: new DataSetQueryRequest { Indicators = [AbsenceSchoolData.IndicatorSessAuthorised] });

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            Assert.NotNull(viewModel);
        }

        [Fact]
        public async Task PreviewTokenIsExpired_Returns403()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(DataSetVersionStatus.Draft)
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)]);

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

            var response = await QueryDataSet(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                previewTokenId: dataSetVersion.PreviewTokens[0].Id,
                request: new DataSetQueryRequest { Indicators = [AbsenceSchoolData.IndicatorSessAuthorised] });

            response.AssertForbidden();
        }

        [Fact]
        public async Task PreviewTokenIsForWrongDataSetVersion_Returns403()
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            var (dataSetVersion1, dataSetVersion2) = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(DataSetVersionStatus.Draft)
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                .Generate(2)
                .ToTuple2();

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2));

            var response = await QueryDataSet(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion1.PublicVersion,
                previewTokenId: dataSetVersion2.PreviewTokens[0].Id,
                request: new DataSetQueryRequest { Indicators = [AbsenceSchoolData.IndicatorSessAuthorised] });

            response.AssertForbidden();
        }

        [Theory]
        [MemberData(nameof(DataSetVersionStatusQueryTheoryData.UnavailableStatusesExceptDraft),
            MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
        public async Task PreviewTokenIsForUnavailableDataSetVersion_Returns403(
            DataSetVersionStatus dataSetVersionStatus)
        {
            DataSet dataSet = DataFixture
                .DefaultDataSet()
                .WithStatusPublished();

            await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

            DataSetVersion dataSetVersion = DataFixture
                .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                .WithStatus(dataSetVersionStatus)
                .WithDataSetId(dataSet.Id)
                .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()]);

            await TestApp.AddTestData<PublicDataDbContext>(context =>
                context.DataSetVersions.AddRange(dataSetVersion));

            var response = await QueryDataSet(
                dataSetId: dataSet.Id,
                dataSetVersion: dataSetVersion.PublicVersion,
                previewTokenId: dataSetVersion.PreviewTokens[0].Id,
                request: new DataSetQueryRequest { Indicators = [AbsenceSchoolData.IndicatorSessAuthorised] });

            response.AssertForbidden();
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                new('a', 11),
                new('a', 12),
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                new('a', 11),
                ""
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                            new DataSetQueryLocationLocalAuthorityCode { Code = new string('a', 31) },
                            new DataSetQueryLocationLocalAuthorityOldCode { OldCode = new string('a', 21) },
                            new DataSetQueryLocationProviderUkprn { Ukprn = new string('a', 21) },
                            new DataSetQueryLocationSchoolUrn { Urn = new string('a', 21) },
                            new DataSetQueryLocationSchoolLaEstab { LaEstab = new string('a', 21) }
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
            validationProblem.AssertHasMaximumLengthError($"{path}[7].code", maxLength: 30);
            validationProblem.AssertHasMaximumLengthError($"{path}[8].oldCode", maxLength: 20);
            validationProblem.AssertHasMaximumLengthError($"{path}[9].ukprn", maxLength: 20);
            validationProblem.AssertHasMaximumLengthError($"{path}[10].urn", maxLength: 20);
            validationProblem.AssertHasMaximumLengthError($"{path}[11].laEstab", maxLength: 20);
        }

        [Fact]
        public async Task AllComparatorsInvalid_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            IDataSetQueryLocation[] invalidLocations =
            [
                new DataSetQueryLocationId { Level = "", Id = "" },
                new DataSetQueryLocationId { Level = "NAT", Id = " " },
                new DataSetQueryLocationId { Level = "NAT", Id = new string('a', 11) },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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

            Assert.Equal(7, validationProblem.Errors.Count);

            validationProblem.AssertHasAllowedValueError(
                expectedPath: "criteria.locations.eq.level",
                value: "LADD",
                allowed: GeographicLevelUtils.OrderedCodes
            );
            validationProblem.AssertHasNotEmptyError(expectedPath: "criteria.locations.notEq.id");
            validationProblem.AssertHasNotEmptyError(expectedPath: "criteria.locations.in[0].id");
            validationProblem.AssertHasAllowedValueError(
                expectedPath: "criteria.locations.in[0].level",
                value: "",
                allowed: GeographicLevelUtils.OrderedCodes
            );
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

            IDataSetQueryLocation[] notFoundLocations =
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                new() { Code = "", Period = "" },
                new() { Code = "AY", Period = "2020/2019" },
                new() { Code = "AY", Period = "2020/2022" },
                new() { Code = "INVALID", Period = "2020" },
                new() { Code = "CY", Period = "2020/2021" },
                new() { Code = "CYQ2", Period = "2020/2021" },
                new() { Code = "RY", Period = "2020/2021" },
                new() { Code = "W10", Period = "2020/2021" },
                new() { Code = "M5", Period = "2020/2021" }
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                new()
                    { Code = "INVALID", Period = "2020" },
                new()
                    { Code = "CY", Period = "" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                new() { Code = "CY", Period = "2021" },
                new() { Code = "CY", Period = "2022" },
                new() { Code = "CY", Period = "2030" },
                new() { Code = "AY", Period = "2023/2024" },
                new() { Code = "AY", Period = "2018/2019" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                new() { Field = "invalid1", Direction = "Asc" },
                new() { Field = "invalid2", Direction = "Desc" },
                new() { Field = "filter", Direction = "Asc" },
                new() { Field = "filter|invalid", Direction = "Asc" },
                new() { Field = "location", Direction = "Asc" },
                new() { Field = "location|invalid", Direction = "Asc" },
                new() { Field = "indicator", Direction = "Asc" },
                new() { Field = "indicator|invalid", Direction = "Asc" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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

            const string filterOptionId = AbsenceSchoolData.FilterNcYear4;

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Assert.Single(meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    Assert.Contains(filterOptionId, meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    break;
                case "NotEq":
                case "NotIn":
                    Assert.Equal(3, meta.Filters[AbsenceSchoolData.FilterNcYear].Count);
                    Assert.DoesNotContain(filterOptionId, meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 108)]
        [InlineData("NotIn", 108)]
        public async Task MultipleOptionsInSameFilter_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] filterOptionIds = [AbsenceSchoolData.FilterNcYear4, AbsenceSchoolData.FilterNcYear8];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
            Assert.Contains(AbsenceSchoolData.IndicatorEnrolments, meta.Indicators);
            Assert.Contains(AbsenceSchoolData.IndicatorSessAuthorised, meta.Indicators);

            switch (comparator)
            {
                case "In":
                    Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterNcYear].Count);
                    Assert.Contains(filterOptionIds[0], meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    Assert.Contains(filterOptionIds[1], meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    break;
                case "NotIn":
                    Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterNcYear].Count);
                    Assert.DoesNotContain(filterOptionIds[0], meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    Assert.DoesNotContain(filterOptionIds[1], meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 150)]
        [InlineData("NotIn", 66)]
        public async Task MultipleOptionsInDifferentFilters_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] filterOptionIds = [
                AbsenceSchoolData.FilterSchoolTypeTotal,
                AbsenceSchoolData.FilterSchoolTypeSecondary,
                AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterSchoolType].Count);
                    Assert.Contains(filterOptionIds[0], meta.Filters[AbsenceSchoolData.FilterSchoolType]);
                    Assert.Contains(filterOptionIds[1], meta.Filters[AbsenceSchoolData.FilterSchoolType]);

                    Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterAcademyType].Count);
                    Assert.Contains(filterOptionIds[2], meta.Filters[AbsenceSchoolData.FilterAcademyType]);
                    Assert.Contains(filterOptionIds[3], meta.Filters[AbsenceSchoolData.FilterAcademyType]);
                    break;
                case "NotIn":
                    Assert.Single(meta.Filters[AbsenceSchoolData.FilterSchoolType]);
                    Assert.DoesNotContain(filterOptionIds[0], meta.Filters[AbsenceSchoolData.FilterSchoolType]);
                    Assert.DoesNotContain(filterOptionIds[1], meta.Filters[AbsenceSchoolData.FilterSchoolType]);

                    Assert.Single(meta.Filters[AbsenceSchoolData.FilterAcademyType]);
                    Assert.DoesNotContain(filterOptionIds[2], meta.Filters[AbsenceSchoolData.FilterAcademyType]);
                    Assert.DoesNotContain(filterOptionIds[3], meta.Filters[AbsenceSchoolData.FilterAcademyType]);
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
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Assert.Contains(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    break;
                case "NotEq":
                case "NotIn":
                    Assert.Equal(3, meta.Locations["LA"].Count);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 72)]
        [InlineData("NotIn", 144)]
        public async Task MultipleOptionsInSameLevel_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            IDataSetQueryLocation[] locations =
            [
                // Sheffield
                new DataSetQueryLocationLocalAuthorityCode { Code = "E08000019" },
                new DataSetQueryLocationId { Level = "LA", Id = AbsenceSchoolData.LocationLaBarnsley }
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Assert.Contains(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    Assert.Contains(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);
                    break;
                case "NotIn":
                    Assert.Equal(2, meta.Locations["LA"].Count);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);
                    break;
            }
        }

        [Theory]
        [InlineData("In", 84)]
        [InlineData("NotIn", 132)]
        public async Task MultipleOptionsInDifferentLevels_Returns200(string comparator, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            IDataSetQueryLocation[] locations =
            [
                // Sheffield
                new DataSetQueryLocationLocalAuthorityCode { Code = "E08000019" },
                new DataSetQueryLocationId { Level = "LA", Id = AbsenceSchoolData.LocationLaBarnsley },
                // The Kingston Academy
                new DataSetQueryLocationSchoolLaEstab { LaEstab = "3144001" },
                // King Athelstan Primary School
                new DataSetQueryLocationSchoolUrn { Urn = "102579" }
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Assert.Contains(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    Assert.Contains(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);

                    Assert.Equal(6, meta.Locations["SCH"].Count);
                    Assert.Contains(AbsenceSchoolData.LocationSchoolKingstonAcademy, meta.Locations["SCH"]);
                    Assert.Contains(AbsenceSchoolData.LocationSchoolKingAthelstanPrimary, meta.Locations["SCH"]);
                    break;
                case "NotIn":
                    Assert.Equal(2, meta.Locations["LA"].Count);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);

                    Assert.Equal(2, meta.Locations["SCH"].Count);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationSchoolKingstonAcademy, meta.Locations["SCH"]);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationSchoolKingAthelstanPrimary, meta.Locations["SCH"]);
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
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                new() { Code = "AY", Period = "2021" },
                new() { Code = "AY", Period = "2022/2023" },
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                new() { Code = TimeIdentifier.AcademicYear, Period = "2021/2022" },
                new() { Code = TimeIdentifier.AcademicYear, Period = "2022/2023" }
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            Eq = new DataSetQueryLocationId
                            {
                                Level = "LA",
                                Id = AbsenceSchoolData.LocationLaSheffield
                            }
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised],
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
                    Indicators = [AbsenceSchoolData.IndicatorSessAuthorised]
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
            Assert.Equal(AbsenceSchoolData.FilterNcYear4, result.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal(AbsenceSchoolData.FilterSchoolTypeTotal, result.Filters[AbsenceSchoolData.FilterSchoolType]);

            Assert.Equal(GeographicLevel.LocalAuthority, result.GeographicLevel);

            Assert.Equal(3, result.Locations.Count);
            Assert.Equal(AbsenceSchoolData.LocationLaBarnsley, result.Locations["LA"]);
            Assert.Equal(AbsenceSchoolData.LocationNatEngland, result.Locations["NAT"]);
            Assert.Equal(AbsenceSchoolData.LocationRegionYorkshire, result.Locations["REG"]);

            Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriod.Code);
            Assert.Equal("2022/2023", result.TimePeriod.Period);

            Assert.Single(result.Values);
            Assert.Equal("577798", result.Values[AbsenceSchoolData.IndicatorSessAuthorised]);
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
                        AbsenceSchoolData.IndicatorEnrolments,
                        AbsenceSchoolData.IndicatorSessAuthorised,
                        AbsenceSchoolData.IndicatorSessPossible,
                        AbsenceSchoolData.IndicatorSessUnauthorised,
                        AbsenceSchoolData.IndicatorSessUnauthorisedPercent,
                    ]
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(216, viewModel.Results.Count);

            var values = viewModel.Results
                .SelectMany(result => result.Values)
                .GroupBy(kv => kv.Key, kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.ToList());

            var enrolments = values[AbsenceSchoolData.IndicatorEnrolments].Select(int.Parse).ToList();

            Assert.Equal(216, enrolments.Count);
            Assert.Equal(999598, enrolments.Max());
            Assert.Equal(1072, enrolments.Min());

            var sessAuthorised = values[AbsenceSchoolData.IndicatorSessAuthorised].Select(int.Parse).ToList();

            Assert.Equal(216, sessAuthorised.Count);
            Assert.Equal(4967515, sessAuthorised.Max());
            Assert.Equal(22441, sessAuthorised.Min());

            var sessPossible = values[AbsenceSchoolData.IndicatorSessPossible].Select(int.Parse).ToList();

            Assert.Equal(216, sessPossible.Count);
            Assert.Equal(9934276, sessPossible.Max());
            Assert.Equal(18306, sessPossible.Min());

            var sessUnauthorised = values[AbsenceSchoolData.IndicatorSessUnauthorised].Select(int.Parse).ToList();

            Assert.Equal(216, sessUnauthorised.Count);
            Assert.Equal(494993, sessUnauthorised.Max());
            Assert.Equal(2883, sessUnauthorised.Min());

            var sessUnauthorisedPercent = values[AbsenceSchoolData.IndicatorSessUnauthorisedPercent].Select(float.Parse).ToList();

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
                        AbsenceSchoolData.IndicatorEnrolments,
                        AbsenceSchoolData.IndicatorSessAuthorised,
                        AbsenceSchoolData.IndicatorSessPossible,
                        AbsenceSchoolData.IndicatorSessUnauthorised,
                        AbsenceSchoolData.IndicatorSessUnauthorisedPercent,
                    ]
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(216, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(3, meta.Filters.Count);

            Assert.Equal(3, meta.Filters[AbsenceSchoolData.FilterAcademyType].Count);

            Assert.Contains(
                AbsenceSchoolData.FilterAcademyTypePrimarySponsorLed,
                meta.Filters[AbsenceSchoolData.FilterAcademyType]
            );
            Assert.Contains(
                AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                meta.Filters[AbsenceSchoolData.FilterAcademyType]
            );
            Assert.Contains(
                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed,
                meta.Filters[AbsenceSchoolData.FilterAcademyType]
            );

            Assert.Equal(4, meta.Filters[AbsenceSchoolData.FilterNcYear].Count);
            Assert.Contains(AbsenceSchoolData.FilterNcYear4, meta.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Contains(AbsenceSchoolData.FilterNcYear6, meta.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Contains(AbsenceSchoolData.FilterNcYear8, meta.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Contains(AbsenceSchoolData.FilterNcYear10, meta.Filters[AbsenceSchoolData.FilterNcYear]);

            Assert.Equal(3, meta.Filters[AbsenceSchoolData.FilterSchoolType].Count);
            Assert.Contains(
                AbsenceSchoolData.FilterSchoolTypePrimary,
                meta.Filters[AbsenceSchoolData.FilterSchoolType]
            );
            Assert.Contains(
                AbsenceSchoolData.FilterSchoolTypeSecondary,
                meta.Filters[AbsenceSchoolData.FilterSchoolType]
            );
            Assert.Contains(AbsenceSchoolData.FilterSchoolTypeTotal, meta.Filters[AbsenceSchoolData.FilterSchoolType]);

            Assert.Equal(4, meta.Locations.Count);

            Assert.Single(meta.Locations["NAT"]);
            Assert.Contains(AbsenceSchoolData.LocationNatEngland, meta.Locations["NAT"]);

            Assert.Equal(2, meta.Locations["REG"].Count);
            Assert.Contains(AbsenceSchoolData.LocationRegionOuterLondon, meta.Locations["REG"]);
            Assert.Contains(AbsenceSchoolData.LocationRegionYorkshire, meta.Locations["REG"]);

            Assert.Equal(4, meta.Locations["LA"].Count);
            Assert.Contains(AbsenceSchoolData.LocationLaBarnet, meta.Locations["LA"]);
            Assert.Contains(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);
            Assert.Contains(AbsenceSchoolData.LocationLaKingstonUponThames, meta.Locations["LA"]);
            Assert.Contains(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);

            Assert.Equal(8, meta.Locations["SCH"].Count);
            Assert.Contains(AbsenceSchoolData.LocationSchoolColindalePrimary, meta.Locations["SCH"]);
            Assert.Contains(AbsenceSchoolData.LocationSchoolGreenhillPrimary, meta.Locations["SCH"]);
            Assert.Contains(AbsenceSchoolData.LocationSchoolHoylandPrimary, meta.Locations["SCH"]);
            Assert.Contains(AbsenceSchoolData.LocationSchoolKingAthelstanPrimary, meta.Locations["SCH"]);
            Assert.Contains(AbsenceSchoolData.LocationSchoolNewfieldSecondary, meta.Locations["SCH"]);
            Assert.Contains(AbsenceSchoolData.LocationSchoolPenistoneGrammar, meta.Locations["SCH"]);
            Assert.Contains(AbsenceSchoolData.LocationSchoolKingstonAcademy, meta.Locations["SCH"]);
            Assert.Contains(AbsenceSchoolData.LocationSchoolWrenAcademy, meta.Locations["SCH"]);

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
            Assert.Contains(AbsenceSchoolData.IndicatorEnrolments, meta.Indicators);
            Assert.Contains(AbsenceSchoolData.IndicatorSessAuthorised, meta.Indicators);
            Assert.Contains(AbsenceSchoolData.IndicatorSessPossible, meta.Indicators);
            Assert.Contains(AbsenceSchoolData.IndicatorSessUnauthorised, meta.Indicators);
            Assert.Contains(AbsenceSchoolData.IndicatorSessUnauthorisedPercent, meta.Indicators);
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
                        AbsenceSchoolData.IndicatorEnrolments,
                        AbsenceSchoolData.IndicatorSessAuthorised,
                        AbsenceSchoolData.IndicatorSessPossible,
                        AbsenceSchoolData.IndicatorSessUnauthorised,
                        AbsenceSchoolData.IndicatorSessUnauthorisedPercent,
                    ],
                    Debug = true
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(216, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(3, meta.Filters.Count);

            var academyTypes = meta.Filters[$"{AbsenceSchoolData.FilterAcademyType} :: academy_type"];

            Assert.Equal(3, academyTypes.Count);
            Assert.Contains(
                $"{AbsenceSchoolData.FilterAcademyTypePrimarySponsorLed} :: Primary sponsor led academy",
                academyTypes
            );
            Assert.Contains(
                $"{AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool} :: Secondary free school",
                academyTypes
            );
            Assert.Contains(
                $"{AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed} :: Secondary sponsor led academy",
                academyTypes
            );

            var ncYears = meta.Filters[$"{AbsenceSchoolData.FilterNcYear} :: ncyear"];

            Assert.Equal(4, ncYears.Count);
            Assert.Contains($"{AbsenceSchoolData.FilterNcYear4} :: Year 4", ncYears);
            Assert.Contains($"{AbsenceSchoolData.FilterNcYear6} :: Year 6", ncYears);
            Assert.Contains($"{AbsenceSchoolData.FilterNcYear8} :: Year 8", ncYears);
            Assert.Contains($"{AbsenceSchoolData.FilterNcYear10} :: Year 10", ncYears);

            var schoolTypes = meta.Filters[$"{AbsenceSchoolData.FilterSchoolType} :: school_type"];

            Assert.Equal(3, schoolTypes.Count);
            Assert.Contains($"{AbsenceSchoolData.FilterSchoolTypePrimary} :: State-funded primary", schoolTypes);
            Assert.Contains($"{AbsenceSchoolData.FilterSchoolTypeSecondary} :: State-funded secondary", schoolTypes);
            Assert.Contains($"{AbsenceSchoolData.FilterSchoolTypeTotal} :: Total", schoolTypes);

            Assert.Equal(4, meta.Locations.Count);

            Assert.Single(meta.Locations["NAT"]);
            Assert.Contains(
                $"{AbsenceSchoolData.LocationNatEngland} :: England (code = E92000001)",
                meta.Locations["NAT"]
            );

            Assert.Equal(2, meta.Locations["REG"].Count);
            Assert.Contains(
                $"{AbsenceSchoolData.LocationRegionOuterLondon} :: Outer London (code = E13000002)",
                meta.Locations["REG"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationRegionYorkshire} :: Yorkshire and The Humber (code = E12000003)",
                meta.Locations["REG"]
            );

            Assert.Equal(4, meta.Locations["LA"].Count);
            Assert.Contains(
                $"{AbsenceSchoolData.LocationLaBarnet} :: Barnet (code = E09000003, oldCode = 302)",
                meta.Locations["LA"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationLaBarnsley} :: Barnsley (code = E08000016, oldCode = 370)",
                meta.Locations["LA"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationLaKingstonUponThames} :: Kingston upon Thames / Richmond upon Thames (code = E09000021 / E09000027, oldCode = 314)",
                meta.Locations["LA"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationLaSheffield} :: Sheffield (code = E08000019, oldCode = 373)",
                meta.Locations["LA"]
            );

            Assert.Equal(8, meta.Locations["SCH"].Count);
            Assert.Contains(
                $"{AbsenceSchoolData.LocationSchoolColindalePrimary} :: Colindale Primary School (urn = 101269, laEstab = 3022014)",
                meta.Locations["SCH"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationSchoolGreenhillPrimary} :: Greenhill Primary School (urn = 145374, laEstab = 3732341)",
                meta.Locations["SCH"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationSchoolHoylandPrimary} :: Hoyland Springwood Primary School (urn = 141973, laEstab = 3702039)",
                meta.Locations["SCH"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationSchoolKingAthelstanPrimary} :: King Athelstan Primary School (urn = 102579, laEstab = 3142032)",
                meta.Locations["SCH"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationSchoolNewfieldSecondary} :: Newfield Secondary School (urn = 140821, laEstab = 3734008)",
                meta.Locations["SCH"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationSchoolPenistoneGrammar} :: Penistone Grammar School (urn = 106653, laEstab = 3704027)",
                meta.Locations["SCH"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationSchoolKingstonAcademy} :: The Kingston Academy (urn = 141862, laEstab = 3144001)",
                meta.Locations["SCH"]
            );
            Assert.Contains(
                $"{AbsenceSchoolData.LocationSchoolWrenAcademy} :: Wren Academy Finchley (urn = 135507, laEstab = 3026906)",
                meta.Locations["SCH"]
            );

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
            Assert.Contains($"{AbsenceSchoolData.IndicatorEnrolments} :: enrolments", meta.Indicators);
            Assert.Contains($"{AbsenceSchoolData.IndicatorSessAuthorised} :: sess_authorised", meta.Indicators);
            Assert.Contains($"{AbsenceSchoolData.IndicatorSessPossible} :: sess_possible", meta.Indicators);
            Assert.Contains($"{AbsenceSchoolData.IndicatorSessUnauthorised} :: sess_unauthorised", meta.Indicators);
            Assert.Contains(
                $"{AbsenceSchoolData.IndicatorSessUnauthorisedPercent} :: sess_unauthorised_percent",
                meta.Indicators
            );
        }

        [Fact]
        public async Task AllFacetsMixture_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            NotEq = AbsenceSchoolData.FilterNcYear8,
                            In =
                            [
                                AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed
                            ]
                        },
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            NotEq = "NAT"
                        },
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            Eq = new DataSetQueryLocationId
                            {
                                Level = "NAT",
                                Id = AbsenceSchoolData.LocationNatEngland
                            },
                            NotIn =
                            [
                                // Outer London
                                new DataSetQueryLocationCode { Level = "REG", Code = "E13000002" },
                                // Barnsley
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
            Assert.Equal(AbsenceSchoolData.FilterNcYear10, result.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal(
                AbsenceSchoolData.FilterSchoolTypeSecondary,
                result.Filters[AbsenceSchoolData.FilterSchoolType]
            );
            Assert.Equal(
                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed,
                result.Filters[AbsenceSchoolData.FilterAcademyType]
            );

            Assert.Equal(GeographicLevel.School, result.GeographicLevel);

            Assert.Equal(4, result.Locations.Count);
            Assert.Equal(AbsenceSchoolData.LocationNatEngland, result.Locations["NAT"]);
            Assert.Equal(AbsenceSchoolData.LocationRegionYorkshire, result.Locations["REG"]);
            Assert.Equal(AbsenceSchoolData.LocationLaSheffield, result.Locations["LA"]);
            Assert.Equal(
                AbsenceSchoolData.LocationSchoolNewfieldSecondary,
                result.Locations["SCH"]
            );

            Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriod.Code);
            Assert.Equal("2021/2022", result.TimePeriod.Period);

            Assert.Equal(2, result.Values.Count);
            Assert.Equal("752009", result.Values[AbsenceSchoolData.IndicatorEnrolments]);
            Assert.Equal("262396", result.Values[AbsenceSchoolData.IndicatorSessAuthorised]);
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
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                NotEq = AbsenceSchoolData.FilterNcYear8,
                In =
                [
                    AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                    AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed
                ]
            },
            GeographicLevels = new DataSetGetQueryGeographicLevels
            {
                NotEq = "NAT"
            },
            Locations = new DataSetQueryCriteriaLocations
            {
                NotIn =
                [
                    // Outer London
                    new DataSetQueryLocationCode { Level = "REG", Code = "E13000002" },
                    // Barnsley
                    new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "370" },
                ]
            },
            TimePeriods = new DataSetQueryCriteriaTimePeriods
            {
                Eq = new DataSetQueryTimePeriod { Period = "2021/2022", Code = "AY" },
            }
        };

        public static readonly TheoryData<IDataSetQueryCriteria> EquivalentCriteria = new()
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
                                    Eq = AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                                },
                            },
                            new DataSetQueryCriteriaFacets
                            {
                                Filters = new DataSetQueryCriteriaFilters
                                {
                                    Eq = AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed,
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
        public async Task EquivalentCriteria_Returns200(IDataSetQueryCriteria criteria)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                    Criteria = criteria
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            var result = Assert.Single(viewModel.Results);

            Assert.Equal(3, result.Filters.Count);
            Assert.Equal(AbsenceSchoolData.FilterNcYear10, result.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal(
                AbsenceSchoolData.FilterSchoolTypeSecondary,
                result.Filters[AbsenceSchoolData.FilterSchoolType]
            );
            Assert.Equal(
                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed,
                result.Filters[AbsenceSchoolData.FilterAcademyType]
            );

            Assert.Equal(GeographicLevel.School, result.GeographicLevel);

            Assert.Equal(4, result.Locations.Count);
            Assert.Equal(AbsenceSchoolData.LocationNatEngland, result.Locations["NAT"]);
            Assert.Equal(AbsenceSchoolData.LocationRegionYorkshire, result.Locations["REG"]);
            Assert.Equal(AbsenceSchoolData.LocationLaSheffield, result.Locations["LA"]);
            Assert.Equal(
                AbsenceSchoolData.LocationSchoolNewfieldSecondary,
                result.Locations["SCH"]
            );

            Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriod.Code);
            Assert.Equal("2021/2022", result.TimePeriod.Period);

            Assert.Equal(2, result.Values.Count);
            Assert.Equal("752009", result.Values[AbsenceSchoolData.IndicatorEnrolments]);
            Assert.Equal("262396", result.Values[AbsenceSchoolData.IndicatorSessAuthorised]);
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
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
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
                NotEq = AbsenceSchoolData.FilterNcYear8,
                In =
                [
                    AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                    AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed
                ]
            },
            GeographicLevels = new DataSetGetQueryGeographicLevels
            {
                NotEq = "NAT"
            },
            Locations = new DataSetQueryCriteriaLocations
            {
                Eq = new DataSetQueryLocationId { Level = "NAT", Id = AbsenceSchoolData.LocationNatEngland },
                NotIn =
                [
                    // Outer London
                    new DataSetQueryLocationCode { Level = "REG", Code = "E13000002" },
                    // Barnsley
                    new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "370" },
                ]
            }
        };

        public static readonly TheoryData<IDataSetQueryCriteria> EquivalentCriteria = new()
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
        public async Task EquivalentCriteria_Returns200(IDataSetQueryCriteria criteria)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                    Criteria = criteria
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            var results = viewModel.Results;

            Assert.Equal(2, results.Count);

            // Result 1

            Assert.Equal(3, results[0].Filters.Count);
            Assert.Equal(AbsenceSchoolData.FilterNcYear10, results[0].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal(
                AbsenceSchoolData.FilterSchoolTypeSecondary,
                results[0].Filters[AbsenceSchoolData.FilterSchoolType]
            );
            Assert.Equal(
                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed,
                results[0].Filters[AbsenceSchoolData.FilterAcademyType]
            );

            Assert.Equal(GeographicLevel.School, results[0].GeographicLevel);

            Assert.Equal(4, results[0].Locations.Count);
            Assert.Equal(AbsenceSchoolData.LocationNatEngland, results[0].Locations["NAT"]);
            Assert.Equal(AbsenceSchoolData.LocationRegionYorkshire, results[0].Locations["REG"]);
            Assert.Equal(AbsenceSchoolData.LocationLaSheffield, results[0].Locations["LA"]);
            Assert.Equal(AbsenceSchoolData.LocationSchoolNewfieldSecondary, results[0].Locations["SCH"]);

            Assert.Equal(TimeIdentifier.AcademicYear, results[0].TimePeriod.Code);
            Assert.Equal("2022/2023", results[0].TimePeriod.Period);

            Assert.Equal(2, results[0].Values.Count);
            Assert.Equal("751028", results[0].Values[AbsenceSchoolData.IndicatorEnrolments]);
            Assert.Equal("175843", results[0].Values[AbsenceSchoolData.IndicatorSessAuthorised]);

            // Result 2

            Assert.Equal(3, results[1].Filters.Count);
            Assert.Equal(AbsenceSchoolData.FilterNcYear10, results[1].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal(
                AbsenceSchoolData.FilterSchoolTypeSecondary,
                results[1].Filters[AbsenceSchoolData.FilterSchoolType]
            );
            Assert.Equal(
                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed,
                results[1].Filters[AbsenceSchoolData.FilterAcademyType]
            );

            Assert.Equal(GeographicLevel.School, results[1].GeographicLevel);

            Assert.Equal(4, results[1].Locations.Count);
            Assert.Equal(AbsenceSchoolData.LocationNatEngland, results[1].Locations["NAT"]);
            Assert.Equal(AbsenceSchoolData.LocationRegionYorkshire, results[1].Locations["REG"]);
            Assert.Equal(AbsenceSchoolData.LocationLaSheffield, results[1].Locations["LA"]);
            Assert.Equal(AbsenceSchoolData.LocationSchoolNewfieldSecondary, results[1].Locations["SCH"]);

            Assert.Equal(TimeIdentifier.AcademicYear, results[1].TimePeriod.Code);
            Assert.Equal("2021/2022", results[1].TimePeriod.Period);

            Assert.Equal(2, results[1].Values.Count);
            Assert.Equal("752009", results[1].Values[AbsenceSchoolData.IndicatorEnrolments]);
            Assert.Equal("262396", results[1].Values[AbsenceSchoolData.IndicatorSessAuthorised]);
        }
    }

    public class NotConditionTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        private static readonly DataSetQueryCriteriaFacets BaseFacets = new()
        {
            Filters = new DataSetQueryCriteriaFilters
            {
                In =
                [
                    AbsenceSchoolData.FilterSchoolTypePrimary,
                    AbsenceSchoolData.FilterNcYear8
                ],
            },
            GeographicLevels = new DataSetGetQueryGeographicLevels
            {
                In = ["NAT", "REG", "LA"]
            },
            Locations = new DataSetQueryCriteriaLocations
            {
                In =
                [
                    // Barnet
                    new DataSetQueryLocationLocalAuthorityCode { Code = "E09000003" },
                    // Barnsley
                    new DataSetQueryLocationLocalAuthorityCode { Code = "E08000016" },
                    // Kingston upon Thames
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

        public static readonly TheoryData<IDataSetQueryCriteria> EquivalentCriteria = new()
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
                                    NotEq = AbsenceSchoolData.FilterNcYear8,
                                    In =
                                    [
                                        AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                                        AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed
                                    ]
                                },
                                GeographicLevels = new DataSetGetQueryGeographicLevels
                                {
                                    NotEq = "NAT"
                                },
                                Locations = new DataSetQueryCriteriaLocations
                                {
                                    Eq = new DataSetQueryLocationId
                                    {
                                        Level = "NAT",
                                        Id = AbsenceSchoolData.LocationNatEngland
                                    },
                                    NotIn =
                                    [
                                        // Outer London
                                        new DataSetQueryLocationCode { Level = "REG", Code = "E13000002" },
                                        // Barnsley
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
                                In =
                                [
                                    AbsenceSchoolData.FilterSchoolTypePrimary,
                                    AbsenceSchoolData.FilterNcYear8
                                ],
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
                                    // Barnet
                                    new DataSetQueryLocationLocalAuthorityCode { Code = "E09000003" },
                                    // Barnsley
                                    new DataSetQueryLocationLocalAuthorityCode { Code = "E08000016" },
                                    // Kingston upon Thames
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
        public async Task EquivalentCriteria_Returns200(IDataSetQueryCriteria criteria)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                    Criteria = criteria
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            var result = Assert.Single(viewModel.Results);

            Assert.Equal(3, result.Filters.Count);
            Assert.Equal(AbsenceSchoolData.FilterNcYear10, result.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal(
                AbsenceSchoolData.FilterSchoolTypeSecondary,
                result.Filters[AbsenceSchoolData.FilterSchoolType]
            );
            Assert.Equal(
                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed,
                result.Filters[AbsenceSchoolData.FilterAcademyType]
            );

            Assert.Equal(GeographicLevel.School, result.GeographicLevel);

            Assert.Equal(4, result.Locations.Count);
            Assert.Equal(AbsenceSchoolData.LocationNatEngland, result.Locations["NAT"]);
            Assert.Equal(AbsenceSchoolData.LocationRegionYorkshire, result.Locations["REG"]);
            Assert.Equal(AbsenceSchoolData.LocationLaSheffield, result.Locations["LA"]);
            Assert.Equal(AbsenceSchoolData.LocationSchoolNewfieldSecondary, result.Locations["SCH"]);

            Assert.Equal(TimeIdentifier.AcademicYear, result.TimePeriod.Code);
            Assert.Equal("2021/2022", result.TimePeriod.Period);

            Assert.Equal(2, result.Values.Count);
            Assert.Equal("752009", result.Values[AbsenceSchoolData.IndicatorEnrolments]);
            Assert.Equal("262396", result.Values[AbsenceSchoolData.IndicatorSessAuthorised]);
        }
    }

    public class SortsTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Fact]
        public async Task NoFields_SingleTimePeriod_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterSchoolTypeTotal
                        },
                        GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                        {
                            Eq = "NAT"
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020/2021"}
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(4, viewModel.Results.Count);

            Assert.All(viewModel.Results, result =>
            {
                Assert.Equal(AbsenceSchoolData.FilterSchoolTypeTotal, result.Filters[AbsenceSchoolData.FilterSchoolType]);
                Assert.Equal(GeographicLevel.Country, result.GeographicLevel);
                Assert.Equal("2020/2021", result.TimePeriod.Period);
            });

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[0].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("930365", viewModel.Results[0].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[1].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("390233", viewModel.Results[1].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear8, viewModel.Results[2].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("966035", viewModel.Results[2].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear10, viewModel.Results[3].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("687704", viewModel.Results[3].Values[AbsenceSchoolData.IndicatorEnrolments]);
        }

        [Fact]
        public async Task NoFields_MultipleTimePeriods_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterSchoolTypePrimary
                        },
                        GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                        {
                            Eq = "NAT"
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            In =
                            [
                                new DataSetQueryTimePeriod { Code = "AY", Period = "2021/2022" },
                                new DataSetQueryTimePeriod { Code = "AY", Period = "2022/2023" }
                            ]
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(4, viewModel.Results.Count);

            Assert.All(viewModel.Results, result =>
            {
                Assert.Equal(AbsenceSchoolData.FilterSchoolTypePrimary, result.Filters[AbsenceSchoolData.FilterSchoolType]);
                Assert.Equal(GeographicLevel.Country, result.GeographicLevel);
            });

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[0].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2022/2023", viewModel.Results[0].TimePeriod.Period);
            Assert.Equal("654884", viewModel.Results[0].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[1].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2022/2023", viewModel.Results[1].TimePeriod.Period);
            Assert.Equal("235647", viewModel.Results[1].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[2].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2021/2022", viewModel.Results[2].TimePeriod.Period);
            Assert.Equal("611553", viewModel.Results[2].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[3].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2021/2022", viewModel.Results[3].TimePeriod.Period);
            Assert.Equal("752711", viewModel.Results[3].Values[AbsenceSchoolData.IndicatorEnrolments]);
        }

        [Fact]
        public async Task SingleField_TimePeriodAsc_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts = [new DataSetQuerySort { Field = "timePeriod", Direction = "Asc" }],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterSchoolTypePrimary
                        },
                        GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                        {
                            Eq = "NAT"
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(6, viewModel.Results.Count);

            Assert.All(viewModel.Results, result =>
            {
                Assert.Equal(AbsenceSchoolData.FilterSchoolTypePrimary, result.Filters[AbsenceSchoolData.FilterSchoolType]);
                Assert.Equal(GeographicLevel.Country, result.GeographicLevel);
            });

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[0].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2020/2021", viewModel.Results[0].TimePeriod.Period);
            Assert.Equal("233870", viewModel.Results[0].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[1].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2020/2021", viewModel.Results[1].TimePeriod.Period);
            Assert.Equal("510682", viewModel.Results[1].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[2].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2021/2022", viewModel.Results[2].TimePeriod.Period);
            Assert.Equal("611553", viewModel.Results[2].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[3].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2021/2022", viewModel.Results[3].TimePeriod.Period);
            Assert.Equal("752711", viewModel.Results[3].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[4].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2022/2023", viewModel.Results[4].TimePeriod.Period);
            Assert.Equal("654884", viewModel.Results[4].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[5].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2022/2023", viewModel.Results[5].TimePeriod.Period);
            Assert.Equal("235647", viewModel.Results[5].Values[AbsenceSchoolData.IndicatorEnrolments]);
        }

        [Fact]
        public async Task SingleField_TimePeriodDesc_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts = [new DataSetQuerySort { Field = "timePeriod", Direction = "Desc" }],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterSchoolTypePrimary
                        },
                        GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                        {
                            Eq = "NAT"
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(6, viewModel.Results.Count);

            Assert.All(viewModel.Results, result =>
            {
                Assert.Equal(AbsenceSchoolData.FilterSchoolTypePrimary, result.Filters[AbsenceSchoolData.FilterSchoolType]);
                Assert.Equal(GeographicLevel.Country, result.GeographicLevel);
            });

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[0].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2022/2023", viewModel.Results[0].TimePeriod.Period);
            Assert.Equal("654884", viewModel.Results[0].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[1].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2022/2023", viewModel.Results[1].TimePeriod.Period);
            Assert.Equal("235647", viewModel.Results[1].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[2].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2021/2022", viewModel.Results[2].TimePeriod.Period);
            Assert.Equal("611553", viewModel.Results[2].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[3].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2021/2022", viewModel.Results[3].TimePeriod.Period);
            Assert.Equal("752711", viewModel.Results[3].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear4, viewModel.Results[4].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2020/2021", viewModel.Results[4].TimePeriod.Period);
            Assert.Equal("233870", viewModel.Results[4].Values[AbsenceSchoolData.IndicatorEnrolments]);

            Assert.Equal(AbsenceSchoolData.FilterNcYear6, viewModel.Results[5].Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal("2020/2021", viewModel.Results[5].TimePeriod.Period);
            Assert.Equal("510682", viewModel.Results[5].Values[AbsenceSchoolData.IndicatorEnrolments]);
        }

        [Fact]
        public async Task SingleField_GeographicLevelAsc_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts = [new DataSetQuerySort { Field = "geographicLevel", Direction = "Asc" }],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterNcYear4
                        },
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            Eq = new DataSetQueryLocationId { Id = AbsenceSchoolData.LocationNatEngland, Level = "NAT" }
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(18, viewModel.Results.Count);

            GeographicLevel[] expectedSequence =
            [
                GeographicLevel.LocalAuthority,
                GeographicLevel.Country,
                GeographicLevel.Region,
                GeographicLevel.School
            ];

            Assert.Equal(expectedSequence, GetSequence(viewModel.Results.Select(r => r.GeographicLevel)));
        }

        [Fact]
        public async Task SingleField_GeographicLevelDesc_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();
            
            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts = [new DataSetQuerySort { Field = "geographicLevel", Direction = "Desc" }],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterNcYear4
                        },
                        Locations = new DataSetQueryCriteriaLocations
                        {
                            Eq = new DataSetQueryLocationId { Id = AbsenceSchoolData.LocationNatEngland, Level = "NAT" }
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(18, viewModel.Results.Count);

            GeographicLevel[] expectedSequence =
            [
                GeographicLevel.School,
                GeographicLevel.Region,
                GeographicLevel.Country,
                GeographicLevel.LocalAuthority,
            ];

            Assert.Equal(expectedSequence, GetSequence(viewModel.Results.Select(r => r.GeographicLevel)));
        }

        [Fact]
        public async Task SingleField_LocationAsc_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts = [new DataSetQuerySort { Field = "location|LA", Direction = "Asc" }],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterNcYear4
                        },
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            Eq = "LA"
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(11, viewModel.Results.Count);

            string[] expectedSequence =
            [
                AbsenceSchoolData.LocationLaBarnet,
                AbsenceSchoolData.LocationLaBarnsley,
                AbsenceSchoolData.LocationLaKingstonUponThames,
                AbsenceSchoolData.LocationLaSheffield,
            ];

            Assert.Equal(expectedSequence, GetSequence(viewModel.Results.Select(r => r.Locations["LA"])));
        }

        [Fact]
        public async Task SingleField_LocationDesc_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts = [new DataSetQuerySort { Field = "location|LA", Direction = "Desc" }],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterNcYear4
                        },
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            Eq = "LA"
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(11, viewModel.Results.Count);

            string[] expectedSequence =
            [
                AbsenceSchoolData.LocationLaSheffield,
                AbsenceSchoolData.LocationLaKingstonUponThames,
                AbsenceSchoolData.LocationLaBarnsley,
                AbsenceSchoolData.LocationLaBarnet,
            ];

            Assert.Equal(expectedSequence, GetSequence(viewModel.Results.Select(r => r.Locations["LA"])));
        }

        [Fact]
        public async Task SingleField_FilterAsc_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts =
                    [
                        new DataSetQuerySort
                        {
                            Field = $"filter|{AbsenceSchoolData.FilterNcYear}",
                            Direction = "Asc"
                        }
                    ],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            Eq = "NAT"
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(8, viewModel.Results.Count);

            string[] expectedSequence =
            [
                AbsenceSchoolData.FilterNcYear10,
                AbsenceSchoolData.FilterNcYear4,
                AbsenceSchoolData.FilterNcYear6,
                AbsenceSchoolData.FilterNcYear8,
            ];

            Assert.Equal(
                expectedSequence,
                GetSequence(viewModel.Results.Select(r => r.Filters[AbsenceSchoolData.FilterNcYear]))
            );
        }

        [Fact]
        public async Task SingleField_FilterDesc_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts =
                    [
                        new DataSetQuerySort
                        {
                            Field = $"filter|{AbsenceSchoolData.FilterNcYear}",
                            Direction = "Desc"
                        }
                    ],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            Eq = "NAT"
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(8, viewModel.Results.Count);

            string[] expectedSequence =
            [
                AbsenceSchoolData.FilterNcYear8,
                AbsenceSchoolData.FilterNcYear6,
                AbsenceSchoolData.FilterNcYear4,
                AbsenceSchoolData.FilterNcYear10,
            ];

            Assert.Equal(
                expectedSequence,
                GetSequence(viewModel.Results.Select(r => r.Filters[AbsenceSchoolData.FilterNcYear]))
            );
        }

        [Fact]
        public async Task SingleField_IndicatorAsc_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts =
                    [
                        new DataSetQuerySort
                        {
                            Field = $"indicator|{AbsenceSchoolData.IndicatorEnrolments}",
                            Direction = "Asc"
                        }
                    ],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterNcYear4
                        },
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            Eq = "REG"
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(4, viewModel.Results.Count);

            string[] expectedSequence =
            [
                "636969",
                "748965",
                "794394",
                "960185",
            ];

            Assert.Equal(
                expectedSequence,
                GetSequence(viewModel.Results.Select(r => r.Values[AbsenceSchoolData.IndicatorEnrolments]))
            );
        }

        [Fact]
        public async Task SingleField_IndicatorDesc_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts =
                    [
                        new DataSetQuerySort
                        {
                            Field = $"indicator|{AbsenceSchoolData.IndicatorEnrolments}",
                            Direction = "Desc"
                        }
                    ],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterNcYear4
                        },
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            Eq = "REG"
                        },
                        TimePeriods = new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020" }
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(4, viewModel.Results.Count);

            string[] expectedSequence =
            [
                "960185",
                "794394",
                "748965",
                "636969",
            ];

            Assert.Equal(
                expectedSequence,
                GetSequence(viewModel.Results.Select(r => r.Values[AbsenceSchoolData.IndicatorEnrolments]))
            );
        }

        [Fact]
        public async Task MultipleFields_Returns200_CorrectSequence()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: new DataSetQueryRequest
                {
                    Indicators = [AbsenceSchoolData.IndicatorEnrolments],
                    Sorts =
                    [
                        new DataSetQuerySort { Field = "timePeriod", Direction = "Asc" },
                        new DataSetQuerySort { Field = "location|LA", Direction = "Desc" },
                        new DataSetQuerySort
                        {
                            Field = $"filter|{AbsenceSchoolData.FilterNcYear}",
                            Direction = "Asc"
                        }
                    ],
                    Criteria = new DataSetQueryCriteriaFacets
                    {
                        Filters = new DataSetQueryCriteriaFilters
                        {
                            Eq = AbsenceSchoolData.FilterSchoolTypeTotal
                        },
                        GeographicLevels = new DataSetGetQueryGeographicLevels
                        {
                            Eq = "LA"
                        }
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            string[] expectedTimePeriodSequence =
            [
                "2020/2021",
                "2021/2022",
                "2022/2023",
            ];

            string[] expectedLocationSequence =
            [
                AbsenceSchoolData.LocationLaSheffield,
                AbsenceSchoolData.LocationLaKingstonUponThames,
                AbsenceSchoolData.LocationLaBarnsley,
                AbsenceSchoolData.LocationLaBarnet,
            ];

            string[] expectedFilterSequence =
            [
                AbsenceSchoolData.FilterNcYear10,
                AbsenceSchoolData.FilterNcYear4,
                AbsenceSchoolData.FilterNcYear6,
                AbsenceSchoolData.FilterNcYear8,
            ];

            // Creates a cartesian product of the different combinations we expect
            var expectedSequence = expectedTimePeriodSequence
                .SelectMany(
                    _ => expectedLocationSequence,
                    (timePeriod, location) => (timePeriod, location)
                )
                .SelectMany(
                    _ => expectedFilterSequence,
                    (tuple, filter) => new
                    {
                        TimePeriod = tuple.timePeriod,
                        Location = tuple.location,
                        Filter = filter
                    }
                )
                .ToList();

            var actualSequence = viewModel.Results
                .Select(result => new
                {
                    TimePeriod = result.TimePeriod.Period,
                    Location = result.Locations["LA"],
                    Filter = result.Filters[AbsenceSchoolData.FilterNcYear]
                })
                .ToList();

            Assert.Equal(48, viewModel.Results.Count);
            Assert.Equal(actualSequence, expectedSequence);
        }

        private static List<T> GetSequence<T>(IEnumerable<T> values)
        {
            var sequence = new List<T>();

            foreach (var value in values)
            {
                var previous = sequence.LastOrDefault();

                if (value!.Equals(previous))
                {
                    continue;
                }

                sequence.Add(value);
            }

            return sequence;
        }
    }
    
    public class QueryAnalyticsEnabledTests : DataSetsControllerPostQueryTests
    {
        public QueryAnalyticsEnabledTests(TestApplicationFactory testApp) : base(testApp)
        {
            testApp.AddAppSettings("appsettings.AnalyticsEnabled.json"); 
        }

        [Fact]
        public async Task SuccessfulQuery_CapturedByAnalytics()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var request = new DataSetQueryRequest
            {
                Page = 2,
                PageSize = 3,
                Indicators = ListOf(AbsenceSchoolData.IndicatorEnrolments),
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = AbsenceSchoolData.FilterSchoolTypeTotal
                    },
                    GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                    {
                        Eq = "NAT"
                    },
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        Eq = new DataSetQueryTimePeriod
                        {
                            Code = "AY",
                            Period = "2020/2021"
                        }
                    },
                    Locations = new DataSetQueryCriteriaLocations
                    {
                        Eq = new DataSetQueryLocationId
                        {
                            Id = AbsenceSchoolData.LocationNatEngland,
                            Level = "NAT"
                        }
                    }
                },
                Sorts = ListOf(new DataSetQuerySort
                {
                    Direction = "Asc",
                    Field = "timePeriod"
                }),
                Debug = true
            };

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: request);
            
            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            
            // There are 4 results for the query above, but we are requesting page 2 and a page size of 3,
            // and so this 2nd page only displays the final single result of the 4.
            Assert.Single(viewModel.Results);
            
            // Add a slight delay as the writing of the query details for analytics is non-blocking
            // and could occur slightly after the query result is returned to the user.
            Thread.Sleep(2000);

            var analyticsPath = _analyticsPathResolver.PublicApiQueriesDirectoryPath();
            
            // Expect the successful query to have recorded its query for analytics.
            Assert.True(Directory.Exists(analyticsPath));
            var queryFiles = Directory.GetFiles(analyticsPath);
            var queryFile = Assert.Single(queryFiles);
            var contents = await File.ReadAllTextAsync(queryFile);
            var capturedQuery = JsonSerializer.Deserialize<CaptureDataSetVersionQueryRequest>(contents, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Assert.NotNull(capturedQuery);

            var expectedRequest = new DataSetQueryRequest
            {
                Page = 2,
                PageSize = 3,
                Indicators = ListOf(AbsenceSchoolData.IndicatorEnrolments),
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = AbsenceSchoolData.FilterSchoolTypeTotal
                    },
                    GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                    {
                        Eq = "NAT"
                    },
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        Eq = new DataSetQueryTimePeriod
                        {
                            Code = "AY",
                            Period = "2020/2021"
                        }
                    },
                    Locations = new DataSetQueryCriteriaLocations
                    {
                        Eq = new DataSetQueryLocationId
                        {
                            Id = AbsenceSchoolData.LocationNatEngland,
                            Level = "NAT"
                        }
                    }
                },
                Sorts = ListOf(new DataSetQuerySort
                {
                    Direction = "Asc",
                    Field = "timePeriod"
                }),
                Debug = true
            };

            capturedQuery.Query.AssertDeepEqualTo(expectedRequest);

            Assert.Equal(capturedQuery.DataSetId, dataSetVersion.DataSetId);
            Assert.Equal(capturedQuery.DataSetVersionId, dataSetVersion.Id);
            Assert.Equal(1, capturedQuery.ResultsCount);
            Assert.Equal(4, capturedQuery.TotalRowsCount);

            capturedQuery.StartTime.AssertUtcNow(withinMillis: 5000);
            capturedQuery.EndTime.AssertUtcNow(withinMillis: 5000);
            Assert.True(capturedQuery.EndTime > capturedQuery.StartTime);
        }
        
        [Fact]
        public async Task UnsuccessfulQuery_NotCapturedByAnalytics()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var request = new DataSetQueryRequest
            {
                Page = 1,
                PageSize = 1000,
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = "NonExistent"
                    }
                }
            };
            
            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: request
            );
            
            // Add a slight delay as the writing of the query details for analytics is non-blocking
            // and could occur slightly after the query result is returned to the user.
            Thread.Sleep(2000);

            // Check that the folder for capturing queries for analytics was never created.
            Assert.False(Directory.Exists(_analyticsPathResolver.PublicApiQueriesDirectoryPath()));

            response.AssertBadRequest();
        }
        
        [Fact]
        public async Task AdminRequest_NotCapturedByAnalytics()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var request = new DataSetQueryRequest
            {
                Page = 2,
                PageSize = 3,
                Indicators = ListOf(AbsenceSchoolData.IndicatorEnrolments),
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = AbsenceSchoolData.FilterSchoolTypeTotal
                    },
                    GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                    {
                        Eq = "NAT"
                    },
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        Eq = new DataSetQueryTimePeriod
                        {
                            Code = "AY",
                            Period = "2020/2021"
                        }
                    },
                    Locations = new DataSetQueryCriteriaLocations
                    {
                        Eq = new DataSetQueryLocationId
                        {
                            Id = AbsenceSchoolData.LocationNatEngland,
                            Level = "NAT"
                        }
                    }
                },
                Sorts = ListOf(new DataSetQuerySort
                {
                    Direction = "Asc",
                    Field = "timePeriod"
                }),
                Debug = true
            };

            var adminUser = DataFixture.AdminAccessUser();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: request,
                user: adminUser);
            
            response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            
            // Add a slight delay as the writing of the query details for analytics is non-blocking
            // and could occur slightly after the query result is returned to the user.
            Thread.Sleep(2000);

            var analyticsPath = _analyticsPathResolver.PublicApiQueriesDirectoryPath();
            
            // Expect the successful call to have been omitted from analytics because it originates
            // from the Admin App.
            Assert.False(Directory.Exists(analyticsPath));
        }
        
        [Fact]
        public async Task ExceptionThrownByQueryAnalyticsManager_SuccessfulResultsStillReturned()
        {
            // Set up the manager to throw an exception when the service attempts to add a query to it.
            var analyticsManagerMock = new Mock<IAnalyticsManager>(MockBehavior.Strict);
            
            analyticsManagerMock
                .Setup(m => m.Read(It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await Task.Delay(Timeout.Infinite);
                    return null!;
                });
            
            analyticsManagerMock
                .Setup(m => m.Add(
                    It.IsAny<CaptureDataSetVersionQueryRequest>(), 
                    It.IsAny<CancellationToken>()))
                .Throws(new Exception("Error"));
            
            var app = TestApp.ConfigureServices(services => services
                .ReplaceService<IDataSetVersionPathResolver>(_dataSetVersionPathResolver)
                .ReplaceService(analyticsManagerMock));
            
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var request = new DataSetQueryRequest
            {
                Indicators = ListOf(AbsenceSchoolData.IndicatorEnrolments),
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters { Eq = AbsenceSchoolData.FilterSchoolTypeTotal },
                    GeographicLevels = new DataSetQueryCriteriaGeographicLevels { Eq = "NAT" },
                    TimePeriods =
                        new DataSetQueryCriteriaTimePeriods
                        {
                            Eq = new DataSetQueryTimePeriod { Code = "AY", Period = "2020/2021" }
                        },
                    Locations = new DataSetQueryCriteriaLocations
                    {
                        Eq = new DataSetQueryLocationId { Id = AbsenceSchoolData.LocationNatEngland, Level = "NAT" }
                    }
                },
                Sorts = ListOf(new DataSetQuerySort { Direction = "Asc", Field = "timePeriod" })
            };

            var response = await QueryDataSet(
                app: app,
                dataSetId: dataSetVersion.DataSetId,
                request: request);
            
            // Assert that the mock threw an exception as expected.
            analyticsManagerMock
                .Verify(s => s.Add(
                    It.IsAny<CaptureDataSetVersionQueryRequest>(), 
                    It.IsAny<CancellationToken>()));

            // Assert that the result was still returned despite the above exception.
            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            Assert.Equal(4, viewModel.Results.Count);
        }
    }

    public class QueryAnalyticsDisabledTests(TestApplicationFactory testApp) : DataSetsControllerPostQueryTests(testApp)
    {
        [Fact]
        public async Task SuccessfulQuery_AnalyticsDisabled_NotCapturedByAnalytics()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var request = new DataSetQueryRequest
            {
                Page = 2,
                PageSize = 3,
                Indicators = ListOf(AbsenceSchoolData.IndicatorEnrolments),
                Criteria = new DataSetQueryCriteriaFacets
                {
                    Filters = new DataSetQueryCriteriaFilters
                    {
                        Eq = AbsenceSchoolData.FilterSchoolTypeTotal
                    },
                    GeographicLevels = new DataSetQueryCriteriaGeographicLevels
                    {
                        Eq = "NAT"
                    },
                    TimePeriods = new DataSetQueryCriteriaTimePeriods
                    {
                        Eq = new DataSetQueryTimePeriod
                        {
                            Code = "AY",
                            Period = "2020/2021"
                        }
                    },
                    Locations = new DataSetQueryCriteriaLocations
                    {
                        Eq = new DataSetQueryLocationId
                        {
                            Id = AbsenceSchoolData.LocationNatEngland,
                            Level = "NAT"
                        }
                    }
                },
                Sorts = ListOf(new DataSetQuerySort
                {
                    Direction = "Asc",
                    Field = "timePeriod"
                }),
                Debug = true
            };

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                request: request);
            
            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            
            // There are 4 results for the query above, but we are requesting page 2 and a page size of 3,
            // and so this 2nd page only displays the final single result of the 4.
            Assert.Single(viewModel.Results);
            
            // Add a slight delay as the writing of the query details for analytics is non-blocking
            // and could occur slightly after the query result is returned to the user.
            Thread.Sleep(2000);

            var analyticsPath = _analyticsPathResolver.PublicApiQueriesDirectoryPath();
            
            // Expect the successful query not to have recorded its query for analytics, as this
            // feature was not enabled via appsettings.
            Assert.False(Directory.Exists(analyticsPath));
        }
    }
    
    private async Task<HttpResponseMessage> QueryDataSet(
        Guid dataSetId,
        DataSetQueryRequest request,
        string? dataSetVersion = null,
        Guid? previewTokenId = null,
        WebApplicationFactory<Startup>? app = null,
        ClaimsPrincipal? user = null)
    {
        var query = new Dictionary<string, StringValues>();

        if (dataSetVersion is not null)
        {
            query["dataSetVersion"] = dataSetVersion;
        }

        var client = (app ?? BuildApp())
            .WithUser(user)
            .CreateClient();
        
        client.AddPreviewTokenHeader(previewTokenId);

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
        return TestApp
            .ConfigureServices(services => services
                .ReplaceService<IDataSetVersionPathResolver>(_dataSetVersionPathResolver)
                .ReplaceService<IAnalyticsPathResolver>(_analyticsPathResolver, optional: true));
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
