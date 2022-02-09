#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.SignalR;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ManageContent;

public class ReleaseContentBlockServiceTests
{
    private readonly Guid _defaultUserId = Guid.NewGuid();

    [Fact]
    public async Task LockContentBlock()
    {
        var release = new Release();

        var user = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@test.com"
        };

        var contentBlock = new HtmlBlock
        {
            ContentSection = new ContentSection
            {
                Release = new ReleaseContentSection
                {
                    Release = release
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddAsync(user);
            await contentDbContext.SaveChangesAsync();
        }

        ReleaseContentBlockLockViewModel viewModel;

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var client = new Mock<IReleaseContentHubClient>();
            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(release.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.LockContentBlock(contentBlock.Id);

            viewModel = result.AssertRight();

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel), Times.Once);

            VerifyAllMocks(client, hubContext);

            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(release.Id, viewModel.ReleaseId);
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
        var release = new Release();

        var user = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@test.com"
        };

        var previousLocked = DateTime.UtcNow.AddMinutes(-9);

        var contentBlock = new HtmlBlock
        {
            Locked = previousLocked,
            LockedBy = user,
            ContentSection = new ContentSection
            {
                Release = new ReleaseContentSection
                {
                    Release = release
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddAsync(user);
            await contentDbContext.SaveChangesAsync();
        }

        ReleaseContentBlockLockViewModel viewModel;

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var client = new Mock<IReleaseContentHubClient>();
            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(release.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.LockContentBlock(contentBlock.Id);

            viewModel = result.AssertRight();

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel), Times.Once);

            VerifyAllMocks(client, hubContext);

            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(release.Id, viewModel.ReleaseId);
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
        var release = new Release();

        var previousUser = new User
        {
            FirstName = "Rob",
            LastName = "Rowe",
            Email = "rob@test.com"
        };

        var nextUser = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@test.com"
        };

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
                Release = new ReleaseContentSection
                {
                    Release = release
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddAsync(nextUser);
            await contentDbContext.SaveChangesAsync();
        }

