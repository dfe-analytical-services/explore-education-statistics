#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataGuidanceServiceTests
    {
        private static readonly List<DataGuidanceSubjectViewModel> DataGuidanceSubjects =
            new()
            {
                new DataGuidanceSubjectViewModel
                {
                    Id = Guid.NewGuid(),
                    Content = "Subject Guidance",
                    Filename = "data.csv",
                    Name = "Subject",
                    GeographicLevels = new List<string>
                    {
                        "National", "Local authority", "Local authority district"
                    },
                    TimePeriods = new TimePeriodLabels("2020/21 Q3", "2021/22 Q1"),
                    Variables = new List<LabelValue>
                    {
                        new("Filter label", "test_filter"),
                        new("Indicator label", "test_indicator")
                    }
                }
            };

        [Fact]
        public async Task Get()
        {
            var release = new Release
            {
                DataGuidance = "Release Guidance"
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id, new List<Guid>
                {
                    releaseFile.File.SubjectId.Value
                })).ReturnsAsync(DataGuidanceSubjects);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object);

                var result = await service.Get(release.Id);

                Assert.True(result.IsRight);

                dataGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id, new List<Guid>
                    {
                        releaseFile.File.SubjectId.Value
                    }), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Release Guidance", result.Right.Content);
                Assert.Equal(DataGuidanceSubjects, result.Right.Subjects);
            }
        }

        [Fact]
        public async Task Get_ReplacementInProgressIsIgnored()
        {
            var release = new Release
            {
                DataGuidance = "Release Guidance"
            };

            var originalFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var replacementFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid(),
                    Replacing = originalFile.File
                }
            };

           originalFile.File.ReplacedBy = replacementFile.File;

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(originalFile, replacementFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id, new List<Guid>
                {
                    originalFile.File.SubjectId.Value
                })).ReturnsAsync(DataGuidanceSubjects);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object);

                var result = await service.Get(release.Id);

                Assert.True(result.IsRight);

                // The Subject id of the replacement file should not be included

                dataGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id, new List<Guid>
                    {
                        originalFile.File.SubjectId.Value
                    }), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Release Guidance", result.Right.Content);
                Assert.Equal(DataGuidanceSubjects, result.Right.Subjects);
            }
        }

        [Fact]
        public async Task Get_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.Get(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task Update_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.Update(
                Guid.NewGuid(),
                new DataGuidanceUpdateViewModel
                {
                    Content = "Updated Release Guidance",
                    Subjects = new List<DataGuidanceUpdateSubjectViewModel>()
                });

            result.AssertNotFound();
        }

        [Fact]
        public async Task Update_NoSubjects()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                DataGuidance = "Release Guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id, new List<Guid>())).ReturnsAsync(new List<DataGuidanceSubjectViewModel>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object,
                    statisticsDbContext: statisticsDbContext);

                var result = await service.Update(
                    release.Id,
                    new DataGuidanceUpdateViewModel
                    {
                        Content = "Updated Release Guidance",
                        Subjects = new List<DataGuidanceUpdateSubjectViewModel>()
                    });

                Assert.True(result.IsRight);

                dataGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id, new List<Guid>()), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Updated Release Guidance", result.Right.Content);
                Assert.Empty(result.Right.Subjects);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Equal("Updated Release Guidance",
                    (await contentDbContext.Releases.FindAsync(release.Id))?.DataGuidance);
            }
        }

        [Fact]
        public async Task Update_NoMatchingSubjects()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                DataGuidance = "Release Guidance"
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id, new List<Guid>
                {
                    releaseFile.File.SubjectId.Value
                })).ReturnsAsync(DataGuidanceSubjects);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object,
                    statisticsDbContext: statisticsDbContext);

                var result = await service.Update(
                    release.Id,
                    new DataGuidanceUpdateViewModel
                    {
                        Content = "Updated Release Guidance",
                        Subjects = new List<DataGuidanceUpdateSubjectViewModel>
                        {
                            new DataGuidanceUpdateSubjectViewModel
                            {
                                Id = Guid.NewGuid(),
                                Content = "Not a valid subject"
                            }
                        }
                    });

                Assert.True(result.IsRight);

                dataGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id, new List<Guid>
                    {
                        releaseFile.File.SubjectId.Value
                    }), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Updated Release Guidance", result.Right.Content);
                Assert.Equal(DataGuidanceSubjects, result.Right.Subjects);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Equal("Updated Release Guidance",
                    (await contentDbContext.Releases.FindAsync(release.Id))?.DataGuidance);
            }
        }

        [Fact]
        public async Task Update_WithSubjects()
        {
            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                DataGuidance = "Release Guidance"
            };

            var statsRelease = new Data.Model.Release
            {
                Id = contentRelease.Id,
            };

            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = subject1.Id
                }
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = contentRelease,
                File = new File
                {
                    Filename = "file2.csv",
                    Type = FileType.Data,
                    SubjectId = subject2.Id
                }
            };

            var releaseSubject1 = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = subject1,
                DataGuidance = "Subject 1 Guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = subject2,
                DataGuidance = "Subject 2 Guidance"
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
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(statsRelease.Id, new List<Guid>
                {
                    subject1.Id, subject2.Id
                })).ReturnsAsync(DataGuidanceSubjects);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object,
                    statisticsDbContext: statisticsDbContext);

                // Update Release and Subject 1
                var result = await service.Update(
                    contentRelease.Id,
                    new DataGuidanceUpdateViewModel
                    {
                        Content = "Release Guidance Updated",
                        Subjects = new List<DataGuidanceUpdateSubjectViewModel>
                        {
                            new DataGuidanceUpdateSubjectViewModel
                            {
                                Id = releaseSubject1.Subject.Id,
                                Content = "Subject 1 Guidance Updated"
                            }
                        }
                    }
                );

                Assert.True(result.IsRight);

                dataGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(statsRelease.Id, new List<Guid>
                    {
                        subject1.Id, subject2.Id
                    }), Times.Once);

                Assert.Equal(contentRelease.Id, result.Right.Id);
                Assert.Equal("Release Guidance Updated", result.Right.Content);
                Assert.Equal(DataGuidanceSubjects, result.Right.Subjects);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                Assert.Equal("Release Guidance Updated",
                    (await contentDbContext.Releases
                        .FindAsync(contentRelease.Id))?.DataGuidance);

                // Assert only one Subject has been updated
                Assert.Equal("Subject 1 Guidance Updated",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsRelease.Id
                                     && rs.SubjectId == releaseSubject1.Subject.Id)
                        .FirstAsync()).DataGuidance);

                Assert.Equal("Subject 2 Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsRelease.Id
                                     && rs.SubjectId == releaseSubject2.Subject.Id)
                        .FirstAsync()).DataGuidance);
            }
        }

        [Fact]
        public async Task Update_WithSubjects_AmendedRelease()
        {
            var contentReleaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                DataGuidance = "Version 1 Release Guidance"
            };

            var contentReleaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id,
                DataGuidance = "Version 2 Release Guidance"
            };

            var statsReleaseVersion1 = new Data.Model.Release
            {
                Id = contentReleaseVersion1.Id
            };

            var statsReleaseVersion2 = new Data.Model.Release
            {
                Id = contentReleaseVersion2.Id
            };

            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
            };

            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
            };

            // Version 1 has one Subject, version 2 adds another Subject

            var releaseVersion1File1 = new ReleaseFile
            {
                Release = contentReleaseVersion1,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = subject1.Id
                }
            };

            var releaseVersion2File1 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = subject1.Id
                }
            };

            var releaseVersion2File2 = new ReleaseFile
            {
                Release = contentReleaseVersion2,
                File = new File
                {
                    Filename = "file2.csv",
                    Type = FileType.Data,
                    SubjectId = subject2.Id
                }
            };

            var releaseVersion1Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion1,
                Subject = subject1,
                DataGuidance = "Version 1 Subject 1 Guidance"
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject1,
                DataGuidance = "Version 2 Subject 1 Guidance"
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject2,
                DataGuidance = "Version 2 Subject 2 Guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.AddRangeAsync(releaseVersion1File1, releaseVersion2File1,
                    releaseVersion2File2);
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

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(statsReleaseVersion2.Id, new List<Guid>
                {
                    subject1.Id, subject2.Id
                })).ReturnsAsync(DataGuidanceSubjects);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object,
                    statisticsDbContext: statisticsDbContext);

                // Update Release and Subject 1 on version 2
                var result = await service.Update(
                    contentReleaseVersion2.Id,
                    new DataGuidanceUpdateViewModel
                    {
                        Content = "Version 2 Release Guidance Updated",
                        Subjects = new List<DataGuidanceUpdateSubjectViewModel>
                        {
                            new DataGuidanceUpdateSubjectViewModel
                            {
                                Id = subject1.Id,
                                Content = "Version 2 Subject 1 Guidance Updated"
                            }
                        }
                    }
                );

                Assert.True(result.IsRight);

                dataGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(statsReleaseVersion2.Id, new List<Guid>
                    {
                        subject1.Id, subject2.Id
                    }), Times.Once);

                Assert.Equal(contentReleaseVersion2.Id, result.Right.Id);
                Assert.Equal("Version 2 Release Guidance Updated", result.Right.Content);
                Assert.Equal(DataGuidanceSubjects, result.Right.Subjects);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                Assert.Equal("Version 1 Release Guidance",
                    (await contentDbContext.Releases
                        .FindAsync(contentReleaseVersion1.Id))?.DataGuidance);

                Assert.Equal("Version 2 Release Guidance Updated",
                    (await contentDbContext.Releases
                        .FindAsync(contentReleaseVersion2.Id))?.DataGuidance);

                // Assert the same Subject on version 1 hasn't been affected
                Assert.Equal("Version 1 Subject 1 Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsReleaseVersion1.Id && rs.SubjectId == subject1.Id)
                        .FirstAsync()).DataGuidance);

                // Assert only one Subject on version 2 has been updated
                Assert.Equal("Version 2 Subject 1 Guidance Updated",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsReleaseVersion2.Id && rs.SubjectId == subject1.Id)
                        .FirstAsync()).DataGuidance);

                Assert.Equal("Version 2 Subject 2 Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsReleaseVersion2.Id && rs.SubjectId == subject2.Id)
                        .FirstAsync()).DataGuidance);
            }
        }

        [Fact]
        public async Task Validate_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var service = SetupService(contentDbContext: contentDbContext,
                dataGuidanceSubjectService: dataGuidanceSubjectService.Object);

            var result = await service.Validate(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task Validate_NoDataFiles()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task Validate_DataGuidancePopulated()
        {
            var release = new Release
            {
                DataGuidance = "Release Guidance"
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService.Setup(mock => mock.Validate(release.Id))
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);

                dataGuidanceSubjectService.Verify(mock => mock.Validate(release.Id), Times.Once);
            }
        }

        [Fact]
        public async Task Validate_ReleaseDataGuidanceNotPopulated()
        {
            var release = new Release
            {
                DataGuidance = null
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object);

                var result = await service.Validate(release.Id);

                result.AssertBadRequest(PublicDataGuidanceRequired);
            }
        }

        [Fact]
        public async Task Validate_SubjectDataGuidanceNotPopulated()
        {
            var release = new Release
            {
                DataGuidance = "Release Guidance"
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceSubjectService = new Mock<IDataGuidanceSubjectService>(MockBehavior.Strict);

            dataGuidanceSubjectService.Setup(mock => mock.Validate(release.Id))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceSubjectService: dataGuidanceSubjectService.Object);

                var result = await service.Validate(release.Id);
                dataGuidanceSubjectService.Verify(mock => mock.Validate(release.Id), Times.Once);
                result.AssertBadRequest(PublicDataGuidanceRequired);
            }
        }

        private static DataGuidanceService SetupService(
            ContentDbContext contentDbContext,
            StatisticsDbContext? statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IDataGuidanceSubjectService? dataGuidanceSubjectService = null,
            IUserService? userService = null,
            IReleaseDataFileRepository? releaseDataFileRepository = null)
        {
            return new DataGuidanceService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                dataGuidanceSubjectService ?? Mock.Of<IDataGuidanceSubjectService>(),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                releaseDataFileRepository ?? new ReleaseDataFileRepository(contentDbContext)
            );
        }
    }
}
