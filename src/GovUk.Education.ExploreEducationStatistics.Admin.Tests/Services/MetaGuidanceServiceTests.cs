using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class MetaGuidanceServiceTests
    {
        private static readonly List<MetaGuidanceSubjectViewModel> SubjectMetaGuidance =
            new List<MetaGuidanceSubjectViewModel>
            {
                new MetaGuidanceSubjectViewModel
                {
                    Id = Guid.NewGuid(),
                    Content = "Subject Meta Guidance",
                    Filename = "data.csv",
                    Name = "Subject",
                    GeographicLevels = new List<string>
                    {
                        "National", "Local Authority", "Local Authority District"
                    },
                    TimePeriods = new MetaGuidanceSubjectTimePeriodsViewModel("2020_AYQ3", "2021_AYQ1"),
                    Variables = new List<LabelValue>
                    {
                        new LabelValue("Filter label", "test_filter"),
                        new LabelValue("Indicator label", "test_indicator")
                    }
                }
            };

        [Fact]
        public async Task Get()
        {
            var release = new Release
            {
                MetaGuidance = "Release Meta Guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id)).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.Get(release.Id);

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Release Meta Guidance", result.Right.Content);
                Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
            }
        }

        [Fact]
        public async Task UpdateRelease()
        {
            var release = new Release
            {
                MetaGuidance = "Release Meta Guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id)).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.UpdateRelease(release.Id, new MetaGuidanceUpdateReleaseViewModel
                {
                    Content = "Updated Release Meta Guidance"
                });

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Updated Release Meta Guidance", result.Right.Content);
                Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Equal("Updated Release Meta Guidance",
                    (await contentDbContext.Releases.FindAsync(release.Id)).MetaGuidance);
            }
        }

        [Fact]
        public async Task UpdateSubject()
        {
            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                MetaGuidance = "Release Meta Guidance"
            };

            var statsRelease = new Data.Model.Release
            {
                Id = contentRelease.Id,
            };

            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Filename = "file1.csv",
                Name = "Subject 1"
            };

            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Filename = "file2.csv",
                Name = "Subject 2"
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = subject1,
                MetaGuidance = "Subject 1 Meta Guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = subject2,
                MetaGuidance = "Subject 2 Meta Guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(statsRelease.Id)).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                // Update Subject 1
                var result = await service.UpdateSubject(
                    contentRelease.Id, subject1.Id, new MetaGuidanceUpdateSubjectViewModel
                    {
                        Content = "Subject 1 Meta Guidance Updated"
                    });

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(statsRelease.Id), Times.Once);

                Assert.Equal(contentRelease.Id, result.Right.Id);
                Assert.Equal("Release Meta Guidance", result.Right.Content);
                Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Assert only one Subject has been updated
                Assert.Equal("Subject 1 Meta Guidance Updated",
                    (await statisticsDbContext.ReleaseSubject
                        .Where(rs => rs.ReleaseId == statsRelease.Id && rs.SubjectId == subject1.Id)
                        .FirstAsync()).MetaGuidance);

                Assert.Equal("Subject 2 Meta Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .Where(rs => rs.ReleaseId == statsRelease.Id && rs.SubjectId == subject2.Id)
                        .FirstAsync()).MetaGuidance);
            }
        }

        [Fact]
        public async Task UpdateSubject_AmendedRelease()
        {
            var contentReleaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                MetaGuidance = "Version 1 Release Meta Guidance"
            };

            var contentReleaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id,
                MetaGuidance = "Version 2 Release Meta Guidance"
            };

            var statsReleaseVersion1 = new Data.Model.Release
            {
                Id = contentReleaseVersion1.Id,
                PreviousVersionId = contentReleaseVersion1.PreviousVersionId
            };

            var statsReleaseVersion2 = new Data.Model.Release
            {
                Id = contentReleaseVersion2.Id,
                PreviousVersionId = contentReleaseVersion2.PreviousVersionId
            };

            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Filename = "file1.csv",
                Name = "Subject 1"
            };

            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Filename = "file2.csv",
                Name = "Subject 2"
            };

            // Version 1 has one Subject, version 2 adds another Subject

            var releaseVersion1Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion1,
                Subject = subject1,
                MetaGuidance = "Version 1 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject1,
                MetaGuidance = "Version 2 Subject 1 Meta Guidance"
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject2,
                MetaGuidance = "Version 2 Subject 2 Meta Guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.AddRangeAsync(releaseVersion1Subject1, releaseVersion2Subject1,
                    releaseVersion2Subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(statsReleaseVersion2.Id)).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                // Update Subject 1 on version 2
                var result = await service.UpdateSubject(
                    contentReleaseVersion2.Id, subject1.Id, new MetaGuidanceUpdateSubjectViewModel
                    {
                        Content = "Version 2 Subject 1 Meta Guidance Updated"
                    });

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(statsReleaseVersion2.Id), Times.Once);

                Assert.Equal(contentReleaseVersion2.Id, result.Right.Id);
                Assert.Equal("Version 2 Release Meta Guidance", result.Right.Content);
                Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                // Assert the same Subject on version 1 hasn't been affected
                Assert.Equal("Version 1 Subject 1 Meta Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .Where(rs => rs.ReleaseId == statsReleaseVersion1.Id && rs.SubjectId == subject1.Id)
                        .FirstAsync()).MetaGuidance);

                // Assert only one Subject on version 2 has been updated
                Assert.Equal("Version 2 Subject 1 Meta Guidance Updated",
                    (await statisticsDbContext.ReleaseSubject
                        .Where(rs => rs.ReleaseId == statsReleaseVersion2.Id && rs.SubjectId == subject1.Id)
                        .FirstAsync()).MetaGuidance);

                Assert.Equal("Version 2 Subject 2 Meta Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .Where(rs => rs.ReleaseId == statsReleaseVersion2.Id && rs.SubjectId == subject2.Id)
                        .FirstAsync()).MetaGuidance);
            }
        }

        private static MetaGuidanceService SetupMetaGuidanceService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IMetaGuidanceSubjectService metaGuidanceSubjectService = null,
            IPersistenceHelper<StatisticsDbContext> statisticsPersistenceHelper = null,
            IUserService userService = null)
        {
            return new MetaGuidanceService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                metaGuidanceSubjectService ?? new Mock<IMetaGuidanceSubjectService>().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                statisticsPersistenceHelper ?? new PersistenceHelper<StatisticsDbContext>(statisticsDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}