        ReleaseContentBlockLockViewModel viewModel;

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var client = new Mock<IReleaseContentHubClient>();
            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(release.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.LockContentBlock(contentBlock.Id);

            viewModel = result.AssertRight();

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel), Times.Once);

            VerifyAllMocks(client, hubContext);

            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(release.Id, viewModel.ReleaseId);
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
        var release = new Release();

        var user = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@test.com"
        };

        var contentBlock = new HtmlBlock
        {
            // Lock is still valid, but we're using the
            // force flag to lock the block anyway.
            Locked = DateTime.UtcNow.AddMinutes(-9),
            LockedBy = new User
            {
                FirstName = "Rob",
                LastName = "Rowe",
                Email = "rob@test.com"
            },
            ContentSection = new ContentSection
            {
                Release = new ReleaseContentSection
                {
                    Release = release
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddAsync(user);
            await contentDbContext.SaveChangesAsync();
        }

        ReleaseContentBlockLockViewModel viewModel;

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var client = new Mock<IReleaseContentHubClient>();
            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(release.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.LockContentBlock(contentBlock.Id, force: true);

            viewModel = result.AssertRight();

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel), Times.Once);

            VerifyAllMocks(client, hubContext);

            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(release.Id, viewModel.ReleaseId);
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
        var release = new Release();

        var previousUser = new User
        {
            FirstName = "Rob",
            LastName = "Rowe",
            Email = "rob@test.com"
        };
        var nextUser = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@test.com"
        };

        var locked = DateTime.UtcNow.AddMinutes(-9);

        var contentBlock = new HtmlBlock
        {
            // Block was locked recently. Another user
            // should not be able to acquire the lock.
            Locked = locked,
            LockedBy = previousUser,
            ContentSection = new ContentSection
            {
                Release = new ReleaseContentSection
                {
                    Release = release
                }
            }
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
            var result = await service.LockContentBlock(contentBlock.Id);

            var viewModel = result.AssertRight();
            Assert.Equal(contentBlock.Id, viewModel.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel.SectionId);
            Assert.Equal(release.Id, viewModel.ReleaseId);
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
        var release = new Release();

        var user1 = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane@test.com"
        };
        var user2 = new User
        {
            FirstName = "Rob",
            LastName = "Rowe",
            Email = "rob@test.com"
        };

        var contentBlock = new HtmlBlock
        {
            ContentSection = new ContentSection
            {
                Release = new ReleaseContentSection
                {
                    Release = release
                }
            }
        };

        var contextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            await contentDbContext.AddAsync(contentBlock);
            await contentDbContext.AddRangeAsync(user1, user2);
            await contentDbContext.SaveChangesAsync();
        }

        ReleaseContentBlockLockViewModel viewModel1;

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var client = new Mock<IReleaseContentHubClient>();
            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(release.Id.ToString()))
                .Returns(client.Object);

            var userService = new Mock<IUserService>(Strict);

            userService
                .Setup(s => s.MatchesPolicy(It.IsAny<Release>(), CanUpdateSpecificRelease))
                .ReturnsAsync(true);

            userService
                .SetupSequence(s => s.GetUserId())
                .Returns(user1.Id)
                .Returns(user2.Id);

            var service = BuildService(
                contentDbContext,
                hubContext: hubContext.Object,
                userService: userService.Object);

            // Simulate simultaneous calls
            var task1 = service.LockContentBlock(contentBlock.Id);
            var task2 = service.LockContentBlock(contentBlock.Id);

            var results = await Task.WhenAll(task1, task2);

            viewModel1 = results[0].AssertRight();
            var viewModel2 = results[1].AssertRight();

            viewModel1.AssertDeepEqualTo(viewModel2);

            // Clients in the release group are notified that the content block is locked
            client.Verify(s => s.ContentBlockLocked(viewModel1), Times.Once);

            VerifyAllMocks(client, hubContext, userService);

            Assert.Equal(contentBlock.Id, viewModel1.Id);
            Assert.Equal(contentBlock.ContentSectionId, viewModel1.SectionId);
            Assert.Equal(release.Id, viewModel1.ReleaseId);
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
        var release = new Release();

        var user = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "test@test.com"
        };

        var contentBlock = new HtmlBlock
        {
            Locked = DateTime.UtcNow.AddMinutes(-9),
            LockedBy = user,
            ContentSection = new ContentSection
            {
                Release = new ReleaseContentSection
                {
                    Release = release,
                }
            }
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
            ReleaseContentBlockUnlockViewModel viewModel = null!;

            var match = new CaptureMatch<ReleaseContentBlockUnlockViewModel>(
                value => { viewModel = value; }
            );

            var client = new Mock<IReleaseContentHubClient>(Strict);

            // Clients in the release group are notified that the content block is unlocked
            client
                .Setup(s => s.ContentBlockUnlocked(Capture.With(match)))
                .Returns(Task.CompletedTask);

            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(release.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.UnlockContentBlock(contentBlock.Id);

            VerifyAllMocks(client, hubContext);

            result.AssertRight();

            Assert.Equal(viewModel.Id, contentBlock.Id);
            Assert.Equal(viewModel.SectionId, contentBlock.ContentSectionId);
            Assert.Equal(viewModel.ReleaseId, release.Id);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.Null(savedContentBlock!.Locked);
            Assert.Null(savedContentBlock.LockedBy);
            Assert.Null(savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task UnlockContentBlock_PreviousLockExpired()
    {
        var release = new Release();

        var nextUser = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "test@test.com"
        };
        var previousUser = new User
        {
            FirstName = "Rob",
            LastName = "Rowe",
            Email = "rob@test.com"
        };

        var contentBlock = new HtmlBlock
        {
            // Block was locked a while ago and can now be
            // unlocked by other users as we assume the
            // user is now idle or has disconnected.
            Locked = DateTime.UtcNow.AddMinutes(-20),
            LockedBy = previousUser,
            ContentSection = new ContentSection
            {
                Release = new ReleaseContentSection
                {
                    Release = release,
                }
            }
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
            ReleaseContentBlockUnlockViewModel viewModel = null!;

            var match = new CaptureMatch<ReleaseContentBlockUnlockViewModel>(
                value => { viewModel = value; }
            );

            var client = new Mock<IReleaseContentHubClient>(Strict);

            // Clients in the release group are notified that the content block is unlocked
            client
                .Setup(s => s.ContentBlockUnlocked(Capture.With(match)))
                .Returns(Task.CompletedTask);

            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(release.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.UnlockContentBlock(contentBlock.Id);

            VerifyAllMocks(client, hubContext);

            result.AssertRight();

            Assert.Equal(viewModel.Id, contentBlock.Id);
            Assert.Equal(viewModel.SectionId, contentBlock.ContentSectionId);
            Assert.Equal(viewModel.ReleaseId, release.Id);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.Null(savedContentBlock!.Locked);
            Assert.Null(savedContentBlock.LockedBy);
            Assert.Null(savedContentBlock.LockedById);
        }
    }

    [Fact]
    public async Task UnlockContentBlock_PreviousLockNotExpired()
    {
        var release = new Release();

        var nextUser = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "test@test.com"
        };
        var previousUser = new User
        {
            FirstName = "Rob",
            LastName = "Rowe",
            Email = "rob@test.com"
        };

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
                Release = new ReleaseContentSection
                {
                    Release = release,
                }
            }
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
        var release = new Release();

        var nextUser = new User
        {
            Id = _defaultUserId,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "test@test.com"
        };
        var previousUser = new User
        {
            FirstName = "Rob",
            LastName = "Rowe",
            Email = "rob@test.com"
        };

        var locked = DateTime.UtcNow.AddMinutes(-5);

        var contentBlock = new HtmlBlock
        {
            // Locked by another user and has not expired yet,
            // so need to use force flag to unlock it.
            Locked = locked,
            LockedBy = previousUser,
            ContentSection = new ContentSection
            {
                Release = new ReleaseContentSection
                {
                    Release = release,
                }
            }
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
            ReleaseContentBlockUnlockViewModel viewModel = null!;

            var match = new CaptureMatch<ReleaseContentBlockUnlockViewModel>(
                value => { viewModel = value; }
            );

            var client = new Mock<IReleaseContentHubClient>(Strict);

            // Clients in the release group are notified that the content block is unlocked
            client
                .Setup(s => s.ContentBlockUnlocked(Capture.With(match)))
                .Returns(Task.CompletedTask);

            var hubContext = new Mock<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict);

            hubContext
                .Setup(s => s.Clients.Group(release.Id.ToString()))
                .Returns(client.Object);

            var service = BuildService(contentDbContext, hubContext: hubContext.Object);
            var result = await service.UnlockContentBlock(contentBlock.Id, force: true);

            VerifyAllMocks(client, hubContext);

            result.AssertRight();

            Assert.Equal(viewModel.Id, contentBlock.Id);
            Assert.Equal(viewModel.SectionId, contentBlock.ContentSectionId);
            Assert.Equal(viewModel.ReleaseId, release.Id);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contextId))
        {
            var savedContentBlock = await contentDbContext.ContentBlocks.FindAsync(contentBlock.Id);

            Assert.Null(savedContentBlock!.Locked);
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

    private ReleaseContentBlockService BuildService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext>? persistenceHelper = null,
        IUserService? userService = null,
        IHubContext<ReleaseContentHub, IReleaseContentHubClient>? hubContext = null)
    {
        return new ReleaseContentBlockService(
            contentDbContext: contentDbContext,
            persistenceHelper: persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            userService: userService ?? AlwaysTrueUserService(_defaultUserId).Object,
            hubContext: hubContext ?? Mock.Of<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(Strict)
        );
    }
}