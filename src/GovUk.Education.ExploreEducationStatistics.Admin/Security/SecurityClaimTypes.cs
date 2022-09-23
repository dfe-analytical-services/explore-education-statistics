#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
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
        ApproveAllMethodologies,
        MarkAllMethodologiesDraft,
        MakeAmendmentsOfAllMethodologies,
        DeleteAllMethodologies
    }
}
