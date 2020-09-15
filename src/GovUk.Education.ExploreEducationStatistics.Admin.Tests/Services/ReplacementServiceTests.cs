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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using FootnoteService = GovUk.Education.ExploreEducationStatistics.Data.Model.Services.FootnoteService;
using IReleaseService = GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.IReleaseService;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReplacementServiceTests
    {
        private const string CountryCodeEngland = "E92000001";

        [Fact]
        public async Task GetReplacementPlan_ReleaseFileReferenceHasWrongFileType()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Original subject"
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Replacement subject"
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

            // Use a ReleaseFileType that is not Data
            var originalReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "original.csv",
                ReleaseFileType = ReleaseFileTypes.Ancillary,
                Release = contentReleaseVersion1,
                SubjectId = originalSubject.Id
            };

            var replacementReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "replacement.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion2,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion1,
                ReleaseFileReference = originalReleaseFileReference
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = originalReleaseFileReference
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = replacementReleaseFileReference
            };

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(originalReleaseFileReference, replacementReleaseFileReference);
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

                var result = await replacementService.GetReplacementPlan(originalReleaseFileReference.Id,
                    replacementReleaseFileReference.Id);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, ReplacementFileTypesMustBeData);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_OriginalSubjectBelongsToDifferentRelease()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Original subject"
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Replacement subject"
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

            var originalReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "original.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentRelease1Version1,
                SubjectId = originalSubject.Id
            };

            var replacementReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "replacement.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentRelease2,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentRelease1Version1,
                ReleaseFileReference = originalReleaseFileReference
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentRelease1Version2,
                ReleaseFileReference = originalReleaseFileReference
            };

            // Link the replacement to the unrelated Release
            var replacementReleaseFile = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentRelease2,
                ReleaseFileReference = replacementReleaseFileReference
            };

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentRelease1Version1, contentRelease1Version2, contentRelease2);
                await contentDbContext.AddRangeAsync(originalReleaseFileReference, replacementReleaseFileReference);
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

                var result = await replacementService.GetReplacementPlan(originalReleaseFileReference.Id,
                    replacementReleaseFileReference.Id);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, ReplacementDataFileMustBeForRelatedRelease);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_OriginalSubjectNotUsed()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Original subject"
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Replacement subject"
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

            var originalReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "original.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion1,
                SubjectId = originalSubject.Id
            };

            var replacementReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "replacement.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion2,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion1,
                ReleaseFileReference = originalReleaseFileReference
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = originalReleaseFileReference
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = replacementReleaseFileReference
            };

            var mocks = Mocks();

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(originalReleaseFileReference, replacementReleaseFileReference);
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

                var result = await replacementService.GetReplacementPlan(originalReleaseFileReference.Id,
                    replacementReleaseFileReference.Id);

                Assert.True(result.IsRight);
                var replacementPlan = result.Right;

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
                Id = Guid.NewGuid(),
                Name = "Original subject"
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Replacement subject"
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

            var originalReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "original.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion1,
                SubjectId = originalSubject.Id
            };

            var replacementReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "replacement.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion2,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion1,
                ReleaseFileReference = originalReleaseFileReference
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = originalReleaseFileReference
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = replacementReleaseFileReference
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

            var table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    ColumnGroups = new List<List<TableHeader>>
                    {
                        new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.Country, CountryCodeEngland)
                        }
                    },
                    Columns = new List<TableHeader>
                    {
                        new TableHeader("2019_CY", TableHeaderType.TimePeriod),
                        new TableHeader("2020_CY", TableHeaderType.TimePeriod)
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new List<TableHeader>
                        {
                            new TableHeader(originalFilterItem.Id.ToString(), TableHeaderType.Filter)
                        }
                    },
                    Rows = new List<TableHeader>
                    {
                        new TableHeader(originalIndicator.Id.ToString(), TableHeaderType.Indicator)
                    }
                }
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
                        Country = new[] {CountryCodeEngland}
                    },
                    TimePeriod = timePeriod
                },
                Table = table
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
                    new FilterFootnote
                    {
                        Filter = originalFilter
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroup = originalFilterGroup
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
                    {
                        FilterItem = originalFilterItem
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion2,
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
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(originalReleaseFileReference, replacementReleaseFileReference);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddRangeAsync(releaseContentBlock);
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
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(originalReleaseFileReference.Id,
                    replacementReleaseFileReference.Id);

                Assert.True(result.IsRight);
                var replacementPlan = result.Right;

                Assert.Equal(originalSubject.Id, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementSubject.Id, replacementPlan.ReplacementSubjectId);

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

                Assert.NotNull(dataBlockPlan.Locations);
                Assert.Single(dataBlockPlan.Locations);
                Assert.True(dataBlockPlan.Locations.ContainsKey(GeographicLevel.Country.ToString()));
                Assert.Empty(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Matched);
                Assert.Single(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Unmatched);
                Assert.Equal(dataBlock.Query.Locations.Country,
                    dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Unmatched);
                Assert.False(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Valid);

                Assert.NotNull(dataBlockPlan.TimePeriods);
                Assert.Equal(timePeriod, dataBlockPlan.TimePeriods.Query);
                Assert.False(dataBlockPlan.TimePeriods.Valid);

                Assert.False(dataBlockPlan.Valid);

                Assert.Equal(5, replacementPlan.Footnotes.Count());

                var footnoteForFilterPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilter.Id);
                Assert.Equal(footnoteForFilter.Content, footnoteForFilterPlan.Content);
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
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterGroup.Id);
                Assert.Equal(footnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
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
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterItem.Id);
                Assert.Equal(footnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
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
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);
                Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);
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

                var footnoteForSubjectPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForSubject.Id);
                Assert.Equal(footnoteForSubject.Content, footnoteForSubjectPlan.Content);
                Assert.Empty(footnoteForSubjectPlan.Filters);
                Assert.Empty(footnoteForSubjectPlan.FilterGroups);
                Assert.Empty(footnoteForSubjectPlan.FilterItems);
                Assert.Empty(footnoteForSubjectPlan.Indicators);

                Assert.True(footnoteForSubjectPlan.Valid);

                Assert.False(replacementPlan.Valid);
            }
        }

        [Fact]
        public async Task GetReplacementPlan_AllReplacementDataPresent_ReplacementValid()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Original subject"
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Replacement subject"
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

            var originalReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "original.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion1,
                SubjectId = originalSubject.Id
            };

            var replacementReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "replacement.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion2,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion1,
                ReleaseFileReference = originalReleaseFileReference
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = originalReleaseFileReference
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = replacementReleaseFileReference
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
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem1
                }
            };

            var originalFilterGroup2 = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem2
                }
            };

            var replacementFilterGroup1 = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem1
                }
            };

            var replacementFilterGroup2 = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem2
                }
            };

            var originalFilter1 = new Filter
            {
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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

            var table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    ColumnGroups = new List<List<TableHeader>>
                    {
                        new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.Country, CountryCodeEngland)
                        }
                    },
                    Columns = new List<TableHeader>
                    {
                        new TableHeader("2019_CY", TableHeaderType.TimePeriod),
                        new TableHeader("2020_CY", TableHeaderType.TimePeriod)
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new List<TableHeader>
                        {
                            new TableHeader(originalFilterItem1.Id.ToString(), TableHeaderType.Filter)
                        }
                    },
                    Rows = new List<TableHeader>
                    {
                        new TableHeader(originalIndicator.Id.ToString(), TableHeaderType.Indicator)
                    }
                }
            };

            var dataBlock = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Test DataBlock",
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[] {originalFilterItem1.Id},
                    Indicators = new[] {originalIndicator.Id},
                    Locations = new LocationQuery
                    {
                        Country = new[] {CountryCodeEngland}
                    },
                    TimePeriod = timePeriod
                },
                Table = table
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
                    new FilterFootnote
                    {
                        Filter = originalFilter1
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroup = originalFilterGroup1
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
                    {
                        FilterItem = originalFilterItem1
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new IndicatorFootnote
                    {
                        Indicator = originalIndicator
                    }
                });

            var footnoteForSubject = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Subject",
                subject: originalSubject);

            var mocks = Mocks();

            mocks.LocationService.Setup(service => service.GetObservationalUnits(replacementSubject.Id))
                .Returns(new Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>>
                {
                    {
                        GeographicLevel.Country,
                        new List<Country>
                        {
                            new Country(CountryCodeEngland, "England")
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
                await contentDbContext.AddRangeAsync(originalReleaseFileReference, replacementReleaseFileReference);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddRangeAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalFilter1, originalFilter2,
                    replacementFilter1, replacementFilter2);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddRangeAsync(footnoteForFilter, footnoteForFilterGroup,
                    footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.GetReplacementPlan(originalReleaseFileReference.Id,
                    replacementReleaseFileReference.Id);

                Assert.True(result.IsRight);
                var replacementPlan = result.Right;

                Assert.Equal(originalSubject.Id, replacementPlan.OriginalSubjectId);
                Assert.Equal(replacementSubject.Id, replacementPlan.ReplacementSubjectId);

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
                Assert.Equal(replacementFilterItem1.Id, dataBlockFilterItemPlan.Target);
                Assert.True(dataBlockFilterItemPlan.Valid);

                Assert.NotNull(dataBlockPlan.Locations);
                Assert.Single(dataBlockPlan.Locations);
                Assert.True(dataBlockPlan.Locations.ContainsKey(GeographicLevel.Country.ToString()));
                Assert.Single(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Matched);
                Assert.Equal(dataBlock.Query.Locations.Country,
                    dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Matched);
                Assert.Empty(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Unmatched);
                Assert.True(dataBlockPlan.Locations[GeographicLevel.Country.ToString()].Valid);

                Assert.NotNull(dataBlockPlan.TimePeriods);
                Assert.Equal(timePeriod, dataBlockPlan.TimePeriods.Query);
                Assert.True(dataBlockPlan.TimePeriods.Valid);

                Assert.True(dataBlockPlan.Valid);

                Assert.Equal(5, replacementPlan.Footnotes.Count());

                var footnoteForFilterPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilter.Id);
                Assert.Equal(footnoteForFilter.Content, footnoteForFilterPlan.Content);
                Assert.Single(footnoteForFilterPlan.Filters);
                Assert.Empty(footnoteForFilterPlan.FilterGroups);
                Assert.Empty(footnoteForFilterPlan.FilterItems);
                Assert.Empty(footnoteForFilterPlan.Indicators);

                var footnoteForFilterFilterPlan = footnoteForFilterPlan.Filters.First();
                Assert.Equal(originalFilter1.Id, footnoteForFilterFilterPlan.Id);
                Assert.Equal(originalFilter1.Label, footnoteForFilterFilterPlan.Label);
                Assert.Equal(originalFilter1.Name, footnoteForFilterFilterPlan.Name);
                Assert.Equal(replacementFilter1.Id, footnoteForFilterFilterPlan.Target);
                Assert.True(footnoteForFilterFilterPlan.Valid);

                Assert.True(footnoteForFilterPlan.Valid);

                var footnoteForFilterGroupPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterGroup.Id);
                Assert.Equal(footnoteForFilterGroup.Content, footnoteForFilterGroupPlan.Content);
                Assert.Empty(footnoteForFilterGroupPlan.Filters);
                Assert.Single(footnoteForFilterGroupPlan.FilterGroups);
                Assert.Empty(footnoteForFilterGroupPlan.FilterItems);
                Assert.Empty(footnoteForFilterGroupPlan.Indicators);

                var footnoteForFilterGroupFilterGroupPlan = footnoteForFilterGroupPlan.FilterGroups.First();
                Assert.Equal(originalFilterGroup1.Id, footnoteForFilterGroupFilterGroupPlan.Id);
                Assert.Equal(originalFilterGroup1.Label, footnoteForFilterGroupFilterGroupPlan.Label);
                Assert.Equal(replacementFilterGroup1.Id, footnoteForFilterGroupFilterGroupPlan.Target);
                Assert.True(footnoteForFilterGroupFilterGroupPlan.Valid);

                Assert.True(footnoteForFilterGroupPlan.Valid);

                var footnoteForFilterItemPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForFilterItem.Id);
                Assert.Equal(footnoteForFilterItem.Content, footnoteForFilterItemPlan.Content);
                Assert.Empty(footnoteForFilterItemPlan.Filters);
                Assert.Empty(footnoteForFilterItemPlan.FilterGroups);
                Assert.Single(footnoteForFilterItemPlan.FilterItems);
                Assert.Empty(footnoteForFilterItemPlan.Indicators);

                var footnoteForFilterItemFilterItemPlan = footnoteForFilterItemPlan.FilterItems.First();
                Assert.Equal(originalFilterItem1.Id, footnoteForFilterItemFilterItemPlan.Id);
                Assert.Equal(originalFilterItem1.Label, footnoteForFilterItemFilterItemPlan.Label);
                Assert.Equal(replacementFilterItem1.Id, footnoteForFilterItemFilterItemPlan.Target);
                Assert.True(footnoteForFilterItemFilterItemPlan.Valid);

                Assert.True(footnoteForFilterItemPlan.Valid);

                var footnoteForIndicatorPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForIndicator.Id);
                Assert.Equal(footnoteForIndicator.Content, footnoteForIndicatorPlan.Content);
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

                var footnoteForSubjectPlan =
                    replacementPlan.Footnotes.Single(plan => plan.Id == footnoteForSubject.Id);
                Assert.Equal(footnoteForSubject.Content, footnoteForSubjectPlan.Content);
                Assert.Empty(footnoteForSubjectPlan.Filters);
                Assert.Empty(footnoteForSubjectPlan.FilterGroups);
                Assert.Empty(footnoteForSubjectPlan.FilterItems);
                Assert.Empty(footnoteForSubjectPlan.Indicators);

                Assert.True(footnoteForSubjectPlan.Valid);

                Assert.True(replacementPlan.Valid);
            }
        }

        [Fact]
        public async Task Replace_ReplacementPlanInvalid()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Original subject"
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Replacement subject"
            };

            var contentRelease = new Content.Model.Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null
            };

            var statsRelease = new Release
            {
                Id = contentRelease.Id,
                PreviousVersionId = contentRelease.PreviousVersionId
            };

            var originalReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "original.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentRelease,
                SubjectId = originalSubject.Id
            };

            var replacementReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "replacement.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentRelease,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentRelease,
                ReleaseFileReference = originalReleaseFileReference
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentRelease,
                ReleaseFileReference = replacementReleaseFileReference
            };

            var originalFilterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
                Label = "Original Test filter item"
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

            var originalIndicator = new Indicator
            {
                Id = Guid.NewGuid(),
                Label = "Original Indicator",
                Name = "original_indicator"
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
                        new TableHeader("2019_CY", TableHeaderType.TimePeriod),
                        new TableHeader("2020_CY", TableHeaderType.TimePeriod)
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.Country, CountryCodeEngland)
                        }
                    },
                    Rows = new List<TableHeader>()
                }
            };

            var dataBlock = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Test DataBlock",
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new Guid[] { },
                    Indicators = new Guid[] { },
                    Locations = new LocationQuery
                    {
                        Country = new[] {CountryCodeEngland}
                    },
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

            mocks.LocationService.Setup(service => service.GetObservationalUnits(replacementSubject.Id))
                .Returns(new Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>>());

            mocks.TimePeriodService.Setup(service => service.GetTimePeriods(replacementSubject.Id))
                .Returns(new List<(int Year, TimeIdentifier TimeIdentifier)>());

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(originalReleaseFileReference, replacementReleaseFileReference);
                await contentDbContext.AddRangeAsync(originalReleaseFile, replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddRangeAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalFilter);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.Replace(originalReleaseFileReference.Id,
                    replacementReleaseFileReference.Id);

                mocks.ReleaseService.VerifyNoOtherCalls();
                
                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, ReplacementMustBeValid);
            }
        }

        [Fact]
        public async Task Replace()
        {
            var originalSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Original subject"
            };

            var replacementSubject = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Replacement subject"
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

            var originalReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "original.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion1,
                SubjectId = originalSubject.Id
            };

            var replacementReleaseFileReference = new ReleaseFileReference
            {
                Id = Guid.NewGuid(),
                Filename = "replacement.csv",
                ReleaseFileType = ReleaseFileTypes.Data,
                Release = contentReleaseVersion2,
                SubjectId = replacementSubject.Id
            };

            var originalReleaseFile1 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion1,
                ReleaseFileReference = originalReleaseFileReference
            };

            var originalReleaseFile2 = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = originalReleaseFileReference
            };

            var replacementReleaseFile = new ReleaseFile
            {
                Id = Guid.NewGuid(),
                Release = contentReleaseVersion2,
                ReleaseFileReference = replacementReleaseFileReference
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
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem1
                }
            };

            var originalFilterGroup2 = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    originalFilterItem2
                }
            };

            var replacementFilterGroup1 = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem1
                }
            };

            var replacementFilterGroup2 = new FilterGroup
            {
                Id = Guid.NewGuid(),
                Label = "Default group - not changing",
                FilterItems = new List<FilterItem>
                {
                    replacementFilterItem2
                }
            };

            var originalFilter1 = new Filter
            {
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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
                Id = Guid.NewGuid(),
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

            var table = new TableBuilderConfiguration
            {
                TableHeaders = new TableHeaders
                {
                    ColumnGroups = new List<List<TableHeader>>
                    {
                        new List<TableHeader>
                        {
                            TableHeader.NewLocationHeader(GeographicLevel.Country, CountryCodeEngland)
                        }
                    },
                    Columns = new List<TableHeader>
                    {
                        new TableHeader("2019_CY", TableHeaderType.TimePeriod),
                        new TableHeader("2020_CY", TableHeaderType.TimePeriod)
                    },
                    RowGroups = new List<List<TableHeader>>
                    {
                        new List<TableHeader>
                        {
                            new TableHeader(originalFilterItem1.Id.ToString(), TableHeaderType.Filter)
                        }
                    },
                    Rows = new List<TableHeader>
                    {
                        new TableHeader(originalIndicator.Id.ToString(), TableHeaderType.Indicator)
                    }
                }
            };

            var dataBlock = new DataBlock
            {
                Id = Guid.NewGuid(),
                Name = "Test DataBlock",
                Query = new ObservationQueryContext
                {
                    SubjectId = originalSubject.Id,
                    Filters = new[] {originalFilterItem1.Id},
                    Indicators = new[] {originalIndicator.Id},
                    Locations = new LocationQuery
                    {
                        Country = new[] {CountryCodeEngland}
                    },
                    TimePeriod = timePeriod
                },
                Table = table
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
                    new FilterFootnote
                    {
                        Filter = originalFilter1
                    }
                });

            var footnoteForFilterGroup = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter group",
                filterGroupFootnotes: new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroup = originalFilterGroup1
                    }
                });

            var footnoteForFilterItem = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                filterItemFootnotes: new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
                    {
                        FilterItem = originalFilterItem1
                    }
                });

            var footnoteForIndicator = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Filter item",
                indicatorFootnotes: new List<IndicatorFootnote>
                {
                    new IndicatorFootnote
                    {
                        Indicator = originalIndicator
                    }
                });

            var footnoteForSubject = CreateFootnote(statsReleaseVersion2,
                "Test footnote for Subject",
                subject: originalSubject);

            var mocks = Mocks();

            mocks.LocationService.Setup(service => service.GetObservationalUnits(replacementSubject.Id))
                .Returns(new Dictionary<GeographicLevel, IEnumerable<IObservationalUnit>>
                {
                    {
                        GeographicLevel.Country,
                        new List<Country>
                        {
                            new Country(CountryCodeEngland, "England")
                        }
                    }
                });

            mocks.ReleaseService.Setup(service => service.RemoveDataFilesAsync(
                    contentReleaseVersion2.Id, originalReleaseFileReference.Filename, originalSubject.Name))
                .ReturnsAsync(Unit.Instance);

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
                await contentDbContext.AddRangeAsync(originalReleaseFileReference, replacementReleaseFileReference);
                await contentDbContext.AddRangeAsync(originalReleaseFile1, originalReleaseFile2,
                    replacementReleaseFile);
                await contentDbContext.AddAsync(dataBlock);
                await contentDbContext.AddRangeAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(originalSubject, replacementSubject);
                await statisticsDbContext.AddRangeAsync(originalFilter1, originalFilter2,
                    replacementFilter1, replacementFilter2);
                await statisticsDbContext.AddRangeAsync(originalIndicatorGroup, replacementIndicatorGroup);
                await statisticsDbContext.AddRangeAsync(footnoteForFilter, footnoteForFilterGroup,
                    footnoteForFilterItem, footnoteForIndicator, footnoteForSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReplacementService(contentDbContext, statisticsDbContext, mocks);

                var result = await replacementService.Replace(originalReleaseFileReference.Id,
                    replacementReleaseFileReference.Id);

                mocks.ReleaseService.Verify(
                    mock => mock.RemoveDataFilesAsync(contentReleaseVersion2.Id, originalReleaseFileReference.Filename,
                        originalSubject.Name), Times.Once());

                mocks.ReleaseService.VerifyNoOtherCalls();

                Assert.True(result.IsRight);

                var replacedDataBlock = await contentDbContext.DataBlocks.FindAsync(dataBlock.Id);
                Assert.NotNull(replacedDataBlock);
                Assert.Equal(dataBlock.Name, replacedDataBlock.Name);
                Assert.Equal(replacementSubject.Id, replacedDataBlock.Query.SubjectId);

                Assert.Single(replacedDataBlock.Query.Indicators);
                Assert.Equal(replacementIndicator.Id, replacedDataBlock.Query.Indicators.First());

                Assert.Single(replacedDataBlock.Query.Filters);
                Assert.Equal(replacementFilterItem1.Id, replacedDataBlock.Query.Filters.First());

                Assert.NotNull(replacedDataBlock.Query.Locations);
                Assert.Equal(dataBlock.Query.Locations, replacedDataBlock.Query.Locations);

                Assert.NotNull(replacedDataBlock.Query.TimePeriod);
                Assert.Equal(timePeriod, replacedDataBlock.Query.TimePeriod);

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
                Assert.Equal(CountryCodeEngland,
                    replacedDataBlock.Table.TableHeaders.ColumnGroups.First().First().Value);
                Assert.Single(replacedDataBlock.Table.TableHeaders.Rows);
                Assert.Equal(TableHeaderType.Indicator, replacedDataBlock.Table.TableHeaders.Rows.First().Type);
                Assert.Equal(replacementIndicator.Id.ToString(),
                    replacedDataBlock.Table.TableHeaders.Rows.First().Value);
                Assert.Single(replacedDataBlock.Table.TableHeaders.RowGroups);
                Assert.Single(replacedDataBlock.Table.TableHeaders.RowGroups.First());
                Assert.Equal(TableHeaderType.Filter,
                    replacedDataBlock.Table.TableHeaders.RowGroups.First().First().Type);
                Assert.Equal(replacementFilterItem1.Id.ToString(),
                    replacedDataBlock.Table.TableHeaders.RowGroups.First().First().Value);

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
            }
        }

        private static Footnote CreateFootnote(Release release,
            string content,
            ICollection<FilterFootnote> filterFootnotes = null,
            ICollection<FilterGroupFootnote> filterGroupFootnotes = null,
            ICollection<FilterItemFootnote> filterItemFootnotes = null,
            ICollection<IndicatorFootnote> indicatorFootnotes = null,
            Subject subject = null)
        {
            return new Footnote
            {
                Id = Guid.NewGuid(),
                Content = content,
                Filters = filterFootnotes,
                FilterGroups = filterGroupFootnotes,
                FilterItems = filterItemFootnotes,
                Indicators = indicatorFootnotes,
                Subjects = subject != null
                    ? new List<SubjectFootnote>
                    {
                        new SubjectFootnote
                        {
                            Subject = subject
                        }
                    }
                    : new List<SubjectFootnote>(),
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
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
            (Mock<ILocationService> locationService,
                Mock<IReleaseService> releaseService,
                Mock<ITimePeriodService> timePeriodService) mocks)
        {
            var (locationService, releaseService, timePeriodService) = mocks;

            return new ReplacementService(
                contentDbContext,
                statisticsDbContext,
                new FilterService(statisticsDbContext, new Mock<ILogger<FilterService>>().Object),
                new IndicatorService(statisticsDbContext, new Mock<ILogger<IndicatorService>>().Object),
                locationService.Object,
                new FootnoteService(statisticsDbContext, new Mock<ILogger<FootnoteService>>().Object),
                releaseService.Object,
                timePeriodService.Object,
                new PersistenceHelper<ContentDbContext>(contentDbContext));
        }

        private static (Mock<ILocationService> LocationService,
            Mock<IReleaseService> ReleaseService,
            Mock<ITimePeriodService> TimePeriodService) Mocks()
        {
            return (
                new Mock<ILocationService>(),
                new Mock<IReleaseService>(),
                new Mock<ITimePeriodService>());
        }
    }
}