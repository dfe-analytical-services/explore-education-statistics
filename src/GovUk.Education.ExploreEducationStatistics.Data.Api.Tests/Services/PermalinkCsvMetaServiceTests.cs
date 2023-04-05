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
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Services;

public class PermalinkCsvMetaServiceTests
{
    private readonly DataFixture _fixture = new();

    private readonly Dictionary<GeographicLevel, List<string>> _regionLocalAuthorityHierarchy = new()
    {
        {
            GeographicLevel.LocalAuthority, ListOf(GeographicLevel.Region.ToString())
        }
    };

    [Fact]
    public async Task GetCsvMeta_SubjectNotFound()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
            .WithSubject(subject)
            .GenerateList(2);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicators = _fixture.DefaultIndicator()
            .ForIndex(0, i => i
                .SetIndicatorGroup(_fixture.DefaultIndicatorGroup()
                    .WithSubject(subject))
            )
            .ForRange(1..3, i => i
                .SetIndicatorGroup(_fixture.DefaultIndicatorGroup()
                    .WithSubject(subject))
            )
            .GenerateList();

        var locations = _fixture.DefaultLocation()
            .WithGeographicLevel(GeographicLevel.Country)
            .GenerateList(1);

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var subjectId = Guid.NewGuid();

            var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subjectId))
                .ReturnsAsync((ReleaseSubject?)null);

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subjectId
            };

            var permalink = new LegacyPermalink
            {
                Query = query,
                FullTable = new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta
                    {
                        Filters = FiltersMetaViewModelBuilder.BuildFilters(filters),
                        Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                        LocationsHierarchical = LocationViewModelBuilder
                            .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                            .ToDictionary(
                                level => level.Key.ToString().CamelCase(),
                                level => level.Value)
                    }
                }
            };

            var result = await service.GetCsvMeta(permalink);

            VerifyAllMocks(releaseSubjectService);

            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Filters.Count);

            var viewModelFilter0 = viewModel.Filters[filters[0].Name];
            var viewModelFilter1 = viewModel.Filters[filters[1].Name];

            Assert.Equal(filters[0].Id, viewModelFilter0.Id);
            Assert.Equal(filters[0].Name, viewModelFilter0.Name);
            Assert.Equal(filters[1].Id, viewModelFilter1.Id);
            Assert.Equal(filters[1].Name, viewModelFilter1.Name);

            var viewModelFilter0Items = viewModelFilter0.Items;
            Assert.Equal(2, viewModelFilter0Items.Count);

            var viewModelFilter0Item0 = viewModelFilter0Items[filterItems[0].Id];
            var viewModelFilter0Item1 = viewModelFilter0Items[filterItems[1].Id];

            Assert.Equal(filterItems[0].Id, viewModelFilter0Item0.Id);
            Assert.Equal(filterItems[0].Label, viewModelFilter0Item0.Label);
            Assert.Equal(filterItems[1].Id, viewModelFilter0Item1.Id);
            Assert.Equal(filterItems[1].Label, viewModelFilter0Item1.Label);

            var viewModelFilter1Items = viewModelFilter1.Items;
            Assert.Equal(2, viewModelFilter1Items.Count);

            var viewModelFilter1Item0 = viewModelFilter1Items[filterItems[2].Id];
            var viewModelFilter1Item1 = viewModelFilter1Items[filterItems[3].Id];

            Assert.Equal(filterItems[2].Id, viewModelFilter1Item0.Id);
            Assert.Equal(filterItems[2].Label, viewModelFilter1Item0.Label);
            Assert.Equal(filterItems[3].Id, viewModelFilter1Item1.Id);
            Assert.Equal(filterItems[3].Label, viewModelFilter1Item1.Label);

            Assert.Equal(3, viewModel.Indicators.Count);
            AssertIndicatorCsvViewModel(indicators[0], viewModel.Indicators[indicators[0].Name]);
            AssertIndicatorCsvViewModel(indicators[1], viewModel.Indicators[indicators[1].Name]);
            AssertIndicatorCsvViewModel(indicators[2], viewModel.Indicators[indicators[2].Name]);

            Assert.Single(viewModel.Locations);
            Assert.Equal(locations[0].GetCsvValues(), viewModel.Locations[locations[0].Id]);

            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
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
    public async Task GetCsvMeta_SubjectNotFound_LocationsExist()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
            .WithSubject(subject)
            .GenerateList(1);

        var indicators = _fixture.DefaultIndicator()
            .ForInstance(i => i
                .SetIndicatorGroup(_fixture.DefaultIndicatorGroup()
                    .WithSubject(subject))
            )
            .GenerateList(1);

        var locations = _fixture.DefaultLocation()
            .ForRange(..2, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForRange(2..4, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateList();

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var subjectId = Guid.NewGuid();

            var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subjectId))
                .ReturnsAsync((ReleaseSubject?)null);

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subjectId
            };

            var permalink = new LegacyPermalink
            {
                Query = query,
                FullTable = new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta
                    {
                        Filters = FiltersMetaViewModelBuilder.BuildFilters(filters),
                        Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                        LocationsHierarchical = LocationViewModelBuilder
                            .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                            .ToDictionary(
                                level => level.Key.ToString().CamelCase(),
                                level => level.Value)
                    }
                }
            };

            var result = await service.GetCsvMeta(permalink);

            VerifyAllMocks(releaseSubjectService);

            var viewModel = result.AssertRight();

            Assert.Equal(4, viewModel.Locations.Count);
            Assert.Equal(locations[0].GetCsvValues(),viewModel.Locations[locations[0].Id]);
            Assert.Equal(locations[1].GetCsvValues(),viewModel.Locations[locations[1].Id]);
            Assert.Equal(locations[2].GetCsvValues(),viewModel.Locations[locations[2].Id]);
            Assert.Equal(locations[3].GetCsvValues(),viewModel.Locations[locations[3].Id]);

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
                indicators[0].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetCsvMeta_SubjectNotFound_LocationsMissing()
    {
        var subject = _fixture.DefaultSubject().Generate();

        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
            .WithSubject(subject)
            .GenerateList(1);

        var indicators = _fixture.DefaultIndicator()
            .ForInstance(i => i
                .SetIndicatorGroup(_fixture.DefaultIndicatorGroup()
                    .WithSubject(subject))
            )
            .GenerateList(1);

        var locations = _fixture.DefaultLocation()
            .ForRange(..2, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForRange(2..3, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateArray();

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            // Only save some of the locations into the database.
            await statisticsDbContext.Location.AddRangeAsync(locations[0]);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var subjectId = Guid.NewGuid();

            var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(subjectId))
                .ReturnsAsync((ReleaseSubject?)null);

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = subjectId
            };

            var permalink = new LegacyPermalink
            {
                Query = query,
                FullTable = new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta
                    {
                        Filters = FiltersMetaViewModelBuilder.BuildFilters(filters),
                        Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                        LocationsHierarchical = LocationViewModelBuilder
                            .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                            .ToDictionary(
                                level => level.Key.ToString().CamelCase(),
                                level => level.Value)
                    }
                }
            };

            var result = await service.GetCsvMeta(permalink);

            VerifyAllMocks(releaseSubjectService);

            var viewModel = result.AssertRight();

            Assert.Equal(3, viewModel.Locations.Count);

            // This locations exists in the database - can get all attribute columns.
            Assert.Equal(locations[0].GetCsvValues(),viewModel.Locations[locations[0].Id]);

            // These locations do not exist in the database anymore. We have to infer location
            // columns from permalink meta meaning that we are missing various columns.
            var viewModelLocation1 = viewModel.Locations[locations[1].Id];
            var viewModelLocation2 = viewModel.Locations[locations[2].Id];

            // Is missing country columns
            Assert.Equal(2, viewModelLocation1.Count);
            Assert.Equal(locations[1].Region!.Code, viewModelLocation1["region_code"]);
            Assert.Equal(locations[1].Region!.Name, viewModelLocation1["region_name"]);

            // Is missing country columns and old_la_code
            Assert.Equal(5,viewModelLocation2.Count);
            Assert.Equal(locations[2].Region!.Code, viewModelLocation2["region_code"]);
            Assert.Equal(locations[2].Region!.Name, viewModelLocation2["region_name"]);
            Assert.Equal(locations[2].LocalAuthority!.Code, viewModelLocation2["new_la_code"]);
            Assert.Equal(locations[2].LocalAuthority!.Name, viewModelLocation2["la_name"]);
            Assert.Empty(viewModelLocation2["old_la_code"]);

            var expectedHeaders = new List<string>
            {
                "time_period",
                "time_identifier",
                "geographic_level",
                "country_code",
                "country_name",
                "region_code",
                "region_name",
                // Note that old_la_code is missing. It's not possible to
                // infer it, so we just exclude it from the CSV headers.
                "new_la_code",
                "la_name",
                filters[0].Name,
                indicators[0].Name,
            };

            Assert.Equal(expectedHeaders, viewModel.Headers);
        }
    }

    [Fact]
    public async Task GetCsvMeta_SubjectExists_DataFileExists()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(2);

        var filterItems = filters
            .SelectMany(f => f.FilterGroups)
            .SelectMany(fg => fg.FilterItems)
            .ToList();

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
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
            .ForIndex(0, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForIndex(1, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
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
            var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(releaseSubject.SubjectId))
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
                filters[1].Name,
                filters[0].Name,
                indicators[2].Name,
                indicators[0].Name,
                indicators[1].Name
            );

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

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
                SubjectId = releaseSubject.SubjectId
            };

            var permalink = new LegacyPermalink
            {
                Query = query,
                FullTable = new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta
                    {
                        Filters = FiltersMetaViewModelBuilder.BuildFilters(filters),
                        Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                        LocationsHierarchical = LocationViewModelBuilder
                            .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                            .ToDictionary(
                                level => level.Key.ToString().CamelCase(),
                                level => level.Value)
                    }
                }
            };

            var result = await service.GetCsvMeta(permalink);

            VerifyAllMocks(releaseSubjectService, releaseFileBlobService);

            var viewModel = result.AssertRight();

            Assert.Equal(2, viewModel.Filters.Count);

            var viewModelFilter0 = viewModel.Filters[filters[0].Name];
            var viewModelFilter1 = viewModel.Filters[filters[1].Name];

            Assert.Equal(filters[0].Id, viewModelFilter0.Id);
            Assert.Equal(filters[0].Name, viewModelFilter0.Name);
            Assert.Equal(filters[1].Id, viewModelFilter1.Id);
            Assert.Equal(filters[1].Name, viewModelFilter1.Name);

            var viewModelFilter0Items = viewModelFilter0.Items;
            Assert.Single(viewModelFilter0Items);

            var viewModelFilter0Item0 = viewModelFilter0Items[filterItems[0].Id];

            Assert.Equal(filterItems[0].Id, viewModelFilter0Item0.Id);
            Assert.Equal(filterItems[0].Label, viewModelFilter0Item0.Label);

            var viewModelFilter1Items = viewModelFilter1.Items;
            Assert.Single(viewModelFilter1Items);

            var viewModelFilter1Item0 = viewModelFilter1Items[filterItems[1].Id];

            Assert.Equal(filterItems[1].Id, viewModelFilter1Item0.Id);
            Assert.Equal(filterItems[1].Label, viewModelFilter1Item0.Label);

            Assert.Equal(3, viewModel.Indicators.Count);
            AssertIndicatorCsvViewModel(indicators[0], viewModel.Indicators[indicators[0].Name]);
            AssertIndicatorCsvViewModel(indicators[1], viewModel.Indicators[indicators[1].Name]);
            AssertIndicatorCsvViewModel(indicators[2], viewModel.Indicators[indicators[2].Name]);

            Assert.Equal(2, viewModel.Locations.Count);
            Assert.Equal(locations[0].GetCsvValues(), viewModel.Locations[locations[0].Id]);
            Assert.Equal(locations[1].GetCsvValues(), viewModel.Locations[locations[1].Id]);

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
    public async Task GetCsvMeta_SubjectExists_MultipleReleaseFilesExist()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(2);

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
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
            .ForIndex(0, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForIndex(1, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
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
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(releaseSubject.SubjectId))
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
                filters[1].Name,
                filters[0].Name,
                indicators[2].Name,
                indicators[0].Name,
                indicators[1].Name
            );

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

            releaseFileBlobService
                .Setup(s => s.StreamBlob(
                        It.Is<ReleaseFile>(rf =>
                            rf.FileId == releaseDataFile.FileId && rf.ReleaseId == releaseDataFile.ReleaseId),
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
                SubjectId = releaseSubject.SubjectId
            };

            var permalink = new LegacyPermalink
            {
                Query = query,
                FullTable = new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta
                    {
                        Filters = FiltersMetaViewModelBuilder.BuildFilters(filters),
                        Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                        LocationsHierarchical = LocationViewModelBuilder
                            .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                            .ToDictionary(
                                level => level.Key.ToString().CamelCase(),
                                level => level.Value)
                    }
                }
            };

            var result = await service.GetCsvMeta(permalink);

            VerifyAllMocks(releaseSubjectService, releaseFileBlobService);

            var viewModel = result.AssertRight();

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
    public async Task GetCsvMeta_SubjectExists_ReleaseFileNotFound_HeadersInDefaultOrder()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(2);

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
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
            .ForIndex(0, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForIndex(1, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
            await statisticsDbContext.Location.AddRangeAsync(locations);
            await statisticsDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
        {
            var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(releaseSubject.SubjectId))
                .ReturnsAsync(releaseSubject);

            var service = BuildService(
                contentDbContext: contentDbContext,
                statisticsDbContext: statisticsDbContext,
                releaseSubjectService: releaseSubjectService.Object
            );

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.SubjectId
            };

            var permalink = new LegacyPermalink
            {
                Query = query,
                FullTable = new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta
                    {
                        Filters = FiltersMetaViewModelBuilder.BuildFilters(filters),
                        Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                        LocationsHierarchical = LocationViewModelBuilder
                            .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                            .ToDictionary(
                                level => level.Key.ToString().CamelCase(),
                                level => level.Value)
                    }
                }
            };

            var result = await service.GetCsvMeta(permalink);

            VerifyAllMocks(releaseSubjectService);

            var viewModel = result.AssertRight();

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
    public async Task GetCsvMeta_SubjectExists_BlobNotFound_HeadersInDefaultOrder()
    {
        var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 1)
            .GenerateList(2);

        var indicatorGroups = _fixture.DefaultIndicatorGroup()
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
            .ForIndex(0, l => l
                .SetPresetRegion()
                .SetGeographicLevel(GeographicLevel.Region))
            .ForIndex(1, l => l
                .SetPresetRegionAndLocalAuthority()
                .SetGeographicLevel(GeographicLevel.LocalAuthority))
            .GenerateList();

        var releaseSubject = new ReleaseSubject
        {
            Release = _fixture.DefaultStatsRelease(),
            Subject = _fixture.DefaultSubject()
                .WithFilters(filters)
                .WithIndicatorGroups(indicatorGroups)
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
            var releaseSubjectService = new Mock<IReleaseSubjectService>(Strict);

            releaseSubjectService
                .Setup(s => s.FindForLatestPublishedVersion(releaseSubject.SubjectId))
                .ReturnsAsync(releaseSubject);

            var releaseFileBlobService = new Mock<IReleaseFileBlobService>(Strict);

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
                SubjectId = releaseSubject.SubjectId
            };

            var permalink = new LegacyPermalink
            {
                Query = query,
                FullTable = new PermalinkTableBuilderResult
                {
                    SubjectMeta = new PermalinkResultSubjectMeta
                    {
                        Filters = FiltersMetaViewModelBuilder.BuildFilters(filters),
                        Indicators = IndicatorsMetaViewModelBuilder.BuildIndicators(indicators),
                        LocationsHierarchical = LocationViewModelBuilder
                            .BuildLocationAttributeViewModels(locations, _regionLocalAuthorityHierarchy)
                            .ToDictionary(
                                level => level.Key.ToString().CamelCase(),
                                level => level.Value)
                    }
                }
            };

            var result = await service.GetCsvMeta(permalink);

            VerifyAllMocks(releaseSubjectService);

            var viewModel = result.AssertRight();

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

    private static void AssertIndicatorCsvViewModel(Indicator indicator, IndicatorCsvMetaViewModel viewModel)
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
        return new(
            logger: Mock.Of<ILogger<PermalinkCsvMetaService>>(),
            contentDbContext: contentDbContext,
            statisticsDbContext: statisticsDbContext,
            releaseSubjectService: releaseSubjectService ?? Mock.Of<IReleaseSubjectService>(Strict),
            releaseFileBlobService: releaseFileBlobService ?? Mock.Of<IReleaseFileBlobService>(Strict)
        );
    }
}
