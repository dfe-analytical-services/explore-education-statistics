#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class MethodologyImageServiceTests
    {
        [Fact]
        public async Task Stream()
        {
            var methodologyVersion = new MethodologyVersion();

            var methodologyFile = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    ContentType = "image/png",
                    Type = Image
                }
            };

            var fileData = new byte[] { 0 };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddAsync(methodologyFile);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

            publicBlobStorageService
                .SetupDownloadToStream(PublicMethodologyFiles, methodologyFile.Path(), fileData);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = await service.Stream(methodologyVersion.Id, methodologyFile.File.Id);

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                var fileStreamResult = result.AssertRight();

                Assert.Equal("image/png", fileStreamResult.ContentType);
                Assert.Equal("image.png", fileStreamResult.FileDownloadName);
                Assert.Equal(fileData, fileStreamResult.FileStream.ReadFully());
            }
        }

        [Fact]
        public async Task Stream_MethodologyNotFound()
        {
            var methodologyFile = new MethodologyFile
            {
                MethodologyVersion = new MethodologyVersion(),
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
                await contentDbContext.MethodologyFiles.AddAsync(methodologyFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext);

                var result = await service.Stream(Guid.NewGuid(), methodologyFile.File.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Stream_MethodologyFileNotFound()
        {
            var methodologyVersion = new MethodologyVersion();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext);

                var result = await service.Stream(methodologyVersion.Id, Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Stream_BlobDoesNotExist()
        {
            var methodologyVersion = new MethodologyVersion();

            var methodologyFile = new MethodologyFile
            {
                MethodologyVersion = methodologyVersion,
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
                await contentDbContext.MethodologyVersions.AddAsync(methodologyVersion);
                await contentDbContext.MethodologyFiles.AddAsync(methodologyFile);
                await contentDbContext.SaveChangesAsync();
            }

            var publicBlobStorageService = new Mock<IPublicBlobStorageService>(Strict);

            publicBlobStorageService.SetupDownloadToStreamNotFound(PublicMethodologyFiles, methodologyFile.Path());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupMethodologyImageService(contentDbContext: contentDbContext,
                    publicBlobStorageService: publicBlobStorageService.Object);

                var result = await service.Stream(methodologyVersion.Id, methodologyFile.File.Id);

                MockUtils.VerifyAllMocks(publicBlobStorageService);

                result.AssertNotFound();
            }
        }

        private static MethodologyImageService SetupMethodologyImageService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IPublicBlobStorageService? publicBlobStorageService = null)
        {
            return new MethodologyImageService(
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                publicBlobStorageService ?? new Mock<IPublicBlobStorageService>(Strict).Object,
                MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}
