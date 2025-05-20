using System.Globalization;
using System.Net.Mime;
using System.Text.Json;
using CsvHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.TheoryData;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class DataSetsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixtureWithCommonTestDataSetup(testApp)
{
    private const string BaseUrl = "v1/data-sets";

    public abstract class GetDataSetTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        public class PublishedDataSetTests(TestApplicationFactory testApp) : GetDataSetTests(testApp)
        {
            [Theory]
            [MemberData(nameof(DataSetStatusTheoryData.AvailableStatuses),
                MemberType = typeof(DataSetStatusTheoryData))]
            public async Task DataSetIsAvailable_Returns200(DataSetStatus dataSetStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(dataSetStatus);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSet(dataSet.Id);

                var content = response.AssertOk<DataSetViewModel>(useSystemJson: true);

                Assert.NotNull(content);
                Assert.Equal(dataSet.Id, content.Id);
                Assert.Equal(dataSet.Title, content.Title);
                Assert.Equal(dataSet.Summary, content.Summary);
                Assert.Equal(dataSet.Status, content.Status);
                Assert.Equal(dataSet.SupersedingDataSetId, content.SupersedingDataSetId);
                Assert.NotNull(content.LatestVersion);
                Assert.Equal(dataSetVersion.PublicVersion, content.LatestVersion.Version);
                Assert.Equal(
                    dataSetVersion.Published.TruncateNanoseconds(),
                    content.LatestVersion.Published
                );
                Assert.Equal(dataSetVersion.TotalResults, content.LatestVersion.TotalResults);
                Assert.Equal(dataSetVersion.Release.DataSetFileId, content.LatestVersion.File.Id);
                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary!.TimePeriodRange.Start.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.Start.Code),
                    content.LatestVersion.TimePeriods.Start);
                Assert.Equal(
                    TimePeriodFormatter.FormatLabel(
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Period,
                        dataSetVersion.MetaSummary.TimePeriodRange.End.Code),
                    content.LatestVersion.TimePeriods.End);
                Assert.Equal(dataSetVersion.MetaSummary.GeographicLevels, content.LatestVersion.GeographicLevels);
                Assert.Equal(dataSetVersion.MetaSummary.Filters, content.LatestVersion.Filters);
                Assert.Equal(dataSetVersion.MetaSummary.Indicators, content.LatestVersion.Indicators);
            }

            [Theory]
            [MemberData(nameof(DataSetStatusTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetStatusTheoryData))]
            public async Task DataSetNotAvailable_Returns403(DataSetStatus dataSetStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(dataSetStatus);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var response = await GetDataSet(dataSet.Id);

                response.AssertForbidden();
            }

            [Fact]
            public async Task DataSetDoesNotExist_Returns404()
            {
                var response = await GetDataSet(Guid.NewGuid());

                response.AssertNotFound();
            }
        }

        public class PreviewTokenTests(TestApplicationFactory testApp) : GetDataSetTests(testApp)
        {
            [Theory]
            [MemberData(nameof(DataSetStatusTheoryData.AllStatuses),
                MemberType = typeof(DataSetStatusTheoryData))]
            public async Task PreviewTokenIsActive_Returns200(DataSetStatus dataSetStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(dataSetStatus);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSet(
                    dataSetId: dataSet.Id,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                var content = response.AssertOk<DataSetViewModel>(useSystemJson: true);

                Assert.NotNull(content);
                Assert.Equal(dataSet.Id, content.Id);
                Assert.Equal(dataSet.Title, content.Title);
                Assert.Equal(dataSet.Summary, content.Summary);
                Assert.Equal(dataSet.Status, content.Status);
                Assert.Equal(dataSet.SupersedingDataSetId, content.SupersedingDataSetId);
                Assert.Null(content.LatestVersion);
            }
            
            /// <summary>
            /// Despite the wrong Preview Token being used, the Data Set itself is available to the
            /// public, and so the auth succeeds.
            /// </summary>
            [Fact]
            public async Task PreviewTokenIsActiveButForLiveVersion_DataSetIsAvailable_ReturnsOk()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(DataSetStatus.Published);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion liveDataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Published)
                    .WithPublished(DateTimeOffset.UtcNow)
                    .WithVersionNumber(1, 0)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                DataSetVersion draftDataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithVersionNumber(1, 1)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(draftDataSetVersion, liveDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                // We are requesting the DataSet but with the live DataSetVersion's preview token
                // rather than the current draft's.
                var response = await GetDataSet(
                    dataSetId: dataSet.Id,
                    previewTokenId: liveDataSetVersion.PreviewTokens[0].Id);

                response.AssertOk();
            }
            
            [Fact]
            public async Task PreviewTokenIsExpired_Returns403()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusDraft();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSet(
                    dataSetId: dataSet.Id,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                response.AssertForbidden();
            }

            [Fact]
            public async Task PreviewTokenIsForWrongDataSet_Returns403()
            {
                var (dataSet1, dataSet2) = DataFixture
                    .DefaultDataSet()
                    .WithStatusDraft()
                    .GenerateTuple2();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSets.AddRange(dataSet1, dataSet2);
                });

                var dataSet1Version = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSet(dataSet1)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet1.LatestDraftVersion = dsv)
                    .Generate();

                var dataSet2Version = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSet(dataSet2)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet2.LatestDraftVersion = dsv)
                    .Generate();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSet1Version, dataSet2Version);
                    context.DataSets.UpdateRange(dataSet1, dataSet2);
                });

                // Firstly check that a correctly matched preview token will 
                // produce a successful response.
                var successResponse = await GetDataSet(
                    dataSetId: dataSet1.Id,
                    previewTokenId: dataSet1Version.PreviewTokens[0].Id);

                successResponse.AssertOk();

                // Then check that a mismatched preview token will 
                // produce a forbidden response.
                var forbiddenResponse = await GetDataSet(
                    dataSetId: dataSet1.Id,
                    previewTokenId: dataSet2Version.PreviewTokens[0].Id);

                forbiddenResponse.AssertForbidden();
            }

            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatusesExceptDraft),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task PreviewTokenIsForUnavailableDataSetVersion_Returns403(
                DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusDraft();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSet(
                    dataSetId: dataSet.Id,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                response.AssertForbidden();
            }
        }

        private async Task<HttpResponseMessage> GetDataSet(
            Guid dataSetId,
            Guid? previewTokenId = null)
        {
            var client = BuildApp().CreateClient();
            client.AddPreviewTokenHeader(previewTokenId);
            
            var uri = new Uri($"{BaseUrl}/{dataSetId}", UriKind.Relative);

            return await client.GetAsync(uri);
        }
    }

    public class GetDataSetMetaTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        public class NoQueryParametersTests(TestApplicationFactory testApp) : GetDataSetMetaTests(testApp)
        {
            [Fact]
            public async Task ReturnsCorrectViewModel()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var filterMetas = DataFixture
                    .DefaultFilterMeta()
                    .WithOptions(() => DataFixture
                        .DefaultFilterOptionMeta()
                        .GenerateList(3))
                    .GenerateList(3);

                var allLocationOptionMetaTypesGeneratorByLevel =
                    new Dictionary<GeographicLevel, Func<LocationOptionMeta>>
                    {
                        { GeographicLevel.School, () => DataFixture.DefaultLocationSchoolOptionMeta() },
                        { GeographicLevel.LocalAuthority, () => DataFixture.DefaultLocationLocalAuthorityOptionMeta() },
                        { GeographicLevel.RscRegion, () => DataFixture.DefaultLocationRscRegionOptionMeta() },
                        { GeographicLevel.Provider, () => DataFixture.DefaultLocationProviderOptionMeta() },
                        { GeographicLevel.EnglishDevolvedArea, () => DataFixture.DefaultLocationCodedOptionMeta() },
                    };

                var locationMetas = allLocationOptionMetaTypesGeneratorByLevel
                    .Select(locationOptionMetaGenerator => DataFixture
                        .DefaultLocationMeta()
                        .WithOptions(() => new List<LocationOptionMeta>
                        {
                            locationOptionMetaGenerator.Value.Invoke(),
                            locationOptionMetaGenerator.Value.Invoke(),
                            locationOptionMetaGenerator.Value.Invoke()
                        })
                        .WithLevel(locationOptionMetaGenerator.Key))
                    .Select(locationMeta => (LocationMeta)locationMeta)
                    .ToList();

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .WithFilterMetas(() => filterMetas)
                    .WithLocationMetas(() => locationMetas)
                    .WithGeographicLevelMeta()
                    .WithIndicatorMetas(() =>
                        DataFixture
                            .DefaultIndicatorMeta()
                            .GenerateList(3)
                    )
                    .WithTimePeriodMetas(() =>
                        DataFixture
                            .DefaultTimePeriodMeta()
                            .GenerateList(3)
                    )
                    .WithMetaSummary()
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSetId: dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                Assert.Equal(dataSetVersion.FilterMetas.Count, content.Filters.Count);

                foreach (var filter in content.Filters)
                {
                    var filterMeta = Assert.Single(dataSetVersion.FilterMetas,
                        fm => fm.PublicId == filter.Id);

                    Assert.Equal(filterMeta.Hint, filter.Hint);
                    Assert.Equal(filterMeta.Label, filter.Label);

                    var allFilterMetaLinks = filterMeta.Options
                        .SelectMany(o => o.MetaLinks)
                        .ToList();

                    Assert.Equal(filterMeta.OptionLinks.Count, filter.Options.Count);

                    foreach (var filterOptionMetaViewModel in filter.Options)
                    {
                        var filterOptionMetaLink = Assert.Single(
                            allFilterMetaLinks,
                            link => link.PublicId == filterOptionMetaViewModel.Id);

                        var filterOptionMeta = Assert.Single(
                            filterMeta.Options,
                            o => o.Id == filterOptionMetaLink.OptionId);

                        Assert.Equal(filterOptionMeta.Label, filterOptionMetaViewModel.Label);
                    }
                }

                Assert.Equal(dataSetVersion.LocationMetas.Count, content.Locations.Count);

                foreach (var locationGroup in content.Locations)
                {
                    var locationMeta = Assert.Single(
                        dataSetVersion.LocationMetas,
                        m => m.Level == locationGroup.Level.Code);

                    Assert.Equal(locationMeta.Level.GetEnumLabel(), locationGroup.Level.Label);
                    Assert.Equal(locationMeta.OptionLinks.Count, locationGroup.Options.Count);

                    foreach (var locationOption in locationGroup.Options)
                    {
                        var locationOptionMetaLink = Assert.Single(
                            locationMeta.OptionLinks,
                            o => o.PublicId == locationOption.Id);

                        var locationOptionMeta = locationOptionMetaLink.Option;

                        switch (locationOptionMeta)
                        {
                            case LocationCodedOptionMeta codedMeta:
                                var codedViewModel =
                                    Assert.IsType<LocationCodedOptionViewModel>(locationOption);
                                Assert.Equal(codedMeta.Label, codedViewModel.Label);
                                Assert.Equal(codedMeta.Code, codedViewModel.Code);
                                break;
                            case LocationLocalAuthorityOptionMeta localAuthorityMeta:
                                var localAuthorityViewModel =
                                    Assert.IsType<LocationLocalAuthorityOptionViewModel>(locationOption);
                                Assert.Equal(localAuthorityMeta.Label, localAuthorityViewModel.Label);
                                Assert.Equal(localAuthorityMeta.Code, localAuthorityViewModel.Code);
                                Assert.Equal(localAuthorityMeta.OldCode, localAuthorityViewModel.OldCode);
                                break;
                            case LocationProviderOptionMeta providerMeta:
                                var providerViewModel =
                                    Assert.IsType<LocationProviderOptionViewModel>(locationOption);
                                Assert.Equal(providerMeta.Label, providerViewModel.Label);
                                Assert.Equal(providerMeta.Ukprn, providerViewModel.Ukprn);
                                break;
                            case LocationRscRegionOptionMeta rscRegionMeta:
                                var rscRegionViewModel =
                                    Assert.IsType<LocationRscRegionOptionViewModel>(locationOption);
                                Assert.Equal(rscRegionMeta.Label, rscRegionViewModel.Label);
                                break;
                            case LocationSchoolOptionMeta schoolMeta:
                                var schoolViewModel =
                                    Assert.IsType<LocationSchoolOptionViewModel>(locationOption);
                                Assert.Equal(schoolMeta.Label, schoolViewModel.Label);
                                Assert.Equal(schoolMeta.Urn, schoolViewModel.Urn);
                                Assert.Equal(schoolMeta.LaEstab, schoolViewModel.LaEstab);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }

                Assert.Equal(dataSetVersion.GeographicLevelMeta!.Levels.Count, content.GeographicLevels.Count);
                Assert.All(
                    content.GeographicLevels,
                    level => Assert.Equal(level.Code.GetEnumLabel(), level.Label)
                );

                Assert.Equal(dataSetVersion.IndicatorMetas.Count, content.Indicators.Count);

                foreach (var indicator in content.Indicators)
                {
                    var indicatorMeta = Assert.Single(dataSetVersion.IndicatorMetas, im => im.PublicId == indicator.Id);

                    Assert.Equal(indicatorMeta.Label, indicator.Label);
                    Assert.Equal(indicatorMeta.Unit, indicator.Unit);
                    Assert.Equal(indicatorMeta.DecimalPlaces, indicator.DecimalPlaces);
                }

                Assert.Equal(dataSetVersion.TimePeriodMetas.Count, content.TimePeriods.Count);

                foreach (var timePeriod in content.TimePeriods)
                {
                    var timePeriodMeta = Assert.Single(
                        dataSetVersion.TimePeriodMetas,
                        tp => tp.Code == timePeriod.Code
                              && tp.Period == timePeriod.Period);

                    Assert.Equal(
                        TimePeriodFormatter.FormatLabel(timePeriodMeta.Period, timePeriodMeta.Code),
                        timePeriod.Label);
                }
            }

            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.AvailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task VersionAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatus(dataSetVersionStatus)
                    .WithPublished(DateTimeOffset.UtcNow)
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSetId: dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);
            }

            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task VersionNotAvailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSetId: dataSet.Id);

                response.AssertForbidden();
            }

            [Fact]
            public async Task DataSetDoesNotExist_Returns404()
            {
                var response = await GetDataSetMeta(dataSetId: Guid.NewGuid());

                response.AssertNotFound();
            }
        }

        public class DataSetVersionQueryParameterTests(TestApplicationFactory testApp) : GetDataSetMetaTests(testApp)
        {
            [Fact]
            public async Task VersionSpecified_ReturnsCorrectVersion()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var dataSetVersions = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .ForIndex(0, dsv => dsv
                        .SetVersionNumber(1, 0)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                                    .DefaultFilterMeta()
                                    .WithLabel("filter 1")
                            ]
                        )
                    )
                    .ForIndex(1, dsv => dsv
                        .SetVersionNumber(1, 1)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                                    .DefaultFilterMeta()
                                    .WithLabel("filter 2")
                            ]
                        )
                    )
                    .ForIndex(2, dsv => dsv
                        .SetVersionNumber(2, 0)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                                    .DefaultFilterMeta()
                                    .WithLabel("filter 3")
                            ]
                        )
                    )
                    .ForIndex(3, dsv => dsv
                        .SetVersionNumber(2, 1)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                                    .DefaultFilterMeta()
                                    .WithLabel("filter 4")
                            ]
                        )
                    )
                    .GenerateList();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                    context.DataSetVersions.AddRange(dataSetVersions));

                var response = await GetDataSetMeta(dataSetId: dataSet.Id, dataSetVersion: "2.0");

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);
                Assert.Equal("filter 3", content.Filters.Single().Label);
            }

            [Fact]
            public async Task VersionUnspecified_ReturnsLatestVersion()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var dataSetVersions = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .ForIndex(0, dsv => dsv
                        .SetVersionNumber(1, 0)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                                    .DefaultFilterMeta()
                                    .WithLabel("filter 1")
                            ]
                        )
                    )
                    .ForIndex(1, dsv => dsv
                        .SetVersionNumber(1, 1)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                                    .DefaultFilterMeta()
                                    .WithLabel("filter 2")
                            ]
                        )
                    )
                    .ForIndex(2, dsv => dsv
                        .SetVersionNumber(2, 0)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                                    .DefaultFilterMeta()
                                    .WithLabel("filter 3")
                            ]
                        )
                    )
                    .ForIndex(3, dsv => dsv
                        .SetVersionNumber(2, 1)
                        .SetFilterMetas(() =>
                            [
                                DataFixture
                                    .DefaultFilterMeta()
                                    .WithLabel("filter 4")
                            ]
                        )
                    )
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv)
                    .GenerateList();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSetVersions);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSetId: dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);
                Assert.Equal("filter 4", content.Filters.Single().Label);
            }

            [Fact]
            public async Task VersionExistsForOtherDataSet_Returns404()
            {
                DataSet dataSet1 = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                DataSet dataSet2 = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                    context.DataSets.AddRange(dataSet1, dataSet2));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 3)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet1.Id);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

                var response =
                    await GetDataSetMeta(dataSetId: dataSet2.Id, dataSetVersion: dataSetVersion.PublicVersion);

                response.AssertNotFound();
            }

            [Fact]
            public async Task VersionDoesNotExist_Returns404()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var response = await GetDataSetMeta(dataSetId: dataSet.Id, dataSetVersion: "1.0");

                response.AssertNotFound();
            }
            
            [Theory]
            [MemberData(nameof(DataSetVersionStatusQueryTheoryData.NonPublishedStatus),
                MemberType = typeof(DataSetVersionStatusQueryTheoryData))]
            public async Task WildCardSpecified_RequestsNonPublishedVersion_Returns404(DataSetVersionStatus versionStatus)
            {
                var (dataSet, _) = await SetupDataSetWithSpecifiedVersionStatuses(versionStatus);
            
                var response = await GetDataSetMeta(dataSetId: dataSet.Id, dataSetVersion: "2.*");

                response.AssertNotFound();
            }
        
            [Fact]
            public async Task WildCardSpecified_RequestsPublishedVersion_Returns200()
            {
                var (dataSet, _) = await SetupDataSetWithSpecifiedVersionStatuses(DataSetVersionStatus.Published);
            
                var response = await GetDataSetMeta(dataSetId: dataSet.Id, dataSetVersion: "2.*");

                var viewModel = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);
                Assert.NotNull(viewModel);
            }
        }

        public class TypesQueryParameterTests(TestApplicationFactory testApp) : GetDataSetMetaTests(testApp)
        {
            [Fact]
            public async Task TypesNotSpecified_ReturnsAllMeta()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(dataSetId: dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                Assert.NotEmpty(content.Filters);
                Assert.NotEmpty(content.Locations);
                Assert.NotEmpty(content.Indicators);
                Assert.NotEmpty(content.TimePeriods);
            }

            [Theory]
            [InlineData(DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.TimePeriods)]
            public async Task OneTypeSpecified_ReturnsOnlySpecifiedMetaType(DataSetMetaType metaType)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(
                    dataSetId: dataSet.Id,
                    types: [metaType.ToString()]);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                switch (metaType)
                {
                    case DataSetMetaType.Filters:
                        Assert.NotEmpty(content.Filters);
                        Assert.Empty(content.Locations);
                        Assert.Empty(content.Indicators);
                        Assert.Empty(content.TimePeriods);
                        break;
                    case DataSetMetaType.Locations:
                        Assert.Empty(content.Filters);
                        Assert.NotEmpty(content.Locations);
                        Assert.Empty(content.Indicators);
                        Assert.Empty(content.TimePeriods);
                        break;
                    case DataSetMetaType.Indicators:
                        Assert.Empty(content.Filters);
                        Assert.Empty(content.Locations);
                        Assert.NotEmpty(content.Indicators);
                        Assert.Empty(content.TimePeriods);
                        break;
                    case DataSetMetaType.TimePeriods:
                        Assert.Empty(content.Filters);
                        Assert.Empty(content.Locations);
                        Assert.Empty(content.Indicators);
                        Assert.NotEmpty(content.TimePeriods);
                        break;
                }
            }

            [Theory]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations, DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations, DataSetMetaType.Indicators,
                DataSetMetaType.TimePeriods)]
            public async Task MultipleTypesSpecified_ReturnsOnlySpecifiedMetaTypes(params DataSetMetaType[] metaTypes)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(
                    dataSetId: dataSet.Id,
                    types: metaTypes.Select(t => t.ToString()).ToList());

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                var allMetaTypes = EnumUtil.GetEnums<DataSetMetaType>().ToHashSet();
                var specifiedMetaTypes = metaTypes.ToHashSet();
                var unspecifiedMetaTypes = allMetaTypes.Except(specifiedMetaTypes);

                foreach (var type in specifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.NotEmpty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.NotEmpty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.NotEmpty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.NotEmpty(content.TimePeriods);
                            break;
                    }
                }

                foreach (var type in unspecifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.Empty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.Empty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.Empty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.Empty(content.TimePeriods);
                            break;
                    }
                }
            }

            [Theory]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.Locations, DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Indicators, DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.TimePeriods, DataSetMetaType.TimePeriods)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Filters, DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Locations, DataSetMetaType.Locations, DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.Indicators, DataSetMetaType.Indicators, DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.TimePeriods, DataSetMetaType.TimePeriods, DataSetMetaType.Filters)]
            public async Task DuplicateTypesSpecified_ReturnsOnlySpecifiedMetaTypes(params DataSetMetaType[] metaTypes)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(
                    dataSetId: dataSet.Id,
                    types: metaTypes.Select(t => t.ToString()).ToList());

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                var allMetaTypes = EnumUtil.GetEnums<DataSetMetaType>().ToHashSet();
                var specifiedMetaTypes = metaTypes.ToHashSet();
                var unspecifiedMetaTypes = allMetaTypes.Except(specifiedMetaTypes);

                foreach (var type in specifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.NotEmpty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.NotEmpty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.NotEmpty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.NotEmpty(content.TimePeriods);
                            break;
                    }
                }

                foreach (var type in unspecifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.Empty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.Empty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.Empty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.Empty(content.TimePeriods);
                            break;
                    }
                }
            }

            [Theory]
            [InlineData(DataSetMetaType.Filters)]
            [InlineData(DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.TimePeriods)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations, DataSetMetaType.Indicators)]
            [InlineData(DataSetMetaType.Filters, DataSetMetaType.Locations, DataSetMetaType.Indicators,
                DataSetMetaType.TimePeriods)]
            public async Task ArrayQueryParameterSyntax_ReturnsOnlySpecifiedMetaTypes(
                params DataSetMetaType[] metaTypes)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatusPublished()
                    .WithDataSetId(dataSet.Id)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var query = metaTypes
                    .Select((mt, index) => new
                    {
                        mt,
                        index
                    })
                    .ToDictionary(a => $"types[{a.index}]", a => a.mt.ToString());

                var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSet.Id}/meta", query!);

                var client = TestApp.CreateClient();

                var response = await client.GetAsync(uri);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                var allMetaTypes = EnumUtil.GetEnums<DataSetMetaType>().ToHashSet();
                var specifiedMetaTypes = metaTypes.ToHashSet();
                var unspecifiedMetaTypes = allMetaTypes.Except(specifiedMetaTypes);

                foreach (var type in specifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.NotEmpty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.NotEmpty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.NotEmpty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.NotEmpty(content.TimePeriods);
                            break;
                    }
                }

                foreach (var type in unspecifiedMetaTypes)
                {
                    switch (type)
                    {
                        case DataSetMetaType.Filters:
                            Assert.Empty(content.Filters);
                            break;
                        case DataSetMetaType.Locations:
                            Assert.Empty(content.Locations);
                            break;
                        case DataSetMetaType.Indicators:
                            Assert.Empty(content.Indicators);
                            break;
                        case DataSetMetaType.TimePeriods:
                            Assert.Empty(content.TimePeriods);
                            break;
                    }
                }
            }

            [Fact]
            public async Task EmptyList_AllowedValueError()
            {
                var response = await GetDataSetMeta(dataSetId: Guid.NewGuid(), types: []);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[0]",
                    value: null,
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }

            [Theory]
            [InlineData("filters", "filters")]
            [InlineData("locations", "locations")]
            [InlineData("indicators", "indicators")]
            [InlineData("timePeriods", "timePeriods")]
            [InlineData("invalid", "invalid")]
            [InlineData(null, "")]
            [InlineData(null, " ")]
            public async Task InvalidMetaType_AllowedValueError(string? invalidType, string metaType)
            {
                var response = await GetDataSetMeta(dataSetId: Guid.NewGuid(), types: [metaType]);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[0]",
                    value: invalidType,
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }

            [Fact]
            public async Task MultipleInvalidMetaTypes_AllowedValueError()
            {
                var response = await GetDataSetMeta(dataSetId: Guid.NewGuid(), types: ["invalid1", "invalid2"]);

                var validationProblem = response.AssertValidationProblem();

                Assert.Equal(2, validationProblem.Errors.Count);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[0]",
                    value: "invalid1",
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[1]",
                    value: "invalid2",
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }

            [Fact]
            public async Task MixedValidAndInvalidMetaTypes_AllowedValueError()
            {
                var response = await GetDataSetMeta(
                    dataSetId: Guid.NewGuid(),
                    types: [DataSetMetaType.Filters.ToString(), "invalid"]);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[1]",
                    value: "invalid",
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }
        }

        public class PreviewTokenTests(TestApplicationFactory testApp) : GetDataSetMetaTests(testApp)
        {
            [Theory]
            [MemberData(nameof(DataSetStatusTheoryData.AllStatuses),
                MemberType = typeof(DataSetStatusTheoryData))]
            public async Task PreviewTokenIsActive_Returns200(DataSetStatus dataSetStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(dataSetStatus);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await GetDataSetMeta(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);
            }

            [Fact]
            public async Task PreviewTokenIsActiveButForLiveVersion_DraftVersionTargeted_Returns403()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(DataSetStatus.Published);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion liveDataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Published)
                    .WithPublished(DateTimeOffset.UtcNow)
                    .WithVersionNumber(1, 0)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);
                
                DataSetVersion draftDataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithVersionNumber(1, 1)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(liveDataSetVersion, draftDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                // We are requesting the draft DataSetVersion but with the live DataSetVersion's preview token.
                var response = await GetDataSetMeta(
                    dataSetId: dataSet.Id,
                    dataSetVersion: draftDataSetVersion.PublicVersion,
                    previewTokenId: liveDataSetVersion.PreviewTokens[0].Id);

                response.AssertForbidden();
            }
            
            [Fact]
            public async Task PreviewTokenIsExpired_Returns403()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusDraft();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)]);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSetVersions.Add(dataSetVersion));

                var response = await GetDataSetMeta(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                response.AssertForbidden();
            }

            [Fact]
            public async Task PreviewTokenIsForWrongDataSetVersion_Returns403()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusDraft();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var (dataSetVersion1, dataSetVersion2) = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .GenerateTuple2();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                    context.DataSetVersions.AddRange(dataSetVersion1, dataSetVersion2));

                var response = await GetDataSetMeta(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion1.PublicVersion,
                    previewTokenId: dataSetVersion2.PreviewTokens[0].Id);

                response.AssertForbidden();
            }

            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatusesExceptDraft),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task PreviewTokenIsForUnavailableDataSetVersion_Returns403(
                DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusDraft();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSetId(dataSet.Id)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()]);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                    context.DataSetVersions.AddRange(dataSetVersion));

                var response = await GetDataSetMeta(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                response.AssertForbidden();
            }
        }

        private async Task<HttpResponseMessage> GetDataSetMeta(
            Guid dataSetId,
            string? dataSetVersion = null,
            IReadOnlyList<string>? types = null,
            Guid? previewTokenId = null)
        {
            var client = BuildApp().CreateClient();
            client.AddPreviewTokenHeader(previewTokenId);

            var query = new Dictionary<string, string?>
            {
                { "dataSetVersion", dataSetVersion },
            };

            if (types is not null)
            {
                query.Add("types", types.JoinToString(","));
            }

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/meta", query);

            return await client.GetAsync(uri);
        }
    }

    public abstract class DownloadDataSetCsvTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        private static readonly List<TestClass> CsvData =
        [
            new()
            {
                FirstColumn = "first-column-value-1",
                SecondColumn = "second-column-value-1"
            },

            new()
            {
                FirstColumn = "first-column-value-2",
                SecondColumn = "second-column-value-2"
            }
        ];

        public class DownloadTests(TestApplicationFactory testApp) : DownloadDataSetCsvTests(testApp)
        {
            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.AvailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task LatestVersionAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(dataSetVersion, CsvData);

                var response = await DownloadDataSet(dataSet.Id);

                response.AssertOk();

                Assert.Equal(MediaTypeNames.Text.Csv, response.Content.Headers.ContentType!.MediaType);

                var contentEncoding = Assert.Single(response.Content.Headers.ContentEncoding);
                Assert.Equal(ContentEncodings.Gzip, contentEncoding);

                var results = await DecompressGZippedCsv(response);

                Assert.Equal(CsvData.Count, results.Count);
                Assert.Equal(CsvData, results);
            }

            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.AvailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task RequestedVersionAvailable_Returns200(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion latestDataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatusPublished()
                    .WithDataSet(dataSet);

                DataSetVersion requestedDataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(dataSetVersionStatus)
                    .WithVersionNumber(major: 1, minor: 1, patch: 0)
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange([latestDataSetVersion, requestedDataSetVersion]);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(requestedDataSetVersion, CsvData);

                var response = await DownloadDataSet(dataSet.Id, requestedDataSetVersion.PublicVersion);

                response.AssertOk();

                Assert.Equal(MediaTypeNames.Text.Csv, response.Content.Headers.ContentType!.MediaType);

                var contentEncoding = Assert.Single(response.Content.Headers.ContentEncoding);
                Assert.Equal(ContentEncodings.Gzip, contentEncoding);

                var results = await DecompressGZippedCsv(response);

                Assert.Equal(CsvData.Count, results.Count);
                Assert.Equal(CsvData, results);
            }

            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task LatestVersionUnavailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await DownloadDataSet(dataSet.Id);

                response.AssertForbidden();
            }

            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatuses),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task RequestedVersionUnavailable_Returns403(DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion latestDataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatusPublished()
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                DataSetVersion requestedDataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(dataSetVersionStatus)
                    .WithVersionNumber(major: 1, minor: 1, patch: 0)
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange([latestDataSetVersion, requestedDataSetVersion]);
                    context.DataSets.Update(dataSet);
                });

                var response = await DownloadDataSet(dataSet.Id, requestedDataSetVersion.PublicVersion);

                response.AssertForbidden();
            }

            [Fact]
            public async Task DataSetDoesNotExist_Returns404()
            {
                var response = await DownloadDataSet(Guid.NewGuid());

                response.AssertNotFound();
            }

            [Fact]
            public async Task LatestVersionDoesNotExist_Returns404()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var response = await DownloadDataSet(dataSet.Id);

                response.AssertNotFound();
            }

            [Fact]
            public async Task RequestedVersionDoesNotExist_Returns404()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatusPublished()
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                var response = await DownloadDataSet(dataSet.Id, "2.0");

                response.AssertNotFound();
            }
        
            [Fact]
            public async Task WildCardSpecified_RequestsPublishedVersion_Returns200()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion1 = DataFixture
                    .DefaultDataSetVersion()
                    .WithVersionNumber(major: 1, minor: 0, patch: 0)
                    .WithStatusPublished()
                    .WithDataSet(dataSet);
                DataSetVersion dataSetVersion2 = DataFixture
                    .DefaultDataSetVersion()
                    .WithVersionNumber(major: 2, minor: 0, patch: 0)
                    .WithStatusPublished()
                    .WithDataSet(dataSet);
                DataSetVersion requestedDataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(DataSetVersionStatus.Published)
                    .WithVersionNumber(major: 2, minor: 1, patch: 0)
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange([dataSetVersion1, dataSetVersion2, requestedDataSetVersion]);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(requestedDataSetVersion, CsvData);

                var response = await DownloadDataSet(dataSet.Id, "2.*");

                response.AssertOk();
            }
            
            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.NonPublishedStatus),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task WildCardSpecified_RequestsNonPublishedVersion_Returns404(DataSetVersionStatus versionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion liveDataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithVersionNumber(major: 1, minor: 0, patch: 0)
                    .WithStatusPublished()
                    .WithDataSet(dataSet)
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);
                DataSetVersion dataSetVersion1 = DataFixture
                    .DefaultDataSetVersion()
                    .WithVersionNumber(major: 2, minor: 0, patch: 0)
                    .WithStatus(versionStatus)
                    .WithDataSet(dataSet);
                DataSetVersion dataSetVersion2 = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(versionStatus)
                    .WithVersionNumber(major: 2, minor: 1, patch: 0)
                    .WithDataSet(dataSet);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange([dataSetVersion1, dataSetVersion2, liveDataSetVersion]);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(liveDataSetVersion, CsvData);

                var response = await DownloadDataSet(dataSet.Id, "2.*");

                response.AssertNotFound();
            }
        }

        public class PreviewTokenTests(TestApplicationFactory testApp) : DownloadDataSetCsvTests(testApp)
        {
            [Theory]
            [MemberData(nameof(DataSetStatusTheoryData.AllStatuses),
                MemberType = typeof(DataSetStatusTheoryData))]
            public async Task PreviewTokenIsActive_Returns200(DataSetStatus dataSetStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(dataSetStatus);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSet(dataSet)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(dataSetVersion, CsvData);

                var response = await DownloadDataSet(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                response.AssertOk();

                Assert.Equal(MediaTypeNames.Text.Csv, response.Content.Headers.ContentType!.MediaType);

                var contentEncoding = Assert.Single(response.Content.Headers.ContentEncoding);
                Assert.Equal(ContentEncodings.Gzip, contentEncoding);

                var results = await DecompressGZippedCsv(response);

                Assert.Equal(CsvData.Count, results.Count);
                Assert.Equal(CsvData, results);
            }
            
            [Fact]
            public async Task PreviewTokenIsActiveButForLiveVersion_DraftVersionTargeted_Returns403()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(DataSetStatus.Published);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion liveDataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(DataSetVersionStatus.Published)
                    .WithVersionNumber(1, 0)
                    .WithDataSet(dataSet)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);
                
                DataSetVersion draftDataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithVersionNumber(1, 1)
                    .WithDataSet(dataSet)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(liveDataSetVersion, draftDataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(liveDataSetVersion, CsvData);

                // We are requesting the draft DataSetVersion but with the live DataSetVersion's preview token.
                var response = await DownloadDataSet(
                    dataSetId: dataSet.Id,
                    dataSetVersion: draftDataSetVersion.PublicVersion,
                    previewTokenId: liveDataSetVersion.PreviewTokens[0].Id);

                response.AssertForbidden();
            }

            [Theory]
            [MemberData(nameof(DataSetStatusTheoryData.AllStatuses),
                MemberType = typeof(DataSetStatusTheoryData))]
            public async Task PreviewTokenIsExpired_Returns403(DataSetStatus dataSetStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(dataSetStatus);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSet(dataSet)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(dataSetVersion, CsvData);

                var response = await DownloadDataSet(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                response.AssertForbidden();
            }
            
            [Fact]
            public async Task PreviewTokenIsForWrongDataSet_Returns403()
            {
                var (dataSet1, dataSet2) = DataFixture
                    .DefaultDataSet()
                    .WithStatus(DataSetStatus.Draft)
                    .GenerateTuple2();

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSets.AddRange(dataSet1, dataSet2);
                });
            
                var dataSet1Version = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSet(dataSet1)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet1.LatestDraftVersion = dsv)
                    .Generate();
            
                var dataSet2Version = DataFixture
                    .DefaultDataSetVersion(filters: 1, indicators: 1, locations: 1, timePeriods: 2)
                    .WithStatus(DataSetVersionStatus.Draft)
                    .WithDataSet(dataSet2)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken()])
                    .FinishWith(dsv => dataSet2.LatestDraftVersion = dsv)
                    .Generate();
            
                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.AddRange(dataSet1Version, dataSet2Version);
                    context.DataSets.UpdateRange(dataSet1, dataSet2);
                });
                
                await CreateGZippedTestCsv(dataSet1Version, CsvData);
            
                // Firstly check that a correctly matched preview token will 
                // produce a successful response

                var successResponse = await DownloadDataSet(
                    dataSetId: dataSet1.Id,
                    dataSetVersion: dataSet1Version.PublicVersion,
                    previewTokenId: dataSet1Version.PreviewTokens[0].Id);
                
                successResponse.AssertOk();
                
                // Then check that a mismatched preview token will 
                // produce a forbidden response.
                var forbiddenResponse = await DownloadDataSet(
                    dataSetId: dataSet1.Id,
                    dataSetVersion: dataSet1Version.PublicVersion,
                    previewTokenId: dataSet2Version.PreviewTokens[0].Id);
            
                forbiddenResponse.AssertForbidden();
            }
            
            [Theory]
            [MemberData(nameof(DataSetVersionStatusViewTheoryData.UnavailableStatusesExceptDraft),
                MemberType = typeof(DataSetVersionStatusViewTheoryData))]
            public async Task PreviewTokenIsForUnavailableDataSetVersion_Returns403(
                DataSetVersionStatus dataSetVersionStatus)
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatus(DataSetStatus.Draft);

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(dataSetVersionStatus)
                    .WithDataSet(dataSet)
                    .WithPreviewTokens(() => [DataFixture.DefaultPreviewToken(expired: true)])
                    .FinishWith(dsv => dataSet.LatestDraftVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(dataSetVersion, CsvData);

                var response = await DownloadDataSet(
                    dataSetId: dataSet.Id,
                    dataSetVersion: dataSetVersion.PublicVersion,
                    previewTokenId: dataSetVersion.PreviewTokens[0].Id);

                response.AssertForbidden();
            }
        }

        public class AnalyticsEnabledTests : DownloadDataSetCsvTests, IDisposable
        {
            public AnalyticsEnabledTests(TestApplicationFactory testApp) : base(testApp)
            {
                testApp.AddAppSettings("appsettings.AnalyticsEnabled.json");
            }

            public void Dispose()
            {
                var queriesDirectory = GetAnalyticsPathResolver().PublicApiDataSetVersionCallsDirectoryPath();
                if (Directory.Exists(queriesDirectory))
                {
                    Directory.Delete(queriesDirectory, recursive: true);
                }
            }

            public record PreviewTokenSummary(
                string Label,
                DateTimeOffset Created,
                DateTimeOffset Expiry);

            public static readonly TheoryData<(PreviewTokenSummary?, string?)>
                PreviewTokensAndRequestedDataSetVersions =
                [
                    (null, null),
                    (new PreviewTokenSummary(Label: "Preview token",
                        Created: DateTimeOffset.UtcNow.AddDays(-1),
                        Expiry: DateTimeOffset.UtcNow.AddDays(1)), "1.0.*")
                ];

            [Theory]
            [MemberData(nameof(PreviewTokensAndRequestedDataSetVersions))]
            public async Task AnalyticsRequestCaptured(
                (PreviewTokenSummary?, string?) previewTokenAndRequestedDataSetVersion)
            {
                var (expectedPreviewToken, requestedDataSetVersion) = previewTokenAndRequestedDataSetVersion;
                
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                DataSetVersion dataSetVersion = DataFixture
                    .DefaultDataSetVersion()
                    .WithStatus(DataSetVersionStatus.Published)
                    .WithDataSet(dataSet)
                    .WithPreviewTokens(expectedPreviewToken != null
                        ? DataFixture
                            .DefaultPreviewToken()
                            .WithLabel(expectedPreviewToken.Label)
                            .WithCreated(expectedPreviewToken.Created)
                            .WithExpiry(expectedPreviewToken.Expiry)
                            .Generate(1) 
                        : [])
                    .FinishWith(dsv => dataSet.LatestLiveVersion = dsv);

                await TestApp.AddTestData<PublicDataDbContext>(context =>
                {
                    context.DataSetVersions.Add(dataSetVersion);
                    context.DataSets.Update(dataSet);
                });

                await CreateGZippedTestCsv(dataSetVersion, CsvData);

                var persistedPreviewToken = dataSetVersion
                    .PreviewTokens
                    .SingleOrDefault();
                
                var response = await DownloadDataSet(
                    dataSetId: dataSet.Id,
                    dataSetVersion: requestedDataSetVersion,
                    previewTokenId: persistedPreviewToken?.Id);
                
                response.AssertOk();
                
                var results = await DecompressGZippedCsv(response);
                Assert.Equal(CsvData, results);

                // Add a slight delay as the writing of the analytics capture is non-blocking
                // and could occur slightly after the Controller response is returned to the user.
                Thread.Sleep(2000);

                var analyticsPath = GetAnalyticsPathResolver().PublicApiDataSetVersionCallsDirectoryPath();

                // Expect the successful call to have been recorded for analytics.
                Assert.True(Directory.Exists(analyticsPath));
                var analyticsFiles = Directory.GetFiles(analyticsPath);
                var analyticFile = Assert.Single(analyticsFiles);
                var contents = await File.ReadAllTextAsync(analyticFile);

                var capturedCall = JsonSerializer.Deserialize<CaptureDataSetVersionCallRequest>(contents,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Assert.NotNull(capturedCall);
                Assert.Equal(DataSetVersionCallType.DownloadCsv, capturedCall.Type);
                Assert.Equal(dataSet.Id, capturedCall.DataSetId);
                Assert.Equal(dataSet.Title, capturedCall.DataSetTitle);
                Assert.Equal(dataSetVersion.Id, capturedCall.DataSetVersionId);
                Assert.Equal(dataSetVersion.SemVersion().ToString(), capturedCall.DataSetVersion);
                Assert.Equal(requestedDataSetVersion, capturedCall.RequestedDataSetVersion);
                Assert.Null(capturedCall.Parameters);
                capturedCall.StartTime.AssertUtcNow(withinMillis: 5000);

                if (expectedPreviewToken == null)
                {
                    Assert.Null(capturedCall.PreviewToken);
                }
                else
                {
                    Assert.NotNull(persistedPreviewToken);
                    Assert.NotNull(capturedCall.PreviewToken);
                    Assert.Equal(expectedPreviewToken.Label, capturedCall.PreviewToken.Label);
                    Assert.Equal(dataSetVersion.Id, capturedCall.PreviewToken.DataSetVersionId);
                    Assert.Equal(expectedPreviewToken.Created.TruncateMicroseconds(), capturedCall.PreviewToken.Created);
                    Assert.Equal(expectedPreviewToken.Expiry.TruncateMicroseconds(), capturedCall.PreviewToken.Expiry);
                }
            }
        }

        private async Task CreateGZippedTestCsv(DataSetVersion dataSetVersion, IReadOnlyList<TestClass> csvData)
        {
            var dataSetVersionPathResolver = BuildApp().Services.GetRequiredService<IDataSetVersionPathResolver>();

            await using var memStream = new MemoryStream();
            await using var streamWriter = new StreamWriter(memStream);
            await using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);

            await csvWriter.WriteRecordsAsync(csvData);
            await csvWriter.FlushAsync();

            var dataSetVersionDirectoryPath = dataSetVersionPathResolver.DirectoryPath(dataSetVersion);
            Directory.CreateDirectory(dataSetVersionDirectoryPath);

            var filePath = Path.Combine(dataSetVersionDirectoryPath, DataSetFilenames.CsvDataFile);

            await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

            await CompressionUtils.CompressToStream(memStream, fileStream, ContentEncodings.Gzip);
        }

        private static async Task<List<TestClass>> DecompressGZippedCsv(HttpResponseMessage downloadResponse)
        {
            await using var downloadStream = await downloadResponse.Content.ReadAsStreamAsync();
            await using var targetStream = new MemoryStream();
            using var streamReader = new StreamReader(targetStream);
            using var csvReader = new CsvReader(streamReader, CultureInfo.InvariantCulture);

            await CompressionUtils.DecompressToStream(downloadStream, targetStream, ContentEncodings.Gzip);

            return csvReader.GetRecords<TestClass>().ToList();
        }

        private async Task<HttpResponseMessage> DownloadDataSet(
            Guid dataSetId,
            string? dataSetVersion = null,
            Guid? previewTokenId = null)
        {
            var client = BuildApp().CreateClient();
            client.AddPreviewTokenHeader(previewTokenId);

            var query = new Dictionary<string, StringValues>();

            if (dataSetVersion is not null)
            {
                query["dataSetVersion"] = dataSetVersion;
            }

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/csv", query);

            return await client.GetAsync(uri);
        }

        private record TestClass
        {
            public string FirstColumn { get; init; } = null!;
            public string SecondColumn { get; init; } = null!;
        }
    }

    private IAnalyticsPathResolver GetAnalyticsPathResolver()
    {
        return BuildApp().Services.GetRequiredService<IAnalyticsPathResolver>();
    }

    private WebApplicationFactory<Startup> BuildApp()
    {
        return TestApp;
    }
}
