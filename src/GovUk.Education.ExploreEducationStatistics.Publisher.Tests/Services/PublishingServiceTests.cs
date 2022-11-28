#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services
{
    public class PublishingServiceTests
    {
        private const string? PublicStorageConnectionString = "public-storage-conn";

        [Fact]
        public async Task PublishMethodologyFiles()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid()
            };

            var logger = new Mock<ILogger<PublishingService>>();
            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var privateBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.Get(methodologyVersion.Id))
                .ReturnsAsync(methodologyVersion);

            methodologyService.Setup(mock => mock.GetFiles(methodologyVersion.Id, Image))
                .ReturnsAsync(new List<File>());

            publicBlobStorageService.Setup(mock => mock.DeleteBlobs(
                    PublicMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    null))
                .Returns(Task.CompletedTask);

            privateBlobStorageService.Setup(mock => mock.CopyDirectory(
                PrivateMethodologyFiles,
                $"{methodologyVersion.Id}/",
                PublicMethodologyFiles,
                $"{methodologyVersion.Id}/",
                It.Is<IBlobStorageService.CopyDirectoryOptions>(options =>
                    options.DestinationConnectionString == PublicStorageConnectionString)))
                .ReturnsAsync(new List<BlobInfo>());

            var service = BuildPublishingService(publicStorageConnectionString: PublicStorageConnectionString,
                methodologyService: methodologyService.Object,
                publicBlobStorageService: publicBlobStorageService.Object,
                privateBlobStorageService: privateBlobStorageService.Object,
                logger: logger.Object);

            await service.PublishMethodologyFiles(methodologyVersion.Id);

            MockUtils.VerifyAllMocks(methodologyService, publicBlobStorageService, privateBlobStorageService);
        }

        [Fact]
        public async Task PublishMethodologyFilesIfApplicableForRelease_ReleaseHasNoRelatedMethodologies()
        {
            var releaseId = Guid.NewGuid();

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var privateBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetLatestByRelease(releaseId))
                .ReturnsAsync(new List<MethodologyVersion>());

            // No other invocations on the services expected because the release has no related methodologies

            var service = BuildPublishingService(methodologyService: methodologyService.Object,
                publicBlobStorageService: publicBlobStorageService.Object,
                privateBlobStorageService: privateBlobStorageService.Object,
                publicationRepository: publicationRepository.Object,
                releaseService: releaseService.Object);

            await service.PublishMethodologyFilesIfApplicableForRelease(releaseId);

            MockUtils.VerifyAllMocks(methodologyService,
                publicBlobStorageService,
                privateBlobStorageService,
                publicationRepository,
                releaseService);
        }

        [Fact]
        public async Task PublishMethodologyFilesIfApplicableForRelease_ReleaseHasDraftMethodology()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = Immediately,
                Status = Draft
            };

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var privateBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetLatestByRelease(release.Id))
                .ReturnsAsync(ListOf(methodologyVersion));

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(true);

            releaseService.Setup(mock => mock.Get(release.Id))
                .ReturnsAsync(release);

            // No invocations on the storage services expected because the methodology is draft

            var service = BuildPublishingService(methodologyService: methodologyService.Object,
                publicBlobStorageService: publicBlobStorageService.Object,
                privateBlobStorageService: privateBlobStorageService.Object,
                publicationRepository: publicationRepository.Object,
                releaseService: releaseService.Object);

            await service.PublishMethodologyFilesIfApplicableForRelease(release.Id);

            MockUtils.VerifyAllMocks(methodologyService,
                publicBlobStorageService,
                privateBlobStorageService,
                publicationRepository,
                releaseService);
        }

        [Fact]
        public async Task PublishMethodologyFilesIfApplicableForRelease_ReleaseHasMethodologyScheduledWithThisRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = release.Id,
                Status = Approved
            };

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var privateBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetLatestByRelease(release.Id))
                .ReturnsAsync(AsList(methodologyVersion));

            methodologyService.Setup(mock => mock.GetFiles(methodologyVersion.Id, Image))
                .ReturnsAsync(new List<File>());

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(true);

            // Invocations on the storage services expected because the methodology is scheduled with this release

            publicBlobStorageService.Setup(mock => mock.DeleteBlobs(
                    PublicMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    null))
                .Returns(Task.CompletedTask);

            privateBlobStorageService.Setup(mock => mock.CopyDirectory(
                    PrivateMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    PublicMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    It.Is<IBlobStorageService.CopyDirectoryOptions>(options =>
                        options.DestinationConnectionString == PublicStorageConnectionString)))
                .ReturnsAsync(new List<BlobInfo>());

            releaseService.Setup(mock => mock.Get(release.Id))
                .ReturnsAsync(release);

            var service = BuildPublishingService(publicStorageConnectionString: PublicStorageConnectionString,
                methodologyService: methodologyService.Object,
                publicBlobStorageService: publicBlobStorageService.Object,
                privateBlobStorageService: privateBlobStorageService.Object,
                publicationRepository: publicationRepository.Object,
                releaseService: releaseService.Object);

            await service.PublishMethodologyFilesIfApplicableForRelease(release.Id);

            MockUtils.VerifyAllMocks(methodologyService,
                publicBlobStorageService,
                privateBlobStorageService,
                publicationRepository,
                releaseService);
        }

        [Fact]
        public async Task PublishMethodologyFilesIfApplicableForRelease_ReleaseHasMethodologyScheduledWithOtherRelease()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = WithRelease,
                ScheduledWithReleaseId = Guid.NewGuid(),
                Status = Approved
            };

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var privateBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetLatestByRelease(release.Id))
                .ReturnsAsync(ListOf(methodologyVersion));

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(true);

            releaseService.Setup(mock => mock.Get(release.Id))
                .ReturnsAsync(release);

            // No invocations on the storage services expected because the methodology is scheduled with another release

            var service = BuildPublishingService(methodologyService: methodologyService.Object,
                publicBlobStorageService: publicBlobStorageService.Object,
                privateBlobStorageService: privateBlobStorageService.Object,
                publicationRepository: publicationRepository.Object,
                releaseService: releaseService.Object);

            await service.PublishMethodologyFilesIfApplicableForRelease(release.Id);

            MockUtils.VerifyAllMocks(methodologyService,
                publicBlobStorageService,
                privateBlobStorageService,
                publicationRepository,
                releaseService);
        }

        [Fact]
        public async Task
            PublishMethodologyFilesIfApplicableForRelease_FirstPublicReleaseHasMethodologyScheduledImmediately()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = Immediately,
                Status = Approved
            };

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var privateBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetLatestByRelease(release.Id))
                .ReturnsAsync(ListOf(methodologyVersion));

            methodologyService.Setup(mock => mock.GetFiles(methodologyVersion.Id, Image))
                .ReturnsAsync(new List<File>());

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(false);

            // Invocations on the storage services expected because this will be the first published release.
            // The methodology and its files will be published for the first time with this release

            publicBlobStorageService.Setup(mock => mock.DeleteBlobs(
                    PublicMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    null))
                .Returns(Task.CompletedTask);

            privateBlobStorageService.Setup(mock => mock.CopyDirectory(
                    PrivateMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    PublicMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    It.Is<IBlobStorageService.CopyDirectoryOptions>(options =>
                        options.DestinationConnectionString == PublicStorageConnectionString)))
                .ReturnsAsync(new List<BlobInfo>());

            releaseService.Setup(mock => mock.Get(release.Id))
                .ReturnsAsync(release);

            var service = BuildPublishingService(publicStorageConnectionString: PublicStorageConnectionString,
                methodologyService: methodologyService.Object,
                publicBlobStorageService: publicBlobStorageService.Object,
                privateBlobStorageService: privateBlobStorageService.Object,
                publicationRepository: publicationRepository.Object,
                releaseService: releaseService.Object);

            await service.PublishMethodologyFilesIfApplicableForRelease(release.Id);

            MockUtils.VerifyAllMocks(methodologyService,
                publicBlobStorageService,
                privateBlobStorageService,
                publicationRepository,
                releaseService);
        }

        [Fact]
        public async Task
            PublishMethodologyFilesIfApplicableForRelease_NotFirstPublicReleaseHasMethodologyScheduledImmediately()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
                PublicationId = Guid.NewGuid()
            };

            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid(),
                PublishingStrategy = Immediately,
                Status = Approved
            };

            var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var privateBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var publicationRepository = new Mock<IPublicationRepository>(MockBehavior.Strict);
            var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

            methodologyService.Setup(mock => mock.GetLatestByRelease(release.Id))
                .ReturnsAsync(ListOf(methodologyVersion));

            publicationRepository.Setup(mock => mock.IsPublished(release.PublicationId))
                .ReturnsAsync(true);

            releaseService.Setup(mock => mock.Get(release.Id))
                .ReturnsAsync(release);

            // No invocations on the storage services expected because the publication already has published releases.
            // Files for this methodology will be published independently of this release

            var service = BuildPublishingService(publicStorageConnectionString: PublicStorageConnectionString,
                methodologyService: methodologyService.Object,
                publicBlobStorageService: publicBlobStorageService.Object,
                privateBlobStorageService: privateBlobStorageService.Object,
                publicationRepository: publicationRepository.Object,
                releaseService: releaseService.Object);

            await service.PublishMethodologyFilesIfApplicableForRelease(release.Id);

            MockUtils.VerifyAllMocks(methodologyService,
                publicBlobStorageService,
                privateBlobStorageService,
                publicationRepository,
                releaseService);
        }

        [Fact]
        public async Task PublishStagedReleaseContent()
        {
            var publicBlobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            publicBlobStorageService.Setup(mock => mock.MoveDirectory(PublicContent,
                    PublicContentStagingPath(),
                    PublicContent,
                    string.Empty,
                    null))
                .Returns(Task.CompletedTask);

            var service = BuildPublishingService(
                publicBlobStorageService: publicBlobStorageService.Object);

            await service.PublishStagedReleaseContent();

            MockUtils.VerifyAllMocks(publicBlobStorageService);
        }

        private static PublishingService BuildPublishingService(
            string? publicStorageConnectionString = null,
            IBlobStorageService? privateBlobStorageService = null,
            IBlobStorageService? publicBlobStorageService = null,
            IMethodologyService? methodologyService = null,
            IPublicationRepository? publicationRepository = null,
            IReleaseService? releaseService = null,
            ILogger<PublishingService>? logger = null)
        {
            return new PublishingService(
                publicStorageConnectionString ?? "",
                privateBlobStorageService ?? Mock.Of<IBlobStorageService>(MockBehavior.Strict),
                publicBlobStorageService ?? Mock.Of<IBlobStorageService>(MockBehavior.Strict),
                methodologyService ?? Mock.Of<IMethodologyService>(MockBehavior.Strict),
                publicationRepository ?? Mock.Of<IPublicationRepository>(MockBehavior.Strict),
                releaseService ?? Mock.Of<IReleaseService>(MockBehavior.Strict),
                logger ?? Mock.Of<ILogger<PublishingService>>()
            );
        }
    }
}
