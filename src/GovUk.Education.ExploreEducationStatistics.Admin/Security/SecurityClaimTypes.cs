#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security;

public static class SecurityScopes
{
    /// <summary>
    /// This is the name of the Scope that the SPA will be passing to the Admin API with their valid JWTs.
    /// They will have gained this in their scope Claims when logging in via the Identity Provider.
    /// The "simple" name of the scope is provided to the Admin API always, despite the SPA requesting the scope
    /// with its fully-qualified "api://guid/access-admin-api" name in the case of Entra ID (in the case of
    /// Keycloak, only the simple scope name is ever required when requesting access tokens however).
    /// </summary>
    public const string AccessAdminApiScope = "access-admin-api";
}

public enum SecurityClaimTypes
{
    /**
     * General role-based page access
     */
    ApplicationAccessGranted,
    AnalystPagesAccessGranted,
    PrereleasePagesAccessGranted,
    ManageAnyUser,
    AccessAllImports,

    /**
     * Publication management
     */
    AccessAllPublications,
    CreateAnyPublication,
    UpdateAllPublications,
    AdoptAnyMethodology,

    /**
     * Release management
     */
    CreateAnyRelease,
    AccessAllReleases,
    UpdateAllReleases,
    MarkAllReleasesAsDraft,
    SubmitAllReleasesToHigherReview,
    ApproveAllReleases,
    MakeAmendmentsOfAllReleases,
    PublishAllReleases,
    DeleteAllReleaseAmendments,
    CancelAllFileImports,

    /**
     * Pre Release management
     */
    CanViewPrereleaseContacts,

    /**
     * Taxonomy management
     */
    ManageAllTaxonomy,

    /**
     * Methodology management
     */
    CreateAnyMethodology,
    AccessAllMethodologies,
    UpdateAllMethodologies,
    MarkAllMethodologiesDraft,
    SubmitAllMethodologiesToHigherReview,
    ApproveAllMethodologies,
    MakeAmendmentsOfAllMethodologies,
    DeleteAllMethodologies
}
