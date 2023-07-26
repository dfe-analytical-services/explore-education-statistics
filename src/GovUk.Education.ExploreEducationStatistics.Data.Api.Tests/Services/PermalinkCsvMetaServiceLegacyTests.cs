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
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services;

// TODO EES-3755 Remove after Permalink snapshot migration work is complete
public class PermalinkCsvMetaServiceLegacyTests
{
    private readonly DataFixture _fixture = new();

    private readonly Dictionary<GeographicLevel, List<string>> _regionLocalAuthorityHierarchy = new()
    {
        {
            GeographicLevel.LocalAuthority, CollectionUtils.ListOf(GeographicLevel.Region.ToString())
        }
    };

    [Fact]
    public async Task GetCsvMeta_WithLocationIds_TableResultMetaHasNoFilterGroupCsvColumnValues()
    {
        // Set up a test where the permalink table result meta filters don't have group csv column values.
        // This is the case for all permalinks generated before EES-4363 and EES-4364.
        // Values are required to generate the csv for the filters with non-default groups, so need to be fetched
        // from the database when building the csv meta.

        var subject = _fixture.DefaultSubject().Generate();

        // Create a subject with 3 filters, 2 of which have non-default groups
        var filters = _fixture.DefaultFilter()
            .WithSubject(subject)
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
            .WithSubject(subject)
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1))
            )
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
            .GenerateList(4);

        var observations = _fixture.DefaultObservation()
            .WithSubject(subject)
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(locations[0])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(2..4, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(locations[1])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(4..6, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                .SetLocation(locations[2])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .ForRange(6..8, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                .SetLocation(locations[3])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .GenerateList(8);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = subject
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(MockBehavior.Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            // Ordering of CSV headers in meta should reflect the
            // column ordering in the original data file's header.
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
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

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);

            releaseFileBlobService
                .Setup(s => s.StreamBlob(
                    It.Is<ReleaseFile>(rf =>
                        rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId),
                    null,
                    default
                ))
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object,
                releaseFileBlobService: releaseFileBlobService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id
            };

            // Build the permalink without the filter GroupCsvColumn values in the table result meta
            var permalink = BuildLegacyPermalink(
                query: query,
                observations: observations,
                indicators: indicators,
                filters: filters,
                locations: locations,
                includeFilterGroupCsvColumnsInSubjectMeta: false
            );

            Assert.All(permalink.FullTable.SubjectMeta.Filters, pair => Assert.Null(pair.Value.GroupCsvColumn));

            var result = await service.GetCsvMeta(permalink);

            MockUtils.VerifyAllMocks(releaseSubjectService, releaseFileBlobService);

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

            Assert.Equal(4, viewModel.Locations.Count);
            Assert.Equal(locations[0].GetCsvValues(), viewModel.Locations[locations[0].Id]);
            Assert.Equal(locations[1].GetCsvValues(), viewModel.Locations[locations[1].Id]);
            Assert.Equal(locations[2].GetCsvValues(), viewModel.Locations[locations[2].Id]);
            Assert.Equal(locations[3].GetCsvValues(), viewModel.Locations[locations[3].Id]);

            // Ordering of headers is same as original data file.
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
    public async Task GetCsvMeta_WithLocationIds_SubjectMetaHasNoLocationIds()
    {
        // Set up a test where the observations have location id's but the subject meta locations do not.
        // The changes to add location id's to each were made at different times and there was a period where
        // the subject meta locations did not have location id's, but the observations had location id's as well as
        // locations.

        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
            .WithSubject(subject)
            .GenerateList(2);

        var filter0Items = filters[0].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter1Items = filters[1].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .WithSubject(subject)
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1))
            )
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
            .GenerateList(4);

        var observations = _fixture.DefaultObservation()
            .WithSubject(subject)
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0])
                .SetLocation(locations[0])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(2..4, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0])
                .SetLocation(locations[1])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(4..6, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1])
                .SetLocation(locations[2])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .ForRange(6..8, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1])
                .SetLocation(locations[3])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .GenerateList(8);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = subject
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(MockBehavior.Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);

            releaseFileBlobService
                .Setup(s => s.StreamBlob(
                    It.Is<ReleaseFile>(rf =>
                        rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId),
                    null,
                    default
                ))
                .ThrowsAsync(new FileNotFoundException("File not found"));

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object,
                releaseFileBlobService: releaseFileBlobService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id
            };

            // Build the permalink using observations that have location id's AND locations,
            // but subject meta without location ids
            var permalink = BuildLegacyPermalink(
                query: query,
                observations: observations,
                indicators: indicators,
                filters: filters,
                locations: locations,
                observationViewModelStrategy: ObservationViewModelTestBuildStrategy.WithLocationIdAndLocationObject,
                includeLocationsIdsInSubjectMeta: false
            );

            var result = await service.GetCsvMeta(permalink);

            MockUtils.VerifyAllMocks(releaseSubjectService, releaseFileBlobService);

            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Filters.Count);

            var viewModelFilter0 = viewModel.Filters[filters[0].Name];
            var viewModelFilter1 = viewModel.Filters[filters[1].Name];

            AssertFilterCsvViewModel(filters[0], viewModelFilter0);
            AssertFilterCsvViewModel(filters[1], viewModelFilter1);

            var viewModelFilter0Items = viewModelFilter0.Items;
            Assert.Equal(2, viewModelFilter0Items.Count);

            AssertFilterItemCsvViewModel(filter0Items[0], viewModelFilter0Items[filter0Items[0].Id]);
            AssertFilterItemCsvViewModel(filter0Items[1], viewModelFilter0Items[filter0Items[1].Id]);

            var viewModelFilter1Items = viewModelFilter1.Items;
            Assert.Equal(2, viewModelFilter1Items.Count);

            AssertFilterItemCsvViewModel(filter1Items[0], viewModelFilter1Items[filter1Items[0].Id]);
            AssertFilterItemCsvViewModel(filter1Items[1], viewModelFilter1Items[filter1Items[1].Id]);

            Assert.Equal(3, viewModel.Indicators.Count);
            AssertIndicatorCsvMetaViewModel(indicators[0], viewModel.Indicators[indicators[0].Name]);
            AssertIndicatorCsvMetaViewModel(indicators[1], viewModel.Indicators[indicators[1].Name]);
            AssertIndicatorCsvMetaViewModel(indicators[2], viewModel.Indicators[indicators[2].Name]);

            Assert.Empty(viewModel.Locations);

            // Headers are in a default ordering.
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
                "la_name",
                filters[0].Name,
                filters[1].Name,
                indicators[0].Name,
                indicators[1].Name,
                indicators[2].Name
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetCsvMeta_WithLocationIds()
    {
        // We don't have any remaining variations of unit tests for a LegacyPermalink
        // *with* location id's after this one, because after checking for the presence of location id's,
        // the functionality should be the same internally as
        // GetCsvMeta(Guid subjectId, SubjectResultMetaViewModel tableResultMeta, CancellationToken cancellationToken)
        // which is unit tested in PermalinkCsvMetaServiceTests.

        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter()
            .WithSubject(subject)
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
            .WithSubject(subject)
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1))
            )
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
            .GenerateList(4);

        var observations = _fixture.DefaultObservation()
            .WithSubject(subject)
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(locations[0])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(2..4, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(locations[1])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(4..6, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                .SetLocation(locations[2])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .ForRange(6..8, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                .SetLocation(locations[3])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .GenerateList(8);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = subject
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(MockBehavior.Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            // Ordering of CSV headers in meta should reflect the
            // column ordering in the original data file's header.
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
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

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);

            releaseFileBlobService
                .Setup(s => s.StreamBlob(
                    It.Is<ReleaseFile>(rf =>
                        rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId),
                    null,
                    default
                ))
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object,
                releaseFileBlobService: releaseFileBlobService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id
            };

            var permalink = BuildLegacyPermalink(
                query: query,
                observations: observations,
                indicators: indicators,
                filters: filters,
                locations: locations
            );

            var result = await service.GetCsvMeta(permalink);

            MockUtils.VerifyAllMocks(releaseSubjectService, releaseFileBlobService);

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

            Assert.Equal(4, viewModel.Locations.Count);
            Assert.Equal(locations[0].GetCsvValues(), viewModel.Locations[locations[0].Id]);
            Assert.Equal(locations[1].GetCsvValues(), viewModel.Locations[locations[1].Id]);
            Assert.Equal(locations[2].GetCsvValues(), viewModel.Locations[locations[2].Id]);
            Assert.Equal(locations[3].GetCsvValues(), viewModel.Locations[locations[3].Id]);

            // Ordering of headers is same as original data file.
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
    public async Task GetCsvMeta_WithoutLocationIds_ReleaseSubjectAndBlobExists_HeadersInDataFileOrder()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter()
            .WithSubject(subject)
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
            .WithSubject(subject)
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1))
            )
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
            .GenerateList(4);

        var observations = _fixture.DefaultObservation()
            .WithSubject(subject)
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(locations[0])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(2..4, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0], filter2Items[0])
                .SetLocation(locations[1])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(4..6, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                .SetLocation(locations[2])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .ForRange(6..8, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1], filter2Items[1])
                .SetLocation(locations[3])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .GenerateList(8);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = subject
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(MockBehavior.Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            // Ordering of CSV headers in meta should reflect the
            // column ordering in the original data file's header.
            var csv = string.Join(
                ',',
                "time_period",
                "time_identifier",
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

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);

            releaseFileBlobService
                .Setup(s => s.StreamBlob(
                    It.Is<ReleaseFile>(rf =>
                        rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId),
                    null,
                    default
                ))
                .ReturnsAsync(csv.ToStream());

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object,
                releaseFileBlobService: releaseFileBlobService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id
            };

            // Build the permalink using observations and subject meta without location ids
            var permalink = BuildLegacyPermalink(
                query: query,
                observations: observations,
                indicators: indicators,
                filters: filters,
                locations: locations,
                observationViewModelStrategy: ObservationViewModelTestBuildStrategy.WithLocationObjectOnly,
                includeLocationsIdsInSubjectMeta: false
            );

            var result = await service.GetCsvMeta(permalink);

            MockUtils.VerifyAllMocks(releaseSubjectService, releaseFileBlobService);

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

            Assert.Empty(viewModel.Locations);

            // Ordering of headers is same as original data file.
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
    public async Task GetCsvMeta_WithoutLocationIds_ReleaseSubjectNotFound_HeadersInDefaultOrder()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
            .WithSubject(subject)
            .GenerateList(2);

        var filter0Items = filters[0].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter1Items = filters[1].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .WithSubject(subject)
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1))
            )
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
            .GenerateList(4);

        var observations = _fixture.DefaultObservation()
            .WithSubject(subject)
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0])
                .SetLocation(locations[0])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(2..4, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0])
                .SetLocation(locations[1])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(4..6, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1])
                .SetLocation(locations[2])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .ForRange(6..8, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1])
                .SetLocation(locations[3])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .GenerateList(8);

        var contextId = Guid.NewGuid().ToString();
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(MockBehavior.Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subject.Id))
                .ReturnsAsync((ReleaseSubject?) null);

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id
            };

            // Build the permalink using observations and subject meta without location ids
            var permalink = BuildLegacyPermalink(
                query: query,
                observations: observations,
                indicators: indicators,
                filters: filters,
                locations: locations,
                observationViewModelStrategy: ObservationViewModelTestBuildStrategy.WithLocationObjectOnly,
                includeLocationsIdsInSubjectMeta: false
            );

            var result = await service.GetCsvMeta(permalink);

            MockUtils.VerifyAllMocks(releaseSubjectService);

            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Filters.Count);

            var viewModelFilter0 = viewModel.Filters[filters[0].Name];
            var viewModelFilter1 = viewModel.Filters[filters[1].Name];

            AssertFilterCsvViewModel(filters[0], viewModelFilter0);
            AssertFilterCsvViewModel(filters[1], viewModelFilter1);

            var viewModelFilter0Items = viewModelFilter0.Items;
            Assert.Equal(2, viewModelFilter0Items.Count);

            AssertFilterItemCsvViewModel(filter0Items[0], viewModelFilter0Items[filter0Items[0].Id]);
            AssertFilterItemCsvViewModel(filter0Items[1], viewModelFilter0Items[filter0Items[1].Id]);

            var viewModelFilter1Items = viewModelFilter1.Items;
            Assert.Equal(2, viewModelFilter1Items.Count);

            AssertFilterItemCsvViewModel(filter1Items[0], viewModelFilter1Items[filter1Items[0].Id]);
            AssertFilterItemCsvViewModel(filter1Items[1], viewModelFilter1Items[filter1Items[1].Id]);

            Assert.Equal(3, viewModel.Indicators.Count);
            AssertIndicatorCsvMetaViewModel(indicators[0], viewModel.Indicators[indicators[0].Name]);
            AssertIndicatorCsvMetaViewModel(indicators[1], viewModel.Indicators[indicators[1].Name]);
            AssertIndicatorCsvMetaViewModel(indicators[2], viewModel.Indicators[indicators[2].Name]);

            Assert.Empty(viewModel.Locations);

            // Headers are in a default ordering.
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
                "la_name",
                filters[0].Name,
                filters[1].Name,
                indicators[0].Name,
                indicators[1].Name,
                indicators[2].Name
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetCsvMeta_WithoutLocationIds_BlobNotFound_HeadersInDefaultOrder()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
            .WithSubject(subject)
            .GenerateList(2);

        var filter0Items = filters[0].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var filter1Items = filters[1].FilterGroups
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .WithSubject(subject)
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator().Generate(1))
            )
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
            .GenerateList(4);

        var observations = _fixture.DefaultObservation()
            .WithSubject(subject)
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0])
                .SetLocation(locations[0])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(2..4, o => o
                .SetFilterItems(filter0Items[0], filter1Items[0])
                .SetLocation(locations[1])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(4..6, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1])
                .SetLocation(locations[2])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .ForRange(6..8, o => o
                .SetFilterItems(filter0Items[1], filter1Items[1])
                .SetLocation(locations[3])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .GenerateList(8);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = subject
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(MockBehavior.Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);

            releaseFileBlobService
                .Setup(s => s.StreamBlob(
                    It.Is<ReleaseFile>(rf =>
                        rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId),
                    null,
                    default
                ))
                .ThrowsAsync(new FileNotFoundException("File not found"));

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object,
                releaseFileBlobService: releaseFileBlobService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id
            };

            // Build the permalink using observations and subject meta without location ids
            var permalink = BuildLegacyPermalink(
                query: query,
                observations: observations,
                indicators: indicators,
                filters: filters,
                locations: locations,
                observationViewModelStrategy: ObservationViewModelTestBuildStrategy.WithLocationObjectOnly,
                includeLocationsIdsInSubjectMeta: false
            );

            var result = await service.GetCsvMeta(permalink);

            MockUtils.VerifyAllMocks(releaseSubjectService, releaseFileBlobService);

            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Filters.Count);

            var viewModelFilter0 = viewModel.Filters[filters[0].Name];
            var viewModelFilter1 = viewModel.Filters[filters[1].Name];

            AssertFilterCsvViewModel(filters[0], viewModelFilter0);
            AssertFilterCsvViewModel(filters[1], viewModelFilter1);

            var viewModelFilter0Items = viewModelFilter0.Items;
            Assert.Equal(2, viewModelFilter0Items.Count);

            AssertFilterItemCsvViewModel(filter0Items[0], viewModelFilter0Items[filter0Items[0].Id]);
            AssertFilterItemCsvViewModel(filter0Items[1], viewModelFilter0Items[filter0Items[1].Id]);

            var viewModelFilter1Items = viewModelFilter1.Items;
            Assert.Equal(2, viewModelFilter1Items.Count);

            AssertFilterItemCsvViewModel(filter1Items[0], viewModelFilter1Items[filter1Items[0].Id]);
            AssertFilterItemCsvViewModel(filter1Items[1], viewModelFilter1Items[filter1Items[1].Id]);

            Assert.Equal(3, viewModel.Indicators.Count);
            AssertIndicatorCsvMetaViewModel(indicators[0], viewModel.Indicators[indicators[0].Name]);
            AssertIndicatorCsvMetaViewModel(indicators[1], viewModel.Indicators[indicators[1].Name]);
            AssertIndicatorCsvMetaViewModel(indicators[2], viewModel.Indicators[indicators[2].Name]);

            Assert.Empty(viewModel.Locations);

            // Headers are in a default ordering.
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
                "la_name",
                filters[0].Name,
                filters[1].Name,
                indicators[0].Name,
                indicators[1].Name,
                indicators[2].Name
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetCsvMeta_WithoutLocationIds_FilterAndIndicatorViewModelsHaveNoName()
    {
        // This test covers a scenario for Permalinks created before EES-613.
        // Prior to EES-613, the FilterMetaViewModel and IndicatorMetaViewModel had no Name property.

        // We generate filter and indicator test data as normal.
        // When setting up the view models for the Permalink we set the Name property of the view model to null.

        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
            .WithSubject(subject)
            .ForIndex(0, s => s.SetLabel("Filter 0"))
            .ForIndex(1, s => s.SetLabel("Filter 1"))
            .GenerateList(2);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
            .WithSubject(subject)
            .ForIndex(0, ig => ig
                .SetIndicators(_fixture.DefaultIndicator()
                    .WithLabel("Indicator 0")
                    .Generate(1))
            )
            .ForIndex(1, ig => ig
                .SetIndicators(_fixture.DefaultIndicator()
                    .ForIndex(0, s => s.SetLabel("Indicator 1"))
                    .ForIndex(1, s => s.SetLabel("Indicator 2"))
                    .Generate(2))
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
            .GenerateList(4);

        var observations = _fixture.DefaultObservation()
            .WithSubject(subject)
            .WithMeasures(indicators)
            .ForRange(..2, o => o
                .SetFilterItems(filterItems[0], filterItems[2])
                .SetLocation(locations[0])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(2..4, o => o
                .SetFilterItems(filterItems[0], filterItems[2])
                .SetLocation(locations[1])
                .SetTimePeriod(2022, TimeIdentifier.AcademicYear))
            .ForRange(4..6, o => o
                .SetFilterItems(filterItems[1], filterItems[3])
                .SetLocation(locations[2])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .ForRange(6..8, o => o
                .SetFilterItems(filterItems[1], filterItems[3])
                .SetLocation(locations[3])
                .SetTimePeriod(2023, TimeIdentifier.AcademicYear))
            .GenerateList(8);

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = subject
        };

        var releaseFile = new ReleaseFile
        {
            Release = new Content.Model.Release
            {
                Id = releaseSubject.Release.Id,
            },
            File = new File
            {
                SubjectId = subject.Id,
                Type = FileType.Data
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();

            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = ContentDbUtils.InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(MockBehavior.Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subject.Id))
                .ReturnsAsync(releaseSubject);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(MockBehavior.Strict);

            releaseFileBlobService
                .Setup(s => s.StreamBlob(
                    It.Is<ReleaseFile>(rf =>
                        rf.FileId == releaseFile.FileId && rf.ReleaseId == releaseFile.ReleaseId),
                    null,
                    default
                ))
                .ThrowsAsync(new FileNotFoundException("File not found"));

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object,
                releaseFileBlobService: releaseFileBlobService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subject.Id
            };

            // Build the permalink using observations and subject meta without filter and indicator names
            var permalink = BuildLegacyPermalink(
                query: query,
                observations: observations,
                indicators: indicators,
                filters: filters,
                locations: locations,
                observationViewModelStrategy: ObservationViewModelTestBuildStrategy.WithLocationObjectOnly,
                includeLocationsIdsInSubjectMeta: false,
                includeFilterAndIndicatorNamesInSubjectMeta: false
            );

            Assert.All(permalink.FullTable.SubjectMeta.Filters, pair => Assert.Null(pair.Value.Name));
            Assert.All(permalink.FullTable.SubjectMeta.Indicators, indicator => Assert.Null(indicator.Name));

            var result = await service.GetCsvMeta(permalink);

            MockUtils.VerifyAllMocks(releaseSubjectService, releaseFileBlobService);

            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Filters.Count);

            // Expect filter view models to have names based on snake case of filter names
            const string expectedViewModelFilter0Name = "filter_0";
            const string expectedViewModelFilter1Name = "filter_1";

            var viewModelFilter0 = viewModel.Filters[expectedViewModelFilter0Name];
            var viewModelFilter1 = viewModel.Filters[expectedViewModelFilter1Name];

            Assert.Equal(expectedViewModelFilter0Name, viewModelFilter0.Name);
            Assert.Equal(expectedViewModelFilter1Name, viewModelFilter1.Name);

            Assert.Equal(3, viewModel.Indicators.Count);

            // Expect indicator view models to have names based on snake case of indicator names
            const string expectedViewModelIndicator0Name = "indicator_0";
            const string expectedViewModelIndicator1Name = "indicator_1";
            const string expectedViewModelIndicator2Name = "indicator_2";

            var viewModelIndicator0 = viewModel.Indicators[expectedViewModelIndicator0Name];
            var viewModelIndicator1 = viewModel.Indicators[expectedViewModelIndicator1Name];
            var viewModelIndicator2 = viewModel.Indicators[expectedViewModelIndicator2Name];

            Assert.Equal(expectedViewModelIndicator0Name, viewModelIndicator0.Name);
            Assert.Equal(expectedViewModelIndicator1Name, viewModelIndicator1.Name);
            Assert.Equal(expectedViewModelIndicator2Name, viewModelIndicator2.Name);

            // Filters/indicator columns have the expected names
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
                "la_name",
                expectedViewModelFilter0Name,
                expectedViewModelFilter1Name,
                expectedViewModelIndicator0Name,
                expectedViewModelIndicator1Name,
                expectedViewModelIndicator2Name
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    private LegacyPermalink BuildLegacyPermalink(ObservationQueryContext query,
        IReadOnlyList<Observation> observations,
        IReadOnlyList<Indicator> indicators,
        IReadOnlyList<Filter> filters,
        IList<Location> locations,
        ObservationViewModelTestBuildStrategy observationViewModelStrategy = ObservationViewModelTestBuildStrategy.WithLocationIdOnly,
        bool includeLocationsIdsInSubjectMeta = true,
        bool includeFilterAndIndicatorNamesInSubjectMeta = true,
        bool includeFilterGroupCsvColumnsInSubjectMeta = true)
    {
        var filterViewModels = FiltersMetaViewModelBuilder.BuildFilters(filters);
        var indicatorViewModels = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators);

        if (!includeFilterAndIndicatorNamesInSubjectMeta)
        {
            filterViewModels = filterViewModels.ToDictionary(
                pair => pair.Key,
                pair => pair.Value with
                {
                    Name = null
                });

            indicatorViewModels = indicatorViewModels
                .Select(viewModel => viewModel with
                {
                    Name = null
                })
                .ToList();
        }

        if (!includeFilterGroupCsvColumnsInSubjectMeta)
        {
            filterViewModels = filterViewModels.ToDictionary(
                pair => pair.Key,
                pair => pair.Value with
                {
                    GroupCsvColumn = null
                });
        }

        return new LegacyPermalink
        {
            Query = query,
            FullTable = new PermalinkTableBuilderResult
            {
                Results = observations
                    .Select(observation =>
                        ObservationViewModelBuilderTestUtils.BuildObservationViewModel(
                            observation: observation,
                            indicators: indicators,
                            buildStrategy: observationViewModelStrategy)
                    )
                    .ToList(),
                SubjectMeta = new PermalinkResultSubjectMeta
                {
                    Filters = filterViewModels,
                    Indicators = indicatorViewModels,
                    LocationsHierarchical = LocationViewModelBuilderTestUtils
                        .BuildLocationAttributeViewModels(
                            locations: locations,
                            hierarchies: _regionLocalAuthorityHierarchy,
                            includeLocationIds: includeLocationsIdsInSubjectMeta)
                        .ToDictionary(
                            level => level.Key.ToString().CamelCase(),
                            level => level.Value)
                }
            }
        };
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

    private static PermalinkCsvMetaService BuildService(
        ContentDbContext contentDbContext,
        StatisticsDbContext statisticsDbContext,
        IReleaseSubjectService? releaseSubjectService = null,
        IReleaseFileBlobService? releaseFileBlobService = null)
    {
        return new PermalinkCsvMetaService(
            logger: Mock.Of<ILogger<PermalinkCsvMetaService>>(),
            contentDbContext: contentDbContext,
            statisticsDbContext: statisticsDbContext,
            releaseSubjectService: releaseSubjectService ?? Mock.Of<IReleaseSubjectService>(MockBehavior.Strict),
            releaseFileBlobService: releaseFileBlobService ?? Mock.Of<IReleaseFileBlobService>(MockBehavior.Strict)
        );
    }
}
