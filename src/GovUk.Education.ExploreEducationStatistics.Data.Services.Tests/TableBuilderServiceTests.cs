#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Options;
using Moq;
using Snapshooter.Xunit;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TableBuilderServiceTests
    {
        private readonly DataFixture _fixture = new();

        [Fact]
        public async Task Query_LatestRelease()
        {
            var releaseId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            var publication = new Publication
            {
                Id = publicationId,
                LatestPublishedRelease = new Content.Model.Release
                {
                    Id = releaseId,
                }
            };
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Id = releaseId,
                    PublicationId = publicationId
                },
                Subject = new Subject()
            };

            var indicator1Id = Guid.NewGuid();
            var indicator2Id = Guid.NewGuid();
            var location1Id = Guid.NewGuid();
            var location2Id = Guid.NewGuid();
            var location3Id = Guid.NewGuid();

            var observations = new List<Observation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Location = new Location
                    {
                        Id = location1Id,
                        GeographicLevel = GeographicLevel.Country
                    },
                    Measures = new Dictionary<Guid, string>
                    {
                        { indicator1Id, "123" },
                        { indicator2Id, "456" },
                    },
                    FilterItems = ListOf(new ObservationFilterItem
                    {
                        FilterItem = new FilterItem("Filter Item 1", Guid.NewGuid())
                    }),
                    Year = 2019,
                    TimeIdentifier = AcademicYear,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Location = new Location
                    {
                        Id = location2Id,
                        GeographicLevel = GeographicLevel.Institution
                    },
                    Measures = new Dictionary<Guid, string>
                    {
                        { indicator1Id, "678" },
                    },
                    FilterItems = ListOf(new ObservationFilterItem
                    {
                        FilterItem = new FilterItem("Filter Item 2", Guid.NewGuid())
                    }),
                    Year = 2020,
                    TimeIdentifier = AcademicYear,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Location = new Location
                    {
                        Id = location3Id,
                        GeographicLevel = GeographicLevel.Provider
                    },
                    Measures = new Dictionary<Guid, string>
                    {
                        { indicator1Id, "789" },
                        { Guid.NewGuid(), "1123" },
                        { Guid.NewGuid(), "1456" },
                    },
                    FilterItems = ListOf(new ObservationFilterItem
                    {
                        FilterItem = new FilterItem("Filter Item 3", Guid.NewGuid())
                    }),
                    Year = 2020,
                    TimeIdentifier = AcademicYear,
                }
            };

            var subjectMeta = new SubjectResultMetaViewModel
            {
                Indicators = new List<IndicatorMetaViewModel>
                {
                    new()
                    {
                        Label = "Test indicator"
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.MatchedObservations.AddRangeAsync(
                    new MatchedObservation(observations[0].Id),
                    new MatchedObservation(observations[2].Id)
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Indicators = new[] { indicator1Id, indicator2Id },
                    LocationIds = ListOf(location1Id, location2Id, location3Id),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2019,
                        StartCode = AcademicYear,
                        EndYear = 2020,
                        EndCode = AcademicYear,
                    }
                };

                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(statisticsDbContext.MatchedObservations);

                var subjectResultMetaService = new Mock<ISubjectResultMetaService>(Strict);

                subjectResultMetaService
                    .Setup(
                        s => s.GetSubjectMeta(
                            releaseId,
                            query,
                            It.IsAny<IList<Observation>>()
                        )
                    )
                    .ReturnsAsync(subjectMeta);

                var service = BuildTableBuilderService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext,
                    observationService: observationService.Object,
                    subjectResultMetaService: subjectResultMetaService.Object
                );

                var result = await service.Query(query);

                VerifyAllMocks(
                    observationService,
                    subjectResultMetaService
                );

                var observationResults = result.AssertRight().Results.ToList();

                Assert.Equal(2, observationResults.Count);

                Assert.Equal(observations[0].Id, observationResults[0].Id);
                Assert.Equal(GeographicLevel.Country, observationResults[0].GeographicLevel);
                Assert.Equal(location1Id, observationResults[0].LocationId);
                Assert.Equal("2019_AY", observationResults[0].TimePeriod);
                Assert.Equal(2, observationResults[0].Measures.Count);
                Assert.Equal("123", observationResults[0].Measures[indicator1Id]);
                Assert.Equal("456", observationResults[0].Measures[indicator2Id]);
                Assert.Equal(
                    ListOf(observations[0].FilterItems.ToList()[0].FilterItemId),
                    observationResults[0].Filters
                );

                Assert.Equal(observations[2].Id, observationResults[1].Id);
                Assert.Equal(GeographicLevel.Provider, observationResults[1].GeographicLevel);
                Assert.Equal(location3Id, observationResults[1].LocationId);
                Assert.Equal("2020_AY", observationResults[1].TimePeriod);
                Assert.Single(observationResults[1].Measures);
                Assert.Equal("789", observationResults[1].Measures[indicator1Id]);
                Assert.Equal(
                    ListOf(observations[2].FilterItems.ToList()[0].FilterItemId),
                    observationResults[1].Filters
                );

                Assert.Equal(subjectMeta, result.Right.SubjectMeta);
            }
        }

        [Fact]
        public async Task Query_LatestRelease_SubjectNotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext, contentDbContext: contentDbContext);

                var query = new ObservationQueryContext
                {
                    // Does not match the saved release subject
                    SubjectId = Guid.NewGuid(),
                };

                var result = await service.Query(query);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Query_LatestRelease_PublicationNotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    PublicationId = Guid.NewGuid(),
                },
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext, contentDbContext: contentDbContext);

                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.SubjectId,
                };

                var result = await service.Query(query);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Query_LatestRelease_ReleaseNotFound()
        {
            var publicationId = Guid.NewGuid();
            var publication = new Publication
            {
                Id = publicationId
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    PublicationId = publicationId,
                },
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext, contentDbContext: contentDbContext);

                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.SubjectId,
                };

                var result = await service.Query(query);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Query_LatestRelease_PredictedTableTooBig()
        {
            var releaseId = Guid.NewGuid();
            var publicationId = Guid.NewGuid();

            var publication = new Publication
            {
                Id = publicationId,
                LatestPublishedRelease = new Content.Model.Release
                {
                    Id = releaseId,
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Id = releaseId,
                    PublicationId = publicationId
                },
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Filters = new[] { Guid.NewGuid(), Guid.NewGuid() },
                    Indicators = new[] { Guid.NewGuid(), Guid.NewGuid() },
                    LocationIds = ListOf(Guid.NewGuid()),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2019,
                        StartCode = AcademicYear,
                        EndYear = 2020,
                        EndCode = AcademicYear
                    }
                };

                var filterItemRepository = new Mock<IFilterItemRepository>(Strict);

                filterItemRepository
                    .Setup(s => s.CountFilterItemsByFilter(query.Filters))
                    .ReturnsAsync(new Dictionary<Guid, int>
                    {
                        {
                            // For the purpose of calculating the potential table size,
                            // treat all the Filter Items as belonging to the same Filter
                            Guid.NewGuid(), query.Filters.Count()
                        }
                    });

                var options = Options.Create(new TableBuilderOptions
                {
                    // 2 Filter items (from 1 Filter), 1 Location, and 2 Time periods provide 4 different combinations,
                    // assuming that all the data is provided. For 2 Indicators this would be 8 table cells rendered.
                    // Configure a maximum table size limit lower than 8.
                    MaxTableCellsAllowed = 7
                });

                var service = BuildTableBuilderService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext,
                    filterItemRepository: filterItemRepository.Object,
                    options: options
                );

                var result = await service.Query(query);

                VerifyAllMocks(filterItemRepository);

                result.AssertBadRequest(ValidationErrorMessages.QueryExceedsMaxAllowableTableSize);
            }
        }

        [Fact]
        public async Task Query_ReleaseId()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var indicator1Id = Guid.NewGuid();
            var indicator2Id = Guid.NewGuid();
            var location1Id = Guid.NewGuid();
            var location2Id = Guid.NewGuid();
            var location3Id = Guid.NewGuid();

            var observations = new List<Observation>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Location = new Location
                    {
                        Id = location1Id,
                        GeographicLevel = GeographicLevel.Country
                    },
                    Measures = new Dictionary<Guid, string>
                    {
                        { indicator1Id, "123" },
                        { indicator2Id, "456" },
                    },
                    FilterItems = ListOf(new ObservationFilterItem
                    {
                        FilterItem = new FilterItem("Filter Item 1", Guid.NewGuid())
                    }),
                    Year = 2019,
                    TimeIdentifier = AcademicYear,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Location = new Location
                    {
                        Id = location2Id,
                        GeographicLevel = GeographicLevel.Institution
                    },
                    Measures = new Dictionary<Guid, string>
                    {
                        { indicator1Id, "678" },
                    },
                    FilterItems = ListOf(new ObservationFilterItem
                    {
                        FilterItem = new FilterItem("Filter Item 2", Guid.NewGuid())
                    }),
                    Year = 2020,
                    TimeIdentifier = AcademicYear,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Location = new Location
                    {
                        Id = location3Id,
                        GeographicLevel = GeographicLevel.Provider
                    },
                    Measures = new Dictionary<Guid, string>
                    {
                        { indicator1Id, "789" },
                        { Guid.NewGuid(), "1123" },
                        { Guid.NewGuid(), "1456" },
                    },
                    FilterItems = ListOf(new ObservationFilterItem
                    {
                        FilterItem = new FilterItem("Filter Item 3", Guid.NewGuid())
                    }),
                    Year = 2020,
                    TimeIdentifier = AcademicYear,
                }
            };

            var subjectMeta = new SubjectResultMetaViewModel
            {
                Indicators = new List<IndicatorMetaViewModel>
                {
                    new()
                    {
                        Label = "Test indicator"
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.MatchedObservations.AddRangeAsync(
                    new MatchedObservation(observations[0].Id),
                    new MatchedObservation(observations[2].Id)
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Indicators = new[] { indicator1Id, indicator2Id, },
                    LocationIds = ListOf(location1Id, location2Id, location3Id),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2019,
                        StartCode = AcademicYear,
                        EndYear = 2020,
                        EndCode = AcademicYear,
                    }
                };

                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(statisticsDbContext.MatchedObservations);

                var subjectResultMetaService = new Mock<ISubjectResultMetaService>(Strict);

                subjectResultMetaService
                    .Setup(
                        s => s.GetSubjectMeta(
                            releaseSubject.ReleaseId,
                            query,
                            It.IsAny<IList<Observation>>()
                        )
                    )
                    .ReturnsAsync(subjectMeta);

                var service = BuildTableBuilderService(
                    statisticsDbContext: statisticsDbContext,
                    observationService: observationService.Object,
                    subjectResultMetaService: subjectResultMetaService.Object
                );

                var result = await service.Query(releaseSubject.ReleaseId, query);

                VerifyAllMocks(observationService, subjectResultMetaService);

                var observationResults = result.AssertRight().Results.ToList();

                Assert.Equal(2, observationResults.Count);

                Assert.Equal(observations[0].Id, observationResults[0].Id);
                Assert.Equal(GeographicLevel.Country, observationResults[0].GeographicLevel);
                Assert.Equal(location1Id, observationResults[0].LocationId);
                Assert.Equal("2019_AY", observationResults[0].TimePeriod);
                Assert.Equal(2, observationResults[0].Measures.Count);
                Assert.Equal("123", observationResults[0].Measures[indicator1Id]);
                Assert.Equal("456", observationResults[0].Measures[indicator2Id]);
                Assert.Equal(
                    ListOf(observations[0].FilterItems.ToList()[0].FilterItemId),
                    observationResults[0].Filters
                );

                Assert.Equal(observations[2].Id, observationResults[1].Id);
                Assert.Equal(GeographicLevel.Provider, observationResults[1].GeographicLevel);
                Assert.Equal(location3Id, observationResults[1].LocationId);
                Assert.Equal("2020_AY", observationResults[1].TimePeriod);
                Assert.Single(observationResults[1].Measures);
                Assert.Equal("789", observationResults[1].Measures[indicator1Id]);
                Assert.Equal(
                    ListOf(observations[2].FilterItems.ToList()[0].FilterItemId),
                    observationResults[1].Filters
                );

                Assert.Equal(subjectMeta, result.Right.SubjectMeta);
            }
        }

        [Fact]
        public async Task Query_ReleaseId_ReleaseNotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext);

                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                };

                var result = await service.Query(Guid.NewGuid(), query);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Query_ReleaseId_SubjectNotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext);

                var query = new ObservationQueryContext
                {
                    // Does not match saved release subject
                    SubjectId = Guid.NewGuid(),
                };

                var result = await service.Query(Guid.NewGuid(), query);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Query_ReleaseId_PredictedTableTooBig()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Filters = new[] { Guid.NewGuid(), Guid.NewGuid() },
                    Indicators = new[] { Guid.NewGuid(), Guid.NewGuid() },
                    LocationIds = ListOf(Guid.NewGuid()),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2019,
                        StartCode = AcademicYear,
                        EndYear = 2020,
                        EndCode = AcademicYear
                    }
                };

                var filterItemRepository = new Mock<IFilterItemRepository>(Strict);

                filterItemRepository
                    .Setup(s => s.CountFilterItemsByFilter(
                        query.Filters))
                    .ReturnsAsync(new Dictionary<Guid, int>
                    {
                        {
                            // For the purpose of calculating the potential table size,
                            // treat all the Filter Items as belonging to the same Filter
                            Guid.NewGuid(), query.Filters.Count()
                        }
                    });

                var options = Options.Create(new TableBuilderOptions
                {
                    // 2 Filter items (from 1 Filter), 1 Location, and 2 Time periods provide 4 different combinations,
                    // assuming that all the data is provided. For 2 Indicators this would be 8 table cells rendered.
                    // Configure a maximum table size limit lower than 8.
                    MaxTableCellsAllowed = 7
                });

                var service = BuildTableBuilderService(
                    statisticsDbContext: statisticsDbContext,
                    filterItemRepository: filterItemRepository.Object,
                    options: options
                );

                var result = await service.Query(releaseSubject.ReleaseId, query);

                VerifyAllMocks(filterItemRepository);

                result.AssertBadRequest(ValidationErrorMessages.QueryExceedsMaxAllowableTableSize);
            }
        }

        [Fact]
        public async Task QueryToCsvStream_LatestRelease()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                LatestPublishedRelease = new Content.Model.Release
                {
                    Id = Guid.NewGuid(),
                }
            };

            var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
                .GenerateList(2);

            var filterItems = filters
                .SelectMany(f => f.FilterGroups)
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var indicatorGroups = _fixture.DefaultIndicatorGroup()
                .ForIndex(0, i => i
                    .SetIndicators(_fixture.DefaultIndicator().Generate(1))
                )
                .ForIndex(1, i => i
                    .SetIndicators(_fixture.DefaultIndicator().Generate(2))
                )
                .GenerateList();

            var indicators = indicatorGroups
                .SelectMany(ig => ig.Indicators)
                .ToList();

            var locations = _fixture.DefaultLocation()
                .ForRange(..2, l => l
                    .SetPresetRegion()
                    .SetGeographicLevel(GeographicLevel.Region))
                .ForRange(2..4, l => l
                    .SetPresetRegionAndLocalAuthority()
                    .SetGeographicLevel(GeographicLevel.LocalAuthority))
                .GenerateList();

            var observations = _fixture.DefaultObservation()
                .WithMeasures(indicators)
                .ForRange(..2, o => o
                    .SetFilterItems(filterItems[0], filterItems[2])
                    .SetLocation(locations[0])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(2..4, o => o
                    .SetFilterItems(filterItems[0], filterItems[2])
                    .SetLocation(locations[1])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(4..6, o => o
                    .SetFilterItems(filterItems[1], filterItems[3])
                    .SetLocation(locations[2])
                    .SetTimePeriod(2023, AcademicYear))
                .ForRange(6..8, o => o
                    .SetFilterItems(filterItems[1], filterItems[3])
                    .SetLocation(locations[3])
                    .SetTimePeriod(2023, AcademicYear))
                .GenerateList();

            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Id = publication.LatestPublishedRelease.Id,
                    PublicationId = publication.Id
                },
                Subject = _fixture.DefaultSubject()
                    .WithFilters(filters)
                    .WithIndicatorGroups(indicatorGroups)
                    .WithObservations(observations)
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.MatchedObservations.AddRangeAsync(
                    observations.Select(o => new MatchedObservation(o.Id)).ToArray()
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Indicators = indicators.Select(i => i.Id).ToList(),
                    LocationIds = locations.Select(l => l.Id).ToList(),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2022,
                        StartCode = AcademicYear,
                        EndYear = 2023,
                        EndCode = AcademicYear,
                    }
                };

                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(statisticsDbContext.MatchedObservations);

                var subjectCsvMeta = new SubjectCsvMetaViewModel
                {
                    Filters = FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(filterItems),
                    Indicators = indicators
                        .Select(i => new IndicatorCsvMetaViewModel(i))
                        .ToDictionary(i => i.Name),
                    Locations = locations.ToDictionary(l => l.Id, l => l.GetCsvValues()),
                    Headers = new List<string>
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
                        indicators[2].Name
                    }
                };

                var subjectCsvMetaService = new Mock<ISubjectCsvMetaService>(Strict);

                subjectCsvMetaService
                    .Setup(
                        s => s.GetSubjectCsvMeta(
                            It.Is<ReleaseSubject>(rs =>
                                rs.ReleaseId == releaseSubject.ReleaseId
                                && rs.SubjectId == releaseSubject.SubjectId),
                            query,
                            It.IsAny<IList<Observation>>(),
                            default
                        )
                    )
                    .ReturnsAsync(subjectCsvMeta);

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    contentDbContext,
                    observationService: observationService.Object,
                    subjectCsvMetaService: subjectCsvMetaService.Object
                );

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(query, stream);

                VerifyAllMocks(observationService, subjectCsvMetaService);

                result.AssertRight();

                stream.Seek(0L, SeekOrigin.Begin);

                Snapshot.Match(stream.ReadToEnd());
            }
        }

        [Fact]
        public async Task QueryToCsvStream_LatestRelease_ReleaseNotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext
                );

                // SubjectId exists, but no Content.Model.Release
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.SubjectId,
                };

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(query, stream);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task QueryToCsvStream_LatestRelease_SubjectNotFound()
        {
            var publicationId = Guid.NewGuid();

            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    PublicationId = publicationId
                },
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext, contentDbContext);

                // SubjectId does not exist
                var query = new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid(),
                };

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(query, stream);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task QueryToCsvStream_LatestRelease_PredictedTableTooBig()
        {
            var publication = new Publication
            {
                Id = Guid.NewGuid(),
                LatestPublishedRelease = new Content.Model.Release
                {
                    Id = Guid.NewGuid(),
                }
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = new Release
                {
                    Id = publication.LatestPublishedRelease.Id,
                    PublicationId = publication.Id
                },
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Filters = new[] { Guid.NewGuid(), Guid.NewGuid() },
                    Indicators = new[] { Guid.NewGuid(), Guid.NewGuid() },
                    LocationIds = ListOf(Guid.NewGuid()),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2019,
                        StartCode = AcademicYear,
                        EndYear = 2020,
                        EndCode = AcademicYear
                    }
                };

                var filterItemRepository = new Mock<IFilterItemRepository>(Strict);

                filterItemRepository
                    .Setup(s => s.CountFilterItemsByFilter(query.Filters))
                    .ReturnsAsync(new Dictionary<Guid, int>
                    {
                        {
                            // For the purpose of calculating the potential table size,
                            // treat all the Filter Items as belonging to the same Filter
                            Guid.NewGuid(), query.Filters.Count()
                        }
                    });

                var options = Options.Create(new TableBuilderOptions
                {
                    // 2 Filter items (from 1 Filter), 1 Location, and 2 Time periods provide 4 different combinations,
                    // assuming that all the data is provided. For 2 Indicators this would be 8 table cells rendered.
                    // Configure a maximum table size limit lower than 8.
                    MaxTableCellsAllowed = 7
                });

                var service = BuildTableBuilderService(
                    statisticsDbContext: statisticsDbContext,
                    contentDbContext: contentDbContext,
                    filterItemRepository: filterItemRepository.Object,
                    options: options
                );

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(query, stream);

                VerifyAllMocks(filterItemRepository);

                result.AssertBadRequest(ValidationErrorMessages.QueryExceedsMaxAllowableTableSize);
            }
        }

        [Fact]
        public async Task QueryToCsvStream_ReleaseId()
        {
            var filters = _fixture.DefaultFilter(filterGroupCount: 1, filterItemCount: 2)
                .GenerateList(2);

            var filterItems = filters
                .SelectMany(f => f.FilterGroups)
                .SelectMany(fg => fg.FilterItems)
                .ToList();

            var indicatorGroups = _fixture.DefaultIndicatorGroup()
                .ForIndex(0, i => i
                    .SetIndicators(_fixture.DefaultIndicator().Generate(1))
                )
                .ForIndex(1, i => i
                    .SetIndicators(_fixture.DefaultIndicator().Generate(2))
                )
                .GenerateList();

            var indicators = indicatorGroups
                .SelectMany(ig => ig.Indicators)
                .ToList();

            var locations = _fixture.DefaultLocation()
                .WithPresetRegion()
                .WithGeographicLevel(GeographicLevel.Region)
                .GenerateList(2);

            var observations = _fixture.DefaultObservation()
                .WithMeasures(indicators)
                .ForRange(..2, o => o
                    .SetFilterItems(filterItems[0], filterItems[2])
                    .SetLocation(locations[0])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(2..4, o => o
                    .SetFilterItems(filterItems[1], filterItems[3])
                    .SetLocation(locations[1])
                    .SetTimePeriod(2023, AcademicYear))
                .GenerateList();

            var releaseSubject = new ReleaseSubject
            {
                Release = _fixture.DefaultStatsRelease(),
                Subject = _fixture.DefaultSubject()
                    .WithFilters(filters)
                    .WithIndicatorGroups(indicatorGroups)
                    .WithObservations(observations)
            };


            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.MatchedObservations.AddRangeAsync(
                    observations.Select(o => new MatchedObservation(o.Id)).ToArray()
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Indicators = indicators.Select(i => i.Id).ToList(),
                    LocationIds = locations.Select(l => l.Id).ToList(),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2022,
                        StartCode = AcademicYear,
                        EndYear = 2023,
                        EndCode = AcademicYear,
                    }
                };

                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(statisticsDbContext.MatchedObservations);

                var subjectCsvMeta = new SubjectCsvMetaViewModel
                {
                    Filters = FiltersMetaViewModelBuilder.BuildCsvFiltersFromFilterItems(filterItems),
                    Indicators = indicators
                        .Select(i => new IndicatorCsvMetaViewModel(i))
                        .ToDictionary(i => i.Name),
                    Locations = locations.ToDictionary(l => l.Id, l => l.GetCsvValues()),
                    Headers = new List<string>
                    {
                        "time_period",
                        "time_identifier",
                        "geographic_level",
                        "country_code",
                        "country_name",
                        "region_code",
                        "region_name",
                        filters[0].Name,
                        filters[1].Name,
                        indicators[0].Name,
                        indicators[1].Name,
                        indicators[2].Name
                    }
                };

                var subjectCsvMetaService = new Mock<ISubjectCsvMetaService>(Strict);

                subjectCsvMetaService
                    .Setup(
                        s => s.GetSubjectCsvMeta(
                            It.Is<ReleaseSubject>(rs =>
                                rs.ReleaseId == releaseSubject.ReleaseId
                                && rs.SubjectId == releaseSubject.SubjectId),
                            query,
                            It.IsAny<IList<Observation>>(),
                            default
                        )
                    )
                    .ReturnsAsync(subjectCsvMeta);

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    contentDbContext,
                    observationService: observationService.Object,
                    subjectCsvMetaService: subjectCsvMetaService.Object
                );

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(releaseSubject.ReleaseId, query, stream);

                VerifyAllMocks(observationService, subjectCsvMetaService);

                result.AssertRight();

                stream.Seek(0L, SeekOrigin.Begin);

                Snapshot.Match(stream.ReadToEnd());
            }
        }

        [Fact]
        public async Task QueryToCsvStream_ReleaseId_NoFilters()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var indicators = _fixture.DefaultIndicator()
                .WithIndicatorGroup(
                    _fixture.DefaultIndicatorGroup()
                        .WithSubject(releaseSubject.Subject))
                .GenerateList(3);

            var locations = _fixture.DefaultLocation()
                .WithPresetRegion()
                .WithGeographicLevel(GeographicLevel.Region)
                .GenerateList(2);

            var observations = _fixture.DefaultObservation()
                .WithSubject(releaseSubject.Subject)
                .WithMeasures(indicators)
                .ForRange(..2, o => o
                    .SetLocation(locations[0])
                    .SetTimePeriod(2022, AcademicYear))
                .ForRange(2..4, o => o
                    .SetLocation(locations[1])
                    .SetTimePeriod(2022, AcademicYear))
                .GenerateList();

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.Observation.AddRangeAsync(observations);
                await statisticsDbContext.MatchedObservations.AddRangeAsync(
                    observations.Select(o => new MatchedObservation(o.Id)).ToArray()
                );
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Indicators = indicators.Select(i => i.Id).ToList(),
                    LocationIds = locations.Select(l => l.Id).ToList(),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2022,
                        StartCode = AcademicYear,
                        EndYear = 2023,
                        EndCode = AcademicYear,
                    }
                };

                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(statisticsDbContext.MatchedObservations);

                var subjectCsvMeta = new SubjectCsvMetaViewModel
                {
                    Indicators = indicators
                        .Select(i => new IndicatorCsvMetaViewModel(i))
                        .ToDictionary(i => i.Name),
                    Locations = locations.ToDictionary(l => l.Id, l => l.GetCsvValues()),
                    Headers = new List<string>
                    {
                        "time_period",
                        "time_identifier",
                        "geographic_level",
                        "country_code",
                        "country_name",
                        "region_code",
                        "region_name",
                        indicators[0].Name,
                        indicators[1].Name
                    }
                };

                var subjectCsvMetaService = new Mock<ISubjectCsvMetaService>(Strict);

                subjectCsvMetaService
                    .Setup(
                        s => s.GetSubjectCsvMeta(
                            It.Is<ReleaseSubject>(rs =>
                                rs.ReleaseId == releaseSubject.ReleaseId
                                && rs.SubjectId == releaseSubject.SubjectId),
                            query,
                            It.IsAny<IList<Observation>>(),
                            default
                        )
                    )
                    .ReturnsAsync(subjectCsvMeta);

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    contentDbContext,
                    observationService: observationService.Object,
                    subjectCsvMetaService: subjectCsvMetaService.Object
                );

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(releaseSubject.ReleaseId, query, stream);

                VerifyAllMocks(observationService, subjectCsvMetaService);

                result.AssertRight();

                stream.Seek(0L, SeekOrigin.Begin);

                Snapshot.Match(stream.ReadToEnd());
            }
        }

        [Fact]
        public async Task QueryToCsvStream_ReleaseId_ReleaseNotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext, contentDbContext);

                // SubjectId exists, but no Content.Model.Release
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.SubjectId,
                };

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(Guid.NewGuid(), query, stream);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task QueryToCsvStream_ReleaseId_SubjectNotFound()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext, contentDbContext);

                // SubjectId does not exist
                var query = new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid(),
                };

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(releaseSubject.ReleaseId, query, stream);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task QueryToCsvStream_ReleaseId_PredictedTableTooBig()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject()
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.ReleaseSubject.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject.Subject.Id,
                    Filters = new[] { Guid.NewGuid(), Guid.NewGuid() },
                    Indicators = new[] { Guid.NewGuid(), Guid.NewGuid() },
                    LocationIds = ListOf(Guid.NewGuid()),
                    TimePeriod = new TimePeriodQuery
                    {
                        StartYear = 2019,
                        StartCode = AcademicYear,
                        EndYear = 2020,
                        EndCode = AcademicYear
                    }
                };

                var filterItemRepository = new Mock<IFilterItemRepository>(Strict);

                filterItemRepository
                    .Setup(s => s.CountFilterItemsByFilter(query.Filters))
                    .ReturnsAsync(new Dictionary<Guid, int>
                    {
                        {
                            // For the purpose of calculating the potential table size,
                            // treat all the Filter Items as belonging to the same Filter
                            Guid.NewGuid(), query.Filters.Count()
                        }
                    });

                var options = Options.Create(new TableBuilderOptions
                {
                    // 2 Filter items (from 1 Filter), 1 Location, and 2 Time periods provide 4 different combinations,
                    // assuming that all the data is provided. For 2 Indicators this would be 8 table cells rendered.
                    // Configure a maximum table size limit lower than 8.
                    MaxTableCellsAllowed = 7
                });

                var service = BuildTableBuilderService(
                    statisticsDbContext: statisticsDbContext,
                    filterItemRepository: filterItemRepository.Object,
                    options: options
                );

                using var stream = new MemoryStream();

                var result = await service.QueryToCsvStream(releaseSubject.ReleaseId, query, stream);

                VerifyAllMocks(filterItemRepository);

                result.AssertBadRequest(ValidationErrorMessages.QueryExceedsMaxAllowableTableSize);
            }
        }

        private static IOptions<TableBuilderOptions> DefaultOptions()
        {
            return Options.Create(new TableBuilderOptions
            {
                MaxTableCellsAllowed = 25000
            });
        }

        private static TableBuilderService BuildTableBuilderService(
            StatisticsDbContext statisticsDbContext,
            ContentDbContext? contentDbContext = null,
            IFilterItemRepository? filterItemRepository = null,
            IObservationService? observationService = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            ISubjectResultMetaService? subjectResultMetaService = null,
            ISubjectCsvMetaService? subjectCsvMetaService = null,
            ISubjectRepository? subjectRepository = null,
            IUserService? userService = null,
            IReleaseRepository? releaseRepository = null,
            IOptions<TableBuilderOptions>? options = null)
        {
            return new(
                statisticsDbContext,
                filterItemRepository ?? Mock.Of<IFilterItemRepository>(Strict),
                observationService ?? Mock.Of<IObservationService>(Strict),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                subjectResultMetaService ?? Mock.Of<ISubjectResultMetaService>(Strict),
                subjectCsvMetaService ?? Mock.Of<ISubjectCsvMetaService>(Strict),
                subjectRepository ?? new SubjectRepository(statisticsDbContext),
                userService ?? AlwaysTrueUserService().Object,
                releaseRepository ?? new ReleaseRepository(contentDbContext ?? Mock.Of<ContentDbContext>()),
                options ?? DefaultOptions()
            );
        }
    }
}
