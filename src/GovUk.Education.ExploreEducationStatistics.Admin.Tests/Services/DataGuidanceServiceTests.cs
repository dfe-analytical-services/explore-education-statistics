#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataGuidanceServiceTests
{
    private static readonly List<DataGuidanceDataSetViewModel> DataGuidanceDataSets = new()
    {
        new DataGuidanceDataSetViewModel
        {
            FileId = Guid.NewGuid(),
            Content = "Test data set guidance",
            Filename = "data.csv",
            Name = "Test data set",
            GeographicLevels = new List<string>
            {
                "National",
                "Local authority",
                "Local authority district",
            },
            TimePeriods = new TimePeriodLabels("2020/21 Q3", "2021/22 Q1"),
            Variables = new List<LabelValue>
            {
                new("Filter label", "test_filter"),
                new("Indicator label", "test_indicator"),
            },
        },
    };

    [Fact]
    public async Task GetDataGuidance()
    {
        var releaseVersion = new ReleaseVersion { DataGuidance = "Release guidance" };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, default))
            .ReturnsAsync(DataGuidanceDataSets);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            var result = await service.GetDataGuidance(releaseVersion.Id);

            VerifyAllMocks(dataGuidanceDataSetService);

            var viewModel = result.AssertRight();

            Assert.Equal(releaseVersion.Id, viewModel.Id);
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
                DataSets = new List<DataGuidanceDataSetUpdateRequest>(),
            }
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateDataGuidance_NoDataSets()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PreviousVersionId = null,
            DataGuidance = "Release guidance",
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, default))
            .ReturnsAsync(new List<DataGuidanceDataSetViewModel>());

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            var result = await service.UpdateDataGuidance(
                releaseVersion.Id,
                new DataGuidanceUpdateRequest
                {
                    Content = "Updated release guidance",
                    DataSets = new List<DataGuidanceDataSetUpdateRequest>(),
                }
            );

            VerifyAllMocks(dataGuidanceDataSetService);

            var viewModel = result.AssertRight();

            Assert.Equal(releaseVersion.Id, viewModel.Id);
            Assert.Equal("Updated release guidance", viewModel.Content);
            Assert.Empty(viewModel.DataSets);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualReleaseVersion = await contentDbContext.ReleaseVersions.FirstAsync(rv =>
                rv.Id == releaseVersion.Id
            );

            Assert.Equal("Updated release guidance", actualReleaseVersion.DataGuidance);
        }
    }

    [Fact]
    public async Task UpdateDataGuidance_DataSetNotAttachedToRelease()
    {
        var release1 = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            DataGuidance = "Release 1 guidance",
        };

        var release2 = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            DataGuidance = "Release 2 guidance",
        };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = release1,
            File = new File { Filename = "file1.csv", Type = FileType.Data },
            Summary = "Data set 1 guidance",
        };

        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = release2,
            File = new File { Filename = "file2.csv", Type = FileType.Data },
            Summary = "Data set 2 guidance",
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(release1, release2);
            contentDbContext.ReleaseFiles.AddRange(releaseFile1, releaseFile2);
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
                            Content = "Data set 1 guidance updated",
                        },
                        new()
                        {
                            FileId = releaseFile2.FileId,
                            Content = "Data set 2 guidance updated",
                        },
                    },
                }
            );

            result.AssertBadRequest(DataGuidanceDataSetNotAttachedToRelease);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Assert no changes have been made to the release version or any of the data sets
            var actualReleaseVersion = await contentDbContext.ReleaseVersions.FirstAsync(rv =>
                rv.Id == release1.Id
            );

            Assert.Equal("Release 1 guidance", actualReleaseVersion.DataGuidance);

            var actualReleaseFile1 = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                rf.ReleaseVersionId == release1.Id && rf.FileId == releaseFile1.FileId
            );

            Assert.Equal("Data set 1 guidance", actualReleaseFile1.Summary);

            var actualReleaseFile2 = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                rf.ReleaseVersionId == release2.Id && rf.FileId == releaseFile2.FileId
            );

            Assert.Equal("Data set 2 guidance", actualReleaseFile2.Summary);
        }
    }

    [Fact]
    public async Task UpdateDataGuidance_WithDataSets()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            DataGuidance = "Release guidance",
        };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File { Filename = "file1.csv", Type = FileType.Data },
            Summary = "Data set 1 guidance",
        };

        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File { Filename = "file2.csv", Type = FileType.Data },
            Summary = "Data set 2 guidance",
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFile1, releaseFile2);
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion.Id, null, default))
            .ReturnsAsync(DataGuidanceDataSets);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            // Update release and data set 1
            var result = await service.UpdateDataGuidance(
                releaseVersion.Id,
                new DataGuidanceUpdateRequest
                {
                    Content = "Release guidance updated",
                    DataSets = new List<DataGuidanceDataSetUpdateRequest>
                    {
                        new()
                        {
                            FileId = releaseFile1.FileId,
                            Content = "Data set 1 guidance updated",
                        },
                    },
                }
            );

            VerifyAllMocks(dataGuidanceDataSetService);

            var viewModel = result.AssertRight();

            Assert.Equal(releaseVersion.Id, viewModel.Id);
            Assert.Equal("Release guidance updated", viewModel.Content);
            Assert.Equal(DataGuidanceDataSets, viewModel.DataSets);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualReleaseVersion = await contentDbContext.ReleaseVersions.FirstAsync(rv =>
                rv.Id == releaseVersion.Id
            );

            Assert.Equal("Release guidance updated", actualReleaseVersion.DataGuidance);

            // Assert only one data set has been updated
            var actualReleaseFile1 = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == releaseFile1.FileId
            );

            Assert.Equal("Data set 1 guidance updated", actualReleaseFile1.Summary);

            var actualReleaseFile2 = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                rf.ReleaseVersionId == releaseVersion.Id && rf.FileId == releaseFile2.FileId
            );

            Assert.Equal("Data set 2 guidance", actualReleaseFile2.Summary);
        }
    }

    [Fact]
    public async Task UpdateDataGuidance_WithDataSets_AmendedRelease()
    {
        var releaseVersion1 = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PreviousVersionId = null,
            DataGuidance = "Version 1 release guidance",
        };

        var releaseVersion2 = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PreviousVersionId = releaseVersion1.Id,
            DataGuidance = "Version 2 release guidance",
        };

        var originalPublishedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Version 1 has one data set, version 2 adds another data set

        var releaseVersion1File1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion1,
            File = new File { Filename = "file1.csv", Type = FileType.Data },
            Summary = "Version 1 data set 1 guidance",
            Published = originalPublishedDate,
        };

        var releaseVersion2File1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion2,
            File = new File { Filename = "file1.csv", Type = FileType.Data },
            Summary = "Version 2 data set 1 guidance",
            Published = originalPublishedDate,
        };

        var releaseVersion2File2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion2,
            File = new File { Filename = "file2.csv", Type = FileType.Data },
            Summary = "Version 2 data set 2 guidance",
            Published = originalPublishedDate,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion1, releaseVersion2);
            contentDbContext.ReleaseFiles.AddRange(
                releaseVersion1File1,
                releaseVersion2File1,
                releaseVersion2File2
            );
            await contentDbContext.SaveChangesAsync();
        }

        var dataGuidanceDataSetService = new Mock<IDataGuidanceDataSetService>(Strict);

        dataGuidanceDataSetService
            .Setup(s => s.ListDataSets(releaseVersion2.Id, null, default))
            .ReturnsAsync(DataGuidanceDataSets);

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: contentDbContext,
                dataGuidanceDataSetService: dataGuidanceDataSetService.Object
            );

            // Update release and data set 1 on version 2
            var result = await service.UpdateDataGuidance(
                releaseVersion2.Id,
                new()
                {
                    Content = "Version 2 release guidance updated",
                    DataSets =
                    [
                        new()
                        {
                            FileId = releaseVersion2File1.FileId,
                            Content = "Version 2 data set 1 guidance updated",
                        },
                    ],
                }
            );

            var viewModel = result.AssertRight();

            VerifyAllMocks(dataGuidanceDataSetService);

            Assert.Equal(releaseVersion2.Id, viewModel.Id);
            Assert.Equal("Version 2 release guidance updated", viewModel.Content);
            Assert.Equal(DataGuidanceDataSets, viewModel.DataSets);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var actualReleaseVersion1 = await contentDbContext.ReleaseVersions.FirstAsync(rv =>
                rv.Id == releaseVersion1.Id
            );

            Assert.Equal("Version 1 release guidance", actualReleaseVersion1.DataGuidance);

            var actualReleaseVersion2 = await contentDbContext.ReleaseVersions.FirstAsync(rv =>
                rv.Id == releaseVersion2.Id
            );

            Assert.Equal("Version 2 release guidance updated", actualReleaseVersion2.DataGuidance);

            // Assert the same data set on version 1 hasn't been affected
            var actualReleaseVersion1File1 = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                rf.ReleaseVersionId == releaseVersion1.Id
                && rf.FileId == releaseVersion1File1.FileId
            );

            Assert.Equal("Version 1 data set 1 guidance", actualReleaseVersion1File1.Summary);
            Assert.NotNull(actualReleaseVersion1File1.Published);

            // Assert only one data set on version 2 has been updated
            var actualReleaseVersion2File1 = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                rf.ReleaseVersionId == releaseVersion2.Id
                && rf.FileId == releaseVersion2File1.FileId
            );

            Assert.Equal(
                "Version 2 data set 1 guidance updated",
                actualReleaseVersion2File1.Summary
            );
            Assert.Null(actualReleaseVersion2File1.Published);

            var actualReleaseVersion2File2 = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                rf.ReleaseVersionId == releaseVersion2.Id
                && rf.FileId == releaseVersion2File2.FileId
            );

            Assert.Equal("Version 2 data set 2 guidance", actualReleaseVersion2File2.Summary);
            Assert.NotNull(actualReleaseVersion2File2.Published);
        }
    }

    [Fact]
    public async Task ValidateForReleaseChecklist_NoRelease()
    {
        await using var contentDbContext = InMemoryApplicationDbContext();
        var service = SetupService(contentDbContext: contentDbContext);

        var result = await service.ValidateForReleaseChecklist(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task ValidateForReleaseChecklist_SucceedsWithNoDataSets()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.ValidateForReleaseChecklist(releaseVersion.Id);

            result.AssertRight();
        }
    }

    [Theory]
    [InlineData(null, "Data set 1 guidance", false)]
    [InlineData("", "Data set 1 guidance", false)]
    [InlineData(" ", "Data set 1 guidance", false)]
    [InlineData("Release guidance", null, false)]
    [InlineData("Release guidance", "", false)]
    [InlineData("Release guidance", " ", false)]
    [InlineData("Release guidance", "Data set 1 guidance", true)]
    public async Task ValidateForReleaseChecklist(
        string? releaseGuidance,
        string? dataSet1Guidance,
        bool expectedValidResult
    )
    {
        var releaseVersion = new ReleaseVersion { DataGuidance = releaseGuidance };

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File { Filename = "file1.csv", Type = FileType.Data },
            Summary = dataSet1Guidance,
        };

        // Create an second data set which always has valid data guidance
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File { Filename = "file2.csv", Type = FileType.Data },
            Summary = "Data set 2 guidance",
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFile1, releaseFile2);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext);

            var result = await service.ValidateForReleaseChecklist(releaseVersion.Id);

            if (expectedValidResult)
            {
                result.AssertRight();
            }
            else
            {
                result.AssertBadRequest(PublicDataGuidanceRequired);
            }
        }
    }

    private static DataGuidanceService SetupService(
        ContentDbContext contentDbContext,
        IDataGuidanceDataSetService? dataGuidanceDataSetService = null,
        IUserService? userService = null,
        IReleaseDataFileRepository? releaseDataFileRepository = null
    )
    {
        return new DataGuidanceService(
            contentDbContext,
            dataGuidanceDataSetService ?? Mock.Of<IDataGuidanceDataSetService>(Strict),
            userService ?? AlwaysTrueUserService().Object,
            releaseDataFileRepository ?? new ReleaseDataFileRepository(contentDbContext)
        );
    }
}
