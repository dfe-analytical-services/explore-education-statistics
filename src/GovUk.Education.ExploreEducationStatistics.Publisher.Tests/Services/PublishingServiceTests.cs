using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class PublishingServiceTests
{
    private const string PublicStorageConnectionString = "public-storage-conn";

    [Fact]
    public async Task PublishMethodologyFiles()
    {
        var methodologyVersion = new MethodologyVersion { Id = Guid.NewGuid() };

        var logger = new Mock<ILogger<PublishingService>>();
        var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);

        methodologyService
            .Setup(mock => mock.Get(methodologyVersion.Id))
            .ReturnsAsync(methodologyVersion);

        methodologyService
            .Setup(mock => mock.GetFiles(methodologyVersion.Id, Image))
            .ReturnsAsync(new List<File>());

        publicBlobStorageService
            .Setup(mock =>
                mock.DeleteBlobs(PublicMethodologyFiles, $"{methodologyVersion.Id}/", null)
            )
            .Returns(Task.CompletedTask);

        privateBlobStorageService
            .Setup(mock =>
                mock.CopyDirectory(
                    PrivateMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    PublicMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    It.Is<IBlobStorageService.CopyDirectoryOptions>(options =>
                        options.DestinationConnectionString == PublicStorageConnectionString
                    )
                )
            )
            .ReturnsAsync(new List<BlobInfo>());

        var service = BuildPublishingService(
            methodologyService: methodologyService.Object,
            publicBlobStorageService: publicBlobStorageService.Object,
            privateBlobStorageService: privateBlobStorageService.Object,
            logger: logger.Object
        );

        await service.PublishMethodologyFiles(methodologyVersion.Id);

        MockUtils.VerifyAllMocks(
            methodologyService,
            publicBlobStorageService,
            privateBlobStorageService
        );
    }

    [Fact]
    public async Task PublishMethodologyFilesIfApplicableForRelease_ReleaseHasNoRelatedMethodologies()
    {
        var releaseVersion = new ReleaseVersion { Id = Guid.NewGuid() };

        var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
        var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

        methodologyService
            .Setup(mock => mock.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync(new List<MethodologyVersion>());

        releaseService.Setup(mock => mock.Get(releaseVersion.Id)).ReturnsAsync(releaseVersion);

        // No other invocations on the services expected because the release has no related methodologies

        var service = BuildPublishingService(
            methodologyService: methodologyService.Object,
            releaseService: releaseService.Object
        );

        await service.PublishMethodologyFilesIfApplicableForRelease(releaseVersion.Id);

        MockUtils.VerifyAllMocks(methodologyService, releaseService);
    }

    [Fact]
    public async Task PublishMethodologyFilesIfApplicableForRelease_ReleaseHasDraftMethodology()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PublicationId = Guid.NewGuid(),
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = Immediately,
            Status = Draft,
        };

        var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

        methodologyService
            .Setup(mock =>
                mock.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion)
            )
            .ReturnsAsync(false);

        methodologyService
            .Setup(mock => mock.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync(ListOf(methodologyVersion));

        releaseService.Setup(mock => mock.Get(releaseVersion.Id)).ReturnsAsync(releaseVersion);

        // No invocations on the storage services expected because the methodology is draft

        var service = BuildPublishingService(
            methodologyService: methodologyService.Object,
            publicBlobStorageService: publicBlobStorageService.Object,
            privateBlobStorageService: privateBlobStorageService.Object,
            releaseService: releaseService.Object
        );

        await service.PublishMethodologyFilesIfApplicableForRelease(releaseVersion.Id);

        MockUtils.VerifyAllMocks(
            methodologyService,
            publicBlobStorageService,
            privateBlobStorageService,
            releaseService
        );
    }

    [Fact]
    public async Task PublishMethodologyFilesIfApplicableForRelease_ReleaseHasMethodologyScheduledWithThisRelease()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PublicationId = Guid.NewGuid(),
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = releaseVersion.Id,
            Status = Approved,
        };

        var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

        methodologyService
            .Setup(mock =>
                mock.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion)
            )
            .ReturnsAsync(true);

        methodologyService
            .Setup(mock => mock.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync(AsList(methodologyVersion));

        methodologyService
            .Setup(mock => mock.GetFiles(methodologyVersion.Id, Image))
            .ReturnsAsync(new List<File>());

        // Invocations on the storage services expected because the methodology is scheduled with this release

        publicBlobStorageService
            .Setup(mock =>
                mock.DeleteBlobs(PublicMethodologyFiles, $"{methodologyVersion.Id}/", null)
            )
            .Returns(Task.CompletedTask);

        privateBlobStorageService
            .Setup(mock =>
                mock.CopyDirectory(
                    PrivateMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    PublicMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    It.Is<IBlobStorageService.CopyDirectoryOptions>(options =>
                        options.DestinationConnectionString == PublicStorageConnectionString
                    )
                )
            )
            .ReturnsAsync(new List<BlobInfo>());

        releaseService.Setup(mock => mock.Get(releaseVersion.Id)).ReturnsAsync(releaseVersion);

        var service = BuildPublishingService(
            methodologyService: methodologyService.Object,
            publicBlobStorageService: publicBlobStorageService.Object,
            privateBlobStorageService: privateBlobStorageService.Object,
            releaseService: releaseService.Object
        );

        await service.PublishMethodologyFilesIfApplicableForRelease(releaseVersion.Id);

        MockUtils.VerifyAllMocks(
            methodologyService,
            publicBlobStorageService,
            privateBlobStorageService,
            releaseService
        );
    }

    [Fact]
    public async Task PublishMethodologyFilesIfApplicableForRelease_ReleaseHasMethodologyScheduledWithOtherRelease()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PublicationId = Guid.NewGuid(),
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = WithRelease,
            ScheduledWithReleaseVersionId = Guid.NewGuid(),
            Status = Approved,
        };

        var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

        methodologyService
            .Setup(mock =>
                mock.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion)
            )
            .ReturnsAsync(false);

        methodologyService
            .Setup(mock => mock.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync(ListOf(methodologyVersion));

        releaseService.Setup(mock => mock.Get(releaseVersion.Id)).ReturnsAsync(releaseVersion);

        // No invocations on the storage services expected because the methodology is scheduled with another release

        var service = BuildPublishingService(
            methodologyService: methodologyService.Object,
            publicBlobStorageService: publicBlobStorageService.Object,
            privateBlobStorageService: privateBlobStorageService.Object,
            releaseService: releaseService.Object
        );

        await service.PublishMethodologyFilesIfApplicableForRelease(releaseVersion.Id);

        MockUtils.VerifyAllMocks(
            methodologyService,
            publicBlobStorageService,
            privateBlobStorageService,
            releaseService
        );
    }

    [Fact]
    public async Task PublishMethodologyFilesIfApplicableForRelease_FirstPublicReleaseHasMethodologyScheduledImmediately()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PublicationId = Guid.NewGuid(),
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = Immediately,
            Status = Approved,
        };

        var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

        methodologyService
            .Setup(mock =>
                mock.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion)
            )
            .ReturnsAsync(true);

        methodologyService
            .Setup(mock => mock.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync(ListOf(methodologyVersion));

        methodologyService
            .Setup(mock => mock.GetFiles(methodologyVersion.Id, Image))
            .ReturnsAsync(new List<File>());

        // Invocations on the storage services expected because this will be the first published release.
        // The methodology and its files will be published for the first time with this release

        publicBlobStorageService
            .Setup(mock =>
                mock.DeleteBlobs(PublicMethodologyFiles, $"{methodologyVersion.Id}/", null)
            )
            .Returns(Task.CompletedTask);

        privateBlobStorageService
            .Setup(mock =>
                mock.CopyDirectory(
                    PrivateMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    PublicMethodologyFiles,
                    $"{methodologyVersion.Id}/",
                    It.Is<IBlobStorageService.CopyDirectoryOptions>(options =>
                        options.DestinationConnectionString == PublicStorageConnectionString
                    )
                )
            )
            .ReturnsAsync(new List<BlobInfo>());

        releaseService.Setup(mock => mock.Get(releaseVersion.Id)).ReturnsAsync(releaseVersion);

        var service = BuildPublishingService(
            methodologyService: methodologyService.Object,
            publicBlobStorageService: publicBlobStorageService.Object,
            privateBlobStorageService: privateBlobStorageService.Object,
            releaseService: releaseService.Object
        );

        await service.PublishMethodologyFilesIfApplicableForRelease(releaseVersion.Id);

        MockUtils.VerifyAllMocks(
            methodologyService,
            publicBlobStorageService,
            privateBlobStorageService,
            releaseService
        );
    }

    [Fact]
    public async Task PublishMethodologyFilesIfApplicableForRelease_NotFirstPublicReleaseHasMethodologyScheduledImmediately()
    {
        var releaseVersion = new ReleaseVersion
        {
            Id = Guid.NewGuid(),
            PublicationId = Guid.NewGuid(),
        };

        var methodologyVersion = new MethodologyVersion
        {
            Id = Guid.NewGuid(),
            PublishingStrategy = Immediately,
            Status = Approved,
        };

        var methodologyService = new Mock<IMethodologyService>(MockBehavior.Strict);
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(MockBehavior.Strict);
        var releaseService = new Mock<IReleaseService>(MockBehavior.Strict);

        methodologyService
            .Setup(mock =>
                mock.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion)
            )
            .ReturnsAsync(false);

        methodologyService
            .Setup(mock => mock.GetLatestVersionByRelease(releaseVersion))
            .ReturnsAsync(ListOf(methodologyVersion));

        releaseService.Setup(mock => mock.Get(releaseVersion.Id)).ReturnsAsync(releaseVersion);

        // No invocations on the storage services expected because the publication already has published releases.
        // Files for this methodology will be published independently of this release

        var service = BuildPublishingService(
            methodologyService: methodologyService.Object,
            publicBlobStorageService: publicBlobStorageService.Object,
            privateBlobStorageService: privateBlobStorageService.Object,
            releaseService: releaseService.Object
        );

        await service.PublishMethodologyFilesIfApplicableForRelease(releaseVersion.Id);

        MockUtils.VerifyAllMocks(
            methodologyService,
            publicBlobStorageService,
            privateBlobStorageService,
            releaseService
        );
    }

    [Fact]
    public async Task PublishStagedReleaseContent()
    {
        var publicBlobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);

        publicBlobStorageService
            .Setup(mock =>
                mock.MoveDirectory(
                    PublicContent,
                    PublicContentStagingPath(),
                    PublicContent,
                    string.Empty,
                    null
                )
            )
            .Returns(Task.CompletedTask);

        var service = BuildPublishingService(
            publicBlobStorageService: publicBlobStorageService.Object
        );

        await service.PublishStagedReleaseContent();

        MockUtils.VerifyAllMocks(publicBlobStorageService);
    }

    private static PublishingService BuildPublishingService(
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IPublicBlobStorageService? publicBlobStorageService = null,
        IMethodologyService? methodologyService = null,
        IReleaseService? releaseService = null,
        IOptions<AppOptions>? appOptions = null,
        ILogger<PublishingService>? logger = null
    )
    {
        return new PublishingService(
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(MockBehavior.Strict),
            publicBlobStorageService ?? Mock.Of<IPublicBlobStorageService>(MockBehavior.Strict),
            methodologyService ?? Mock.Of<IMethodologyService>(MockBehavior.Strict),
            releaseService ?? Mock.Of<IReleaseService>(MockBehavior.Strict),
            appOptions ?? DefaultAppOptions(),
            logger ?? Mock.Of<ILogger<PublishingService>>()
        );
    }

    private static IOptions<AppOptions> DefaultAppOptions()
    {
        return new AppOptions
        {
            PrivateStorageConnectionString = string.Empty,
            PublicStorageConnectionString = PublicStorageConnectionString,
            NotifierStorageConnectionString = string.Empty,
            PublisherStorageConnectionString = string.Empty,
            PublishScheduledReleasesFunctionCronSchedule = string.Empty,
            StageScheduledReleasesFunctionCronSchedule = string.Empty,
        }.ToOptionsWrapper();
    }
}
