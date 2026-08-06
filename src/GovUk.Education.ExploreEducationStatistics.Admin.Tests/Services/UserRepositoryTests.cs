#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public abstract class UserRepositoryTests
{
    private readonly DataFixture _dataFixture = new();

    public static readonly TheoryData<Func<DataFixture, User>> AllNonSoftDeletedTypesOfUser =
    [
        // Active User
        fixture => fixture.DefaultUser(),
        // User with Pending Invite
        fixture => fixture.DefaultUserWithPendingInvite(),
        // User with Expired Invite
        fixture => fixture.DefaultUserWithExpiredInvite(),
    ];

    public static readonly TheoryData<Func<DataFixture, User>> AllInactiveTypesOfUser =
    [
        // User with Pending Invite
        fixture => fixture.DefaultUserWithPendingInvite(),
        // User with Expired Invite
        fixture => fixture.DefaultUserWithExpiredInvite(),
        // Soft Deleted User
        fixture => fixture.DefaultSoftDeletedUser(),
    ];

    public static readonly TheoryData<ActionResult> GlobalRoleServiceFailureResults =
    [
        new ForbidResult(),
        new NotFoundResult(),
    ];

    public class FindPendingUserInviteByEmailTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsNull()
        {
            User user = _dataFixture.DefaultUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail(user.Email);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsUser()
        {
            User user = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsNull()
        {
            User user = _dataFixture.DefaultUserWithExpiredInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail(user.Email);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task DifferentCase_ReturnsUser()
        {
            User user = _dataFixture.DefaultUserWithPendingInvite().WithEmail("test@test.com");

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail("TEST@TEST.COM");

                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            User user = _dataFixture.DefaultSoftDeletedUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindPendingUserInviteByEmail(user.Email);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = BuildRepository(contentDbContext);
            var result = await repository.FindPendingUserInviteByEmail("test@test.com");
            Assert.Null(result);
        }
    }

    public class FindActiveUserByEmailTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsUser()
        {
            User user = _dataFixture.DefaultUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsNull()
        {
            User user = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail(user.Email);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsNull()
        {
            User user = _dataFixture.DefaultUserWithExpiredInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail(user.Email);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task DifferentCase_ReturnsUser()
        {
            User user = _dataFixture.DefaultUser().WithEmail("test@test.com");

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail("TEST@TEST.COM");

                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            User user = _dataFixture.DefaultSoftDeletedUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserByEmail(user.Email);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = BuildRepository(contentDbContext);
            var result = await repository.FindActiveUserByEmail("test@test.com");
            Assert.Null(result);
        }
    }

    public class FindActiveUserByIdTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsUser()
        {
            User user = _dataFixture.DefaultUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserById(user.Id);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsNull()
        {
            User user = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserById(user.Id);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsNull()
        {
            User user = _dataFixture.DefaultUserWithExpiredInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserById(user.Id);
                Assert.Null(result);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            User user = _dataFixture.DefaultSoftDeletedUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindActiveUserById(user.Id);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = BuildRepository(contentDbContext);
            var result = await repository.FindActiveUserById(Guid.NewGuid());
            Assert.Null(result);
        }
    }

    public class FindUserByEmailTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsUser()
        {
            User user = _dataFixture.DefaultUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsUser()
        {
            User user = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsUser()
        {
            User user = _dataFixture.DefaultUserWithExpiredInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserByEmail(user.Email);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task DifferentCase_ReturnsUser()
        {
            User user = _dataFixture.DefaultUser().WithEmail("test@test.com");

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserByEmail("TEST@TEST.COM");

                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            User user = _dataFixture.DefaultSoftDeletedUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserByEmail(user.Email);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = BuildRepository(contentDbContext);
            var result = await repository.FindUserByEmail("test@test.com");
            Assert.Null(result);
        }
    }

    public class FindUserByIdTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ReturnsUser()
        {
            User user = _dataFixture.DefaultUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserById(user.Id);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_ReturnsUser()
        {
            User user = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserById(user.Id);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_ReturnsUser()
        {
            User user = _dataFixture.DefaultUserWithExpiredInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserById(user.Id);
                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_ReturnsNull()
        {
            User user = _dataFixture.DefaultSoftDeletedUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                var result = await repository.FindUserById(user.Id);

                Assert.Null(result);
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ReturnsNull()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = BuildRepository(contentDbContext);
            var result = await repository.FindUserById(Guid.NewGuid());
            Assert.Null(result);
        }
    }

    public class FindDeletedUserPlaceholderTests : UserRepositoryTests
    {
        [Fact]
        public async Task Success()
        {
            User user = _dataFixture.DefaultDeletedUserPlaceholder();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);

                var result = await repository.FindDeletedUserPlaceholder();

                Assert.NotNull(result);
                Assert.Equal(user.Id, result.Id);
                Assert.Equal(User.DeletedUserPlaceholderEmail, result.Email);
            }
        }

        [Fact]
        public async Task DeletedUserDoesNotExist_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = BuildRepository(contentDbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repository.FindDeletedUserPlaceholder()
            );
        }
    }

    public class SoftDeleteUserTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_SoftDeletesUser()
        {
            User user = _dataFixture.DefaultUser();
            var deletedById = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            // Ensure initial state is correct
            Assert.True(user.Active);
            Assert.Null(user.SoftDeleted);
            Assert.Null(user.DeletedById);

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                await repository.SoftDeleteUser(user.Id, deletedById);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                Assert.False(updatedUser.Active);
                updatedUser.SoftDeleted.AssertUtcNow();
                Assert.Equal(deletedById, updatedUser.DeletedById);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_SoftDeletesUser()
        {
            User user = _dataFixture.DefaultUserWithPendingInvite();
            var deletedById = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            // Ensure initial state is correct
            Assert.True(user.IsInvitePending());

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                await repository.SoftDeleteUser(user.Id, deletedById);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                Assert.False(updatedUser.Active);
                updatedUser.SoftDeleted.AssertUtcNow();
                Assert.Equal(deletedById, updatedUser.DeletedById);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_SoftDeletesUser()
        {
            User user = _dataFixture.DefaultUserWithExpiredInvite();
            var deletedById = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            // Ensure initial state is correct
            Assert.True(user.IsInviteExpired());

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                await repository.SoftDeleteUser(user.Id, deletedById);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                Assert.False(updatedUser.Active);
                updatedUser.SoftDeleted.AssertUtcNow();
                Assert.Equal(deletedById, updatedUser.DeletedById);
            }
        }

        [Fact]
        public async Task UserAlreadySoftDeleted_ThrowsException()
        {
            User user = _dataFixture.DefaultSoftDeletedUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            // Ensure initial state is correct
            Assert.False(user.Active);
            Assert.NotNull(user.SoftDeleted);
            Assert.NotNull(user.DeletedById);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);

                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await repository.SoftDeleteUser(user.Id, Guid.NewGuid())
                );
            }
        }

        [Fact]
        public async Task UserDoesNotExist_ThrowsException()
        {
            await using var contentDbContext = InMemoryApplicationDbContext();

            var repository = BuildRepository(contentDbContext);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repository.SoftDeleteUser(Guid.NewGuid(), Guid.NewGuid())
            );
        }
    }

    public class CreateOrUpdateTests : UserRepositoryTests
    {
        [Fact]
        public async Task ActiveUser_ThrowsException()
        {
            User user = _dataFixture.DefaultUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);

                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await repository.CreateOrUpdate(
                        email: user.Email,
                        createdById: Guid.NewGuid(),
                        createdDate: DateTimeOffset.UtcNow
                    )
                );
            }
        }

        [Theory]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        // Check the role is defaulted to StandardUser if no role is supplied
        [InlineData(GlobalRoles.Role.BauUser, null, GlobalRoles.Role.StandardUser)]
        public async Task UserWithPendingInvite_UpdatesUser(
            GlobalRoles.Role oldRole,
            GlobalRoles.Role? newRole,
            GlobalRoles.Role expectedUpdatedRole
        )
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            User user = _dataFixture
                .DefaultUserWithPendingInvite()
                .WithRoleId(oldRole.GetEnumValue())
                .WithCreatedById(oldCreatedById)
                .WithCreated(oldCreatedDate);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = newRole is null
                    ? await repository.CreateOrUpdate(
                        email: user.Email,
                        createdById: newCreatedById,
                        createdDate: newCreatedDate
                    )
                    : await repository.CreateOrUpdate(
                        email: user.Email,
                        role: newRole.Value,
                        createdById: newCreatedById,
                        createdDate: newCreatedDate
                    );

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Equal(oldCreatedById, result.CreatedById);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);

                Assert.Equal(expectedUpdatedRole.GetEnumValue(), result.RoleId);
                Assert.Equal(newCreatedDate, result.Created);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
                Assert.False(updatedUser.Active);
                Assert.Equal(oldCreatedById, updatedUser.CreatedById);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);

                Assert.Equal(expectedUpdatedRole.GetEnumValue(), updatedUser.RoleId);
                Assert.Equal(newCreatedDate, updatedUser.Created);
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_SuppliedDateNotUtc_StoresCreatedDateInUtc()
        {
            var newCreatedDate = new DateTimeOffset(2025, 10, 28, 12, 0, 0, TimeSpan.FromHours(1));

            User user = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    createdById: Guid.NewGuid(),
                    createdDate: newCreatedDate
                );

                Assert.Equal(newCreatedDate, result.Created);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                Assert.Equal(newCreatedDate, updatedUser.Created); // Still should represent the same instance in time
                Assert.Equal(TimeSpan.Zero, updatedUser.Created.Offset); // But the offset should be zero as it is UTC
            }
        }

        [Fact]
        public async Task UserWithPendingInvite_CreatedDateNotSupplied_UpdatesUserCreatedDateToNow()
        {
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);

            User user = _dataFixture.DefaultUserWithPendingInvite().WithCreated(oldCreatedDate);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.CreateOrUpdate(email: user.Email, createdById: Guid.NewGuid());

                result.Created.AssertUtcNow();
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                updatedUser.Created.AssertUtcNow();
            }
        }

        [Theory]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        // Check the role is defaulted to StandardUser if no role is supplied
        [InlineData(GlobalRoles.Role.BauUser, null, GlobalRoles.Role.StandardUser)]
        public async Task UserWithExpiredInvite_UpdatesUser(
            GlobalRoles.Role oldRole,
            GlobalRoles.Role? newRole,
            GlobalRoles.Role expectedUpdatedRole
        )
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            User user = _dataFixture
                .DefaultUserWithExpiredInvite()
                .WithRoleId(oldRole.GetEnumValue())
                .WithCreatedById(oldCreatedById);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = newRole is null
                    ? await repository.CreateOrUpdate(
                        email: user.Email,
                        createdById: newCreatedById,
                        createdDate: newCreatedDate
                    )
                    : await repository.CreateOrUpdate(
                        email: user.Email,
                        role: newRole.Value,
                        createdById: newCreatedById,
                        createdDate: newCreatedDate
                    );

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);

                // These fields should always be updated for expired invites being re-invited
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), result.RoleId);
                Assert.Equal(newCreatedById, result.CreatedById);
                Assert.Equal(newCreatedDate, result.Created);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
                Assert.False(updatedUser.Active);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);

                // These fields should always be updated for expired invites being re-invited
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), updatedUser.RoleId);
                Assert.Equal(newCreatedById, updatedUser.CreatedById);
                Assert.Equal(newCreatedDate, updatedUser.Created);
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_SuppliedDateNotUtc_StoresCreatedDateInUtc()
        {
            var newCreatedDate = new DateTimeOffset(2025, 10, 28, 12, 0, 0, TimeSpan.FromHours(1));

            User user = _dataFixture.DefaultUserWithExpiredInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    createdById: Guid.NewGuid(),
                    createdDate: newCreatedDate
                );

                Assert.Equal(newCreatedDate, result.Created);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                Assert.Equal(newCreatedDate, updatedUser.Created); // Still should represent the same instance in time
                Assert.Equal(TimeSpan.Zero, updatedUser.Created.Offset); // But the offset should be zero as it is UTC
            }
        }

        [Fact]
        public async Task UserWithExpiredInvite_CreatedDateNotSupplied_UpdatesUserCreatedDateToNow()
        {
            User user = _dataFixture.DefaultUserWithExpiredInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.CreateOrUpdate(email: user.Email, createdById: Guid.NewGuid());

                result.Created.AssertUtcNow();
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                updatedUser.Created.AssertUtcNow();
            }
        }

        [Theory]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        // Check the role is defaulted to StandardUser if no role is supplied
        [InlineData(GlobalRoles.Role.BauUser, null, GlobalRoles.Role.StandardUser)]
        public async Task SoftDeletedUser_UpdatesUser(
            GlobalRoles.Role oldRole,
            GlobalRoles.Role? newRole,
            GlobalRoles.Role expectedUpdatedRole
        )
        {
            var oldCreatedById = Guid.NewGuid();
            var newCreatedById = Guid.NewGuid();
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(-1);

            User user = _dataFixture
                .DefaultSoftDeletedUser()
                .WithRoleId(oldRole.GetEnumValue())
                .WithCreatedById(oldCreatedById)
                .WithCreated(oldCreatedDate);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = newRole is null
                    ? await repository.CreateOrUpdate(
                        email: user.Email,
                        createdById: newCreatedById,
                        createdDate: newCreatedDate
                    )
                    : await repository.CreateOrUpdate(
                        email: user.Email,
                        role: newRole.Value,
                        createdById: newCreatedById,
                        createdDate: newCreatedDate
                    );

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, result.Id);
                Assert.Equal(user.Email, result.Email);
                Assert.False(result.Active);

                // These fields should always be updated for soft-deleted users being re-invited
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), result.RoleId);
                Assert.Equal(newCreatedById, result.CreatedById);
                Assert.Equal(newCreatedDate, result.Created);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                // All of these fields should be untouched by the update
                Assert.Equal(user.Id, updatedUser.Id);
                Assert.Equal(user.Email, updatedUser.Email);
                Assert.False(updatedUser.Active);

                // These fields should always be updated for soft-deleted users being re-invited
                Assert.Equal(expectedUpdatedRole.GetEnumValue(), updatedUser.RoleId);
                Assert.Equal(newCreatedById, updatedUser.CreatedById);
                Assert.Equal(newCreatedDate, updatedUser.Created);
                Assert.Null(updatedUser.SoftDeleted);
                Assert.Null(updatedUser.DeletedById);
                Assert.Null(updatedUser.FirstName);
                Assert.Null(updatedUser.LastName);
            }
        }

        [Fact]
        public async Task SoftDeletedUser_SuppliedDateNotUtc_StoresCreatedDateInUtc()
        {
            var newCreatedDate = new DateTimeOffset(2025, 10, 28, 12, 0, 0, TimeSpan.FromHours(1));

            User user = _dataFixture.DefaultSoftDeletedUser();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.CreateOrUpdate(
                    email: user.Email,
                    createdById: Guid.NewGuid(),
                    createdDate: newCreatedDate
                );

                Assert.Equal(newCreatedDate, result.Created);
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                Assert.Equal(newCreatedDate, updatedUser.Created); // Still should represent the same instance in time
                Assert.Equal(TimeSpan.Zero, updatedUser.Created.Offset); // But the offset should be zero as it is UTC
            }
        }

        [Fact]
        public async Task SoftDeletedUser_CreatedDateNotSupplied_UpdatesUserCreatedDateToNow()
        {
            var oldCreatedDate = DateTimeOffset.UtcNow.AddDays(-2);

            User user = _dataFixture.DefaultSoftDeletedUser().WithCreated(oldCreatedDate);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var userPreReleaseRoleRepository = new Mock<IUserPreReleaseRoleRepository>(MockBehavior.Strict);
            userPreReleaseRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var userPublicationRoleRepository = new Mock<IUserPublicationRoleRepository>(MockBehavior.Strict);
            userPublicationRoleRepository
                .Setup(mock => mock.RemoveForUser(user.Id, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    userPreReleaseRoleRepository: userPreReleaseRoleRepository.Object,
                    userPublicationRoleRepository: userPublicationRoleRepository.Object
                );

                var result = await repository.CreateOrUpdate(email: user.Email, createdById: Guid.NewGuid());

                result.Created.AssertUtcNow();
            }

            MockUtils.VerifyAllMocks(userPreReleaseRoleRepository, userPublicationRoleRepository);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                updatedUser.Created.AssertUtcNow();
            }
        }

        [Theory]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        // Check the role is defaulted to StandardUser if no role is supplied
        [InlineData(null, GlobalRoles.Role.StandardUser)]
        public async Task UserDoesNotExist_CreatesNewUser(GlobalRoles.Role? suppliedRole, GlobalRoles.Role expectedRole)
        {
            const string email = "TEST@test.com";
            var createdById = Guid.NewGuid();
            var createdDate = DateTimeOffset.UtcNow.AddDays(-1);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);

                var result = suppliedRole is null
                    ? await repository.CreateOrUpdate(email: email, createdById: createdById, createdDate: createdDate)
                    : await repository.CreateOrUpdate(
                        email: email,
                        role: suppliedRole.Value,
                        createdById: createdById,
                        createdDate: createdDate
                    );

                Assert.Equal(email.ToLower(), result.Email);
                Assert.Null(result.FirstName);
                Assert.Null(result.LastName);
                Assert.False(result.Active);
                Assert.Equal(expectedRole.GetEnumValue(), result.RoleId);
                Assert.Equal(createdById, result.CreatedById);
                Assert.Equal(createdDate, result.Created);
                Assert.Null(result.SoftDeleted);
                Assert.Null(result.DeletedById);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var newUser = await contentDbContext.Users.SingleAsync();

                Assert.Equal(email.ToLower(), newUser.Email);
                Assert.Null(newUser.FirstName);
                Assert.Null(newUser.LastName);
                Assert.False(newUser.Active);
                Assert.Equal(expectedRole.GetEnumValue(), newUser.RoleId);
                Assert.Equal(createdById, newUser.CreatedById);
                Assert.Equal(createdDate, newUser.Created);
                Assert.Null(newUser.SoftDeleted);
                Assert.Null(newUser.DeletedById);
            }
        }

        [Fact]
        public async Task SuppliedCreatedDateInFuture_ThrowsException()
        {
            var newCreatedDate = DateTimeOffset.UtcNow.AddDays(1);

            User user = _dataFixture.DefaultUserWithPendingInvite();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);

                await Assert.ThrowsAsync<ArgumentException>(async () =>
                    await repository.CreateOrUpdate(
                        email: user.Email,
                        createdById: Guid.NewGuid(),
                        createdDate: newCreatedDate
                    )
                );
            }
        }

        [Fact]
        public async Task UserDoesNotExist_SuppliedDateNotUtc_StoresCreatedDateInUtc()
        {
            var createdDate = new DateTimeOffset(2025, 10, 28, 12, 0, 0, TimeSpan.FromHours(1));

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(
                    email: "test@test.com",
                    createdById: Guid.NewGuid(),
                    createdDate: createdDate
                );

                Assert.Equal(createdDate, result.Created); // Still should represent the same instance in time
                Assert.Equal(TimeSpan.Zero, result.Created.Offset); // But the offset should be zero as it is UTC
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var newUser = await contentDbContext.Users.SingleAsync();

                Assert.Equal(createdDate, newUser.Created); // Still should represent the same instance in time
                Assert.Equal(TimeSpan.Zero, newUser.Created.Offset); // But the offset should be zero as it is UTC
            }
        }

        [Fact]
        public async Task UserDoesNotExist_CreatedDateNotSupplied_CreatesNewUserWithCreatedDateNow()
        {
            const string email = "test@test.com";
            var createdById = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);

                var result = await repository.CreateOrUpdate(email: email, createdById: createdById);

                result.Created.AssertUtcNow();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var newUser = await contentDbContext.Users.SingleAsync();

                newUser.Created.AssertUtcNow();
            }
        }
    }

    public class UpdateGlobalRoleTests : UserRepositoryTests
    {
        [Theory]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.StandardUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.StandardUser, GlobalRoles.Role.StandardUser)]
        [InlineData(GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser, GlobalRoles.Role.BauUser)]
        public async Task Success(
            GlobalRoles.Role oldRole,
            GlobalRoles.Role newRole,
            GlobalRoles.Role expectedUpdatedRole
        )
        {
            // Active User
            User user = _dataFixture.DefaultUser().WithRoleId(oldRole.GetEnumValue());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var globalRoleService = new Mock<IGlobalRoleService>(MockBehavior.Strict);
            globalRoleService.Setup(mock => mock.UpdateGlobalRoleForUser(user.Id, newRole)).ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    globalRoleService: globalRoleService.Object
                );

                var result = await repository.UpdateGlobalRole(userId: user.Id, newRole: newRole);

                Assert.Equal(expectedUpdatedRole.GetEnumValue(), result.RoleId);
            }

            MockUtils.VerifyAllMocks(globalRoleService);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var updatedUser = await contentDbContext.Users.SingleAsync(u => u.Id == user.Id);

                Assert.Equal(expectedUpdatedRole.GetEnumValue(), updatedUser.RoleId);
            }
        }

        [Theory]
        [MemberData(nameof(GlobalRoleServiceFailureResults))]
        public async Task UpdatingGlobalRoleFails_Throws(ActionResult actionResult)
        {
            var role = GlobalRoles.Role.StandardUser;

            // Active User
            User user = _dataFixture.DefaultUser().WithRoleId(role.GetEnumValue());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            var globalRoleService = new Mock<IGlobalRoleService>(MockBehavior.Strict);
            globalRoleService.Setup(mock => mock.UpdateGlobalRoleForUser(user.Id, role)).ReturnsAsync(actionResult);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(
                    contentDbContext: contentDbContext,
                    globalRoleService: globalRoleService.Object
                );

                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await repository.UpdateGlobalRole(userId: user.Id, newRole: role)
                );
            }
        }

        [Theory]
        [MemberData(nameof(AllInactiveTypesOfUser))]
        public async Task UserIsNotActive_Throws(Func<DataFixture, User> userFactory)
        {
            var user = userFactory(new DataFixture());

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                contentDbContext.Users.Add(user);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = BuildRepository(contentDbContext);
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                    await repository.UpdateGlobalRole(userId: user.Id, newRole: GlobalRoles.Role.StandardUser)
                );
            }
        }

        [Fact]
        public async Task NoUser_Throws()
        {
            var repository = BuildRepository();
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await repository.UpdateGlobalRole(userId: Guid.NewGuid(), newRole: GlobalRoles.Role.StandardUser)
            );
        }
    }

    private static UserRepository BuildRepository(
        ContentDbContext? contentDbContext = null,
        IUserPreReleaseRoleRepository? userPreReleaseRoleRepository = null,
        IUserPublicationRoleRepository? userPublicationRoleRepository = null,
        IGlobalRoleService? globalRoleService = null
    )
    {
        return new(
            contentDbContext: contentDbContext ?? InMemoryApplicationDbContext(),
            userPreReleaseRoleRepository: userPreReleaseRoleRepository
                ?? Mock.Of<IUserPreReleaseRoleRepository>(MockBehavior.Strict),
            userPublicationRoleRepository: userPublicationRoleRepository
                ?? Mock.Of<IUserPublicationRoleRepository>(MockBehavior.Strict),
            globalRoleService: globalRoleService ?? Mock.Of<IGlobalRoleService>(MockBehavior.Strict)
        );
    }
}
