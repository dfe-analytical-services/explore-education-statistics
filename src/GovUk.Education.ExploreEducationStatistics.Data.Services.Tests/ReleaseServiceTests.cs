using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Database.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Database.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class ReleaseServiceTests
    {
        [Fact]
        public async Task GetRelease()
        {
            var statisticsRelease = new Data.Model.Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
                MetaGuidance = "Guidance 1"

            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
                MetaGuidance = "Guidance 2"
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                },
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 2",
                File = new File
                {
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
                HighlightDescription = "Test highlight description",
                Query = new ObservationQueryContext
                {
                    SubjectId = releaseSubject1.Subject.Id,
                }
            };

            var releaseContentBlock = new ReleaseContentBlock
            {
                Release = contentRelease,
                ContentBlock = dataBlock,
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.AddRangeAsync(import1, import2);
                await contentDbContext.AddAsync(releaseContentBlock);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>();

                metaGuidanceSubjectService
                    .Setup(s => s.GetTimePeriods(releaseSubject1.SubjectId))
                    .ReturnsAsync(new TimePeriodLabels("2020/21", "2021/22"));

                metaGuidanceSubjectService
                    .Setup(s => s.GetTimePeriods(releaseSubject2.SubjectId))
                    .ReturnsAsync(new TimePeriodLabels("2030", "2031"));

                metaGuidanceSubjectService
                    .Setup(s => s.GetGeographicLevels(releaseSubject1.SubjectId))
                    .ReturnsAsync(new List<string>
                    {
                        "Local Authority",
                        "Local Authority District"
                    });

                metaGuidanceSubjectService
                    .Setup(s => s.GetGeographicLevels(releaseSubject2.SubjectId))
                    .ReturnsAsync(new List<string>
                    {
                        "National"
                    });

                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object
                );

                var result = await service.GetRelease(contentRelease.Id);

                MockUtils.VerifyAllMocks(metaGuidanceSubjectService);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.Id);

                var subjects = result.Right.Subjects;

                Assert.NotNull(subjects);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseFile1.Name, subjects[0].Name);
                Assert.Equal(releaseSubject1.MetaGuidance, subjects[0].Content);

                Assert.Equal("2020/21", subjects[0].TimePeriods.From);
                Assert.Equal("2021/22", subjects[0].TimePeriods.To);

                Assert.Equal(2, subjects[0].GeographicLevels.Count);
                Assert.Equal("Local Authority", subjects[0].GeographicLevels[0]);
                Assert.Equal("Local Authority District", subjects[0].GeographicLevels[1]);

                Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
                Assert.Equal(releaseFile2.Name, subjects[1].Name);
                Assert.Equal(releaseSubject2.MetaGuidance, subjects[1].Content);

                Assert.Equal("2030", subjects[1].TimePeriods.From);
                Assert.Equal("2031", subjects[1].TimePeriods.To);

                Assert.Single(subjects[1].GeographicLevels);
                Assert.Equal("National", subjects[1].GeographicLevels[0]);

                var highlights = result.Right.Highlights;

                Assert.NotNull(highlights);
                Assert.Single(highlights);

                Assert.Equal(dataBlock.Id, highlights[0].Id);
                Assert.Equal(dataBlock.HighlightName, highlights[0].Name);
                Assert.Equal(dataBlock.HighlightDescription, highlights[0].Description);
            }
        }

        [Fact]
        public async Task GetRelease_FiltersPendingReplacementSubjects()
        {
            var statisticsRelease = new Data.Model.Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
            };

            var releaseSubject2Replacement = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
            };
            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2, releaseSubject2Replacement);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject1.Subject.Id
                }
            };

            var file2 = new File
            {
                Filename = "data2.csv",
                Type = FileType.Data,
                SubjectId = releaseSubject2.Subject.Id,
            };

            var file2Replacement = new File
            {
                Filename = "data2_replacement.csv",
                Type = FileType.Data,
                SubjectId =  releaseSubject2Replacement.Subject.Id,
                Replacing = file2
            };

            file2.ReplacedBy = file2Replacement;

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 2",
                File = file2
            };

            var releaseFile2Replacement = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 2 Replacement",
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
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2, releaseFile2Replacement);
                await contentDbContext.AddRangeAsync(import1, import2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.GetRelease(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.Id);

                var subjects = result.Right.Subjects;

                Assert.NotNull(subjects);
                Assert.Equal(2, subjects.Count);
                Assert.Equal(releaseSubject1.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseFile1.Name, subjects[0].Name);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[1].Id);
                Assert.Equal(releaseFile2.Name, subjects[1].Name);
            }
        }

        [Fact]
        public async Task GetRelease_FiltersImportingSubjects()
        {
            var statisticsRelease = new Data.Model.Release();

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject
                {
                    Id = Guid.NewGuid()
                }
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
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
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.AddRangeAsync(import1, import2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.GetRelease(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.Id);

                var subjects = result.Right.Subjects;

                Assert.Single(subjects);
                Assert.Equal(releaseSubject2.Subject.Id, subjects[0].Id);
                Assert.Equal(releaseFile2.Name, subjects[0].Name);
            }
        }

        [Fact]
        public async Task GetRelease_FiltersSubjectsWithNoImport()
        {
            var statisticsRelease = new Data.Model.Release();
            var releaseSubject = new ReleaseSubject
            {
                Release = statisticsRelease,
                Subject = new Subject(),
            };

            var statisticsDbContextId = Guid.NewGuid().ToString();
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(releaseSubject);
                await statisticsDbContext.SaveChangesAsync();
            }

            var contentRelease = new Release
            {
                Id = statisticsRelease.Id,
            };

            var releaseFile = new ReleaseFile
            {
                Release = contentRelease,
                Name = "Subject 1",
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                    SubjectId = releaseSubject.Subject.Id
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(contentRelease);
                await contentDbContext.AddRangeAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.GetRelease(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.Id);

                var subjects = result.Right.Subjects;

                Assert.Empty(subjects);
            }
        }

        [Fact]
        public async Task GetRelease_FiltersSubjectsWithNoFileSubjectId()
        {
            var releaseId = Guid.NewGuid();

            var release = new Release
            {
                Id = releaseId
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "data1.csv",
                    Type = FileType.Data,
                }
            };

            var import = new DataImport
            {
                File = releaseFile.File,
                Status = DataImportStatus.COMPLETE
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(releaseFile);
                await contentDbContext.AddRangeAsync(import);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildReleaseService(contentDbContext: contentDbContext);

                var result = await service.GetRelease(release.Id);

                Assert.True(result.IsRight);

                Assert.Equal(release.Id, result.Right.Id);

                var subjects = result.Right.Subjects;

                Assert.Empty(subjects);
            }
        }

        [Fact]
        public async Task GetRelease_FiltersHighlights()
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
                    Id = Guid.NewGuid()
                }
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
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
                HighlightName = "Test highlight name 1",
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
                HighlightName = "Test highlight name 3",
                HighlightDescription = "Test highlight description 3",
                Query = new ObservationQueryContext
                {
                    SubjectId = Guid.NewGuid(),
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = BuildReleaseService(
                    contentDbContext: contentDbContext,
                    statisticsDbContext: statisticsDbContext
                );

                var result = await service.GetRelease(contentRelease.Id);

                Assert.True(result.IsRight);

                Assert.Equal(contentRelease.Id, result.Right.Id);

                var highlights = result.Right.Highlights;

                Assert.NotNull(highlights);
                Assert.Single(highlights);

                Assert.Equal(dataBlock1.Id, highlights[0].Id);
                Assert.Equal(dataBlock1.HighlightName, highlights[0].Name);
                Assert.Equal(dataBlock1.HighlightDescription, highlights[0].Description);
            }
        }

        private static ReleaseService BuildReleaseService(
            ContentDbContext contentDbContext = null,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            StatisticsDbContext statisticsDbContext = null,
            IUserService userService = null,
            IMetaGuidanceSubjectService metaGuidanceSubjectService = null)
        {
            return new (
                contentDbContext ?? Mock.Of<ContentDbContext>(),
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                metaGuidanceSubjectService ?? Mock.Of<IMetaGuidanceSubjectService>()
            );
        }
    }
}
