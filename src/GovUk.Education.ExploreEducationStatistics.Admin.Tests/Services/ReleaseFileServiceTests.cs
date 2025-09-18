#nullable enable
using System.IO.Compression;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ReleaseFileServiceTests : IDisposable
{
    private readonly DataFixture _dataFixture = new();

    private readonly List<string> _filePaths = new();

    public void Dispose()
    {
        // Cleanup any files that have been
        // written to the filesystem.
        _filePaths.ForEach(System.IO.File.Delete);
    }

    private readonly User _user = new()
    {
        Id = Guid.NewGuid(),
        Email = "test@test.com"
    };

    [Fact]
    public async Task Delete()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var chartFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart
            }
        };

        var imageFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image.png",
                Type = Image
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(ancillaryFile, chartFile, imageFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.Delete(releaseVersionId: releaseVersion.Id,
                ancillaryFile.File.Id);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            Assert.True(result.IsRight);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()), Times.Once);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
            Assert.Null(
                await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

            // Check that other files remain untouched
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(chartFile.File.Id));
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
        }
    }

    [Fact]
    public async Task Delete_FileFromAmendment()
    {
        var releaseVersion = new ReleaseVersion();

        var amendmentReleaseVersion = new ReleaseVersion
        {
            PreviousVersionId = releaseVersion.Id
        };

        var ancillaryFile = new File
        {
            RootPath = Guid.NewGuid(),
            Filename = "ancillary.pdf",
            Type = Ancillary,
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = ancillaryFile
        };

        var amendmentReleaseFile = new ReleaseFile
        {
            ReleaseVersion = amendmentReleaseVersion,
            File = ancillaryFile
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion, amendmentReleaseVersion);
            contentDbContext.Files.AddRange(ancillaryFile);
            contentDbContext.ReleaseFiles.AddRange(releaseFile, amendmentReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.Delete(releaseVersionId: amendmentReleaseVersion.Id,
                fileId: ancillaryFile.Id);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            Assert.True(result.IsRight);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Check that the file is unlinked from the amendment
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseFile.Id));

            // Check that the file and link to the previous version remain untouched
            Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.Id));
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseFile.Id));
        }
    }

    [Fact]
    public async Task Delete_InvalidFileType()
    {
        var releaseVersion = new ReleaseVersion();

        var dataFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = Guid.NewGuid()
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.Add(dataFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.Delete(releaseVersionId: releaseVersion.Id,
                fileId: dataFile.File.Id);

            result.AssertBadRequest(FileTypeInvalid);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Check that the file remains untouched
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.File.Id));
        }
    }

    [Fact]
    public async Task Delete_ReleaseNotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(ancillaryFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.Delete(Guid.NewGuid(), ancillaryFile.File.Id);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            result.AssertNotFound();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Check that the file remains untouched
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));
        }
    }

    [Fact]
    public async Task Delete_FileNotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.Delete(releaseVersionId: releaseVersion.Id,
                fileId: Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task Delete_MultipleFiles()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var chartFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart
            }
        };

        var imageFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image.png",
                Type = Image
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(ancillaryFile, chartFile, imageFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()))
            .Returns(Task.CompletedTask);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
            .Returns(Task.CompletedTask);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.Delete(releaseVersion.Id, new List<Guid>
            {
                ancillaryFile.File.Id,
                chartFile.File.Id,
                imageFile.File.Id
            });

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            Assert.True(result.IsRight);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()), Times.Once);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
            Assert.Null(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
            Assert.Null(await contentDbContext.Files.FindAsync(chartFile.File.Id));

            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
            Assert.Null(await contentDbContext.Files.FindAsync(imageFile.File.Id));
        }
    }

    [Fact]
    public async Task Delete_MultipleFilesWithAnInvalidFileType()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var dataFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = Guid.NewGuid()
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(ancillaryFile, dataFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.Delete(releaseVersion.Id, new List<Guid>
            {
                ancillaryFile.File.Id,
                dataFile.File.Id
            });

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            result.AssertBadRequest(FileTypeInvalid);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Check that all the files remain untouched
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.File.Id));
        }
    }

    [Fact]
    public async Task Delete_MultipleFilesWithReleaseNotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var chartFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart
            }
        };

        var imageFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image.png",
                Type = Image
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(ancillaryFile, chartFile, imageFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.Delete(Guid.NewGuid(), new List<Guid>
            {
                ancillaryFile.File.Id,
                chartFile.File.Id,
                imageFile.File.Id
            });

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            result.AssertNotFound();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Check that all the files remain untouched
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(chartFile.File.Id));

            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
        }
    }

    [Fact]
    public async Task Delete_MultipleFilesWithAFileNotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(ancillaryFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.Delete(releaseVersion.Id, new List<Guid>
            {
                ancillaryFile.File.Id,
                // Include an unknown id
                Guid.NewGuid()
            });

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            result.AssertNotFound();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Check that the files remain untouched
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));
        }
    }

    [Fact]
    public async Task Delete_MultipleFilesWithAFileFromAmendment()
    {
        var releaseVersion = new ReleaseVersion();

        var amendmentRelease = new ReleaseVersion
        {
            PreviousVersionId = releaseVersion.Id
        };

        var ancillaryFile = new File
        {
            RootPath = Guid.NewGuid(),
            Filename = "ancillary.pdf",
            Type = Ancillary
        };

        var chartFile = new File
        {
            RootPath = Guid.NewGuid(),
            Filename = "chart.png",
            Type = Chart
        };

        var ancillaryReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = ancillaryFile
        };

        var ancillaryAmendmentReleaseFile = new ReleaseFile
        {
            ReleaseVersion = amendmentRelease,
            File = ancillaryFile
        };

        var chartAmendmentReleaseFile = new ReleaseFile
        {
            ReleaseVersion = amendmentRelease,
            File = chartFile
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion, amendmentRelease);
            contentDbContext.Files.AddRange(ancillaryFile, chartFile);
            contentDbContext.ReleaseFiles.AddRange(ancillaryReleaseFile, ancillaryAmendmentReleaseFile,
                chartAmendmentReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.Delete(amendmentRelease.Id, new List<Guid>
            {
                ancillaryFile.Id,
                chartFile.Id
            });

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            Assert.True(result.IsRight);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Check that the ancillary file is unlinked from the amendment
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryAmendmentReleaseFile.Id));

            // Check that the ancillary file and link to the previous version remain untouched
            Assert.NotNull(
                await contentDbContext.Files.FindAsync(ancillaryFile.Id));
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryReleaseFile.Id));

            // Check that the chart file and link to the amendment are removed
            Assert.Null(await contentDbContext.Files.FindAsync(chartFile.Id));
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartAmendmentReleaseFile.Id));
        }
    }

    [Fact]
    public async Task DeleteAll()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var chartFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart
            }
        };

        var dataFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = Guid.NewGuid()
            }
        };

        var imageFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image.png",
                Type = Image
            }
        };

        var dataGuidanceFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data-guidance.txt",
                Type = DataGuidance
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(ancillaryFile, chartFile, dataFile, imageFile, dataGuidanceFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()))
            .Returns(Task.CompletedTask);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
            .Returns(Task.CompletedTask);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()))
            .Returns(Task.CompletedTask);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, dataGuidanceFile.Path()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.DeleteAll(releaseVersion.Id);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            Assert.True(result.IsRight);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, dataGuidanceFile.Path()), Times.Once);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
            Assert.Null(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
            Assert.Null(await contentDbContext.Files.FindAsync(chartFile.File.Id));

            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
            Assert.Null(await contentDbContext.Files.FindAsync(imageFile.File.Id));

            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(dataGuidanceFile.Id));
            Assert.Null(await contentDbContext.Files.FindAsync(dataGuidanceFile.File.Id));

            // Check that data files remain untouched
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
            Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.File.Id));
        }
    }

    [Fact]
    public async Task DeleteAll_ReleaseNotFound()
    {
        await using var contentDbContext = InMemoryContentDbContext();

        var service = SetupReleaseFileService(contentDbContext: contentDbContext);

        var result = await service.DeleteAll(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task DeleteAll_NoFiles()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.DeleteAll(releaseVersion.Id);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            Assert.True(result.IsRight);
        }
    }

    [Fact]
    public async Task DeleteAll_FileFromAmendment()
    {
        var releaseVersion = new ReleaseVersion();

        var amendmentRelease = new ReleaseVersion
        {
            PreviousVersionId = releaseVersion.Id
        };

        var ancillaryFile = new File
        {
            RootPath = Guid.NewGuid(),
            Filename = "ancillary.pdf",
            Type = Ancillary
        };

        var chartFile = new File
        {
            RootPath = Guid.NewGuid(),
            Filename = "chart.png",
            Type = Chart
        };

        var dataGuidanceFile = new File
        {
            RootPath = Guid.NewGuid(),
            Filename = "data-guidance.txt",
            Type = DataGuidance,
        };

        var ancillaryReleaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = ancillaryFile
        };

        var ancillaryAmendmentReleaseFile = new ReleaseFile
        {
            ReleaseVersion = amendmentRelease,
            File = ancillaryFile
        };

        var chartAmendmentReleaseFile = new ReleaseFile
        {
            ReleaseVersion = amendmentRelease,
            File = chartFile
        };

        var dataGuidanceAmendmentReleaseFile = new ReleaseFile
        {
            ReleaseVersion = amendmentRelease,
            File = dataGuidanceFile
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion, amendmentRelease);
            contentDbContext.Files.AddRange(ancillaryFile, chartFile, dataGuidanceFile);
            contentDbContext.ReleaseFiles.AddRange(
                ancillaryReleaseFile,
                ancillaryAmendmentReleaseFile,
                chartAmendmentReleaseFile,
                dataGuidanceAmendmentReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
            .Returns(Task.CompletedTask);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, dataGuidanceFile.Path()))
            .Returns(Task.CompletedTask);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.DeleteAll(amendmentRelease.Id);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            Assert.True(result.IsRight);

            privateBlobStorageService.Verify(mock =>
                mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // Check that the ancillary file is unlinked from the amendment
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryAmendmentReleaseFile.Id));

            // Check that the ancillary file and link to the previous version remain untouched
            Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.Id));
            Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryReleaseFile.Id));

            // Check that the chart file and link to the amendment are removed
            Assert.Null(await contentDbContext.Files.FindAsync(chartFile.Id));
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartAmendmentReleaseFile.Id));

            // Check that the data guidance file and link to the amendment are removed
            Assert.Null(await contentDbContext.Files.FindAsync(dataGuidanceFile.Id));
            Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(dataGuidanceAmendmentReleaseFile.Id));
        }
    }

    [Fact]
    public async Task ListAll_NoFiles()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.ListAll(releaseVersion.Id, Ancillary, Chart);

            Assert.True(result.IsRight);
            Assert.Empty(result.Right);
        }
    }

    [Fact]
    public async Task ListAll_ReleaseNotFound()
    {
        await using var contentDbContext = InMemoryContentDbContext();

        var service = SetupReleaseFileService(contentDbContext: contentDbContext);

        var result = await service.ListAll(Guid.NewGuid(), Ancillary, Chart);

        result.AssertNotFound();
    }

    [Fact]
    public async Task ListAll()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            Name = "Ancillary Test File 1",
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary_1.pdf",
                ContentLength = 10240,
                Type = Ancillary,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("ancillary1@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var ancillaryFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            Name = "Ancillary Test File 2",
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "Ancillary 2.pdf",
                ContentLength = 10240,
                Type = Ancillary,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("ancillary2@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var chartFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                ContentLength = 20480,
                Type = Chart,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("chart@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var dataFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data,
                SubjectId = Guid.NewGuid(),
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("dataFile@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var imageFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image.png",
                ContentLength = 30720,
                Type = Image,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("image@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(ancillaryFile1, ancillaryFile2,
                chartFile, dataFile, imageFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.ListAll(releaseVersion.Id, Ancillary, Chart, Image);

            Assert.True(result.IsRight);

            var fileInfoList = result.Right.ToList();
            Assert.Equal(4, fileInfoList.Count);

            Assert.Equal(ancillaryFile1.File.Id, fileInfoList[0].Id);
            Assert.Equal("pdf", fileInfoList[0].Extension);
            Assert.Equal("ancillary_1.pdf", fileInfoList[0].FileName);
            Assert.Equal("Ancillary Test File 1", fileInfoList[0].Name);
            Assert.Equal("10 Kb", fileInfoList[0].Size);
            Assert.Equal(Ancillary, fileInfoList[0].Type);

            Assert.Equal(ancillaryFile2.File.Id, fileInfoList[1].Id);
            Assert.Equal("pdf", fileInfoList[1].Extension);
            Assert.Equal("Ancillary 2.pdf", fileInfoList[1].FileName);
            Assert.Equal("Ancillary Test File 2", fileInfoList[1].Name);
            Assert.Equal("10 Kb", fileInfoList[1].Size);
            Assert.Equal(Ancillary, fileInfoList[1].Type);

            Assert.Equal(chartFile.File.Id, fileInfoList[2].Id);
            Assert.Equal("png", fileInfoList[2].Extension);
            Assert.Equal("chart.png", fileInfoList[2].FileName);
            Assert.Equal("", fileInfoList[2].Name);
            Assert.Equal("20 Kb", fileInfoList[2].Size);
            Assert.Equal(Chart, fileInfoList[2].Type);

            Assert.Equal(imageFile.File.Id, fileInfoList[3].Id);
            Assert.Equal("png", fileInfoList[3].Extension);
            Assert.Equal("image.png", fileInfoList[3].FileName);
            Assert.Equal("", fileInfoList[3].Name);
            Assert.Equal("30 Kb", fileInfoList[3].Size);
            Assert.Equal(Image, fileInfoList[3].Type);
        }
    }

    [Fact]
    public async Task GetAncillaryFiles()
    {
        var releaseVersion = new ReleaseVersion();

        var ancillaryFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            Name = "Ancillary Test File 1",
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary_1.pdf",
                ContentLength = 10240,
                Type = Ancillary,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("ancillary1@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var ancillaryFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            Name = "Ancillary Test File 2",
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "Ancillary 2.pdf",
                ContentLength = 10240,
                Type = Ancillary,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("ancillary2@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var chartFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("chart@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var dataFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("dataFile@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var imageFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image.png",
                Type = Image,
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("image@test.com")
                    .Generate(),
                Created = DateTime.UtcNow
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(ancillaryFile1, ancillaryFile2,
                chartFile, dataFile, imageFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.GetAncillaryFiles(releaseVersion.Id);

            Assert.True(result.IsRight);

            var fileInfoList = result.Right.ToList();
            Assert.Equal(2, fileInfoList.Count);

            Assert.Equal(ancillaryFile1.File.Id, fileInfoList[0].Id);
            Assert.Equal("pdf", fileInfoList[0].Extension);
            Assert.Equal("ancillary_1.pdf", fileInfoList[0].FileName);
            Assert.Equal("Ancillary Test File 1", fileInfoList[0].Name);
            Assert.Equal("10 Kb", fileInfoList[0].Size);
            Assert.Equal(Ancillary, fileInfoList[0].Type);
            Assert.Equal(ancillaryFile1.File.Created, fileInfoList[0].Created);
            Assert.Equal(ancillaryFile1.File.CreatedBy.Email, fileInfoList[0].UserName);

            Assert.Equal(ancillaryFile2.File.Id, fileInfoList[1].Id);
            Assert.Equal("pdf", fileInfoList[1].Extension);
            Assert.Equal("Ancillary 2.pdf", fileInfoList[1].FileName);
            Assert.Equal("Ancillary Test File 2", fileInfoList[1].Name);
            Assert.Equal("10 Kb", fileInfoList[1].Size);
            Assert.Equal(Ancillary, fileInfoList[1].Type);
            Assert.Equal(ancillaryFile2.File.Created, fileInfoList[1].Created);
            Assert.Equal(ancillaryFile2.File.CreatedBy.Email, fileInfoList[1].UserName);
        }
    }

    [Fact]
    public async Task GetFile()
    {
        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            Name = "Test PDF File",
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                ContentLength = 10240,
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.GetFile(releaseFile.ReleaseVersionId, releaseFile.FileId);
            Assert.True(result.IsRight);

            var fileInfo = result.Right;
            Assert.Equal(releaseFile.Name, fileInfo.Name);
            Assert.Equal(releaseFile.FileId, fileInfo.Id);
            Assert.Equal("pdf", fileInfo.Extension);
            Assert.Equal("ancillary.pdf", fileInfo.FileName);
            Assert.Equal("10 Kb", fileInfo.Size);
            Assert.Equal(Ancillary, fileInfo.Type);
        }
    }

    [Fact]
    public async Task GetFile_NoRelease()
    {
        await using var contentDbContext = InMemoryContentDbContext();

        var service = SetupReleaseFileService(contentDbContext: contentDbContext);

        var result = await service.GetFile(Guid.NewGuid(), Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetFile_NoReleaseFile()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.GetFile(releaseVersionId: releaseVersion.Id,
                fileId: Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetDownloadToken()
    {
        var releaseVersion = new ReleaseVersion();

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "Ancillary.pdf",
                ContentType = "application/pdf",
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService
            .SetupGetDownloadToken(
                container: PrivateReleaseFiles,
                filename: releaseFile.File.Filename,
                path: releaseFile.Path(),
                contentType: releaseFile.File.ContentType);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.GetBlobDownloadToken(
                releaseVersionId: releaseVersion.Id,
                fileId: releaseFile.File.Id,
                cancellationToken: default);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            var token = result.AssertRight();

            Assert.Equal("application/pdf", token.ContentType);
            Assert.Equal("Ancillary.pdf", token.Filename);
            Assert.Equal("token", token.Token);
            Assert.Equal(releaseFile.Path(), token.Path);
            Assert.Equal(PrivateReleaseFiles.Name, token.ContainerName);
        }
    }

    [Fact]
    public async Task GetDownloadToken_ReleaseNotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.GetBlobDownloadToken(
                releaseVersionId: Guid.NewGuid(),
                fileId: releaseFile.File.Id,
                cancellationToken: default);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetDownloadToken_FileNotFound()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.GetBlobDownloadToken(
                releaseVersionId: releaseVersion.Id,
                fileId: Guid.NewGuid(),
                cancellationToken: default);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetDownloadToken_BlobDoesNotExist()
    {
        var releaseVersion = new ReleaseVersion();

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                ContentType = "application/pdf",
                Type = Ancillary
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService.SetupGetDownloadTokenNotFound(
            container: PrivateReleaseFiles,
            filename: releaseFile.File.Filename,
            path: releaseFile.Path(),
            cancellationToken: default);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var result = await service.GetBlobDownloadToken(
                releaseVersionId: releaseVersion.Id,
                fileId: releaseFile.File.Id,
                cancellationToken: default);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task ZipFilesToStream_ValidFileTypes()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.csv",
                Type = FileType.Data
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };
        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService
            .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile1.Path(), true);
        privateBlobStorageService
            .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile2.Path(), true);
        privateBlobStorageService
            .SetupGetDownloadStream(PrivateReleaseFiles, releaseFile1.Path(), "Test data blob");
        privateBlobStorageService
            .SetupGetDownloadStream(PrivateReleaseFiles, releaseFile2.Path(), "Test ancillary blob");

        var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(Strict);

        dataGuidanceFileWriter
            .Setup(
                s => s.WriteToStream(
                    It.IsAny<Stream>(),
                    It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                    ListOf(releaseFile1.FileId))
            )
            .Returns<Stream, ReleaseVersion, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
            .Callback<Stream, ReleaseVersion, IEnumerable<Guid>?>(
                (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
            );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var path = GenerateZipFilePath();
            await using var stream = System.IO.File.OpenWrite(path);

            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataGuidanceFileWriter: dataGuidanceFileWriter.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: fileIds
            );

            MockUtils.VerifyAllMocks(dataGuidanceFileWriter);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Equal(3, zip.Entries.Count);
            Assert.StartsWith("data/data", zip.Entries[0].FullName);
            Assert.EndsWith(".csv", zip.Entries[0].FullName);
            Assert.Equal("Test data blob", zip.Entries[0].Open().ReadToEnd());

            Assert.Equal("supporting-files/ancillary.pdf", zip.Entries[1].FullName);
            Assert.Equal("Test ancillary blob", zip.Entries[1].Open().ReadToEnd());

            // Data guidance is generated if there is at least one data file
            Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[2].FullName);
            Assert.Equal("Test data guidance blob", zip.Entries[2].Open().ReadToEnd());
        }
    }

    [Fact]
    public async Task ZipFilesToStream_DataGuidanceForMultipleDataFiles()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data-1.csv",
                Type = FileType.Data
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data-2.csv",
                Type = FileType.Data
            }
        };
        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService
            .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile1.Path(), true);
        privateBlobStorageService
            .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile2.Path(), true);
        privateBlobStorageService
            .SetupGetDownloadStream(PrivateReleaseFiles, releaseFile1.Path(), "Test data 1 blob");
        privateBlobStorageService
            .SetupGetDownloadStream(PrivateReleaseFiles, releaseFile2.Path(), "Test data 2 blob");

        var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(Strict);

        dataGuidanceFileWriter
            .Setup(
                s => s.WriteToStream(
                    It.IsAny<Stream>(),
                    It.Is<ReleaseVersion>(rv => rv.Id == releaseVersion.Id),
                    ListOf(releaseFile1.FileId, releaseFile2.FileId))
            )
            .Returns<Stream, ReleaseVersion, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
            .Callback<Stream, ReleaseVersion, IEnumerable<Guid>?>(
                (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
            );

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var path = GenerateZipFilePath();
            await using var stream = System.IO.File.OpenWrite(path);

            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                dataGuidanceFileWriter: dataGuidanceFileWriter.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: fileIds
            );

            MockUtils.VerifyAllMocks(dataGuidanceFileWriter);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Equal(3, zip.Entries.Count);
            Assert.StartsWith("data/data-1", zip.Entries[0].FullName);
            Assert.EndsWith(".csv", zip.Entries[0].FullName);
            Assert.Equal("Test data 1 blob", zip.Entries[0].Open().ReadToEnd());

            Assert.StartsWith("data/data-2", zip.Entries[1].FullName);
            Assert.EndsWith(".csv", zip.Entries[1].FullName);
            Assert.Equal("Test data 2 blob", zip.Entries[1].Open().ReadToEnd());

            // Data guidance is generated if there is at least one data file
            Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[2].FullName);
            Assert.Equal("Test data guidance blob", zip.Entries[2].Open().ReadToEnd());
        }
    }

    [Fact]
    public async Task ZipFilesToStream_OrderedAlphabetically()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-2.pdf",
                Type = Ancillary,
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-3.pdf",
                Type = Ancillary
            }
        };
        var releaseFile3 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "test-1.pdf",
                Type = Ancillary
            }
        };
        var releaseFiles = ListOf(releaseFile1, releaseFile2, releaseFile3);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        await using var stream = System.IO.File.OpenWrite(path);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService
            .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile1.Path(), true);
        privateBlobStorageService
            .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile2.Path(), true);
        privateBlobStorageService
            .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile3.Path(), true);
        privateBlobStorageService
            .SetupGetDownloadStream(PrivateReleaseFiles, releaseFile1.Path(), "Test 2 blob");
        privateBlobStorageService
            .SetupGetDownloadStream(PrivateReleaseFiles, releaseFile2.Path(), "Test 3 blob");
        privateBlobStorageService
            .SetupGetDownloadStream(PrivateReleaseFiles, releaseFile3.Path(), "Test 1 blob");

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: fileIds
            );

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Equal(3, zip.Entries.Count);
            Assert.Equal("supporting-files/test-1.pdf", zip.Entries[0].FullName);
            Assert.Equal("Test 1 blob", zip.Entries[0].Open().ReadToEnd());

            Assert.Equal("supporting-files/test-2.pdf", zip.Entries[1].FullName);
            Assert.Equal("Test 2 blob", zip.Entries[1].Open().ReadToEnd());

            Assert.Equal("supporting-files/test-3.pdf", zip.Entries[2].FullName);
            Assert.Equal("Test 3 blob", zip.Entries[2].Open().ReadToEnd());
        }
    }

    [Fact]
    public async Task ZipFilesToStream_FiltersInvalidFileTypes()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.meta.csv",
                Type = Metadata,
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.zip",
                Type = DataZip
            }
        };
        var releaseFile3 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.jpg",
                Type = Chart
            }
        };
        var releaseFile4 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image.jpg",
                Type = Image
            }
        };

        var releaseFiles = ListOf(releaseFile1, releaseFile2, releaseFile3, releaseFile4);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        await using var stream = System.IO.File.OpenWrite(path);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: fileIds
            );

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            Assert.Empty(zip.Entries);
        }
    }

    [Fact]
    public async Task ZipFilesToStream_FiltersFilesNotInBlobStorage()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data.pdf",
                Type = FileType.Data,
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            }
        };

        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        await using var stream = System.IO.File.OpenWrite(path);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        // Files do not exist in blob storage
        privateBlobStorageService.SetupGetDownloadStreamNotFound(PrivateReleaseFiles, releaseFile1.Path());
        privateBlobStorageService.SetupGetDownloadStreamNotFound(PrivateReleaseFiles, releaseFile2.Path());

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: fileIds
            );

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            Assert.Empty(zip.Entries);
        }
    }

    [Fact]
    public async Task ZipFilesToStream_FiltersFilesForOtherReleases()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        // Files are for other releases
        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary-1.pdf",
                Type = Ancillary,
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary-2.pdf",
                Type = Ancillary
            }
        };

        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        await using var stream = System.IO.File.OpenWrite(path);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: fileIds
            );

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            Assert.Empty(zip.Entries);
        }
    }

    [Fact]
    public async Task ZipFilesToStream_Empty()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        await using var stream = System.IO.File.OpenWrite(path);

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var fileIds = ListOf(Guid.NewGuid(), Guid.NewGuid());
            var result = await service.ZipFilesToStream(releaseVersion.Id, stream, fileIds);

            MockUtils.VerifyAllMocks(privateBlobStorageService);

            Assert.True(result.IsRight);

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Empty(zip.Entries);
        }
    }

    [Fact]
    public async Task ZipFilesToStream_Cancelled()
    {
        ReleaseVersion releaseVersion = _dataFixture.DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease()
                .WithPublication(_dataFixture.DefaultPublication()));

        var releaseFile1 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary-1.pdf",
                Type = Ancillary
            }
        };
        var releaseFile2 = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary-2.pdf",
                Type = Ancillary
            }
        };

        var releaseFiles = ListOf(releaseFile1, releaseFile2);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFiles);
            await contentDbContext.SaveChangesAsync();
        }

        var path = GenerateZipFilePath();
        await using var stream = System.IO.File.OpenWrite(path);

        var tokenSource = new CancellationTokenSource();

        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

        privateBlobStorageService
            .SetupGetDownloadStream(
                container: PrivateReleaseFiles,
                path: releaseFile1.Path(),
                content: "Test ancillary blob",
                cancellationToken: tokenSource.Token);
        
        // After the first file has completed, we cancel the request
        // to prevent the next file from being fetched.
        privateBlobStorageService
            .SetupGetDownloadStream(
                container: PrivateReleaseFiles,
                path: releaseFile2.Path(),
                content: "Test ancillary blob 2",
                cancellationToken: tokenSource.Token,
                callback: tokenSource.Cancel);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(
                contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object);

            var fileIds = releaseFiles.Select(file => file.FileId).ToList();

            var result = await service.ZipFilesToStream(
                releaseVersionId: releaseVersion.Id,
                outputStream: stream,
                fileIds: fileIds,
                cancellationToken: tokenSource.Token
            );

            result.AssertRight();

            using var zip = ZipFile.OpenRead(path);

            // Entries are sorted alphabetically
            Assert.Single(zip.Entries);
            Assert.Equal("supporting-files/ancillary-1.pdf", zip.Entries[0].FullName);
            Assert.Equal("Test ancillary blob", zip.Entries[0].Open().ReadToEnd());
        }
    }

    [Fact]
    public async Task UpdateDataFileDetails()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            Name = "Test CSV File",
            File = new File
            {
                RootPath = releaseVersion.Id,
                Filename = "test.csv",
                Type = FileType.Data,
                Created = new DateTime(),
                CreatedBy = _dataFixture.DefaultUser()
                    .WithEmail("test@test.com")
                    .Generate(),
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.UpdateDataFileDetails(
                releaseFile.ReleaseVersionId,
                releaseFile.FileId,
                new ReleaseDataFileUpdateRequest
                {
                    Title = "New file title",
                    Summary = "New file summary"
                }
            );

            Assert.True(result.IsRight);
            Assert.IsType<Unit>(result.Right);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var updatedReleaseFile = await contentDbContext.ReleaseFiles
                .AsQueryable()
                .FirstAsync(rf =>
                    rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                    && rf.FileId == releaseFile.FileId);

            Assert.Equal("New file title", updatedReleaseFile.Name);
            Assert.Equal("New file summary", updatedReleaseFile.Summary);
        }
    }

    [Theory]
    [InlineData("New file title", "New file title", null, "Old file summary")]
    [InlineData(null, "Old file title", "New file summary", "New file summary")]
    [InlineData(null, "Old file title", null, "Old file summary")]
    public async Task UpdateDataFileDetails_OnlyTitleOrSummary(
        string? requestedFileName,
        string? expectedFileName,
        string? requestedFileSummary,
        string? expectedFileSummary)
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var originalPublishedDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            Name = "Old file title",
            Summary = "Old file summary",
            File = new File
            {
                RootPath = releaseVersion.Id,
                Type = FileType.Data,
                Filename = "test.csv",
            },
            Published = originalPublishedDate,
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.UpdateDataFileDetails(
                releaseFile.ReleaseVersionId,
                releaseFile.FileId,
                new ReleaseDataFileUpdateRequest
                {
                    Title = requestedFileName,
                    Summary = requestedFileSummary,
                }
            );

            Assert.True(result.IsRight);
            Assert.IsType<Unit>(result.Right);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var updatedReleaseFile = await contentDbContext.ReleaseFiles
                .AsQueryable()
                .FirstAsync(rf =>
                    rf.ReleaseVersionId == releaseFile.ReleaseVersionId
                    && rf.FileId == releaseFile.FileId);

            Assert.Equal(expectedFileName, updatedReleaseFile.Name);
            Assert.Equal(expectedFileSummary, updatedReleaseFile.Summary);

            if (string.IsNullOrWhiteSpace(requestedFileName) &&
                string.IsNullOrWhiteSpace(requestedFileSummary))
            {
                Assert.Equal(originalPublishedDate, updatedReleaseFile.Published);
            }
            else
            {
                Assert.Null(updatedReleaseFile.Published);
            }
        }
    }

    [Fact]
    public async Task UpdateDataFileDetails_NoRelease()
    {
        await using var contentDbContext = InMemoryContentDbContext();
        var service = SetupReleaseFileService(contentDbContext);

        var result = await service.UpdateDataFileDetails(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new ReleaseDataFileUpdateRequest
            {
                Title = "New file title",
            }
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateDataFileDetails_NoReleaseFile()
    {
        var releaseVersion = new ReleaseVersion();

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = SetupReleaseFileService(contentDbContext);

            var result = await service.UpdateDataFileDetails(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new ReleaseDataFileUpdateRequest
                {
                    Title = "New file title",
                }
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UploadAncillary()
    {
        const string filename = "ancillary.pdf";

        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var formFile = CreateFormFileMock(filename, "application/pdf").Object;
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        var fileValidatorService = new Mock<IFileValidatorService>(Strict);

        privateBlobStorageService.Setup(mock =>
            mock.UploadFile(PrivateReleaseFiles,
                It.Is<string>(path =>
                    path.Contains(FilesPath(releaseVersion.Id, Ancillary))),
                formFile
            )).Returns(Task.CompletedTask);

        fileValidatorService.Setup(mock =>
                mock.ValidateFileForUpload(formFile, Ancillary))
            .ReturnsAsync(Unit.Instance);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                fileValidatorService: fileValidatorService.Object);

            var result = await service.UploadAncillary(
                releaseVersion.Id,
                new ReleaseAncillaryFileUploadRequest
                {
                    Title = "Test name",
                    Summary = "Test summary",
                    File = formFile
                }
            );

            MockUtils.VerifyAllMocks(privateBlobStorageService, fileValidatorService);

            var fileInfo = result.AssertRight();

            fileValidatorService.Verify(mock =>
                mock.ValidateFileForUpload(formFile, Ancillary), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(releaseVersion.Id, Ancillary))),
                    formFile
                ), Times.Once);

            Assert.True(fileInfo.Id.HasValue);
            Assert.Equal("pdf", fileInfo.Extension);
            Assert.Equal("ancillary.pdf", fileInfo.FileName);
            Assert.Equal("Test name", fileInfo.Name);
            Assert.Equal("Test summary", fileInfo.Summary);
            Assert.Equal("10 Kb", fileInfo.Size);
            Assert.Equal(Ancillary, fileInfo.Type);
            Assert.Equal("test@test.com", fileInfo.UserName);
            Assert.InRange(DateTime.UtcNow.Subtract(fileInfo.Created.GetValueOrDefault()).Milliseconds, 0, 1500);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFile = await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleOrDefaultAsync(rf =>
                    rf.ReleaseVersionId == releaseVersion.Id
                    && rf.File.Filename == filename
                    && rf.File.Type == Ancillary
                );

            Assert.NotNull(releaseFile);
            var file = releaseFile.File;

            Assert.Equal(10240, file.ContentLength);
            Assert.Equal("application/pdf", file.ContentType);
            Assert.InRange(DateTime.UtcNow.Subtract(file.Created.GetValueOrDefault()).Milliseconds, 0, 1500);
            Assert.Equal(_user.Id, file.CreatedById);
        }
    }

    [Fact]
    public async Task UpdateAncillary()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "oldAncillary.csv",
                Type = Ancillary,
                RootPath = releaseVersion.Id,
                ContentType = "text/csv",
                ContentLength = 1024,
                Created = DateTime.UtcNow.AddDays(-1),
            },
            Name = "Ancillary name",
            Summary = "Ancillary summary",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.Add(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var newFormFile = CreateFormFileMock("newAncillary.pdf", "application/pdf").Object;
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        var fileValidatorService = new Mock<IFileValidatorService>(Strict);

        privateBlobStorageService.Setup(mock =>
                mock.DeleteBlob(
                    PrivateReleaseFiles,
                    releaseFile.File.Path()))
            .Returns(Task.CompletedTask);

        privateBlobStorageService.Setup(mock =>
            mock.UploadFile(PrivateReleaseFiles,
                It.Is<string>(path =>
                    path.Contains(FilesPath(releaseVersion.Id, Ancillary))),
                newFormFile
            )).Returns(Task.CompletedTask);

        fileValidatorService.Setup(mock =>
                mock.ValidateFileForUpload(newFormFile, Ancillary))
            .ReturnsAsync(Unit.Instance);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                fileValidatorService: fileValidatorService.Object);

            var result = await service.UpdateAncillary(
                releaseVersionId: releaseVersion.Id,
                fileId: releaseFile.FileId,
                request: new ReleaseAncillaryFileUpdateRequest
                {
                    File = newFormFile,
                    Title = "New ancillary name",
                    Summary = "New ancillary summary",
                });

            MockUtils.VerifyAllMocks(privateBlobStorageService, fileValidatorService);

            var fileInfo = result.AssertRight();

            fileValidatorService.Verify(mock =>
                mock.ValidateFileForUpload(newFormFile, Ancillary), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(releaseVersion.Id, Ancillary))),
                    newFormFile
                ), Times.Once);

            Assert.True(fileInfo.Id.HasValue);
            Assert.Equal("pdf", fileInfo.Extension);
            Assert.Equal("newAncillary.pdf", fileInfo.FileName);
            Assert.Equal("New ancillary name", fileInfo.Name);
            Assert.Equal("New ancillary summary", fileInfo.Summary);
            Assert.Equal("10 Kb", fileInfo.Size);
            Assert.Equal(Ancillary, fileInfo.Type);
            Assert.Equal("test@test.com", fileInfo.UserName);
            Assert.InRange(DateTime.UtcNow.Subtract(fileInfo.Created.GetValueOrDefault()).Milliseconds, 0, 1500);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            Assert.Null(await contentDbContext.Files
                .FirstOrDefaultAsync(f => f.Id == releaseFile.FileId));

            Assert.Null(await contentDbContext.ReleaseFiles
                .FirstOrDefaultAsync(rf =>
                    rf.ReleaseVersionId == releaseVersion.Id
                    && rf.FileId == releaseFile.FileId));

            var dbNewReleaseFile = Assert.Single(await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf =>
                    rf.ReleaseVersionId == releaseVersion.Id
                    && rf.File.Filename == "newAncillary.pdf"
                    && rf.File.Type == Ancillary)
                .ToListAsync());

            Assert.Equal("New ancillary name", dbNewReleaseFile.Name);
            Assert.Equal("New ancillary summary", dbNewReleaseFile.Summary);

            var newFile = dbNewReleaseFile.File;

            Assert.Equal(10240, newFile.ContentLength);
            Assert.Equal("application/pdf", newFile.ContentType);
            Assert.InRange(DateTime.UtcNow.Subtract(newFile.Created.GetValueOrDefault()).Milliseconds, 0, 1500);
            Assert.Equal(_user.Id, newFile.CreatedById);
        }
    }

    [Fact]
    public async Task UpdateAncillary_DoNotRemoveFileAttachedToOtherRelease()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "oldAncillary.csv",
                Type = Ancillary,
                RootPath = releaseVersion.Id,
                ContentType = "text/csv",
                ContentLength = 1024,
                Created = DateTime.UtcNow.AddDays(-1),
            },
            Name = "Ancillary name",
            Summary = "Ancillary summary",
        };

        var otherReleaseFile = new ReleaseFile
        {
            ReleaseVersion = new ReleaseVersion(),
            File = releaseFile.File,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFile, otherReleaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var newFormFile = CreateFormFileMock("newAncillary.pdf", "application/pdf").Object;
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        var fileValidatorService = new Mock<IFileValidatorService>(Strict);

        privateBlobStorageService.Setup(mock =>
            mock.UploadFile(PrivateReleaseFiles,
                It.Is<string>(path =>
                    path.Contains(FilesPath(releaseVersion.Id, Ancillary))),
                newFormFile
            )).Returns(Task.CompletedTask);

        fileValidatorService.Setup(mock =>
                mock.ValidateFileForUpload(newFormFile, Ancillary))
            .ReturnsAsync(Unit.Instance);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                fileValidatorService: fileValidatorService.Object);

            var result = await service.UpdateAncillary(
                releaseVersionId: releaseVersion.Id,
                fileId: releaseFile.FileId,
                request: new ReleaseAncillaryFileUpdateRequest
                {
                    File = newFormFile,
                    Title = "New ancillary name",
                    Summary = "New ancillary summary",
                });

            MockUtils.VerifyAllMocks(privateBlobStorageService, fileValidatorService);

            var fileInfo = result.AssertRight();

            fileValidatorService.Verify(mock =>
                mock.ValidateFileForUpload(newFormFile, Ancillary), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(releaseVersion.Id, Ancillary))),
                    newFormFile
                ), Times.Once);

            Assert.True(fileInfo.Id.HasValue);
            Assert.Equal("New ancillary name", fileInfo.Name);
            Assert.Equal("New ancillary summary", fileInfo.Summary);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            // File shouldn't be removed as its attached to another release
            Assert.NotNull(await contentDbContext.Files
                .FirstOrDefaultAsync(f => f.Id == releaseFile.FileId));
            Assert.NotNull(await contentDbContext.ReleaseFiles
                .FirstOrDefaultAsync(rf =>
                    rf.ReleaseVersionId == otherReleaseFile.ReleaseVersionId
                    && rf.FileId == otherReleaseFile.FileId));

            Assert.Null(await contentDbContext.ReleaseFiles
                .FirstOrDefaultAsync(rf =>
                    rf.ReleaseVersionId == releaseVersion.Id
                    && rf.FileId == releaseFile.FileId));

            var dbNewReleaseFile = Assert.Single(await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .Where(rf =>
                    rf.ReleaseVersionId == releaseVersion.Id
                    && rf.File.Filename == "newAncillary.pdf"
                    && rf.File.Type == Ancillary)
                .ToListAsync());
            Assert.Equal("New ancillary name", dbNewReleaseFile.Name);
            Assert.Equal("New ancillary summary", dbNewReleaseFile.Summary);
        }
    }

    [Fact]
    public async Task UpdateAncillary_NoFile()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
        };

        var releaseFile = new ReleaseFile
        {
            ReleaseVersion = releaseVersion,
            File = new File
            {
                Filename = "oldAncillary.csv",
                Type = Ancillary,
                RootPath = releaseVersion.Id,
                ContentType = "text/csv",
                ContentLength = 1024,
                Created = DateTime.UtcNow.AddDays(-1),
            },
            Name = "Ancillary name",
            Summary = "Ancillary summary",
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            contentDbContext.ReleaseFiles.AddRange(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.UpdateAncillary(
                releaseVersionId: releaseVersion.Id,
                fileId: releaseFile.FileId,
                request: new ReleaseAncillaryFileUpdateRequest
                {
                    Title = "New ancillary name",
                    Summary = "New ancillary summary",
                });

            var fileInfo = result.AssertRight();

            Assert.True(fileInfo.Id.HasValue);
            Assert.Equal("New ancillary name", fileInfo.Name);
            Assert.Equal("New ancillary summary", fileInfo.Summary);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            Assert.NotNull(await contentDbContext.Files
                .FirstOrDefaultAsync(f => f.Id == releaseFile.FileId));

            var dbReleaseFile = Assert.Single(await contentDbContext.ReleaseFiles.ToListAsync());
            Assert.Equal(releaseVersion.Id, dbReleaseFile.ReleaseVersionId);
            Assert.Equal(releaseFile.FileId, dbReleaseFile.FileId);
            Assert.Equal("New ancillary name", dbReleaseFile.Name);
            Assert.Equal("New ancillary summary", dbReleaseFile.Summary);
        }
    }

    [Fact]
    public async Task UploadChart()
    {
        const string filename = "chart.png";

        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var formFile = CreateFormFileMock(filename, "image/png").Object;
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);
        var fileValidatorService = new Mock<IFileValidatorService>(Strict);

        privateBlobStorageService.Setup(mock =>
            mock.UploadFile(PrivateReleaseFiles,
                It.Is<string>(path =>
                    path.Contains(FilesPath(releaseVersion.Id, Chart))),
                formFile
            )).Returns(Task.CompletedTask);

        fileValidatorService.Setup(mock =>
                mock.ValidateFileForUpload(formFile, Chart))
            .ReturnsAsync(Unit.Instance);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                privateBlobStorageService: privateBlobStorageService.Object,
                fileValidatorService: fileValidatorService.Object);

            var result = await service.UploadChart(releaseVersion.Id, formFile);

            MockUtils.VerifyAllMocks(privateBlobStorageService, fileValidatorService);

            Assert.True(result.IsRight);

            fileValidatorService.Verify(mock =>
                mock.ValidateFileForUpload(formFile, Chart), Times.Once);

            privateBlobStorageService.Verify(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(releaseVersion.Id, Chart))),
                    formFile
                ), Times.Once);

            Assert.True(result.Right.Id.HasValue);
            Assert.Equal("png", result.Right.Extension);
            Assert.Equal("chart.png", result.Right.FileName);
            Assert.Equal("", result.Right.Name);
            Assert.Equal("10 Kb", result.Right.Size);
            Assert.Equal(Chart, result.Right.Type);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var releaseFile = await contentDbContext.ReleaseFiles
                .Include(rf => rf.File)
                .SingleOrDefaultAsync(rf =>
                    rf.ReleaseVersionId == releaseVersion.Id
                    && rf.File.Filename == filename
                    && rf.File.Type == Chart
                );

            Assert.NotNull(releaseFile);
            var file = releaseFile.File;

            Assert.Equal(10240, file.ContentLength);
            Assert.Equal("image/png", file.ContentType);
            file.Created.AssertUtcNow();
            Assert.Equal(_user.Id, file.CreatedById);
        }
    }

    private string GenerateZipFilePath()
    {
        var path = Path.GetTempPath() + Guid.NewGuid() + ".zip";
        _filePaths.Add(path);

        return path;
    }

    private ReleaseFileService SetupReleaseFileService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IFileRepository? fileRepository = null,
        IFileValidatorService? fileValidatorService = null,
        IReleaseFileRepository? releaseFileRepository = null,
        IDataGuidanceFileWriter? dataGuidanceFileWriter = null,
        IUserService? userService = null)
    {
        contentDbContext.Users.Add(_user);
        contentDbContext.SaveChanges();

        return new ReleaseFileService(
            contentDbContext,
            contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(Strict),
            fileRepository ?? new FileRepository(contentDbContext),
            fileValidatorService ?? Mock.Of<IFileValidatorService>(Strict),
            releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
            dataGuidanceFileWriter ?? Mock.Of<IDataGuidanceFileWriter>(Strict),
            userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
        );
    }
}
