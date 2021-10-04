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
                    TimePeriods = new TimePeriodLabels("2020/21 Q3", "2021/22 Q1"),
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

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id, new List<Guid>
                {
                    releaseFile.File.SubjectId.Value
                })).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.Get(release.Id);

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id, new List<Guid>
                    {
                        releaseFile.File.SubjectId.Value
                    }), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Release Meta Guidance", result.Right.Content);
                Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
            }
        }

        [Fact]
        public async Task Get_ReplacementInProgressIsIgnored()
        {
            var release = new Release
            {
                MetaGuidance = "Release Meta Guidance"
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

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id, new List<Guid>
                {
                    originalFile.File.SubjectId.Value
                })).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.Get(release.Id);

                Assert.True(result.IsRight);

                // The Subject id of the replacement file should not be included

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id, new List<Guid>
                    {
                        originalFile.File.SubjectId.Value
                    }), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Release Meta Guidance", result.Right.Content);
                Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
            }
        }

        [Fact]
        public async Task Get_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext);

                var result = await service.Get(Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Update_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext);

                var result = await service.Update(
                    Guid.NewGuid(),
                    new MetaGuidanceUpdateViewModel
                    {
                        Content = "Updated Release Meta Guidance",
                        Subjects = new List<MetaGuidanceUpdateSubjectViewModel>()
                    });

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Update_NoSubjects()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                MetaGuidance = "Release Meta Guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id, new List<Guid>())).ReturnsAsync(new List<MetaGuidanceSubjectViewModel>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object,
                    statisticsDbContext: statisticsDbContext);

                var result = await service.Update(
                    release.Id,
                    new MetaGuidanceUpdateViewModel
                    {
                        Content = "Updated Release Meta Guidance",
                        Subjects = new List<MetaGuidanceUpdateSubjectViewModel>()
                    });

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id, new List<Guid>()), Times.Once);

                Assert.Equal(release.Id, result.Right.Id);
                Assert.Equal("Updated Release Meta Guidance", result.Right.Content);
                Assert.Empty(result.Right.Subjects);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Equal("Updated Release Meta Guidance",
                    (await contentDbContext.Releases.FindAsync(release.Id)).MetaGuidance);
            }
        }

        [Fact]
        public async Task Update_NoMatchingSubjects()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                MetaGuidance = "Release Meta Guidance"
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

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(release.Id, new List<Guid>
                {
                    releaseFile.File.SubjectId.Value
                })).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object,
                    statisticsDbContext: statisticsDbContext);

                var result = await service.Update(
                    release.Id,
                    new MetaGuidanceUpdateViewModel
                    {
                        Content = "Updated Release Meta Guidance",
                        Subjects = new List<MetaGuidanceUpdateSubjectViewModel>
                        {
                            new MetaGuidanceUpdateSubjectViewModel
                            {
                                Id = Guid.NewGuid(),
                                Content = "Not a valid subject"
                            }
                        }
                    });

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(release.Id, new List<Guid>
                    {
                        releaseFile.File.SubjectId.Value
                    }), Times.Once);

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
        public async Task Update_WithSubjects()
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
                await contentDbContext.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.AddRangeAsync(statsRelease);
                await statisticsDbContext.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(statsRelease.Id, new List<Guid>
                {
                    subject1.Id, subject2.Id
                })).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object,
                    statisticsDbContext: statisticsDbContext);

                // Update Release and Subject 1
                var result = await service.Update(
                    contentRelease.Id,
                    new MetaGuidanceUpdateViewModel
                    {
                        Content = "Release Meta Guidance Updated",
                        Subjects = new List<MetaGuidanceUpdateSubjectViewModel>
                        {
                            new MetaGuidanceUpdateSubjectViewModel
                            {
                                Id = releaseSubject1.Subject.Id,
                                Content = "Subject 1 Meta Guidance Updated"
                            }
                        }
                    }
                );

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(statsRelease.Id, new List<Guid>
                    {
                        subject1.Id, subject2.Id
                    }), Times.Once);

                Assert.Equal(contentRelease.Id, result.Right.Id);
                Assert.Equal("Release Meta Guidance Updated", result.Right.Content);
                Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                Assert.Equal("Release Meta Guidance Updated",
                    (await contentDbContext.Releases
                        .FindAsync(contentRelease.Id)).MetaGuidance);

                // Assert only one Subject has been updated
                Assert.Equal("Subject 1 Meta Guidance Updated",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsRelease.Id
                                     && rs.SubjectId == releaseSubject1.Subject.Id)
                        .FirstAsync()).MetaGuidance);

                Assert.Equal("Subject 2 Meta Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsRelease.Id
                                     && rs.SubjectId == releaseSubject2.Subject.Id)
                        .FirstAsync()).MetaGuidance);
            }
        }

        [Fact]
        public async Task Update_WithSubjects_AmendedRelease()
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

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock =>
                mock.GetSubjects(statsReleaseVersion2.Id, new List<Guid>
                {
                    subject1.Id, subject2.Id
                })).ReturnsAsync(SubjectMetaGuidance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object,
                    statisticsDbContext: statisticsDbContext);

                // Update Release and Subject 1 on version 2
                var result = await service.Update(
                    contentReleaseVersion2.Id,
                    new MetaGuidanceUpdateViewModel
                    {
                        Content = "Version 2 Release Meta Guidance Updated",
                        Subjects = new List<MetaGuidanceUpdateSubjectViewModel>
                        {
                            new MetaGuidanceUpdateSubjectViewModel
                            {
                                Id = subject1.Id,
                                Content = "Version 2 Subject 1 Meta Guidance Updated"
                            }
                        }
                    }
                );

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock =>
                    mock.GetSubjects(statsReleaseVersion2.Id, new List<Guid>
                    {
                        subject1.Id, subject2.Id
                    }), Times.Once);

                Assert.Equal(contentReleaseVersion2.Id, result.Right.Id);
                Assert.Equal("Version 2 Release Meta Guidance Updated", result.Right.Content);
                Assert.Equal(SubjectMetaGuidance, result.Right.Subjects);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                Assert.Equal("Version 1 Release Meta Guidance",
                    (await contentDbContext.Releases
                        .FindAsync(contentReleaseVersion1.Id)).MetaGuidance);

                Assert.Equal("Version 2 Release Meta Guidance Updated",
                    (await contentDbContext.Releases
                        .FindAsync(contentReleaseVersion2.Id)).MetaGuidance);

                // Assert the same Subject on version 1 hasn't been affected
                Assert.Equal("Version 1 Subject 1 Meta Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsReleaseVersion1.Id && rs.SubjectId == subject1.Id)
                        .FirstAsync()).MetaGuidance);

                // Assert only one Subject on version 2 has been updated
                Assert.Equal("Version 2 Subject 1 Meta Guidance Updated",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsReleaseVersion2.Id && rs.SubjectId == subject1.Id)
                        .FirstAsync()).MetaGuidance);

                Assert.Equal("Version 2 Subject 2 Meta Guidance",
                    (await statisticsDbContext.ReleaseSubject
                        .AsQueryable()
                        .Where(rs => rs.ReleaseId == statsReleaseVersion2.Id && rs.SubjectId == subject2.Id)
                        .FirstAsync()).MetaGuidance);
            }
        }

        [Fact]
        public async Task Validate_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.Validate(Guid.NewGuid());

                result.AssertNotFound();
            }
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

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);
            }
        }

        [Fact]
        public async Task Validate_MetaGuidancePopulated()
        {
            var release = new Release
            {
                MetaGuidance = "Release Meta Guidance"
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

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock => mock.Validate(release.Id))
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.Validate(release.Id);

                Assert.True(result.IsRight);

                metaGuidanceSubjectService.Verify(mock => mock.Validate(release.Id), Times.Once);
            }
        }

        [Fact]
        public async Task Validate_ReleaseMetaGuidanceNotPopulated()
        {
            var release = new Release
            {
                MetaGuidance = null
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

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.Validate(release.Id);

                result.AssertBadRequest(PublicMetaGuidanceRequired);
            }
        }

        [Fact]
        public async Task Validate_SubjectMetaGuidanceNotPopulated()
        {
            var release = new Release
            {
                MetaGuidance = "Release Meta Guidance"
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

            var metaGuidanceSubjectService = new Mock<IMetaGuidanceSubjectService>(MockBehavior.Strict);

            metaGuidanceSubjectService.Setup(mock => mock.Validate(release.Id))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMetaGuidanceService(contentDbContext: contentDbContext,
                    metaGuidanceSubjectService: metaGuidanceSubjectService.Object);

                var result = await service.Validate(release.Id);
                metaGuidanceSubjectService.Verify(mock => mock.Validate(release.Id), Times.Once);
                result.AssertBadRequest(PublicMetaGuidanceRequired);
            }
        }

        private static MetaGuidanceService SetupMetaGuidanceService(
            ContentDbContext contentDbContext,
            StatisticsDbContext statisticsDbContext = null,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IMetaGuidanceSubjectService metaGuidanceSubjectService = null,
            IUserService userService = null,
            IReleaseDataFileRepository releaseDataFileRepository = null)
        {
            return new MetaGuidanceService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                metaGuidanceSubjectService ?? new Mock<IMetaGuidanceSubjectService>().Object,
                statisticsDbContext ?? new Mock<StatisticsDbContext>().Object,
                userService ?? MockUtils.AlwaysTrueUserService().Object,
                releaseDataFileRepository ?? new ReleaseDataFileRepository(contentDbContext)
            );
        }
    }
}
