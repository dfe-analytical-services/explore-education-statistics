#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies;

public class MethodologyApprovalServiceTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task UpdateApprovalStatus_MethodologyHasImages()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var imageFile1 = new MethodologyFile
        {
            MethodologyVersion = methodologyVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image1.png",
                Type = FileType.Image,
            },
        };

        var imageFile2 = new MethodologyFile
        {
            MethodologyVersion = methodologyVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image2.png",
                Type = FileType.Image,
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = Immediately,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(
                new List<HtmlBlock>
                {
                    new()
                    {
                        Body =
                            $@"
    <img src=""/api/methodologies/{{methodologyId}}/images/{imageFile1.File.Id}""/>
    <img src=""/api/methodologies/{{methodologyId}}/images/{imageFile2.File.Id}""/>",
                    },
                }
            );

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            VerifyAllMocks(contentService, methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Null(updatedMethodologyVersion.ScheduledWithReleaseVersion);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodology = await context
                .MethodologyVersions.Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(updatedMethodology.Published);
            Assert.Equal(Approved, updatedMethodology.Status);
            Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
            updatedMethodology.Updated.AssertUtcNow();
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingMethodologyWithUnusedImages()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var imageFile1 = new MethodologyFile
        {
            MethodologyVersion = methodologyVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image1.png",
                Type = FileType.Image,
            },
        };

        var imageFile2 = new MethodologyFile
        {
            MethodologyVersion = methodologyVersion,
            File = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "image2.png",
                Type = FileType.Image,
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = Immediately,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var imageService = new Mock<IMethodologyImageService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        imageService
            .Setup(mock =>
                mock.Delete(methodologyVersion.Id, new List<Guid> { imageFile1.File.Id, imageFile2.File.Id }, true)
            )
            .ReturnsAsync(Unit.Instance);

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyImageService: imageService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            imageService.Verify(
                mock =>
                    mock.Delete(methodologyVersion.Id, new List<Guid> { imageFile1.File.Id, imageFile2.File.Id }, true),
                Times.Once
            );

            VerifyAllMocks(contentService, imageService, methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Null(updatedMethodologyVersion.ScheduledWithReleaseVersion);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodology = await context
                .MethodologyVersions.Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(updatedMethodology.Published);
            Assert.Equal(Approved, updatedMethodology.Status);
            Assert.Equal(Immediately, updatedMethodology.PublishingStrategy);
            updatedMethodology.Updated.AssertUtcNow();
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingMethodologyAddsMethodologyStatus()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = Immediately,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            VerifyAllMocks(contentService, methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyStatus = await context.MethodologyStatus.SingleAsync(ms =>
                ms.MethodologyVersionId == methodologyVersion.Id
            );

            Assert.Equal("Test approval", methodologyStatus.InternalReleaseNote);
            Assert.Equal(Approved, methodologyStatus.ApprovalStatus);
            methodologyStatus.Created.AssertUtcNow();
            Assert.Equal(UserId, methodologyStatus.CreatedById);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_SubmittingForHigherReviewAddsMethodologyStatus()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Submitted for higher review",
            Status = HigherLevelReview,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
        var userReleaseRoleService = new Mock<IUserReleaseRoleService>(Strict);
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        userReleaseRoleService
            .Setup(mock => mock.ListLatestActiveUserReleaseRolesByPublication(publication.Id, ReleaseRole.Approver))
            .ReturnsAsync([]);

        userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, false, []);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                userReleaseRoleService: userReleaseRoleService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object
            );

            var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
            var updatedMethodologyVersion = result.AssertRight();

            VerifyAllMocks(
                contentService,
                methodologyVersionRepository,
                userPublicationRoleRepository,
                userReleaseRoleService
            );

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyStatus = await context.MethodologyStatus.SingleAsync(ms =>
                ms.MethodologyVersionId == methodologyVersion.Id
            );

            Assert.Equal("Submitted for higher review", methodologyStatus.InternalReleaseNote);
            Assert.Equal(HigherLevelReview, methodologyStatus.ApprovalStatus);
            methodologyStatus.Created.AssertUtcNow();
            Assert.Equal(UserId, methodologyStatus.CreatedById);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovedBackToDraftAddsMethodologyStatus()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            Status = Approved,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Moving to draft",
            Status = Draft,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
        var userReleaseRoleService = new Mock<IUserReleaseRoleService>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        userReleaseRoleService
            .Setup(mock => mock.ListLatestActiveUserReleaseRolesByPublication(publication.Id, ReleaseRole.Approver))
            .ReturnsAsync([]);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                userReleaseRoleService: userReleaseRoleService.Object
            );

            var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
            var updatedMethodologyVersion = result.AssertRight();

            VerifyAllMocks(contentService, methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyStatus = await context.MethodologyStatus.SingleAsync(ms =>
                ms.MethodologyVersionId == methodologyVersion.Id
            );

            Assert.Equal("Moving to draft", methodologyStatus.InternalReleaseNote);
            Assert.Equal(Draft, methodologyStatus.ApprovalStatus);
            methodologyStatus.Created.AssertUtcNow();
            Assert.Equal(UserId, methodologyStatus.CreatedById);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_NoStatusUpdateRequiredNoMethodologyStatusAdded()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            // Request tailored so MethodologyApprovalUpdateRequest.IsStatusUpdateRequired returns false
            Status = Draft,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: context);

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var methodologyStatuses = await context
                .MethodologyStatus.Where(ms => ms.MethodologyVersionId == methodologyVersion.Id)
                .ToListAsync();

            Assert.Empty(methodologyStatuses);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingUsingImmediateStrategy_NotCurrentlyPubliclyVisible()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            PublishingStrategy = Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = Immediately,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            VerifyAllMocks(contentService, methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.False(updatedMethodologyVersion.Published.HasValue);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Null(updatedMethodologyVersion.ScheduledWithReleaseVersion);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions.Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.False(updatedMethodologyVersion.Published.HasValue);
            Assert.Equal(Approved, updatedMethodologyVersion.Status);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            updatedMethodologyVersion.Updated.AssertUtcNow();
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingUsingImmediateStrategy()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            PublishingStrategy = Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = Immediately,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
        var publishingService = new Mock<IPublishingService>(Strict);
        var methodologyCacheService = new Mock<IMethodologyCacheService>(Strict);
        var redirectsCacheService = new Mock<IRedirectsCacheService>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(true);

        publishingService
            .Setup(mock => mock.PublishMethodologyFiles(methodologyVersion.Id, CancellationToken.None))
            .ReturnsAsync(Unit.Instance);

        methodologyCacheService
            .Setup(mock => mock.UpdateSummariesTree())
            .ReturnsAsync(
                new Either<ActionResult, List<AllMethodologiesThemeViewModel>>(
                    new List<AllMethodologiesThemeViewModel>()
                )
            );

        redirectsCacheService
            .Setup(mock => mock.UpdateRedirects())
            .ReturnsAsync(
                new RedirectsViewModel(
                    PublicationRedirects: [],
                    MethodologyRedirects: [],
                    ReleaseRedirectsByPublicationSlug: []
                )
            );

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                publishingService: publishingService.Object,
                methodologyCacheService: methodologyCacheService.Object,
                redirectsCacheService: redirectsCacheService.Object
            );

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            VerifyAllMocks(
                contentService,
                methodologyVersionRepository,
                publishingService,
                methodologyCacheService,
                redirectsCacheService
            );

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            updatedMethodologyVersion.Published.AssertUtcNow();
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Null(updatedMethodologyVersion.ScheduledWithReleaseVersion);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions.Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            updatedMethodologyVersion.Published.AssertUtcNow();
            Assert.Equal(Approved, updatedMethodologyVersion.Status);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            updatedMethodologyVersion.Updated.AssertUtcNow();
            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingUsingImmediateStrategy_ScheduledWithReleaseIsCleared()
    {
        Publication publication = _dataFixture.DefaultPublication();

        ReleaseVersion scheduledWithReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));

        var methodologyVersion = new MethodologyVersion
        {
            PublishingStrategy = WithRelease,
            Status = Approved,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
            // Existing ScheduledWithReleaseVersion should be cleared
            ScheduledWithReleaseVersion = scheduledWithReleaseVersion,
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = Immediately,
            Status = Approved,
            // Requested id should be ignored
            WithReleaseId = scheduledWithReleaseVersion.Id,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            VerifyAllMocks(contentService, methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
            Assert.Null(updatedMethodologyVersion.ScheduledWithReleaseVersion);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions.Include(mv => mv.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Equal(Approved, updatedMethodologyVersion.Status);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
            Assert.Null(updatedMethodologyVersion.ScheduledWithReleaseVersionId);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_NonLiveRelease()
    {
        Publication publication = _dataFixture.DefaultPublication();

        ReleaseVersion scheduledWithReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication));

        var methodologyVersion = new MethodologyVersion
        {
            PublishingStrategy = Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = WithRelease,
            WithReleaseId = scheduledWithReleaseVersion.Id,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            context.Publications.Add(publication);
            context.MethodologyVersions.Add(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            VerifyAllMocks(contentService, methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);
            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(WithRelease, updatedMethodologyVersion.PublishingStrategy);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);

            Assert.NotNull(updatedMethodologyVersion.ScheduledWithReleaseVersion);
            Assert.Equal(scheduledWithReleaseVersion.Id, updatedMethodologyVersion.ScheduledWithReleaseVersion!.Id);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions.Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Approved, updatedMethodologyVersion.Status);
            Assert.Equal(WithRelease, updatedMethodologyVersion.PublishingStrategy);
            Assert.Equal(scheduledWithReleaseVersion.Id, updatedMethodologyVersion.ScheduledWithReleaseVersionId);
            updatedMethodologyVersion.Updated.AssertUtcNow();
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_ReleaseIdMissing()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            PublishingStrategy = Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        // Create a request with a release Id that is null
        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = WithRelease,
            WithReleaseId = null,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: context);

            var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_ReleaseIdNotFound()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            PublishingStrategy = Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        // Create a request with a random release version Id that won't exist
        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = WithRelease,
            WithReleaseId = Guid.NewGuid(),
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.Publications.AddAsync(publication);
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: context);

            var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_ReleaseAlreadyPublished()
    {
        Publication publication = _dataFixture
            .DefaultPublication()
            .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);

        var methodologyVersion = new MethodologyVersion
        {
            PublishingStrategy = Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        // Include a published release version in the request which the methodology cannot be made dependant on
        var scheduledWithReleaseVersion = publication.Releases[0].Versions.Single(rv => rv is { Published: not null });

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = WithRelease,
            WithReleaseId = scheduledWithReleaseVersion.Id,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            context.Publications.Add(publication);
            context.MethodologyVersions.Add(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: context);

            var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
            result.AssertBadRequest(MethodologyCannotDependOnPublishedRelease);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_ApprovingUsingWithReleaseStrategy_ReleaseNotRelated()
    {
        var (publication1, publication2) = _dataFixture.DefaultPublication().GenerateTuple2();

        // Release version is not from the same publication as the one linked to the methodology
        ReleaseVersion scheduledWithReleaseVersion = _dataFixture
            .DefaultReleaseVersion()
            .WithRelease(_dataFixture.DefaultRelease().WithPublication(publication1));

        var methodologyVersion = new MethodologyVersion
        {
            PublishingStrategy = Immediately,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = "Publication title",
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication2 }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "Test approval",
            PublishingStrategy = WithRelease,
            WithReleaseId = scheduledWithReleaseVersion.Id,
            Status = Approved,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            context.AddRange(publication1, publication2);
            context.MethodologyVersions.Add(methodologyVersion);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: context);

            var result = await service.UpdateApprovalStatus(methodologyVersion.Id, request);
            result.AssertBadRequest(MethodologyCannotDependOnRelease);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_MovingApprovedMethodologyToDraft()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            Published = null,
            PublishingStrategy = Immediately,
            Status = Approved,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "A release note",
            PublishingStrategy = Immediately,
            Status = Draft,
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object
            );

            // Un-approving is allowed for users that can approve the methodology providing it's not publicly accessible
            // Test that un-approving alters the status
            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            VerifyAllMocks(contentService, methodologyVersionRepository);

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Null(updatedMethodologyVersion.ScheduledWithReleaseVersion);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions.Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Draft, updatedMethodologyVersion.Status);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            updatedMethodologyVersion.Updated.AssertUtcNow();
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }
    }

    [Fact]
    public async Task UpdateApprovalStatus_SubmitForHigherReview()
    {
        Publication publication = _dataFixture.DefaultPublication();

        var methodologyVersion = new MethodologyVersion
        {
            Published = null,
            Status = Draft,
            Methodology = new Methodology
            {
                OwningPublicationTitle = publication.Title,
                Publications = ListOf(new PublicationMethodology { Owner = true, Publication = publication }),
            },
        };

        var request = new MethodologyApprovalUpdateRequest
        {
            LatestInternalReleaseNote = "A release note",
            Status = HigherLevelReview,
        };

        var userPublicationRoles = _dataFixture
            .DefaultUserPublicationRole()
            .ForIndex(
                0,
                s =>
                    s.SetRole(PublicationRole.Allower)
                        .SetUser(_dataFixture.DefaultUser().WithEmail("publication-approver1@email.com"))
                        .SetPublication(publication)
            )
            // This one will not receive an email as only users with the Approver role should be notified
            .ForIndex(
                1,
                s =>
                    s.SetRole(PublicationRole.Owner)
                        .SetUser(_dataFixture.DefaultUser().WithEmail("publication-owner@email.com"))
                        .SetPublication(publication)
            )
            // This one will not receive an email as only users for the owning publication should be notified
            .ForIndex(
                2,
                s =>
                    s.SetRole(PublicationRole.Allower)
                        .SetUser(_dataFixture.DefaultUser().WithEmail("publication-approver2@email.com"))
                        .SetPublication(_dataFixture.DefaultPublication())
            )
            .Generate(3);

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            await context.MethodologyVersions.AddAsync(methodologyVersion);
            await context.SaveChangesAsync();
        }

        var contentService = new Mock<IMethodologyContentService>(Strict);
        var methodologyVersionRepository = new Mock<IMethodologyVersionRepository>(Strict);
        var userReleaseRoleService = new Mock<IUserReleaseRoleService>(Strict);
        var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(Strict);
        var emailTemplateService = new Mock<IEmailTemplateService>(Strict);

        contentService
            .Setup(mock => mock.GetContentBlocks<HtmlBlock>(methodologyVersion.Id))
            .ReturnsAsync(new List<HtmlBlock>());

        methodologyVersionRepository
            .Setup(mock => mock.IsToBePublished(It.Is<MethodologyVersion>(mv => mv.Id == methodologyVersion.Id)))
            .ReturnsAsync(false);

        userReleaseRoleService
            .Setup(mock => mock.ListLatestActiveUserReleaseRolesByPublication(publication.Id, ReleaseRole.Approver))
            .ReturnsAsync([
                _dataFixture
                    .DefaultUserReleaseRole()
                    .WithUser(_dataFixture.DefaultUser().WithEmail("release-approver@email.com"))
                    .WithReleaseVersion(
                        _dataFixture
                            .DefaultReleaseVersion()
                            .WithRelease(
                                _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
                            )
                    )
                    .WithRole(ReleaseRole.Approver)
                    .Generate(),
            ]);

        userPublicationRoleRepository.SetupQuery(ResourceRoleFilter.ActiveOnly, false, [.. userPublicationRoles]);

        emailTemplateService
            .Setup(mock =>
                mock.SendMethodologyHigherReviewEmail(
                    "release-approver@email.com",
                    methodologyVersion.Id,
                    methodologyVersion.Title
                )
            )
            .Returns(Unit.Instance);
        emailTemplateService
            .Setup(mock =>
                mock.SendMethodologyHigherReviewEmail(
                    "publication-approver1@email.com",
                    methodologyVersion.Id,
                    methodologyVersion.Title
                )
            )
            .Returns(Unit.Instance);

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(
                contentDbContext: context,
                methodologyContentService: contentService.Object,
                methodologyVersionRepository: methodologyVersionRepository.Object,
                userReleaseRoleService: userReleaseRoleService.Object,
                userPublicationRoleRepository: userPublicationRoleRepository.Object,
                emailTemplateService: emailTemplateService.Object
            );

            var updatedMethodologyVersion = (
                await service.UpdateApprovalStatus(methodologyVersion.Id, request)
            ).AssertRight();

            VerifyAllMocks(
                contentService,
                methodologyVersionRepository,
                userPublicationRoleRepository,
                userReleaseRoleService
            );

            Assert.Equal(methodologyVersion.Id, updatedMethodologyVersion.Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(Immediately, updatedMethodologyVersion.PublishingStrategy);
            Assert.Null(updatedMethodologyVersion.ScheduledWithReleaseVersion);
            Assert.Equal(request.Status, updatedMethodologyVersion.Status);
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }

        await using (var context = InMemoryApplicationDbContext(contentDbContextId))
        {
            var updatedMethodologyVersion = await context
                .MethodologyVersions.Include(m => m.Methodology)
                .SingleAsync(m => m.Id == methodologyVersion.Id);

            Assert.Null(updatedMethodologyVersion.Published);
            Assert.Equal(HigherLevelReview, updatedMethodologyVersion.Status);
            updatedMethodologyVersion.Updated.AssertUtcNow();
            Assert.Null(updatedMethodologyVersion.Methodology.LatestPublishedVersionId);
        }
    }

    private static MethodologyApprovalService SetupService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IMethodologyContentService? methodologyContentService = null,
        IMethodologyFileRepository? methodologyFileRepository = null,
        IMethodologyVersionRepository? methodologyVersionRepository = null,
        IMethodologyImageService? methodologyImageService = null,
        IPublishingService? publishingService = null,
        IUserService? userService = null,
        IUserReleaseRoleService? userReleaseRoleService = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IMethodologyCacheService? methodologyCacheService = null,
        IEmailTemplateService? emailTemplateService = null,
        IRedirectsCacheService? redirectsCacheService = null
    )
    {
        return new(
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            contentDbContext,
            methodologyContentService ?? Mock.Of<IMethodologyContentService>(Strict),
            methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
            methodologyVersionRepository ?? Mock.Of<IMethodologyVersionRepository>(Strict),
            methodologyImageService ?? Mock.Of<IMethodologyImageService>(Strict),
            publishingService ?? Mock.Of<IPublishingService>(Strict),
            userService ?? AlwaysTrueUserService(UserId).Object,
            userReleaseRoleService ?? Mock.Of<IUserReleaseRoleService>(Strict),
            userPublicationRoleRepository ?? Mock.Of<IUserPublicationRoleRepository>(Strict),
            methodologyCacheService ?? Mock.Of<IMethodologyCacheService>(Strict),
            emailTemplateService ?? Mock.Of<IEmailTemplateService>(Strict),
            redirectsCacheService ?? Mock.Of<IRedirectsCacheService>(Strict)
        );
    }
}
