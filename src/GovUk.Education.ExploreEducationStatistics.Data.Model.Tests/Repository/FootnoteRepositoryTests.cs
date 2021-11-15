using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Repository
{
    public class FootnoteRepositoryTests
    {
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
                        Subject = releaseSubject1.Subject
                    },
                    new SubjectFootnote
                    {
                        Subject = releaseSubject2.Subject
                    },
                },
                Filters = new List<FilterFootnote>
                {
                    new FilterFootnote
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject1.Subject
                        }
                    },
                    new FilterFootnote
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject2.Subject
                        }
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroup = new FilterGroup
                        {
                            Filter = new Filter
                            {
                                Subject = releaseSubject1.Subject
                            }
                        }
                    },
                    new FilterGroupFootnote
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
                    new FilterItemFootnote
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
                    new FilterItemFootnote
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
                    new IndicatorFootnote
                    {
                        Indicator = new Indicator
                        {
                            IndicatorGroup = new IndicatorGroup
                            {
                                Subject = releaseSubject1.Subject
                            }
                        }
                    },
                    new IndicatorFootnote
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(releaseSubject1, releaseSubject2);
                await context.AddAsync(footnote);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = repository.GetFootnotes(release.Id).ToList();

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
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                    // Check that footnote is still fetched
                    // even if it also linked to another release
                    new ReleaseFootnote
                    {
                        Release = otherRelease
                    }
                },
                Subjects = new List<SubjectFootnote>
                {
                    new SubjectFootnote
                    {
                        Subject = releaseSubject1.Subject
                    }
                },
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
                        Subject = releaseSubject2.Subject
                    }
                },
            };

            var footnoteForOtherRelease = new Footnote
            {
                Content = "Test footnote for other release",
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = new Release()
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new SubjectFootnote
                    {
                        Subject = new Subject()
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(releaseSubject1, releaseSubject2);
                await context.AddRangeAsync(footnote1, footnote2, footnoteForOtherRelease);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = repository.GetFootnotes(release.Id).ToList();

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
                        Subject = releaseSubject1.Subject
                    }
                },
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
                        Subject = releaseSubject2.Subject
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(releaseSubject1, releaseSubject2);
                await context.AddRangeAsync(footnote1, footnote2);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = repository.GetFootnotes(release.Id, releaseSubject2.SubjectId).ToList();

                Assert.Single(results);
                Assert.Equal("Test footnote 2", results[0].Content);
            }
        }

        [Fact]
        public async Task GetFootnotes_FiltersBySubjectIds()
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
                        Subject = releaseSubject1.Subject
                    }
                },
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
                        Subject = releaseSubject2.Subject
                    }
                },
            };

            var footnote3 = new Footnote
            {
                Content = "Test footnote 3",
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
                        Subject = releaseSubject3.Subject
                    }
                },
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(release);
                await context.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject3);
                await context.AddRangeAsync(footnote1, footnote2, footnote3);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = repository.GetFootnotes(
                        release.Id,
                        new List<Guid> {releaseSubject1.SubjectId, releaseSubject3.SubjectId}
                    )
                    .ToList();

                Assert.Equal(2, results.Count);

                Assert.Equal("Test footnote 1", results[0].Content);
                Assert.Equal("Test footnote 3", results[1].Content);
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
                Releases = new List<ReleaseFootnote>
                {
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                    new ReleaseFootnote
                    {
                        Release = otherRelease
                    }
                },
                Subjects = new List<SubjectFootnote>
                {
                    new SubjectFootnote
                    {
                        Subject = releaseSubject.Subject
                    },
                    new SubjectFootnote
                    {
                        Subject = otherReleaseSubject.Subject
                    },
                },
                Filters = new List<FilterFootnote>
                {
                    new FilterFootnote
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject.Subject
                        }
                    },
                    new FilterFootnote
                    {
                        Filter = new Filter
                        {
                            Subject = otherReleaseSubject.Subject
                        }
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
                    {
                        FilterGroup = new FilterGroup
                        {
                            Filter = new Filter
                            {
                                Subject = releaseSubject.Subject
                            }
                        }
                    },
                    new FilterGroupFootnote
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
                    new FilterItemFootnote
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
                    new FilterItemFootnote
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
                    new IndicatorFootnote
                    {
                        Indicator = new Indicator
                        {
                            IndicatorGroup = new IndicatorGroup
                            {
                                Subject = releaseSubject.Subject
                            }
                        }
                    },
                    new IndicatorFootnote
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await context.AddRangeAsync(release, otherRelease);
                await context.AddRangeAsync(releaseSubject, otherReleaseSubject);
                await context.AddAsync(footnote);
                await context.SaveChangesAsync();
            }

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var repository = BuildFootnoteRepository(context);
                var results = repository.GetFootnotes(release.Id, releaseSubject.SubjectId).ToList();

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
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new SubjectFootnote
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
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                },
                Filters = new List<FilterFootnote>
                {
                    new FilterFootnote
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
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
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
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                },
                FilterItems = new List<FilterItemFootnote>
                {
                    new FilterItemFootnote
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
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                },
                Indicators = new List<IndicatorFootnote>
                {
                    new IndicatorFootnote
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
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
                    new ReleaseFootnote
                    {
                        Release = release
                    },
                },
                Subjects = new List<SubjectFootnote>
                {
                    new SubjectFootnote
                    {
                        Subject = releaseSubject1.Subject,
                    }
                },
                Filters = new List<FilterFootnote>
                {
                    new FilterFootnote
                    {
                        Filter = new Filter
                        {
                            Subject = releaseSubject2.Subject
                        },
                    }
                },
                FilterGroups = new List<FilterGroupFootnote>
                {
                    new FilterGroupFootnote
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
                    new FilterItemFootnote
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
                    new IndicatorFootnote
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
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

            await using (var context = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
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
