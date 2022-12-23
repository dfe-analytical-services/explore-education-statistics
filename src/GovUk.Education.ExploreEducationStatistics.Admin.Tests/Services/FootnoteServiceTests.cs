#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FootnoteServiceTests
    {
        [Fact]
        public async Task CreateFootnote()
        {
            var release = new Release();

            var releaseSubjects = new List<ReleaseSubject>
            {
                new()
                {
                    Release = release,
                    Subject = new Subject()
                },
                new()
                {
                    Release = release,
                    Subject = new Subject
                    {
                        Filters = new List<Filter>
                        {
                            new()
                            {
                                Label = "Filter 1"
                            },
                            new()
                            {
                                Label = "Filter 2",
                                FilterGroups = new List<FilterGroup>
                                {
                                    new()
                                    {
                                        Label = "Filter 2 group 1",
                                    },
                                    new()
                                    {
                                        Label = "Filter 2 group 2",
                                        FilterItems = new List<FilterItem>
                                        {
                                            new()
                                            {
                                                Label = "Filter 2 group 2 item 1"
                                            }
                                        }
                                    },
                                }
                            }
                        },

                        IndicatorGroups = new List<IndicatorGroup>
                        {
                            new()
                            {
                                Label = "Indicator group 1",
                                Indicators = new List<Indicator>
                                {
                                    new()
                                    {
                                        Label = "Indicator group 1 item 1"
                                    }
                                }
                            }
                        },
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            // Footnote can be applied to the entire dataset with a subject link
            // or to specific data with links to filters/filter groups/filter items/indicators
            // Get the target references which the created footnote will be applied to
            var subject = releaseSubjects[0].Subject;
            var filter = releaseSubjects[1].Subject
                .Filters.First();
            var filterGroup = releaseSubjects[1].Subject
                .Filters.ToList()[1]
                .FilterGroups.First();
            var filterItem = releaseSubjects[1].Subject
                .Filters.ToList()[1]
                .FilterGroups.ToList()[1]
                .FilterItems.First();
            var indicator = releaseSubjects[1].Subject.IndicatorGroups.First().Indicators.First();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = (await service.CreateFootnote(
                    releaseId: release.Id,
                    "Test footnote",
                    filterIds: SetOf(filter.Id),
                    filterGroupIds: SetOf(filterGroup.Id),
                    filterItemIds: SetOf(filterItem.Id),
                    indicatorIds: SetOf(indicator.Id),
                    subjectIds: SetOf(subject.Id)
                )).AssertRight();

                MockUtils.VerifyAllMocks(dataBlockService);

                Assert.Equal("Test footnote", result.Content);
                Assert.Equal(0, result.Order);

                var releaseFootnote = Assert.Single(result.Releases);
                Assert.Equal(release.Id, releaseFootnote.ReleaseId);

                var subjectFootnote = Assert.Single(result.Subjects);
                Assert.Equal(subject.Id, subjectFootnote.SubjectId);

                var filterFootnote = Assert.Single(result.Filters);
                Assert.Equal(filter.Id, filterFootnote.FilterId);

                var filterGroupFootnote = Assert.Single(result.FilterGroups);
                Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);

                var filterItemFootnote = Assert.Single(result.FilterItems);
                Assert.Equal(filterItem.Id, filterItemFootnote.FilterItemId);

                var indicatorFootnote = Assert.Single(result.Indicators);
                Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var footnotes = await statisticsDbContext.Footnote
                    .Include(f => f.Releases)
                    .Include(f => f.Subjects)
                    .Include(f => f.Filters)
                    .Include(f => f.FilterGroups)
                    .Include(f => f.FilterItems)
                    .Include(f => f.Indicators)
                    .ToListAsync();

                var savedFootnote = Assert.Single(footnotes);

                Assert.Equal("Test footnote", savedFootnote.Content);
                Assert.Equal(0, savedFootnote.Order);

                var releaseFootnote = Assert.Single(savedFootnote.Releases);
                Assert.Equal(release.Id, releaseFootnote.ReleaseId);

                var subjectFootnote = Assert.Single(savedFootnote.Subjects);
                Assert.Equal(subject.Id, subjectFootnote.SubjectId);

                var filterFootnote = Assert.Single(savedFootnote.Filters);
                Assert.Equal(filter.Id, filterFootnote.FilterId);

                var filterGroupFootnote = Assert.Single(savedFootnote.FilterGroups);
                Assert.Equal(filterGroup.Id, filterGroupFootnote.FilterGroupId);

                var filterItemFootnote = Assert.Single(savedFootnote.FilterItems);
                Assert.Equal(filterItem.Id, filterItemFootnote.FilterItemId);

                var indicatorFootnote = Assert.Single(savedFootnote.Indicators);
                Assert.Equal(indicator.Id, indicatorFootnote.IndicatorId);
            }
        }

        [Fact]
        public async Task CreateFootnote_MultipleFootnotesHaveExpectedOrder()
        {
            var subject = new Subject();

            // Create a release which already has some existing footnotes
            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 2",
                            Order = 1,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    }
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = (await service.CreateFootnote(
                    releaseId: release.Id,
                    "Footnote 3",
                    filterIds: SetOf<Guid>(),
                    filterGroupIds: SetOf<Guid>(),
                    filterItemIds: SetOf<Guid>(),
                    indicatorIds: SetOf<Guid>(),
                    subjectIds: SetOf(subject.Id)
                )).AssertRight();

                MockUtils.VerifyAllMocks(dataBlockService);

                // Check that the created footnote is assigned the next order in sequence
                Assert.Equal("Footnote 3", result.Content);
                Assert.Equal(2, result.Order);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var footnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

                Assert.Equal(3, footnotes.Count);

                Assert.Equal("Footnote 1", footnotes[0].Content);
                Assert.Equal(0, footnotes[0].Order);
                Assert.Equal("Footnote 2", footnotes[1].Content);
                Assert.Equal(1, footnotes[1].Order);
                Assert.Equal("Footnote 3", footnotes[2].Content);
                Assert.Equal(2, footnotes[2].Order);
            }
        }

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
                    new()
                    {
                        Release = release
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new()
                    {
                        Subject = subject
                    }
                },
                Filters = new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = filter
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = filterGroup
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = filterItem
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new()
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
                    new()
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
                var result = await service.GetFootnote(
                    releaseId: invalidReleaseId,
                    footnoteId: footnote.Id);

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
                    new()
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

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var filter = new Filter
            {
                Label = "Test filter 1",
                Subject = releaseSubject.Subject
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
                IndicatorGroup = new IndicatorGroup
                {
                    Subject = releaseSubject.Subject
                }
            };

            var footnote1 = new Footnote
            {
                Content = "Test footnote 1",
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Release = release
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new()
                    {
                        Subject = releaseSubject.Subject
                    }
                },
                Filters = new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = filter
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = filterGroup
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = filterItem
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = indicator
                    }
                },
                Order = 0
            };

            var footnote2 = new Footnote
            {
                Content = "Test footnote 2",
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Release = release
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new()
                    {
                        Subject = releaseSubject.Subject
                    }
                },
                Filters = new List<FilterFootnote>(),
                FilterGroups = new List<FilterGroupFootnote>(),
                FilterItems = new List<FilterItemFootnote>(),
                Indicators = new List<IndicatorFootnote>(),
                Order = 1
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(release, amendment);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject);
                await statisticsDbContext.Footnote.AddRangeAsync(footnote1, footnote2);

                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.Releases.AddRangeAsync(new Content.Model.Release
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
                var service = SetupFootnoteService(
                    statisticsDbContext,
                    contentDbContext);

                var result =
                    await service.CopyFootnotes(release.Id, amendment.Id);

                result.AssertRight();
                Assert.Equal(2, result.Right.Count);
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
                Assert.Equal(originalFootnote.Content, newFootnote.Content);
                Assert.Equal(originalFootnote.Order, newFootnote.Order);

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
            var subject1 = new Subject();
            var subject2 = new Subject();

            var subject2Filter1 = new Filter
            {
                Label = "Subject 2 filter 1",
                Subject = subject2
            };

            var subject2Filter2 = new Filter
            {
                Label = "Subject 2 filter 2",
                Subject = subject2,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 2 group 1"
                    },
                    new()
                    {
                        Label = "Filter 2 group 2",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 1 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject2IndicatorGroup1 = new IndicatorGroup
            {
                Label = "Subject 2 indicator group 1",
                Subject = subject2,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Label = "Indicator 1"
                    }
                }
            };

            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Test footnote 1",
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject1
                                }
                            },
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject2Filter1
                                }
                            },
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = subject2Filter2.FilterGroups[0]
                                }
                            },
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = subject2Filter2.FilterGroups[1].FilterItems[0]
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject2IndicatorGroup1.Indicators[0]
                                }
                            },
                            Order = 0
                        }
                    }
                }
            };

            var releaseSubjects = new List<ReleaseSubject>
            {
                new()
                {
                    Release = release,
                    Subject = subject1
                },
                new()
                {
                    Release = release,
                    Subject = subject2
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Subject.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.Filter.AddRangeAsync(subject2Filter1, subject2Filter2);
                await statisticsDbContext.IndicatorGroup.AddAsync(subject2IndicatorGroup1);
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubjects);
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

                var result = await service.DeleteFootnote(
                    releaseId: release.Id,
                    footnoteId: release.Footnotes.ToList()[0].FootnoteId);

                MockUtils.VerifyAllMocks(dataBlockService);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                Assert.Empty(statisticsDbContext.Footnote);
                Assert.Empty(statisticsDbContext.ReleaseFootnote);
                Assert.Empty(statisticsDbContext.SubjectFootnote);
                Assert.Empty(statisticsDbContext.FilterFootnote);
                Assert.Empty(statisticsDbContext.FilterGroupFootnote);
                Assert.Empty(statisticsDbContext.FilterItemFootnote);
                Assert.Empty(statisticsDbContext.IndicatorFootnote);
            }
        }

        [Fact]
        public async Task DeleteFootnote_MultipleFootnotesHaveExpectedOrder()
        {
            var subject = new Subject();

            // Create a release which already has some existing footnotes
            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 2",
                            Order = 1,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 3",
                            Order = 2,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    }
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                var result = await service.DeleteFootnote(
                    releaseId: release.Id,
                    footnoteId: release.Footnotes.First().Footnote.Id);

                MockUtils.VerifyAllMocks(dataBlockService);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var footnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

                Assert.Equal(2, footnotes.Count);

                // Expect that the remaining footnotes have been reordered
                Assert.Equal("Footnote 2", footnotes[0].Content);
                Assert.Equal(0, footnotes[0].Order);
                Assert.Equal("Footnote 3", footnotes[1].Content);
                Assert.Equal(1, footnotes[1].Order);
            }
        }

        [Fact]
        public async Task UpdateFootnote_AddCriteria()
        {
            var release = new Release();

            var subject = new Subject();

            var filterItem = new FilterItem();

            var filterGroup = new FilterGroup
            {
                FilterItems = new List<FilterItem>
                {
                    filterItem,
                },
            };

            var filter = new Filter
            {
                Subject = subject,
                FilterGroups = new List<FilterGroup>
                {
                    filterGroup,
                },
            };

            var indicator = new Indicator();

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
                    new()
                    {
                        Release = release,
                    }
                },
                Subjects = new List<SubjectFootnote>(),
                Filters = new List<FilterFootnote>(),
                FilterGroups = new List<FilterGroupFootnote>(),
                FilterItems = new List<FilterItemFootnote>(),
                Indicators = new List<IndicatorFootnote>(),
                Order = 1
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
                    releaseId: release.Id,
                    footnoteId: footnote.Id,
                    "Updated footnote",
                    filterIds: SetOf(filter.Id),
                    filterGroupIds: SetOf(filterGroup.Id),
                    filterItemIds: SetOf(filterItem.Id),
                    indicatorIds: SetOf(indicator.Id),
                    subjectIds: SetOf(subject.Id));

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(dataBlockService);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var savedFootnote = Assert.Single(statisticsDbContext.Footnote);
                Assert.Equal(footnote.Id, savedFootnote.Id);
                Assert.Equal("Updated footnote", savedFootnote.Content);
                Assert.Equal(1, savedFootnote.Order);

                var savedReleaseFootnote = Assert.Single(statisticsDbContext.ReleaseFootnote);
                Assert.Equal(release.Id, savedReleaseFootnote.ReleaseId);
                Assert.Equal(footnote.Id, savedReleaseFootnote.FootnoteId);

                var savedSubjectFootnote = Assert.Single(statisticsDbContext.SubjectFootnote);
                Assert.Equal(subject.Id, savedSubjectFootnote.SubjectId);
                Assert.Equal(footnote.Id, savedSubjectFootnote.FootnoteId);

                var savedFilterFootnote = Assert.Single(statisticsDbContext.FilterFootnote);
                Assert.Equal(filter.Id, savedFilterFootnote.FilterId);
                Assert.Equal(footnote.Id, savedFilterFootnote.FootnoteId);

                var savedFilterGroupFootnote = Assert.Single(statisticsDbContext.FilterGroupFootnote);
                Assert.Equal(filterGroup.Id, savedFilterGroupFootnote.FilterGroupId);
                Assert.Equal(footnote.Id, savedFilterGroupFootnote.FootnoteId);

                var savedFilterItemFootnote = Assert.Single(statisticsDbContext.FilterItemFootnote);
                Assert.Equal(filterItem.Id, savedFilterItemFootnote.FilterItemId);
                Assert.Equal(footnote.Id, savedFilterItemFootnote.FootnoteId);

                var savedIndicatorFootnote = Assert.Single(statisticsDbContext.IndicatorFootnote);
                Assert.Equal(indicator.Id, savedIndicatorFootnote.IndicatorId);
                Assert.Equal(footnote.Id, savedIndicatorFootnote.FootnoteId);
            }
        }

        [Fact]
        public async Task UpdateFootnote_RemoveCriteria()
        {
            var release = new Release();

            var subject = new Subject();

            var filterItem = new FilterItem();

            var filterGroup = new FilterGroup
            {
                FilterItems = new List<FilterItem>
                {
                    filterItem,
                },
            };

            var filter = new Filter
            {
                Subject = subject,
                FilterGroups = new List<FilterGroup>
                {
                    filterGroup,
                },
            };

            var indicator = new Indicator();

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
                    new()
                    {
                        Release = release,
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new()
                    {
                        Subject = subject,
                    }
                },
                Filters = new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = filter,
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = filterGroup,
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = filterItem,
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = indicator,
                    }
                },
                Order = 1
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
                    releaseId: release.Id,
                    footnoteId: footnote.Id,
                    "Updated footnote",
                    filterIds: SetOf<Guid>(),
                    filterGroupIds: SetOf<Guid>(),
                    filterItemIds: SetOf<Guid>(),
                    indicatorIds: SetOf<Guid>(),
                    subjectIds: SetOf<Guid>());

                result.AssertRight();
            }

            MockUtils.VerifyAllMocks(dataBlockService);

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var savedFootnote = Assert.Single(statisticsDbContext.Footnote);
                Assert.Equal(footnote.Id, savedFootnote.Id);
                Assert.Equal("Updated footnote", savedFootnote.Content);
                Assert.Equal(1, savedFootnote.Order);

                var savedReleaseFootnote = Assert.Single(statisticsDbContext.ReleaseFootnote);
                Assert.Equal(release.Id, savedReleaseFootnote.ReleaseId);
                Assert.Equal(footnote.Id, savedReleaseFootnote.FootnoteId);

                Assert.Empty(statisticsDbContext.SubjectFootnote);
                Assert.Empty(statisticsDbContext.FilterFootnote);
                Assert.Empty(statisticsDbContext.FilterGroupFootnote);
                Assert.Empty(statisticsDbContext.FilterItemFootnote);
                Assert.Empty(statisticsDbContext.IndicatorFootnote);
            }
        }

        [Fact]
        public async Task UpdateFootnotes()
        {
            var subject = new Subject();

            // Create a release which already has some existing footnotes
            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 2",
                            Order = 1,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 3",
                            Order = 2,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    }
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            var dataBlockService = new Mock<IDataBlockService>(Strict);

            dataBlockService.Setup(mock => mock.InvalidateCachedDataBlocks(release.Id))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    dataBlockService: dataBlockService.Object);

                // Create a request with identical footnotes but in a new order
                var request = new FootnotesUpdateRequest
                {
                    FootnoteIds = ListOf(
                        release.Footnotes.ToList()[2].FootnoteId,
                        release.Footnotes.ToList()[0].FootnoteId,
                        release.Footnotes.ToList()[1].FootnoteId
                    )
                };

                var result = await service.UpdateFootnotes(
                    release.Id,
                    request);

                MockUtils.VerifyAllMocks(dataBlockService);

                result.AssertRight();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var footnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

                Assert.Equal(3, footnotes.Count);

                // Check the footnotes have been reordered
                Assert.Equal("Footnote 3", footnotes[0].Content);
                Assert.Equal(0, footnotes[0].Order);
                Assert.Equal("Footnote 1", footnotes[1].Content);
                Assert.Equal(1, footnotes[1].Order);
                Assert.Equal("Footnote 2", footnotes[2].Content);
                Assert.Equal(2, footnotes[2].Order);
            }
        }

        [Fact]
        public async Task UpdateFootnotes_ReleaseNotFound()
        {
            var subject = new Subject();

            // Create some existing footnotes but for a different release than the one which will be used in the update
            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 2",
                            Order = 1,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    }
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                // Attempt to update the footnotes but use a different release id
                var result = await service.UpdateFootnotes(
                    Guid.NewGuid(),
                    new FootnotesUpdateRequest
                    {
                        FootnoteIds = ListOf(
                            release.Footnotes.ToList()[0].FootnoteId,
                            release.Footnotes.ToList()[1].FootnoteId
                        )
                    });

                result.AssertNotFound();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var footnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

                // Verify that the footnotes remain untouched
                Assert.Equal(2, footnotes.Count);
                Assert.Equal("Footnote 1", footnotes[0].Content);
                Assert.Equal(0, footnotes[0].Order);
                Assert.Equal("Footnote 2", footnotes[1].Content);
                Assert.Equal(1, footnotes[1].Order);
            }
        }

        [Fact]
        public async Task UpdateFootnotes_FootnoteMissing()
        {
            var subject = new Subject();

            // Create a release which already has some existing footnotes
            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 2",
                            Order = 1,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    }
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                // Request has the first footnote id missing
                var request = new FootnotesUpdateRequest
                {
                    FootnoteIds = ListOf(
                        release.Footnotes.ToList()[1].FootnoteId
                    )
                };

                var result = await service.UpdateFootnotes(
                    release.Id,
                    request);

                result.AssertBadRequest(FootnotesDifferFromReleaseFootnotes);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var footnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

                // Verify that the footnotes remain untouched
                Assert.Equal(2, footnotes.Count);
                Assert.Equal("Footnote 1", footnotes[0].Content);
                Assert.Equal(0, footnotes[0].Order);
                Assert.Equal("Footnote 2", footnotes[1].Content);
                Assert.Equal(1, footnotes[1].Order);
            }
        }

        [Fact]
        public async Task UpdateFootnotes_FootnoteNotForRelease()
        {
            var subject = new Subject();

            // Create a release which already has some existing footnotes
            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Footnote 2",
                            Order = 1,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject
                                }
                            }
                        }
                    }
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = subject
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.Subject.AddAsync(subject);
                await statisticsDbContext.Release.AddAsync(release);
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();

                await contentDbContext.AddAsync(new Content.Model.Release
                {
                    Id = release.Id
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = SetupFootnoteService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext);

                // Request has a footnote id not for this release
                var request = new FootnotesUpdateRequest
                {
                    FootnoteIds = ListOf(
                        release.Footnotes.ToList()[1].FootnoteId,
                        release.Footnotes.ToList()[0].FootnoteId,
                        Guid.NewGuid()
                    )
                };

                var result = await service.UpdateFootnotes(
                    release.Id,
                    request);

                result.AssertBadRequest(FootnotesDifferFromReleaseFootnotes);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var footnotes = await statisticsDbContext.Footnote
                    .OrderBy(f => f.Order)
                    .ToListAsync();

                // Verify that the footnotes remain untouched
                Assert.Equal(2, footnotes.Count);
                Assert.Equal("Footnote 1", footnotes[0].Content);
                Assert.Equal(0, footnotes[0].Order);
                Assert.Equal("Footnote 2", footnotes[1].Content);
                Assert.Equal(1, footnotes[1].Order);
            }
        }

        private static FootnoteService SetupFootnoteService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext? contentDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IUserService? userService = null,
            IDataBlockService? dataBlockService = null,
            IFootnoteRepository? footnoteRepository = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null)
        {
            var contentContext = contentDbContext ?? new Mock<ContentDbContext>().Object;

            return new FootnoteService(
                statisticsDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                dataBlockService ?? Mock.Of<IDataBlockService>(Strict),
                footnoteRepository ?? new FootnoteRepository(statisticsDbContext),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext)
            );
        }
    }
}
