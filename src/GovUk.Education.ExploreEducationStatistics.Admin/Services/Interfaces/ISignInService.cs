#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface ISignInService
{
    public Task<Either<ActionResult, LoginResult>> RegisterOrSignIn();
}

public enum LoginResult
{
    LoginSuccess,
    RegistrationSuccess,
    NoInvite,
    ExpiredInvite
}
