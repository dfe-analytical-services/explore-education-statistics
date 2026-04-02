#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security;

public static class StartupSecurityConfiguration
{
    /**
     * Configure security Policies
     */
    public static void ConfigureAuthorizationPolicies(IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // This policy ensures that the user has been issued a valid access token from the Identity Provider
            // and has consented to the SPA accessing the Admin APIs on their behalf.  This policy does not enforce
            // that the user has any roles yet.  This policy is useful in cases where protected endpoints can be
            // called by a user who is not yet fully registered on the service.
            options.AddPolicy(
                nameof(SecurityPolicies.AuthenticatedByIdentityProvider),
                policy => policy.RequireAssertion(context => context.User.HasScope(SecurityScopes.AccessAdminApiScope))
            );

            // This policy ensures that the user has been issued a valid access token from the Identity Provider
            // and has consented to the SPA accessing the Admin APIs on their behalf.  It also ensures that the user
            // has been allocated a role that gives them permission to use the protected pages of the Admin SPA and
            // the majority of the API endpoints.  This policy is useful for protecting endpoints where we expect
            // the user to have registered successfully with the service already.
            options.AddPolicy(
                nameof(SecurityPolicies.RegisteredUser),
                policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim =>
                            claim.Type == nameof(SecurityClaimTypes.ApplicationAccessGranted)
                        ) && context.User.HasScope(SecurityScopes.AccessAdminApiScope)
                    )
            );

            /*
             * General role-based page access
             */
            options.AddPolicy(nameof(SecurityPolicies.IsBauUser), policy => policy.RequireRole(RoleNames.BauUser));

            options.AddPolicy(
                nameof(SecurityPolicies.CanAccessAnalystPages),
                policy => policy.RequireClaim(nameof(SecurityClaimTypes.AnalystPagesAccessGranted))
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanAccessPrereleasePages),
                policy => policy.RequireClaim(nameof(SecurityClaimTypes.PrereleasePagesAccessGranted))
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanManageUsersOnSystem),
                policy => policy.RequireClaim(nameof(SecurityClaimTypes.ManageAnyUser))
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanAccessAllImports),
                policy => policy.RequireClaim(nameof(SecurityClaimTypes.AccessAllImports))
            );

            /*
             * Publication management
             */
            options.AddPolicy(
                nameof(SecurityPolicies.CanViewAllPublications),
                policy => policy.RequireClaim(nameof(SecurityClaimTypes.AccessAllPublications))
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanViewSpecificPublication),
                policy => policy.Requirements.Add(new ViewSpecificPublicationRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanUpdateSpecificPublicationSummary),
                policy => policy.Requirements.Add(new UpdatePublicationSummaryRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanUpdatePublication),
                policy => policy.RequireClaim(nameof(SecurityClaimTypes.UpdateAllPublications))
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanUpdateContact),
                policy => policy.Requirements.Add(new UpdateContactRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanUpdateDrafters),
                policy => policy.Requirements.Add(new UpdateDraftersRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanViewReleaseTeamAccess),
                policy => policy.Requirements.Add(new ViewSpecificPublicationReleaseTeamAccessRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanManagePublicationReleaseSeries),
                policy => policy.Requirements.Add(new ManagePublicationReleaseSeriesRequirement())
            );

            /*
             * Release management
             */
            options.AddPolicy(
                nameof(SecurityPolicies.CanCreateReleaseForSpecificPublication),
                policy => policy.Requirements.Add(new CreateReleaseForSpecificPublicationRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanUpdateSpecificRelease),
                policy => policy.Requirements.Add(new UpdateSpecificReleaseRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanViewAllReleases),
                policy => policy.RequireClaim(nameof(SecurityClaimTypes.AccessAllReleases))
            );

            options.AddPolicy(
                nameof(ContentSecurityPolicies.CanViewSpecificReleaseVersion),
                policy => policy.Requirements.Add(new ViewReleaseVersionRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanUpdateSpecificReleaseVersion),
                policy => policy.Requirements.Add(new UpdateSpecificReleaseVersionRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanMarkSpecificReleaseAsDraft),
                policy => policy.Requirements.Add(new MarkReleaseAsDraftRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanSubmitSpecificReleaseToHigherReview),
                policy => policy.Requirements.Add(new MarkReleaseAsHigherLevelReviewRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanApproveSpecificRelease),
                policy => policy.Requirements.Add(new MarkReleaseAsApprovedRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanMakeAmendmentOfSpecificReleaseVersion),
                policy => policy.Requirements.Add(new MakeAmendmentOfSpecificReleaseRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanDeleteSpecificReleaseVersion),
                policy => policy.Requirements.Add(new DeleteSpecificReleaseRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanDeleteTestRelease),
                policy => policy.Requirements.Add(new DeleteTestReleaseRequirement())
            );

            options.AddPolicy(
                nameof(DataSecurityPolicies.CanViewSubjectData),
                policy => policy.Requirements.Add(new ViewSubjectDataRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanViewSpecificPreReleaseSummary),
                policy => policy.Requirements.Add(new ViewSpecificPreReleaseSummaryRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanResolveSpecificComment),
                policy => policy.Requirements.Add(new ResolveSpecificCommentRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanUpdateSpecificComment),
                policy => policy.Requirements.Add(new UpdateSpecificCommentRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanDeleteSpecificComment),
                policy => policy.Requirements.Add(new DeleteSpecificCommentRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanCancelOngoingImports),
                policy => policy.Requirements.Add(new CancelSpecificFileImportRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanViewReleaseStatusHistory),
                policy => policy.Requirements.Add(new ViewReleaseStatusHistoryRequirement())
            );

            /*
             * Pre Release management
             */
            options.AddPolicy(
                nameof(SecurityPolicies.CanAssignPreReleaseUsersToSpecificRelease),
                policy => policy.Requirements.Add(new AssignPrereleaseContactsToSpecificReleaseRequirement())
            );

            /*
             * Taxonomy management
             */
            options.AddPolicy(
                nameof(SecurityPolicies.CanManageAllTaxonomy),
                policy => policy.RequireClaim(nameof(SecurityClaimTypes.ManageAllTaxonomy))
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanCreatePublicationForSpecificTheme),
                policy => policy.Requirements.Add(new CreatePublicationForSpecificThemeRequirement())
            );

            /*
             * Methodology management
             */
            options.AddPolicy(
                nameof(SecurityPolicies.CanAdoptMethodologyForSpecificPublication),
                policy => policy.Requirements.Add(new AdoptMethodologyForSpecificPublicationRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanCreateMethodologyForSpecificPublication),
                policy => policy.Requirements.Add(new CreateMethodologyForSpecificPublicationRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanDropMethodologyLink),
                policy => policy.Requirements.Add(new DropMethodologyLinkRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanManageExternalMethodologyForSpecificPublication),
                policy => policy.Requirements.Add(new ManageExternalMethodologyForSpecificPublicationRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanViewSpecificMethodology),
                policy => policy.Requirements.Add(new ViewSpecificMethodologyRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanUpdateSpecificMethodology),
                policy => policy.Requirements.Add(new UpdateSpecificMethodologyRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanMarkSpecificMethodologyAsDraft),
                policy => policy.Requirements.Add(new MarkMethodologyAsDraftRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanSubmitSpecificMethodologyToHigherReview),
                policy => policy.Requirements.Add(new MarkMethodologyAsHigherLevelReviewRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanApproveSpecificMethodology),
                policy => policy.Requirements.Add(new MarkMethodologyAsApprovedRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanMakeAmendmentOfSpecificMethodology),
                policy => policy.Requirements.Add(new MakeAmendmentOfSpecificMethodologyRequirement())
            );

            options.AddPolicy(
                nameof(SecurityPolicies.CanDeleteSpecificMethodology),
                policy => policy.Requirements.Add(new DeleteSpecificMethodologyRequirement())
            );
        });
    }

    /**
     * Set up our Resource-based Authorization Handlers and supporting services in DI
     */
    public static void ConfigureResourceBasedAuthorization(IServiceCollection services)
    {
        services.AddTransient<IAuthorizationService, DefaultAuthorizationService>();
        services.AddTransient<IUserService, UserService>();

        /*
         * Publication management
         */
        services.AddTransient<IAuthorizationHandler, ViewSpecificPublicationAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, UpdatePublicationSummaryAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, UpdateContactAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, CreatePublicationForSpecificThemeAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, CreateReleaseForSpecificPublicationAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, CreateMethodologyForSpecificPublicationAuthorizationHandler>();
        services.AddTransient<
            IAuthorizationHandler,
            ManageExternalMethodologyForSpecificPublicationAuthorizationHandler
        >();
        services.AddTransient<IAuthorizationHandler, ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ManagePublicationReleaseSeriesAuthorizationHandler>();

        /*
         * Release management
         */
        services.AddTransient<IAuthorizationHandler, ViewSpecificReleaseVersionAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, UpdateSpecificReleaseAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, UpdateSpecificReleaseVersionAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, DeleteSpecificReleaseAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, DeleteTestReleaseAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MarkReleaseAsDraftAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MarkReleaseAsHigherLevelReviewAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MarkReleaseAsApprovedAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MakeAmendmentOfSpecificReleaseAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ViewSubjectDataAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ViewSpecificPreReleaseSummaryAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ResolveSpecificCommentAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, UpdateSpecificCommentAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, DeleteSpecificCommentAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, CancelSpecificFileImportAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ViewReleaseStatusHistoryAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, UpdateDraftersAuthorizationHandler>();

        /*
         * Pre Release management
         */
        services.AddTransient<IAuthorizationHandler, AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler>();

        /*
         * Methodology management
         */
        services.AddTransient<IAuthorizationHandler, AdoptMethodologyForSpecificPublicationAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, DropMethodologyLinkAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, ViewSpecificMethodologyAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, UpdateSpecificMethodologyAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MarkMethodologyAsDraftAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MarkMethodologyAsHigherLevelReviewAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MarkMethodologyAsApprovedAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, MakeAmendmentOfSpecificMethodologyAuthorizationHandler>();
        services.AddTransient<IAuthorizationHandler, DeleteSpecificMethodologyAuthorizationHandler>();
    }
}
