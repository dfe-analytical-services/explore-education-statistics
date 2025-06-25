#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Options;
using Microsoft.Extensions.Options;
using Semver;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using IReleaseVersionService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseVersionService;
using ReleaseVersion = GovUk.Education.ExploreEducationStatistics.Data.Model.ReleaseVersion;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReplacementServiceTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");

        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task GetReplacementPlan_ReplacementFileNotFound()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalReleaseFile.FileId,
                    replacementFileId: Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetReplacementPlan_OriginalFileNotFound()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = new File
                {
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: Guid.NewGuid(),
                    replacementFileId: replacementReleaseFile.FileId);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetReplacementPlan_ReleaseNotFound()
        {
            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = Guid.NewGuid()
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = Guid.NewGuid(),
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Files.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: Guid.NewGuid(),
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetReplacementPlan_ReplacementFileAttachedToDifferentRelease()
        {
            var (release1, release2) = _fixture.DefaultReleaseVersion().GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = Guid.NewGuid()
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = Guid.NewGuid()
            };

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = release1,
                File = originalFile
            };

            // Link the replacement to the unrelated Release
            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = release2,
                File = replacementFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(release1, release2);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: release1.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetReplacementPlan_OriginalFileIsNotUsedByAnyDependentData()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId
            };

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                    replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                var replacementPlan = result.AssertRight();

                Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

                Assert.Empty(replacementPlan.DataBlocks);
                Assert.Empty(replacementPlan.Footnotes);
                Assert.True(replacementPlan.Valid);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_NoReplacementDataPresent_ReplacementInvalid()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId
            };

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Original Test filter item"
            };

            var replacementFilterItem = new FilterItem
            {
                Label = "Replacement Test filter item"
            };

            var originalFilterGroup = new FilterGroup
            {
                Label = "Original Default group",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem
                }
            };

            var replacementFilterGroup = new FilterGroup
            {
                Label = "Replacement Default group",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem
                }
            };

            var originalFilter = new Filter
            {
                Label = "Original Test filter",
                Name = "original_test_filter",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup
                }
            };

            var replacementFilter = new Filter
            {
                Label = "Replacement Test filter",
                Name = "replacement_test_filter",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementFilterGroup
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Original Indicator",
                Name = "original_indicator"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Replacement Indicator",
                Name = "replacement_indicator"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Original Default group",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Replacement Default group",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var originalLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[] {originalFilterItem.Id},
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod
                },
                ReleaseVersion = releaseVersion
            };

            var footnoteForSubject = CreateFootnote(statsReleaseVersion,
                "Test footnote for Subject",
                subject: originalReleaseSubject.Subject);

            var footnoteForFilter = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter",
                filterFootnotes: new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = originalFilter
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = originalFilterGroup
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = originalFilterItem
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = originalIndicator
                    }
                });

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                    replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalFilter, replacementFilter);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Footnote.AddRange(footnoteForFilter, footnoteForFilterGroup,
                    footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
                statisticsDbContext.Location.AddRange(originalLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                var replacementPlan = result.AssertRight();

                Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

                Assert.Single(replacementPlan.DataBlocks);
                var dataBlockPlan = replacementPlan.DataBlocks.First();
                Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
                Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

                Assert.Single(dataBlockPlan.IndicatorGroups);

                var dataBlockIndicatorGroupPlan = dataBlockPlan.IndicatorGroups.First();

                Assert.Equal(originalIndicator.IndicatorGroup.Id, dataBlockIndicatorGroupPlan.Key);
                Assert.Equal(originalIndicator.IndicatorGroup.Label, dataBlockIndicatorGroupPlan.Value.Label);
                Assert.Single(dataBlockIndicatorGroupPlan.Value.Indicators);
                Assert.False(dataBlockIndicatorGroupPlan.Value.Valid);

                var dataBlockIndicatorPlan = dataBlockIndicatorGroupPlan.Value.Indicators.First();
                Assert.Equal(originalIndicator.Id, dataBlockIndicatorPlan.Id);
                Assert.Equal(originalIndicator.Label, dataBlockIndicatorPlan.Label);
                Assert.Equal(originalIndicator.Name, dataBlockIndicatorPlan.Name);
                Assert.Null(dataBlockIndicatorPlan.Target);
                Assert.False(dataBlockIndicatorPlan.Valid);

                Assert.Single(dataBlockPlan.Filters);

                var dataBlockFilterPlan = dataBlockPlan.Filters.First();

                Assert.Equal(originalFilter.Id, dataBlockFilterPlan.Key);
                Assert.Equal(originalFilter.Id, dataBlockFilterPlan.Value.Id);
                Assert.Equal(originalFilter.Label, dataBlockFilterPlan.Value.Label);
                Assert.Equal(originalFilter.Name, dataBlockFilterPlan.Value.Name);
                Assert.False(dataBlockFilterPlan.Value.Valid);

                Assert.Single(dataBlockFilterPlan.Value.Groups);

                var dataBlockFilterGroupPlan = dataBlockFilterPlan.Value.Groups.First();

                Assert.Equal(originalFilterGroup.Id, dataBlockFilterGroupPlan.Key);
                Assert.Equal(originalFilterGroup.Id, dataBlockFilterGroupPlan.Value.Id);
                Assert.Equal(originalFilterGroup.Label, dataBlockFilterGroupPlan.Value.Label);
                Assert.False(dataBlockFilterGroupPlan.Value.Valid);

                Assert.Single(dataBlockFilterGroupPlan.Value.Filters);

                var dataBlockFilterItemPlan = dataBlockFilterGroupPlan.Value.Filters.First();

                Assert.Equal(originalFilterItem.Id, dataBlockFilterItemPlan.Id);
                Assert.Equal(originalFilterItem.Label, dataBlockFilterItemPlan.Label);
                Assert.Null(dataBlockFilterItemPlan.Target);
                Assert.False(dataBlockFilterItemPlan.Valid);

                Assert.NotNull(dataBlockPlan.Locations);
                Assert.Single(dataBlockPlan.Locations);
                Assert.True(dataBlockPlan.Locations.ContainsKey(GeographicLevel.Country.ToString()));
                Assert.Single(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].LocationAttributes);
                Assert.False(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Valid);

                var dataBlockLocationPlan = dataBlockPlan
                    .Locations[GeographicLevel.Country.ToString()]
                    .LocationAttributes
                    .First();

                Assert.Equal(originalLocation.Id, dataBlockLocationPlan.Id);
                Assert.Equal(_england.Code, dataBlockLocationPlan.Code);
                Assert.Equal(_england.Name, dataBlockLocationPlan.Label);
                Assert.Null(dataBlockLocationPlan.Target);
                Assert.False(dataBlockLocationPlan.Valid);

                Assert.NotNull(dataBlockPlan.TimePeriods);
                Assert.False(dataBlockPlan.TimePeriods!.Valid);

                Assert.Equal(timePeriod.StartYear, dataBlockPlan.TimePeriods.Start.Year);
                Assert.Equal(timePeriod.StartCode, dataBlockPlan.TimePeriods.Start.Code);
                Assert.False(dataBlockPlan.TimePeriods.Start.Valid);

                Assert.Equal(timePeriod.EndYear, dataBlockPlan.TimePeriods.End.Year);
                Assert.Equal(timePeriod.EndCode, dataBlockPlan.TimePeriods.End.Code);
                Assert.False(dataBlockPlan.TimePeriods.End.Valid);

                Assert.False(dataBlockPlan.Valid);
                Assert.False(dataBlockPlan.Fixable);

                Assert.Equal(5, replacementPlan.Footnotes.Count());

                var footnoteForFilterPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilter.Id);

                Assert.Equal(footnoteForFilter.Content, footnoteForFilterPlan.Content);
                Assert.Single(footnoteForFilterPlan.Filters);
                Assert.Empty(footnoteForFilterPlan.FilterGroups);
                Assert.Empty(footnoteForFilterPlan.FilterItems);
                Assert.Empty(footnoteForFilterPlan.IndicatorGroups);

                var footnoteForFilterFilterPlan = footnoteForFilterPlan.Filters.First();

                Assert.Equal(originalFilter.Id, footnoteForFilterFilterPlan.Id);
                Assert.Equal(originalFilter.Label, footnoteForFilterFilterPlan.Label);
                Assert.Null(footnoteForFilterFilterPlan.Target);
                Assert.False(footnoteForFilterFilterPlan.Valid);

                Assert.False(footnoteForFilterPlan.Valid);

                var footnoteForFilterGroupPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterGroup.Id);

                Assert.False(footnoteForFilterGroupPlan.Valid);

                Assert.Equal(footnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
                Assert.Empty(footnoteForFilterGroupPlan.Filters);
                Assert.Single(footnoteForFilterGroupPlan.FilterGroups);
                Assert.Empty(footnoteForFilterGroupPlan.FilterItems);
                Assert.Empty(footnoteForFilterGroupPlan.IndicatorGroups);

                var footnoteForFilterGroupFilterGroupPlan = footnoteForFilterGroupPlan.FilterGroups.First();

                Assert.Equal(originalFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Id);
                Assert.Equal(originalFilterGroup.Label, footnoteForFilterGroupFilterGroupPlan.Label);
                Assert.Equal(originalFilterGroup.Filter.Id, footnoteForFilterGroupFilterGroupPlan.FilterId);
                Assert.Equal(originalFilterGroup.Filter.Label, footnoteForFilterGroupFilterGroupPlan.FilterLabel);
                Assert.Null(footnoteForFilterGroupFilterGroupPlan.Target);
                Assert.False(footnoteForFilterGroupFilterGroupPlan.Valid);

                Assert.False(footnoteForFilterGroupPlan.Valid);

                var footnoteForFilterItemPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterItem.Id);

                Assert.Equal(footnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
                Assert.Empty(footnoteForFilterItemPlan.Filters);
                Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
                Assert.Single(footnoteForFilterItemPlan.FilterItems);
                Assert.Empty(footnoteForFilterItemPlan.IndicatorGroups);

                var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();

                Assert.Equal(originalFilterItem.Id, footnoteForFilterItemFilterItemPlan.Id);
                Assert.Equal(originalFilterItem.Label, footnoteForFilterItemFilterItemPlan.Label);
                Assert.Equal(originalFilterItem.FilterGroup.Filter.Id, footnoteForFilterItemFilterItemPlan.FilterId);
                Assert.Equal(originalFilterItem.FilterGroup.Filter.Label,
                    footnoteForFilterItemFilterItemPlan.FilterLabel);
                Assert.Equal(originalFilterItem.FilterGroup.Id, footnoteForFilterItemFilterItemPlan.FilterGroupId);
                Assert.Equal(originalFilterItem.FilterGroup.Label,
                    footnoteForFilterItemFilterItemPlan.FilterGroupLabel);
                Assert.Null(footnoteForFilterItemFilterItemPlan.Target);
                Assert.False(footnoteForFilterItemFilterItemPlan.Valid);

                Assert.False(footnoteForFilterItemPlan.Valid);

                var footnoteForIndicatorPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);

                Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);
                Assert.Empty(footnoteForIndicatorPlan.Filters);
                Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
                Assert.Empty(footnoteForIndicatorPlan.FilterItems);
                Assert.Single(footnoteForIndicatorPlan.IndicatorGroups);

                var footnoteForIndicatorIndicatorGroupPlan = footnoteForIndicatorPlan.IndicatorGroups.First();

                Assert.Equal(originalIndicator.IndicatorGroup.Id, footnoteForIndicatorIndicatorGroupPlan.Key);
                Assert.Equal(originalIndicator.IndicatorGroup.Label,
                    footnoteForIndicatorIndicatorGroupPlan.Value.Label);
                Assert.Single(footnoteForIndicatorIndicatorGroupPlan.Value.Indicators);
                Assert.False(footnoteForIndicatorIndicatorGroupPlan.Value.Valid);

                var footnoteForIndicatorIndicatorPlan =
                    footnoteForIndicatorIndicatorGroupPlan.Value.Indicators.First();

                Assert.Equal(originalIndicator.Id, footnoteForIndicatorIndicatorPlan.Id);
                Assert.Equal(originalIndicator.Label, footnoteForIndicatorIndicatorPlan.Label);
                Assert.Equal(originalIndicator.Name, footnoteForIndicatorIndicatorPlan.Name);
                Assert.Null(footnoteForIndicatorIndicatorPlan.Target);
                Assert.False(footnoteForIndicatorIndicatorPlan.Valid);

                Assert.False(footnoteForIndicatorPlan.Valid);

                var footnoteForSubjectPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForSubject.Id);

                Assert.Equal(footnoteForSubject.Content, footnoteForSubjectPlan.Content);
                Assert.Empty(footnoteForSubjectPlan.Filters);
                Assert.Empty(footnoteForSubjectPlan.FilterGroups);
                Assert.Empty(footnoteForSubjectPlan.FilterItems);
                Assert.Empty(footnoteForSubjectPlan.IndicatorGroups);

                Assert.True(footnoteForSubjectPlan.Valid);

                Assert.False(replacementPlan.Valid);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_SelectedFilterItemsNoLongerExistButSomeDo_ReplacementInvalidButFixable()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId
            };

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalDefaultFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item"
            };

            var originalDefaultFilterItem2 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item 2"
            };

            var replacementDefaultFilterItem = new FilterItem
            {
                Label = "Test filter item",
            };

            var originalDefaultFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalDefaultFilterItem,
                    originalDefaultFilterItem2
                }
            };

            var replacementDefaultFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementDefaultFilterItem
                }
            };

            var originalDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalDefaultFilterGroup
                }
            };

            var replacementDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementDefaultFilterGroup
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var location = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[]
                    {
                        originalDefaultFilterItem.Id,
                        originalDefaultFilterItem2.Id
                    },
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(location.Id),
                    TimePeriod = timePeriod
                },
                ReleaseVersion = releaseVersion
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    location
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                    replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalDefaultFilter, replacementDefaultFilter);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Location.AddRange(location);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                var replacementPlan = result.AssertRight();
                Assert.False(replacementPlan.Valid);

                Assert.Single(replacementPlan.DataBlocks);
                var dataBlockPlan = replacementPlan.DataBlocks.First();
                Assert.False(dataBlockPlan.Valid);
                Assert.True(dataBlockPlan.Fixable);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_AllOriginalFilterItemsNoLongerExist_ReplacementInvalidAndNotFixable()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId
            };

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalDefaultFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item"
            };

            var replacementDefaultFilterItem = new FilterItem
            {
                Label = "Test filter item - changing!",
            };

            var originalDefaultFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalDefaultFilterItem
                }
            };

            var replacementDefaultFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementDefaultFilterItem,
                }
            };

            var originalDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalDefaultFilterGroup
                }
            };

            var replacementDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementDefaultFilterGroup
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var location = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[]
                    {
                        originalDefaultFilterItem.Id
                    },
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(location.Id),
                    TimePeriod = timePeriod
                },
                ReleaseVersion = releaseVersion
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    location
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                    replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalDefaultFilter, replacementDefaultFilter);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Location.AddRange(location);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                var replacementPlan = result.AssertRight();
                Assert.False(replacementPlan.Valid);

                Assert.Single(replacementPlan.DataBlocks);
                var dataBlockPlan = replacementPlan.DataBlocks.First();
                Assert.False(dataBlockPlan.Valid);
                Assert.False(dataBlockPlan.Fixable);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_NewFiltersIntroduced_ReplacementInvalidAndNotFixable()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId
            };

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalDefaultFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item"
            };

            var replacementDefaultFilterItem = new FilterItem
            {
                Label = "Test filter item"
            };

            var replacementNewlyIntroducedFiltersFilterItem = new FilterItem
            {
                Label = "Filter item for newly introduced Filter"
            };

            var originalDefaultFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalDefaultFilterItem
                }
            };

            var replacementDefaultFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementDefaultFilterItem
                }
            };

            var replacementNewlyIntroducedFilterGroup = new FilterGroup
            {
                Label = "Newly introduced filter group",
                FilterItems = new List<FilterItem>
                {
                    replacementNewlyIntroducedFiltersFilterItem
                }
            };

            var originalDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalDefaultFilterGroup
                }
            };

            var replacementDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementDefaultFilterGroup
                }
            };

            var replacementNewlyIntroducedFilter = new Filter
            {
                Label = "Newly introduced filter",
                Name = "newly_introduced_filter",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementNewlyIntroducedFilterGroup
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var location = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[]
                    {
                        originalDefaultFilterItem.Id
                    },
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(location.Id),
                    TimePeriod = timePeriod
                },
                ReleaseVersion = releaseVersion
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    location
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                    replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalDefaultFilter,
                    replacementDefaultFilter, replacementNewlyIntroducedFilter);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Location.AddRange(location);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                var replacementPlan = result.AssertRight();
                Assert.False(replacementPlan.Valid);

                Assert.Single(replacementPlan.DataBlocks);
                var dataBlockPlan = replacementPlan.DataBlocks.First();
                Assert.False(dataBlockPlan.Valid);
                Assert.False(dataBlockPlan.Fixable);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_ReplacementHasDifferentLocation_LocationMatchedByCode_ReplacementValid()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId
            };

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var replacementFilterItem = new FilterItem
            {
                Label = "Test filter item - not changing"
            };

            var originalFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem
                }
            };

            var replacementFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem
                }
            };

            var originalFilter = new Filter
            {
                Label = "Filter - not changing",
                Name = "filter_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup
                }
            };

            var replacementFilter = new Filter
            {
                Label = "Filter - not changing",
                Name = "filter_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementFilterGroup
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var originalLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                LocalAuthority = _derby
            };

            // Replacement location has a different id but the primary attribute code remains the same
            var replacementLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _derby
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[] {originalFilterItem.Id},
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod
                },
                ReleaseVersion = releaseVersion
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    replacementLocation
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalFilter, replacementFilter);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Location.AddRange(originalLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                var replacementPlan = result.AssertRight();

                Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

                Assert.Single(replacementPlan.DataBlocks);
                var dataBlockPlan = replacementPlan.DataBlocks.First();
                Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
                Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

                Assert.NotNull(dataBlockPlan.Locations);
                Assert.Single(dataBlockPlan.Locations);
                Assert.True(dataBlockPlan.Locations.ContainsKey(GeographicLevel.LocalAuthority.ToString()));
                Assert.Single(dataBlockPlan.Locations[GeographicLevel.LocalAuthority.ToString()].LocationAttributes);
                Assert.True(dataBlockPlan.Locations[GeographicLevel.LocalAuthority.ToString()].Valid);

                var dataBlockLocationPlan = dataBlockPlan
                    .Locations[GeographicLevel.LocalAuthority.ToString()]
                    .LocationAttributes
                    .First();

                Assert.Equal(originalLocation.Id, dataBlockLocationPlan.Id);
                Assert.Equal(_derby.Code, dataBlockLocationPlan.Code);
                Assert.Equal(_derby.Name, dataBlockLocationPlan.Label);
                Assert.Equal(replacementLocation.Id, dataBlockLocationPlan.Target);
                Assert.True(dataBlockLocationPlan.Valid);

                Assert.True(dataBlockPlan.Valid);
                Assert.True(replacementPlan.Valid);
            }
        }

        /// <summary>
        /// Please note this test will reduce in the number of cases when the feature flag has been removed from it.
        /// </summary>
        /// <param name="dataSetVersionStatus">The data set version status of the replaced file's API data set version</param>
        /// <param name="majorVersionUpdate">Whether the user has uploaded a file that results in a major version update</param>
        /// <param name="enableReplacementOfPublicApiDataSets">Whether the feature flag for EES-5779 is switched on or off</param>
        /// <param name="expectedValidValue">The expected value for the scenario set up</param>
        [Theory]
        //When user has uploaded major version data set
        [InlineData(DataSetVersionStatus.Published, true, true, false)]
        [InlineData(DataSetVersionStatus.Mapping, true, true, false)]
        [InlineData(DataSetVersionStatus.Draft, true, true, false)]
        [InlineData(DataSetVersionStatus.Published, true, false, false)]
        [InlineData(DataSetVersionStatus.Mapping, true, false, false)]
        [InlineData(DataSetVersionStatus.Draft, true, false, false)]
        //When user has uploaded minor version
        [InlineData(DataSetVersionStatus.Published, false, true, false)]
        [InlineData(DataSetVersionStatus.Mapping, false, true, false)]
        [InlineData(DataSetVersionStatus.Draft, false, true, true)]
        [InlineData(DataSetVersionStatus.Published, false, false, false)]
        [InlineData(DataSetVersionStatus.Mapping, false, false, false)]
        [InlineData(DataSetVersionStatus.Draft, false, false, false)]
        //When API data set version status is not appropriate to be replaced
        [InlineData(DataSetVersionStatus.Processing, false, true, false)]
        [InlineData(DataSetVersionStatus.Failed, false, true, false)]
        [InlineData(DataSetVersionStatus.Deprecated, false, true, false)]
        [InlineData(DataSetVersionStatus.Withdrawn, false, true, false)]
        [InlineData(DataSetVersionStatus.Cancelled, false, true, false)]
        [InlineData(DataSetVersionStatus.Processing, false, false, false)]
        [InlineData(DataSetVersionStatus.Failed, false, false, false)]
        [InlineData(DataSetVersionStatus.Deprecated, false, false, false)]
        [InlineData(DataSetVersionStatus.Withdrawn, false, false, false)]
        [InlineData(DataSetVersionStatus.Cancelled, false, false, false)]
        public async Task GetReplacementPlan_FileIsLinkedToPublicApiDataSet_ReplacementValidated(
            DataSetVersionStatus dataSetVersionStatus, 
            bool  majorVersionUpdate,
            bool enableReplacementOfPublicApiDataSets, 
            bool expectedValidValue)
        {
            DataSet dataSet = _fixture
                .DefaultDataSet();

            DataSetVersion dataSetVersion = _fixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 1, patch: 1)
                .WithStatus(dataSetVersionStatus)
                .WithDataSet(dataSet);

            Content.Model.ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            File originalFile = _fixture
                .DefaultFile()
                .WithSubjectId(originalReleaseSubject.SubjectId)
                .WithType(FileType.Data);

            File replacementFile = _fixture
                .DefaultFile()
                .WithSubjectId(replacementReleaseSubject.SubjectId)
                .WithType(FileType.Data);

            var (originalReleaseFile, replacementReleaseFile) = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, rv =>
                    rv.SetFile(originalFile)
                        .SetPublicApiDataSetId(dataSet.Id)
                        .SetPublicApiDataSetVersion(dataSetVersion.SemVersion()))
                .ForIndex(1, rv =>
                    rv.SetFile(replacementFile)
                        .SetPublicApiDataSetId(dataSet.Id)
                        .SetPublicApiDataSetVersion(dataSetVersion.SemVersion()))
                .GenerateTuple2();

            var dataSetVersionService = new Mock<IDataSetVersionService>(Strict);
            dataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                originalReleaseFile.PublicApiDataSetId!.Value,
                originalReleaseFile.PublicApiDataSetVersion!, 
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataSetVersion);

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var dataSetVersionMappingService = new Mock<IDataSetVersionMappingService>(Strict);
            dataSetVersionMappingService.Setup(service => service.GetMappingStatus(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MappingStatusViewModel
                {
                    FiltersComplete = majorVersionUpdate,
                    LocationsComplete = majorVersionUpdate,
                    HasDeletionChanges = majorVersionUpdate,
                    FiltersHaveMajorChange = majorVersionUpdate,
                    LocationsHaveMajorChange = majorVersionUpdate
                });
            
            var options = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
            {
                EnableReplacementOfPublicApiDataSets = enableReplacementOfPublicApiDataSets
            });
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(
                    contentDbContext,
                    statisticsDbContext,
                    dataSetVersionService: dataSetVersionService.Object,
                    timePeriodService: timePeriodService.Object,
                    locationRepository: locationRepository.Object,
                    dataSetVersionMappingService: dataSetVersionMappingService.Object,
                    featureFlags: options
                    );

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(dataSetVersionService);

                var replacementPlan = result.AssertRight();
                
                Assert.NotNull(replacementPlan.ApiDataSetVersionPlan);
                Assert.Equal(dataSet.Id, replacementPlan.ApiDataSetVersionPlan.DataSetId);
                Assert.Equal(dataSet.Title, replacementPlan.ApiDataSetVersionPlan.DataSetTitle);
                Assert.Equal(dataSetVersion.Id, replacementPlan.ApiDataSetVersionPlan.Id);
                Assert.Equal(dataSetVersion.PublicVersion, replacementPlan.ApiDataSetVersionPlan.Version);
                if (enableReplacementOfPublicApiDataSets)
                {
                    Assert.Equal(expectedValidValue, replacementPlan.ApiDataSetVersionPlan.Valid);
                }
                else
                {
                    Assert.Null(replacementPlan.ApiDataSetVersionPlan.MappingStatus);
                    Assert.False(replacementPlan.ApiDataSetVersionPlan.Valid);
                }
                Assert.Equal(dataSetVersion.Status, replacementPlan.ApiDataSetVersionPlan.Status);

                Assert.Equal(replacementPlan.ApiDataSetVersionPlan.Valid, expectedValidValue);
                Assert.Equal(replacementPlan.Valid, expectedValidValue);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_AllReplacementDataPresent_ReplacementValid()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId
            };

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalDefaultFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var originalPrimarySchoolsFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Primary schools"
            };

            var originalPrimaryAndSecondarySchoolsFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Primary and secondary schools"
            };

            var replacementDefaultFilterItem = new FilterItem
            {
                Label = "Test filter item - not changing"
            };

            var replacementPrimarySchoolsFilterItem = new FilterItem
            {
                Label = "Primary schools"
            };

            var replacementPrimaryAndSecondarySchoolsFilterItem = new FilterItem
            {
                Label = "Primary and secondary schools"
            };

            var originalDefaultFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalDefaultFilterItem
                }
            };

            var originalIndividualSchoolTypeFilterGroup = new FilterGroup
            {
                Label = "Individual",
                FilterItems = new List<FilterItem>
                {
                    originalPrimarySchoolsFilterItem
                }
            };

            var originalCombinedSchoolTypeFilterGroup = new FilterGroup
            {
                Label = "Combined",
                FilterItems = new List<FilterItem>
                {
                    originalPrimaryAndSecondarySchoolsFilterItem
                }
            };

            var replacementDefaultFilterGroup = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementDefaultFilterItem
                }
            };

            var replacementIndividualSchoolTypeFilterGroup = new FilterGroup
            {
                Label = "Individual",
                FilterItems = new List<FilterItem>
                {
                    replacementPrimarySchoolsFilterItem
                }
            };

            var replacementCombinedSchoolTypeFilterGroup = new FilterGroup
            {
                Label = "Combined",
                FilterItems = new List<FilterItem>
                {
                    replacementPrimaryAndSecondarySchoolsFilterItem
                }
            };

            var originalDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalDefaultFilterGroup
                }
            };

            var originalSchoolTypeFilter = new Filter
            {
                Label = "School type",
                Name = "school_type",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalIndividualSchoolTypeFilterGroup,
                    originalCombinedSchoolTypeFilterGroup,
                }
            };

            var replacementDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementDefaultFilterGroup
                }
            };

            var replacementSchoolTypeFilter = new Filter
            {
                Label = "School type",
                Name = "school_type",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementIndividualSchoolTypeFilterGroup,
                    replacementCombinedSchoolTypeFilterGroup
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var location = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[]
                    {
                        originalDefaultFilterItem.Id,
                        originalPrimarySchoolsFilterItem.Id,
                        originalPrimaryAndSecondarySchoolsFilterItem.Id,
                    },
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(location.Id),
                    TimePeriod = timePeriod
                },
                ReleaseVersion = releaseVersion
            };

            var footnoteForFilter = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter",
                filterFootnotes: new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = originalDefaultFilter
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = originalDefaultFilterGroup
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = originalDefaultFilterItem
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = originalIndicator
                    }
                });

            var footnoteForSubject = CreateFootnote(statsReleaseVersion,
                "Test footnote for Subject",
                subject: originalReleaseSubject.Subject);

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    location
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                    replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalDefaultFilter, originalSchoolTypeFilter,
                    replacementDefaultFilter, replacementSchoolTypeFilter);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Location.AddRange(location);
                statisticsDbContext.Footnote.AddRange(footnoteForFilter, footnoteForFilterGroup,
                    footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.GetReplacementPlan(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                var replacementPlan = result.AssertRight();

                Assert.Equal(originalReleaseSubject.SubjectId, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementReleaseSubject.SubjectId, replacementPlan.ReplacementSubjectId);

                Assert.Single(replacementPlan.DataBlocks);
                var dataBlockPlan = replacementPlan.DataBlocks.First();
                Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
                Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

                Assert.Single(dataBlockPlan.IndicatorGroups);

                var dataBlockIndicatorGroupPlan = dataBlockPlan.IndicatorGroups.First();

                Assert.Equal(originalIndicator.IndicatorGroup.Id, dataBlockIndicatorGroupPlan.Key);
                Assert.Equal(originalIndicator.IndicatorGroup.Label, dataBlockIndicatorGroupPlan.Value.Label);
                Assert.Single(dataBlockIndicatorGroupPlan.Value.Indicators);
                Assert.True(dataBlockIndicatorGroupPlan.Value.Valid);

                var dataBlockIndicatorPlan = dataBlockIndicatorGroupPlan.Value.Indicators.First();

                Assert.Equal(originalIndicator.Id, dataBlockIndicatorPlan.Id);
                Assert.Equal(originalIndicator.Label, dataBlockIndicatorPlan.Label);
                Assert.Equal(originalIndicator.Name, dataBlockIndicatorPlan.Name);
                Assert.Equal(replacementIndicator.Id, dataBlockIndicatorPlan.Target);
                Assert.True(dataBlockIndicatorPlan.Valid);

                Assert.Equal(2, dataBlockPlan.Filters.Count);

                var dataBlockDefaultFilterPlan =
                    dataBlockPlan.Filters.First(f => f.Key.Equals(originalDefaultFilter.Id));

                Assert.Equal(originalDefaultFilter.Id, dataBlockDefaultFilterPlan.Value.Id);
                Assert.Equal(originalDefaultFilter.Label, dataBlockDefaultFilterPlan.Value.Label);
                Assert.Equal(originalDefaultFilter.Name, dataBlockDefaultFilterPlan.Value.Name);
                Assert.True(dataBlockDefaultFilterPlan.Value.Valid);

                Assert.Single(dataBlockDefaultFilterPlan.Value.Groups);

                var dataBlockDefaultFilterGroupPlan = dataBlockDefaultFilterPlan.Value.Groups.First();

                Assert.Equal(originalDefaultFilterGroup.Id, dataBlockDefaultFilterGroupPlan.Key);
                Assert.Equal(originalDefaultFilterGroup.Id, dataBlockDefaultFilterGroupPlan.Value.Id);
                Assert.Equal(originalDefaultFilterGroup.Label, dataBlockDefaultFilterGroupPlan.Value.Label);
                Assert.Single(dataBlockDefaultFilterGroupPlan.Value.Filters);
                Assert.True(dataBlockDefaultFilterGroupPlan.Value.Valid);

                var dataBlockDefaultFilterItemPlan = dataBlockDefaultFilterGroupPlan.Value.Filters.First();

                Assert.Equal(originalDefaultFilterItem.Id, dataBlockDefaultFilterItemPlan.Id);
                Assert.Equal(originalDefaultFilterItem.Label, dataBlockDefaultFilterItemPlan.Label);
                Assert.Equal(replacementDefaultFilterItem.Id, dataBlockDefaultFilterItemPlan.Target);
                Assert.True(dataBlockDefaultFilterItemPlan.Valid);

                var dataBlockSchoolTypeFilterPlan =
                    dataBlockPlan.Filters.First(f => f.Key.Equals(originalSchoolTypeFilter.Id));

                Assert.Equal(originalSchoolTypeFilter.Id, dataBlockSchoolTypeFilterPlan.Value.Id);
                Assert.Equal(originalSchoolTypeFilter.Label, dataBlockSchoolTypeFilterPlan.Value.Label);
                Assert.Equal(originalSchoolTypeFilter.Name, dataBlockSchoolTypeFilterPlan.Value.Name);
                Assert.True(dataBlockSchoolTypeFilterPlan.Value.Valid);

                Assert.Equal(2, dataBlockSchoolTypeFilterPlan.Value.Groups.Count);

                var dataBlockIndividualSchoolTypeFilterGroupPlan =
                    dataBlockSchoolTypeFilterPlan.Value.Groups.First(g =>
                        g.Key == originalIndividualSchoolTypeFilterGroup.Id);

                Assert.Equal(originalIndividualSchoolTypeFilterGroup.Id,
                    dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Id);
                Assert.Equal(originalIndividualSchoolTypeFilterGroup.Label,
                    dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Label);
                Assert.Single(dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Filters);
                Assert.True(dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Valid);

                var dataBlockCombinedSchoolTypeFilterGroupPlan =
                    dataBlockSchoolTypeFilterPlan.Value.Groups.First(g =>
                        g.Key == originalCombinedSchoolTypeFilterGroup.Id);

                Assert.Equal(originalCombinedSchoolTypeFilterGroup.Id,
                    dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Id);
                Assert.Equal(originalCombinedSchoolTypeFilterGroup.Label,
                    dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Label);
                Assert.Single(dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Filters);
                Assert.True(dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Valid);

                var dataBlockPrimarySchoolsFilterItemPlan =
                    dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Filters.First();

                Assert.Equal(originalPrimarySchoolsFilterItem.Id, dataBlockPrimarySchoolsFilterItemPlan.Id);
                Assert.Equal(originalPrimarySchoolsFilterItem.Label, dataBlockPrimarySchoolsFilterItemPlan.Label);
                Assert.Equal(replacementPrimarySchoolsFilterItem.Id, dataBlockPrimarySchoolsFilterItemPlan.Target);
                Assert.True(dataBlockPrimarySchoolsFilterItemPlan.Valid);

                Assert.NotNull(dataBlockPlan.Locations);
                Assert.Single(dataBlockPlan.Locations);
                Assert.True(dataBlockPlan.Locations.ContainsKey(GeographicLevel.Country.ToString()));
                Assert.Single(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].LocationAttributes);
                Assert.True(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Valid);

                var dataBlockLocationPlan = dataBlockPlan
                    .Locations[GeographicLevel.Country.ToString()]
                    .LocationAttributes
                    .First();

                Assert.Equal(location.Id, dataBlockLocationPlan.Id);
                Assert.Equal(_england.Code, dataBlockLocationPlan.Code);
                Assert.Equal(_england.Name, dataBlockLocationPlan.Label);
                Assert.Equal(location.Id, dataBlockLocationPlan.Target);
                Assert.True(dataBlockLocationPlan.Valid);

                Assert.NotNull(dataBlockPlan.TimePeriods);
                Assert.True(dataBlockPlan.TimePeriods?.Valid);

                Assert.Equal(timePeriod.StartYear, dataBlockPlan.TimePeriods?.Start.Year);
                Assert.Equal(timePeriod.StartCode, dataBlockPlan.TimePeriods?.Start.Code);
                Assert.True(dataBlockPlan.TimePeriods?.Start.Valid);

                Assert.Equal(timePeriod.EndYear, dataBlockPlan.TimePeriods?.End.Year);
                Assert.Equal(timePeriod.EndCode, dataBlockPlan.TimePeriods?.End.Code);
                Assert.True(dataBlockPlan.TimePeriods?.End.Valid);

                Assert.True(dataBlockPlan.Valid);
                Assert.False(dataBlockPlan.Fixable);

                Assert.Equal(5, replacementPlan.Footnotes.Count());

                var footnoteForFilterPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilter.Id);

                Assert.Equal(footnoteForFilter.Content, footnoteForFilterPlan.Content);
                Assert.Single(footnoteForFilterPlan.Filters);
                Assert.Empty(footnoteForFilterPlan.FilterGroups);
                Assert.Empty(footnoteForFilterPlan.FilterItems);
                Assert.Empty(footnoteForFilterPlan.IndicatorGroups);

                var footnoteForFilterFilterPlan = footnoteForFilterPlan.Filters.First();

                Assert.Equal(originalDefaultFilter.Id, footnoteForFilterFilterPlan.Id);
                Assert.Equal(originalDefaultFilter.Label, footnoteForFilterFilterPlan.Label);
                Assert.Equal(replacementDefaultFilter.Id, footnoteForFilterFilterPlan.Target);
                Assert.True(footnoteForFilterFilterPlan.Valid);

                Assert.True(footnoteForFilterPlan.Valid);

                var footnoteForFilterGroupPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterGroup.Id);

                Assert.Equal(footnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
                Assert.Empty(footnoteForFilterGroupPlan.Filters);
                Assert.Single(footnoteForFilterGroupPlan.FilterGroups);
                Assert.Empty(footnoteForFilterGroupPlan.FilterItems);
                Assert.Empty(footnoteForFilterGroupPlan.IndicatorGroups);

                var footnoteForFilterGroupFilterGroupPlan = footnoteForFilterGroupPlan.FilterGroups.First();

                Assert.Equal(originalDefaultFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Id);
                Assert.Equal(originalDefaultFilterGroup.Label, footnoteForFilterGroupFilterGroupPlan.Label);
                Assert.Equal(originalDefaultFilterGroup.Filter.Id, footnoteForFilterGroupFilterGroupPlan.FilterId);
                Assert.Equal(originalDefaultFilterGroup.Filter.Label,
                    footnoteForFilterGroupFilterGroupPlan.FilterLabel);
                Assert.Equal(replacementDefaultFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Target);
                Assert.True(footnoteForFilterGroupFilterGroupPlan.Valid);

                Assert.True(footnoteForFilterGroupPlan.Valid);

                var footnoteForFilterItemPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterItem.Id);

                Assert.Equal(footnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
                Assert.Empty(footnoteForFilterItemPlan.Filters);
                Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
                Assert.Single(footnoteForFilterItemPlan.FilterItems);
                Assert.Empty(footnoteForFilterItemPlan.IndicatorGroups);

                var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();

                Assert.Equal(originalDefaultFilterItem.Id, footnoteForFilterItemFilterItemPlan.Id);
                Assert.Equal(originalDefaultFilterItem.Label, footnoteForFilterItemFilterItemPlan.Label);
                Assert.Equal(originalDefaultFilterItem.FilterGroup.Filter.Id,
                    footnoteForFilterItemFilterItemPlan.FilterId);
                Assert.Equal(originalDefaultFilterItem.FilterGroup.Filter.Label,
                    footnoteForFilterItemFilterItemPlan.FilterLabel);
                Assert.Equal(originalDefaultFilterItem.FilterGroup.Id,
                    footnoteForFilterItemFilterItemPlan.FilterGroupId);
                Assert.Equal(originalDefaultFilterItem.FilterGroup.Label,
                    footnoteForFilterItemFilterItemPlan.FilterGroupLabel);
                Assert.Equal(replacementDefaultFilterItem.Id, footnoteForFilterItemFilterItemPlan.Target);
                Assert.True(footnoteForFilterItemFilterItemPlan.Valid);

                Assert.True(footnoteForFilterItemPlan.Valid);

                var footnoteForIndicatorPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);
                Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);

                Assert.Empty(footnoteForIndicatorPlan.Filters);
                Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
                Assert.Empty(footnoteForIndicatorPlan.FilterItems);
                Assert.Single(footnoteForIndicatorPlan.IndicatorGroups);

                var footnoteForIndicatorIndicatorGroupPlan =
                    footnoteForIndicatorPlan.IndicatorGroups.First();

                Assert.Equal(originalIndicator.IndicatorGroup.Id, footnoteForIndicatorIndicatorGroupPlan.Key);
                Assert.Equal(originalIndicator.IndicatorGroup.Label,
                    footnoteForIndicatorIndicatorGroupPlan.Value.Label);
                Assert.Single(footnoteForIndicatorIndicatorGroupPlan.Value.Indicators);
                Assert.True(footnoteForIndicatorIndicatorGroupPlan.Value.Valid);

                var footnoteForIndicatorIndicatorPlan =
                    footnoteForIndicatorIndicatorGroupPlan.Value.Indicators.First();

                Assert.Equal(originalIndicator.Id, footnoteForIndicatorIndicatorPlan.Id);
                Assert.Equal(originalIndicator.Label, footnoteForIndicatorIndicatorPlan.Label);
                Assert.Equal(originalIndicator.Name, footnoteForIndicatorIndicatorPlan.Name);
                Assert.Equal(replacementIndicator.Id, footnoteForIndicatorIndicatorPlan.Target);
                Assert.True(footnoteForIndicatorIndicatorPlan.Valid);

                Assert.True(footnoteForIndicatorPlan.Valid);

                var footnoteForSubjectPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForSubject.Id);
                Assert.Equal(footnoteForSubject.Content, footnoteForSubjectPlan.Content);
                Assert.Empty(footnoteForSubjectPlan.Filters);
                Assert.Empty(footnoteForSubjectPlan.FilterGroups);
                Assert.Empty(footnoteForSubjectPlan.FilterItems);
                Assert.Empty(footnoteForSubjectPlan.IndicatorGroups);

                Assert.True(footnoteForSubjectPlan.Valid);

                Assert.Null(replacementPlan.ApiDataSetVersionPlan);

                Assert.True(replacementPlan.Valid);
            }
        }

        [Fact]
        public async Task Replace_ReplacementPlanInvalid()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.Country,
                Country = _england
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };
            var table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    ColumnGroups = new List<List<TableHeader>>(),
                    Columns = new List<TableHeader>
                    {
                        new("2019_CY", TableHeaderType.TimePeriod),
                        new("2020_CY", TableHeaderType.TimePeriod)
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new()
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.Country, originalLocation.Id.ToString())
                        }
                    },
                    Rows = new List<TableHeader>()
                }
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new Guid[] { },
                    Indicators = new Guid[] { },
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod
                },
                Table = table,
                ReleaseVersion = releaseVersion
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Location.AddRange(originalLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.Replace(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                result.AssertBadRequest(ReplacementMustBeValid);
            }
        }

        [Fact]
        public async Task Replace_OriginalFileIsNotAssociatedWithReplacementFile()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = originalFile
            };

            // Don't set up a link to the replacement file
            originalFile.ReplacedBy = null;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    replacementService.Replace(
                        releaseVersionId: releaseVersion.Id,
                        originalFileId: originalReleaseFile.FileId,
                        replacementFileId: replacementReleaseFile.FileId));

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                Assert.Equal("Original file has no link with the replacement file", exception.Message);
            }
        }

        [Fact]
        public async Task Replace_ReplacementFileIsNotAssociatedWithOriginalFile()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            // Set up the replacement file but without a link to the original file
            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = null
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object);

                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    replacementService.Replace(
                        releaseVersionId: releaseVersion.Id,
                        originalFileId: originalReleaseFile.FileId,
                        replacementFileId: replacementReleaseFile.FileId));

                VerifyAllMocks(locationRepository,
                    timePeriodService);

                Assert.Equal("Replacement file has no link with the original file", exception.Message);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Replace_FileIsLinkedToPublicApiDataSet_SuccessIfFeatureFlagIsOnOrValidationProblemIfNot(bool enableReplacementOfPublicApiDataSets)
        {
            DataSet dataSet = _fixture
                .DefaultDataSet();

            DataSetVersion dataSetVersion = _fixture
                .DefaultDataSetVersion()
                .WithVersionNumber(major: 1, minor: 1, patch: 1)
                .WithDataSet(dataSet);

            Content.Model.ReleaseVersion releaseVersion = _fixture
                .DefaultReleaseVersion();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            File replacementFile = _fixture
                .DefaultFile()
                .WithSubjectId(replacementReleaseSubject.SubjectId)
                .WithType(FileType.Data);

            File originalFile = _fixture
                .DefaultFile()
                .WithReplacedBy(replacementFile).WithReplacedById(replacementFile.Id)
                .WithSubjectId(originalReleaseSubject.SubjectId)
                .WithType(FileType.Data);

            replacementFile.ReplacingId = originalFile.Id;

            var (originalReleaseFile, replacementReleaseFile) = _fixture.DefaultReleaseFile()
                .WithReleaseVersion(releaseVersion)
                .ForIndex(0, rv => 
                    rv.SetFile(originalFile)
                    .SetPublicApiDataSetId(dataSet.Id)
                    .SetPublicApiDataSetVersion(dataSetVersion.SemVersion()))
                .ForIndex(1, rv =>
                {
                    rv.SetFile(replacementFile)
                        .SetPublicApiDataSetId(dataSet.Id)
                        .SetPublicApiDataSetVersion(dataSetVersion.SemVersion());
                })
                .GenerateTuple2();

            var dataSetVersionService = new Mock<IDataSetVersionService>(Strict);
            dataSetVersionService.Setup(mock => mock.GetDataSetVersion(
                originalReleaseFile.PublicApiDataSetId!.Value,
                It.IsAny<SemVersion>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(dataSetVersion);

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());
            
            var dataSetVersionMappingService = new Mock<IDataSetVersionMappingService>(Strict);
            dataSetVersionMappingService.Setup(service => service.GetMappingStatus(
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MappingStatusViewModel
                {
                    FiltersComplete = true,
                    LocationsComplete = true,
                    HasDeletionChanges = false,
                    FiltersHaveMajorChange = false,
                    LocationsHaveMajorChange = false
                });
            var options = Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
            {
                EnableReplacementOfPublicApiDataSets = enableReplacementOfPublicApiDataSets
            });
            
            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();
            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
            if (enableReplacementOfPublicApiDataSets)
            {
                releaseVersionService.Setup(service => service.RemoveDataFiles(It.IsAny<Guid>(), It.IsAny<Guid>()))
                    .ReturnsAsync(Unit.Instance);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.Add(releaseVersion);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(
                    contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    timePeriodService: timePeriodService.Object,
                    dataSetVersionService: dataSetVersionService.Object,
                    featureFlags: options,
                    dataSetVersionMappingService: dataSetVersionMappingService.Object,
                    releaseVersionService: releaseVersionService.Object);

                var result = await replacementService.Replace(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);
              
                if (enableReplacementOfPublicApiDataSets)
                {
                    VerifyAllMocks(
                        locationRepository,
                        timePeriodService,
                        dataSetVersionService,
                        dataSetVersionMappingService);
                    result.AssertRight();
                }
                else
                {
                    VerifyAllMocks(
                        locationRepository,
                        timePeriodService,
                        dataSetVersionService);
                    result.AssertBadRequest(ReplacementMustBeValid);
                }
            }
        }

        [Fact]
        public async Task Replace()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile,
                Summary = "Original data set guidance"
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile,
                Summary = null
            };

            var originalFilterItem1 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var originalFilterItem2 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var replacementFilterItem1 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var replacementFilterItem2 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var originalFilterGroup1 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem1
                }
            };

            var originalFilterGroup2 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem2
                }
            };

            var replacementFilterGroup1 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem1
                }
            };

            var replacementFilterGroup2 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem2
                }
            };

            var originalFilter1 = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup1
                }
            };

            var originalFilter2 = new Filter
            {
                Label = "Test filter 2 - not changing",
                Name = "test_filter_2_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup2
                }
            };

            var replacementFilter1 = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementFilterGroup1
                }
            };

            var replacementFilter2 = new Filter
            {
                Label = "Test filter 2 - not changing",
                Name = "test_filter_2_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementFilterGroup2
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var originalLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                LocalAuthority = _derby
            };

            var replacementLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _derby
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[] {originalFilterItem1.Id, originalFilterItem2.Id},
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod,
                    FilterHierarchiesOptions = null, // it is null by default, but included to be visible to you, dear test reader
                },
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        ColumnGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                TableHeader.NewLocationHeader(GeographicLevel.LocalAuthority,
                                    originalLocation.Id.ToString())
                            }
                        },
                        Columns = new List<TableHeader>
                        {
                            new("2019_CY", TableHeaderType.TimePeriod),
                            new("2020_CY", TableHeaderType.TimePeriod)
                        },
                        RowGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                new TableHeader(originalFilterItem1.Id.ToString(), TableHeaderType.Filter),
                                new TableHeader(originalFilterItem2.Id.ToString(), TableHeaderType.Filter)
                            }
                        },
                        Rows = new List<TableHeader>
                        {
                            new(originalIndicator.Id.ToString(), TableHeaderType.Indicator)
                        }
                    }
                },
                Charts = new List<IChart>
                {
                    new LineChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>
                        {
                            {
                                "major",
                                new ChartAxisConfiguration
                                {
                                    DataSets = new List<ChartDataSet>
                                    {
                                        new()
                                        {
                                            Filters = new List<Guid>
                                            {
                                                originalFilterItem1.Id
                                            },
                                            Indicator = originalIndicator.Id,
                                            Location = new ChartDataSetLocation
                                            {
                                                Level = GeographicLevel.LocalAuthority.ToString().CamelCase(),
                                                Value = originalLocation.Id
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        Legend = new ChartLegend
                        {
                            Items = new List<ChartLegendItem>
                            {
                                new()
                                {
                                    DataSet = new ChartBaseDataSet
                                    {
                                        Filters = new List<Guid>
                                        {
                                            originalFilterItem1.Id
                                        },
                                        Indicator = originalIndicator.Id,
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = GeographicLevel.LocalAuthority.ToString().CamelCase(),
                                            Value = originalLocation.Id
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                ReleaseVersion = releaseVersion
            };

            var dataBlockVersion = new DataBlockVersion
            {
                Id = dataBlock.Id,
                ContentBlock = dataBlock
            };

            var footnoteForFilter = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter",
                filterFootnotes: new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = originalFilter1
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = originalFilterGroup1
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = originalFilterItem1
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = originalIndicator
                    }
                });

            var footnoteForSubject = CreateFootnote(statsReleaseVersion,
                "Test footnote for Subject",
                subject: originalReleaseSubject.Subject);

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    replacementLocation
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.Files.AddRange(originalFile, replacementFile);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                    replacementReleaseFile);
                contentDbContext.DataBlockVersions.AddRange(dataBlockVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalFilter1, originalFilter2,
                    replacementFilter1, replacementFilter2);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Location.AddRange(originalLocation);
                statisticsDbContext.Footnote.AddRange(footnoteForFilter, footnoteForFilterGroup,
                    footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
            releaseVersionService.Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id))
                .ReturnsAsync(Unit.Instance);

            var cacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            cacheKeyService.Setup(service =>
                    service.CreateCacheKeyForDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id))
                .ReturnsAsync(cacheKey);

            var privateBlobCacheService = new Mock<IPrivateBlobCacheService>(Strict);
            privateBlobCacheService.Setup(service => service.DeleteItemAsync(cacheKey))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    privateBlobCacheService: privateBlobCacheService.Object,
                    cacheKeyService: cacheKeyService.Object,
                    locationRepository: locationRepository.Object,
                    releaseVersionService: releaseVersionService.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.Replace(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(privateBlobCacheService,
                    cacheKeyService,
                    locationRepository,
                    releaseVersionService,
                    timePeriodService);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Check that the original file was unlinked from the replacement before the mock call to remove it.
                var originalFileUpdated = await contentDbContext.Files.FindAsync(originalFile.Id);
                Assert.NotNull(originalFileUpdated);
                Assert.Null(originalFileUpdated.ReplacedById);

                // Check that the replacement file was unlinked from the original.
                var replacementFileUpdated = await contentDbContext.Files.FindAsync(replacementFile.Id);
                Assert.NotNull(replacementFileUpdated);
                Assert.Null(replacementFileUpdated.ReplacingId);

                var replacedDataBlock = await contentDbContext.DataBlocks
                    .FirstAsync(db => db.Id == dataBlock.Id);
                Assert.Equal(dataBlock.Name, replacedDataBlock.Name);
                Assert.Equal(replacementReleaseSubject.SubjectId, replacedDataBlock.Query.SubjectId);

                Assert.Single(replacedDataBlock.Query.Indicators);
                Assert.Equal(replacementIndicator.Id, replacedDataBlock.Query.Indicators.First());

                var replacedFilterItemIds = replacedDataBlock.Query.GetFilterItemIds().ToList(); // all filterItems including those in FilterHierarchiesOptions
                Assert.Equal(2, replacedFilterItemIds.Count);
                Assert.Equal(2, replacedDataBlock.Query.GetNonHierarchicalFilterItemIds().Count()); // all filter items for the query are in `Filters` - there are no hierarchical filter items in the query
                Assert.Equal(replacementFilterItem1.Id, replacedFilterItemIds[0]);
                Assert.Equal(replacementFilterItem2.Id, replacedFilterItemIds[1]);

                Assert.Null(replacedDataBlock.Query.FilterHierarchiesOptions);

                var replacedLocationId = Assert.Single(replacedDataBlock.Query.LocationIds);
                Assert.Equal(replacementLocation.Id, replacedLocationId);

                Assert.NotNull(replacedDataBlock.Query.TimePeriod);
                timePeriod.AssertDeepEqualTo(replacedDataBlock.Query.TimePeriod);

                Assert.Equal(2, replacedDataBlock.Table.TableHeaders.Columns.Count);
                Assert.Equal(TableHeaderType.TimePeriod, replacedDataBlock.Table.TableHeaders.Columns.First().Type);
                Assert.Equal("2019_CY", replacedDataBlock.Table.TableHeaders.Columns.First().Value);
                Assert.Equal(TableHeaderType.TimePeriod,
                    replacedDataBlock.Table.TableHeaders.Columns.ElementAt(1).Type);
                Assert.Equal("2020_CY", replacedDataBlock.Table.TableHeaders.Columns.ElementAt(1).Value);
                Assert.Single(replacedDataBlock.Table.TableHeaders.ColumnGroups);
                Assert.Single(replacedDataBlock.Table.TableHeaders.ColumnGroups.First());
                Assert.Equal(TableHeaderType.Location,
                    replacedDataBlock.Table.TableHeaders.ColumnGroups.First().First().Type);
                Assert.Equal(replacementLocation.Id.ToString(),
                    replacedDataBlock.Table.TableHeaders.ColumnGroups.First().First().Value);
                Assert.Single(replacedDataBlock.Table.TableHeaders.Rows);
                Assert.Equal(TableHeaderType.Indicator, replacedDataBlock.Table.TableHeaders.Rows.First().Type);
                Assert.Equal(replacementIndicator.Id.ToString(),
                    replacedDataBlock.Table.TableHeaders.Rows.First().Value);

                Assert.Single(replacedDataBlock.Table.TableHeaders.RowGroups);
                var replacementRowGroup = replacedDataBlock.Table.TableHeaders.RowGroups.First().ToList();
                Assert.Equal(2, replacementRowGroup.Count);
                Assert.Equal(TableHeaderType.Filter, replacementRowGroup[0].Type);
                Assert.Equal(replacementFilterItem1.Id.ToString(), replacementRowGroup[0].Value);
                Assert.Equal(TableHeaderType.Filter, replacementRowGroup[1].Type);
                Assert.Equal(replacementFilterItem2.Id.ToString(), replacementRowGroup[1].Value);

                var chartMajorAxis = replacedDataBlock.Charts[0].Axes?["major"];
                Assert.NotNull(chartMajorAxis);
                var dataSet = Assert.Single(chartMajorAxis.DataSets);
                Assert.NotNull(dataSet);
                Assert.Single(dataSet.Filters);
                Assert.Equal(replacementFilterItem1.Id, dataSet.Filters[0]);
                Assert.Equal(replacementIndicator.Id, dataSet.Indicator);
                Assert.NotNull(dataSet.Location);
                Assert.Equal(replacementLocation.Id, dataSet.Location.Value);

                var chartLegendItems = replacedDataBlock.Charts[0].Legend?.Items;
                Assert.NotNull(chartLegendItems);
                var chartLegendItem = Assert.Single(chartLegendItems);
                Assert.NotNull(chartLegendItem);
                var filter = Assert.Single(chartLegendItem.DataSet.Filters);
                Assert.Equal(replacementFilterItem1.Id, filter);
                Assert.Equal(replacementIndicator.Id, chartLegendItem.DataSet.Indicator);
                Assert.NotNull(chartLegendItem.DataSet.Location);
                Assert.Equal(replacementLocation.Id, chartLegendItem.DataSet.Location.Value);

                var replacedFootnoteForFilter = await GetFootnoteById(statisticsDbContext, footnoteForFilter.Id);
                Assert.NotNull(replacedFootnoteForFilter);
                Assert.Equal(footnoteForFilter.Content, replacedFootnoteForFilter.Content);
                Assert.Single(replacedFootnoteForFilter.Filters);
                Assert.Empty(replacedFootnoteForFilter.FilterGroups);
                Assert.Empty(replacedFootnoteForFilter.FilterItems);
                Assert.Empty(replacedFootnoteForFilter.Indicators);
                Assert.Empty(replacedFootnoteForFilter.Subjects);

                Assert.Equal(replacementFilter1.Id, replacedFootnoteForFilter.Filters.First().Filter.Id);
                Assert.Equal(replacementFilter1.Label, replacedFootnoteForFilter.Filters.First().Filter.Label);
                Assert.Equal(replacementFilter1.Name, replacedFootnoteForFilter.Filters.First().Filter.Name);

                var replacedFootnoteForFilterGroup =
                    await GetFootnoteById(statisticsDbContext, footnoteForFilterGroup.Id);
                Assert.NotNull(replacedFootnoteForFilterGroup);
                Assert.Equal(footnoteForFilterGroup.Content, replacedFootnoteForFilterGroup.Content);
                Assert.Single(replacedFootnoteForFilterGroup.FilterGroups);
                Assert.Empty(replacedFootnoteForFilterGroup.Filters);
                Assert.Single(replacedFootnoteForFilterGroup.FilterGroups);
                Assert.Empty(replacedFootnoteForFilterGroup.FilterItems);
                Assert.Empty(replacedFootnoteForFilterGroup.Indicators);
                Assert.Empty(replacedFootnoteForFilterGroup.Subjects);

                Assert.Equal(replacementFilterGroup1.Id,
                    replacedFootnoteForFilterGroup.FilterGroups.First().FilterGroup.Id);
                Assert.Equal(replacementFilterGroup1.Label,
                    replacedFootnoteForFilterGroup.FilterGroups.First().FilterGroup.Label);

                var replacedFootnoteForFilterItem =
                    await GetFootnoteById(statisticsDbContext, footnoteForFilterItem.Id);
                Assert.NotNull(replacedFootnoteForFilterItem);
                Assert.Equal(footnoteForFilterItem.Content, replacedFootnoteForFilterItem.Content);
                Assert.Empty(replacedFootnoteForFilterItem.Filters);
                Assert.Empty(replacedFootnoteForFilterItem.FilterGroups);
                Assert.Single(replacedFootnoteForFilterItem.FilterItems);
                Assert.Empty(replacedFootnoteForFilterItem.Indicators);
                Assert.Empty(replacedFootnoteForFilterItem.Subjects);

                Assert.Equal(replacementFilterItem1.Id,
                    replacedFootnoteForFilterItem.FilterItems.First().FilterItem.Id);
                Assert.Equal(replacementFilterItem1.Label,
                    replacedFootnoteForFilterItem.FilterItems.First().FilterItem.Label);

                var replacedFootnoteForIndicator = await GetFootnoteById(statisticsDbContext, footnoteForIndicator.Id);
                Assert.NotNull(replacedFootnoteForIndicator);
                Assert.Equal(footnoteForIndicator.Content, replacedFootnoteForIndicator.Content);
                Assert.Empty(replacedFootnoteForIndicator.Filters);
                Assert.Empty(replacedFootnoteForIndicator.FilterGroups);
                Assert.Empty(replacedFootnoteForIndicator.FilterItems);
                Assert.Single(replacedFootnoteForIndicator.Indicators);
                Assert.Empty(replacedFootnoteForIndicator.Subjects);

                Assert.Equal(replacementIndicator.Id, replacedFootnoteForIndicator.Indicators.First().Indicator.Id);
                Assert.Equal(replacementIndicator.Label,
                    replacedFootnoteForIndicator.Indicators.First().Indicator.Label);
                Assert.Equal(replacementIndicator.Name, replacedFootnoteForIndicator.Indicators.First().Indicator.Name);

                var replacedFootnoteForSubject = await GetFootnoteById(statisticsDbContext, footnoteForSubject.Id);
                Assert.NotNull(replacedFootnoteForSubject);
                Assert.Equal(footnoteForSubject.Content, replacedFootnoteForSubject.Content);
                Assert.Empty(replacedFootnoteForSubject.Filters);
                Assert.Empty(replacedFootnoteForSubject.FilterGroups);
                Assert.Empty(replacedFootnoteForSubject.FilterItems);
                Assert.Empty(replacedFootnoteForSubject.Indicators);
                Assert.Single(replacedFootnoteForSubject.Subjects);

                Assert.Equal(replacementReleaseSubject.SubjectId,
                    replacedFootnoteForSubject.Subjects.First().Subject.Id);

                // Check the original data guidance has been retained on the replacement
                var updatedReleaseFile = await contentDbContext.ReleaseFiles
                    .FirstAsync(rf => rf.ReleaseVersionId == releaseVersion.Id
                                      && rf.FileId == replacementFile.Id);
                Assert.Equal("Original data set guidance", updatedReleaseFile.Summary);

                Assert.Null(updatedReleaseFile.FilterSequence);
                Assert.Null(updatedReleaseFile.IndicatorSequence);
            }
        }

        [Fact]
        public async Task Replace_ReplacesFilterHierarchiesOptions()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFilter1Id = Guid.NewGuid();
            var originalFilter2Id = Guid.NewGuid();

            var originalFilterItem1Id = Guid.NewGuid();
            var originalFilterItem2Id = Guid.NewGuid();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId,
                FilterHierarchies = [
                    new DataSetFileFilterHierarchy(
                        FilterIds: [ originalFilter1Id, originalFilter2Id, ],
                        Tiers: [
                            new Dictionary<Guid, List<Guid>>
                            {
                                { originalFilterItem1Id, [originalFilterItem2Id] }
                            },
                        ]
                    ),
                ],
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile,
                Summary = "Original data set guidance"
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile,
                Summary = null
            };

            var originalFilter1 = new Filter
            {
                Id = originalFilter1Id,
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Default group - not changing",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = originalFilterItem1Id,
                                Label = "Test filter item - not changing"
                            },
                        }
                    }
                }
            };

            var originalFilter2 = new Filter
            {
                Id = originalFilter2Id,
                Label = "Test filter 2 - not changing",
                Name = "test_filter_2_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Default group - not changing",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = originalFilterItem2Id,
                                Label = "Test filter item - not changing"
                            },
                        }
                    },
                }
            };

            var replacementFilter1 = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Default group - not changing",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Test filter item - not changing"
                            },
                        }
                    },
                }
            };

            var replacementFilter2 = new Filter
            {
                Label = "Test filter 2 - not changing",
                Name = "test_filter_2_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Default group - not changing",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Id = Guid.NewGuid(),
                                Label = "Test filter item - not changing"
                            },
                        }
                    },
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var originalLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                LocalAuthority = _derby
            };

            var replacementLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _derby
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = [],
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod,
                    FilterHierarchiesOptions = new List<FilterHierarchyOptions>
                    {
                        new()
                        {
                            LeafFilterId = originalFilter2.Id,
                            // This would actually be an invalid data set, as there should also be two
                            // additional Total filterItems for both filters in a filter hierarchy
                            Options = [[originalFilterItem1Id, originalFilterItem2Id]],
                        }
                    }

                },
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        ColumnGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                TableHeader.NewLocationHeader(GeographicLevel.LocalAuthority,
                                    originalLocation.Id.ToString())
                            }
                        },
                        Columns = new List<TableHeader>
                        {
                            new("2019_CY", TableHeaderType.TimePeriod),
                            new("2020_CY", TableHeaderType.TimePeriod)
                        },
                        RowGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                new TableHeader(originalFilterItem1Id.ToString(), TableHeaderType.Filter),
                                new TableHeader(originalFilterItem2Id.ToString(), TableHeaderType.Filter)
                            }
                        },
                        Rows = new List<TableHeader>
                        {
                            new(originalIndicator.Id.ToString(), TableHeaderType.Indicator)
                        }
                    }
                },
                Charts = [],
                ReleaseVersion = releaseVersion
            };

            var dataBlockVersion = new DataBlockVersion
            {
                Id = dataBlock.Id,
                ContentBlock = dataBlock
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    replacementLocation
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.Files.AddRange(originalFile, replacementFile);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile,
                    replacementReleaseFile);
                contentDbContext.DataBlockVersions.AddRange(dataBlockVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalFilter1, originalFilter2,
                    replacementFilter1, replacementFilter2);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Location.AddRange(originalLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
            releaseVersionService.Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id, false))
                .ReturnsAsync(Unit.Instance);

            var cacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            cacheKeyService.Setup(service =>
                    service.CreateCacheKeyForDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id))
                .ReturnsAsync(cacheKey);

            var privateBlobCacheService = new Mock<IPrivateBlobCacheService>(Strict);
            privateBlobCacheService.Setup(service => service.DeleteItemAsync(cacheKey))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    privateBlobCacheService: privateBlobCacheService.Object,
                    cacheKeyService: cacheKeyService.Object,
                    locationRepository: locationRepository.Object,
                    releaseVersionService: releaseVersionService.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.Replace(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(privateBlobCacheService,
                    cacheKeyService,
                    locationRepository,
                    releaseVersionService,
                    timePeriodService);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the original file was unlinked from the replacement before the mock call to remove it.
                var originalFileUpdated = await contentDbContext.Files.FindAsync(originalFile.Id);
                Assert.NotNull(originalFileUpdated);
                Assert.Null(originalFileUpdated.ReplacedById);

                // Check that the replacement file was unlinked from the original.
                var replacementFileUpdated = await contentDbContext.Files.FindAsync(replacementFile.Id);
                Assert.NotNull(replacementFileUpdated);
                Assert.Null(replacementFileUpdated.ReplacingId);

                var replacedDataBlock = await contentDbContext.DataBlocks
                    .FirstAsync(db => db.Id == dataBlock.Id);
                Assert.Equal(dataBlock.Name, replacedDataBlock.Name);
                Assert.Equal(replacementReleaseSubject.SubjectId, replacedDataBlock.Query.SubjectId);

                Assert.Single(replacedDataBlock.Query.Indicators);
                Assert.Equal(replacementIndicator.Id, replacedDataBlock.Query.Indicators.First());

                var replacedLocationId = Assert.Single(replacedDataBlock.Query.LocationIds);
                Assert.Equal(replacementLocation.Id, replacedLocationId);

                Assert.NotNull(replacedDataBlock.Query.TimePeriod);
                timePeriod.AssertDeepEqualTo(replacedDataBlock.Query.TimePeriod);

                Assert.Empty(replacedDataBlock.Query.GetNonHierarchicalFilterItemIds());

                var hierarchiesOptions = replacedDataBlock.Query.FilterHierarchiesOptions;
                Assert.NotNull(hierarchiesOptions);

                var hierarchyOptions = Assert.Single(hierarchiesOptions);

                Assert.Equal(replacementFilter2.Id, hierarchyOptions.LeafFilterId);
                Assert.Equal([[
                        replacementFilter1.FilterGroups[0].FilterItems[0].Id,
                        replacementFilter2.FilterGroups[0].FilterItems[0].Id
                    ]],
                    hierarchyOptions.Options);
            }
        }

        [Fact]
        public async Task Replace_MapChart_ReplacesChartDataSetConfigs()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalFilterItem1 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var originalFilterItem2 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var replacementFilterItem1 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var replacementFilterItem2 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var originalFilterGroup1 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem1
                }
            };

            var originalFilterGroup2 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem2
                }
            };

            var replacementFilterGroup1 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem1
                }
            };

            var replacementFilterGroup2 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem2
                }
            };

            var originalFilter1 = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup1
                }
            };

            var originalFilter2 = new Filter
            {
                Label = "Test filter 2 - not changing",
                Name = "test_filter_2_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup2
                }
            };

            var replacementFilter1 = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementFilterGroup1
                }
            };

            var replacementFilter2 = new Filter
            {
                Label = "Test filter 2 - not changing",
                Name = "test_filter_2_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementFilterGroup2
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var originalLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                LocalAuthority = _derby
            };

            var replacementLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _derby
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[] {originalFilterItem1.Id, originalFilterItem2.Id},
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod
                },
                Table = new TableBuilderConfiguration(),
                Charts = new List<IChart>
                {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>(),
                        Map = new MapChartConfig
                        {
                            DataSetConfigs = new List<ChartDataSetConfig>
                            {
                                new()
                                {
                                    DataSet = new ChartBaseDataSet
                                    {
                                        Filters = new List<Guid>
                                        {
                                            originalFilterItem1.Id,
                                            originalFilterItem2.Id
                                        },
                                        Indicator = originalIndicator.Id,
                                        Location = new ChartDataSetLocation
                                        {
                                            Level = GeographicLevel.LocalAuthority.ToString().CamelCase(),
                                            Value = originalLocation.Id
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                ReleaseVersion = releaseVersion
            };

            var dataBlockVersion = new DataBlockVersion
            {
                Id = dataBlock.Id,
                ContentBlock = dataBlock
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    replacementLocation
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.Files.AddRange(originalFile, replacementFile);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalFilter1,
                    originalFilter2,
                    replacementFilter1,
                    replacementFilter2);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                statisticsDbContext.Location.AddRange(originalLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            var cacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            cacheKeyService.Setup(service =>
                    service.CreateCacheKeyForDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id))
                .ReturnsAsync(cacheKey);

            var privateBlobCacheService = new Mock<IPrivateBlobCacheService>(Strict);
            privateBlobCacheService.Setup(service => service.DeleteItemAsync(cacheKey))
                .Returns(Task.CompletedTask);

            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
            releaseVersionService.Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    privateBlobCacheService: privateBlobCacheService.Object,
                    cacheKeyService: cacheKeyService.Object,
                    locationRepository: locationRepository.Object,
                    releaseVersionService: releaseVersionService.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.Replace(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(privateBlobCacheService,
                    cacheKeyService,
                    locationRepository,
                    releaseVersionService,
                    timePeriodService);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var replacedDataBlock = await contentDbContext.DataBlocks
                    .FirstAsync(db => db.Id == dataBlock.Id);

                var mapChart = Assert.IsType<MapChart>(replacedDataBlock.Charts[0]);

                var chartDataSetConfigs = mapChart.Map.DataSetConfigs;
                Assert.NotNull(chartDataSetConfigs);
                var chartDataSetConfig = Assert.Single(chartDataSetConfigs);

                Assert.Equal(2, chartDataSetConfig.DataSet.Filters.Count);
                Assert.Equal(replacementFilterItem1.Id, chartDataSetConfig.DataSet.Filters[0]);
                Assert.Equal(replacementFilterItem2.Id, chartDataSetConfig.DataSet.Filters[1]);

                Assert.Equal(replacementIndicator.Id, chartDataSetConfig.DataSet.Indicator);

                Assert.NotNull(chartDataSetConfig.DataSet.Location);
                Assert.Equal(replacementLocation.Id, chartDataSetConfig.DataSet.Location!.Value);
            }
        }

        [Fact]
        public async Task Replace_MapChart_ReplacesChartDataSetConfigsWithNullLocation()
        {
            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            var originalFilterItem1 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var replacementFilterItem1 = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var originalFilterGroup1 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem1
                }
            };

            var replacementFilterGroup1 = new FilterGroup
            {
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem1
                }
            };

            var originalFilter1 = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = originalReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup1
                }
            };

            var replacementFilter1 = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementReleaseSubject.Subject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementFilterGroup1
                }
            };

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var replacementIndicator = new Indicator
            {
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = originalReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementReleaseSubject.Subject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
            };

            var replacementLocation = new Location
            {
                Id = Guid.NewGuid(),
                GeographicLevel = GeographicLevel.LocalAuthority,
                Country = _england,
                LocalAuthority = _derby
            };

            var timePeriod = new TimePeriodQuery
            {
                StartYear = 2019,
                StartCode = CalendarYear,
                EndYear = 2020,
                EndCode = CalendarYear
            };

            var dataBlock = new DataBlock
            {
                Name = "Test DataBlock",
                Query = new FullTableQuery
                {
                    SubjectId = originalReleaseSubject.SubjectId,
                    Filters = new[] {originalFilterItem1.Id},
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = new List<Guid>(),
                    TimePeriod = timePeriod
                },
                Table = new TableBuilderConfiguration(),
                Charts = new List<IChart>
                {
                    new MapChart
                    {
                        Axes = new Dictionary<string, ChartAxisConfiguration>(),
                        Map = new MapChartConfig
                        {
                            DataSetConfigs = new List<ChartDataSetConfig>
                            {
                                new()
                                {
                                    DataSet = new ChartBaseDataSet
                                    {
                                        Filters = new List<Guid>
                                        {
                                            originalFilterItem1.Id,
                                        },
                                        Indicator = originalIndicator.Id,
                                        Location = null,
                                    }
                                }
                            }
                        }
                    }
                },
                ReleaseVersion = releaseVersion
            };

            var dataBlockVersion = new DataBlockVersion
            {
                Id = dataBlock.Id,
                ContentBlock = dataBlock
            };

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>
                {
                    replacementLocation
                });

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.Files.AddRange(originalFile, replacementFile);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                contentDbContext.DataBlocks.AddRange(dataBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalFilter1,
                    replacementFilter1);
                statisticsDbContext.IndicatorGroup.AddRange(originalIndicatorGroup,
                    replacementIndicatorGroup);
                await statisticsDbContext.SaveChangesAsync();
            }

            var cacheKey = new DataBlockTableResultCacheKey(dataBlockVersion);

            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            cacheKeyService.Setup(service =>
                    service.CreateCacheKeyForDataBlock(dataBlock.ReleaseVersionId, dataBlock.Id))
                .ReturnsAsync(cacheKey);

            var privateBlobCacheService = new Mock<IPrivateBlobCacheService>(Strict);
            privateBlobCacheService.Setup(service => service.DeleteItemAsync(cacheKey))
                .Returns(Task.CompletedTask);

            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
            releaseVersionService.Setup(service => service.RemoveDataFiles(
                releaseVersion.Id, originalFile.Id)).ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    privateBlobCacheService: privateBlobCacheService.Object,
                    cacheKeyService: cacheKeyService.Object,
                    locationRepository: locationRepository.Object,
                    releaseVersionService: releaseVersionService.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.Replace(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(privateBlobCacheService,
                    cacheKeyService,
                    locationRepository,
                    releaseVersionService,
                    timePeriodService);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var replacedDataBlock = await contentDbContext.DataBlocks.SingleAsync(db => db.Id == dataBlock.Id);

                var mapChart = Assert.IsType<MapChart>(replacedDataBlock.Charts[0]);

                var chartDataSetConfigs = mapChart.Map.DataSetConfigs;
                Assert.NotNull(chartDataSetConfigs);
                var chartDataSetConfig = Assert.Single(chartDataSetConfigs);

                var filterId = Assert.Single(chartDataSetConfig.DataSet.Filters);
                Assert.Equal(replacementFilterItem1.Id, filterId);

                Assert.Equal(replacementIndicator.Id, chartDataSetConfig.DataSet.Indicator);

                Assert.Null(chartDataSetConfig.DataSet.Location);
            }
        }

        [Fact]
        public async Task Replace_FilterSequenceIsReplaced()
        {
            // Basic test replacing a filter sequence, exercising the service with in-memory data and dependencies.
            // See ReplaceServiceHelperTests.ReplaceFilterSequence for a more comprehensive test of the actual replacement.

            var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

            var statsReleaseVersion = _fixture.DefaultStatsReleaseVersion()
                .WithId(releaseVersion.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsReleaseVersion)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = releaseVersion,
                File = replacementFile
            };

            // Define a set of filters, filter groups and filter items belonging to the original subject
            var originalFilters = new List<Filter>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter a",
                    Name = "filter_a",
                    Subject = originalReleaseSubject.Subject,
                    FilterGroups = new List<FilterGroup>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Group a",
                            FilterItems = new List<FilterItem>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Label = "Item a"
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Label = "Item b"
                                }
                            }
                        }
                    }
                }
            };

            // Define a sequence for the original subject which is expected to be updated after the replacement
            originalReleaseFile.FilterSequence = new List<FilterSequenceEntry>
            {
                // Filter a
                new(originalFilters[0].Id,
                    new List<FilterGroupSequenceEntry>
                    {
                        // Group a
                        new(
                            originalFilters[0].FilterGroups[0].Id,
                            new List<Guid>
                            {
                                // Item b, Indicator a
                                originalFilters[0].FilterGroups[0].FilterItems[1].Id,
                                originalFilters[0].FilterGroups[0].FilterItems[0].Id
                            }
                        )
                    }
                )
            };

            // Define the set of filters, filter groups and filter items belonging to the replacement subject
            var replacementFilters = new List<Filter>
            {
                // 'Filter a' is identical
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter a",
                    Name = "filter_a",
                    Subject = replacementReleaseSubject.Subject,
                    FilterGroups = new List<FilterGroup>
                    {
                        // 'Group a' is identical
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Group a",
                            FilterItems = new List<FilterItem>
                            {
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Label = "Item a"
                                },
                                new()
                                {
                                    Id = Guid.NewGuid(),
                                    Label = "Item b"
                                }
                            }
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(releaseVersion);
                contentDbContext.Files.AddRange(originalFile, replacementFile);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsReleaseVersion);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.Filter.AddRange(originalFilters);
                statisticsDbContext.Filter.AddRange(replacementFilters);
                await statisticsDbContext.SaveChangesAsync();
            }

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
            releaseVersionService.Setup(service => service.RemoveDataFiles(releaseVersion.Id, originalFile.Id))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    releaseVersionService: releaseVersionService.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.Replace(
                    releaseVersionId: releaseVersion.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    releaseVersionService,
                    timePeriodService);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var replacedReleaseFile = await contentDbContext.ReleaseFiles
                    .SingleAsync(rf => rf.ReleaseVersionId == statsReleaseVersion.Id
                                       && rf.File.SubjectId == replacementReleaseSubject.SubjectId);

                // Verify the updated sequence of filters on the replacement subject
                var updatedSequence = replacedReleaseFile.FilterSequence;
                Assert.NotNull(updatedSequence);

                // 'Filter a' should be the only filter in the sequence
                var filterA = Assert.Single(updatedSequence);
                Assert.Equal(replacementFilters[0].Id, filterA.Id);
                var filterAGroups = filterA.ChildSequence;

                // 'Group a' should be the only group in the sequence
                var filterAGroupA = Assert.Single(filterAGroups);
                Assert.Equal(replacementFilters[0].FilterGroups[0].Id, filterAGroupA.Id);

                // 'Group a' should still have two filter items in the same order as the original sequence
                Assert.Equal(2, filterAGroupA.ChildSequence.Count);
                Assert.Equal(replacementFilters[0].FilterGroups[0].FilterItems[1].Id, filterAGroupA.ChildSequence[0]);
                Assert.Equal(replacementFilters[0].FilterGroups[0].FilterItems[0].Id, filterAGroupA.ChildSequence[1]);
            }
        }

        [Fact]
        public async Task Replace_IndicatorSequenceIsReplaced()
        {
            // Basic test replacing an indicator sequence, exercising the service with in-memory data and dependencies.
            // See ReplaceServiceHelperTests.ReplaceIndicatorSequence for a more comprehensive test of the actual replacement.

            var contentRelease = _fixture.DefaultReleaseVersion().Generate();

            var statsRelease = _fixture.DefaultStatsReleaseVersion()
                .WithId(contentRelease.Id)
                .Generate();

            var (originalReleaseSubject, replacementReleaseSubject) = _fixture.DefaultReleaseSubject()
                .WithReleaseVersion(statsRelease)
                .WithSubjects(_fixture.DefaultSubject().Generate(2))
                .GenerateTuple2();

            var originalFile = new File
            {
                Type = FileType.Data,
                SubjectId = originalReleaseSubject.SubjectId
            };

            var replacementFile = new File
            {
                Type = FileType.Data,
                SubjectId = replacementReleaseSubject.SubjectId,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                ReleaseVersion = contentRelease,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                ReleaseVersion = contentRelease,
                File = replacementFile
            };

            // Define a set of indicator groups and indicators belonging to the original subject
            var originalGroups = new List<IndicatorGroup>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Group a",
                    Subject = originalReleaseSubject.Subject,
                    Indicators = new List<Indicator>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Indicator a",
                            Name = "indicator_a"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Indicator b",
                            Name = "indicator_b"
                        }
                    }
                }
            };

            // Define a sequence for the original subject which is expected to be updated after the replacement
            originalReleaseFile.IndicatorSequence = new List<IndicatorGroupSequenceEntry>
            {
                // Group a
                new(
                    originalGroups[0].Id,
                    new List<Guid>
                    {
                        // Indicator b, Indicator a
                        originalGroups[0].Indicators[1].Id,
                        originalGroups[0].Indicators[0].Id
                    }
                )
            };

            // Define the set of indicator groups and indicators belonging to the replacement subject
            var replacementGroups = new List<IndicatorGroup>
            {
                // 'Group a' is identical
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Group a",
                    Subject = replacementReleaseSubject.Subject,
                    Indicators = new List<Indicator>
                    {
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Indicator a",
                            Name = "indicator_a"
                        },
                        new()
                        {
                            Id = Guid.NewGuid(),
                            Label = "Indicator b",
                            Name = "indicator_b"
                        }
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.ReleaseVersions.AddRange(contentRelease);
                contentDbContext.Files.AddRange(originalFile, replacementFile);
                contentDbContext.ReleaseFiles.AddRange(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                statisticsDbContext.ReleaseVersion.AddRange(statsRelease);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                statisticsDbContext.IndicatorGroup.AddRange(originalGroups);
                statisticsDbContext.IndicatorGroup.AddRange(replacementGroups);
                statisticsDbContext.ReleaseSubject.AddRange(originalReleaseSubject,
                    replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var locationRepository = new Mock<ILocationRepository>(Strict);
            locationRepository.Setup(service => service.GetDistinctForSubject(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<Location>());

            var timePeriodService = new Mock<ITimePeriodService>(Strict);
            timePeriodService.Setup(service => service.GetTimePeriods(replacementReleaseSubject.SubjectId))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var releaseVersionService = new Mock<IReleaseVersionService>(Strict);
            releaseVersionService.Setup(service => service.RemoveDataFiles(contentRelease.Id, originalFile.Id))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext,
                    statisticsDbContext,
                    locationRepository: locationRepository.Object,
                    releaseVersionService: releaseVersionService.Object,
                    timePeriodService: timePeriodService.Object);

                var result = await replacementService.Replace(
                    releaseVersionId: contentRelease.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(locationRepository,
                    releaseVersionService,
                    timePeriodService);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var replacedReleaseFile = await contentDbContext.ReleaseFiles
                    .SingleAsync(rf => rf.ReleaseVersionId == statsRelease.Id
                                       && rf.File.SubjectId == replacementReleaseSubject.SubjectId);

                // Verify the updated sequence of indicators on the replacement subject
                var updatedSequence = replacedReleaseFile.IndicatorSequence;
                Assert.NotNull(updatedSequence);

                // 'Group a' should be the only group in the sequence
                var groupA = Assert.Single(updatedSequence);
                Assert.Equal(replacementGroups[0].Id, groupA.Id);

                // 'Group a' should still have two indicators in the same order as the original sequence
                Assert.Equal(2, groupA.ChildSequence.Count);
                Assert.Equal(replacementGroups[0].Indicators[1].Id, groupA.ChildSequence[0]);
                Assert.Equal(replacementGroups[0].Indicators[0].Id, groupA.ChildSequence[1]);
            }
        }

        private static Footnote CreateFootnote(ReleaseVersion releaseVersion,
            string content,
            List<FilterFootnote>? filterFootnotes = null,
            List<FilterGroupFootnote>? filterGroupFootnotes = null,
            List<FilterItemFootnote>? filterItemFootnotes = null,
            List<IndicatorFootnote>? indicatorFootnotes = null,
            Subject? subject = null)
        {
            return new Footnote
            {
                Content = content,
                Filters = filterFootnotes ?? new List<FilterFootnote>(),
                FilterGroups = filterGroupFootnotes ?? new List<FilterGroupFootnote>(),
                FilterItems = filterItemFootnotes ?? new List<FilterItemFootnote>(),
                Indicators = indicatorFootnotes ?? new List<IndicatorFootnote>(),
                Subjects = subject != null
                    ? new List<SubjectFootnote>
                    {
                        new()
                        {
                            Subject = subject
                        }
                    }
                    : new List<SubjectFootnote>(),
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        ReleaseVersion = releaseVersion
                    }
                }
            };
        }

        private static async Task<Footnote> GetFootnoteById(StatisticsDbContext context, Guid id)
        {
            return await context.Footnote
                .Include(footnote => footnote.Filters)
                .ThenInclude(filterFootnote => filterFootnote.Filter)
                .Include(footnote => footnote.FilterGroups)
                .ThenInclude(filterGroupFootnote => filterGroupFootnote.FilterGroup)
                .Include(footnote => footnote.FilterItems)
                .ThenInclude(filterItemFootnote => filterItemFootnote.FilterItem)
                .Include(footnote => footnote.Indicators)
                .ThenInclude(indicatorFootnote => indicatorFootnote.Indicator)
                .Include(footnote => footnote.Subjects)
                .ThenInclude(subjectFootnote => subjectFootnote.Subject)
                .SingleAsync(footnote => footnote.Id == id);
        }

        private static ReplacementService BuildReplacementService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            ILocationRepository? locationRepository = null,
            IReleaseVersionService? releaseVersionService = null,
            IDataSetVersionService? dataSetVersionService = null,
            ITimePeriodService? timePeriodService = null,
            ICacheKeyService? cacheKeyService = null,
            IPrivateBlobCacheService? privateBlobCacheService = null,
            IDataSetVersionMappingService? dataSetVersionMappingService = null,
            IOptions<FeatureFlagsOptions>? featureFlags = null
            )
        {
            featureFlags ??= Microsoft.Extensions.Options.Options.Create(new FeatureFlagsOptions()
            {
                EnableReplacementOfPublicApiDataSets = false
            });
            return new ReplacementService(
                contentDbContext,
                statisticsDbContext,
                new FilterRepository(statisticsDbContext),
                new IndicatorRepository(statisticsDbContext),
                new IndicatorGroupRepository(statisticsDbContext),
                locationRepository ?? Mock.Of<ILocationRepository>(Strict),
                new FootnoteRepository(statisticsDbContext),
                releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
                dataSetVersionService ?? Mock.Of<IDataSetVersionService>(),
                timePeriodService ?? Mock.Of<ITimePeriodService>(Strict),
                AlwaysTrueUserService().Object,
                cacheKeyService ?? Mock.Of<ICacheKeyService>(Strict),
                privateBlobCacheService ?? Mock.Of<IPrivateBlobCacheService>(Strict),
                dataSetVersionMappingService ?? Mock.Of<IDataSetVersionMappingService>(Strict),
                featureFlags
            );
        }
    }
}
