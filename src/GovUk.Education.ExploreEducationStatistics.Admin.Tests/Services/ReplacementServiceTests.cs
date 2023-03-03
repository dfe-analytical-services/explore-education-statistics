#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Cache;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Chart;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReplacementServiceTests
    {
        private readonly Country _england = new("E92000001", "England");
        private readonly LocalAuthority _derby = new("E06000015", "", "Derby");

        [Fact]
        public async Task GetReplacementPlan_FileHasWrongFileType()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentReleaseVersion1 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var contentReleaseVersion2 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id
            };

            var statsReleaseVersion1 = new Release
            {
                Id = contentReleaseVersion1.Id
            };

            var statsReleaseVersion2 = new Release
            {
                Id = contentReleaseVersion2.Id
            };

            // Use a FileType that is not Data
            var originalFile = new File
            {
                Filename = "original.csv",
                Type = Ancillary,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                File = originalFile
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = replacementFile
            };

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentReleaseVersion2.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                result.AssertBadRequest(ReplacementFileTypesMustBeData);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_OriginalSubjectBelongsToDifferentRelease()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentRelease1Version1 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var contentRelease1Version2 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentRelease1Version1.Id
            };

            // Create an unrelated Release
            var contentRelease2 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var statsRelease1Version1 = new Release
            {
                Id = contentRelease1Version1.Id
            };

            var statsRelease1Version2 = new Release
            {
                Id = contentRelease1Version2.Id
            };

            var statsRelease2 = new Release
            {
                Id = contentRelease2.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Release = contentRelease1Version1,
                File = originalFile
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Release = contentRelease1Version2,
                File = originalFile
            };

            // Link the replacement to the unrelated Release
            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentRelease2,
                File = replacementFile
            };

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentRelease1Version1, contentRelease1Version2, contentRelease2);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease1Version1, statsRelease1Version2, statsRelease2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentRelease2.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetReplacementPlan_OriginalSubjectNotUsed()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentReleaseVersion1 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var contentReleaseVersion2 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id
            };

            var statsReleaseVersion1 = new Release
            {
                Id = contentReleaseVersion1.Id
            };

            var statsReleaseVersion2 = new Release
            {
                Id = contentReleaseVersion2.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                File = originalFile
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = replacementFile
            };

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>());

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentReleaseVersion2.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                var replacementPlan = result.AssertRight();

                Assert.Equal(originalSubject.Id, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementSubject.Id, replacementPlan.ReplacementSubjectId);

                Assert.Empty(replacementPlan.DataBlocks);
                Assert.Empty(replacementPlan.Footnotes);
                Assert.True(replacementPlan.Valid);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_NoReplacementDataPresent_ReplacementInvalid()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentReleaseVersion1 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var contentReleaseVersion2 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id
            };

            var statsReleaseVersion1 = new Release
            {
                Id = contentReleaseVersion1.Id
            };

            var statsReleaseVersion2 = new Release
            {
                Id = contentReleaseVersion2.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                File = originalFile
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentReleaseVersion2,
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
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup
                }
            };

            var replacementFilter = new Filter
            {
                Label = "Replacement Test filter",
                Name = "replacement_test_filter",
                Subject = replacementSubject,
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
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Replacement Default group",
                Subject = replacementSubject,
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
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[] {originalFilterItem.Id},
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentReleaseVersion2,
                ContentBlock = dataBlock
            };

            var footnoteForSubject = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Subject",
                subject: originalSubject);

            var footnoteForFilter = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter",
                filterFootnotes: new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = originalFilter
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = originalFilterGroup
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = originalFilterItem
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = originalIndicator
                    }
                });

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>());

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalFilter, replacementFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddRangeAsync(footnoteForFilter, footnoteForFilterGroup,
                    footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
                await statisticsDbContext.AddAsync(originalLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentReleaseVersion2.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                var replacementPlan = result.AssertRight();

                Assert.Equal(originalSubject.Id, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementSubject.Id, replacementPlan.ReplacementSubjectId);

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
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var statsRelease = new Release
            {
                Id = contentRelease.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
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
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalDefaultFilterGroup
                }
            };

            var replacementDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementSubject,
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
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementSubject,
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
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[]
                    {
                        originalDefaultFilterItem.Id,
                        originalDefaultFilterItem2.Id
                    },
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(location.Id),
                    TimePeriod = timePeriod
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentRelease,
                ContentBlock = dataBlock
            };

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>
                {
                    location
                });

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentRelease);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalDefaultFilter, replacementDefaultFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddAsync(location);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentRelease.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

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
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var statsRelease = new Release
            {
                Id = contentRelease.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
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
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalDefaultFilterGroup
                }
            };

            var replacementDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementSubject,
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
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementSubject,
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
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[]
                    {
                        originalDefaultFilterItem.Id
                    },
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(location.Id),
                    TimePeriod = timePeriod
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentRelease,
                ContentBlock = dataBlock
            };

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>
                {
                    location
                });

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentRelease);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalDefaultFilter, replacementDefaultFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddAsync(location);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentRelease.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

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
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var statsRelease = new Release
            {
                Id = contentRelease.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
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
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalDefaultFilterGroup
                }
            };

            var replacementDefaultFilter = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementSubject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementDefaultFilterGroup
                }
            };

            var replacementNewlyIntroducedFilter = new Filter
            {
                Label = "Newly introduced filter",
                Name = "newly_introduced_filter",
                Subject = replacementSubject,
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
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementSubject,
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
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[]
                    {
                        originalDefaultFilterItem.Id
                    },
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(location.Id),
                    TimePeriod = timePeriod
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentRelease,
                ContentBlock = dataBlock
            };

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>
                {
                    location
                });

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentRelease);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalDefaultFilter,
                    replacementDefaultFilter, replacementNewlyIntroducedFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddAsync(location);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentRelease.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

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
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid()
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
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
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup
                }
            };

            var replacementFilter = new Filter
            {
                Label = "Filter - not changing",
                Name = "filter_not_changing",
                Subject = replacementSubject,
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
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementSubject,
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
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[] {originalFilterItem.Id},
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentRelease,
                ContentBlock = dataBlock
            };

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>
                {
                    replacementLocation
                });

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentRelease);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalFilter, replacementFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddAsync(originalLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentRelease.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                var replacementPlan = result.AssertRight();

                Assert.Equal(originalSubject.Id, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementSubject.Id, replacementPlan.ReplacementSubjectId);

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

        [Fact]
        public async Task GetReplacementPlan_AllReplacementDataPresent_ReplacementValid()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentReleaseVersion1 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var contentReleaseVersion2 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id
            };

            var statsReleaseVersion1 = new Release
            {
                Id = contentReleaseVersion1.Id
            };

            var statsReleaseVersion2 = new Release
            {
                Id = contentReleaseVersion2.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                File = originalFile
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentReleaseVersion2,
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
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalDefaultFilterGroup
                }
            };

            var originalSchoolTypeFilter = new Filter
            {
                Label = "School type",
                Name = "school_type",
                Subject = originalSubject,
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
                Subject = replacementSubject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementDefaultFilterGroup
                }
            };

            var replacementSchoolTypeFilter = new Filter
            {
                Label = "School type",
                Name = "school_type",
                Subject = replacementSubject,
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
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementSubject,
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
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[]
                    {
                        originalDefaultFilterItem.Id,
                        originalPrimarySchoolsFilterItem.Id,
                        originalPrimaryAndSecondarySchoolsFilterItem.Id,
                    },
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(location.Id),
                    TimePeriod = timePeriod
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentReleaseVersion2,
                ContentBlock = dataBlock
            };

            var footnoteForFilter = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter",
                filterFootnotes: new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = originalDefaultFilter
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = originalDefaultFilterGroup
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = originalDefaultFilterItem
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = originalIndicator
                    }
                });

            var footnoteForSubject = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Subject",
                subject: originalSubject);

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>
                {
                    location
                });

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalDefaultFilter, originalSchoolTypeFilter,
                    replacementDefaultFilter, replacementSchoolTypeFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddAsync(location);
                await statisticsDbContext.AddRangeAsync(footnoteForFilter, footnoteForFilterGroup,
                    footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(
                    releaseId: contentReleaseVersion2.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                var replacementPlan = result.AssertRight();

                Assert.Equal(originalSubject.Id, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementSubject.Id, replacementPlan.ReplacementSubjectId);

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
                    dataBlockSchoolTypeFilterPlan.Value.Groups.First(g => g.Key == originalIndividualSchoolTypeFilterGroup.Id);

                Assert.Equal(originalIndividualSchoolTypeFilterGroup.Id,
                    dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Id);
                Assert.Equal(originalIndividualSchoolTypeFilterGroup.Label,
                    dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Label);
                Assert.Single(dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Filters);
                Assert.True(dataBlockIndividualSchoolTypeFilterGroupPlan.Value.Valid);

                var dataBlockCombinedSchoolTypeFilterGroupPlan =
                    dataBlockSchoolTypeFilterPlan.Value.Groups.First(g => g.Key == originalCombinedSchoolTypeFilterGroup.Id);

                Assert.Equal(originalCombinedSchoolTypeFilterGroup.Id, dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Id);
                Assert.Equal(originalCombinedSchoolTypeFilterGroup.Label, dataBlockCombinedSchoolTypeFilterGroupPlan.Value.Label);
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
                Assert.Equal(originalDefaultFilterGroup.Filter.Label, footnoteForFilterGroupFilterGroupPlan.FilterLabel);
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
                Assert.Equal(originalIndicator.IndicatorGroup.Label, footnoteForIndicatorIndicatorGroupPlan.Value.Label);
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

                Assert.True(replacementPlan.Valid);
            }
        }

        [Fact]
        public async Task Replace_ReplacementPlanInvalid()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var statsRelease = new Release
            {
                Id = contentRelease.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
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
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new Guid[] { },
                    Indicators = new Guid[] { },
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod
                },
                Table = table
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentRelease,
                ContentBlock = dataBlock
            };

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>());

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddAsync(originalLocation);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.Replace(
                    releaseId: contentRelease.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                result.AssertBadRequest(ReplacementMustBeValid);
            }
        }

        [Fact]
        public async Task Replace()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };

            var contentReleaseVersion1 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Publication = publication
            };

            var contentReleaseVersion2 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id,
                Publication = publication
            };

            var statsReleaseVersion1 = new Release
            {
                Id = contentReleaseVersion1.Id
            };

            var statsReleaseVersion2 = new Release
            {
                Id = contentReleaseVersion2.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                File = originalFile
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = replacementFile
            };

            var originalReleaseSubject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion1,
                Subject = originalSubject,
                DataGuidance = "Original guidance version 1"
            };

            var originalReleaseSubject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = originalSubject,
                DataGuidance = "Original guidance version 2"
            };

            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = replacementSubject,
                DataGuidance = null
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
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup1
                }
            };

            var originalFilter2 = new Filter
            {
                Label = "Test filter 2 - not changing",
                Name = "test_filter_2_not_changing",
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup2
                }
            };

            var replacementFilter1 = new Filter
            {
                Label = "Test filter 1 - not changing",
                Name = "test_filter_1_not_changing",
                Subject = replacementSubject,
                FilterGroups = new List<FilterGroup>
                {
                    replacementFilterGroup1
                }
            };

            var replacementFilter2 = new Filter
            {
                Label = "Test filter 2 - not changing",
                Name = "test_filter_2_not_changing",
                Subject = replacementSubject,
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
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Label = "Default group - not changing",
                Subject = replacementSubject,
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
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[] {originalFilterItem1.Id, originalFilterItem2.Id},
                    Indicators = new[] {originalIndicator.Id},
                    LocationIds = ListOf(originalLocation.Id),
                    TimePeriod = timePeriod
                },
                Table = new TableBuilderConfiguration
                {
                    TableHeaders = new TableHeaders
                    {
                        ColumnGroups = new List<List<TableHeader>>
                        {
                            new()
                            {
                                TableHeader.NewLocationHeader(GeographicLevel.LocalAuthority, originalLocation.Id.ToString())
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
                                    DataSet = new ChartLegendItemDataSet
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
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentReleaseVersion2,
                ContentBlock = dataBlock
            };

            var footnoteForFilter = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter",
                filterFootnotes: new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = originalFilter1
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = originalFilterGroup1
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = originalFilterItem1
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = originalIndicator
                    }
                });

            var footnoteForSubject = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Subject",
                subject: originalSubject);

            var mocks = Mocks();

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>
                {
                    replacementLocation
                });

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalReleaseSubject1, originalReleaseSubject2,
                    replacementReleaseSubject);
                await statisticsDbContext.AddRangeAsync(originalFilter1, originalFilter2,
                    replacementFilter1, replacementFilter2);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddAsync(originalLocation);
                await statisticsDbContext.AddRangeAsync(footnoteForFilter, footnoteForFilterGroup,
                    footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            mocks.ReleaseService.Setup(service => service.RemoveDataFiles(
                contentReleaseVersion2.Id, originalFile.Id)).ReturnsAsync(Unit.Instance);

            var cacheKey = new DataBlockTableResultCacheKey(releaseContentBlock);

            mocks.cacheKeyService
                .Setup(service => service
                    .CreateCacheKeyForDataBlock(releaseContentBlock.ReleaseId, releaseContentBlock.ContentBlockId))
                .ReturnsAsync(cacheKey);

            mocks.cacheService
                .Setup(service => service.DeleteItem(cacheKey))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.Replace(
                    releaseId: contentReleaseVersion2.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                mocks.ReleaseService.Verify(
                    mock => mock.RemoveDataFiles(contentReleaseVersion2.Id, originalFile.Id),
                    Times.Once());

                VerifyAllMocks(mocks);

                result.AssertRight();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Check that the original file was unlinked from the replacement before the mock call to remove it.
                var originalFileUpdated = await contentDbContext.Files.FindAsync(originalFile.Id);
                Assert.NotNull(originalFileUpdated);
                Assert.Null(originalFileUpdated!.ReplacedById);

                // Check that the replacement file was unlinked from the original.
                var replacementFileUpdated = await contentDbContext.Files.FindAsync(replacementFile.Id);
                Assert.NotNull(replacementFileUpdated);
                Assert.Null(replacementFileUpdated!.ReplacingId);

                var replacedDataBlock = await contentDbContext.DataBlocks.FindAsync(dataBlock.Id);
                Assert.NotNull(replacedDataBlock);
                Assert.Equal(dataBlock.Name, replacedDataBlock!.Name);
                Assert.Equal(replacementSubject.Id, replacedDataBlock.Query.SubjectId);

                Assert.Single(replacedDataBlock.Query.Indicators);
                Assert.Equal(replacementIndicator.Id, replacedDataBlock.Query.Indicators.First());

                var replacedFilterItemIds = replacedDataBlock.Query.Filters.ToList();
                Assert.Equal(2, replacedFilterItemIds.Count);
                Assert.Equal(replacementFilterItem1.Id, replacedFilterItemIds[0]);
                Assert.Equal(replacementFilterItem2.Id, replacedFilterItemIds[1]);

                var replacedLocationId = Assert.Single(replacedDataBlock.Query.LocationIds);
                Assert.Equal(replacementLocation.Id, replacedLocationId);

                Assert.NotNull(replacedDataBlock.Query.TimePeriod);
                timePeriod.AssertDeepEqualTo(replacedDataBlock.Query.TimePeriod);

                Assert.Equal(2, replacedDataBlock.Table.TableHeaders.Columns.Count());
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
                var dataSet = Assert.Single(chartMajorAxis!.DataSets);
                Assert.NotNull(dataSet);
                Assert.Single(dataSet.Filters);
                Assert.Equal(replacementFilterItem1.Id, dataSet.Filters[0]);
                Assert.Equal(replacementIndicator.Id, dataSet.Indicator);
                Assert.Equal(replacementLocation.Id, dataSet.Location.Value);

                var chartLegendItems = replacedDataBlock.Charts[0].Legend?.Items;
                Assert.NotNull(chartLegendItems);
                var chartLegendItem = Assert.Single(chartLegendItems!);
                Assert.NotNull(chartLegendItem);
                var filter = Assert.Single(chartLegendItem!.DataSet.Filters);
                Assert.Equal(replacementFilterItem1.Id, filter);
                Assert.Equal(replacementIndicator.Id, chartLegendItem.DataSet.Indicator);
                Assert.NotNull(chartLegendItem.DataSet.Location);
                Assert.Equal(replacementLocation.Id, chartLegendItem.DataSet.Location!.Value);

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

                Assert.Equal(replacementSubject.Id, replacedFootnoteForSubject.Subjects.First().Subject.Id);

                var replacedReleaseSubject = await statisticsDbContext.ReleaseSubject
                    .SingleAsync(rs => rs.ReleaseId == statsReleaseVersion2.Id
                                       && rs.SubjectId == replacementSubject.Id);

                // Check the original guidance has been retained on the replacement
                Assert.Equal("Original guidance version 2", replacedReleaseSubject.DataGuidance);

                // Check the sequence of filters and indicators remains untouched
                Assert.Null(replacedReleaseSubject.FilterSequence);
                Assert.Null(replacedReleaseSubject.IndicatorSequence);
            }
        }

        [Fact]
        public async Task Replace_FilterSequenceIsReplaced()
        {
            // Basic test replacing a filter sequence, exercising the service with in-memory data and dependencies.  
            // See ReplaceServiceHelperTests.ReplaceFilterSequence for a more comprehensive test of the actual replacement.

            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                Publication = publication
            };

            var statsRelease = new Release
            {
                Id = contentRelease.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = replacementFile
            };

            var originalReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = originalSubject
            };

            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = replacementSubject
            };

            // Define a set of filters, filter groups and filter items belonging to the original subject
            var originalFilters = new List<Filter>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Filter a",
                    Name = "filter_a",
                    Subject = originalSubject,
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
            originalReleaseSubject.FilterSequence = new List<FilterSequenceEntry>
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
                    Subject = replacementSubject,
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

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(contentRelease);
                await contentDbContext.Files.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.ReleaseFiles.AddRangeAsync(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddAsync(statsRelease);
                await statisticsDbContext.Subject.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.Filter.AddRangeAsync(originalFilters);
                await statisticsDbContext.Filter.AddRangeAsync(replacementFilters);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(originalReleaseSubject,
                    replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>());

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            mocks.ReleaseService.Setup(service => service.RemoveDataFiles(
                contentRelease.Id, originalFile.Id)).ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.Replace(
                    releaseId: contentRelease.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacedReleaseSubject = await statisticsDbContext.ReleaseSubject
                    .SingleAsync(rs => rs.ReleaseId == statsRelease.Id
                                       && rs.SubjectId == replacementSubject.Id);

                // Verify the updated sequence of filters on the replacement subject
                var updatedSequence = replacedReleaseSubject.FilterSequence;
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

            var originalSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid()
            };

            var publication = new Publication
            {
                Id = Guid.NewGuid()
            };

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                Publication = publication
            };

            var statsRelease = new Release
            {
                Id = contentRelease.Id
            };

            var originalFile = new File
            {
                Filename = "original.csv",
                Type = FileType.Data,
                SubjectId = originalSubject.Id
            };

            var replacementFile = new File
            {
                Filename = "replacement.csv",
                Type = FileType.Data,
                SubjectId = replacementSubject.Id,
                Replacing = originalFile
            };

            originalFile.ReplacedBy = replacementFile;

            var originalReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = originalFile
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Release = contentRelease,
                File = replacementFile
            };

            var originalReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = originalSubject
            };

            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = replacementSubject
            };

            // Define a set of indicator groups and indicators belonging to the original subject
            var originalGroups = new List<IndicatorGroup>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Label = "Group a",
                    Subject = originalSubject,
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
            originalReleaseSubject.IndicatorSequence = new List<IndicatorGroupSequenceEntry>
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
                    Subject = replacementSubject,
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

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(contentRelease);
                await contentDbContext.Files.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.ReleaseFiles.AddRangeAsync(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddAsync(statsRelease);
                await statisticsDbContext.Subject.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.IndicatorGroup.AddRangeAsync(originalGroups);
                await statisticsDbContext.IndicatorGroup.AddRangeAsync(replacementGroups);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(originalReleaseSubject,
                    replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            mocks.locationRepository.Setup(service => service.GetDistinctForSubject(replacementSubject.Id))
                .ReturnsAsync(new List<Location>());

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .ReturnsAsync(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            mocks.ReleaseService.Setup(service => service.RemoveDataFiles(
                contentRelease.Id, originalFile.Id)).ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.Replace(
                    releaseId: contentRelease.Id,
                    originalFileId: originalFile.Id,
                    replacementFileId: replacementFile.Id);

                VerifyAllMocks(mocks);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacedReleaseSubject = await statisticsDbContext.ReleaseSubject
                    .SingleAsync(rs => rs.ReleaseId == statsRelease.Id
                                       && rs.SubjectId == replacementSubject.Id);

                // Verify the updated sequence of indicators on the replacement subject
                var updatedSequence = replacedReleaseSubject.IndicatorSequence;
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

        private static Footnote CreateFootnote(Release release,
            string content,
            List<FilterFootnote>? filterFootnotes = null,
            List<FilterGroupFootnote>? filterGroupFootnotes = null,
            List<FilterItemFootnote>? filterItemFootnotes = null,
            List<IndicatorFootnote>? indicatorFootnotes = null,
            Subject? subject = null)
        {
            return new()
            {
                Content = content,
                Filters = filterFootnotes,
                FilterGroups = filterGroupFootnotes,
                FilterItems = filterItemFootnotes,
                Indicators = indicatorFootnotes,
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
                        Release = release
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
            (Mock<ILocationRepository> locationRepository,
                Mock<IReleaseService> releaseService,
                Mock<ITimePeriodService> timePeriodService,
                Mock<ICacheKeyService> cacheKeyService,
                Mock<IBlobCacheService> cacheService) mocks)
        {
            var (locationRepository, releaseService, timePeriodService, cacheKeyService, cacheService) = mocks;

            return new ReplacementService(
                contentDbContext,
                statisticsDbContext,
                new FilterRepository(statisticsDbContext),
                new IndicatorRepository(statisticsDbContext),
                new IndicatorGroupRepository(statisticsDbContext),
                locationRepository.Object,
                new FootnoteRepository(statisticsDbContext),
                releaseService.Object,
                timePeriodService.Object,
                new PersistenceHelper<ContentDbContext>(contentDbContext),
                AlwaysTrueUserService().Object,
                cacheKeyService.Object,
                cacheService.Object
            );
        }

        private static (Mock<ILocationRepository> locationRepository,
            Mock<IReleaseService> ReleaseService,
            Mock<ITimePeriodService> TimePeriodService,
            Mock<ICacheKeyService> cacheKeyService,
            Mock<IBlobCacheService> cacheService) Mocks()
        {
            return (
                new Mock<ILocationRepository>(Strict),
                new Mock<IReleaseService>(Strict),
                new Mock<ITimePeriodService>(Strict),
                new Mock<ICacheKeyService>(Strict),
                new Mock<IBlobCacheService>(Strict));
        }
    }
}
