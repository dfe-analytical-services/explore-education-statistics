#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class FootnoteRepositoryTests
    {
        [Fact]
        public async Task GetFilteredFootnotes()
        {
            var subject1 = new Subject();
            var subject2 = new Subject();

            var subject1Filter1 = new Filter
            {
                Label = "Subject 1 filter 1",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
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

            var subject1Filter2 = new Filter
            {
                Label = "Subject 1 filter 2",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 2 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 2 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject1Filter3 = new Filter
            {
                Label = "Subject 1 filter 3",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 3 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 3 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject1IndicatorGroup1 = new IndicatorGroup
            {
                Label = "Subject 1 indicator group 1",
                Subject = subject1,
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
                            Content = "Applies to subject 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 1",
                            Order = 1,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject1Filter1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 2 group 1",
                            Order = 2,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = subject1Filter2.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 3 group 1 item 1",
                            Order = 3,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = subject1Filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 indicator 1",
                            Order = 4,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject1IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        // Footnote applies to subject 1 filter 1
                        // and subject 1 filter 2 group 1
                        // and subject 1 filter 3 group 1 item 1
                        // and subject 1 indicator 1
                        Footnote = new Footnote
                        {
                            Content = "Applies to multiple attributes of subject 1",
                            Order = 5,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject1Filter1
                                }
                            },
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = subject1Filter2.FilterGroups[0]
                                }
                            },
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = subject1Filter3.FilterGroups[0].FilterItems[0]
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject1IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2",
                            Order = 6,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject2
                                }
                            }
                        }
                    }
                }
            };

            var releaseFootnotes = release.Footnotes.ToList();

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

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filter1, subject1Filter2, subject1Filter3);
                await context.IndicatorGroup.AddAsync(subject1IndicatorGroup1);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                //  Get the footnotes applying to subject 1 or any of its filter items or indicators
                var filter1Group1Item1Id = subject1Filter1.FilterGroups[0].FilterItems[0].Id;
                var filter2Group1Item1Id = subject1Filter2.FilterGroups[0].FilterItems[0].Id;
                var filter3Group1Item1Id = subject1Filter3.FilterGroups[0].FilterItems[0].Id;
                var indicatorGroup1Item1Id = subject1IndicatorGroup1.Indicators[0].Id;

                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(filter1Group1Item1Id, filter2Group1Item1Id, filter3Group1Item1Id),
                    indicatorIds: ListOf(indicatorGroup1Item1Id));

                // Check that only footnotes applying to subject 1 or any of its filter items or indicators are returned
                Assert.Equal(6, results.Count);

                // Footnote applies to the requested subject
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 1", results[0].Content);

                // Footnote applies to a requested filter item via its filter
                Assert.Equal(releaseFootnotes[1].FootnoteId, results[1].Id);
                Assert.Equal("Applies to subject 1 filter 1", results[1].Content);

                // Footnote applies to a requested filter item via its filter group
                Assert.Equal(releaseFootnotes[2].FootnoteId, results[2].Id);
                Assert.Equal("Applies to subject 1 filter 2 group 1", results[2].Content);

                // Footnote applies to a requested filter item
                Assert.Equal(releaseFootnotes[3].FootnoteId, results[3].Id);
                Assert.Equal("Applies to subject 1 filter 3 group 1 item 1", results[3].Content);

                // Footnote applies to a requested indicator 
                Assert.Equal(releaseFootnotes[4].FootnoteId, results[4].Id);
                Assert.Equal("Applies to subject 1 indicator 1", results[4].Content);

                // Footnote applies to a various criteria related to the requested filter items and indicators
                Assert.Equal(releaseFootnotes[5].FootnoteId, results[5].Id);
                Assert.Equal("Applies to multiple attributes of subject 1", results[5].Content);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_FilterBySubject()
        {
            var subject1 = new Subject();
            var subject2 = new Subject();

            var subject1Filter1 = new Filter
            {
                Label = "Subject 1 filter 1",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
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

            var subject2Filter1 = new Filter
            {
                Label = "Subject 2 filter 1",
                Subject = subject2,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
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

            var subject1IndicatorGroup1 = new IndicatorGroup
            {
                Label = "Subject 1 indicator group 1",
                Subject = subject1,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Label = "Indicator 1"
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

            // Set up a release with footnotes that apply to either subject 1 or subject 2 but not both
            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2",
                            Order = 1,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject2
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 1",
                            Order = 2,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject1Filter1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2 filter 1",
                            Order = 3,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject2Filter1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 1 group 1",
                            Order = 4,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = subject1Filter1.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2 filter 1 group 1",
                            Order = 5,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = subject2Filter1.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 1 group 1 item 1",
                            Order = 6,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = subject1Filter1.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2 filter 1 group 1 item 1",
                            Order = 7,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = subject2Filter1.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 indicator 1",
                            Order = 8,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject1IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2 indicator 1",
                            Order = 9,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject2IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    }
                }
            };

            var releaseFootnotes = release.Footnotes.ToList();

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

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filter1, subject2Filter1);
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup1, subject2IndicatorGroup1);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                // This test makes sure that all footnotes which apply exclusively to subjects other than the one
                // requested are excluded

                //  Get the footnotes applying to subject 1 or any of its filter items or indicators
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(subject1Filter1.FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: ListOf(subject1IndicatorGroup1.Indicators[0].Id));

                // Check that only footnotes applying to subject 1 or any of its filter items or indicators are returned
                // and that all footnotes applying to subject 2 are excluded
                Assert.Equal(5, results.Count);

                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 1", results[0].Content);

                Assert.Equal(releaseFootnotes[2].FootnoteId, results[1].Id);
                Assert.Equal("Applies to subject 1 filter 1", results[1].Content);

                // Footnote applies to a requested filter item via its filter group
                Assert.Equal(releaseFootnotes[4].FootnoteId, results[2].Id);
                Assert.Equal("Applies to subject 1 filter 1 group 1", results[2].Content);

                // Footnote applies to a requested filter item via its filter group
                Assert.Equal(releaseFootnotes[6].FootnoteId, results[3].Id);
                Assert.Equal("Applies to subject 1 filter 1 group 1 item 1", results[3].Content);

                Assert.Equal(releaseFootnotes[8].FootnoteId, results[4].Id);
                Assert.Equal("Applies to subject 1 indicator 1", results[4].Content);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_FilterByFiltersAndIndicators()
        {
            var subject1 = new Subject();

            var filter1 = new Filter
            {
                Label = "Filter 1",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 1 group 1 item 1"
                            },
                            new()
                            {
                                Label = "Filter 1 group 1 item 2"
                            }
                        }
                    }
                }
            };

            var indicatorGroup1 = new IndicatorGroup
            {
                Label = "Indicator group 1",
                Subject = subject1,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Label = "Indicator 1"
                    },
                    new()
                    {
                        Label = "Indicator 2"
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
                            Content = "Applies to filter 1",
                            Order = 0,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1",
                            Order = 1,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter1.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter item 1",
                            Order = 2,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter item 2",
                            Order = 3,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[1]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to indicator 1",
                            Order = 4,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to indicator 2",
                            Order = 5,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[1]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 and indicator 1",
                            Order = 6,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter1
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 and indicator 2",
                            Order = 7,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter1
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[1]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1 and indicator 1",
                            Order = 8,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter1.FilterGroups[0]
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1 and indicator 2",
                            Order = 9,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter1.FilterGroups[0]
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[1]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter item 1 and indicator 1",
                            Order = 10,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[0]
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter item 1 and indicator 2",
                            Order = 11,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[0]
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[1]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter item 2 and indicator 1",
                            Order = 12,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[1]
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter item 2 and indicator 2",
                            Order = 13,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[1]
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[1]
                                }
                            }
                        }
                    }
                }
            };

            var releaseFootnotes = release.Footnotes.ToList();

            var releaseSubjects = new List<ReleaseSubject>
            {
                new()
                {
                    Release = release,
                    Subject = subject1
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddAsync(subject1);
                await context.Filter.AddAsync(filter1);
                await context.IndicatorGroup.AddAsync(indicatorGroup1);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                //  Get the footnotes applying to filter item 1 and indicator 1
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(filter1.FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: ListOf(indicatorGroup1.Indicators[0].Id));

                // Check that only footnotes applying to filter item 1, or indicator 1
                // are returned.

                // Any footnotes applying to filter item 2 or only to indicator 2 should be excluded
                Assert.Equal(7, results.Count);

                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to filter 1", results[0].Content);

                Assert.Equal(releaseFootnotes[1].FootnoteId, results[1].Id);
                Assert.Equal("Applies to filter 1 group 1", results[1].Content);

                Assert.Equal(releaseFootnotes[2].FootnoteId, results[2].Id);
                Assert.Equal("Applies to filter item 1", results[2].Content);

                Assert.Equal(releaseFootnotes[4].FootnoteId, results[3].Id);
                Assert.Equal("Applies to indicator 1", results[3].Content);

                Assert.Equal(releaseFootnotes[6].FootnoteId, results[4].Id);
                Assert.Equal("Applies to filter 1 and indicator 1", results[4].Content);

                Assert.Equal(releaseFootnotes[8].FootnoteId, results[5].Id);
                Assert.Equal("Applies to filter 1 group 1 and indicator 1", results[5].Content);

                Assert.Equal(releaseFootnotes[10].FootnoteId, results[6].Id);
                Assert.Equal("Applies to filter item 1 and indicator 1", results[6].Content);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_FilterByFilterItems()
        {
            var subject1 = new Subject();

            var filter1 = new Filter
            {
                Label = "Filter 1",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 1 group 1 item 1"
                            },
                            new()
                            {
                                Label = "Filter 1 group 1 item 2"
                            },
                            new()
                            {
                                Label = "Filter 1 group 1 item 3"
                            }
                        }
                    }
                }
            };

            var filter2 = new Filter
            {
                Label = "Filter 2",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 2 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 2 group 1 item 1"
                            },
                            new()
                            {
                                Label = "Filter 2 group 1 item 2"
                            }
                        }
                    }
                }
            };

            var filter3 = new Filter
            {
                Label = "Filter 3",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 3 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 3 group 1 item 1"
                            }
                        }
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
                            Content = "Applies to filter 1",
                            Order = 0,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 3",
                            Order = 1,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter3
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 and filter 3",
                            Order = 2,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter1
                                },
                                new()
                                {
                                    Filter = filter3
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 and filter 3 group 1",
                            Order = 3,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter1
                                }
                            },
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter3.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 3 and filter 1 group 1",
                            Order = 4,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter3
                                }
                            },
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter1.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 and filter 3 group item 1",
                            Order = 5,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter1
                                }
                            },
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 3 and filter 1 group item 1",
                            Order = 6,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter3
                                }
                            },
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1",
                            Order = 7,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter1.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 3 group 1",
                            Order = 8,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter3.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1 and filter 1 group 1 item 1",
                            Order = 9,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter1.FilterGroups[0]
                                }
                            },
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1 and filter 3 group 1 item 1",
                            Order = 10,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter1.FilterGroups[0]
                                }
                            },
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 3 group 1 and filter 1 group 1 item 1",
                            Order = 11,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter3.FilterGroups[0]
                                }
                            },
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 3 group 1 and filter 3 group 1 item 1",
                            Order = 12,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter3.FilterGroups[0]
                                }
                            },
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1 item 1",
                            Order = 13,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1 item 3",
                            Order = 14,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[2]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 3 group 1 item 1",
                            Order = 15,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to filter 1 group 1 item 1 and filter 1 group 1 item 3",
                            Order = 16,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[0]
                                },
                                new()
                                {
                                    FilterItem = filter1.FilterGroups[0].FilterItems[2]
                                }
                            }
                        }
                    }
                }
            };

            var releaseFootnotes = release.Footnotes.ToList();

            var releaseSubjects = new List<ReleaseSubject>
            {
                new()
                {
                    Release = release,
                    Subject = subject1
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddAsync(subject1);
                await context.Filter.AddRangeAsync(filter1, filter2, filter3);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                // Get the footnotes applying to filter 1 group 1 item 1 or filter 1 group 1 item 2
                // or the filter item id's of filter 2
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(filter1.FilterGroups[0].FilterItems[0].Id,
                        filter1.FilterGroups[0].FilterItems[1].Id,
                        filter2.FilterGroups[0].FilterItems[0].Id,
                        filter2.FilterGroups[0].FilterItems[1].Id),
                    indicatorIds: ListOf<Guid>()
                );

                // Check that only footnotes applying to filter 1 group 1 item 1 are returned.

                // No footnotes apply to filter 1 group 1 item 2 or to the filter items of filter 2 but
                // including their additional id's in the parameter shouldn't alter the result.

                // The footnotes which apply only to filter 3, filter 3 group 1 item 1, or filter 3 group 1 item 1
                // should be excluded.
                Assert.Equal(12, results.Count);

                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to filter 1", results[0].Content);

                Assert.Equal(releaseFootnotes[2].FootnoteId, results[1].Id);
                Assert.Equal("Applies to filter 1 and filter 3", results[1].Content);

                Assert.Equal(releaseFootnotes[3].FootnoteId, results[2].Id);
                Assert.Equal("Applies to filter 1 and filter 3 group 1", results[2].Content);

                Assert.Equal(releaseFootnotes[4].FootnoteId, results[3].Id);
                Assert.Equal("Applies to filter 3 and filter 1 group 1", results[3].Content);

                Assert.Equal(releaseFootnotes[5].FootnoteId, results[4].Id);
                Assert.Equal("Applies to filter 1 and filter 3 group item 1", results[4].Content);

                Assert.Equal(releaseFootnotes[6].FootnoteId, results[5].Id);
                Assert.Equal("Applies to filter 3 and filter 1 group item 1", results[5].Content);

                Assert.Equal(releaseFootnotes[7].FootnoteId, results[6].Id);
                Assert.Equal("Applies to filter 1 group 1", results[6].Content);

                Assert.Equal(releaseFootnotes[9].FootnoteId, results[7].Id);
                Assert.Equal("Applies to filter 1 group 1 and filter 1 group 1 item 1", results[7].Content);

                Assert.Equal(releaseFootnotes[10].FootnoteId, results[8].Id);
                Assert.Equal("Applies to filter 1 group 1 and filter 3 group 1 item 1", results[8].Content);

                Assert.Equal(releaseFootnotes[11].FootnoteId, results[9].Id);
                Assert.Equal("Applies to filter 3 group 1 and filter 1 group 1 item 1", results[9].Content);

                Assert.Equal(releaseFootnotes[13].FootnoteId, results[10].Id);
                Assert.Equal("Applies to filter 1 group 1 item 1", results[10].Content);

                Assert.Equal(releaseFootnotes[16].FootnoteId, results[11].Id);
                Assert.Equal("Applies to filter 1 group 1 item 1 and filter 1 group 1 item 3", results[11].Content);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_FilterByIndicators()
        {
            var subject1 = new Subject();

            var indicatorGroup1 = new IndicatorGroup
            {
                Label = "Indicator group 1",
                Subject = subject1,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Label = "Indicator 1"
                    },
                    new()
                    {
                        Label = "Indicator 2"
                    },
                    new()
                    {
                        Label = "Indicator 3"
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
                            Content = "Applies to indicator 1",
                            Order = 0,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to indicator 3",
                            Order = 1,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[2]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to indicator 1 and indicator 3",
                            Order = 2,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[0]
                                },
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[2]
                                }
                            }
                        }
                    },
                }
            };

            var releaseFootnotes = release.Footnotes.ToList();

            var releaseSubjects = new List<ReleaseSubject>
            {
                new()
                {
                    Release = release,
                    Subject = subject1
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddAsync(subject1);
                await context.IndicatorGroup.AddAsync(indicatorGroup1);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                //  Get the footnotes applying to indicator 1 or indicator 2
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf<Guid>(),
                    indicatorIds: ListOf(indicatorGroup1.Indicators[0].Id,
                        indicatorGroup1.Indicators[1].Id)
                );

                // Check that only footnotes applying to indicator 1 are returned.
                // No footnotes apply to indicator 2 but including its id in the parameter shouldn't alter the result.
                // The footnote which applies only to indicator 3 should be excluded.
                Assert.Equal(2, results.Count);

                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to indicator 1", results[0].Content);

                Assert.Equal(releaseFootnotes[2].FootnoteId, results[1].Id);
                Assert.Equal("Applies to indicator 1 and indicator 3", results[1].Content);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_FootnotesApplyToMultipleSubjects()
        {
            var subject1 = new Subject();
            var subject2 = new Subject();

            var subject1Filter1 = new Filter
            {
                Label = "Subject 1 filter 1",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
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

            var subject2Filter1 = new Filter
            {
                Label = "Subject 2 filter 1",
                Subject = subject2,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
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

            var subject2Filter2 = new Filter
            {
                Label = "Subject 2 filter 2",
                Subject = subject2,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 2 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 2 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject2Filter3 = new Filter
            {
                Label = "Subject 2 filter 3",
                Subject = subject2,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 3 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 3 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject1IndicatorGroup1 = new IndicatorGroup
            {
                Label = "Indicator group 1",
                Subject = subject1,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Label = "Indicator 1"
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

            // Set up a release with footnotes that apply to both subject 1 and subject 2
            var release = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        // Footnote applies to subject 1 and subject 2
                        Footnote = new Footnote
                        {
                            Content = "Footnote 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject1
                                },
                                new()
                                {
                                    Subject = subject2
                                }
                            }
                        }
                    },
                    new()
                    {
                        // Footnote applies to subject 1 filter 1
                        // and subject 2 filter 1
                        // and subject 2 filter 2 group 1
                        // and subject 2 filter 3 group 1 item 1
                        Footnote = new Footnote
                        {
                            Content = "Footnote 2",
                            Order = 1,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject1Filter1
                                },
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
                                    FilterItem = subject2Filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        // Footnote applies to subject 1 indicator 1
                        // and subject 2 filter 1
                        Footnote = new Footnote
                        {
                            Content = "Footnote 3",
                            Order = 2,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject2Filter1
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject1IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        // Footnote applies to subject 1 filter  1
                        // and subject 2 indicator 1
                        Footnote = new Footnote
                        {
                            Content = "Footnote 4",
                            Order = 3,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject1Filter1
                                }
                            },
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject2IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        // Footnote applies to subject 1 indicator  1
                        // and subject 2 indicator 1
                        Footnote = new Footnote
                        {
                            Content = "Footnote 5",
                            Order = 4,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject1IndicatorGroup1.Indicators[0]
                                },
                                new()
                                {
                                    Indicator = subject2IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    }
                }
            };

            var releaseFootnotes = release.Footnotes.ToList();

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

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filter1, subject2Filter1, subject2Filter2, subject2Filter3);
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup1, subject2IndicatorGroup1);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                // This test makes sure that applying footnotes to more than one subject doesn't exclude them from the
                // result.
                
                // Get the footnotes applying to subject 1 or any of its filter items or indicators
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(subject1Filter1.FilterGroups[0].FilterItems[0].Id),
                    indicatorIds: ListOf(subject1IndicatorGroup1.Indicators[0].Id));

                // Check that all of the footnotes are returned even though they have also been applied to subject 2
                Assert.Equal(5, results.Count);

                // Footnote applies to the requested subject as well as another subject
                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Footnote 1", results[0].Content);

                // Footnote applies to a requested filter item via its filter as well as another subject's filter,
                // filter group, and filter item
                Assert.Equal(releaseFootnotes[1].FootnoteId, results[1].Id);
                Assert.Equal("Footnote 2", results[1].Content);

                // Footnote applies to a requested indicator as well as another subject's filter
                Assert.Equal(releaseFootnotes[2].FootnoteId, results[2].Id);
                Assert.Equal("Footnote 3", results[2].Content);

                // Footnote applies to a requested filter via its filter item as well as another subject's indicator
                Assert.Equal(releaseFootnotes[3].FootnoteId, results[3].Id);
                Assert.Equal("Footnote 4", results[3].Content);

                // Footnote applies to a requested indicator as well as another subject's indicator
                Assert.Equal(releaseFootnotes[4].FootnoteId, results[4].Id);
                Assert.Equal("Footnote 5", results[4].Content);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_FilterByEmptyListOfFiltersAndIndicators()
        {
            var subject1 = new Subject();
            var subject2 = new Subject();

            var subject1Filter1 = new Filter
            {
                Label = "Subject 1 filter 1",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
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

            var subject1Filter2 = new Filter
            {
                Label = "Subject 1 filter 2",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 2 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 2 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject1Filter3 = new Filter
            {
                Label = "Subject 1 filter 3",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 3 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 3 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject1IndicatorGroup1 = new IndicatorGroup
            {
                Label = "Subject 1 indicator group 1",
                Subject = subject1,
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
                            Content = "Applies to subject 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 1",
                            Order = 1,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject1Filter1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 2 group 1",
                            Order = 2,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = subject1Filter2.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 3 group 1 item 1",
                            Order = 3,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = subject1Filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 indicator 1",
                            Order = 4,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject1IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2",
                            Order = 5,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject2
                                }
                            }
                        }
                    }
                }
            };

            var releaseFootnotes = release.Footnotes.ToList();

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

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filter1, subject1Filter2, subject1Filter3);
                await context.IndicatorGroup.AddAsync(subject1IndicatorGroup1);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                //  Get footnotes applying directly to subject 1 using empty lists of filter item and indicator id's
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject1.Id,
                    filterItemIds: ListOf<Guid>(),
                    indicatorIds: ListOf<Guid>());

                // Check that only the footnotes which apply directly to subject 1 are returned
                // Other footnotes related to subject 1 should be ignored as no filter items or indicators were requested
                Assert.Single(results);

                Assert.Equal(releaseFootnotes[0].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 1", results[0].Content);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_IgnoresFootnotesUnrelatedToRelease()
        {
            var subject1 = new Subject();

            var filter1 = new Filter
            {
                Label = "Filter 1",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
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

            var filter2 = new Filter
            {
                Label = "Filter 2",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 2 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 2 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var filter3 = new Filter
            {
                Label = "Filter 3",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 3 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 3 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var indicatorGroup1 = new IndicatorGroup
            {
                Label = "Indicator group 1",
                Subject = subject1,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Label = "Indicator 1"
                    }
                }
            };

            var release1 = new Release
            {
                Footnotes = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 1",
                            Order = 1,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = filter1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 2 group 1",
                            Order = 2,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = filter2.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 3 group 1 item 1",
                            Order = 3,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 indicator 1",
                            Order = 4,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = indicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    }
                }
            };

            // Create another release which has no footnotes
            var release2 = new Release();

            var releaseSubjects = new List<ReleaseSubject>
            {
                new()
                {
                    Release = release1,
                    Subject = subject1
                },
                new()
                {
                    Release = release2,
                    Subject = subject1
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddAsync(subject1);
                await context.Filter.AddRangeAsync(filter1, filter2, filter3);
                await context.IndicatorGroup.AddAsync(indicatorGroup1);
                await context.Release.AddRangeAsync(release1, release2);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                // This test covers a case where a subject is shared by multiple releases. It makes sure that when other
                // releases have footnotes for the subject, that there's no way of retrieving those footnotes that
                // belong to other releases by specifying the subject id and filter item and indicator id's of the subject.

                var filter1Group1Item1Id = filter1.FilterGroups[0].FilterItems[0].Id;
                var filter2Group1Item1Id = filter2.FilterGroups[0].FilterItems[0].Id;
                var filter3Group1Item1Id = filter3.FilterGroups[0].FilterItems[0].Id;
                var indicatorGroup1Item1Id = indicatorGroup1.Indicators[0].Id;

                var results = await repository.GetFilteredFootnotes(
                    releaseId: release2.Id, // release 2 has no footnotes
                    subjectId: subject1.Id,
                    filterItemIds: ListOf(filter1Group1Item1Id, filter2Group1Item1Id, filter3Group1Item1Id),
                    indicatorIds: ListOf(indicatorGroup1Item1Id));

                // Check that no footnotes are returned even though subject 1 has footnotes for a different release
                Assert.Empty(results);
            }
        }

        [Fact]
        public async Task GetFilteredFootnotes_IgnoresRequestedFilterItemsAndIndicatorsUnrelatedToSubject()
        {
            var subject1 = new Subject();
            var subject2 = new Subject();

            var subject1Filter1 = new Filter
            {
                Label = "Subject 1 filter 1",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 1 group 1",
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

            var subject1Filter2 = new Filter
            {
                Label = "Subject 1 filter 2",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 2 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 2 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject1Filter3 = new Filter
            {
                Label = "Subject 1 filter 3",
                Subject = subject1,
                FilterGroups = new List<FilterGroup>
                {
                    new()
                    {
                        Label = "Filter 3 group 1",
                        FilterItems = new List<FilterItem>
                        {
                            new()
                            {
                                Label = "Filter 3 group 1 item 1"
                            }
                        }
                    }
                }
            };

            var subject1IndicatorGroup1 = new IndicatorGroup
            {
                Label = "Subject 1 indicator group 1",
                Subject = subject1,
                Indicators = new List<Indicator>
                {
                    new()
                    {
                        Label = "Indicator 1"
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
                            Content = "Applies to subject 1",
                            Order = 0,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 1",
                            Order = 1,
                            Filters = new List<FilterFootnote>
                            {
                                new()
                                {
                                    Filter = subject1Filter1
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 2 group 1",
                            Order = 2,
                            FilterGroups = new List<FilterGroupFootnote>
                            {
                                new()
                                {
                                    FilterGroup = subject1Filter2.FilterGroups[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 filter 3 group 1 item 1",
                            Order = 3,
                            FilterItems = new List<FilterItemFootnote>
                            {
                                new()
                                {
                                    FilterItem = subject1Filter3.FilterGroups[0].FilterItems[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 1 indicator 1",
                            Order = 4,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject1IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2",
                            Order = 5,
                            Subjects = new List<SubjectFootnote>
                            {
                                new()
                                {
                                    Subject = subject2
                                }
                            }
                        }
                    },
                    new()
                    {
                        Footnote = new Footnote
                        {
                            Content = "Applies to subject 2 indicator 1",
                            Order = 6,
                            Indicators = new List<IndicatorFootnote>
                            {
                                new()
                                {
                                    Indicator = subject2IndicatorGroup1.Indicators[0]
                                }
                            }
                        }
                    }
                }
            };

            var releaseFootnotes = release.Footnotes.ToList();

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

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.Subject.AddRangeAsync(subject1, subject2);
                await context.Filter.AddRangeAsync(subject1Filter1, subject1Filter2, subject1Filter3);
                await context.IndicatorGroup.AddRangeAsync(subject1IndicatorGroup1, subject2IndicatorGroup1);
                await context.Release.AddAsync(release);
                await context.ReleaseSubject.AddRangeAsync(releaseSubjects);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);

                // Get footnotes applying to subject 2
                // but also include filter item and indicator id's from subject 1
                var results = await repository.GetFilteredFootnotes(
                    releaseId: release.Id,
                    subjectId: subject2.Id,
                    filterItemIds: ListOf(
                        subject1Filter1.FilterGroups[0].FilterItems[0].Id, // subject 1
                        subject1Filter2.FilterGroups[0].FilterItems[0].Id, // subject 1
                        subject1Filter3.FilterGroups[0].FilterItems[0].Id // subject 1
                    ),
                    indicatorIds: ListOf(
                        subject1IndicatorGroup1.Indicators[0].Id, // subject 1
                        subject2IndicatorGroup1.Indicators[0].Id // subject 2
                    ));

                // Check that only the footnotes which apply to subject 2 are returned
                // The filter item and indicator id's related to subject 1 should have been ignored
                // Footnotes applying to subject 1 should be excluded
                Assert.Equal(2, results.Count);

                Assert.Equal(releaseFootnotes[5].FootnoteId, results[0].Id);
                Assert.Equal("Applies to subject 2", results[0].Content);

                Assert.Equal(releaseFootnotes[6].FootnoteId, results[1].Id);
                Assert.Equal("Applies to subject 2 indicator 1", results[1].Content);
            }
        }

        [Fact]
        public async Task GetFootnotes_MapsAllCriteria()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var footnote = new Footnote
            {
                Content = "Test footnote",
                Order = 0,
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
                        Subject = releaseSubject1.Subject
                    },
                    new()
                    {
                        Subject = releaseSubject2.Subject
                    },
                },
                Filters = new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject1.Subject
                        }
                    },
                    new()
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject2.Subject
                        }
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = new FilterGroup
                        {
                            Filter = new Filter
                            {
                                Subject = releaseSubject1.Subject
                            }
                        }
                    },
                    new()
                    {
                        FilterGroup = new FilterGroup
                        {
                            Filter = new Filter
                            {
                                Subject = releaseSubject2.Subject
                            }
                        }
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = new FilterItem
                        {
                            FilterGroup = new FilterGroup
                            {
                                Filter = new Filter
                                {
                                    Subject = releaseSubject1.Subject,
                                }
                            }
                        }
                    },
                    new()
                    {
                        FilterItem = new FilterItem
                        {
                            FilterGroup = new FilterGroup
                            {
                                Filter = new Filter
                                {
                                    Subject = releaseSubject2.Subject,
                                }
                            }
                        }
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = new Indicator
                        {
                            IndicatorGroup = new IndicatorGroup
                            {
                                Subject = releaseSubject1.Subject
                            }
                        }
                    },
                    new()
                    {
                        Indicator = new Indicator
                        {
                            IndicatorGroup = new IndicatorGroup
                            {
                                Subject = releaseSubject2.Subject
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(releaseSubject1, releaseSubject2);
                await context.AddAsync(footnote);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release.Id);

                Assert.Single(results);

                Assert.Equal("Test footnote", results[0].Content);

                var footnoteReleases = results[0].Releases.ToList();

                Assert.Single(footnoteReleases);
                Assert.Equal(release.Id, footnoteReleases[0].ReleaseId);

                var footnoteSubjects = results[0].Subjects.ToList();

                Assert.Equal(2, footnoteSubjects.Count);
                Assert.Equal(releaseSubject1.SubjectId, footnoteSubjects[0].SubjectId);
                Assert.Equal(releaseSubject2.SubjectId, footnoteSubjects[1].SubjectId);

                var footnoteFilters = results[0].Filters.ToList();

                Assert.Equal(2, footnoteSubjects.Count);
                Assert.Equal(releaseSubject1.SubjectId, footnoteFilters[0].Filter.SubjectId);
                Assert.Equal(releaseSubject2.SubjectId, footnoteFilters[1].Filter.SubjectId);

                var footnoteFilterGroups = results[0].FilterGroups.ToList();

                Assert.Equal(2, footnoteFilterGroups.Count);
                Assert.Equal(releaseSubject1.SubjectId, footnoteFilterGroups[0].FilterGroup.Filter.SubjectId);
                Assert.Equal(releaseSubject2.SubjectId, footnoteFilterGroups[1].FilterGroup.Filter.SubjectId);

                var footnoteFilterItems = results[0].FilterItems.ToList();

                Assert.Equal(2, footnoteFilterItems.Count);
                Assert.Equal(releaseSubject1.SubjectId, footnoteFilterItems[0].FilterItem.FilterGroup.Filter.SubjectId);
                Assert.Equal(releaseSubject2.SubjectId, footnoteFilterItems[1].FilterItem.FilterGroup.Filter.SubjectId);

                var footnoteIndicators = results[0].Indicators.ToList();

                Assert.Equal(2, footnoteIndicators.Count);
                Assert.Equal(releaseSubject1.SubjectId, footnoteIndicators[0].Indicator.IndicatorGroup.SubjectId);
                Assert.Equal(releaseSubject2.SubjectId, footnoteIndicators[1].Indicator.IndicatorGroup.SubjectId);
            }
        }

        [Fact]
        public async Task GetFootnotes_FiltersByRelease()
        {
            var release = new Release();
            var otherRelease = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var footnote1 = new Footnote
            {
                Content = "Test footnote 1",
                Order = 0,
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Release = release
                    },
                    // Check that footnote is still fetched
                    // even if it also linked to another release
                    new()
                    {
                        Release = otherRelease
                    }
                },
                Subjects = new List<SubjectFootnote>
                {
                    new()
                    {
                        Subject = releaseSubject1.Subject
                    }
                },
            };

            var footnote2 = new Footnote
            {
                Content = "Test footnote 2",
                Order = 1,
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
                        Subject = releaseSubject2.Subject
                    }
                },
            };

            var footnoteForOtherRelease = new Footnote
            {
                Content = "Test footnote for other release",
                Order = 0,
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Release = new Release()
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new()
                    {
                        Subject = new Subject()
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(releaseSubject1, releaseSubject2);
                await context.AddRangeAsync(footnote1, footnote2, footnoteForOtherRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release.Id);

                Assert.Equal(2, results.Count);

                Assert.Equal("Test footnote 1", results[0].Content);

                var footnote1Releases = results[0].Releases.ToList();

                Assert.Single(footnote1Releases);
                Assert.Equal(release.Id, footnote1Releases[0].ReleaseId);

                var footnote1Subjects = results[0].Subjects.ToList();

                Assert.Single(footnote1Subjects);
                Assert.Equal(releaseSubject1.SubjectId, footnote1Subjects[0].SubjectId);

                Assert.Equal("Test footnote 2", results[1].Content);

                var footnote2Releases = results[1].Releases.ToList();

                Assert.Single(footnote2Releases);
                Assert.Equal(release.Id, footnote2Releases[0].ReleaseId);

                var footnote2Subjects = results[1].Subjects.ToList();
                Assert.Single(footnote2Subjects);
                Assert.Equal(releaseSubject2.SubjectId, footnote2Subjects[0].SubjectId);
            }
        }

        [Fact]
        public async Task GetFootnotes_FiltersBySubjectId()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var footnote1 = new Footnote
            {
                Content = "Test footnote 1",
                Order = 0,
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
                        Subject = releaseSubject1.Subject
                    }
                },
            };

            var footnote2 = new Footnote
            {
                Content = "Test footnote 2",
                Order = 1,
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
                        Subject = releaseSubject2.Subject
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(releaseSubject1, releaseSubject2);
                await context.AddRangeAsync(footnote1, footnote2);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release.Id, releaseSubject2.SubjectId);

                Assert.Single(results);
                Assert.Equal("Test footnote 2", results[0].Content);
            }
        }

        [Fact]
        public async Task GetFootnotes_FiltersCriteriaBySubjectId()
        {
            var release = new Release();
            var otherRelease = new Release();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var otherReleaseSubject = new ReleaseSubject
            {
                Release = otherRelease,
                Subject = new Subject()
            };

            var footnote = new Footnote
            {
                Content = "Test footnote",
                Order = 0,
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Release = release
                    },
                    new()
                    {
                        Release = otherRelease
                    }
                },
                Subjects = new List<SubjectFootnote>
                {
                    new()
                    {
                        Subject = releaseSubject.Subject
                    },
                    new()
                    {
                        Subject = otherReleaseSubject.Subject
                    },
                },
                Filters = new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject.Subject
                        }
                    },
                    new()
                    {
                        Filter = new Filter
                        {
                            Subject = otherReleaseSubject.Subject
                        }
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = new FilterGroup
                        {
                            Filter = new Filter
                            {
                                Subject = releaseSubject.Subject
                            }
                        }
                    },
                    new()
                    {
                        FilterGroup = new FilterGroup
                        {
                            Filter = new Filter
                            {
                                Subject = otherReleaseSubject.Subject
                            }
                        }
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = new FilterItem
                        {
                            FilterGroup = new FilterGroup
                            {
                                Filter = new Filter
                                {
                                    Subject = releaseSubject.Subject
                                }
                            }
                        }
                    },
                    new()
                    {
                        FilterItem = new FilterItem
                        {
                            FilterGroup = new FilterGroup
                            {
                                Filter = new Filter
                                {
                                    Subject = otherReleaseSubject.Subject,
                                }
                            }
                        }
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = new Indicator
                        {
                            IndicatorGroup = new IndicatorGroup
                            {
                                Subject = releaseSubject.Subject
                            }
                        }
                    },
                    new()
                    {
                        Indicator = new Indicator
                        {
                            IndicatorGroup = new IndicatorGroup
                            {
                                Subject = otherReleaseSubject.Subject
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(release, otherRelease);
                await context.AddRangeAsync(releaseSubject, otherReleaseSubject);
                await context.AddAsync(footnote);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release.Id, releaseSubject.SubjectId);

                Assert.Single(results);
                Assert.Equal("Test footnote", results[0].Content);

                var footnoteReleases = results[0].Releases.ToList();

                Assert.Single(footnoteReleases);
                Assert.Equal(releaseSubject.ReleaseId, footnoteReleases[0].ReleaseId);

                var footnoteSubjects = results[0].Subjects.ToList();

                Assert.Single(footnoteSubjects);
                Assert.Equal(releaseSubject.SubjectId, footnoteSubjects[0].SubjectId);

                var footnoteFilters = results[0].Filters.ToList();

                Assert.Single(footnoteSubjects);
                Assert.Equal(releaseSubject.SubjectId, footnoteFilters[0].Filter.SubjectId);

                var footnoteFilterGroups = results[0].FilterGroups.ToList();

                Assert.Single(footnoteFilterGroups);
                Assert.Equal(releaseSubject.SubjectId, footnoteFilterGroups[0].FilterGroup.Filter.SubjectId);

                var footnoteFilterItems = results[0].FilterItems.ToList();

                Assert.Single(footnoteFilterItems);
                Assert.Equal(releaseSubject.SubjectId, footnoteFilterItems[0].FilterItem.FilterGroup.Filter.SubjectId);

                var footnoteIndicators = results[0].Indicators.ToList();

                Assert.Single(footnoteIndicators);
                Assert.Equal(releaseSubject.SubjectId, footnoteIndicators[0].Indicator.IndicatorGroup.SubjectId);
            }
        }

        [Fact]
        public async Task GetFootnotes_OrdersFootnotes()
        {
            var release = new Release();

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var footnotes = new List<Footnote>
            {
                new()
                {
                    Content = "Footnote 3",
                    Releases = new List<ReleaseFootnote>
                    {
                        new()
                        {
                            Release = release
                        }
                    },
                    Subjects = new List<SubjectFootnote>
                    {
                        new()
                        {
                            Subject = releaseSubject.Subject
                        }
                    },
                    Order = 2
                },
                new()
                {
                    Content = "Footnote 1",
                    Releases = new List<ReleaseFootnote>
                    {
                        new()
                        {
                            Release = release
                        }
                    },
                    Subjects = new List<SubjectFootnote>
                    {
                        new()
                        {
                            Subject = releaseSubject.Subject
                        }
                    },
                    Order = 0
                },
                new()
                {
                    Content = "Footnote 2",
                    Releases = new List<ReleaseFootnote>
                    {
                        new()
                        {
                            Release = release
                        }
                    },
                    Subjects = new List<SubjectFootnote>
                    {
                        new()
                        {
                            Subject = releaseSubject.Subject
                        }
                    },
                    Order = 1
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.AddRangeAsync(releaseSubject);
                await context.AddRangeAsync(footnotes);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetFootnotes(release.Id);

                Assert.Equal(3, results.Count);

                Assert.Equal("Footnote 1", results[0].Content);
                Assert.Equal(0, results[0].Order);

                Assert.Equal("Footnote 2", results[1].Content);
                Assert.Equal(1, results[1].Order);

                Assert.Equal("Footnote 3", results[2].Content);
                Assert.Equal(2, results[2].Order);
            }
        }

        [Fact]
        public async Task GetSubjectsWithNoFootnotes_FootnotePerSubject()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject4 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject5 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubjectWithNoFootnotes = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
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
                        Subject = releaseSubject1.Subject,
                    }
                }
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
                Filters = new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject2.Subject
                        }
                    }
                }
            };
            var footnote3 = new Footnote
            {
                Content = "Test footnote 3",
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Release = release
                    },
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = new FilterGroup
                        {
                            Filter = new Filter
                            {
                                Subject = releaseSubject3.Subject
                            }
                        }
                    }
                }
            };
            var footnote4 = new Footnote
            {
                Content = "Test footnote 4",
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Release = release
                    },
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = new FilterItem
                        {
                            FilterGroup = new FilterGroup
                            {
                                Filter = new Filter
                                {
                                    Subject = releaseSubject4.Subject
                                }
                            }
                        }
                    },
                }
            };
            var footnote5 = new Footnote
            {
                Content = "Test footnote 5",
                Releases = new List<ReleaseFootnote>
                {
                    new()
                    {
                        Release = release
                    },
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = new Indicator
                        {
                            IndicatorGroup = new IndicatorGroup
                            {
                                Subject = releaseSubject5.Subject
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.AddRangeAsync(
                    releaseSubject1,
                    releaseSubject2,
                    releaseSubject3,
                    releaseSubject4,
                    releaseSubject5,
                    releaseSubjectWithNoFootnotes
                );
                await context.AddRangeAsync(footnote1, footnote2, footnote3, footnote4, footnote5);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetSubjectsWithNoFootnotes(release.Id);

                Assert.Single(results);

                Assert.Equal(releaseSubjectWithNoFootnotes.Subject.Id, results[0].Id);
            }
        }

        [Fact]
        public async Task GetSubjectsWithNoFootnotes_FootnoteForMultipleSubjects()
        {
            var release = new Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject2 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject3 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject4 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubject5 = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };
            var releaseSubjectWithNoFootnotes = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject()
            };

            var footnote = new Footnote
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
                        Subject = releaseSubject1.Subject,
                    }
                },
                Filters = new List<FilterFootnote>
                {
                    new()
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject2.Subject
                        },
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new()
                    {
                        FilterGroup = new FilterGroup
                        {
                            Filter = new Filter
                            {
                                Subject = releaseSubject3.Subject
                            },
                        }
                    }
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new()
                    {
                        FilterItem = new FilterItem
                        {
                            FilterGroup = new FilterGroup
                            {
                                Filter = new Filter
                                {
                                    Subject = releaseSubject4.Subject
                                },
                            }
                        }
                    }
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new()
                    {
                        Indicator = new Indicator
                        {
                            IndicatorGroup = new IndicatorGroup
                            {
                                Subject = releaseSubject5.Subject
                            }
                        }
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(release);
                await context.AddRangeAsync(
                    releaseSubject1,
                    releaseSubject2,
                    releaseSubject3,
                    releaseSubject4,
                    releaseSubject5,
                    releaseSubjectWithNoFootnotes
                );
                await context.AddRangeAsync(footnote);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = await repository.GetSubjectsWithNoFootnotes(release.Id);

                Assert.Single(results);

                Assert.Equal(releaseSubjectWithNoFootnotes.Subject.Id, results[0].Id);
            }
        }

        private static FootnoteRepository BuildFootnoteRepository(StatisticsDbContext context)
        {
            return new FootnoteRepository(context);
        }
    }
}
