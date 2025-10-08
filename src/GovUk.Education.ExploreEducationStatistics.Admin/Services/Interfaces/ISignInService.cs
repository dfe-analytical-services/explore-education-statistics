#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface ISignInService
{
    public Task<Either<ActionResult, SignInResponseViewModel>> RegisterOrSignIn();
}

public enum LoginResult
{
    LoginSuccess,
    RegistrationSuccess,
    NoInvite,
    ExpiredInvite,
}

public record UserProfile(Guid Id, string FirstName);

public record SignInResponseViewModel(
    [property: JsonConverter(typeof(StringEnumConverter))] LoginResult LoginResult,
    UserProfile? UserProfile = null
);
