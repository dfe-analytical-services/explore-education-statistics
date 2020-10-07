using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FootnoteServiceTests
    {
        [Fact]
        public async void GetFootnote()
        {
            var release = new Release();

            var subject = new Subject
            {
                Name = "Test subject 1"
            };

            var filter = new Filter
            {
                Label = "Test filter 1"
            };

            var filterGroup = new FilterGroup
            {
                Filter = filter,
                Label = "Test filter group 1"
            };

            var filterItem = new FilterItem
            {
                Label = "Test filter item 1",
                FilterGroup = filterGroup
            };

            var indicator = new Indicator
            {
                Label = "Test indicator 1",
                IndicatorGroup = new IndicatorGroup()
            };

            var footnote = new Footnote
            {
                Content = "Test footnote",
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new SubjectFootnote
                    {
                        Subject = subject
                    }
                },
                Filters = new List<FilterFootnote>
                {
                    new FilterFootnote
                    {
                        Filter = filter
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroup = filterGroup
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
                    {
                        FilterItem = filterItem
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new IndicatorFootnote
                    {
                        Indicator = indicator
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = DbUtils.InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(footnote);

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = DbUtils.InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var result = await service.GetFootnote(release.Id, footnote.Id);

                Assert.True(result.IsRight);

                Assert.Equal(footnote.Id, result.Right.Id);
                Assert.Equal("Test footnote", result.Right.Content);

                Assert.Single(result.Right.Releases);
                Assert.Equal(release.Id, result.Right.Releases.First().ReleaseId);

                Assert.Single(result.Right.Subjects);
                Assert.Equal(subject.Id, result.Right.Subjects.First().SubjectId);

                Assert.Single(result.Right.Filters);
                Assert.Equal(filter.Id, result.Right.Filters.First().Filter.Id);
                Assert.Equal(filter.Label, result.Right.Filters.First().Filter.Label);

                Assert.Single(result.Right.FilterGroups);
                Assert.Equal(filterGroup.Id, result.Right.FilterGroups.First().FilterGroup.Id);
                Assert.Equal(filterGroup.Label, result.Right.FilterGroups.First().FilterGroup.Label);

                Assert.Single(result.Right.FilterItems);
                Assert.Equal(filterItem.Id, result.Right.FilterItems.First().FilterItem.Id);
                Assert.Equal(filterItem.Label, result.Right.FilterItems.First().FilterItem.Label);

                Assert.Single(result.Right.Indicators);
                Assert.Equal(indicator.Id, result.Right.Indicators.First().Indicator.Id);
                Assert.Equal(indicator.Label, result.Right.Indicators.First().Indicator.Label);
            }
        }

        [Fact]
        public async void GetFootnote_ReleaseNotFound()
        {
            var footnote = new Footnote
            {
                Content = "Test footnote",
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = new Release()
                    },
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = DbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(footnote);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = DbUtils.InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var invalidReleaseId = Guid.NewGuid();
                var result = await service.GetFootnote(invalidReleaseId, footnote.Id);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async void GetFootnote_FootnoteNotFound()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = DbUtils.InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(release);

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = DbUtils.InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var result = await service.GetFootnote(release.Id, Guid.NewGuid());

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async void GetFootnote_ReleaseAndFootnoteNotRelated()
        {
            var release = new Release();
            var footnote = new Footnote
            {
                Content = "Test footnote",
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = new Release()
                    },
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = DbUtils.InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(release);
                await statisticsDbContext.AddAsync(footnote);

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = DbUtils.InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var result = await service.GetFootnote(release.Id, footnote.Id);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        private FootnoteService SetupFootnoteService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext contentDbContext = null,
            ILogger<FootnoteService> logger = null,
            IFilterService filterService = null,
            IFilterGroupService filterGroupService = null,
            IFilterItemService filterItemService = null,
            IIndicatorService indicatorService = null,
            ISubjectService subjectService = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IUserService userService = null,
            IFootnoteService commonFootnoteService = null,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper = null)
        {
            var contentContext = contentDbContext ?? new Mock<ContentDbContext>().Object;

            return new FootnoteService(
                statisticsDbContext,
                logger ?? new Mock<ILogger<FootnoteService>>().Object,
                filterService ?? new Mock<IFilterService>().Object,
                filterGroupService ?? new Mock<IFilterGroupService>().Object,
                filterItemService ?? new Mock<IFilterItemService>().Object,
                indicatorService ?? new Mock<IIndicatorService>().Object,
                subjectService ?? new Mock<ISubjectService>().Object,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                commonFootnoteService ?? new Data.Model.Services.FootnoteService(
                    statisticsDbContext,
                    new Mock<ILogger<Data.Model.Services.FootnoteService>>().Object
                ),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext)
            );
        }
    }
}