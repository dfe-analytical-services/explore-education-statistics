using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Release = GovUk.Education.ExploreEducationStatistics.Data.Model.Release;

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

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.Subject.Id,
                Indicators = new[] { indicator1Id, indicator2Id, },
                Locations = new LocationQuery
                {
                    Country = new[] { "england" }
                },
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = TimeIdentifier.AcademicYear,
                    EndYear = 2020,
                    EndCode = TimeIdentifier.AcademicYear,
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var observations = new List<Observation>
                {
                    new Observation
                    {
                        Measures = new Dictionary<Guid, string>
                        {
                            { indicator1Id, "123" },
                            { indicator2Id, "456" },
                        },
                        FilterItems = new List<ObservationFilterItem>(),
                        Year = 2019,
                        TimeIdentifier = TimeIdentifier.AcademicYear,
                    },
                    new Observation
                    {
                        Measures = new Dictionary<Guid, string>
                        {
                            { indicator1Id, "789" },
                            { Guid.NewGuid(), "1123" },
                            { Guid.NewGuid(), "1456" },
                        },
                        FilterItems = new List<ObservationFilterItem>(),
                        Year = 2020,
                        TimeIdentifier = TimeIdentifier.AcademicYear,
                    },
                };

                var subjectMeta = new ResultSubjectMetaViewModel
                {
                    Indicators = new List<IndicatorMetaViewModel>
                    {
                        new IndicatorMetaViewModel
                        {
                            Label = "Test indicator"
                        }
                    },
                };

                var observationService = new Mock<IObservationService>();

                observationService
                    .Setup(s => s.FindObservations(query))
                    .Returns(observations);

                var resultSubjectMetaService = new Mock<IResultSubjectMetaService>();

                resultSubjectMetaService
                    .Setup(
                        s => s.GetSubjectMeta(
                            release.Id,
                            It.IsAny<SubjectMetaQueryContext>(),
                            It.IsAny<IQueryable<Observation>>()
                        )
                    )
                    .ReturnsAsync(subjectMeta);

                var subjectRepository = new Mock<ISubjectRepository>();

                subjectRepository
                    .Setup(s => s.GetPublicationIdForSubject(query.SubjectId))
                    .ReturnsAsync(publicationId);

                subjectRepository
                    .Setup(s => s.IsSubjectForLatestPublishedRelease(query.SubjectId))
                    .ReturnsAsync(true);

                var releaseRepository = new Mock<IReleaseRepository>();

                releaseRepository
                    .Setup(s => s.GetLatestPublishedRelease(publicationId))
                    .Returns(release);

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    observationService: observationService.Object,
                    resultSubjectMetaService: resultSubjectMetaService.Object,
                    subjectRepository: subjectRepository.Object,
                    releaseRepository: releaseRepository.Object
                );

                var result = await service.Query(query);

                Assert.True(result.IsRight);

                var observationResults = result.Right.Results.ToList();

                Assert.Equal(2, observationResults.Count);

                Assert.Equal("2019_AY", observationResults[0].TimePeriod);
                Assert.Equal(2, observationResults[0].Measures.Count);
                Assert.Equal("123", observationResults[0].Measures[indicator1Id.ToString()]);
                Assert.Equal("456", observationResults[0].Measures[indicator2Id.ToString()]);

                Assert.Equal("2020_AY", observationResults[1].TimePeriod);
                Assert.Single(observationResults[1].Measures);
                Assert.Equal("789", observationResults[1].Measures[indicator1Id.ToString()]);

                Assert.Equal(subjectMeta, result.Right.SubjectMeta);

                MockUtils.VerifyAllMocks(observationService, resultSubjectMetaService, subjectRepository, releaseRepository);
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

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var subjectRepository = new Mock<ISubjectRepository>();

                subjectRepository
                    .Setup(s => s.GetPublicationIdForSubject(query.SubjectId))
                    .ReturnsAsync(publicationId);

                var releaseRepository = new Mock<IReleaseRepository>();

                releaseRepository
                    .Setup(s => s.GetLatestPublishedRelease(publicationId));

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    subjectRepository: subjectRepository.Object,
                    releaseRepository: releaseRepository.Object
                );

                var result = await service.Query(query);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
                MockUtils.VerifyAllMocks(subjectRepository, releaseRepository);
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

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var subjectRepository = new Mock<ISubjectRepository>();

                subjectRepository
                    .Setup(s => s.GetPublicationIdForSubject(query.SubjectId))
                    .ReturnsAsync(publicationId);

                var releaseRepository = new Mock<IReleaseRepository>();

                releaseRepository
                    .Setup(s => s.GetLatestPublishedRelease(publicationId))
                    .Returns(release);

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    subjectRepository: subjectRepository.Object,
                    releaseRepository: releaseRepository.Object
                );

                var result = await service.Query(query);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
                MockUtils.VerifyAllMocks(subjectRepository, releaseRepository);
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

            var query = new ObservationQueryContext
            {
                SubjectId = releaseSubject.Subject.Id,
                Indicators = new[] { indicator1Id, indicator2Id, },
                Locations = new LocationQuery
                {
                    Country = new[] { "england" }
                },
                TimePeriod = new TimePeriodQuery
                {
                    StartYear = 2019,
                    StartCode = TimeIdentifier.AcademicYear,
                    EndYear = 2020,
                    EndCode = TimeIdentifier.AcademicYear,
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var observations = new List<Observation>
                {
                    new Observation
                    {
                        Measures = new Dictionary<Guid, string>
                        {
                            { indicator1Id, "123" },
                            { indicator2Id, "456" },
                        },
                        FilterItems = new List<ObservationFilterItem>(),
                        Year = 2019,
                        TimeIdentifier = TimeIdentifier.AcademicYear,
                    },
                    new Observation
                    {
                        Measures = new Dictionary<Guid, string>
                        {
                            { indicator1Id, "789" },
                            { Guid.NewGuid(), "1123" },
                            { Guid.NewGuid(), "1456" },
                        },
                        FilterItems = new List<ObservationFilterItem>(),
                        Year = 2020,
                        TimeIdentifier = TimeIdentifier.AcademicYear,
                    },
                };

                var subjectMeta = new ResultSubjectMetaViewModel
                {
                    Indicators = new List<IndicatorMetaViewModel>
                    {
                        new IndicatorMetaViewModel
                        {
                            Label = "Test indicator"
                        }
                    },
                };

                var observationService = new Mock<IObservationService>();

                observationService
                    .Setup(s => s.FindObservations(query))
                    .Returns(observations);

                var resultSubjectMetaService = new Mock<IResultSubjectMetaService>();

                resultSubjectMetaService
                    .Setup(
                        s => s.GetSubjectMeta(
                            releaseSubject.ReleaseId,
                            It.IsAny<SubjectMetaQueryContext>(),
                            It.IsAny<IQueryable<Observation>>()
                        )
                    )
                    .ReturnsAsync(subjectMeta);

                var service = BuildTableBuilderService(
                    statisticsDbContext,
                    observationService: observationService.Object,
                    resultSubjectMetaService: resultSubjectMetaService.Object
                );

                var result = await service.Query(releaseSubject.ReleaseId, query);

                Assert.True(result.IsRight);

                var observationResults = result.Right.Results.ToList();

                Assert.Equal(2, observationResults.Count);

                Assert.Equal("2019_AY", observationResults[0].TimePeriod);
                Assert.Equal(2, observationResults[0].Measures.Count);
                Assert.Equal("123", observationResults[0].Measures[indicator1Id.ToString()]);
                Assert.Equal("456", observationResults[0].Measures[indicator2Id.ToString()]);

                Assert.Equal("2020_AY", observationResults[1].TimePeriod);
                Assert.Single(observationResults[1].Measures);
                Assert.Equal("789", observationResults[1].Measures[indicator1Id.ToString()]);

                Assert.Equal(subjectMeta, result.Right.SubjectMeta);

                MockUtils.VerifyAllMocks(observationService, resultSubjectMetaService);
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

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext);

                var result = await service.Query(Guid.NewGuid(), query);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
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

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = StatisticsDbUtils.InMemoryStatisticsDbContext(contextId))
            {
                var service = BuildTableBuilderService(statisticsDbContext);

                var result = await service.Query(Guid.NewGuid(), query);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        private TableBuilderService BuildTableBuilderService(
            StatisticsDbContext statisticsDbContext,
            IObservationService observationService = null,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper = null,
            IResultSubjectMetaService resultSubjectMetaService = null,
            ISubjectRepository subjectRepository = null,
            IUserService userService = null,
            IResultBuilder<Observation, ObservationViewModel> resultBuilder = null,
            IReleaseRepository releaseRepository = null)
        {
            return new TableBuilderService(
                observationService ?? new Mock<IObservationService>().Object,
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                resultSubjectMetaService ?? new Mock<IResultSubjectMetaService>().Object,
                subjectRepository ?? new Mock<ISubjectRepository>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                resultBuilder ?? new ResultBuilder(DataServiceMapperUtils.DataServiceMapper()),
                releaseRepository ?? new Mock<IReleaseRepository>().Object
            );
        }
    }
}
