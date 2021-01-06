using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseMetaServiceTests
    {
        [Fact]
        public async Task GetSubjects()
        {
            var releaseId = Guid.NewGuid();

            var contentRelease = new Release
            {
                Id = releaseId
            };

            var statisticsRelease = new Data.Model.Release
            {
                Id = releaseId
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    Name = "Subject 1"
                }
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    Name = "Subject 2"
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Release = contentRelease,
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Release = contentRelease,
                    Filename = "data2.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject2.Subject.Id,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(It.IsAny<Guid>(), It.IsAny<string>()))
                    .ReturnsAsync(
                        new ImportStatus()
                        {
                            Status = IStatus.COMPLETE
                        }
                    );

                var replacementService = BuildReleaseMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    importStatusService: importStatusService.Object
                );

                var result = await replacementService.GetSubjects(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.ReleaseId);

                var subjects = result.Right.Subjects;

                Assert.NotNull(subjects);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseSubject1.Subject.Name, subjects[0].Label);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
                Assert.Equal(releaseSubject2.Subject.Name, subjects[1].Label);
            }
        }

        [Fact]
        public async Task GetSubjects_FiltersPendingReplacementSubjects()
        {
            var releaseId = Guid.NewGuid();

            var contentRelease = new Release
            {
                Id = releaseId
            };

            var statisticsRelease = new Data.Model.Release
            {
                Id = releaseId
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    Name = "Subject 1"
                }
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    Name = "Subject 2"
                }
            };

            var releaseSubject2Replacement = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    Name = "Subject 2 Replacement"
                }
            };

            var file2 = new File
            {
                Release = contentRelease,
                Filename = "data2.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject2.Subject.Id,
            };

            var file2Replacement = new File
            {
                Release = contentRelease,
                Filename = "data2_replacement.csv",
                Type = FileType.Data,
                SubjectId =  releaseSubject2Replacement.Subject.Id,
                Replacing = file2
            };

            file2.ReplacedBy = file2Replacement;

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Release = contentRelease,
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                File = file2
            };

            var releaseFile2Replacement = new ReleaseFile
            {
                Release = contentRelease,
                File = file2Replacement
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2, releaseFile2Replacement);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject2Replacement);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s => s.GetImportStatus(It.IsAny<Guid>(), It.IsAny<string>()))
                    .ReturnsAsync(
                        new ImportStatus()
                        {
                            Status = IStatus.COMPLETE
                        }
                    );

                var replacementService = BuildReleaseMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    importStatusService: importStatusService.Object
                );

                var result = await replacementService.GetSubjects(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.ReleaseId);

                var subjects = result.Right.Subjects;

                Assert.NotNull(subjects);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseSubject1.Subject.Name, subjects[0].Label);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
                Assert.Equal(releaseSubject2.Subject.Name, subjects[1].Label);
            }
        }

        [Fact]
        public async Task GetSubjects_FiltersImportingSubjects()
        {
            var releaseId = Guid.NewGuid();

            var contentRelease = new Release
            {
                Id = releaseId
            };

            var statisticsRelease = new Data.Model.Release
            {
                Id = releaseId
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    Name = "Subject 1"
                }
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid(),
                    Name = "Subject 2"
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Release = contentRelease,
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Release = contentRelease,
                    Filename = "data2.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject2.Subject.Id,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var importStatusService = new Mock<IImportStatusService>();

                importStatusService
                    .Setup(s =>
                        s.GetImportStatus(releaseSubject1.ReleaseId, releaseFile1.File.Filename))
                    .ReturnsAsync(
                        new ImportStatus()
                        {
                            Status = IStatus.STAGE_1
                        }
                    );

                importStatusService
                    .Setup(s =>
                        s.GetImportStatus(releaseSubject2.ReleaseId, releaseFile2.File.Filename))
                    .ReturnsAsync(
                        new ImportStatus()
                        {
                            Status = IStatus.COMPLETE
                        }
                    );

                var replacementService = BuildReleaseMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    importStatusService: importStatusService.Object
                );

                var result = await replacementService.GetSubjects(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.ReleaseId);

                var subjects = result.Right.Subjects;

                Assert.Single(subjects);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseSubject2.Subject.Name, subjects[0].Label);
            }
        }

        private static ReleaseMetaService BuildReleaseMetaService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            StatisticsDbContext statisticsDbContext = null,
            IUserService userService = null,
            IImportStatusService importStatusService = null)
        {
            return new ReleaseMetaService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                importStatusService ?? new Mock<IImportStatusService>().Object
            );
        }
    }
}