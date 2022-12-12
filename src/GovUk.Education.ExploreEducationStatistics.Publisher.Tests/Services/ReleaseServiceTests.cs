#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class ReleaseServiceTests
    {
        [Fact]
        public async Task GetFiles()
        {
            var release = new Release();
            var releaseFiles = new List<ReleaseFile>
            {
                new()
                {
                    Release = release,
                    Name = "Ancillary Test File",
                    File = new File
                    {
                        Filename = "ancillary.pdf",
                        ContentLength = 10240,
                        Type = Ancillary
                    }
                },
                new()
                {
                    Release = release,
                    File = new File
                    {
                        Filename = "chart.png",
                        Type = Chart
                    }
                },
                new()
                {
                    Release = release,
                    Name = "Data Test File",
                    File = new File
                    {
                        Filename = "data.csv",
                        ContentLength = 20480,
                        Type = FileType.Data
                    }
                },
                new()
                {
                    Release = release,
                    File = new File
                    {
                        Filename = "data.meta.csv",
                        Type = Metadata
                    }
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release);
                await contentDbContext.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext);

                var result = await service.GetFiles(release.Id,
                    Ancillary,
                    Chart);

                Assert.Equal(2, result.Count);
                Assert.Equal(releaseFiles[0].File.Id, result[0].Id);
                Assert.Equal(releaseFiles[1].File.Id, result[1].Id);
            }
        }

        [Fact]
        public async Task GetLatestRelease()
        {
            var publication = new Publication();

            var release1 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2017",
                TimePeriodCoverage = AcademicYearQ4,
                Slug = "2017-18-q4",
                Published = new DateTime(2019, 4, 1),
                ApprovalStatus = Approved
            };

            var release2 = new Release
            {
                Id = new Guid("e7e1aae3-a0a1-44b7-bdf3-3df4a363ce20"),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ1,
                Slug = "2018-19-q1",
                Published = new DateTime(2019, 3, 1),
                ApprovalStatus = Approved,
            };

            var release3V0 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ2,
                Slug = "2018-19-q2",
                Published = new DateTime(2019, 1, 1),
                ApprovalStatus = Approved,
                Version = 0,
                PreviousVersionId = null
            };

            var release3V1 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ2,
                Slug = "2018-19-q2",
                Published = new DateTime(2019, 2, 1),
                ApprovalStatus = Approved,
                Version = 1,
                PreviousVersionId = release3V0.Id
            };

            var release3V2Deleted = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ2,
                Slug = "2018-19-q2",
                Published = null,
                ApprovalStatus = Approved,
                Version = 2,
                PreviousVersionId = release3V1.Id,
                SoftDeleted = true
            };

            var release3V3NotPublished = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ2,
                Slug = "2018-19-q2",
                Published = null,
                ApprovalStatus = Approved,
                Version = 3,
                PreviousVersionId = release3V1.Id
            };

            var release4 = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                ReleaseName = "2018",
                TimePeriodCoverage = AcademicYearQ3,
                Slug = "2018-19-q3",
                Published = null,
                ApprovalStatus = Approved
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(publication);
                await contentDbContext.AddRangeAsync(release1,
                    release2,
                    release3V0,
                    release3V1,
                    release3V2Deleted,
                    release3V3NotPublished,
                    release4);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext);

                var result = await service.GetLatestRelease(publication.Id, Enumerable.Empty<Guid>());

                Assert.Equal(release3V1.Id, result.Id);
                Assert.Equal("Academic Year Q2 2018/19", result.Title);
            }
        }

        [Fact]
        public async Task SetPublishedDates()
        {
            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Publication = new Publication(),
                Version = 0
            };

            var statisticsRelease = new Data.Model.Release
            {
                Id = contentRelease.Id
            };

            var published = DateTime.UtcNow;

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await contentDbContext.Releases.AddAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.Release.AddAsync(statisticsRelease);
                await statisticsDbContext.SaveChangesAsync();
            }

            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService.Setup(mock =>
                    mock.SetPublishedDatesByPublication(contentRelease.PublicationId, published))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    methodologyService: methodologyService.Object);

                await service.SetPublishedDates(contentRelease.Id, published);

                VerifyAllMocks(methodologyService);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var actualContentRelease = await contentDbContext
                    .Releases
                    .Include(r => r.Publication)
                    .SingleAsync(r => r.Id == contentRelease.Id);
                
                var actualStatisticsRelease = await statisticsDbContext
                    .Release
                    .SingleAsync(r => r.Id == contentRelease.Id);

                Assert.Equal(published, actualContentRelease.Published);
                Assert.Equal(published, actualContentRelease.Publication.Published);
                Assert.Equal(published, actualStatisticsRelease.Published);

                Assert.True(actualContentRelease.DataLastPublished.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(actualContentRelease.DataLastPublished!.Value).Milliseconds, 0,
                    1500);
            }
        }

        [Fact]
        public async Task SetPublishedDates_ReleaseHasNoStatisticsData()
        {
            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                Publication = new Publication(),
                Version = 0
            };

            var published = DateTime.UtcNow;

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService.Setup(mock =>
                    mock.SetPublishedDatesByPublication(contentRelease.PublicationId, published))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext())
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    methodologyService: methodologyService.Object);

                await service.SetPublishedDates(contentRelease.Id, published);

                VerifyAllMocks(methodologyService);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var actualContentRelease = await contentDbContext.Releases
                    .Include(r => r.Publication)
                    .SingleAsync(r => r.Id == contentRelease.Id);

                Assert.NotNull(actualContentRelease);

                Assert.Equal(published, actualContentRelease.Published);
                Assert.Equal(published, actualContentRelease.Publication.Published);

                Assert.True(actualContentRelease.DataLastPublished.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(actualContentRelease.DataLastPublished!.Value).Milliseconds, 0,
                    1500);
            }
        }

        [Fact]
        public async Task SetPublishedDates_AmendedReleaseHasPublishedDateOfPreviousVersion()
        {
            var publication = new Publication();

            var previousContentRelease = new Release
            {
                Id = Guid.NewGuid(),
                Publication = publication,
                Published = DateTime.UtcNow.AddDays(-1),
                PreviousVersionId = null,
                Version = 0
            };

            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = previousContentRelease.Id,
                Publication = publication,
                Version = 1
            };

            var statisticsRelease = new Data.Model.Release
            {
                Id = contentRelease.Id
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await contentDbContext.Publications.AddAsync(publication);
                await contentDbContext.Releases.AddRangeAsync(previousContentRelease, contentRelease);
                await contentDbContext.SaveChangesAsync();

                await statisticsDbContext.Release.AddAsync(statisticsRelease);
                await statisticsDbContext.SaveChangesAsync();
            }

            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService.Setup(mock =>
                    mock.SetPublishedDatesByPublication(contentRelease.PublicationId, previousContentRelease.Published.Value))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    methodologyService: methodologyService.Object);

                await service.SetPublishedDates(contentRelease.Id, DateTime.UtcNow);

                VerifyAllMocks(methodologyService);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var actualContentRelease = await contentDbContext
                    .Releases
                    .Include(r => r.Publication)
                    .SingleAsync(r => r.Id == contentRelease.Id);
                
                var actualStatisticsRelease = await statisticsDbContext
                    .Release
                    .SingleAsync(r => r.Id == contentRelease.Id);

                Assert.Equal(previousContentRelease.Published.Value, actualContentRelease.Published);
                Assert.Equal(previousContentRelease.Published.Value, actualContentRelease.Publication.Published);
                Assert.Equal(previousContentRelease.Published.Value, actualStatisticsRelease.Published);

                Assert.True(actualContentRelease.DataLastPublished.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(actualContentRelease.DataLastPublished!.Value).Milliseconds, 0,
                    1500);
            }
        }

        private static ReleaseService BuildReleaseService(
            ContentDbContext? contentDbContext = null,
            StatisticsDbContext? statisticsDbContext = null,
            IMethodologyService? methodologyService = null,
            IReleaseSubjectRepository? releaseSubjectRepository = null)
        {
            return new(
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(Strict),
                methodologyService ?? Mock.Of<IMethodologyService>(Strict),
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict));
        }
    }
}
