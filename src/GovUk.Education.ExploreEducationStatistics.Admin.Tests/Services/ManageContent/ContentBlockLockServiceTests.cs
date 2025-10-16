    #nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent;

public class ContentBlockLockServiceTests
{
    private readonly DataFixture _dataFixture = new();
    private readonly Guid _defaultUserId = Guid.NewGuid();

    [Fact]
    public async Task LockContentBlock()
    {
        var releaseVersion = new ReleaseVersion();

        var user = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var contentBlock = new HtmlBlock
        {
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Add(contentBlock);
            contentDbContext.Users.Add(user);
            await contentDbContext.SaveChangesAsync();
        }

        ContentBlockLockViewModel viewModel;

        var client = new Mock<IReleaseContentHubClient>();
        var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

        hubContext
            .Setup(s => s.Clients.Group(releaseVersion.Id.ToString()))
            .Returns(client.Object);

        var userRepository = new Mock<IUserRepository>(Strict);
        userRepository
            .Setup(s => s.FindActiveUserById(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext, 
                hubContext: hubContext.Object,
                userRepository: userRepository.Object);

            var result = await service.LockContentBlock(contentBlock.Id);

            viewModel = result.AssertRight();

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel), Times.Once);

            VerifyAllMocks(
                client, 
                hubContext,
                userRepository);

            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(releaseVersion.Id, viewModel.ReleaseVersionId);
            Assert.Equal("Jane Doe", viewModel.LockedBy.DisplayName);
            Assert.Equal("jane@test.com", viewModel.LockedBy.Email);

