#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Security;

public class SignInControllerTests
{
    [Fact]
    public async Task RegisterOrSignIn_SignInServiceReturnsInternalServerError_ReturnsInternalServerError()
    {
        var remoteUserId = Guid.NewGuid().ToString();
        var claims = new[] { new Claim(ClaimConstants.NameIdentifierId, remoteUserId) };
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };

        var logger = new Mock<ILogger<SignInController>>(Strict);
        logger.Setup(x =>
            x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (v, t) =>
                        v.ToString()!.Contains($"User with remote Id \"{remoteUserId}\" failed to sign in or register")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );

        var httpContextAccessor = new Mock<IHttpContextAccessor>(Strict);
        httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider))
            .ReturnsAsync(true);

        var signInService = new Mock<ISignInService>(Strict);
        signInService.Setup(mock => mock.RegisterOrSignIn()).ReturnsAsync(new StatusCodeResult(500));

        var service = BuildService(
            logger: logger.Object,
            httpContextAccessor: httpContextAccessor.Object,
            signInService: signInService.Object,
            userService: userService.Object
        );

        var result = await service.RegisterOrSignIn();

        result.AssertInternalServerError();

        VerifyAllMocks(logger, httpContextAccessor, userService, signInService);
    }

    [Fact]
    public async Task RegisterOrSignIn_SignInServiceThrowsException_ThrowsException()
    {
        var remoteUserId = Guid.NewGuid().ToString();
        var claims = new[] { new Claim(ClaimConstants.NameIdentifierId, remoteUserId) };
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };

        var httpContextAccessor = new Mock<IHttpContextAccessor>(Strict);
        httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider))
            .ReturnsAsync(true);

        var signInService = new Mock<ISignInService>(Strict);
        signInService.Setup(mock => mock.RegisterOrSignIn()).ThrowsAsync(new InvalidOperationException());

        var service = BuildService(
            httpContextAccessor: httpContextAccessor.Object,
            signInService: signInService.Object,
            userService: userService.Object
        );

        await Assert.ThrowsAsync<InvalidOperationException>(service.RegisterOrSignIn);

        VerifyAllMocks(httpContextAccessor, userService, signInService);
    }

    [Fact]
    public async Task RegisterOrSignIn_LoginSuccess_ReturnsLoginSuccess()
    {
        var userProfileId = Guid.NewGuid();
        var userProfileFirstName = "FirstName";
        var claims = new[] { new Claim(ClaimConstants.NameIdentifierId, Guid.NewGuid().ToString()) };
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };

        var expectedViewModel = new SignInResponseViewModel(
            LoginResult.LoginSuccess,
            new UserProfile(userProfileId, userProfileFirstName)
        );

        var httpContextAccessor = new Mock<IHttpContextAccessor>(Strict);
        httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider))
            .ReturnsAsync(true);

        var signInService = new Mock<ISignInService>(Strict);
        signInService.Setup(mock => mock.RegisterOrSignIn()).ReturnsAsync(expectedViewModel);

        var service = BuildService(
            httpContextAccessor: httpContextAccessor.Object,
            signInService: signInService.Object,
            userService: userService.Object
        );

        var result = await service.RegisterOrSignIn();

        var signInResponse = result.AssertOkResult(expectedViewModel);

        VerifyAllMocks(httpContextAccessor, userService, signInService);
    }

    [Fact]
    public async Task RegisterOrSignIn_RegistrationSuccess_ReturnsRegistrationSuccess()
    {
        var userProfileId = Guid.NewGuid();
        var userProfileFirstName = "FirstName";
        var claims = new[] { new Claim(ClaimConstants.NameIdentifierId, Guid.NewGuid().ToString()) };
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };

        var expectedViewModel = new SignInResponseViewModel(
            LoginResult.RegistrationSuccess,
            new UserProfile(userProfileId, userProfileFirstName)
        );

        var httpContextAccessor = new Mock<IHttpContextAccessor>(Strict);
        httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider))
            .ReturnsAsync(true);

        var signInService = new Mock<ISignInService>(Strict);
        signInService.Setup(mock => mock.RegisterOrSignIn()).ReturnsAsync(expectedViewModel);

        var service = BuildService(
            httpContextAccessor: httpContextAccessor.Object,
            signInService: signInService.Object,
            userService: userService.Object
        );

        var result = await service.RegisterOrSignIn();

        var signInResponse = result.AssertOkResult(expectedViewModel);

        VerifyAllMocks(httpContextAccessor, userService, signInService);
    }

    [Fact]
    public async Task RegisterOrSignIn_NoInvite_ReturnsNoInvite()
    {
        var remoteUserId = Guid.NewGuid().ToString();
        var claims = new[] { new Claim(ClaimConstants.NameIdentifierId, remoteUserId) };
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };

        var expectedViewModel = new SignInResponseViewModel(LoginResult.NoInvite);

        var logger = new Mock<ILogger<SignInController>>(Strict);
        logger.Setup(x =>
            x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (v, t) => v.ToString()!.Contains($"User with remote Id \"{remoteUserId}\" had no invite to accept")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );

        var httpContextAccessor = new Mock<IHttpContextAccessor>(Strict);
        httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider))
            .ReturnsAsync(true);

        var signInService = new Mock<ISignInService>(Strict);
        signInService.Setup(mock => mock.RegisterOrSignIn()).ReturnsAsync(expectedViewModel);

        var service = BuildService(
            logger: logger.Object,
            httpContextAccessor: httpContextAccessor.Object,
            signInService: signInService.Object,
            userService: userService.Object
        );

        var result = await service.RegisterOrSignIn();

        var signInResponse = result.AssertOkResult(expectedViewModel);

        VerifyAllMocks(logger, httpContextAccessor, userService, signInService);
    }

    [Fact]
    public async Task RegisterOrSignIn_ExpiredInvite_ReturnsExpiredInvite()
    {
        var remoteUserId = Guid.NewGuid().ToString();
        var claims = new[] { new Claim(ClaimConstants.NameIdentifierId, remoteUserId) };
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims)) };

        var expectedViewModel = new SignInResponseViewModel(LoginResult.ExpiredInvite);

        var logger = new Mock<ILogger<SignInController>>(Strict);
        logger.Setup(x =>
            x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>(
                    (v, t) => v.ToString()!.Contains($"User with remote Id \"{remoteUserId}\" had an expired invite")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            )
        );

        var httpContextAccessor = new Mock<IHttpContextAccessor>(Strict);
        httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);

        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider))
            .ReturnsAsync(true);

        var signInService = new Mock<ISignInService>(Strict);
        signInService.Setup(mock => mock.RegisterOrSignIn()).ReturnsAsync(expectedViewModel);

        var service = BuildService(
            logger: logger.Object,
            httpContextAccessor: httpContextAccessor.Object,
            signInService: signInService.Object,
            userService: userService.Object
        );

        var result = await service.RegisterOrSignIn();

        var signInResponse = result.AssertOkResult(expectedViewModel);

        VerifyAllMocks(logger, httpContextAccessor, userService, signInService);
    }

    [Fact]
    public async Task RegisterOrSignIn_NotAuthenticatedByIdentityProvider_ReturnsForbiddenResult()
    {
        var userService = new Mock<IUserService>(Strict);
        userService
            .Setup(mock => mock.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider))
            .ReturnsAsync(false);

        var service = BuildService(userService: userService.Object);

        var result = await service.RegisterOrSignIn();

        result.Result!.AssertForbidden();

        VerifyAllMocks(userService);
    }

    private static SignInController BuildService(
        ILogger<SignInController>? logger = null,
        IHttpContextAccessor? httpContextAccessor = null,
        ISignInService? signInService = null,
        IUserService? userService = null
    )
    {
        return new SignInController(
            logger: logger ?? Mock.Of<ILogger<SignInController>>(Strict),
            httpContextAccessor: httpContextAccessor ?? Mock.Of<IHttpContextAccessor>(Strict),
            signInService: signInService ?? Mock.Of<ISignInService>(Strict),
            userService: userService ?? Mock.Of<IUserService>(Strict)
        );
    }
}
