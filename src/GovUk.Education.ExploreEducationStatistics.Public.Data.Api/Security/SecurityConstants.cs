namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;

public static class SecurityConstants
{
    /// <summary>
    /// Name of an App Role created on the Public API's App Registration, that when assigned to
    /// Applications or their Managed Identities, allows the caller to access unpublished data.
    /// </summary>
    public const string UnpublishedDataReaderAppRole = "UnpublishedData.Read";

    /// <summary>
    /// A User Agent value for use in development to simulate authentication from Admin to the
    /// Public API.
    /// </summary>
    public const string AdminUserAgent = "EES Admin";
}
