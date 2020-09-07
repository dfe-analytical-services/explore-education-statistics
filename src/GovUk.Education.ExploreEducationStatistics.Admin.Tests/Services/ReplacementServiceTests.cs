using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Services.LocationService;
using FootnoteService = GovUk.Education.ExploreEducationStatistics.Data.Model.Services.FootnoteService;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReplacementServiceTests
    {
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
            
            var contentRelease2 = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var statsRelease1Version1 = new Release
            {
                Id = contentRelease1Version1.Id,
                PreviousVersionId = contentRelease1Version1.PreviousVersionId
            };

            var statsRelease1Version2 = new Release
            {
                Id = contentRelease1Version2.Id,
                PreviousVersionId = contentRelease1Version2.PreviousVersionId
            };

            var statsRelease2 = new Release
            {
                Id = contentRelease2.Id,
                PreviousVersionId = contentRelease2.PreviousVersionId
            };

            var originalReleaseSubject1 = new ReleaseSubject
            {
                Release = statsRelease1Version1,
                Subject = originalSubject
            };

            var originalReleaseSubject2 = new ReleaseSubject
            {
                Release = statsRelease1Version2,
                Subject = originalSubject
            };

            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsRelease2,
                Subject = replacementSubject
            };

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentRelease1Version1, contentRelease1Version2, contentRelease2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease1Version1, statsRelease1Version2, statsRelease2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalReleaseSubject1, originalReleaseSubject2, replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(originalSubject.Id, replacementSubject.Id);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, ReplacementDataFileMustBeForSameRelease);
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
                Id = contentReleaseVersion1.Id,
                PreviousVersionId = contentReleaseVersion1.PreviousVersionId
            };
            
            var statsReleaseVersion2 = new Release
            {
                Id = contentReleaseVersion2.Id,
                PreviousVersionId = contentReleaseVersion2.PreviousVersionId
            };

            var originalReleaseSubject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion1,
                Subject = originalSubject
            };

            var originalReleaseSubject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = originalSubject
            };
            
            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = replacementSubject
            };

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalReleaseSubject1, originalReleaseSubject2, replacementReleaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(originalSubject.Id, replacementSubject.Id);

                Assert.True(result.IsRight);
                var replacementPlan = result.Right;
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
                Id = contentReleaseVersion1.Id,
                PreviousVersionId = contentReleaseVersion1.PreviousVersionId
            };
            
            var statsReleaseVersion2 = new Release
            {
                Id = contentReleaseVersion2.Id,
                PreviousVersionId = contentReleaseVersion2.PreviousVersionId
            };

            var originalReleaseSubject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion1,
                Subject = originalSubject
            };

            var originalReleaseSubject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = originalSubject
            };

            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = replacementSubject
            };
            
            var originalFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Original Test filter item"
            };

            var replacementFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Replacement Test filter item"
            };

            var originalFilterGroup = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Original Default group",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem
                }
            };

            var replacementFilterGroup = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Replacement Default group",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem
                }
            };

            var originalFilter = new Filter
            {
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
                Label = "Replacement Indicator",
                Name = "replacement_indicator"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Id = Guid.NewGuid(),
                Label = "Original Default group",
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Id = Guid.NewGuid(),
                Label = "Replacement Default group",
                Subject = replacementSubject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
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
                Id = Guid.NewGuid(),
                Name = "Test DataBlock",
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[] {originalFilterItem.Id},
                    Indicators = new[] {originalIndicator.Id},
                    Locations = new LocationQuery
                    {
                        Country = new[] {"E92000001"}
                    },
                    TimePeriod = timePeriod
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentReleaseVersion2,
                ContentBlock = dataBlock
            };

            var originalFootnoteForFilter = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter",
                filterFootnotes: new List<FilterFootnote>
                {
                    new FilterFootnote
                    {
                        Filter = originalFilter
                    }
                });

            var originalFootnoteForFilterGroup = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroup = originalFilterGroup
                    }
                });

            var originalFootnoteForFilterItem = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
                    {
                        FilterItem = originalFilterItem
                    }
                });

            var originalFootnoteForIndicator = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new IndicatorFootnote
                    {
                        Indicator = originalIndicator
                    }
                });

            var mocks = Mocks();

            mocks.LocationService.Setup(service => service.GetObservationalUnits(replacementSubject.Id))
                .Returns(new Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>>());

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentReleaseVersion2);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddRangeAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalReleaseSubject1, originalReleaseSubject2, replacementReleaseSubject);
                await statisticsDbContext.AddRangeAsync(originalFilter, replacementFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddRangeAsync(originalFootnoteForFilter, originalFootnoteForFilterGroup,
                    originalFootnoteForFilterItem, originalFootnoteForIndicator);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(originalSubject.Id, replacementSubject.Id);

                Assert.True(result.IsRight);
                var replacementPlan = result.Right;

                Assert.Single(replacementPlan.DataBlocks);
                var dataBlockPlan = replacementPlan.DataBlocks.First();
                Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
                Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

                Assert.Single(dataBlockPlan.Indicators);
                var dataBlockIndicatorPlan = dataBlockPlan.Indicators.First();
                Assert.Equal(originalIndicator.Id, dataBlockIndicatorPlan.Id);
                Assert.Equal(originalIndicator.Label, dataBlockIndicatorPlan.Label);
                Assert.Equal(originalIndicator.Name, dataBlockIndicatorPlan.Name);
                Assert.Null(dataBlockIndicatorPlan.Target);
                Assert.False(dataBlockIndicatorPlan.Valid);

                Assert.Single(dataBlockPlan.FilterItems);
                var dataBlockFilterItemPlan = dataBlockPlan.FilterItems.First();
                Assert.Equal(dataBlockFilterItemPlan.Id, dataBlockFilterItemPlan.Id);
                Assert.Equal(dataBlockFilterItemPlan.Label, dataBlockFilterItemPlan.Label);
                Assert.Null(dataBlockFilterItemPlan.Target);
                Assert.False(dataBlockFilterItemPlan.Valid);

                Assert.NotNull(dataBlockPlan.ObservationalUnits);
                var allGeographicLevels = GetGeographicLevelsAsStrings();
                Assert.Equal(allGeographicLevels.Count, dataBlockPlan.ObservationalUnits.Count);
                Assert.False(allGeographicLevels.Except(dataBlockPlan.ObservationalUnits.Keys).Any());

                Assert.Empty(dataBlockPlan.ObservationalUnits[GeographicLevel.Country.ToString()].Matched);
                Assert.Single(dataBlockPlan.ObservationalUnits[GeographicLevel.Country.ToString()].Unmatched);
                Assert.Equal(dataBlock.Query.Locations.Country,
                    dataBlockPlan.ObservationalUnits[GeographicLevel.Country.ToString()].Unmatched);
                Assert.False(dataBlockPlan.ObservationalUnits[GeographicLevel.Country.ToString()].Valid);

                allGeographicLevels.ForEach(level =>
                {
                    if (level != GeographicLevel.Country.ToString())
                    {
                        Assert.Empty(dataBlockPlan.ObservationalUnits[level].Matched);
                        Assert.Empty(dataBlockPlan.ObservationalUnits[level].Unmatched);
                        Assert.True(dataBlockPlan.ObservationalUnits[level].Valid);
                    }
                });

                Assert.NotNull(dataBlockPlan.TimePeriods);
                Assert.Equal(timePeriod, dataBlockPlan.TimePeriods.Query);
                Assert.False(dataBlockPlan.TimePeriods.Valid);

                Assert.False(dataBlockPlan.Valid);
                
                Assert.Equal(4, replacementPlan.Footnotes.Count());

                var footnoteForFilterPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == originalFootnoteForFilter.Id);
                Assert.Equal(originalFootnoteForFilter.Content, footnoteForFilterPlan.Content);
                Assert.Single(footnoteForFilterPlan.Filters);
                Assert.Empty(footnoteForFilterPlan.FilterGroups);
                Assert.Empty(footnoteForFilterPlan.FilterItems);
                Assert.Empty(footnoteForFilterPlan.Indicators);

                var footnoteForFilterFilterPlan = footnoteForFilterPlan.Filters.First();
                Assert.Equal(originalFilter.Id, footnoteForFilterFilterPlan.Id);
                Assert.Equal(originalFilter.Label, footnoteForFilterFilterPlan.Label);
                Assert.Equal(originalFilter.Name, footnoteForFilterFilterPlan.Name);
                Assert.Null(footnoteForFilterFilterPlan.Target);
                Assert.False(footnoteForFilterFilterPlan.Valid);

                Assert.False(footnoteForFilterPlan.Valid);
                
                var footnoteForFilterGroupPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == originalFootnoteForFilterGroup.Id);
                Assert.Equal(originalFootnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
                Assert.Empty(footnoteForFilterGroupPlan.Filters);
                Assert.Single(footnoteForFilterGroupPlan.FilterGroups);
                Assert.Empty(footnoteForFilterGroupPlan.FilterItems);
                Assert.Empty(footnoteForFilterGroupPlan.Indicators);

                var footnoteForFilterGroupFilterGroupPlan = footnoteForFilterGroupPlan.FilterGroups.First();
                Assert.Equal(originalFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Id);
                Assert.Equal(originalFilterGroup.Label, footnoteForFilterGroupFilterGroupPlan.Label);
                Assert.Null(footnoteForFilterGroupFilterGroupPlan.Target);
                Assert.False(footnoteForFilterGroupFilterGroupPlan.Valid);
                
                Assert.False(footnoteForFilterGroupPlan.Valid);

                var footnoteForFilterItemPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == originalFootnoteForFilterItem.Id);
                Assert.Equal(originalFootnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
                Assert.Empty(footnoteForFilterItemPlan.Filters);
                Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
                Assert.Single(footnoteForFilterItemPlan.FilterItems);
                Assert.Empty(footnoteForFilterItemPlan.Indicators);

                var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();
                Assert.Equal(originalFilterItem.Id, footnoteForFilterItemFilterItemPlan.Id);
                Assert.Equal(originalFilterItem.Label, footnoteForFilterItemFilterItemPlan.Label);
                Assert.Null(footnoteForFilterItemFilterItemPlan.Target);
                Assert.False(footnoteForFilterItemFilterItemPlan.Valid);
                
                Assert.False(footnoteForFilterItemPlan.Valid);

                var footnoteForIndicatorPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == originalFootnoteForIndicator.Id);
                Assert.Equal(originalFootnoteForIndicator.Content, footnoteForIndicatorPlan.Content);
                Assert.Empty(footnoteForIndicatorPlan.Filters);
                Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
                Assert.Empty(footnoteForIndicatorPlan.FilterItems);
                Assert.Single(footnoteForIndicatorPlan.Indicators);

                var footnoteForIndicatorIndicatorPlan = footnoteForIndicatorPlan.Indicators.First();
                Assert.Equal(originalIndicator.Id, footnoteForIndicatorIndicatorPlan.Id);
                Assert.Equal(originalIndicator.Label, footnoteForIndicatorIndicatorPlan.Label);
                Assert.Equal(originalIndicator.Name, footnoteForIndicatorIndicatorPlan.Name);
                Assert.Null(footnoteForIndicatorIndicatorPlan.Target);
                Assert.False(footnoteForIndicatorIndicatorPlan.Valid);
                
                Assert.False(footnoteForIndicatorPlan.Valid);
                
                Assert.False(replacementPlan.Valid);
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
                Id = contentReleaseVersion1.Id,
                PreviousVersionId = contentReleaseVersion1.PreviousVersionId
            };
            
            var statsReleaseVersion2 = new Release
            {
                Id = contentReleaseVersion2.Id,
                PreviousVersionId = contentReleaseVersion2.PreviousVersionId
            };

            var originalReleaseSubject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion1,
                Subject = originalSubject
            };

            var originalReleaseSubject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = originalSubject
            };

            var replacementReleaseSubject = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = replacementSubject
            };

            var originalFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var replacementFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Test filter item - not changing"
            };

            var originalFilterGroup = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem
                }
            };

            var replacementFilterGroup = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem
                }
            };

            var originalFilter = new Filter
            {
                Id = Guid.NewGuid(),
                Label = "Test filter - not changing",
                Name = "test_filter_not_changing",
                Subject = originalSubject,
                FilterGroups = new List<FilterGroup>
                {
                    originalFilterGroup
                }
            };

            var replacementFilter = new Filter
            {
                Id = Guid.NewGuid(),
                Label = "Test filter - not changing",
                Name = "test_filter_not_changing",
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
                Id = Guid.NewGuid(),
                Label = "Indicator - not changing",
                Name = "indicator_not_changing"
            };

            var originalIndicatorGroup = new IndicatorGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                Subject = originalSubject,
                Indicators = new List<Indicator>
                {
                    originalIndicator
                }
            };

            var replacementIndicatorGroup = new IndicatorGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                Subject = replacementSubject,
                Indicators = new List<Indicator>
                {
                    replacementIndicator
                }
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
                Id = Guid.NewGuid(),
                Name = "Test DataBlock",
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[] {originalFilterItem.Id},
                    Indicators = new[] {originalIndicator.Id},
                    Locations = new LocationQuery
                    {
                        Country = new[] {"E92000001"}
                    },
                    TimePeriod = timePeriod
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentReleaseVersion2,
                ContentBlock = dataBlock
            };

            var originalFootnoteForFilter = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter",
                filterFootnotes: new List<FilterFootnote>
                {
                    new FilterFootnote
                    {
                        Filter = originalFilter
                    }
                });

            var originalFootnoteForFilterGroup = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroup = originalFilterGroup
                    }
                });

            var originalFootnoteForFilterItem = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
                    {
                        FilterItem = originalFilterItem
                    }
                });

            var originalFootnoteForIndicator = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new IndicatorFootnote
                    {
                        Indicator = originalIndicator
                    }
                });

            var mocks = Mocks();

            mocks.LocationService.Setup(service => service.GetObservationalUnits(replacementSubject.Id))
                .Returns(new Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>>
                {
                    {
                        GeographicLevel.Country,
                        new List<Country>
                        {
                            new Country("E92000001", "England")
                        }
                    }
                });

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>
                {
                    (2019, CalendarYear),
                    (2020, CalendarYear)
                });

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddRangeAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalReleaseSubject1, originalReleaseSubject2, replacementReleaseSubject);
                await statisticsDbContext.AddRangeAsync(originalFilter, replacementFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddRangeAsync(originalFootnoteForFilter, originalFootnoteForFilterGroup,
                    originalFootnoteForFilterItem, originalFootnoteForIndicator);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(originalSubject.Id, replacementSubject.Id);

                Assert.True(result.IsRight);
                var replacementPlan = result.Right;

                Assert.Single(replacementPlan.DataBlocks);
                var dataBlockPlan = replacementPlan.DataBlocks.First();
                Assert.Equal(dataBlock.Id, dataBlockPlan.Id);
                Assert.Equal(dataBlock.Name, dataBlockPlan.Name);

                Assert.Single(dataBlockPlan.Indicators);
                var dataBlockIndicatorPlan = dataBlockPlan.Indicators.First();
                Assert.Equal(originalIndicator.Id, dataBlockIndicatorPlan.Id);
                Assert.Equal(originalIndicator.Label, dataBlockIndicatorPlan.Label);
                Assert.Equal(originalIndicator.Name, dataBlockIndicatorPlan.Name);
                Assert.Equal(replacementIndicator.Id, dataBlockIndicatorPlan.Target);
                Assert.True(dataBlockIndicatorPlan.Valid);

                Assert.Single(dataBlockPlan.FilterItems);
                var dataBlockFilterItemPlan = dataBlockPlan.FilterItems.First();
                Assert.Equal(dataBlockFilterItemPlan.Id, dataBlockFilterItemPlan.Id);
                Assert.Equal(dataBlockFilterItemPlan.Label, dataBlockFilterItemPlan.Label);
                Assert.Equal(replacementFilterItem.Id, dataBlockFilterItemPlan.Target);
                Assert.True(dataBlockFilterItemPlan.Valid);

                Assert.NotNull(dataBlockPlan.ObservationalUnits);
                var allGeographicLevels = GetGeographicLevelsAsStrings();
                Assert.Equal(allGeographicLevels.Count, dataBlockPlan.ObservationalUnits.Count);
                Assert.False(allGeographicLevels.Except(dataBlockPlan.ObservationalUnits.Keys).Any());

                Assert.Single(dataBlockPlan.ObservationalUnits[GeographicLevel.Country.ToString()].Matched);
                Assert.Equal(dataBlock.Query.Locations.Country,
                    dataBlockPlan.ObservationalUnits[GeographicLevel.Country.ToString()].Matched);
                Assert.Empty(dataBlockPlan.ObservationalUnits[GeographicLevel.Country.ToString()].Unmatched);
                Assert.True(dataBlockPlan.ObservationalUnits[GeographicLevel.Country.ToString()].Valid);

                allGeographicLevels.ForEach(level =>
                {
                    if (level != GeographicLevel.Country.ToString())
                    {
                        Assert.Empty(dataBlockPlan.ObservationalUnits[level].Matched);
                        Assert.Empty(dataBlockPlan.ObservationalUnits[level].Unmatched);
                        Assert.True(dataBlockPlan.ObservationalUnits[level].Valid);
                    }
                });

                Assert.NotNull(dataBlockPlan.TimePeriods);
                Assert.Equal(timePeriod, dataBlockPlan.TimePeriods.Query);
                Assert.True(dataBlockPlan.TimePeriods.Valid);

                Assert.True(dataBlockPlan.Valid);
                
                Assert.Equal(4, replacementPlan.Footnotes.Count());

                var footnoteForFilterPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == originalFootnoteForFilter.Id);
                Assert.Equal(originalFootnoteForFilter.Content, footnoteForFilterPlan.Content);
                Assert.Single(footnoteForFilterPlan.Filters);
                Assert.Empty(footnoteForFilterPlan.FilterGroups);
                Assert.Empty(footnoteForFilterPlan.FilterItems);
                Assert.Empty(footnoteForFilterPlan.Indicators);

                var footnoteForFilterFilterPlan = footnoteForFilterPlan.Filters.First();
                Assert.Equal(originalFilter.Id, footnoteForFilterFilterPlan.Id);
                Assert.Equal(originalFilter.Label, footnoteForFilterFilterPlan.Label);
                Assert.Equal(originalFilter.Name, footnoteForFilterFilterPlan.Name);
                Assert.Equal(replacementFilter.Id, footnoteForFilterFilterPlan.Target);
                Assert.True(footnoteForFilterFilterPlan.Valid);
                
                Assert.True(footnoteForFilterPlan.Valid);

                var footnoteForFilterGroupPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == originalFootnoteForFilterGroup.Id);
                Assert.Equal(originalFootnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
                Assert.Empty(footnoteForFilterGroupPlan.Filters);
                Assert.Single(footnoteForFilterGroupPlan.FilterGroups);
                Assert.Empty(footnoteForFilterGroupPlan.FilterItems);
                Assert.Empty(footnoteForFilterGroupPlan.Indicators);

                var footnoteForFilterGroupFilterGroupPlan = footnoteForFilterGroupPlan.FilterGroups.First();
                Assert.Equal(originalFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Id);
                Assert.Equal(originalFilterGroup.Label, footnoteForFilterGroupFilterGroupPlan.Label);
                Assert.Equal(replacementFilterGroup.Id, footnoteForFilterGroupFilterGroupPlan.Target);
                Assert.True(footnoteForFilterGroupFilterGroupPlan.Valid);

                Assert.True(footnoteForFilterGroupPlan.Valid);
                
                var footnoteForFilterItemPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == originalFootnoteForFilterItem.Id);
                Assert.Equal(originalFootnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
                Assert.Empty(footnoteForFilterItemPlan.Filters);
                Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
                Assert.Single(footnoteForFilterItemPlan.FilterItems);
                Assert.Empty(footnoteForFilterItemPlan.Indicators);

                var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();
                Assert.Equal(originalFilterItem.Id, footnoteForFilterItemFilterItemPlan.Id);
                Assert.Equal(originalFilterItem.Label, footnoteForFilterItemFilterItemPlan.Label);
                Assert.Equal(replacementFilterItem.Id, footnoteForFilterItemFilterItemPlan.Target);
                Assert.True(footnoteForFilterItemFilterItemPlan.Valid);
                
                Assert.True(footnoteForFilterItemPlan.Valid);

                var footnoteForIndicatorPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == originalFootnoteForIndicator.Id);
                Assert.Equal(originalFootnoteForIndicator.Content, footnoteForIndicatorPlan.Content);
                Assert.Empty(footnoteForIndicatorPlan.Filters);
                Assert.Empty(footnoteForIndicatorPlan.FilterGroups);
                Assert.Empty(footnoteForIndicatorPlan.FilterItems);
                Assert.Single(footnoteForIndicatorPlan.Indicators);

                var footnoteForIndicatorIndicatorPlan = footnoteForIndicatorPlan.Indicators.First();
                Assert.Equal(originalIndicator.Id, footnoteForIndicatorIndicatorPlan.Id);
                Assert.Equal(originalIndicator.Label, footnoteForIndicatorIndicatorPlan.Label);
                Assert.Equal(originalIndicator.Name, footnoteForIndicatorIndicatorPlan.Name);
                Assert.Equal(replacementIndicator.Id, footnoteForIndicatorIndicatorPlan.Target);
                Assert.True(footnoteForIndicatorIndicatorPlan.Valid);
                
                Assert.True(footnoteForIndicatorPlan.Valid);
                
                Assert.True(replacementPlan.Valid);
            }
        }

        private static Footnote CreateFootnote(Release release,
            string content,
            ICollection<FilterFootnote> filterFootnotes = null,
            ICollection<FilterGroupFootnote> filterGroupFootnotes = null,
            ICollection<FilterItemFootnote> filterItemFootnotes = null,
            ICollection<IndicatorFootnote> indicatorFootnotes = null)
        {
            return new Footnote
            {
                Id = Guid.NewGuid(),
                Content = content,
                Filters = filterFootnotes,
                FilterGroups = filterGroupFootnotes,
                FilterItems = filterItemFootnotes,
                Indicators = indicatorFootnotes,
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = release
                    }
                }
            };
        }

        private static List<string> GetGeographicLevelsAsStrings()
        {
            return GetEnumValues<GeographicLevel>()
                .Where(geographicLevel => !IgnoredLevels.Contains(geographicLevel))
                .Select(level => level.ToString())
                .ToList();
        }

        private static ReplacementService BuildReplacementService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext,
            (Mock<ILocationService> locationService,
                Mock<ITimePeriodService> timePeriodService) mocks)
        {
            var (locationService, timePeriodService) = mocks;

            return new ReplacementService(
                contentDbContext,
                statisticsDbContext,
                new FilterService(statisticsDbContext, new Mock<ILogger<FilterService>>().Object),
                new IndicatorService(statisticsDbContext, new Mock<ILogger<IndicatorService>>().Object),
                locationService.Object,
                new FootnoteService(statisticsDbContext, new Mock<ILogger<FootnoteService>>().Object),
                timePeriodService.Object,
                new PersistenceHelper<StatisticsDbContext>(statisticsDbContext));
        }

        private static (Mock<ILocationService> LocationService,
            Mock<ITimePeriodService> TimePeriodService) Mocks()
        {
            return (
                new Mock<ILocationService>(),
                new Mock<ITimePeriodService>());
        }
    }
}