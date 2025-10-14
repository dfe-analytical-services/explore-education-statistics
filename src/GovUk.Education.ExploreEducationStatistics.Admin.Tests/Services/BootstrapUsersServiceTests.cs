#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.Extensions.Configuration;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class BootstrapUsersServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(",")]
    [InlineData(" , ")]
    [InlineData(",,")]
    [InlineData(", ,")]
    public void AddBootstrapUsers_EmptyEmailList_DoesNothing(string? bootstrappedEmailsString)
    {
        var bootstrappedUserEmailsConfiguration = new Mock<IConfiguration>(MockBehavior.Strict);
        var bootstrapUsersSection = new Mock<IConfigurationSection>();

        var bauSection = new Mock<IConfigurationSection>();
        bauSection.Setup(s => s.Value)
            .Returns(bootstrappedEmailsString);

        bootstrapUsersSection
            .Setup(s => s.GetSection("BAU"))
            .Returns(bauSection.Object);

        bootstrappedUserEmailsConfiguration
            .Setup(c => c.GetSection("BootstrapUsers"))
            .Returns(bootstrapUsersSection.Object);

        var contentDbContextId = Guid.NewGuid().ToString();

        using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext, bootstrappedUserEmailsConfiguration.Object);

            service.AddBootstrapUsers();
        }

        using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var users = contentDbContext.Users.ToList();

            Assert.Empty(users);
        }
    }

    [Fact]
    public void AddBootstrapUsers_UsersAlreadyExist_DoesNothing()
    {
        var placeholderDeletedUser = _dataFixture.DefaultUser()
            .WithEmail(User.DeletedUserPlaceholderEmail)
            .Generate();
        var existingUsers = _dataFixture.DefaultUser()
            .ForIndex(0, s => s.SetEmail("test1@test.com"))
            .ForIndex(1, s => s.SetEmail("test2@test.com"))
            .GenerateList(2);

        var bootstrappedUserEmailsConfiguration = new Mock<IConfiguration>(MockBehavior.Strict);
        var bootstrapUsersSection = new Mock<IConfigurationSection>();

        var bauSection = new Mock<IConfigurationSection>();
        bauSection.Setup(s => s.Value)
            .Returns($"{existingUsers[0].Email}, {existingUsers[1].Email}");

        bootstrapUsersSection
            .Setup(s => s.GetSection("BAU"))
            .Returns(bauSection.Object);

        bootstrappedUserEmailsConfiguration
            .Setup(c => c.GetSection("BootstrapUsers"))
            .Returns(bootstrapUsersSection.Object);

        var contentDbContextId = Guid.NewGuid().ToString();

        using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.AddRange([placeholderDeletedUser, ..existingUsers]);
            contentDbContext.SaveChanges();
        }

        using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext, bootstrappedUserEmailsConfiguration.Object);

            service.AddBootstrapUsers();
        }

        using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var usersInDatabase = contentDbContext.Users.ToList();

            Assert.Equal(3, usersInDatabase.Count);
            Assert.Equal(placeholderDeletedUser.Email, usersInDatabase[0].Email);
            Assert.Equal(existingUsers[0].Email, usersInDatabase[1].Email);
            Assert.Equal(existingUsers[1].Email, usersInDatabase[2].Email);
        }
    }

    [Fact]
    public void AddBootstrapUsers_SomeUsersAreNew_CreatesNewUsers()
    {
        var placeholderDeletedUser = _dataFixture.DefaultUser()
            .WithEmail(User.DeletedUserPlaceholderEmail)
            .Generate();
        var existingUser = _dataFixture.DefaultUser()
            .WithEmail("test1@test.com")
            .Generate();

        var newUserEmail1 = "test2@test.com";
        var newUserEmail2 = "test3@test.com";

        var bootstrappedUserEmailsConfiguration = new Mock<IConfiguration>(MockBehavior.Strict);
        var bootstrapUsersSection = new Mock<IConfigurationSection>();

        var bauSection = new Mock<IConfigurationSection>();
        bauSection.Setup(s => s.Value)
            .Returns($"{existingUser.Email}, {newUserEmail1}, {newUserEmail2}");

        bootstrapUsersSection
            .Setup(s => s.GetSection("BAU"))
            .Returns(bauSection.Object);

        bootstrappedUserEmailsConfiguration
            .Setup(c => c.GetSection("BootstrapUsers"))
            .Returns(bootstrapUsersSection.Object);

        var contentDbContextId = Guid.NewGuid().ToString();

        using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.Users.AddRange([placeholderDeletedUser, existingUser]);
            contentDbContext.SaveChanges();
        }

        using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext, bootstrappedUserEmailsConfiguration.Object);

            service.AddBootstrapUsers();
        }

        using (var contentDbContext = DbUtils.InMemoryApplicationDbContext(contentDbContextId))
        {
            var usersInDatabase = contentDbContext.Users.ToList();

            Assert.Equal(4, usersInDatabase.Count);
            Assert.Equal(placeholderDeletedUser.Email, usersInDatabase[0].Email);
            Assert.Equal(existingUser.Email, usersInDatabase[1].Email);
            Assert.Equal(newUserEmail1, usersInDatabase[2].Email);
            Assert.Equal(newUserEmail2, usersInDatabase[3].Email);

            Assert.Equal(GlobalRoles.Role.BauUser.GetEnumValue(), usersInDatabase[2].RoleId);
            Assert.False(usersInDatabase[2].Active);
            usersInDatabase[2].Created.AssertUtcNow();
            Assert.Equal(placeholderDeletedUser.Id, usersInDatabase[2].CreatedById);

            Assert.Equal(GlobalRoles.Role.BauUser.GetEnumValue(), usersInDatabase[3].RoleId);
            Assert.False(usersInDatabase[3].Active);
            usersInDatabase[3].Created.AssertUtcNow();
            Assert.Equal(placeholderDeletedUser.Id, usersInDatabase[3].CreatedById);
        }
    }

    private static BootstrapUsersService SetupService(
        ContentDbContext contentDbContext,
        IConfiguration? configuration = null)
    {
        return new(
            configuration ?? Mock.Of<IConfiguration>(MockBehavior.Strict),
            contentDbContext 
        );
    }
}