            Assert.True(DateTime.UtcNow > viewModel.Locked);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.Equal(viewModel.Locked.DateTime, savedContentBlock!.Locked);
            Assert.Equal(viewModel.LockedUntil.DateTime, savedContentBlock.LockedUntil);
            Assert.Equal(viewModel.LockedUntil.Offset, TimeSpan.Zero);
            Assert.Equal(_defaultUserId, savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task LockContentBlock_RefreshesExistingLock()
    {
        var releaseVersion = new ReleaseVersion();

        var user = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var previousLocked = DateTime.UtcNow.AddMinutes(-9);

        var contentBlock = new HtmlBlock
        {
            Locked = previousLocked,
            LockedBy = user,
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Add(contentBlock);
            contentDbContext.Users.Add(user);
            await contentDbContext.SaveChangesAsync();
        }

        ContentBlockLockViewModel viewModel;

        var client = new Mock<IReleaseContentHubClient>();
        var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

        hubContext
            .Setup(s => s.Clients.Group(releaseVersion.Id.ToString()))
            .Returns(client.Object);

        var userRepository = new Mock<IUserRepository>(Strict);
        userRepository
            .Setup(s => s.FindActiveUserById(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                hubContext: hubContext.Object,
                userRepository: userRepository.Object);

            var result = await service.LockContentBlock(contentBlock.Id);

            viewModel = result.AssertRight();

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel), Times.Once);

            VerifyAllMocks(
                client, 
                hubContext,
                userRepository);

            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(releaseVersion.Id, viewModel.ReleaseVersionId);
            Assert.Equal("Jane Doe", viewModel.LockedBy.DisplayName);
            Assert.Equal("jane@test.com", viewModel.LockedBy.Email);

            Assert.True(DateTime.UtcNow > viewModel.Locked);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.Equal(viewModel.Locked.DateTime, savedContentBlock!.Locked);
            Assert.Equal(viewModel.Locked.Offset, TimeSpan.Zero);
            Assert.Equal(viewModel.LockedUntil.DateTime, savedContentBlock.LockedUntil);
            Assert.Equal(viewModel.LockedUntil.Offset, TimeSpan.Zero);
            // Lock has been updated
            Assert.NotEqual(previousLocked, savedContentBlock.Locked);
            Assert.NotEqual(previousLocked, viewModel.Locked.DateTime);
            Assert.True(DateTime.UtcNow > savedContentBlock.Locked);

            Assert.NotNull(savedContentBlock.Updated);
            Assert.Equal(_defaultUserId, savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task LockContentBlock_PreviousLockExpired()
    {
        var releaseVersion = new ReleaseVersion();

        var previousUser = _dataFixture.DefaultUser()
            .WithFirstName("Rob")
            .WithLastName("Rowe")
            .WithEmail("rob@test.com")
            .Generate();

        var nextUser = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var contentBlock = new HtmlBlock
        {
            // Block was locked a while ago, but lock has
            // not been updated since, meaning that the user
            // is either idle or has disconnected.
            // As a sufficient amount of time has passed, we should
            // allow another user to obtain the lock.
            Locked = DateTime.UtcNow.AddMinutes(-11),
            LockedBy = previousUser,
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            contentDbContext.Users.Add(nextUser);
            await contentDbContext.SaveChangesAsync();
        }

        ContentBlockLockViewModel viewModel;

        var client = new Mock<IReleaseContentHubClient>();
        var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

        hubContext
            .Setup(s => s.Clients.Group(releaseVersion.Id.ToString()))
            .Returns(client.Object);

        var userRepository = new Mock<IUserRepository>(Strict);
        userRepository
            .Setup(s => s.FindActiveUserById(nextUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nextUser);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext, 
                hubContext: hubContext.Object,
                userRepository: userRepository.Object);

            var result = await service.LockContentBlock(contentBlock.Id);

            viewModel = result.AssertRight();

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel), Times.Once);

            VerifyAllMocks(
                client, 
                hubContext,
                userRepository);

            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(releaseVersion.Id, viewModel.ReleaseVersionId);
            Assert.Equal("Jane Doe", viewModel.LockedBy.DisplayName);
            Assert.Equal("jane@test.com", viewModel.LockedBy.Email);

            Assert.True(DateTime.UtcNow > viewModel.Locked);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.True(DateTime.UtcNow > savedContentBlock!.Locked);
            Assert.Equal(viewModel.Locked.DateTime, savedContentBlock.Locked);
            Assert.Equal(viewModel.Locked.Offset, TimeSpan.Zero);
            Assert.Equal(viewModel.LockedUntil.DateTime, savedContentBlock.LockedUntil);
            Assert.Equal(viewModel.LockedUntil.Offset, TimeSpan.Zero);
            Assert.Equal(_defaultUserId, savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task LockContentBlock_Force()
    {
        var releaseVersion = new ReleaseVersion();

        var user = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var contentBlock = new HtmlBlock
        {
            // Lock is still valid, but we're using the
            // force flag to lock the block anyway.
            Locked = DateTime.UtcNow.AddMinutes(-9),
            LockedBy = _dataFixture.DefaultUser()
                .WithFirstName("Rob")
                .WithLastName("Rowe")
                .WithEmail("rob@test.com")
                .Generate(),
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            contentDbContext.Users.Add(user);
            await contentDbContext.SaveChangesAsync();
        }

        ContentBlockLockViewModel viewModel;

        var client = new Mock<IReleaseContentHubClient>();
        var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

        hubContext
            .Setup(s => s.Clients.Group(releaseVersion.Id.ToString()))
            .Returns(client.Object);

        var userRepository = new Mock<IUserRepository>(Strict);
        userRepository
            .Setup(s => s.FindActiveUserById(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext, 
                hubContext: hubContext.Object,
                userRepository: userRepository.Object);

            var result = await service.LockContentBlock(contentBlock.Id, force: true);

            viewModel = result.AssertRight();

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel), Times.Once);

            VerifyAllMocks(
                client, 
                hubContext,
                userRepository);

            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(releaseVersion.Id, viewModel.ReleaseVersionId);
            Assert.Equal("Jane Doe", viewModel.LockedBy.DisplayName);
            Assert.Equal("jane@test.com", viewModel.LockedBy.Email);

            Assert.True(DateTime.UtcNow > viewModel.Locked);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.Equal(viewModel.Locked.DateTime, savedContentBlock!.Locked);
            Assert.Equal(viewModel.Locked.Offset, TimeSpan.Zero);
            Assert.Equal(viewModel.LockedUntil.DateTime, savedContentBlock.LockedUntil);
            Assert.Equal(viewModel.LockedUntil.Offset, TimeSpan.Zero);
            Assert.Equal(_defaultUserId, savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task LockContentBlock_PreviousLockNotExpired()
    {
        var releaseVersion = new ReleaseVersion();

        var previousUser = _dataFixture.DefaultUser()
            .WithFirstName("Rob")
            .WithLastName("Rowe")
            .WithEmail("rob@test.com")
            .Generate();

        var nextUser = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var locked = DateTime.UtcNow.AddMinutes(-9);

        var contentBlock = new HtmlBlock
        {
            // Block was locked recently. Another user
            // should not be able to acquire the lock.
            Locked = locked,
            LockedBy = previousUser,
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            contentDbContext.Users.Add(nextUser);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext);

            var result = await service.LockContentBlock(contentBlock.Id);

            var viewModel = result.AssertRight();
            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(releaseVersion.Id, viewModel.ReleaseVersionId);
            Assert.Equal("Rob Rowe", viewModel.LockedBy.DisplayName);
            Assert.Equal("rob@test.com", viewModel.LockedBy.Email);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.Equal(locked, savedContentBlock!.Locked);
            Assert.Equal(locked.AddMinutes(10), savedContentBlock.LockedUntil);
            Assert.Equal(previousUser.Id, savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task LockContentBlock_ConcurrentUsers()
    {
        var releaseVersion = new ReleaseVersion();

        var user1 = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var user2 = _dataFixture.DefaultUser()
            .WithFirstName("Rob")
            .WithLastName("Rowe")
            .WithEmail("rob@test.com")
            .Generate();

        var contentBlock = new HtmlBlock
        {
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            contentDbContext.Add(contentBlock);
            contentDbContext.Users.AddRange(user1, user2);
            await contentDbContext.SaveChangesAsync();
        }

        ContentBlockLockViewModel viewModel1;

        var client = new Mock<IReleaseContentHubClient>();
        var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

        hubContext
            .Setup(s => s.Clients.Group(releaseVersion.Id.ToString()))
            .Returns(client.Object);

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(s => s.MatchesPolicy(It.IsAny<ReleaseVersion>(), CanUpdateSpecificReleaseVersion))
            .ReturnsAsync(true);
        userService
            .SetupSequence(s => s.GetUserId())
            .Returns(user1.Id)
            .Returns(user2.Id);

        var userRepository = new Mock<IUserRepository>(Strict);
        userRepository
            .Setup(s => s.FindActiveUserById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) =>
            {
                return id == user1.Id ? user1 :
                       id == user2.Id ? user2 :
                       throw new ArgumentException($"No user setup for Id {id}");
            });

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(
                contentDbContext: contentDbContext,
                hubContext: hubContext.Object,
                userService: userService.Object,
                userRepository: userRepository.Object);

            // Simulate simultaneous calls
            var task1 = service.LockContentBlock(contentBlock.Id);
            var task2 = service.LockContentBlock(contentBlock.Id);

            var results = await Task.WhenAll(task1, task2);

            viewModel1 = results[0].AssertRight();
            var viewModel2 = results[1].AssertRight();

            viewModel1.AssertDeepEqualTo(viewModel2);

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel1), Times.Once);

            VerifyAllMocks(
                client, 
                hubContext, 
                userService, 
                userRepository);

            Assert.Equal(contentBlock.Id, viewModel1.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel1.SectionId);
            Assert.Equal(releaseVersion.Id, viewModel1.ReleaseVersionId);
            Assert.Equal("Jane Doe", viewModel1.LockedBy.DisplayName);
            Assert.Equal("jane@test.com", viewModel1.LockedBy.Email);

            Assert.True(DateTime.UtcNow > viewModel1.Locked);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.True(DateTime.UtcNow > savedContentBlock!.Locked);
            Assert.Equal(viewModel1.LockedUntil.DateTime, savedContentBlock.LockedUntil);
            Assert.Equal(viewModel1.LockedUntil.Offset, TimeSpan.Zero);
            Assert.Equal(user1.Id, savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task LockContentBlock_NoContentBlock()
    {
        var contextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryContentDbContext(contextId);

        var service = BuildService(contentDbContext);
        var result = await service.LockContentBlock(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task LockContentBlock_NoRelease()
    {
        var contentBlock = new HtmlBlock
        {
            ContentSection = new ContentSection()
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(contentDbContext);
            var result = await service.LockContentBlock(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task UnlockContentBlock()
    {
        var releaseVersion = new ReleaseVersion();

        var user = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var contentBlock = new HtmlBlock
        {
            Locked = DateTime.UtcNow.AddMinutes(-9),
            LockedBy = user,
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddAsync(user);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            ContentBlockUnlockViewModel? viewModel = null;

            var match = new CaptureMatch<ContentBlockUnlockViewModel>(
                value => { viewModel = value; }
            );

            var client = new Mock<IReleaseContentHubClient>(Strict);

            // Clients in the release group are notified that the content block is unlocked
            client
                .Setup(s => s.ContentBlockUnlocked(Capture.With(match)))
                .Returns(Task.CompletedTask);

            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(releaseVersion.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.UnlockContentBlock(contentBlock.Id);

            VerifyAllMocks(client, hubContext);

            result.AssertRight();

            Assert.NotNull(viewModel);
            Assert.Equal(viewModel.Id, contentBlock.Id);
            Assert.Equal(viewModel.SectionId, contentBlock.ContentSectionId);
            Assert.Equal(viewModel.ReleaseVersionId, releaseVersion.Id);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks
                .Include(cb => cb.LockedBy)
                .FirstAsync(cb => cb.Id == contentBlock.Id);
            
            Assert.Null(savedContentBlock.Locked);
            Assert.Null(savedContentBlock.LockedBy);
            Assert.Null(savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task UnlockContentBlock_PreviousLockExpired()
    {
        var releaseVersion = new ReleaseVersion();

        var nextUser = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var previousUser = _dataFixture.DefaultUser()
            .WithFirstName("Rob")
            .WithLastName("Rowe")
            .WithEmail("rob@test.com")
            .Generate();

        var contentBlock = new HtmlBlock
        {
            // Block was locked a while ago and can now be
            // unlocked by other users as we assume the
            // user is now idle or has disconnected.
            Locked = DateTime.UtcNow.AddMinutes(-20),
            LockedBy = previousUser,
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddAsync(nextUser);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            ContentBlockUnlockViewModel? viewModel = null;

            var match = new CaptureMatch<ContentBlockUnlockViewModel>(
                value => { viewModel = value; }
            );

            var client = new Mock<IReleaseContentHubClient>(Strict);

            // Clients in the release group are notified that the content block is unlocked
            client
                .Setup(s => s.ContentBlockUnlocked(Capture.With(match)))
                .Returns(Task.CompletedTask);

            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(releaseVersion.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.UnlockContentBlock(contentBlock.Id);

            VerifyAllMocks(client, hubContext);

            result.AssertRight();

            Assert.NotNull(viewModel);
            Assert.Equal(viewModel.Id, contentBlock.Id);
            Assert.Equal(viewModel.SectionId, contentBlock.ContentSectionId);
            Assert.Equal(viewModel.ReleaseVersionId, releaseVersion.Id);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks
                .Include(cb => cb.LockedBy)
                .FirstAsync(cb => cb.Id == contentBlock.Id);

            Assert.Null(savedContentBlock.Locked);
            Assert.Null(savedContentBlock.LockedBy);
            Assert.Null(savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task UnlockContentBlock_PreviousLockNotExpired()
    {
        var releaseVersion = new ReleaseVersion();

        var nextUser = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var previousUser = _dataFixture.DefaultUser()
            .WithFirstName("Rob")
            .WithLastName("Rowe")
            .WithEmail("rob@test.com")
            .Generate();

        var locked = DateTime.UtcNow.AddMinutes(-5);

        var contentBlock = new HtmlBlock
        {
            // Block is locked by another user and the
            // lock has not expired yet, so another
            // user should not be able to unlock it.
            Locked = locked,
            LockedBy = previousUser,
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddAsync(nextUser);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(contentDbContext);

            var result = await service.UnlockContentBlock(contentBlock.Id);

            result.AssertConflict();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.Equal(locked, savedContentBlock!.Locked);
            Assert.Equal(previousUser.Id, savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task UnlockContentBlock_Force()
    {
        var releaseVersion = new ReleaseVersion();

        var nextUser = _dataFixture.DefaultUser()
            .WithId(_defaultUserId)
            .WithFirstName("Jane")
            .WithLastName("Doe")
            .WithEmail("jane@test.com")
            .Generate();

        var previousUser = _dataFixture.DefaultUser()
            .WithFirstName("Rob")
            .WithLastName("Rowe")
            .WithEmail("rob@test.com")
            .Generate();

        var locked = DateTime.UtcNow.AddMinutes(-5);

        var contentBlock = new HtmlBlock
        {
            // Locked by another user and has not expired yet,
            // so need to use force flag to unlock it.
            Locked = locked,
            LockedBy = previousUser,
            ContentSection = new ContentSection
            {
                ReleaseVersion = releaseVersion
            },
            ReleaseVersion = releaseVersion
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddAsync(nextUser);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            ContentBlockUnlockViewModel? viewModel = null;

            var match = new CaptureMatch<ContentBlockUnlockViewModel>(
                value => { viewModel = value; }
            );

            var client = new Mock<IReleaseContentHubClient>(Strict);

            // Clients in the release group are notified that the content block is unlocked
            client
                .Setup(s => s.ContentBlockUnlocked(Capture.With(match)))
                .Returns(Task.CompletedTask);

            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(releaseVersion.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.UnlockContentBlock(contentBlock.Id, force: true);

            VerifyAllMocks(client, hubContext);

            result.AssertRight();

            Assert.NotNull(viewModel);
            Assert.Equal(viewModel.Id, contentBlock.Id);
            Assert.Equal(viewModel.SectionId, contentBlock.ContentSectionId);
            Assert.Equal(viewModel.ReleaseVersionId, releaseVersion.Id);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks
                .Include(cb => cb.LockedBy)
                .FirstAsync(cb => cb.Id == contentBlock.Id);

            Assert.Null(savedContentBlock.Locked);
            Assert.Null(savedContentBlock.LockedBy);
            Assert.Null(savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task UnlockContentBlock_NoContentBlock()
    {
        var contextId = Guid.NewGuid().ToString();

        await using var contentDbContext = InMemoryContentDbContext(contextId);

        var service = BuildService(contentDbContext);
        var result = await service.UnlockContentBlock(Guid.NewGuid());

        result.AssertNotFound();
    }

    [Fact]
    public async Task UnlockContentBlock_NoRelease()
    {
        var contentBlock = new HtmlBlock
        {
            ContentSection = new ContentSection()
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var service = BuildService(contentDbContext);
            var result = await service.UnlockContentBlock(Guid.NewGuid());

            result.AssertNotFound();
        }
    }

    private ContentBlockLockService BuildService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IUserRepository? userRepository = null,
        IHubContext<ReleaseContentHub, IReleaseContentHubClient>? hubContext = null)
    {
        return new ContentBlockLockService(
            contentDbContext: contentDbContext,
            persistenceHelper: persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService: userService ?? AlwaysTrueUserService(_defaultUserId).Object,
            userRepository: userRepository ?? Mock.Of<IUserRepository>(Strict),
            hubContext: hubContext ?? Mock.Of<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict)
        );
    }
}
