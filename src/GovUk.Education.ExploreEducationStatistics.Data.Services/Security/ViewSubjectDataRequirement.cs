#nullable enable
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security;

/// <summary>
/// Authorization requirement that is used by different handlers in the Data.Api and Admin services.
/// </summary>
public class ViewSubjectDataRequirement : IAuthorizationRequirement
{
}
