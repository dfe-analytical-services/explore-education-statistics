using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Analytics;
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

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class DataSetsControllerGetQueryTests(TestApplicationFactory testApp) : IntegrationTestFixtureWithCommonTestDataSetup(testApp)
{
    private const string BaseUrl = "v1/data-sets";

    private readonly TestDataSetVersionPathResolver _dataSetVersionPathResolver = new()
    {
        Directory = "AbsenceSchool"
    };

    private readonly TestAnalyticsPathResolver _analyticsPathResolver = new();

    public class AccessTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [MemberData(nameof(DataSetVersionStatusQueryTheoryData.UnavailableStatuses),
            MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
        public async Task VersionNotAvailable_Returns403(DataSetVersionStatus versionStatus)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion(versionStatus);

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]);

            response.AssertNotFound();
        }
        
        [Fact]
        public async Task WildCardSpecified_RequestsPublishedVersion_Returns200()
        {
            var (dataSet, versions) = await SetupDataSetWithSpecifiedVersionStatuses(DataSetVersionStatus.Published);
            
            var response = await QueryDataSet(
                dataSetId: dataSet.Id,
                dataSetVersion: "2.*",
                previewTokenId: versions.Last().PreviewTokens[0].Id,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]);

            response.AssertOk();
            
            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            Assert.Equal(1, viewModel.Paging.Page);
            Assert.Equal(1, viewModel.Paging.TotalPages);
            Assert.Equal(216, viewModel.Paging.TotalResults);

            Assert.Empty(viewModel.Warnings);

            Assert.Equal(216, viewModel.Results.Count);
        }
    }

    public class PreviewTokenTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]);

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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]);

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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]);

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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]);

            response.AssertForbidden();
        }
    }

    public class IndicatorValidationTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Fact]
        public async Task Empty_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var client = BuildApp().CreateClient();

            var response = await client.GetAsync($"{BaseUrl}/{dataSetVersion.DataSetId}/query?indicators[0]=");

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError("indicators[0]");
        }

        [Fact]
        public async Task Blank_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: ["", " ", "  "]
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
                indicators: [new string('a', 41), new string('a', 42)]
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

            var response = await client.GetAsync($"{BaseUrl}/{dataSetVersion.DataSetId}/query");

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasRequiredValueError("indicators");
        }

        [Fact]
        public async Task NotFound_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] notFoundIndicators = ["invalid1", "invalid2", "invalid3"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: notFoundIndicators
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasIndicatorsNotFoundError("indicators", notFoundIndicators);
        }
    }

    public class FiltersValidationTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [InlineData("filters.in[0]")]
        [InlineData("filters.notIn[0]")]
        public async Task Empty_Returns400(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, ""
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError(path);
        }

        [Theory]
        [InlineData("filters.in")]
        [InlineData("filters.notIn")]
        public async Task InvalidMix_Returns400(string path)
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, invalidFilters }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(5, validationProblem.Errors.Count);

            validationProblem.AssertHasNotEmptyError($"{path}[0]");
            validationProblem.AssertHasNotEmptyError($"{path}[1]");
            validationProblem.AssertHasNotEmptyError($"{path}[2]");
            validationProblem.AssertHasMaximumLengthError($"{path}[3]", maxLength: 10);
            validationProblem.AssertHasMaximumLengthError($"{path}[4]", maxLength: 10);
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        "filters.eq", new string('a', 11)
                    },
                    {
                        "filters.notEq", new string('a', 12)
                    },
                    {
                        "filters.in[0]", ""
                    },
                    {
                        "filters.notIn", invalidFilters
                    },
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(5, validationProblem.Errors.Count);

            validationProblem.AssertHasMaximumLengthError("filters.eq", maxLength: 10);
            validationProblem.AssertHasMaximumLengthError("filters.notEq", maxLength: 10);
            validationProblem.AssertHasNotEmptyError("filters.in[0]");
            validationProblem.AssertHasMaximumLengthError("filters.notIn[0]", maxLength: 10);
            validationProblem.AssertHasNotEmptyError("filters.notIn[1]");
        }

        [Theory]
        [InlineData("filters.in")]
        [InlineData("filters.notIn")]
        public async Task NotFound_Returns200_HasWarning(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] notFoundFilters =
            [
                "invalid",
                "9999999"
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, new StringValues(["IzBzg", ..notFoundFilters])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Single(viewModel.Warnings);

            viewModel.AssertHasFiltersNotFoundWarning(path, notFoundFilters);
        }
    }

    public class GeographicLevelsValidationTests(TestApplicationFactory testApp)
        : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [InlineData("geographicLevels.in[0]")]
        [InlineData("geographicLevels.notIn[0]")]
        public async Task Empty_Returns400(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, ""
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasAllowedValueError(
                expectedPath: path,
                value: null,
                allowed: GeographicLevelUtils.OrderedCodes
            );
        }

        [Theory]
        [InlineData("geographicLevels.in")]
        [InlineData("geographicLevels.notIn")]
        public async Task InvalidMix_Returns400(string path)
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, invalidLevels
                    }
                }
            );

            var allowed = GeographicLevelUtils.OrderedCodes;

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(7, validationProblem.Errors.Count);

            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[0]", value: null, allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[1]", value: null, allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[2]", value: invalidLevels[2], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[3]", value: invalidLevels[3], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[4]", value: invalidLevels[4], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[5]", value: invalidLevels[5], allowed);
            validationProblem.AssertHasAllowedValueError(expectedPath: $"{path}[6]", value: invalidLevels[6], allowed);
        }

        [Theory]
        [InlineData("geographicLevels.in")]
        [InlineData("geographicLevels.notIn")]
        public async Task NotFound_Returns200_HasWarning(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] notFoundGeographicLevels =
            [
                GeographicLevel.Ward.GetEnumValue(),
                GeographicLevel.OpportunityArea.GetEnumValue(),
                GeographicLevel.PlanningArea.GetEnumValue(),
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, new StringValues(
                            [
                                "LA",
                                ..notFoundGeographicLevels
                            ]
                        )
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Single(viewModel.Warnings);

            viewModel.AssertHasGeographicLevelsNotFoundWarning(path, notFoundGeographicLevels);
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        "geographicLevels.eq", "NATT"
                    },
                    {
                        "geographicLevels.notEq", "LADD"
                    },
                    {
                        "geographicLevels.in", invalidLevels
                    },
                    {
                        "geographicLevels.notIn[0]", ""
                    },
                }
            );

            var validationProblem = response.AssertValidationProblem();

            var allowed = GeographicLevelUtils.OrderedCodes;

            Assert.Equal(5, validationProblem.Errors.Count);

            validationProblem.AssertHasAllowedValueError(
                expectedPath: "geographicLevels.eq",
                value: "NATT",
                allowed: allowed
            );
            validationProblem.AssertHasAllowedValueError(
                expectedPath: "geographicLevels.notEq",
                value: "LADD",
                allowed: allowed
            );
            validationProblem.AssertHasAllowedValueError(
                expectedPath: "geographicLevels.in[0]",
                value: null,
                allowed: allowed
            );
            validationProblem.AssertHasAllowedValueError(
                expectedPath: "geographicLevels.in[1]",
                value: invalidLevels[1],
                allowed: allowed
            );
            validationProblem.AssertHasAllowedValueError(
                expectedPath: "geographicLevels.notIn[0]",
                value: null,
                allowed: allowed
            );
        }
    }

    public class LocationsValidationTests(TestApplicationFactory testApp)
        : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [InlineData("locations.in[0]")]
        [InlineData("locations.notIn[0]")]
        public async Task Empty_Returns400(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, ""
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError(path);
        }

        [Theory]
        [InlineData("locations.in")]
        [InlineData("locations.notIn")]
        public async Task InvalidMix_Returns400(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, new StringValues(
                            [
                                "",
                                "invalid",
                                "||",
                                "LADD|code|12345",
                                "NATT|code|12345",
                                "NAT|invalid|12345",
                                "LA|urn|12345",
                                "SCH|code|12345",
                                "PROV|oldCode|12345",
                                "RSC|code|12345",
                                "NAT|id| ",
                                "LA|code| ",
                                $"NAT|id|{new string('a', 11)}",
                                $"LA|code|{new string('a', 31)}",
                                $"LA|oldCode|{new string('a', 21)}",
                                $"SCH|urn|{new string('a', 21)}",
                                $"PROV|ukprn|{new string('a', 21)}",
                                $"RSC|id|{new string('a', 11)}",
                            ]
                        )
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(18, validationProblem.Errors.Count);

            validationProblem.AssertHasNotEmptyError(expectedPath: $"{path}[0]");
            validationProblem.AssertHasLocationFormatError(expectedPath: $"{path}[1]", value: "invalid");
            validationProblem.AssertHasLocationFormatError(expectedPath: $"{path}[2]", value: "||");

            validationProblem.AssertHasLocationAllowedLevelError(expectedPath: $"{path}[3]", level: "LADD");
            validationProblem.AssertHasLocationAllowedLevelError(expectedPath: $"{path}[4]", level: "NATT");

            validationProblem.AssertHasLocationAllowedPropertyError(
                expectedPath: $"{path}[5]",
                property: "invalid",
                allowedProperties: ["id", "code"]
            );
            validationProblem.AssertHasLocationAllowedPropertyError(
                expectedPath: $"{path}[6]",
                property: "urn",
                allowedProperties: ["id", "code", "oldCode"]
            );
            validationProblem.AssertHasLocationAllowedPropertyError(
                expectedPath: $"{path}[7]",
                property: "code",
                allowedProperties: ["id", "urn", "laEstab"]
            );
            validationProblem.AssertHasLocationAllowedPropertyError(
                expectedPath: $"{path}[8]",
                property: "oldCode",
                allowedProperties: ["id", "ukprn"]
            );
            validationProblem.AssertHasLocationAllowedPropertyError(
                expectedPath: $"{path}[9]",
                property: "code",
                allowedProperties: ["id"]
            );
            validationProblem.AssertHasLocationValueNotEmptyError(expectedPath: $"{path}[10]", property: "id");
            validationProblem.AssertHasLocationValueNotEmptyError(expectedPath: $"{path}[11]", property: "code");

            validationProblem.AssertHasLocationValueMaxLengthError(
                expectedPath: $"{path}[12]",
                property: "id",
                maxLength: 10
            );
            validationProblem.AssertHasLocationValueMaxLengthError(
                expectedPath: $"{path}[13]",
                property: "code",
                maxLength: 30
            );
            validationProblem.AssertHasLocationValueMaxLengthError(
                expectedPath: $"{path}[14]",
                property: "oldCode",
                maxLength: 20
            );
            validationProblem.AssertHasLocationValueMaxLengthError(
                expectedPath: $"{path}[15]",
                property: "urn",
                maxLength: 20
            );
            validationProblem.AssertHasLocationValueMaxLengthError(
                expectedPath: $"{path}[16]",
                property: "ukprn",
                maxLength: 20
            );
            validationProblem.AssertHasLocationValueMaxLengthError(
                expectedPath: $"{path}[17]",
                property: "id",
                maxLength: 10
            );
        }

        [Fact]
        public async Task AllComparatorsInvalid_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] invalidLocations =
            [
                "",
                "||",
                "NAT|id| ",
                $"NAT|id|{new string('a', 11)}",
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        "locations.eq", "LADD|code|12345"
                    },
                    {
                        "locations.notEq", "LA|urn|12345"
                    },
                    {
                        "locations.in", invalidLocations
                    },
                    {
                        "locations.notIn[0]", ""
                    },
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(7, validationProblem.Errors.Count);

            validationProblem.AssertHasLocationAllowedLevelError(expectedPath: "locations.eq", level: "LADD");
            validationProblem.AssertHasLocationAllowedPropertyError(
                expectedPath: "locations.notEq",
                property: "urn",
                allowedProperties: ["id", "code", "oldCode"]
            );
            validationProblem.AssertHasNotEmptyError(expectedPath: "locations.in[0]");
            validationProblem.AssertHasLocationFormatError(expectedPath: "locations.in[1]", value: "||");
            validationProblem.AssertHasLocationValueNotEmptyError(
                expectedPath: "locations.in[2]",
                property: "id"
            );
            validationProblem.AssertHasLocationValueMaxLengthError(
                expectedPath: "locations.in[3]",
                property: "id",
                maxLength: 10
            );
            validationProblem.AssertHasNotEmptyError("locations.notIn[0]");
        }

        [Theory]
        [InlineData("locations.in")]
        [InlineData("locations.notIn")]
        public async Task NotFound_Returns200_HasWarning(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] notFoundLocations =
            [
                "NAT|id|11111111",
                "NAT|code|11111111",
                "REG|id|22222222",
                "LA|id|33333333",
                "LA|code|4444444",
                "LA|oldCode|999",
                "SCH|id|55555555",
                "SCH|urn|666666",
                "SCH|laEstab|7777777",
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, new StringValues(["LA|code|E08000016", ..notFoundLocations])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Single(viewModel.Warnings);

            viewModel.AssertHasLocationsNotFoundWarning(path, notFoundLocations);
        }
    }

    public class TimePeriodsValidationTests(TestApplicationFactory testApp)
        : DataSetsControllerGetQueryTests(testApp)
    {
        [Fact]
        public async Task Empty_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        "timePeriods.in[0]", ""
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError("timePeriods.in[0]");
        }

        [Theory]
        [InlineData("timePeriods.in")]
        [InlineData("timePeriods.notIn")]
        public async Task InvalidMix_Returns400(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, new StringValues(
                            [
                                "",
                                "invalid",
                                "|",
                                "2020/2019|AY",
                                "2020/2022|AY",
                                "2020|INVALID",
                                "2020/2021|CY",
                                "2020/2021|CYQ2",
                                "2020/2021|RY",
                                "2020/2021|W10",
                                "2020/2021|M5",
                            ]
                        )
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(11, validationProblem.Errors.Count);

            validationProblem.AssertHasNotEmptyError($"{path}[0]");
            validationProblem.AssertHasTimePeriodFormatError(expectedPath: $"{path}[1]", value: "invalid");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[2]", code: "");

            validationProblem.AssertHasTimePeriodYearRangeError(expectedPath: $"{path}[3]", period: "2020/2019");
            validationProblem.AssertHasTimePeriodYearRangeError(expectedPath: $"{path}[4]", period: "2020/2022");

            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[5]", code: "INVALID");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[6]", code: "CY");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[7]", code: "CYQ2");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[8]", code: "RY");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[9]", code: "W10");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: $"{path}[10]", code: "M5");
        }

        [Fact]
        public async Task AllComparatorsInvalid_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] invalidTimePeriods =
            [
                "",
                "invalid",
                "|"
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        "timePeriods.eq", "2020/2019|AY"
                    },
                    {
                        "timePeriods.notEq", "2020/2021|W10"
                    },
                    {
                        "timePeriods.in", invalidTimePeriods
                    },
                    {
                        "timePeriods.notIn[0]", ""
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(6, validationProblem.Errors.Count);

            validationProblem.AssertHasTimePeriodYearRangeError("timePeriods.eq", period: "2020/2019");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: "timePeriods.notEq", code: "W10");
            validationProblem.AssertHasNotEmptyError(expectedPath: "timePeriods.in[0]");
            validationProblem.AssertHasTimePeriodFormatError(expectedPath: "timePeriods.in[1]", value: "invalid");
            validationProblem.AssertHasTimePeriodAllowedCodeError(expectedPath: "timePeriods.in[2]", code: "");
            validationProblem.AssertHasNotEmptyError(expectedPath: "timePeriods.notIn[0]");
        }

        [Theory]
        [InlineData("timePeriods.in")]
        [InlineData("timePeriods.notIn")]
        public async Task NotFound_Returns200_HasWarning(string path)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] notFoundTimePeriods =
            [
                "2021|CY",
                "2022|CY",
                "2030|CY",
                "2023/2024|AY",
                "2018/2019|AY",
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        path, new StringValues(["2020/2021|AY", ..notFoundTimePeriods])
                    }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Single(viewModel.Warnings);

            viewModel.AssertHasTimePeriodsNotFoundWarning(path, notFoundTimePeriods);
        }
    }

    public class SortsValidationTests(TestApplicationFactory testApp)
        : DataSetsControllerGetQueryTests(testApp)
    {
        [Fact]
        public async Task Empty_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        "sorts[0]", ""
                    }
                }
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasNotEmptyError("sorts[0]");
        }

        [Fact]
        public async Task InvalidMix_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                sorts:
                [
                    "",
                    "invalid",
                    "|",
                    "test|",
                    "test|invalid",
                    "test|asc",
                    "test|desc",
                    $"{new string('a', 41)}|Asc",
                    $"{new string('b', 41)}|Desc",
                    "missing1|Asc",
                    "missing2|Desc",
                ]
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Equal(10, validationProblem.Errors.Count);

            validationProblem.AssertHasNotEmptyError(expectedPath: "sorts[0]");
            validationProblem.AssertHasSortFormatError(expectedPath: "sorts[1]", value: "invalid");

            validationProblem.AssertHasSortFieldNotEmptyError(expectedPath: "sorts[2]");

            validationProblem.AssertHasSortDirectionError(expectedPath: "sorts[2]", direction: "");
            validationProblem.AssertHasSortDirectionError(expectedPath: "sorts[3]", direction: "");
            validationProblem.AssertHasSortDirectionError(expectedPath: "sorts[4]", direction: "invalid");
            validationProblem.AssertHasSortDirectionError(expectedPath: "sorts[5]", direction: "asc");
            validationProblem.AssertHasSortDirectionError(expectedPath: "sorts[6]", direction: "desc");

            validationProblem.AssertHasSortFieldMaxLengthError(
                expectedPath: "sorts[7]",
                field: new string('a', 41)
            );
            validationProblem.AssertHasSortFieldMaxLengthError(
                expectedPath: "sorts[8]",
                field: new string('b', 41)
            );
        }

        [Fact]
        public async Task FieldsNotFound_Returns400()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] notFoundSorts =
            [
                "invalid1|Asc",
                "invalid2|Desc",
                "filter|Asc",
                "filter|invalid|Asc",
                "location|Asc",
                "location|invalid|Asc",
                "indicator|Asc",
                "indicator|invalid|Asc",
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                sorts: ["timePeriod|Asc", ..notFoundSorts]
            );

            var validationProblem = response.AssertValidationProblem();

            Assert.Single(validationProblem.Errors);

            validationProblem.AssertHasSortFieldsNotFoundError("sorts", notFoundSorts);
        }
    }

    public class PaginationTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task PageTooSmall_Returns400(int page)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                page: page
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                pageSize: pageSize
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                page: page,
                pageSize: pageSize
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(pageResults, viewModel.Results.Count);
            Assert.Equal(page, viewModel.Paging.Page);
            Assert.Equal(pageSize, viewModel.Paging.PageSize);
            Assert.Equal(totalPages, viewModel.Paging.TotalPages);
            Assert.Equal(216, viewModel.Paging.TotalResults);
        }
    }

    public class FiltersQueryTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [InlineData("filters.eq", 54)]
        [InlineData("filters.notEq", 162)]
        [InlineData("filters.in", 54)]
        [InlineData("filters.notIn", 162)]
        public async Task SingleOption_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, AbsenceSchoolData.FilterNcYear4 }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (path)
            {
                case "filters.eq":
                case "filters.in":
                    Assert.Single(meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    Assert.Contains(AbsenceSchoolData.FilterNcYear4, meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    break;
                case "filters.notEq":
                case "filters.notIn":
                    Assert.Equal(3, meta.Filters[AbsenceSchoolData.FilterNcYear].Count);
                    Assert.DoesNotContain(AbsenceSchoolData.FilterNcYear4, meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    break;
            }
        }

        [Theory]
        [InlineData("filters.in", 108)]
        [InlineData("filters.notIn", 108)]
        public async Task MultipleOptionsInSameFilter_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] filterOptionIds = [AbsenceSchoolData.FilterNcYear4, AbsenceSchoolData.FilterNcYear8];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, filterOptionIds }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(2, meta.Indicators.Count);
            Assert.Contains(AbsenceSchoolData.IndicatorEnrolments, meta.Indicators);
            Assert.Contains(AbsenceSchoolData.IndicatorSessAuthorised, meta.Indicators);

            switch (path)
            {
                case "filters.in":
                    Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterNcYear].Count);
                    Assert.Contains(filterOptionIds[0], meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    Assert.Contains(filterOptionIds[1], meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    break;
                case "filters.notIn":
                    Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterNcYear].Count);
                    Assert.DoesNotContain(filterOptionIds[0], meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    Assert.DoesNotContain(filterOptionIds[1], meta.Filters[AbsenceSchoolData.FilterNcYear]);
                    break;
            }
        }

        [Fact]
        public async Task CommaSeparatedOptionsInSameFilter_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] filterOptionIds = [AbsenceSchoolData.FilterNcYear4, AbsenceSchoolData.FilterNcYear8];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.in", filterOptionIds.JoinToString(',') }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(108, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterNcYear].Count);
            Assert.Contains(filterOptionIds[0], meta.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Contains(filterOptionIds[1], meta.Filters[AbsenceSchoolData.FilterNcYear]);
        }

        [Theory]
        [InlineData("filters.in", 150)]
        [InlineData("filters.notIn", 66)]
        public async Task MultipleOptionsInDifferentFilters_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] filterOptionIds =
            [
                AbsenceSchoolData.FilterSchoolTypeTotal,
                AbsenceSchoolData.FilterSchoolTypeSecondary,
                AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, filterOptionIds }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (path)
            {
                case "filters.in":
                    Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterSchoolType].Count);
                    Assert.Contains(filterOptionIds[0], meta.Filters[AbsenceSchoolData.FilterSchoolType]);
                    Assert.Contains(filterOptionIds[1], meta.Filters[AbsenceSchoolData.FilterSchoolType]);

                    Assert.Equal(2, meta.Filters[AbsenceSchoolData.FilterAcademyType].Count);
                    Assert.Contains(filterOptionIds[2], meta.Filters[AbsenceSchoolData.FilterAcademyType]);
                    Assert.Contains(filterOptionIds[3], meta.Filters[AbsenceSchoolData.FilterAcademyType]);
                    break;
                case "filters.notIn":
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

    public class GeographicLevelsQueryTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [InlineData("geographicLevels.eq", 132)]
        [InlineData("geographicLevels.notEq", 84)]
        [InlineData("geographicLevels.in", 132)]
        [InlineData("geographicLevels.notIn", 84)]
        public async Task SingleOption_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            const GeographicLevel geographicLevel = GeographicLevel.LocalAuthority;

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, geographicLevel.GetEnumValue() }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (path)
            {
                case "geographicLevels.eq":
                case "geographicLevels.in":
                    Assert.Single(meta.GeographicLevels);
                    Assert.Contains(geographicLevel, meta.GeographicLevels);
                    break;
                case "geographicLevels.notEq":
                case "geographicLevels.notIn":
                    Assert.Equal(3, meta.GeographicLevels.Count);
                    Assert.DoesNotContain(geographicLevel, meta.GeographicLevels);
                    break;
            }
        }

        [Theory]
        [InlineData("geographicLevels.in", 180)]
        [InlineData("geographicLevels.notIn", 36)]
        public async Task MultipleOptions_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            GeographicLevel[] geographicLevels = [GeographicLevel.Region, GeographicLevel.LocalAuthority];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, geographicLevels.Select(l => l.GetEnumValue()).ToArray() }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (path)
            {
                case "geographicLevels.in":
                    Assert.Equal(2, meta.GeographicLevels.Count);
                    Assert.Contains(geographicLevels[0], meta.GeographicLevels);
                    Assert.Contains(geographicLevels[1], meta.GeographicLevels);
                    break;
                case "geographicLevels.notIn":
                    Assert.Equal(2, meta.GeographicLevels.Count);
                    Assert.DoesNotContain(geographicLevels[0], meta.GeographicLevels);
                    Assert.DoesNotContain(geographicLevels[1], meta.GeographicLevels);
                    break;
            }
        }

        [Fact]
        public async Task CommaSeparatedOptions_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            GeographicLevel[] geographicLevels = [GeographicLevel.Region, GeographicLevel.LocalAuthority];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "geographicLevels.in", geographicLevels.Select(l => l.GetEnumValue()).JoinToString(',') }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(180, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(2, meta.GeographicLevels.Count);
            Assert.Contains(geographicLevels[0], meta.GeographicLevels);
            Assert.Contains(geographicLevels[1], meta.GeographicLevels);
        }
    }

    public class LocationsQueryTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [InlineData("locations.eq", 36)]
        [InlineData("locations.notEq", 180)]
        [InlineData("locations.in", 36)]
        [InlineData("locations.notIn", 180)]
        public async Task SingleOption_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Sheffield
            const string locationStrings = "LA|code|E08000019";

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, locationStrings }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (path)
            {
                case "locations.eq":
                case "locations.in":
                    Assert.Single(meta.Locations["LA"]);
                    Assert.Contains(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    break;
                case "locations.notEq":
                case "locations.notIn":
                    Assert.Equal(3, meta.Locations["LA"].Count);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    break;
            }
        }

        [Theory]
        [InlineData("locations.in", 72)]
        [InlineData("locations.notIn", 144)]
        public async Task MultipleOptionsInSameLevel_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Sheffield and Barnsley
            string[] locationStrings = ["LA|code|E08000019", $"LA|id|{AbsenceSchoolData.LocationLaBarnsley}"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, locationStrings }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (path)
            {
                case "locations.in":
                    Assert.Equal(2, meta.Locations["LA"].Count);
                    Assert.Contains(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    Assert.Contains(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);
                    break;
                case "locations.notIn":
                    Assert.Equal(2, meta.Locations["LA"].Count);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    Assert.DoesNotContain(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);
                    break;
            }
        }

        [Fact]
        public async Task CommaSeparatedOptionsInSameLevel_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Sheffield and Barnsley
            string[] locationStrings = ["LA|code|E08000019", $"LA|id|{AbsenceSchoolData.LocationLaBarnsley}"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "locations.in", locationStrings.JoinToString(',') }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(72, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            Assert.Equal(2, meta.Locations["LA"].Count);
            Assert.Contains(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
            Assert.Contains(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);
        }

        [Theory]
        [InlineData("locations.in", 84)]
        [InlineData("locations.notIn", 132)]
        public async Task MultipleOptionsInDifferentLevels_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            // Sheffield and Barnsley
            // THe Kingston Academy and King Athelstan Primary School
            string[] locationStrings =
            [
                "LA|code|E08000019",
                $"LA|id|{AbsenceSchoolData.LocationLaBarnsley}",
                "SCH|laEstab|3144001",
                "SCH|urn|102579"
            ];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, locationStrings }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(expectedResults, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            switch (path)
            {
                case "locations.in":
                    Assert.Equal(3, meta.Locations["LA"].Count);
                    Assert.Contains(AbsenceSchoolData.LocationLaSheffield, meta.Locations["LA"]);
                    Assert.Contains(AbsenceSchoolData.LocationLaBarnsley, meta.Locations["LA"]);

                    Assert.Equal(6, meta.Locations["SCH"].Count);
                    Assert.Contains(AbsenceSchoolData.LocationSchoolKingstonAcademy, meta.Locations["SCH"]);
                    Assert.Contains(AbsenceSchoolData.LocationSchoolKingAthelstanPrimary, meta.Locations["SCH"]);
                    break;
                case "locations.notIn":
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

    public class TimePeriodsQueryTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Theory]
        [InlineData("timePeriods.eq", 72)]
        [InlineData("timePeriods.notEq", 144)]
        [InlineData("timePeriods.in", 72)]
        [InlineData("timePeriods.notIn", 144)]
        [InlineData("timePeriods.gt", 72)]
        [InlineData("timePeriods.gte", 144)]
        [InlineData("timePeriods.lt", 72)]
        [InlineData("timePeriods.lte", 144)]
        public async Task SingleOption_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            const string timePeriodString = "2021/2022|AY";

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, timePeriodString }
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

            switch (path)
            {
                case "timePeriods.eq":
                case "timePeriods.in":
                    Assert.Single(meta.TimePeriods);
                    Assert.Contains(timePeriod, meta.TimePeriods);
                    break;
                case "timePeriods.notEq":
                case "timePeriods.notIn":
                    Assert.Equal(2, meta.TimePeriods.Count);
                    Assert.DoesNotContain(timePeriod, meta.TimePeriods);
                    break;
                case "timePeriods.lt":
                case "timePeriods.gt":
                    Assert.Single(meta.TimePeriods);
                    Assert.DoesNotContain(timePeriod, meta.TimePeriods);
                    break;
                case "timePeriods.lte":
                case "timePeriods.gte":
                    Assert.Equal(2, meta.TimePeriods.Count);
                    Assert.Contains(timePeriod, meta.TimePeriods);
                    break;
            }
        }

        [Theory]
        [InlineData("timePeriods.in", 144)]
        [InlineData("timePeriods.notIn", 72)]
        public async Task MultipleOptions_Returns200(string path, int expectedResults)
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] timePeriodStrings = ["2021|AY", "2022/2023|AY"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { path, timePeriodStrings }
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

            switch (path)
            {
                case "timePeriods.in":
                    Assert.Equal(2, meta.TimePeriods.Count);
                    Assert.Contains(timePeriods[0], meta.TimePeriods);
                    Assert.Contains(timePeriods[1], meta.TimePeriods);
                    break;
                case "timePeriods.notIn":
                    Assert.Single(meta.TimePeriods);
                    Assert.DoesNotContain(timePeriods[0], meta.TimePeriods);
                    Assert.DoesNotContain(timePeriods[1], meta.TimePeriods);
                    break;
            }
        }

        [Fact]
        public async Task CommaSeparatedOptions_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            string[] timePeriodStrings = ["2021|AY", "2022/2023|AY"];

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "timePeriods.in", timePeriodStrings }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            Assert.Equal(144, viewModel.Results.Count);

            var meta = GatherQueryResultsMeta(viewModel);

            TimePeriodViewModel[] timePeriods =
            [
                new() { Code = TimeIdentifier.AcademicYear, Period = "2021/2022" },
                new() { Code = TimeIdentifier.AcademicYear, Period = "2022/2023" }
            ];

            Assert.Equal(2, meta.TimePeriods.Count);
            Assert.Contains(timePeriods[0], meta.TimePeriods);
            Assert.Contains(timePeriods[1], meta.TimePeriods);
        }
    }

    public class ResultsTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Fact]
        public async Task NoResults_Returns200_HasWarning()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    {
                        "locations.eq", $"LA|id|{AbsenceSchoolData.LocationLaBarnsley}"
                    },
                    {
                        "geographicLevels.eq", "NAT"
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised],
                debug: true
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
                indicators: [AbsenceSchoolData.IndicatorSessAuthorised]
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
                indicators:
                [
                    AbsenceSchoolData.IndicatorEnrolments,
                    AbsenceSchoolData.IndicatorSessAuthorised,
                    AbsenceSchoolData.IndicatorSessPossible,
                    AbsenceSchoolData.IndicatorSessUnauthorised,
                    AbsenceSchoolData.IndicatorSessUnauthorisedPercent,
                ]
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

            var sessUnauthorisedPercent = values[AbsenceSchoolData.IndicatorSessUnauthorisedPercent]
                .Select(float.Parse)
                .ToList();

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
                indicators:
                [
                    AbsenceSchoolData.IndicatorEnrolments,
                    AbsenceSchoolData.IndicatorSessAuthorised,
                    AbsenceSchoolData.IndicatorSessPossible,
                    AbsenceSchoolData.IndicatorSessUnauthorised,
                    AbsenceSchoolData.IndicatorSessUnauthorisedPercent,
                ]
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
                indicators:
                [
                    AbsenceSchoolData.IndicatorEnrolments,
                    AbsenceSchoolData.IndicatorSessAuthorised,
                    AbsenceSchoolData.IndicatorSessPossible,
                    AbsenceSchoolData.IndicatorSessUnauthorised,
                    AbsenceSchoolData.IndicatorSessUnauthorisedPercent,
                ],
                debug: true
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
                meta.Locations["REG"]);
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
        public async Task AllFacets_MixOfConditions_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments, AbsenceSchoolData.IndicatorSessAuthorised],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.notEq", AbsenceSchoolData.FilterNcYear8 },
                    {
                        "filters.in",
                        new StringValues(
                        [
                            AbsenceSchoolData.FilterAcademyTypeSecondaryFreeSchool,
                            AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed
                        ])
                    },
                    {
                        "geographicLevels.notEq", "NAT"
                    },
                    { "locations.eq", $"NAT|id|{AbsenceSchoolData.LocationNatEngland}" },
                    // Outer London, Barnsley
                    { "locations.notIn", new StringValues(["REG|code|E13000002", "LA|oldCode|370"]) },
                    { "timePeriods.gt", "2020/2021|AY" },
                    { "timePeriods.lt", "2022/2023|AY" }
                }
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            var result = Assert.Single(viewModel.Results);

            Assert.Equal(3, result.Filters.Count);
            Assert.Equal(AbsenceSchoolData.FilterNcYear10, result.Filters[AbsenceSchoolData.FilterNcYear]);
            Assert.Equal(AbsenceSchoolData.FilterSchoolTypeSecondary, result.Filters[AbsenceSchoolData.FilterSchoolType]);
            Assert.Equal(AbsenceSchoolData.FilterAcademyTypeSecondarySponsorLed, result.Filters[AbsenceSchoolData.FilterAcademyType]);

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

    public class SortsTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Fact]
        public async Task NoFields_SingleTimePeriod_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypeTotal },
                    { "geographicLevels.eq", "NAT" },
                    { "timePeriods.eq", "2020/2021|AY" },
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypePrimary },
                    { "geographicLevels.eq", "NAT" }
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
        public async Task SingleField_TimePeriodAsc_Returns200()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: ["timePeriod|Asc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypePrimary },
                    { "geographicLevels.eq", "NAT" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: ["timePeriod|Desc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypePrimary },
                    { "geographicLevels.eq", "NAT" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: ["geographicLevel|Asc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterNcYear4 },
                    { "locations.eq", $"NAT|id|{AbsenceSchoolData.LocationNatEngland}" },
                    { "timePeriods.eq", "2020|AY" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: ["geographicLevel|Desc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterNcYear4 },
                    { "locations.eq", $"NAT|id|{AbsenceSchoolData.LocationNatEngland}" },
                    { "timePeriods.eq", "2020|AY" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: ["location|LA|Asc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterNcYear4 },
                    { "geographicLevels.eq", "LA" },
                    { "timePeriods.eq", "2020|AY" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: ["location|LA|Desc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterNcYear4 },
                    { "geographicLevels.eq", "LA" },
                    { "timePeriods.eq", "2020|AY" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: [$"filter|{AbsenceSchoolData.FilterNcYear}|Asc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "geographicLevels.eq", "NAT" },
                    { "timePeriods.eq", "2020|AY" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: [$"filter|{AbsenceSchoolData.FilterNcYear}|Desc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "geographicLevels.eq", "NAT" },
                    { "timePeriods.eq", "2020|AY" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: [$"indicator|{AbsenceSchoolData.IndicatorEnrolments}|Asc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterNcYear4 },
                    { "geographicLevels.eq", "REG" },
                    { "timePeriods.eq", "2020|AY" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts: [$"indicator|{AbsenceSchoolData.IndicatorEnrolments}|Desc"],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterNcYear4 },
                    { "geographicLevels.eq", "REG" },
                    { "timePeriods.eq", "2020|AY" }
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
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                sorts:
                [
                    "timePeriod|Asc",
                    "location|LA|Desc",
                    $"filter|{AbsenceSchoolData.FilterNcYear}|Asc"
                ],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "geographicLevels.eq", "LA" },
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypeTotal }
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

    public class QueryAnalyticsEnabledTests : DataSetsControllerGetQueryTests, IDisposable
    {
        public QueryAnalyticsEnabledTests(TestApplicationFactory testApp) : base(testApp)
        {
            testApp.AddAppSettings("appsettings.AnalyticsEnabled.json");
        }

        public void Dispose()
        {
            var queriesDirectory = _analyticsPathResolver.PublicApiQueriesDirectoryPath();
            if (Directory.Exists(queriesDirectory))
            {
                Directory.Delete(queriesDirectory, recursive: true);
            }
        }

        [Fact]
        public async Task SuccessfulQuery_CapturedByAnalytics()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypeTotal },
                    { "geographicLevels.eq", "NAT" },
                    { "timePeriods.eq", "2020/2021|AY" },
                    { "locations.eq", $"NAT|id|{AbsenceSchoolData.LocationNatEngland}" }
                },
                sorts: ["timePeriod|Asc"],
                debug: true,
                page: 2,
                pageSize: 3
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            
            // There are 4 results for the query above, but we are requesting page 2 and a page size of 3,
            // and so this 2nd page only displays the final single result of the 4.
            Assert.Single(viewModel.Results);

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
            
            await AnalyticsTestAssertions.AssertDataSetVersionQueryAnalyticsCaptured(
                dataSetVersion: dataSetVersion,
                expectedAnalyticsPath: _analyticsPathResolver.PublicApiQueriesDirectoryPath(),
                expectedRequest: expectedRequest,
                expectedResultsCount: 1,
                expectedTotalRows: 4);
        }

        [Fact]
        public async Task UnsuccessfulQuery_NotCapturedByAnalytics()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", "NonExistent" },
                }
            );

            response.AssertBadRequest();
            
            // Check that the folder for capturing queries for analytics was never created.
            AnalyticsTestAssertions.AssertAnalyticsCallNotCaptured(
                _analyticsPathResolver.PublicApiQueriesDirectoryPath());
        }

        [Fact]
        public async Task RequestFromEes_NotCapturedByAnalytics()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypeTotal },
                    { "geographicLevels.eq", "NAT" },
                    { "timePeriods.eq", "2020/2021|AY" },
                    { "locations.eq", $"NAT|id|{AbsenceSchoolData.LocationNatEngland}" }
                },
                sorts: ["timePeriod|Asc"],
                debug: true,
                page: 2,
                pageSize: 3,
                requestSource: "EES");

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);

            // There are 4 results for the query above, but we are requesting page 2 and a page size of 3,
            // and so this 2nd page only displays the final single result of the 4.
            Assert.Single(viewModel.Results);
            
            // Expect the successful call to have been omitted from analytics because it originates
            // from the EES service.
            AnalyticsTestAssertions.AssertAnalyticsCallNotCaptured(
                _analyticsPathResolver.PublicApiQueriesDirectoryPath());
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

            var response = await QueryDataSet(
                app: app,
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypeTotal },
                    { "geographicLevels.eq", "NAT" },
                    { "timePeriods.eq", "2020/2021|AY" },
                    { "locations.eq", $"NAT|id|{AbsenceSchoolData.LocationNatEngland}" }
                },
                sorts: ["timePeriod|Asc"]
            );

            // Verify that the manager threw the Exception as planned.
            analyticsManagerMock
                .Verify(s => s.Add(
                    It.IsAny<CaptureDataSetVersionQueryRequest>(), 
                    It.IsAny<CancellationToken>()));

            // Verify that despite the Exception being thrown, the service still returned
            // the expected successful query.
            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            Assert.Equal(4, viewModel.Results.Count);
        }
    }

    public class QueryAnalyticsDisabledTests(TestApplicationFactory testApp) : DataSetsControllerGetQueryTests(testApp)
    {
        [Fact]
        public async Task SuccessfulQuery_AnalyticsDisabled_NotCapturedByAnalytics()
        {
            var dataSetVersion = await SetupDefaultDataSetVersion();

            var response = await QueryDataSet(
                dataSetId: dataSetVersion.DataSetId,
                indicators: [AbsenceSchoolData.IndicatorEnrolments],
                queryParameters: new Dictionary<string, StringValues>
                {
                    { "filters.eq", AbsenceSchoolData.FilterSchoolTypeTotal },
                    { "geographicLevels.eq", "NAT" },
                    { "timePeriods.eq", "2020/2021|AY" },
                    { "locations.eq", $"NAT|id|{AbsenceSchoolData.LocationNatEngland}" }
                },
                sorts: ["timePeriod|Asc"],
                debug: true,
                page: 2,
                pageSize: 3
            );

            var viewModel = response.AssertOk<DataSetQueryPaginatedResultsViewModel>(useSystemJson: true);
            
            // There are 4 results for the query above, but we are requesting page 2 and a page size of 3,
            // and so this 2nd page only displays the final single result of the 4.
            Assert.Single(viewModel.Results);
            
            // Expect the successful query not to have recorded its query for analytics, as this
            // feature was disabled by appsettings.
            AnalyticsTestAssertions.AssertAnalyticsCallNotCaptured(
                _analyticsPathResolver.PublicApiQueriesDirectoryPath());
        }
    }
        
    private async Task<HttpResponseMessage> QueryDataSet(
        Guid dataSetId,
        IEnumerable<string> indicators,
        string? dataSetVersion = null,
        int? page = null,
        int? pageSize = null,
        IEnumerable<string>? sorts = null,
        bool? debug = null,
        IDictionary<string, StringValues>? queryParameters = null,
        Guid? previewTokenId = null,
        WebApplicationFactory<Startup>? app = null,
        string? requestSource = null)
    {
        var query = new Dictionary<string, StringValues>
        {
            { "indicators", indicators.ToArray() }
        };

        if (dataSetVersion is not null)
        {
            query["dataSetVersion"] = dataSetVersion;
        }

        if (page is not null)
        {
            query["page"] = page.ToString();
        }

        if (pageSize is not null)
        {
            query["pageSize"] = pageSize.ToString();
        }

        if (sorts is not null)
        {
            query["sorts"] = sorts.ToArray();
        }

        if (debug is true)
        {
            query["debug"] = "true";
        }

        if (queryParameters is not null)
        {
            query.AddRange(queryParameters);
        }

        var client = (app ?? BuildApp())
            .CreateClient()
            .WithPreviewTokenHeader(previewTokenId)
            .WithRequestSourceHeader(requestSource);

        var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/query", query);

        return await client.GetAsync(uri);
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

