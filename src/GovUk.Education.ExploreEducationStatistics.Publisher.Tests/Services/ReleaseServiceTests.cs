#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

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
                Assert.Equal("Academic year Q2 2018/19", result.Title);
            }
        }

        [Fact]
        public async Task SetPublishedDate_FirstVersion()
        {
            var release = new Release
            {
                PreviousVersionId = null,
                Version = 0
            };

            var actualPublishedDate = DateTime.UtcNow;

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext);
                await service.SetPublishedDate(release.Id, actualPublishedDate);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var actual = await contentDbContext
                    .Releases
                    .SingleAsync(r => r.Id == release.Id);

                // Expect the published date to have been updated with the actual published date
                Assert.Equal(actualPublishedDate, actual.Published);
            }
        }

        [Fact]
        public async Task SetPublishedDate_AmendedRelease()
        {
            var previousRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddDays(-1),
                PreviousVersionId = null,
                Version = 0
            };

            var release = new Release
            {
                PreviousVersionId = previousRelease.Id,
                Version = 1
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(previousRelease, release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext);
                await service.SetPublishedDate(release.Id, DateTime.UtcNow);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var actual = await contentDbContext
                    .Releases
                    .SingleAsync(r => r.Id == release.Id);

                // Expect the published date to have been copied from the previous version 
                Assert.Equal(previousRelease.Published, actual.Published);
            }
        }

        [Fact]
        public async Task SetPublishedDate_AmendedReleaseAndUpdatePublishedDateIsTrue()
        {
            var previousRelease = new Release
            {
                Id = Guid.NewGuid(),
                Published = DateTime.UtcNow.AddDays(-1),
                PreviousVersionId = null,
                Version = 0
            };

            var release = new Release
            {
                Published = null,
                PreviousVersionId = previousRelease.Id,
                Version = 1,
                UpdatePublishedDate = true
            };

            var actualPublishedDate = DateTime.UtcNow;

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(previousRelease, release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext);
                await service.SetPublishedDate(release.Id, actualPublishedDate);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var actual = await contentDbContext
                    .Releases
                    .SingleAsync(r => r.Id == release.Id);

                // Expect the published date to have been updated with the actual published date
                Assert.Equal(actualPublishedDate, actual.Published);
            }
        }

        private static ReleaseService BuildReleaseService(
            ContentDbContext? contentDbContext = null)
        {
            contentDbContext ??= InMemoryContentDbContext();

            return new(
                contentDbContext,
                releaseRepository: new ReleaseRepository(contentDbContext)
            );
        }
    }
}
