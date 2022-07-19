#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class FileMigrationServiceTests
{
    [Fact]
    public async Task MigrateFile_FileNotFound()
    {
        var service = BuildService(contentDbContext: InMemoryContentDbContext());

        var result = await service.MigrateFile(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task MigrateFile_BlobNotFound()
    {
        var file = new File
        {
            Id = Guid.NewGuid(),
            RootPath = Guid.NewGuid(),
            ContentType = null,
            ContentLength = null,
            Type = FileType.Ancillary
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddAsync(file);
            await contentDbContext.SaveChangesAsync();
        }

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService.SetupFindBlob(BlobContainers.PrivateReleaseFiles,
            file.Path(),
            blob: null);

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext: contentDbContext,
                blobStorageService.Object);

            var result = await service.MigrateFile(file.Id);

            MockUtils.VerifyAllMocks(blobStorageService);

            result.AssertNotFound();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var after = await contentDbContext.Files.SingleAsync(f => f.Id == file.Id);

            Assert.Null(after.ContentType);
            Assert.Null(after.ContentLength);
        }
    }

    [Fact]
    public async Task MigrateFile_ImageFileHasNoLink()
    {
        var file = new File
        {
            Id = Guid.NewGuid(),
            RootPath = Guid.NewGuid(),
            ContentType = null,
            ContentLength = null,
            Type = FileType.Image
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddAsync(file);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext: contentDbContext);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.MigrateFile(file.Id));
            Assert.Equal($"No release or methodology link found for image file '{file.Id}'.", exception.Message);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var after = await contentDbContext.Files.SingleAsync(f => f.Id == file.Id);

            Assert.Null(after.ContentType);
            Assert.Null(after.ContentLength);
        }
    }

    [Fact]
    public async Task MigrateFile()
    {
        var file = new File
        {
            Id = Guid.NewGuid(),
            RootPath = Guid.NewGuid(),
            ContentType = null,
            ContentLength = null,
            Type = FileType.Ancillary
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Files.AddAsync(file);
            await contentDbContext.SaveChangesAsync();
        }

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService.SetupFindBlob(BlobContainers.PrivateReleaseFiles,
            file.Path(),
            new BlobInfo(path: "not used",
                size: "not used",
                contentType: "application/pdf",
                contentLength: 1024));

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext: contentDbContext,
                blobStorageService.Object);

            var result = await service.MigrateFile(file.Id);

            MockUtils.VerifyAllMocks(blobStorageService);

            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var after = await contentDbContext.Files.SingleAsync(f => f.Id == file.Id);

            Assert.Equal("application/pdf", after.ContentType);
            Assert.Equal(1024, after.ContentLength);
        }
    }

    [Fact]
    public async Task MigrateFile_MigratesReleaseImageFile()
    {
        var releaseFile = new ReleaseFile
        {
            Release = new Release(),
            File = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                ContentType = null,
                ContentLength = null,
                Type = FileType.Image
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.ReleaseFiles.AddAsync(releaseFile);
            await contentDbContext.SaveChangesAsync();
        }

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService.SetupFindBlob(BlobContainers.PrivateReleaseFiles,
            releaseFile.Path(),
            new BlobInfo(path: "not used",
                size: "not used",
                contentType: "image/png",
                contentLength: 1024));

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext: contentDbContext,
                blobStorageService.Object);

            var result = await service.MigrateFile(releaseFile.File.Id);

            MockUtils.VerifyAllMocks(blobStorageService);

            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var after = await contentDbContext.Files.SingleAsync(f => f.Id == releaseFile.File.Id);

            Assert.Equal("image/png", after.ContentType);
            Assert.Equal(1024, after.ContentLength);
        }
    }

    [Fact]
    public async Task MigrateFile_MigratesMethodologyImageFile()
    {
        var methodologyFile = new MethodologyFile
        {
            MethodologyVersion = new MethodologyVersion(),
            File = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                ContentType = null,
                ContentLength = null,
                Type = FileType.Image
            }
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.MethodologyFiles.AddAsync(methodologyFile);
            await contentDbContext.SaveChangesAsync();
        }

        var blobStorageService = new Mock<IBlobStorageService>(Strict);

        blobStorageService.SetupFindBlob(BlobContainers.PrivateMethodologyFiles,
            methodologyFile.Path(),
            new BlobInfo(path: "not used",
                size: "not used",
                contentType: "image/png",
                contentLength: 1024));

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = BuildService(contentDbContext: contentDbContext,
                blobStorageService.Object);

            var result = await service.MigrateFile(methodologyFile.File.Id);

            MockUtils.VerifyAllMocks(blobStorageService);

            result.AssertRight();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var after = await contentDbContext.Files.SingleAsync(f => f.Id == methodologyFile.File.Id);

            Assert.Equal("image/png", after.ContentType);
            Assert.Equal(1024, after.ContentLength);
        }
    }

    private static FileMigrationService BuildService(
        ContentDbContext contentDbContext,
        IBlobStorageService? blobStorageService = null)
    {
        return new FileMigrationService(
            contentDbContext,
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            blobStorageService ?? Mock.Of<IBlobStorageService>(Strict));
    }
}
