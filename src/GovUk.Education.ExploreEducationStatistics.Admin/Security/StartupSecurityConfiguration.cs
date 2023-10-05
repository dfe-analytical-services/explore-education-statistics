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
                /**
                 * General role-based page access
                 */
                options.AddPolicy(SecurityPolicies.CanAccessSystem.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ApplicationAccessGranted.ToString()));

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

                /**
                 * Release management
                 */
                options.AddPolicy(SecurityPolicies.CanCreateReleaseForSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new CreateReleaseForSpecificPublicationRequirement()));

                options.AddPolicy(SecurityPolicies.CanViewAllReleases.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllReleases.ToString()));

                options.AddPolicy(ContentSecurityPolicies.CanViewSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new ViewReleaseRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdateSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificReleaseRequirement()));

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
                 * Legacy release management
                 */
                options.AddPolicy(SecurityPolicies.CanManageLegacyReleases.ToString(), policy =>
                    policy.Requirements.Add(new ManageLegacyReleasesRequirement()));
                options.AddPolicy(SecurityPolicies.CanViewLegacyRelease.ToString(), policy =>
                    policy.Requirements.Add(new ViewLegacyReleaseRequirement()));
                options.AddPolicy(SecurityPolicies.CanUpdateLegacyRelease.ToString(), policy =>
                    policy.Requirements.Add(new UpdateLegacyReleaseRequirement()));
                options.AddPolicy(SecurityPolicies.CanDeleteLegacyRelease.ToString(), policy =>
                    policy.Requirements.Add(new DeleteLegacyReleaseRequirement()));

                /**
                 * Pre Release management
                 */
                options.AddPolicy(SecurityPolicies.CanViewPrereleaseContacts.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.CanViewPrereleaseContacts.ToString()));

                options.AddPolicy(SecurityPolicies.CanAssignPrereleaseContactsToSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new AssignPrereleaseContactsToSpecificReleaseRequirement()));

                /**
                 * Taxonomy management
                 */
                options.AddPolicy(SecurityPolicies.CanManageAllTaxonomy.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ManageAllTaxonomy.ToString()));

                options.AddPolicy(SecurityPolicies.CanCreatePublicationForSpecificTopic.ToString(), policy =>
                    policy.Requirements.Add(new CreatePublicationForSpecificTopicRequirement()));

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
            services.AddTransient<IAuthorizationHandler, CreatePublicationForSpecificTopicAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CreateReleaseForSpecificPublicationAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CreateMethodologyForSpecificPublicationAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ManageExternalMethodologyForSpecificPublicationAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewSpecificPublicationReleaseTeamAccessAuthorizationHandler>();

            /**
             * Release management
             */
            services.AddTransient<IAuthorizationHandler, ViewReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, DeleteSpecificReleaseAuthorizationHandler>();
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
             * Legacy release management
             */
            services.AddTransient<IAuthorizationHandler, ManageLegacyReleasesAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ViewLegacyReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateLegacyReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, DeleteLegacyReleaseAuthorizationHandler>();

            /**
             * Pre Release management
             */
            services.AddTransient<IAuthorizationHandler, AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler>();

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
