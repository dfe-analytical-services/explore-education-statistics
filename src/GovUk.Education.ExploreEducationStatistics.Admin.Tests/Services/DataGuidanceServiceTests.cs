#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
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
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using static Moq.MockBehavior;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class DataGuidanceServiceTests
    {
        private static readonly List<DataGuidanceDataSetViewModel> DataGuidanceDataSets =
            new()
            {
                new DataGuidanceDataSetViewModel
                {
                    FileId = Guid.NewGuid(),
                    Content = "Test data set guidance",
                    Filename = "data.csv",
                    Name = "Test data set",
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
        public async Task GetDataGuidance()
        {
            var release = new Release
            {
                DataGuidance = "Release guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

            dataGuidanceDataSetService.Setup(s => s.ListDataSets(release.Id, null, default))
                .ReturnsAsync(DataGuidanceDataSets);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceDataSetService: dataGuidanceDataSetService.Object);

                var result = await service.GetDataGuidance(release.Id);

                VerifyAllMocks(dataGuidanceDataSetService);

                var viewModel = result.AssertRight();

                Assert.Equal(release.Id, viewModel.Id);
                Assert.Equal("Release guidance", viewModel.Content);
                Assert.Equal(DataGuidanceDataSets, viewModel.DataSets);
            }
        }

        [Fact]
        public async Task GetDataGuidance_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.GetDataGuidance(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task UpdateDataGuidance_NoRelease()
        {
            var contentDbContextId = Guid.NewGuid().ToString();

            await using var contentDbContext = InMemoryApplicationDbContext(contentDbContextId);
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.UpdateDataGuidance(
                Guid.NewGuid(),
                new DataGuidanceUpdateRequest
                {
                    Content = "Updated release guidance",
                    DataSets = new List<DataGuidanceDataSetUpdateRequest>()
                });

            result.AssertNotFound();
        }

        [Fact]
        public async Task UpdateDataGuidance_NoDataSets()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                DataGuidance = "Release guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

            dataGuidanceDataSetService.Setup(s => 
                    s.ListDataSets(release.Id, null, default))
                .ReturnsAsync(new List<DataGuidanceDataSetViewModel>());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                    statisticsDbContext: statisticsDbContext);

                var result = await service.UpdateDataGuidance(
                    release.Id,
                    new DataGuidanceUpdateRequest
                    {
                        Content = "Updated release guidance",
                        DataSets = new List<DataGuidanceDataSetUpdateRequest>()
                    });

                VerifyAllMocks(dataGuidanceDataSetService);

                var viewModel = result.AssertRight();

                Assert.Equal(release.Id, viewModel.Id);
                Assert.Equal("Updated release guidance", viewModel.Content);
                Assert.Empty(viewModel.DataSets);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var actualRelease = await contentDbContext.Releases
                    .FirstAsync(r => r.Id == release.Id);

                Assert.Equal("Updated release guidance", actualRelease.DataGuidance);
            }
        }

        [Fact]
        public async Task UpdateDataGuidance_DataSetNotAttachedToRelease()
        {
            var release1 = new Release
            {
                Id = Guid.NewGuid(),
                DataGuidance = "Release 1 guidance"
            };

            var release2 = new Release
            {
                Id = Guid.NewGuid(),
                DataGuidance = "Release 2 guidance"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release1,
                File = new File
                {
                    Filename = "file1.csv",
                    Type = FileType.Data
                }
            };

            var releaseFile2 = new ReleaseFile
            {
                Release = release2,
                File = new File
                {
                    Filename = "file2.csv",
                    Type = FileType.Data
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release1, release2);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext);

                // Attempt to update data set 2 in a request for release 1 which it is not attached to
                var result = await service.UpdateDataGuidance(
                    release1.Id,
                    new DataGuidanceUpdateRequest
                    {
                        Content = "Updated release guidance",
                        DataSets = new List<DataGuidanceDataSetUpdateRequest>
                        {
                            new()
                            {
                                FileId = releaseFile1.FileId,
                                Content = "Data set 1 guidance updated"
                            },
                            new()
                            {
                                FileId = releaseFile2.FileId,
                                Content = "Data set 2 guidance updated"
                            }
                        }
                    });

                result.AssertBadRequest(DataGuidanceDataSetNotAttachedToRelease);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Assert no changes have been made
                var actualRelease = await contentDbContext.Releases
                    .FirstAsync(r => r.Id == release1.Id);

                Assert.Equal("Release 1 guidance", actualRelease.DataGuidance);
            }
        }

        [Fact]
        public async Task UpdateDataGuidance_WithDataSets()
        {
            var contentRelease = new Release
            {
                Id = Guid.NewGuid(),
                DataGuidance = "Release guidance"
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
                DataGuidance = "Data set 1 guidance"
            };

            var releaseSubject2 = new ReleaseSubject
            {
                Release = statsRelease,
                Subject = subject2,
                DataGuidance = "Data set 2 guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentRelease);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFile1, releaseFile2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(statsRelease);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseSubject1, releaseSubject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

            dataGuidanceDataSetService.Setup(s =>
                    s.ListDataSets(statsRelease.Id, null, default))
                .ReturnsAsync(DataGuidanceDataSets);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                    statisticsDbContext: statisticsDbContext);

                // Update release and data set 1
                var result = await service.UpdateDataGuidance(
                    contentRelease.Id,
                    new DataGuidanceUpdateRequest
                    {
                        Content = "Release guidance updated",
                        DataSets = new List<DataGuidanceDataSetUpdateRequest>
                        {
                            new()
                            {
                                FileId = releaseFile1.FileId,
                                Content = "Data set 1 guidance updated"
                            }
                        }
                    }
                );

                VerifyAllMocks(dataGuidanceDataSetService);

                var viewModel = result.AssertRight();

                Assert.Equal(contentRelease.Id, viewModel.Id);
                Assert.Equal("Release guidance updated", viewModel.Content);
                Assert.Equal(DataGuidanceDataSets, viewModel.DataSets);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var actualRelease = await contentDbContext.Releases
                    .FirstAsync(r => r.Id == contentRelease.Id);

                Assert.Equal("Release guidance updated", actualRelease.DataGuidance);

                // Assert only one data set has been updated
                var actualSubject1 = await statisticsDbContext.ReleaseSubject
                    .FirstAsync(rs => rs.ReleaseId == statsRelease.Id
                                && rs.SubjectId == releaseSubject1.Subject.Id);

                Assert.Equal("Data set 1 guidance updated", actualSubject1.DataGuidance);

                var actualSubject2 = await statisticsDbContext.ReleaseSubject
                    .FirstAsync(rs => rs.ReleaseId == statsRelease.Id
                                      && rs.SubjectId == releaseSubject2.Subject.Id);

                Assert.Equal("Data set 2 guidance", actualSubject2.DataGuidance);
            }
        }

        [Fact]
        public async Task UpdateDataGuidance_WithSubjects_AmendedRelease()
        {
            var contentReleaseVersion1 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = null,
                DataGuidance = "Version 1 release guidance"
            };

            var contentReleaseVersion2 = new Release
            {
                Id = Guid.NewGuid(),
                PreviousVersionId = contentReleaseVersion1.Id,
                DataGuidance = "Version 2 release guidance"
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

            // Version 1 has one data set, version 2 adds another data set

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
                DataGuidance = "Version 1 data set 1 guidance"
            };

            var releaseVersion2Subject1 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject1,
                DataGuidance = "Version 2 data set 1 guidance"
            };

            var releaseVersion2Subject2 = new ReleaseSubject
            {
                Release = statsReleaseVersion2,
                Subject = subject2,
                DataGuidance = "Version 2 data set 2 guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            var statisticsDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(contentReleaseVersion1, contentReleaseVersion2);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseVersion1File1, releaseVersion2File1,
                    releaseVersion2File2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                await statisticsDbContext.Release.AddRangeAsync(statsReleaseVersion1, statsReleaseVersion2);
                await statisticsDbContext.Subject.AddRangeAsync(subject1, subject2);
                await statisticsDbContext.ReleaseSubject.AddRangeAsync(releaseVersion1Subject1, releaseVersion2Subject1,
                    releaseVersion2Subject2);
                await statisticsDbContext.SaveChangesAsync();
            }

            var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

            dataGuidanceDataSetService.Setup(s =>
                    s.ListDataSets(statsReleaseVersion2.Id, null, default))
                .ReturnsAsync(DataGuidanceDataSets);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                    statisticsDbContext: statisticsDbContext);

                // Update release and data set 1 on version 2
                var result = await service.UpdateDataGuidance(
                    contentReleaseVersion2.Id,
                    new DataGuidanceUpdateRequest
                    {
                        Content = "Version 2 release guidance updated",
                        DataSets = new List<DataGuidanceDataSetUpdateRequest>
                        {
                            new()
                            {
                                FileId = releaseVersion2File1.FileId,
                                Content = "Version 2 data set 1 guidance updated"
                            }
                        }
                    }
                );

                var viewModel = result.AssertRight();

                VerifyAllMocks(dataGuidanceDataSetService);

                Assert.Equal(contentReleaseVersion2.Id, viewModel.Id);
                Assert.Equal("Version 2 release guidance updated", viewModel.Content);
                Assert.Equal(DataGuidanceDataSets, viewModel.DataSets);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            await using (var statisticsDbContext = InMemoryStatisticsDbContext(statisticsDbContextId))
            {
                var actualReleaseVersion1 = await contentDbContext.Releases
                    .FirstAsync(r => r.Id == contentReleaseVersion1.Id); 

                Assert.Equal("Version 1 release guidance", actualReleaseVersion1.DataGuidance);

                var actualReleaseVersion2 = await contentDbContext.Releases
                    .FirstAsync(r => r.Id == contentReleaseVersion2.Id); 

                Assert.Equal("Version 2 release guidance updated", actualReleaseVersion2.DataGuidance);

                // Assert the same data set on version 1 hasn't been affected
                var actualReleaseVersion1Subject1 = await statisticsDbContext.ReleaseSubject
                    .FirstAsync(rs => rs.ReleaseId == statsReleaseVersion1.Id
                                      && rs.SubjectId == subject1.Id);

                Assert.Equal("Version 1 data set 1 guidance", actualReleaseVersion1Subject1.DataGuidance);

                // Assert only one data set on version 2 has been updated
                var actualReleaseVersion2Subject1 = await statisticsDbContext.ReleaseSubject
                    .FirstAsync(rs => rs.ReleaseId == statsReleaseVersion2.Id
                                      && rs.SubjectId == subject1.Id);

                Assert.Equal("Version 2 data set 1 guidance updated", actualReleaseVersion2Subject1.DataGuidance);

                var actualReleaseVersion2Subject2 = await statisticsDbContext.ReleaseSubject
                    .FirstAsync(rs => rs.ReleaseId == statsReleaseVersion2.Id
                                      && rs.SubjectId == subject2.Id);

                Assert.Equal("Version 2 data set 2 guidance", actualReleaseVersion2Subject2.DataGuidance);
            }
        }

        [Fact]
        public async Task Validate_NoRelease()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();
            var service = SetupService(contentDbContext: contentDbContext);

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
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict);

            releaseDataFileRepository.Setup(s => s.HasAnyDataFiles(release.Id))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    releaseDataFileRepository: releaseDataFileRepository.Object);

                var result = await service.Validate(release.Id);

                VerifyAllMocks(releaseDataFileRepository);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task Validate_DataGuidancePopulated()
        {
            var release = new Release
            {
                DataGuidance = "Release guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

            dataGuidanceDataSetService.Setup(s => s.Validate(release.Id, default))
                .ReturnsAsync(Unit.Instance);

            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict);

            releaseDataFileRepository.Setup(s => s.HasAnyDataFiles(release.Id))
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object);

                var result = await service.Validate(release.Id);

                VerifyAllMocks(dataGuidanceDataSetService,
                    releaseDataFileRepository);

                result.AssertRight();
            }
        }

        [Fact]
        public async Task Validate_ReleaseDataGuidanceNotPopulated()
        {
            var release = new Release
            {
                DataGuidance = null
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict);

            releaseDataFileRepository.Setup(s => s.HasAnyDataFiles(release.Id))
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    releaseDataFileRepository: releaseDataFileRepository.Object);

                var result = await service.Validate(release.Id);

                result.AssertBadRequest(PublicDataGuidanceRequired);
            }
        }

        [Fact]
        public async Task Validate_DataSetDataGuidanceNotPopulated()
        {
            var release = new Release
            {
                DataGuidance = "Release guidance"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddRangeAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

            dataGuidanceDataSetService.Setup(s => s.Validate(release.Id, default))
                .ReturnsAsync(ValidationUtils.ValidationResult(PublicDataGuidanceRequired));

            var releaseDataFileRepository = new Mock<IReleaseDataFileRepository>(Strict);

            releaseDataFileRepository.Setup(s => s.HasAnyDataFiles(release.Id))
                .ReturnsAsync(true);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupService(contentDbContext: contentDbContext,
                    dataGuidanceDataSetService: dataGuidanceDataSetService.Object,
                    releaseDataFileRepository: releaseDataFileRepository.Object);

                var result = await service.Validate(release.Id);

                VerifyAllMocks(dataGuidanceDataSetService);

                result.AssertBadRequest(PublicDataGuidanceRequired);
            }
        }

        private static DataGuidanceService SetupService(
            ContentDbContext contentDbContext,
            StatisticsDbContext? statisticsDbContext = null,
            IDataGuidanceDataSetService? dataGuidanceDataSetService = null,
            IUserService? userService = null,
            IReleaseDataFileRepository? releaseDataFileRepository = null)
        {
            return new DataGuidanceService(
                contentDbContext,
                dataGuidanceDataSetService ?? Mock.Of<IDataGuidanceDataSetService>(Strict),
                statisticsDbContext ?? Mock.Of<StatisticsDbContext>(),
                userService ?? AlwaysTrueUserService().Object,
                releaseDataFileRepository ?? new ReleaseDataFileRepository(contentDbContext)
            );
        }
    }
}
