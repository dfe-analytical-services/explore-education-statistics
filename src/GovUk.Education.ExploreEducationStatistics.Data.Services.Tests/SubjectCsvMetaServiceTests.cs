#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests;

public class SubjectCsvMetaServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task GetSubjectCsvMeta()
    {
        var filters = _fixture.DefaultFilter()
            .ForIndex(0, s =>
                s.SetGroupCsvColumn("filter_0_grouping")
                    .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1)
                        .ForInstance(s => s.Set(
                            fg => fg.Label,
                            (_, _, context) => $"Filter group {context.FixtureTypeIndex}"))
                        .Generate(2)))
            .ForIndex(1, s =>
                s.SetGroupCsvColumn("filter_1_grouping")
                    .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1)
                        .ForInstance(s => s.Set(
                            fg => fg.Label,
                            (_, _, context) => $"Filter group {context.FixtureTypeIndex}"))
                        .Generate(2)))
            .ForIndex(2, s =>
                s.SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 2)
                    .Generate(1)))
            .GenerateList();

        var filter0Items = filters[0].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter1Items = filters[1].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter2Items = filters[2].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1)))
            .ForIndex(1, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(2))
            )
            .GenerateList();

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToList();

        var locations = _fixture.DefaultLocation()
            .ForRange(..2, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForRange(2..4, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateList();

        var observations = _fixture.DefaultObservation()
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(locations[0])
                .SetTimePeriod(2022, AcademicYear))
            .ForRange(2..4, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(locations[1])
                .SetTimePeriod(2022, AcademicYear))
            .ForRange(4..6, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                .SetLocation(locations[2])
                .SetTimePeriod(2023, AcademicYear))
            .ForRange(6..8, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                .SetLocation(locations[3])
                .SetTimePeriod(2023, AcademicYear))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            // Stubbing this out as testing headers in other methods
            releaseFileBlobService
                .Setup(s =>
                    s.StreamBlob(It.IsAny<ReleaseFile>(), null, default))
                .ReturnsAsync("csv_header".ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id).ToList()
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            Assert.Equal(3, viewModel.Filters.Count);

            var viewModelFilter0 = viewModel.Filters[filters[0].Name];
            var viewModelFilter1 = viewModel.Filters[filters[1].Name];
            var viewModelFilter2 = viewModel.Filters[filters[2].Name];

            AssertFilterCsvViewModel(filters[0], viewModelFilter0);
            AssertFilterCsvViewModel(filters[1], viewModelFilter1);
            AssertFilterCsvViewModel(filters[2], viewModelFilter2);

            var viewModelFilter0Items = viewModelFilter0.Items;
            Assert.Equal(2, viewModelFilter0Items.Count);

            AssertFilterItemCsvViewModel(filter0Items[0], viewModelFilter0Items[filter0Items[0].Id]);
            AssertFilterItemCsvViewModel(filter0Items[1], viewModelFilter0Items[filter0Items[1].Id]);

            var viewModelFilter1Items = viewModelFilter1.Items;
            Assert.Equal(2, viewModelFilter1Items.Count);

            AssertFilterItemCsvViewModel(filter1Items[0], viewModelFilter1Items[filter1Items[0].Id]);
            AssertFilterItemCsvViewModel(filter1Items[1], viewModelFilter1Items[filter1Items[1].Id]);

            var viewModelFilter2Items = viewModelFilter2.Items;
            Assert.Equal(2, viewModelFilter2Items.Count);

            AssertFilterItemCsvViewModel(filter2Items[0], viewModelFilter2Items[filter2Items[0].Id]);
            AssertFilterItemCsvViewModel(filter2Items[1], viewModelFilter2Items[filter2Items[1].Id]);

            Assert.Equal(3, viewModel.Indicators.Count);
            AssertIndicatorCsvMetaViewModel(indicators[0], viewModel.Indicators[indicators[0].Name]);
            AssertIndicatorCsvMetaViewModel(indicators[1], viewModel.Indicators[indicators[1].Name]);
            AssertIndicatorCsvMetaViewModel(indicators[2], viewModel.Indicators[indicators[2].Name]);

            var viewModelLocations = viewModel.Locations;

            Assert.Equal(4, viewModelLocations.Count);
            Assert.Equal(locations[0].GetCsvValues(),viewModelLocations[locations[0].Id]);
            Assert.Equal(locations[1].GetCsvValues(),viewModelLocations[locations[1].Id]);
            Assert.Equal(locations[2].GetCsvValues(),viewModelLocations[locations[2].Id]);
            Assert.Equal(locations[3].GetCsvValues(),viewModelLocations[locations[3].Id]);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_OnlyFiltersItemsWithObservations()
    {
        var filters = _fixture.DefaultFilter()
            .ForIndex(0, f => f
                .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 2).Generate(1))
            )
            .ForIndex(1, f => f
                .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 2).Generate(2))
            )
            .ForIndex(2, f => f
                .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 2).Generate(2))
            )
            .GenerateList();

        var filter0Items = filters[0].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter1Items = filters[1].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter2Items = filters[2].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup(indicatorCount: 3)
            .GenerateList(1);

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToList();

        var observations = _fixture.DefaultObservation()
            .WithLocation(_fixture.DefaultLocation())
            .WithMeasures(indicators)
            .ForIndex(0, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0]))
            .ForIndex(1, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1]))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            // Stubbing this out as testing headers in other methods
            releaseFileBlobService
                .Setup(s =>
                    s.StreamBlob(It.IsAny<ReleaseFile>(), null, default))
                .ReturnsAsync("csv_header".ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id)
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            var viewModelFilter0 = viewModel.Filters[filters[0].Name];
            var viewModelFilter1 = viewModel.Filters[filters[1].Name];
            var viewModelFilter2 = viewModel.Filters[filters[2].Name];

            Assert.Equal(3, viewModel.Filters.Count);
            Assert.Equal(filters[0].Id, viewModelFilter0.Id);
            Assert.Equal(filters[1].Id, viewModelFilter1.Id);
            Assert.Equal(filters[2].Id, viewModelFilter2.Id);

            var viewModelFilter0Items = viewModelFilter0.Items;
            Assert.Equal(2, viewModelFilter0Items.Count);
            Assert.Equal(filter0Items[0].Id, viewModelFilter0Items[filter0Items[0].Id].Id);
            Assert.Equal(filter0Items[1].Id, viewModelFilter0Items[filter0Items[1].Id].Id);

            var viewModelFilter1Items = viewModelFilter1.Items;
            Assert.Equal(2, viewModelFilter1Items.Count);
            Assert.Equal(filter1Items[0].Id, viewModelFilter1Items[filter1Items[0].Id].Id);
            Assert.Equal(filter1Items[1].Id, viewModelFilter1Items[filter1Items[1].Id].Id);

            var viewModelFilter2Items = viewModelFilter2.Items;
            Assert.Equal(2, viewModelFilter2Items.Count);
            Assert.Equal(filter2Items[0].Id, viewModelFilter2Items[filter2Items[0].Id].Id);
            Assert.Equal(filter2Items[1].Id, viewModelFilter2Items[filter2Items[1].Id].Id);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_OnlyIndicatorsFromQuery()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(1);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup(indicatorCount: 2)
            .GenerateList(3);

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToArray();

        var observations = _fixture.DefaultObservation()
            .WithLocation(_fixture.DefaultLocation())
            .WithFilterItems(filterItems)
            .WithMeasures(indicators)
            .GenerateList(2);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Filter.AddRangeAsync(filters);
            await statisticsDbContext.Indicator.AddRangeAsync(indicators);
            await statisticsDbContext.Observation.AddRangeAsync(observations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            // Stubbing this out as testing headers in other methods
            releaseFileBlobService
                .Setup(s =>
                    s.StreamBlob(It.IsAny<ReleaseFile>(), null, default))
                .ReturnsAsync("csv_header".ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            // Only indicators from the query will be included in the meta
            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators[1..2]
                    .Concat(indicators[4..6])
                    .Select(i => i.Id)
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            Assert.Equal(3, viewModel.Indicators.Count);
            Assert.Equal(indicators[1].Id, viewModel.Indicators[indicators[1].Name].Id);
            Assert.Equal(indicators[4].Id, viewModel.Indicators[indicators[4].Name].Id);
            Assert.Equal(indicators[5].Id, viewModel.Indicators[indicators[5].Name].Id);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_NoIndicatorsFromQuery()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(1);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup(indicatorCount: 2)
            .GenerateList(2);

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToArray();

        var observations = _fixture.DefaultObservation()
            .WithLocation(_fixture.DefaultLocation())
            .WithFilterItems(filterItems)
            .WithMeasures(indicators)
            .GenerateList(2);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            // Stubbing this out as testing headers in other methods
            releaseFileBlobService
                .Setup(
                    s =>
                        s.StreamBlob(It.IsAny<ReleaseFile>(), null, default)
                )
                .ReturnsAsync("csv_header".ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object
            );

            // Indicator ids don't match any in the database
            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = ListOf(Guid.NewGuid(), Guid.NewGuid())
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            // No indicators match, so none are in the meta
            Assert.Empty(viewModel.Indicators);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_OnlyLocationsWithObservations()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(1);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup(indicatorCount: 3)
            .GenerateList(1);

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToArray();

        var locations = _fixture.DefaultLocation()
            .ForRange(..2, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForRange(2..4, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateList();

        var observations = _fixture.DefaultObservation()
            .WithFilterItems(filterItems)
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetLocation(locations[0]))
            .ForRange(2..4, o => o
                .SetLocation(locations[3]))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            // Stubbing this out as testing headers in other methods
            releaseFileBlobService
                .Setup(s =>
                    s.StreamBlob(It.IsAny<ReleaseFile>(), null, default))
                .ReturnsAsync("csv_header".ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id)
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Locations.Count);
            Assert.Equal(locations[0].GetCsvValues(), viewModel.Locations[locations[0].Id]);
            Assert.Equal(locations[3].GetCsvValues(), viewModel.Locations[locations[3].Id]);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_EmptyObservations()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(1);

        var indicatorGroups = _fixture.DefaultIndicatorGroup(indicatorCount: 3)
            .GenerateList(1);

        var locations = _fixture.DefaultLocation()
            .ForRange(..2, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForRange(2..4, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            // Stubbing this out as testing headers in other methods
            releaseFileBlobService
                .Setup(s =>
                    s.StreamBlob(It.IsAny<ReleaseFile>(), null, default))
                .ReturnsAsync("csv_header".ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, new List<Observation>());

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            Assert.Empty(viewModel.Filters);
            Assert.Empty(viewModel.Indicators);
            Assert.Empty(viewModel.Locations);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_Headers()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(2);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1)))
            .ForIndex(1, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(2)))
            .GenerateList();

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToArray();

        var observations = _fixture.DefaultObservation()
            .ForInstance(o => o
                .SetMeasures(indicators)
                .SetFilterItems(filterItems[0], filterItems[1])
                .SetLocation(_fixture.DefaultLocation()
                    .WithPresetRegionAndLocalAuthority()
                    .WithGeographicLevel(GeographicLevel.LocalAuthority))
                .SetTimePeriod(2022, AcademicYear))
            .GenerateList(1);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Filter.AddRangeAsync(filters);
            await statisticsDbContext.Indicator.AddRangeAsync(indicators);
            await statisticsDbContext.Observation.AddRangeAsync(observations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_name",
                "country_code",
                "la_name",
                "new_la_code",
                "old_la_code",
                "region_name",
                "region_code",
                filters[1].Name,
                filters[0].Name,
                indicators[2].Name,
                indicators[0].Name,
                indicators[1].Name
            );

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            releaseFileBlobService
                .Setup(
                    s => s.StreamBlob(
                        It.Is<ReleaseFile>(
                            rf => rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId
                        ),
                        null,
                        default
                    )
                )
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id).ToList()
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            // Order of headers is dictated by the order of headers in the data file CSV.
            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_name",
                "country_code",
                "la_name",
                "new_la_code",
                "old_la_code",
                "region_name",
                "region_code",
                filters[1].Name,
                filters[0].Name,
                indicators[2].Name,
                indicators[0].Name,
                indicators[1].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_Headers_FiltersHaveNonDefaultGroups()
    {
        var filters = _fixture.DefaultFilter()
            .ForIndex(0, s =>
                s.SetGroupCsvColumn("filter_0_grouping")
                    .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1)
                        .ForInstance(s => s.Set(
                            fg => fg.Label,
                            (_, _, context) => $"Filter group {context.FixtureTypeIndex}"))
                        .Generate(2)))
            .ForIndex(1, s =>
                s.SetGroupCsvColumn("filter_1_grouping")
                    .SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1)
                        .ForInstance(s => s.Set(
                            fg => fg.Label,
                            (_, _, context) => $"Filter group {context.FixtureTypeIndex}"))
                        .Generate(2)))
            .ForIndex(2, s =>
                s.SetFilterGroups(_fixture.DefaultFilterGroup(filterItemCount: 1).Generate(1)))
            .GenerateList();

        var filter0Items = filters[0].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter1Items = filters[1].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter2Items = filters[2].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1)))
            .ForIndex(1, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(2)))
            .GenerateList();

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToArray();

        var location = _fixture.DefaultLocation()
            .WithPresetRegionAndLocalAuthority()
            .WithGeographicLevel(GeographicLevel.LocalAuthority)
            .Generate();

        var observations = _fixture.DefaultObservation()
            .WithMeasures(indicators)
            .ForIndex(0, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(location))
            .ForIndex(1, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[0])
                .SetLocation(location)
                .SetTimePeriod(2022, AcademicYear))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Filter.AddRangeAsync(filters);
            await statisticsDbContext.Indicator.AddRangeAsync(indicators);
            await statisticsDbContext.Observation.AddRangeAsync(observations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_name",
                "country_code",
                "la_name",
                "new_la_code",
                "old_la_code",
                "region_name",
                "region_code",
                filters[1].GroupCsvColumn,
                filters[0].GroupCsvColumn,
                filters[1].Name,
                filters[0].Name,
                filters[2].Name,
                indicators[2].Name,
                indicators[0].Name,
                indicators[1].Name
            );

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            releaseFileBlobService
                .Setup(
                    s => s.StreamBlob(
                        It.Is<ReleaseFile>(
                            rf => rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId
                        ),
                        null,
                        default
                    )
                )
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id).ToList()
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            // Order of headers is dictated by the order of headers in the data file CSV.
            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_name",
                "country_code",
                "la_name",
                "new_la_code",
                "old_la_code",
                "region_name",
                "region_code",
                filters[1].GroupCsvColumn!,
                filters[0].GroupCsvColumn!,
                filters[1].Name,
                filters[0].Name,
                filters[2].Name,
                indicators[2].Name,
                indicators[0].Name,
                indicators[1].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_Headers_OnlyIndicatorsFromQuery()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(1);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup(indicatorCount: 2)
            .GenerateList(2);

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToList();

        var observations = _fixture.DefaultObservation()
            .ForInstance(o => o
                .SetMeasures(indicators)
                .SetFilterItems(filterItems[0])
                .SetLocation(_fixture.DefaultLocation())
                .SetTimePeriod(2022, AcademicYear))
            .GenerateList(1);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                filters[0].Name,
                indicators[0].Name,
                indicators[1].Name,
                indicators[2].Name,
                indicators[3].Name
            );

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            releaseFileBlobService
                .Setup(
                    s => s.StreamBlob(
                        It.Is<ReleaseFile>(
                            rf => rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId
                        ),
                        null,
                        default
                    )
                )
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = ListOf(indicators[1].Id, indicators[3].Id)
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                filters[0].Name,
                // Only indicators specified in the query will be in the meta's CSV headers.
                indicators[1].Name,
                indicators[3].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_Headers_NoIndicatorsFromQuery()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(1);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup(indicatorCount: 2)
            .GenerateList(2);

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToList();

        var observations = _fixture.DefaultObservation()
            .ForInstance(o => o
                .SetMeasures(indicators)
                .SetFilterItems(filterItems[0])
                .SetLocation(_fixture.DefaultLocation())
                .SetTimePeriod(2022, AcademicYear))
            .GenerateList(1);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                filters[0].Name,
                indicators[0].Name,
                indicators[1].Name
            );

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            releaseFileBlobService
                .Setup(
                    s => s.StreamBlob(
                        It.Is<ReleaseFile>(
                            rf => rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId
                        ),
                        null,
                        default
                    )
                )
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            // Indicator ids don't matching any saved in the database
            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = ListOf(Guid.NewGuid(), Guid.NewGuid())
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                filters[0].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_Headers_DoNotMatchAnyMeta()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(1);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup(indicatorCount: 2)
            .GenerateList(1);

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToList();

        var observations = _fixture.DefaultObservation()
            .ForInstance(o => o
                .SetMeasures(indicators)
                .SetFilterItems(filterItems[0])
                .SetLocation(_fixture.DefaultLocation())
                .SetTimePeriod(2022, AcademicYear))
            .GenerateList(1);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            // CSV contains headers that don't correspond to any meta
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                filters[0].Name,
                "another_header",
                indicators[0].Name,
                indicators[1].Name,
                "last_header"
            );

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            releaseFileBlobService
                .Setup(
                    s => s.StreamBlob(
                        It.Is<ReleaseFile>(
                            rf => rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId
                        ),
                        null,
                        default
                    )
                )
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id)
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                filters[0].Name,
                indicators[0].Name,
                indicators[1].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_Headers_MultipleReleaseFilesExist()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(2);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1)))
            .ForIndex(1, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(2)))
            .GenerateList();

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToArray();

        var observations = _fixture.DefaultObservation()
            .ForInstance(o => o
                .SetMeasures(indicators)
                .SetFilterItems(filterItems[0], filterItems[1])
                .SetLocation(_fixture.DefaultLocation()
                    .WithPresetRegionAndLocalAuthority()
                    .WithGeographicLevel(GeographicLevel.LocalAuthority))
                .SetTimePeriod(2022, AcademicYear))
            .GenerateList(1);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var release = new Content.Model.Release
        {
            Id = releaseSubject.Release.Id
        };

        var releaseDataFile = new ReleaseFile
        {
            Release = release,
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        // Create a file for the subject and release which is not a data file
        var releaseMetadataFile = new ReleaseFile
        {
            Release = release,
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Metadata
            }
        };

        // Create a data file for the subject but for a different release
        var releaseDataFileOtherRelease = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = Guid.NewGuid()
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.Releases.AddRangeAsync(release);
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseDataFile,
                releaseMetadataFile,
                releaseDataFileOtherRelease);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Filter.AddRangeAsync(filters);
            await statisticsDbContext.Indicator.AddRangeAsync(indicators);
            await statisticsDbContext.Observation.AddRangeAsync(observations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_name",
                "country_code",
                "la_name",
                "new_la_code",
                "old_la_code",
                "region_name",
                "region_code",
                filters[1].Name,
                filters[0].Name,
                indicators[2].Name,
                indicators[0].Name,
                indicators[1].Name
            );

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            releaseFileBlobService
                .Setup(
                    s => s.StreamBlob(
                        It.Is<ReleaseFile>(
                            rf => rf.FileId == releaseDataFile.FileId && rf.ReleaseId == releaseDataFile.ReleaseId
                        ),
                        null,
                        default
                    )
                )
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id).ToList()
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            // Order of headers is dictated by the order of headers in the data file CSV.
            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_name",
                "country_code",
                "la_name",
                "new_la_code",
                "old_la_code",
                "region_name",
                "region_code",
                filters[1].Name,
                filters[0].Name,
                indicators[2].Name,
                indicators[0].Name,
                indicators[1].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_Headers_ReleaseFileNotFound()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(2);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1)))
            .ForIndex(1, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(2)))
            .GenerateList();

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToList();

        var observations = _fixture.DefaultObservation()
            .ForInstance(o => o
                .SetMeasures(indicators)
                .SetFilterItems(filterItems[0], filterItems[1])
                .SetLocation(_fixture.DefaultLocation()
                    .WithPresetRegionAndLocalAuthority()
                    .WithGeographicLevel(GeographicLevel.LocalAuthority))
                .SetTimePeriod(2022, AcademicYear))
            .GenerateList(1);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id).ToList()
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            var viewModel = result.AssertRight();

            // No release file found. Fallback to a default ordering of headers.
            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                "region_code",
                "region_name",
                "new_la_code",
                "old_la_code",
                "la_name",
                filters[0].Name,
                filters[1].Name,
                indicators[0].Name,
                indicators[1].Name,
                indicators[2].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetSubjectCsvMeta_Headers_BlobNotFound()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(2);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1)))
            .ForIndex(1, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(2)))
            .GenerateList();

        var indicators = indicatorGroups
            .SelectMany(ig => ig.Indicators)
            .ToList();

        var observations = _fixture.DefaultObservation()
            .ForInstance(o => o
                .SetMeasures(indicators)
                .SetFilterItems(filterItems[0], filterItems[1])
                .SetLocation(_fixture.DefaultLocation()
                    .WithPresetRegionAndLocalAuthority()
                    .WithGeographicLevel(GeographicLevel.LocalAuthority))
                .SetTimePeriod(2022, AcademicYear))
            .GenerateList(1);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
                .WithObservations(observations),
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = releaseSubject.Subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            // Release file is saved and can be found.
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            // Blob for release file does not exist in storage
            releaseFileBlobService
                .Setup(
                    s => s.StreamBlob(
                        It.Is<ReleaseFile>(
                            rf => rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId
                        ),
                        null,
                        default
                    )
                )
                .ThrowsAsync(new FileNotFoundException("File not found"));

            var service = BuildService(
                statisticsDbContext: statisticsDbContext,
                contentDbContext: contentDbContext,
                releaseFileBlobService: releaseFileBlobService.Object);

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId,
                Indicators = indicators.Select(i => i.Id).ToList()
            };

            var result =
                await service.GetSubjectCsvMeta(releaseSubject, query, observations);

            VerifyAllMocks(releaseFileBlobService);

            var viewModel = result.AssertRight();

            // No release file blob found. Fallback to a default ordering of headers.
            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                "region_code",
                "region_name",
                "new_la_code",
                "old_la_code",
                "la_name",
                filters[0].Name,
                filters[1].Name,
                indicators[0].Name,
                indicators[1].Name,
                indicators[2].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    private static void AssertFilterCsvViewModel(Filter filter, FilterCsvMetaViewModel viewModel)
    {
        Assert.Equal(filter.Id, viewModel.Id);
        Assert.Equal(filter.Name, viewModel.Name);
        Assert.Equal(filter.GroupCsvColumn, viewModel.GroupCsvColumn);
    }

    private static void AssertFilterItemCsvViewModel(FilterItem filterItem, FilterItemCsvMetaViewModel viewModel)
    {
        Assert.Equal(filterItem.Id, viewModel.Id);
        Assert.Equal(filterItem.Label, viewModel.Label);
        Assert.Equal(filterItem.FilterGroup.Label, viewModel.GroupLabel);
    }

    private static void AssertIndicatorCsvMetaViewModel(Indicator indicator, IndicatorCsvMetaViewModel viewModel)
    {
        Assert.Equal(indicator.Id, viewModel.Id);
        Assert.Equal(indicator.Name, viewModel.Name);
        Assert.Equal(indicator.Label, viewModel.Label);
        Assert.Equal(indicator.DecimalPlaces, viewModel.DecimalPlaces);
        Assert.Equal(indicator.Unit, viewModel.Unit);
    }

    private static SubjectCsvMetaService BuildService(
        StatisticsDbContext statisticsDbContext,
        ContentDbContext contentDbContext,
        IReleaseFileBlobService? releaseFileBlobService = null)
    {
        return new SubjectCsvMetaService(
            logger: Mock.Of<ILogger<SubjectCsvMetaService>>(),
            statisticsDbContext: statisticsDbContext,
            contentDbContext: contentDbContext,
            userService: AlwaysTrueUserService().Object,
            filterItemRepository: new FilterItemRepository(statisticsDbContext),
            releaseFileBlobService: releaseFileBlobService ?? Mock.Of<IReleaseFileBlobService>(Strict)
        );
    }
}
