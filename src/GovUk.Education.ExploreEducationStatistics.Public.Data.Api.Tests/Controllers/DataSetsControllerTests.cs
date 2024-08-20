using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Controllers;

public abstract class DataSetsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private const string BaseUrl = "api/v1/data-sets";

    public class GetDataSetTests(TestApplicationFactory testApp) : DataSetsControllerTests(testApp)
    {
        [Theory]
        [InlineData(DataSetStatus.Published)]
        [InlineData(DataSetStatus.Deprecated)]
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
            Assert.Equal(dataSetVersion.PublicVersion, content.LatestVersion.Version);
            Assert.Equal(
                dataSetVersion.Published.TruncateNanoseconds(),
                content.LatestVersion.Published
            );
            Assert.Equal(dataSetVersion.TotalResults, content.LatestVersion.TotalResults);
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
        [InlineData(DataSetStatus.Draft)]
        [InlineData(DataSetStatus.Withdrawn)]
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

        private async Task<HttpResponseMessage> GetDataSet(Guid dataSetId)
        {
            var client = BuildApp().CreateClient();

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

                var response = await GetDataSetMeta(dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);

                foreach (var filterMetaViewModel in content.Filters)
                {
                    var filterMeta = Assert.Single(dataSetVersion.FilterMetas,
                        fm => fm.PublicId == filterMetaViewModel.Id);
                    Assert.Equal(filterMeta.Hint, filterMetaViewModel.Hint);
                    Assert.Equal(filterMeta.Label, filterMetaViewModel.Label);

                    var allFilterMetaLinks = filterMeta.Options
                        .SelectMany(o => o.MetaLinks)
                        .ToList();

                    foreach (var filterOptionMetaViewModel in filterMetaViewModel.Options)
                    {
                        var filterOptionMetaLink = Assert.Single(
                            allFilterMetaLinks,
                            link => link.PublicId == filterOptionMetaViewModel.Id);

                        var filterOptionMeta = Assert.Single(
                            filterMeta.Options,
                            o => o.Id == filterOptionMetaLink.OptionId);

                        Assert.Equal(filterOptionMeta.Label, filterOptionMetaViewModel.Label);
                        Assert.Equal(filterOptionMeta.IsAggregate, filterOptionMetaViewModel.IsAggregate);
                    }
                }

                foreach (var locationMetaViewModel in content.Locations)
                {
                    var locationMeta = Assert.Single(
                        dataSetVersion.LocationMetas,
                        m => m.Level == locationMetaViewModel.Level.Code);

                    Assert.Equal(locationMeta.Level.GetEnumLabel(), locationMetaViewModel.Level.Label);

                    foreach (var locationOptionViewModel in locationMetaViewModel.Options)
                    {
                        var locationOptionMetaLink = Assert.Single(
                            locationMeta.OptionLinks,
                            o => o.PublicId == locationOptionViewModel.Id);

                        var locationOptionMeta = locationOptionMetaLink.Option;

                        switch (locationOptionMeta)
                        {
                            case LocationCodedOptionMeta codedMeta:
                                var codedViewModel =
                                    Assert.IsType<LocationCodedOptionViewModel>(locationOptionViewModel);
                                Assert.Equal(codedMeta.Label, codedViewModel.Label);
                                Assert.Equal(codedMeta.Code, codedViewModel.Code);
                                break;
                            case LocationLocalAuthorityOptionMeta localAuthorityMeta:
                                var localAuthorityViewModel =
                                    Assert.IsType<LocationLocalAuthorityOptionViewModel>(locationOptionViewModel);
                                Assert.Equal(localAuthorityMeta.Label, localAuthorityViewModel.Label);
                                Assert.Equal(localAuthorityMeta.Code, localAuthorityViewModel.Code);
                                Assert.Equal(localAuthorityMeta.OldCode, localAuthorityViewModel.OldCode);
                                break;
                            case LocationProviderOptionMeta providerMeta:
                                var providerViewModel =
                                    Assert.IsType<LocationProviderOptionViewModel>(locationOptionViewModel);
                                Assert.Equal(providerMeta.Label, providerViewModel.Label);
                                Assert.Equal(providerMeta.Ukprn, providerViewModel.Ukprn);
                                break;
                            case LocationRscRegionOptionMeta rscRegionMeta:
                                var rscRegionViewModel =
                                    Assert.IsType<LocationRscRegionOptionViewModel>(locationOptionViewModel);
                                Assert.Equal(rscRegionMeta.Label, rscRegionViewModel.Label);
                                break;
                            case LocationSchoolOptionMeta schoolMeta:
                                var schoolViewModel =
                                    Assert.IsType<LocationSchoolOptionViewModel>(locationOptionViewModel);
                                Assert.Equal(schoolMeta.Label, schoolViewModel.Label);
                                Assert.Equal(schoolMeta.Urn, schoolViewModel.Urn);
                                Assert.Equal(schoolMeta.LaEstab, schoolViewModel.LaEstab);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }

                Assert.All(
                    content.GeographicLevels,
                    level => Assert.Equal(level.Code.GetEnumLabel(), level.Label)
                );

                foreach (var indicator in content.Indicators)
                {
                    var indicatorMeta = Assert.Single(dataSetVersion.IndicatorMetas, im => im.PublicId == indicator.Id);

                    Assert.Equal(indicatorMeta.Label, indicator.Label);
                    Assert.Equal(indicatorMeta.Unit, indicator.Unit);
                    Assert.Equal(indicatorMeta.DecimalPlaces, indicator.DecimalPlaces);
                }

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
            [InlineData(DataSetVersionStatus.Published)]
            [InlineData(DataSetVersionStatus.Withdrawn)]
            [InlineData(DataSetVersionStatus.Deprecated)]
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

                var response = await GetDataSetMeta(dataSet.Id);

                var content = response.AssertOk<DataSetMetaViewModel>(useSystemJson: true);

                Assert.NotNull(content);
            }

            [Theory]
            [InlineData(DataSetVersionStatus.Processing)]
            [InlineData(DataSetVersionStatus.Failed)]
            [InlineData(DataSetVersionStatus.Mapping)]
            [InlineData(DataSetVersionStatus.Draft)]
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

                var response = await GetDataSetMeta(dataSet.Id);

                response.AssertForbidden();
            }

            [Fact]
            public async Task DataSetDoesNotExist_Returns404()
            {
                var response = await GetDataSetMeta(Guid.NewGuid());

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

                var response = await GetDataSetMeta(dataSet.Id, "2.0");

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

                var response = await GetDataSetMeta(dataSet.Id);

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

                var response = await GetDataSetMeta(dataSet2.Id, dataSetVersion.PublicVersion);

                response.AssertNotFound();
            }

            [Fact]
            public async Task VersionDoesNotExist_Returns404()
            {
                DataSet dataSet = DataFixture
                    .DefaultDataSet()
                    .WithStatusPublished();

                await TestApp.AddTestData<PublicDataDbContext>(context => context.DataSets.Add(dataSet));

                var response = await GetDataSetMeta(dataSet.Id, "1.0");

                response.AssertNotFound();
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

                var response = await GetDataSetMeta(dataSet.Id);

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
                    dataSet.Id,
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
                    dataSet.Id,
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
                    dataSet.Id,
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
                var response = await GetDataSetMeta(Guid.NewGuid(), types: []);

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
                var response = await GetDataSetMeta(Guid.NewGuid(), types: [metaType]);

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
                var response = await GetDataSetMeta(Guid.NewGuid(), types: ["invalid1", "invalid2"]);

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
                var response = await GetDataSetMeta(Guid.NewGuid(),
                    types: [DataSetMetaType.Filters.ToString(), "invalid"]);

                var validationProblem = response.AssertValidationProblem();

                Assert.Single(validationProblem.Errors);

                validationProblem.AssertHasAllowedValueError(
                    expectedPath: "types[1]",
                    value: "invalid",
                    allowed: EnumUtil.GetEnumValues<DataSetMetaType>());
            }
        }

        private async Task<HttpResponseMessage> GetDataSetMeta(
            Guid dataSetId,
            string? dataSetVersion = null,
            IReadOnlyList<string>? types = null)
        {
            var query = new Dictionary<string, string?>
            {
                { "dataSetVersion", dataSetVersion },
            };

            if (types is not null)
            {
                query.Add("types", types.JoinToString(","));
            }

            var uri = QueryHelpers.AddQueryString($"{BaseUrl}/{dataSetId}/meta", query);

            var client = BuildApp().CreateClient();

            return await client.GetAsync(uri);
        }
    }

    private WebApplicationFactory<Startup> BuildApp()
    {
        return TestApp;
    }
}
