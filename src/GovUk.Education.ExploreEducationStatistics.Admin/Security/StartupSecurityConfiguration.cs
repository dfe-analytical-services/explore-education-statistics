#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
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
                options.AddPolicy(SecurityPolicies.AuthenticatedByIdentityProvider.ToString(), policy =>
                    policy.RequireAssertion(context => context.User.HasScope(SecurityScopes.AccessAdminApiScope)));

                // This policy ensures that the user has been issued a valid access token from the Identity Provider
                // and has consented to the SPA accessing the Admin APIs on their behalf.  It also ensures that the user
                // has been allocated a role that gives them permission to use the protected pages of the Admin SPA and
                // the majority of the API endpoints.  This policy is useful for protecting endpoints where we expect
                // the user to have registered successfully with the service already.
                options.AddPolicy(SecurityPolicies.RegisteredUser.ToString(), policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim(claim =>
                            claim.Type == SecurityClaimTypes.ApplicationAccessGranted.ToString()) &&
                        context.User.HasScope(SecurityScopes.AccessAdminApiScope)));

                /**
                 * General role-based page access
                 */

                options.AddPolicy(SecurityPolicies.IsBauUser.ToString(), policy =>
                    policy.RequireRole(RoleNames.BauUser));

                options.AddPolicy(SecurityPolicies.CanAccessAnalystPages.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AnalystPagesAccessGranted.ToString()));

                options.AddPolicy(SecurityPolicies.CanAccessPrereleasePages.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.PrereleasePagesAccessGranted.ToString()));

                options.AddPolicy(SecurityPolicies.CanManageUsersOnSystem.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ManageAnyUser.ToString()));

                options.AddPolicy(SecurityPolicies.CanAccessAllImports.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllImports.ToString()));

                /**
                 * Publication management
                 */
                options.AddPolicy(SecurityPolicies.CanViewAllPublications.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllPublications.ToString()));

                options.AddPolicy(SecurityPolicies.CanViewSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificPublicationRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdateSpecificPublicationSummary.ToString(), policy =>
                    policy.Requirements.Add(new UpdatePublicationSummaryRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdatePublication.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.UpdateAllPublications.ToString()));

                options.AddPolicy(SecurityPolicies.CanUpdateContact.ToString(), policy =>
                    policy.Requirements.Add(new UpdateContactRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdateSpecificReleaseRole.ToString(), policy =>
                    policy.Requirements.Add(new UpdateReleaseRoleRequirement()));

                options.AddPolicy(SecurityPolicies.CanViewReleaseTeamAccess.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificPublicationReleaseTeamAccessRequirement()));

                options.AddPolicy(SecurityPolicies.CanManagePublicationReleaseSeries.ToString(), policy =>
                    policy.Requirements.Add(new ManagePublicationReleaseSeriesRequirement()));


                /**
                 * Release management
                 */
                options.AddPolicy(SecurityPolicies.CanCreateReleaseForSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new CreateReleaseForSpecificPublicationRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdateSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificReleaseRequirement()));

                options.AddPolicy(SecurityPolicies.CanViewAllReleases.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllReleases.ToString()));

                options.AddPolicy(ContentSecurityPolicies.CanViewSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new ViewReleaseRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdateSpecificReleaseVersion.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificReleaseVersionRequirement()));

                options.AddPolicy(SecurityPolicies.CanMarkSpecificReleaseAsDraft.ToString(), policy =>
                    policy.Requirements.Add(new MarkReleaseAsDraftRequirement()));

                options.AddPolicy(SecurityPolicies.CanSubmitSpecificReleaseToHigherReview.ToString(), policy =>
                    policy.Requirements.Add(new MarkReleaseAsHigherLevelReviewRequirement()));

                options.AddPolicy(SecurityPolicies.CanApproveSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new MarkReleaseAsApprovedRequirement()));

                options.AddPolicy(SecurityPolicies.CanMakeAmendmentOfSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new MakeAmendmentOfSpecificReleaseRequirement()));

                options.AddPolicy(SecurityPolicies.CanPublishSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new PublishSpecificReleaseRequirement()));

                options.AddPolicy(SecurityPolicies.CanDeleteSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new DeleteSpecificReleaseRequirement()));

                options.AddPolicy(SecurityPolicies.CanDeleteTestRelease.ToString(), policy =>
                    policy.Requirements.Add(new DeleteTestReleaseRequirement()));

                options.AddPolicy(DataSecurityPolicies.CanViewSubjectData.ToString(), policy =>
                    policy.Requirements.Add(new ViewSubjectDataRequirement()));

                options.AddPolicy(SecurityPolicies.CanViewSpecificPreReleaseSummary.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificPreReleaseSummaryRequirement()));

                options.AddPolicy(SecurityPolicies.CanResolveSpecificComment.ToString(), policy =>
                    policy.Requirements.Add(new ResolveSpecificCommentRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdateSpecificComment.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificCommentRequirement()));

                options.AddPolicy(SecurityPolicies.CanDeleteSpecificComment.ToString(), policy =>
                    policy.Requirements.Add(new DeleteSpecificCommentRequirement()));

                options.AddPolicy(SecurityPolicies.CanCancelOngoingImports.ToString(), policy =>
                    policy.Requirements.Add(new CancelSpecificFileImportRequirement()));

                options.AddPolicy(SecurityPolicies.CanViewReleaseStatusHistory.ToString(), policy =>
                    policy.Requirements.Add(new ViewReleaseStatusHistoryRequirement()));

                /**
                 * Pre Release management
                 */
                options.AddPolicy(SecurityPolicies.CanAssignPreReleaseUsersToSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new AssignPrereleaseContactsToSpecificReleaseRequirement()));

                /**
                 * Taxonomy management
                 */
                options.AddPolicy(SecurityPolicies.CanManageAllTaxonomy.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ManageAllTaxonomy.ToString()));

                options.AddPolicy(SecurityPolicies.CanCreatePublicationForSpecificTheme.ToString(), policy =>
                    policy.Requirements.Add(new CreatePublicationForSpecificThemeRequirement()));

                /**
                 * Methodology management
                 */
                options.AddPolicy(SecurityPolicies.CanAdoptMethodologyForSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new AdoptMethodologyForSpecificPublicationRequirement()));

                options.AddPolicy(SecurityPolicies.CanCreateMethodologyForSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new CreateMethodologyForSpecificPublicationRequirement()));

                options.AddPolicy(SecurityPolicies.CanDropMethodologyLink.ToString(), policy =>
                    policy.Requirements.Add(new DropMethodologyLinkRequirement()));

                options.AddPolicy(SecurityPolicies.CanManageExternalMethodologyForSpecificPublication.ToString(),
                    policy =>
                        policy.Requirements.Add(new ManageExternalMethodologyForSpecificPublicationRequirement()));

                options.AddPolicy(SecurityPolicies.CanViewSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificMethodologyRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdateSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificMethodologyRequirement()));

                options.AddPolicy(SecurityPolicies.CanMarkSpecificMethodologyAsDraft.ToString(), policy =>
                    policy.Requirements.Add(new MarkMethodologyAsDraftRequirement()));

                options.AddPolicy(SecurityPolicies.CanSubmitSpecificMethodologyToHigherReview.ToString(), policy =>
                    policy.Requirements.Add(new MarkMethodologyAsHigherLevelReviewRequirement()));

                options.AddPolicy(SecurityPolicies.CanApproveSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new MarkMethodologyAsApprovedRequirement()));

                options.AddPolicy(SecurityPolicies.CanMakeAmendmentOfSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new MakeAmendmentOfSpecificMethodologyRequirement()));

                options.AddPolicy(SecurityPolicies.CanDeleteSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new DeleteSpecificMethodologyRequirement()));
            });
        }

        /**
         * Set up our Resource-based Authorization Handlers and supporting services in DI
         */
        public static void ConfigureResourceBasedAuthorization(IServiceCollection services)
        {
            services.AddTransient<IAuthorizationService, DefaultAuthorizationService>();
            services.AddTransient<IUserService, UserService>();

            /**
             * Publication management
             */
            services.AddTransient<IAuthorizationHandler, ViewSpecificPublicationAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdatePublicationSummaryAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateContactAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CreatePublicationForSpecificThemeAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CreateReleaseForSpecificPublicationAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CreateMethodologyForSpecificPublicationAuthorizationHandler>();
            services
                .AddTransient<IAuthorizationHandler,
                    ManageExternalMethodologyForSpecificPublicationAuthorizationHandler>();
            services
                .AddTransient<IAuthorizationHandler, ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ManagePublicationReleaseSeriesAuthorizationHandler>();

            /**
             * Release management
             */
            services.AddTransient<IAuthorizationHandler, ViewSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateSpecificReleaseVersionAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, DeleteSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, DeleteTestReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MarkReleaseAsDraftAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MarkReleaseAsHigherLevelReviewAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MarkReleaseAsApprovedAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MakeAmendmentOfSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewSubjectDataAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, PublishSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewSpecificPreReleaseSummaryAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ResolveSpecificCommentAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateSpecificCommentAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, DeleteSpecificCommentAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CancelSpecificFileImportAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewReleaseStatusHistoryAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateReleaseRoleAuthorizationHandler>();

            /**
             * Pre Release management
             */
            services
                .AddTransient<IAuthorizationHandler, AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler>();

            /**
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
}
