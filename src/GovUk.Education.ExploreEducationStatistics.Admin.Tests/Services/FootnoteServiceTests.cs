using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FootnoteServiceTests
    {
        [Fact]
        public async Task GetFootnote()
        {
            var release = new Release();

            var subject = new Subject();

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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
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
        public async Task GetFootnote_ReleaseNotFound()
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(footnote);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var invalidReleaseId = Guid.NewGuid();
                var result = await service.GetFootnote(invalidReleaseId, footnote.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetFootnote_FootnoteNotFound()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(release);

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var result = await service.GetFootnote(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task GetFootnote_ReleaseAndFootnoteNotRelated()
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
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

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(statisticsDbContext, contentDbContext: contentDbContext);

                var result = await service.GetFootnote(release.Id, footnote.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task CopyFootnotes()
        {
            var release = new Release();
            var amendment = new Release();

            var subject = new Subject();

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

            var footnote1 = new Footnote
            {
                Content = "Test footnote 1",
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
                }
            };

            var footnote2 = new Footnote
            {
                Content = "Test footnote 2",
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
                Filters = new List<FilterFootnote>(),
                FilterGroups = new List<FilterGroupFootnote>(),
                FilterItems = new List<FilterItemFootnote>(),
                Indicators = new List<IndicatorFootnote>()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(release, amendment);
                await statisticsDbContext.AddRangeAsync(footnote1, footnote2);

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddRangeAsync(new Content.Model.Release
                    {
                        Id = release.Id
                    },
                    new Content.Model.Release
                    {
                        Id = amendment.Id
                    });

                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var footnoteRepository = new Mock<IFootnoteRepository>(Strict);

                footnoteRepository
                    .Setup(s => s.GetFootnotes(release.Id))
                    .Returns(new List<Footnote>
                    {
                        footnote1,
                        footnote2
                    });

                var newFootnote1 = new Footnote
                {
                    Id = Guid.NewGuid(),
                    Releases = new List<ReleaseFootnote>
                    {
                        new ReleaseFootnote
                        {
                            ReleaseId = amendment.Id
                        }
                    }
                };

                var newFootnote2 = new Footnote
                {
                    Id = Guid.NewGuid(),
                    Releases = new List<ReleaseFootnote>
                    {
                        new ReleaseFootnote
                        {
                            ReleaseId = amendment.Id
                        }
                    }
                };

                footnoteRepository
                    .Setup(s => s.GetFootnote(newFootnote1.Id))
                    .ReturnsAsync(newFootnote1);

                footnoteRepository
                    .Setup(s => s.GetFootnote(newFootnote2.Id))
                    .ReturnsAsync(newFootnote2);

                var guidGenerator = new Mock<IGuidGenerator>();

                guidGenerator
                    .SetupSequence(s => s.NewGuid())
                    .Returns(newFootnote1.Id)
                    .Returns(newFootnote2.Id);

                var dataBlockService = new Mock<IDataBlockService>(Strict);

                dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(amendment.Id))
                    .Returns(Task.CompletedTask);

                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext,
                    dataBlockService: dataBlockService.Object,
                    footnoteRepository: footnoteRepository.Object,
                    guidGenerator: guidGenerator.Object);

                var result =
                    await service.CopyFootnotes(release.Id, amendment.Id);

                Assert.True(result.IsRight);
                Assert.Equal(2, result.Right.Count);
                Assert.Contains(newFootnote1, result.Right);
                Assert.Contains(newFootnote2, result.Right);

                MockUtils.VerifyAllMocks(footnoteRepository);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var newFootnotesFromDb = statisticsDbContext
                    .Footnote
                    .Include(f => f.Filters)
                    .Include(f => f.FilterGroups)
                    .Include(f => f.FilterItems)
                    .Include(f => f.Releases)
                    .Include(f => f.Subjects)
                    .Include(f => f.Indicators)
                    .Where(f => f.Releases.FirstOrDefault(r => r.ReleaseId == amendment.Id) != null)
                    .OrderBy(f => f.Content)
                    .ToList();

                Assert.Equal(2, newFootnotesFromDb.Count);
                AssertFootnoteDetailsCopiedCorrectly(footnote1, newFootnotesFromDb[0]);
                AssertFootnoteDetailsCopiedCorrectly(footnote2, newFootnotesFromDb[1]);
            }

            void AssertFootnoteDetailsCopiedCorrectly(Footnote originalFootnote, Footnote newFootnote)
            {
                Assert.Equal(newFootnote.Content, originalFootnote.Content);

                Assert.Equal(
                    originalFootnote
                        .Filters
                        .SelectNullSafe(f => f.FilterId)
                        .ToList(),
                    newFootnote
                        .Filters
                        .SelectNullSafe(f => f.FilterId)
                        .ToList());

                Assert.Equal(
                    originalFootnote
                        .FilterGroups
                        .SelectNullSafe(f => f.FilterGroupId)
                        .ToList(),
                    newFootnote
                        .FilterGroups
                        .SelectNullSafe(f => f.FilterGroupId)
                        .ToList());

                Assert.Equal(
                    originalFootnote
                        .FilterItems
                        .SelectNullSafe(f => f.FilterItemId)
                        .ToList(),
                    newFootnote
                        .FilterItems
                        .SelectNullSafe(f => f.FilterItemId)
                        .ToList());

                Assert.Equal(
                    originalFootnote
                        .Subjects
                        .SelectNullSafe(f => f.SubjectId)
                        .ToList(),
                    newFootnote
                        .Subjects
                        .SelectNullSafe(f => f.SubjectId)
                        .ToList());

                Assert.Equal(
                    originalFootnote
                        .Indicators
                        .SelectNullSafe(f => f.IndicatorId)
                        .ToList(),
                    newFootnote
                        .Indicators
                        .SelectNullSafe(f => f.IndicatorId)
                        .ToList());
            }
        }

        [Fact]
        public async Task DeleteFootnote()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var filterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
            };

            var filterGroup = new FilterGroup
            {
                Id = Guid.NewGuid(),
                FilterItems = new List<FilterItem>
                {
                    filterItem,
                },
            };

            var filter = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                FilterGroups = new List<FilterGroup>
                {
                    filterGroup,
                },
            };

            var indicator = new Indicator
            {
                Id = Guid.NewGuid(),
            };

            var indicatorGroup = new IndicatorGroup
            {
                Subject = subject,
                Indicators = new List<Indicator>
                {
                    indicator,
                }
            };

            var footnote = new Footnote
            {
                Content = "Test footnote 1",
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = release,
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new SubjectFootnote
                    {
                        Subject = subject,
                    }
                },
                Filters = new List<FilterFootnote>
                {
                    new FilterFootnote
                    {
                        FilterId = filter.Id,
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroupId =  filterGroup.Id,
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
                    {
                        FilterItemId = filterItem.Id,
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new IndicatorFootnote
                    {
                        IndicatorId = indicator.Id,
                    }
                },
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(
                    release,
                    footnote,
                    filter,
                    indicatorGroup,
                    releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id,
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = await service.DeleteFootnote(release.Id, footnote.Id);
                await statisticsDbContext.SaveChangesAsync();

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(dataBlockService);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                Assert.Empty(statisticsDbContext.Footnote.ToList());
                Assert.Empty(statisticsDbContext.ReleaseFootnote.ToList());
                Assert.Empty(statisticsDbContext.SubjectFootnote.ToList());
                Assert.Empty(statisticsDbContext.FilterFootnote.ToList());
                Assert.Empty(statisticsDbContext.FilterGroupFootnote.ToList());
                Assert.Empty(statisticsDbContext.FilterItemFootnote.ToList());
                Assert.Empty(statisticsDbContext.IndicatorFootnote.ToList());
            }
        }

        [Fact]
        public async Task UpdateFootnote_AddCriteria()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var filterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
            };

            var filterGroup = new FilterGroup
            {
                Id = Guid.NewGuid(),
                FilterItems = new List<FilterItem>
                {
                    filterItem,
                },
            };

            var filter = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                FilterGroups = new List<FilterGroup>
                {
                    filterGroup,
                },
            };

            var indicator = new Indicator
            {
                Id = Guid.NewGuid(),
            };

            var indicatorGroup = new IndicatorGroup
            {
                Subject = subject,
                Indicators = new List<Indicator>
                {
                    indicator,
                }
            };

            var footnote = new Footnote
            {
                Content = "Original footnote",
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        ReleaseId = release.Id,
                    }
                },
                Subjects = new List<SubjectFootnote>(),
                Filters = new List<FilterFootnote>(),
                FilterGroups = new List<FilterGroupFootnote>(),
                FilterItems = new List<FilterItemFootnote>(),
                Indicators = new List<IndicatorFootnote>(),
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(
                    release,
                    footnote,
                    filter,
                    indicatorGroup,
                    releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id,
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = await service.UpdateFootnote(
                    release.Id,
                    footnote.Id,
                    "Updated footnote",
                    new List<Guid>{ filter.Id },
                    new List<Guid>{ filterGroup.Id },
                    new List<Guid>{ filterItem.Id },
                    new List<Guid>{ indicator.Id },
                    new List<Guid>{ subject.Id });

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(dataBlockService);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var dbFootnote = Assert.Single(statisticsDbContext.Footnote.ToList());
                Assert.Equal(footnote.Id, dbFootnote.Id);
                Assert.Equal("Updated footnote", dbFootnote.Content);

                var dbReleaseFootnote = Assert.Single(statisticsDbContext.ReleaseFootnote.ToList());
                Assert.Equal(release.Id, dbReleaseFootnote.ReleaseId);
                Assert.Equal(footnote.Id, dbReleaseFootnote.FootnoteId);

                var dbSubjectFootnote = Assert.Single(statisticsDbContext.SubjectFootnote.ToList());
                Assert.Equal(subject.Id, dbSubjectFootnote.SubjectId);
                Assert.Equal(footnote.Id, dbSubjectFootnote.FootnoteId);

                var dbFilterFootnote = Assert.Single(statisticsDbContext.FilterFootnote.ToList());
                Assert.Equal(filter.Id, dbFilterFootnote.FilterId);
                Assert.Equal(footnote.Id, dbFilterFootnote.FootnoteId);

                var dbFilterGroupFootnote = Assert.Single(statisticsDbContext.FilterGroupFootnote.ToList());
                Assert.Equal(filterGroup.Id, dbFilterGroupFootnote.FilterGroupId);
                Assert.Equal(footnote.Id, dbFilterGroupFootnote.FootnoteId);

                var dbFilterItemFootnote = Assert.Single(statisticsDbContext.FilterItemFootnote.ToList());
                Assert.Equal(filterItem.Id, dbFilterItemFootnote.FilterItemId);
                Assert.Equal(footnote.Id, dbFilterItemFootnote.FootnoteId);

                var dbIndicatorFootnote = Assert.Single(statisticsDbContext.IndicatorFootnote.ToList());
                Assert.Equal(indicator.Id, dbIndicatorFootnote.IndicatorId);
                Assert.Equal(footnote.Id, dbIndicatorFootnote.FootnoteId);
            }
        }

        [Fact]
        public async Task UpdateFootnote_RemoveCriteria()
        {
            var release = new Release();

            var subject = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var filterItem = new FilterItem
            {
                Id = Guid.NewGuid(),
            };

            var filterGroup = new FilterGroup
            {
                Id = Guid.NewGuid(),
                FilterItems = new List<FilterItem>
                {
                    filterItem,
                },
            };

            var filter = new Filter
            {
                Id = Guid.NewGuid(),
                SubjectId = subject.Id,
                FilterGroups = new List<FilterGroup>
                {
                    filterGroup,
                },
            };

            var indicator = new Indicator
            {
                Id = Guid.NewGuid(),
            };

            var indicatorGroup = new IndicatorGroup
            {
                Subject = subject,
                Indicators = new List<Indicator>
                {
                    indicator,
                }
            };

            var footnote = new Footnote
            {
                Content = "Original footnote",
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = release,
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new SubjectFootnote
                    {
                        Subject = subject,
                    }
                },
                Filters = new List<FilterFootnote>
                {
                    new FilterFootnote
                    {
                        FilterId = filter.Id,
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroupId =  filterGroup.Id,
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
                    {
                        FilterItemId = filterItem.Id,
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new IndicatorFootnote
                    {
                        IndicatorId = indicator.Id,
                    }
                },
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject,
            };

            var contextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddRangeAsync(
                    release,
                    footnote,
                    filter,
                    indicatorGroup,
                    releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id,
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = await service.UpdateFootnote(
                    release.Id,
                    footnote.Id,
                    "Updated footnote",
                    new List<Guid>(),
                    new List<Guid>(),
                    new List<Guid>(),
                    new List<Guid>(),
                    new List<Guid>());

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(dataBlockService);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var dbFootnote = Assert.Single(statisticsDbContext.Footnote.ToList());
                Assert.Equal(footnote.Id, dbFootnote.Id);
                Assert.Equal("Updated footnote", dbFootnote.Content);

                var dbReleaseFootnote = Assert.Single(statisticsDbContext.ReleaseFootnote.ToList());
                Assert.Equal(release.Id, dbReleaseFootnote.ReleaseId);
                Assert.Equal(footnote.Id, dbReleaseFootnote.FootnoteId);

                Assert.Empty(statisticsDbContext.SubjectFootnote.ToList());
                Assert.Empty(statisticsDbContext.FilterFootnote.ToList());
                Assert.Empty(statisticsDbContext.FilterGroupFootnote.ToList());
                Assert.Empty(statisticsDbContext.FilterItemFootnote.ToList());
                Assert.Empty(statisticsDbContext.IndicatorFootnote.ToList());
            }
        }

        private FootnoteService SetupFootnoteService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IUserService userService = null,
            IDataBlockService dataBlockService = null,
            IFootnoteRepository footnoteRepository = null,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper = null,
            IGuidGenerator guidGenerator = null)
        {
            var contentContext = contentDbContext ?? new Mock<ContentDbContext>().Object;

            return new FootnoteService(
                statisticsDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
                footnoteRepository ?? new FootnoteRepository(statisticsDbContext),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                guidGenerator ?? new SequentialGuidGenerator()
            );
        }
    }
}
