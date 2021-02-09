using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
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
        public async Task GetSubjectsMeta()
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

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };

            var import2 = new DataImport
            {
                File = releaseFile2.File,
                Status = DataImportStatus.COMPLETE
            };

            var dataBlock = new DataBlock
            {
                Name = "Test data block",
                HighlightName = "Test highlight name",
                Query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject1.Subject.Id,
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.AddRangeAsync(import1, import2);
                await contentDbContext.AddAsync(
                    new ReleaseContentBlock
                    {
                        Release = contentRelease,
                        ContentBlock = dataBlock
                    }
                );
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
                var replacementService = BuildReleaseMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await replacementService.GetSubjectsMeta(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.ReleaseId);

                var subjects = result.Right.Subjects;

                Assert.NotNull(subjects);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseSubject1.Subject.Name, subjects[0].Label);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
                Assert.Equal(releaseSubject2.Subject.Name, subjects[1].Label);

                var highlights = result.Right.Highlights;

                Assert.NotNull(highlights);
                Assert.Single(highlights);

                Assert.Equal(dataBlock.Id, highlights[0].Id);
                Assert.Equal(dataBlock.HighlightName, highlights[0].Label);
            }
        }

        [Fact]
        public async Task GetSubjectsMeta_FiltersPendingReplacementSubjects()
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

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };

            var import2 = new DataImport
            {
                File = releaseFile2.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2, releaseFile2Replacement);
                await contentDbContext.AddRangeAsync(import1, import2);
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
                var replacementService = BuildReleaseMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await replacementService.GetSubjectsMeta(contentRelease.Id);

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
        public async Task GetSubjectsMeta_FiltersImportingSubjects()
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

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.STAGE_1
            };

            var import2 = new DataImport
            {
                File = releaseFile2.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.AddRangeAsync(import1, import2);
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
                var replacementService = BuildReleaseMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await replacementService.GetSubjectsMeta(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.ReleaseId);

                var subjects = result.Right.Subjects;

                Assert.Single(subjects);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseSubject2.Subject.Name, subjects[0].Label);
            }
        }

        [Fact]
        public async Task GetSubjectsMeta_FiltersHighlights()
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

            var import1 = new DataImport
            {
                File = releaseFile1.File,
                Status = DataImportStatus.COMPLETE
            };

            // Has highlight name and subject matches
            var dataBlock1 = new DataBlock
            {
                Name = "Test data block 1",
                HighlightName = "Test highlight name",
                Query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject1.Subject.Id,
                }
            };

            // No highlight name
            var dataBlock2 = new DataBlock
            {
                Name = "Test data block 2",
                Query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject1.Subject.Id,
                }
            };

            // Subject does not match
            var dataBlock3 = new DataBlock
            {
                Name = "Test data block 3",
                HighlightName = "Test highlight name",
                Query = new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid(),
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddAsync(releaseFile1);
                await contentDbContext.AddAsync(import1);
                await contentDbContext.AddRangeAsync(
                    new ReleaseContentBlock
                    {
                        Release = contentRelease,
                        ContentBlock = dataBlock1
                    },
                    new ReleaseContentBlock
                    {
                        Release = contentRelease,
                        ContentBlock = dataBlock2
                    },
                    new ReleaseContentBlock
                    {
                        Release = contentRelease,
                        ContentBlock = dataBlock3
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddAsync(releaseSubject1);
                await statisticsDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var replacementService = BuildReleaseMetaService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await replacementService.GetSubjectsMeta(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.ReleaseId);

                var highlights = result.Right.Highlights;

                Assert.NotNull(highlights);
                Assert.Single(highlights);

                Assert.Equal(dataBlock1.Id, highlights[0].Id);
                Assert.Equal(dataBlock1.HighlightName, highlights[0].Label);
            }
        }

        private static ReleaseMetaService BuildReleaseMetaService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            StatisticsDbContext statisticsDbContext = null,
            IDataImportRepository dataImportRepository = null,
            IUserService userService = null)
        {
            return new ReleaseMetaService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                dataImportRepository ?? new DataImportRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}