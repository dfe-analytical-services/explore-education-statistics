#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Pages.Account;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Areas.Identity.Pages.Account
{
    public class ExternalLoginTests
    {
        private const string ExpectedLoginErrorMessage = "Sorry, there was a problem logging you in.";
        private const string ReturnUrl = "http://return.url";
        private const string LoginProvider = "LoginProvider1";
        private const string ProviderKey = "LoginProvider1Key";
        private const string LoginProviderDisplayName = "LoginProvider1DisplayName";

        private readonly IdentityUserLogin<string> _providerDetails = new()
        {
            LoginProvider = LoginProvider,
            ProviderKey = ProviderKey,
            ProviderDisplayName = LoginProviderDisplayName
        };

        [Fact]
        public async Task Login_ExistingUser_Success()
        {
            var (result, loginService) = await DoExistingUserLogin(SignInResult.Success);
            var redirectPage = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(ReturnUrl, redirectPage.Url);
            Assert.False(redirectPage.Permanent);
            Assert.Null(loginService.ErrorMessage);
            Assert.Empty(loginService.ModelState);
        }

        [Fact]
        public async Task Login_ExistingUser_Failed()
        {
            var (result, loginService) = await DoExistingUserLogin(SignInResult.Failed);
            AssertRedirectedToLoginPageUponFailure(result, loginService);
        }

        [Fact]
        public async Task Login_ExistingUser_NotAllowed()
        {
            var (result, loginService) = await DoExistingUserLogin(SignInResult.NotAllowed);
            AssertRedirectedToLoginPageUponFailure(result, loginService);
        }

        [Fact]
        public async Task Login_ExistingUser_LockedOut()
        {
            var (result, loginService) = await DoExistingUserLogin(SignInResult.LockedOut);
            var redirectPage = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Lockout", redirectPage.PageName);
            Assert.False(redirectPage.Permanent);
            Assert.Equal(ExpectedLoginErrorMessage, loginService.ErrorMessage);
            Assert.Empty(loginService.ModelState);
        }

        [Fact]
        public async Task Login_ExistingUser_NewProviderDetails_Success()
        {
            var (result, loginService) = await DoLoginExistingUserWithNewProviderKey(
                ListOf(_providerDetails),
                IdentityResult.Success,
                SignInResult.Success);

            AssertSuccessfulLogin(result, loginService);
        }

        [Fact]
        public async Task Login_ExistingUser_NewProviderDetails_FailureToAddNewProviderDetails()
        {
            var (result, loginService) = await DoLoginExistingUserWithNewProviderKey(
                ListOf(_providerDetails),
                IdentityResult.Failed(
                    new IdentityError
                    {
                        Code = "500",
                        Description = "Error 1"
                    },
                    new IdentityError
                    {
                        Code = "502",
                        Description = "Error 2"
                    }));

            AssertRedirectedToLoginPageUponFailure(
                result,
                loginService,
                ListOf("Error 1", "Error 2"));
        }

        [Fact]
        public async Task Login_ExistingUser_NoExistingProviderDetails_InvalidState()
        {
            var (result, loginService) = await DoLoginExistingUserWithNewProviderKey(
                new List<IdentityUserLogin<string>>());

            AssertRedirectedToLoginPageUponFailure(result, loginService);
        }

        [Fact]
        public async Task Login_RemoteErrorReceived()
        {
            var loginService = BuildService();
            var result = await loginService.OnGetCallbackAsync(ReturnUrl, "Remote Error");
            AssertRedirectedToLoginPageUponFailure(result, loginService);
        }

        [Fact]
        public async Task Login_UnableToGetExternalLoginInfo()
        {
            var signInManager = new Mock<ISignInManagerDelegate>(Strict);

            var loginService = BuildService(signInManager.Object);

            signInManager
                .Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync((ExternalLoginInfo) null!);

            var result = await loginService.OnGetCallbackAsync(ReturnUrl);
            VerifyAllMocks(signInManager);

            AssertRedirectedToLoginPageUponFailure(result, loginService);
        }

        [Fact]
        public async Task Login_EmailClaimNotAvailable()
        {
            var claimsPrincipal = ClaimsPrincipalUtils.CreateClaimsPrincipal(
                Guid.NewGuid(),
                new Claim(ClaimTypes.GivenName, "FirstName"),
                new Claim(ClaimTypes.Surname, "LastName"));

            var signInManager = new Mock<ISignInManagerDelegate>(Strict);

            signInManager
                .Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(new ExternalLoginInfo(
                    claimsPrincipal,
                    LoginProvider,
                    ProviderKey,
                    LoginProviderDisplayName));

            await using var usersDbContext = InMemoryUserAndRolesDbContext();
            await using var contentDbContext = InMemoryApplicationDbContext();

            var loginService = BuildService(
                signInManager.Object,
                usersDbContext: usersDbContext,
                contentDbContext: contentDbContext);

            var result = await loginService.OnGetCallbackAsync(ReturnUrl);
            VerifyAllMocks(signInManager);

            AssertRedirectedToLoginPageUponFailure(result, loginService);
        }

        [Fact]
        public async Task Login_UninvitedNewUser_Failure()
        {
            var claimsPrincipal = CreateClaimsPrincipal(
                "uninviteduser@example.com",
                "FirstName",
                "LastName");

            var userManager = new Mock<IUserManagerDelegate>(Strict);
            var signInManager = new Mock<ISignInManagerDelegate>(Strict);

            await using var usersDbContext = InMemoryUserAndRolesDbContext();
            await using var contentDbContext = InMemoryApplicationDbContext();

            var loginService = BuildService(
                signInManager.Object,
                userManager.Object,
                contentDbContext,
                usersDbContext);

            signInManager
                .Setup(s => s.GetExternalLoginInfoAsync(null))
                .ReturnsAsync(new ExternalLoginInfo(
                    claimsPrincipal,
                    LoginProvider,
                    ProviderKey,
                    LoginProviderDisplayName));

            var result = await loginService.OnGetCallbackAsync(ReturnUrl);
            VerifyAllMocks(userManager, signInManager);

            AssertRedirectedToLoginPageUponFailure(result, loginService);
        }

        [Fact]
        public async Task Login_InvitedUser_Success()
        {
            var contextId = Guid.NewGuid().ToString();

            const string email = "inviteduser@example.com";
            const string firstName = "FirstName";
            const string lastName = "LastName";

            var claimsPrincipal = CreateClaimsPrincipal(email, firstName, lastName);

            var userManager = new Mock<IUserManagerDelegate>(Strict);
            var signInManager = new Mock<ISignInManagerDelegate>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.UserReleaseInvites.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        Email = email,
                        Role = ReleaseRole.Approver,
                    },
                    new UserReleaseInvite
                    {
                        Email = email,
                        Role = ReleaseRole.Lead,
                    },
                    new UserReleaseInvite
                    {
                        Email = "anotheruser@example.com",
                    });

                await contentDbContext.UserPublicationInvites.AddRangeAsync(
                    new UserPublicationInvite
                    {
                        Email = email,
                        Role = PublicationRole.Owner
                    },
                    new UserPublicationInvite
                    {
                        Email = email,
                        Role = PublicationRole.Approver
                    },
                    new UserPublicationInvite
                    {
                        Email = "anotheruser@example.com"
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await usersDbContext.UserInvites.AddRangeAsync(
                    new UserInvite
                    {
                        Email = email,
                        Role = new IdentityRole("Role A"),
                        Accepted = false,
                        Created = DateTime.UtcNow.AddDays(-14).AddSeconds(10) // not yet expired
                    },
                    new UserInvite
                    {
                        Email = "anotheruser@example.com",
                        Role = new IdentityRole("Role B"),
                        Accepted = false,
                        Created = DateTime.UtcNow.AddDays(-14).AddSeconds(10) // not yet expired
                    });

                await usersDbContext.SaveChangesAsync();
            }

            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var loginService = BuildService(
                    signInManager.Object,
                    userManager.Object,
                    contentDbContext,
                    usersDbContext);

                signInManager
                    .Setup(s => s.GetExternalLoginInfoAsync(null))
                    .ReturnsAsync(new ExternalLoginInfo(
                        claimsPrincipal,
                        LoginProvider,
                        ProviderKey,
                        LoginProviderDisplayName));

                userManager
                    .Setup(s => s.CreateAsync(
                        It.Is<ApplicationUser>(u =>
                            u.Email == email &&
                            u.FirstName == firstName &&
                            u.LastName == lastName)))
                    .ReturnsAsync(IdentityResult.Success);

                userManager
                    .Setup(s => s.AddToRoleAsync(
                        It.Is<ApplicationUser>(u =>
                            u.Email == email &&
                            u.FirstName == firstName &&
                            u.LastName == lastName),
                        "Role A"))
                    .ReturnsAsync(IdentityResult.Success);

                userManager
                    .Setup(s => s.AddLoginAsync(
                        It.Is<ApplicationUser>(u =>
                            u.Email == email &&
                            u.FirstName == firstName &&
                            u.LastName == lastName),
                        It.Is<UserLoginInfo>(l =>
                            l.ProviderKey == ProviderKey
                            && l.LoginProvider == LoginProvider
                            && l.ProviderDisplayName == LoginProviderDisplayName)))
                    .ReturnsAsync(IdentityResult.Success);

                signInManager
                    .Setup(s => s.ExternalLoginSignInAsync(LoginProvider, ProviderKey, false, true))
                    .ReturnsAsync(SignInResult.Success);

                var result = await loginService.OnGetCallbackAsync(ReturnUrl);
                VerifyAllMocks(userManager, signInManager);

                AssertSuccessfulLogin(result, loginService);
            }

            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                // Assert that the new user's invites have all been accepted.
                var newUsersInvite = await usersDbContext
                    .UserInvites
                    .AsQueryable()
                    .SingleAsync(invite => invite.Email == email);
                Assert.True(newUsersInvite.Accepted);

                // Assert that the other user's invites have all been left alone.
                var otherUsersInvite = await usersDbContext
                    .UserInvites
                    .AsQueryable()
                    .SingleAsync(invite => invite.Email != email);
                Assert.False(otherUsersInvite.Accepted);

                // Assert that the new user's release role invites have all been left alone.
                var newUsersReleaseRoleInvites = await contentDbContext
                    .UserReleaseInvites
                    .AsQueryable()
                    .Where(invite => invite.Email == email)
                    .ToListAsync();
                Assert.Equal(2, newUsersReleaseRoleInvites.Count);

                // Assert that the other user's release role invites have all been left alone.
                var otherUsersReleaseRoleInvites = await contentDbContext
                    .UserReleaseInvites
                    .AsQueryable()
                    .Where(invite => invite.Email != email)
                    .ToListAsync();
                Assert.Single(otherUsersReleaseRoleInvites);

                // Assert that the new user's publication role invites have all been left alone.
                var newUsersPublicationRoleInvites = await contentDbContext
                    .UserPublicationInvites
                    .AsQueryable()
                    .Where(invite => invite.Email == email)
                    .ToListAsync();
                Assert.Equal(2, newUsersPublicationRoleInvites.Count);

                // Assert that the other user's publication role invites have all been left alone.
                var otherUsersPublicationRoleInvites = await contentDbContext
                    .UserPublicationInvites
                    .AsQueryable()
                    .Where(invite => invite.Email != email)
                    .ToListAsync();
                Assert.Single(otherUsersPublicationRoleInvites);

                // Assert that the user has been assigned the new release roles.
                var newUsersReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(2, newUsersReleaseRoles.Count);
                Assert.Equal(
                    ListOf(ReleaseRole.Approver, ReleaseRole.Lead),
                    newUsersReleaseRoles.Select(r => r.Role));

                // Assert that the user has been assigned the new publication roles.
                var newUserPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Equal(2, newUserPublicationRoles.Count);
                Assert.Equal(
                    ListOf(PublicationRole.Owner, PublicationRole.Approver),
                    newUserPublicationRoles.Select(r => r.Role));
            }
        }
        
        [Fact]
        public async Task Login_InvitedUser_Expired()
        {
            await AssertInviteExpired(DateTime.UtcNow.AddDays(-14).AddSeconds(-1));
        }

        [Fact]
        public async Task Login_InvitedUser_JustExpired()
        {
            await AssertInviteExpired(DateTime.UtcNow.AddDays(-14));
        }
        
        private static async Task AssertInviteExpired(DateTime invitationCreatedDate)
        {
            var contextId = Guid.NewGuid().ToString();

            const string email = "inviteduser@example.com";
            const string firstName = "FirstName";
            const string lastName = "LastName";

            var claimsPrincipal = CreateClaimsPrincipal(email, firstName, lastName);

            var userManager = new Mock<IUserManagerDelegate>(Strict);
            var signInManager = new Mock<ISignInManagerDelegate>(Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.UserReleaseInvites.AddRangeAsync(
                    new UserReleaseInvite
                    {
                        Email = email,
                        Role = ReleaseRole.Approver,
                    },
                    new UserReleaseInvite
                    {
                        Email = email,
                        Role = ReleaseRole.Lead,
                    },
                    new UserReleaseInvite
                    {
                        Email = "anotheruser@example.com",
                    });

                await contentDbContext.UserPublicationInvites.AddRangeAsync(
                    new UserPublicationInvite
                    {
                        Email = email,
                        Role = PublicationRole.Owner
                    },
                    new UserPublicationInvite
                    {
                        Email = email,
                        Role = PublicationRole.Approver
                    },
                    new UserPublicationInvite
                    {
                        Email = "anotheruser@example.com"
                    }
                );
                await contentDbContext.SaveChangesAsync();
            }

            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await usersDbContext.UserInvites.AddRangeAsync(
                    new UserInvite
                    {
                        Email = email,
                        Role = new IdentityRole("Role A"),
                        Accepted = false,
                        Created = invitationCreatedDate  // expired
                    },
                    new UserInvite
                    {
                        Email = "anotheruser@example.com",
                        Role = new IdentityRole("Role B"),
                        Accepted = false,
                        Created = DateTime.UtcNow.AddDays(-1)  // not yet expired
                    });

                await usersDbContext.SaveChangesAsync();
            }

            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var loginService = BuildService(
                    signInManager.Object,
                    userManager.Object,
                    contentDbContext,
                    usersDbContext);

                signInManager
                    .Setup(s => s.GetExternalLoginInfoAsync(null))
                    .ReturnsAsync(new ExternalLoginInfo(
                        claimsPrincipal,
                        LoginProvider,
                        ProviderKey,
                        LoginProviderDisplayName));

                var result = await loginService.OnGetCallbackAsync(ReturnUrl);
                VerifyAllMocks(signInManager);

                AssertInviteExpired(result);
            }

            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                // Assert that the new user's expired invite has been deleted.
                var newUsersInvite = await usersDbContext
                    .UserInvites
                    .AsQueryable()
                    .SingleOrDefaultAsync(invite => invite.Email == email);
                Assert.Null(newUsersInvite);

                // Assert that the other user's invites have all been left alone.
                var otherUsersInvite = await usersDbContext
                    .UserInvites
                    .AsQueryable()
                    .SingleAsync(invite => invite.Email != email);
                Assert.False(otherUsersInvite.Accepted);

                // Assert that the new user's release role invites have all been deleted.
                var newUsersReleaseRoleInvites = await contentDbContext
                    .UserReleaseInvites
                    .AsQueryable()
                    .Where(invite => invite.Email == email)
                    .ToListAsync();
                Assert.Empty(newUsersReleaseRoleInvites);

                // Assert that the other user's release role invites have all been left alone.
                var otherUsersReleaseRoleInvites = await contentDbContext
                    .UserReleaseInvites
                    .AsQueryable()
                    .Where(invite => invite.Email != email)
                    .ToListAsync();
                Assert.Single(otherUsersReleaseRoleInvites);

                // Assert that the new user's publication role invites have all been deleted.
                var newUsersPublicationRoleInvites = await contentDbContext
                    .UserPublicationInvites
                    .AsQueryable()
                    .Where(invite => invite.Email == email)
                    .ToListAsync();
                Assert.Empty(newUsersPublicationRoleInvites);

                // Assert that the other user's publication role invites have all been left alone.
                var otherUsersPublicationRoleInvites = await contentDbContext
                    .UserPublicationInvites
                    .AsQueryable()
                    .Where(invite => invite.Email != email)
                    .ToListAsync();
                Assert.Single(otherUsersPublicationRoleInvites);

                // Assert that the user has not been assigned the new release roles.
                var newUsersReleaseRoles = await contentDbContext
                    .UserReleaseRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(newUsersReleaseRoles);

                // Assert that the user has not been assigned the new publication roles.
                var newUserPublicationRoles = await contentDbContext
                    .UserPublicationRoles
                    .AsQueryable()
                    .ToListAsync();
                Assert.Empty(newUserPublicationRoles);
            }
        }

        private async Task<(IActionResult, ExternalLoginModel)> DoExistingUserLogin(SignInResult signInResult)
        {
            var contextId = Guid.NewGuid().ToString();

            var claimsPrincipal = CreateClaimsPrincipal(
                "existinguser@example.com",
                "FirstName",
                "LastName");

            var userManager = new Mock<IUserManagerDelegate>(Strict);
            var signInManager = new Mock<ISignInManagerDelegate>(Strict);

            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await usersDbContext.UserLogins.AddAsync(new IdentityUserLogin<string>
                {
                    LoginProvider = LoginProvider,
                    ProviderKey = ProviderKey,
                    ProviderDisplayName = LoginProviderDisplayName
                });
                await usersDbContext.SaveChangesAsync();
            }

            await using var contentDbContext = InMemoryApplicationDbContext(contextId);
            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var loginService = BuildService(
                    signInManager.Object,
                    userManager.Object,
                    contentDbContext,
                    usersDbContext);

                signInManager
                    .Setup(s => s.GetExternalLoginInfoAsync(null))
                    .ReturnsAsync(new ExternalLoginInfo(
                        claimsPrincipal,
                        LoginProvider,
                        ProviderKey,
                        LoginProviderDisplayName));

                signInManager
                    .Setup(s => s.ExternalLoginSignInAsync(LoginProvider, ProviderKey, false, true))
                    .ReturnsAsync(signInResult);

                var result = await loginService.OnGetCallbackAsync(ReturnUrl);
                VerifyAllMocks(userManager, signInManager);

                return (result, loginService);
            }
        }

        private async Task<(IActionResult, ExternalLoginModel)> DoLoginExistingUserWithNewProviderKey(
            IList<IdentityUserLogin<string>> existingProviderKeys,
            IdentityResult? addNewIdentityResult = null,
            SignInResult? signInResult = null)
        {
            var contextId = Guid.NewGuid().ToString();
            const string emailAddress = "existinguser@example.com";

            var claimsPrincipal = CreateClaimsPrincipal(
                emailAddress,
                "FirstName",
                "LastName");

            const string newProviderKey = "NewProviderKey";

            var userManager = new Mock<IUserManagerDelegate>(Strict);
            var signInManager = new Mock<ISignInManagerDelegate>(Strict);

            var existingUser = new ApplicationUser
            {
                Email = emailAddress
            };

            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                await usersDbContext.Users.AddAsync(existingUser);

                if (!existingProviderKeys.IsNullOrEmpty())
                {
                    await usersDbContext.UserLogins.AddRangeAsync(existingProviderKeys);
                }

                await usersDbContext.SaveChangesAsync();
            }

            await using var contentDbContext = InMemoryApplicationDbContext(contextId);
            await using (var usersDbContext = InMemoryUserAndRolesDbContext(contextId))
            {
                var loginService = BuildService(
                    signInManager.Object,
                    userManager.Object,
                    contentDbContext,
                    usersDbContext);

                signInManager
                    .Setup(s => s.GetExternalLoginInfoAsync(null))
                    .ReturnsAsync(new ExternalLoginInfo(
                        claimsPrincipal,
                        LoginProvider,
                        newProviderKey,
                        LoginProviderDisplayName));

                var existingProviderKeyInfos = existingProviderKeys
                    .Select(p => new UserLoginInfo(p.LoginProvider, p.ProviderKey, p.ProviderDisplayName))
                    .ToList();

                userManager
                    .Setup(s => s.GetLoginsAsync(
                        It.Is<ApplicationUser>(user => user.Email == existingUser.Email)))
                    .ReturnsAsync(existingProviderKeyInfos);

                if (addNewIdentityResult != null)
                {
                    userManager
                        .Setup(s => s.AddLoginAsync(
                            It.Is<ApplicationUser>(user => user.Email == existingUser.Email),
                            It.Is<UserLoginInfo>(l => l.ProviderKey == newProviderKey
                                                      && l.LoginProvider == LoginProvider
                                                      && l.ProviderDisplayName == LoginProviderDisplayName)))
                        .ReturnsAsync(addNewIdentityResult);
                }

                if (signInResult != null)
                {
                    signInManager
                        .Setup(s => s.ExternalLoginSignInAsync(
                            LoginProvider,
                            newProviderKey,
                            false,
                            true))
                        .ReturnsAsync(signInResult);
                }

                var result = await loginService.OnGetCallbackAsync(ReturnUrl);
                VerifyAllMocks(userManager, signInManager);
                return (result, loginService);
            }
        }

        private static void AssertRedirectedToLoginPageUponFailure(
            IActionResult result,
            ExternalLoginModel loginService,
            IList<string>? expectedModelErrors = null)
        {
            var redirectPage = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./Login", redirectPage.PageName);
            Assert.False(redirectPage.Permanent);
            Assert.Equal(ExpectedLoginErrorMessage, loginService.ErrorMessage);

            if (expectedModelErrors != null)
            {
                AssertModelErrorsEqual(expectedModelErrors, loginService);
            }
            else
            {
                Assert.Empty(loginService.ModelState);
            }
        }

        private static ClaimsPrincipal CreateClaimsPrincipal(string emailAddress, string firstName, string lastName)
        {
            return ClaimsPrincipalUtils.CreateClaimsPrincipal(
                Guid.NewGuid(),
                new Claim(ClaimTypes.Email, emailAddress),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName));
        }

        private static void AssertModelErrorsEqual(IList<string> expectedErrors, ExternalLoginModel loginService)
        {
            Assert.Equal(expectedErrors, loginService
                .ModelState
                .Values
                .SelectMany(v => v
                    .Errors
                    .Select(e => e.ErrorMessage))
                .ToList());
        }

        private static void AssertSuccessfulLogin(IActionResult? result, ExternalLoginModel loginService)
        {
            var redirectPage = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(ReturnUrl, redirectPage.Url);
            Assert.False(redirectPage.Permanent);
            Assert.Null(loginService.ErrorMessage);
            Assert.Empty(loginService.ModelState);
        }
        
        private static void AssertInviteExpired(IActionResult? result)
        {
            var redirectPage = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./InviteExpired", redirectPage.PageName);
            Assert.False(redirectPage.Permanent);
        }

        private static ExternalLoginModel BuildService(
            ISignInManagerDelegate? signInManager = null,
            IUserManagerDelegate? userManager = null,
            ContentDbContext? contentDbContext = null,
            UsersAndRolesDbContext? usersDbContext = null)
        {
            return new ExternalLoginModel(
                signInManager ?? Mock.Of<ISignInManagerDelegate>(Strict),
                userManager ?? Mock.Of<IUserManagerDelegate>(Strict),
                Mock.Of<ILogger<ExternalLoginModel>>(),
                contentDbContext,
                usersDbContext);
        }
    }
}
