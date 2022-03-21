#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class TableBuilderServiceTests
    {
        [Fact]
        public async Task Query_LatestRelease()
        {
            var publicationId = Guid.NewGuid();

            var release = new Release
            {
                PublicationId = publicationId,
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var indicator1Id = Guid.NewGuid();
            var indicator2Id = Guid.NewGuid();
            var location1Id = Guid.NewGuid();
            var location2Id = Guid.NewGuid();
            var location3Id = Guid.NewGuid();

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.Subject.Id,
                Indicators = new[] {indicator1Id, indicator2Id,},
                LocationIds = ListOf(location1Id, location2Id, location3Id),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = TimeIdentifier.AcademicYear,
                    EndYear = 2020,
                    EndCode = TimeIdentifier.AcademicYear,
                }
            };

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
                        FilterItem = new FilterItem("Filter Item 1", new FilterGroup())
                    }),
                    Year = 2019,
                    TimeIdentifier = TimeIdentifier.AcademicYear,
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
                        FilterItem = new FilterItem("Filter Item 2", new FilterGroup())
                    }),
                    Year = 2020,
                    TimeIdentifier = TimeIdentifier.AcademicYear,
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
                        FilterItem = new FilterItem("Filter Item 3", new FilterGroup())
                    }),
                    Year = 2020,
                    TimeIdentifier = TimeIdentifier.AcademicYear,
                }
            };

            var subjectMeta = new ResultSubjectMetaViewModel
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

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(releaseSubject);
                await context.Observation.AddRangeAsync(observations);
                await context.MatchedObservations.AddRangeAsync(
                    new MatchedObservation(observations[0].Id),
                    new MatchedObservation(observations[2].Id));
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var observationService = new Mock<IObservationService>(Strict);
                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(context.MatchedObservations);

                var resultSubjectMetaService = new Mock<IResultSubjectMetaService>(Strict);

                resultSubjectMetaService
                    .Setup(
                        s => s.GetSubjectMeta(
                            release.Id,
                            query,
                            It.IsAny<IList<Observation>>()
                        )
                    )
                    .ReturnsAsync(subjectMeta);

                var subjectRepository = new Mock<ISubjectRepository>(Strict);

                subjectRepository
                    .Setup(s => s.GetPublicationIdForSubject(query.SubjectId))
                    .ReturnsAsync(publicationId);

                subjectRepository
                    .Setup(s => s.IsSubjectForLatestPublishedRelease(query.SubjectId))
                    .ReturnsAsync(true);

                var releaseRepository = new Mock<IReleaseRepository>(Strict);

                releaseRepository
                    .Setup(s => s.GetLatestPublishedRelease(publicationId))
                    .Returns(release);

                var service = BuildTableBuilderService(
                    context,
                    observationService: observationService.Object,
                    resultSubjectMetaService: resultSubjectMetaService.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseRepository: releaseRepository.Object
                );

                var result = await service.Query(query);

                VerifyAllMocks(
                    observationService,
                    resultSubjectMetaService,
                    subjectRepository,
                    releaseRepository);

                var observationResults = result.AssertRight().Results.ToList();

                Assert.Equal(2, observationResults.Count);

                Assert.Equal(observations[0].Id, observationResults[0].Id);
                Assert.Equal(GeographicLevel.Country, observationResults[0].GeographicLevel);
                Assert.Equal(location1Id, observationResults[0].LocationId);
                Assert.Equal("2019_AY", observationResults[0].TimePeriod);
                Assert.Equal(2, observationResults[0].Measures.Count);
                Assert.Equal("123", observationResults[0].Measures[indicator1Id]);
                Assert.Equal("456", observationResults[0].Measures[indicator2Id]);
                Assert.Equal(ListOf(observations[0].FilterItems.ToList()[0].FilterItemId), observationResults[0].Filters);

                Assert.Equal(observations[2].Id, observationResults[1].Id);
                Assert.Equal(GeographicLevel.Provider, observationResults[1].GeographicLevel);
                Assert.Equal(location3Id, observationResults[1].LocationId);
                Assert.Equal("2020_AY", observationResults[1].TimePeriod);
                Assert.Single(observationResults[1].Measures);
                Assert.Equal("789", observationResults[1].Measures[indicator1Id]);
                Assert.Equal(ListOf(observations[2].FilterItems.ToList()[0].FilterItemId), observationResults[1].Filters);

                Assert.Equal(subjectMeta, result.Right.SubjectMeta);
            }
        }

        [Fact]
        public async Task Query_LatestRelease_ReleaseNotFound()
        {
            var publicationId = Guid.NewGuid();

            var query = new ObservationQueryContext
            {
                SubjectId = Guid.NewGuid(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var subjectRepository = new Mock<ISubjectRepository>(Strict);

                subjectRepository
                    .Setup(s => s.GetPublicationIdForSubject(query.SubjectId))
                    .ReturnsAsync(publicationId);

                var releaseRepository = new Mock<IReleaseRepository>(Strict);

                releaseRepository
                    .Setup(s => s.GetLatestPublishedRelease(publicationId))
                    .Returns((Release?) null);

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    subjectRepository: subjectRepository.Object,
                    releaseRepository: releaseRepository.Object
                );

                var result = await service.Query(query);

                VerifyAllMocks(subjectRepository, releaseRepository);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Query_LatestRelease_SubjectNotFound()
        {
            var publicationId = Guid.NewGuid();

            var release = new Release
            {
                PublicationId = publicationId,
            };

            var query = new ObservationQueryContext
            {
                SubjectId = Guid.NewGuid(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using var statisticsDbContext = InMemoryStatisticsDbContext(contextId);
            var subjectRepository = new Mock<ISubjectRepository>(Strict);

            subjectRepository
                .Setup(s => s.GetPublicationIdForSubject(query.SubjectId))
                .ReturnsAsync(publicationId);

            var releaseRepository = new Mock<IReleaseRepository>(Strict);

            releaseRepository
                .Setup(s => s.GetLatestPublishedRelease(publicationId))
                .Returns(release);

            var service = BuildTableBuilderService(
                statisticsDbContext,
                subjectRepository: subjectRepository.Object,
                releaseRepository: releaseRepository.Object
            );

            var result = await service.Query(query);

            VerifyAllMocks(subjectRepository, releaseRepository);

            result.AssertNotFound();
        }

        [Fact]
        public async Task Query_LatestRelease_PredictedTableTooBig()
        {
            var publicationId = Guid.NewGuid();

            var release = new Release
            {
                PublicationId = publicationId
            };

            var releaseSubject = new ReleaseSubject
            {
                Release = release,
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.Subject.Id,
                Filters = new[] {Guid.NewGuid(), Guid.NewGuid()},
                Indicators = new[] {Guid.NewGuid(), Guid.NewGuid()},
                LocationIds = ListOf(Guid.NewGuid()),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = TimeIdentifier.AcademicYear,
                    EndYear = 2020,
                    EndCode = TimeIdentifier.AcademicYear
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
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

                var subjectRepository = new Mock<ISubjectRepository>(Strict);

                subjectRepository
                    .Setup(s => s.GetPublicationIdForSubject(query.SubjectId))
                    .ReturnsAsync(publicationId);

                subjectRepository
                    .Setup(s => s.IsSubjectForLatestPublishedRelease(query.SubjectId))
                    .ReturnsAsync(true);

                var releaseRepository = new Mock<IReleaseRepository>(Strict);

                releaseRepository
                    .Setup(s => s.GetLatestPublishedRelease(publicationId))
                    .Returns(release);

                var options = Options.Create(new TableBuilderOptions
                {
                    // 2 Filter items (from 1 Filter), 1 Location, and 2 Time periods provide 4 different combinations,
                    // assuming that all the data is provided. For 2 Indicators this would be 8 table cells rendered.
                    // Configure a maximum table size limit lower than 8.
                    MaxTableCellsAllowed = 7
                });

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    filterItemRepository: filterItemRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseRepository: releaseRepository.Object,
                    options: options
                );

                var result = await service.Query(query);

                VerifyAllMocks(filterItemRepository, subjectRepository, releaseRepository);

                result.AssertBadRequest(ValidationErrorMessages.QueryExceedsMaxAllowableTableSize);
            }
        }

        [Fact]
        public async Task Query_ReleaseId()
        {
            var releaseSubject = new ReleaseSubject
            {
                Release = new Release(),
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                },
            };

            var indicator1Id = Guid.NewGuid();
            var indicator2Id = Guid.NewGuid();
            var location1Id = Guid.NewGuid();
            var location2Id = Guid.NewGuid();
            var location3Id = Guid.NewGuid();

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.Subject.Id,
                Indicators = new[] { indicator1Id, indicator2Id, },
                LocationIds = ListOf(location1Id, location2Id, location3Id),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = TimeIdentifier.AcademicYear,
                    EndYear = 2020,
                    EndCode = TimeIdentifier.AcademicYear,
                }
            };

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
                        FilterItem = new FilterItem("Filter Item 1", new FilterGroup())
                    }),
                    Year = 2019,
                    TimeIdentifier = TimeIdentifier.AcademicYear,
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
                        FilterItem = new FilterItem("Filter Item 2", new FilterGroup())
                    }),
                    Year = 2020,
                    TimeIdentifier = TimeIdentifier.AcademicYear,
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
                        FilterItem = new FilterItem("Filter Item 3", new FilterGroup())
                    }),
                    Year = 2020,
                    TimeIdentifier = TimeIdentifier.AcademicYear,
                }
            };

            var subjectMeta = new ResultSubjectMetaViewModel
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

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                await context.AddAsync(releaseSubject);
                await context.Observation.AddRangeAsync(observations);
                await context.MatchedObservations.AddRangeAsync(
                    new MatchedObservation(observations[0].Id),
                    new MatchedObservation(observations[2].Id));
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryStatisticsDbContext(contextId))
            {
                var observationService = new Mock<IObservationService>(Strict);

                observationService
                    .Setup(s => s.GetMatchedObservations(query, default))
                    .ReturnsAsync(context.MatchedObservations);

                var resultSubjectMetaService = new Mock<IResultSubjectMetaService>(Strict);

                resultSubjectMetaService
                    .Setup(
                        s => s.GetSubjectMeta(
                            releaseSubject.ReleaseId,
                            query,
                            It.IsAny<IList<Observation>>()
                        )
                    )
                    .ReturnsAsync(subjectMeta);

                var subjectRepository = new Mock<ISubjectRepository>(Strict);
                subjectRepository.Setup(s =>
                        s.IsSubjectForLatestPublishedRelease(releaseSubject.Subject.Id))
                    .ReturnsAsync(false);

                var service = BuildTableBuilderService(
                    context,
                    observationService: observationService.Object,
                    resultSubjectMetaService: resultSubjectMetaService.Object,
                    subjectRepository: subjectRepository.Object
                );

                var result = await service.Query(releaseSubject.ReleaseId, query);

                VerifyAllMocks(observationService, resultSubjectMetaService, subjectRepository);

                var observationResults = result.AssertRight().Results.ToList();

                Assert.Equal(2, observationResults.Count);

                Assert.Equal(observations[0].Id, observationResults[0].Id);
                Assert.Equal(GeographicLevel.Country, observationResults[0].GeographicLevel);
                Assert.Equal(location1Id, observationResults[0].LocationId);
                Assert.Equal("2019_AY", observationResults[0].TimePeriod);
                Assert.Equal(2, observationResults[0].Measures.Count);
                Assert.Equal("123", observationResults[0].Measures[indicator1Id]);
                Assert.Equal("456", observationResults[0].Measures[indicator2Id]);
                Assert.Equal(ListOf(observations[0].FilterItems.ToList()[0].FilterItemId), observationResults[0].Filters);

                Assert.Equal(observations[2].Id, observationResults[1].Id);
                Assert.Equal(GeographicLevel.Provider, observationResults[1].GeographicLevel);
                Assert.Equal(location3Id, observationResults[1].LocationId);
                Assert.Equal("2020_AY", observationResults[1].TimePeriod);
                Assert.Single(observationResults[1].Measures);
                Assert.Equal("789", observationResults[1].Measures[indicator1Id]);
                Assert.Equal(ListOf(observations[2].FilterItems.ToList()[0].FilterItemId), observationResults[1].Filters);

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

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.Subject.Id,
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext);

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

            var query = new ObservationQueryContext
            {
                SubjectId = Guid.NewGuid(),
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext);

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
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                },
            };

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.Subject.Id,
                Filters = new[] {Guid.NewGuid(), Guid.NewGuid()},
                Indicators = new[] {Guid.NewGuid(), Guid.NewGuid()},
                LocationIds = ListOf(Guid.NewGuid()),
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = TimeIdentifier.AcademicYear,
                    EndYear = 2020,
                    EndCode = TimeIdentifier.AcademicYear
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(contextId))
            {
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

                var subjectRepository = new Mock<ISubjectRepository>(Strict);
                subjectRepository.Setup(s =>
                        s.IsSubjectForLatestPublishedRelease(releaseSubject.Subject.Id))
                    .ReturnsAsync(false);

                var options = Options.Create(new TableBuilderOptions
                {
                    // 2 Filter items (from 1 Filter), 1 Location, and 2 Time periods provide 4 different combinations,
                    // assuming that all the data is provided. For 2 Indicators this would be 8 table cells rendered.
                    // Configure a maximum table size limit lower than 8.
                    MaxTableCellsAllowed = 7
                });

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    filterItemRepository: filterItemRepository.Object,
                    subjectRepository: subjectRepository.Object,
                    options: options
                );

                var result = await service.Query(releaseSubject.ReleaseId, query);

                VerifyAllMocks(filterItemRepository, subjectRepository);

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
            IFilterItemRepository? filterItemRepository = null,
            IObservationService? observationService = null,
            IPersistenceHelper<StatisticsDbContext>? statisticsPersistenceHelper = null,
            IResultSubjectMetaService? resultSubjectMetaService = null,
            ISubjectRepository? subjectRepository = null,
            IUserService? userService = null,
            IResultBuilder<Observation, ObservationViewModel>? resultBuilder = null,
            IReleaseRepository? releaseRepository = null,
            IOptions<TableBuilderOptions>? options = null)
        {
            return new(
                statisticsDbContext,
                filterItemRepository ?? Mock.Of<IFilterItemRepository>(Strict),
                observationService ?? Mock.Of<IObservationService>(Strict),
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                resultSubjectMetaService ?? Mock.Of<IResultSubjectMetaService>(Strict),
                subjectRepository ?? Mock.Of<ISubjectRepository>(Strict),
                userService ?? AlwaysTrueUserService().Object,
                resultBuilder ?? new ResultBuilder(),
                releaseRepository ?? Mock.Of<IReleaseRepository>(Strict),
                options ?? DefaultOptions()
            );
        }
    }
}
