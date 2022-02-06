#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.Core.Internal;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Pages.Account;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            
            var redirectPage = Assert.IsType<LocalRedirectResult>(result);
            Assert.Equal(ReturnUrl, redirectPage.Url);
            Assert.False(redirectPage.Permanent);
            Assert.Null(loginService.ErrorMessage);
            Assert.Empty(loginService.ModelState);
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
            var claimsPrincipal = AuthorizationHandlersTestUtil.CreateClaimsPrincipal(
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

            var loginService = BuildService(
                signInManager.Object);

            var result = await loginService.OnGetCallbackAsync(ReturnUrl);
            VerifyAllMocks(signInManager);
            
            AssertRedirectedToLoginPageUponFailure(result, loginService);
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
            var emailAddress = "existinguser@example.com";

            var claimsPrincipal = CreateClaimsPrincipal(
                emailAddress,
                "FirstName",
                "LastName");

            var newProviderKey = "NewProviderKey";

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
            return AuthorizationHandlersTestUtil.CreateClaimsPrincipal(
                Guid.NewGuid(),
                new Claim(ClaimTypes.Email, emailAddress),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName));
        }

        private static void AssertModelErrorsEqual(IList<string> expectedErrors, ExternalLoginModel? loginService)
        {
            Assert.Equal(expectedErrors,
                loginService.ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList());
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