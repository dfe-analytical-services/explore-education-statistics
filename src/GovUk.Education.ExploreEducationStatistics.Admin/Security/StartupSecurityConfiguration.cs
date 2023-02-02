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
                // does this user have minimal permissions to access any admin APIs?
                options.AddPolicy(SecurityPolicies.CanAccessSystem.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ApplicationAccessGranted.ToString()));

                // does this user have the Bau user role?
                options.AddPolicy(SecurityPolicies.IsBauUser.ToString(), policy =>
                    policy.RequireRole(RoleNames.BauUser));

                // does this user have permissions to access analyst pages?
                options.AddPolicy(SecurityPolicies.CanAccessAnalystPages.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AnalystPagesAccessGranted.ToString()));

                // does this user have permissions to access prerelease pages?
                options.AddPolicy(SecurityPolicies.CanAccessPrereleasePages.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.PrereleasePagesAccessGranted.ToString()));

                // does this user have permissions to invite and manage all users on the system?
                options.AddPolicy(SecurityPolicies.CanManageUsersOnSystem.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ManageAnyUser.ToString()));

                options.AddPolicy(SecurityPolicies.CanAccessAllImports.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllImports.ToString()));

                /**
                 * Publication management
                 */
                options.AddPolicy(SecurityPolicies.CanViewAllPublications.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllPublications.ToString()));

                // does this user have permission to view a specific Publication?
                options.AddPolicy(SecurityPolicies.CanViewSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificPublicationRequirement()));

                // does this user have permission to update a specific Publication summary?
                options.AddPolicy(SecurityPolicies.CanUpdateSpecificPublicationSummary.ToString(), policy =>
                    policy.Requirements.Add(new UpdatePublicationSummaryRequirement()));

                // does this user have permission to update publication details (e.g. title, theme/topic, supersededById)
                options.AddPolicy(SecurityPolicies.CanUpdatePublication.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.UpdateAllPublications.ToString()));

                // does this user have permission to update a publication's contact?
                options.AddPolicy(SecurityPolicies.CanUpdateContact.ToString(), policy =>
                    policy.Requirements.Add(new UpdateContactRequirement()));

                // does this user have permission to update a ReleaseRole on a specific Publication?
                options.AddPolicy(SecurityPolicies.CanUpdateSpecificReleaseRole.ToString(), policy =>
                    policy.Requirements.Add(new UpdateReleaseRoleRequirement()));

                // does this user have permission to view the team access for releases of a specific Publication?
                options.AddPolicy(SecurityPolicies.CanViewReleaseTeamAccess.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificPublicationReleaseTeamAccessRequirement()));

                /**
                 * Release management
                 */
                // does this user have permission to create a release under a specific publication?
                options.AddPolicy(SecurityPolicies.CanCreateReleaseForSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new CreateReleaseForSpecificPublicationRequirement()));

                // does this user have permission to view all Releases across the application?
                options.AddPolicy(SecurityPolicies.CanViewAllReleases.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllReleases.ToString()));

                // does this user have permission to view a specific Release?
                options.AddPolicy(ContentSecurityPolicies.CanViewSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new ViewReleaseRequirement()));

                // does this user have permission to update a specific Release?
                options.AddPolicy(SecurityPolicies.CanUpdateSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificReleaseRequirement()));

                // does this user have permission to mark a specific Release as a Draft?
                options.AddPolicy(SecurityPolicies.CanMarkSpecificReleaseAsDraft.ToString(), policy =>
                    policy.Requirements.Add(new MarkReleaseAsDraftRequirement()));

                // does this user have permission to submit a specific Release to Higher Review?
                options.AddPolicy(SecurityPolicies.CanSubmitSpecificReleaseToHigherReview.ToString(), policy =>
                    policy.Requirements.Add(new MarkReleaseAsHigherLevelReviewRequirement()));

                // does this user have permission to approve a specific Release?
                options.AddPolicy(SecurityPolicies.CanApproveSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new MarkReleaseAsApprovedRequirement()));

                // does this user have permission to make an amendment of an existing release?
                options.AddPolicy(SecurityPolicies.CanMakeAmendmentOfSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new MakeAmendmentOfSpecificReleaseRequirement()));

                // does this user have permission to publish a specific Release?
                options.AddPolicy(SecurityPolicies.CanPublishSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new PublishSpecificReleaseRequirement()));

                // does this user have permission to delete an existing release?
                options.AddPolicy(SecurityPolicies.CanDeleteSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new DeleteSpecificReleaseRequirement()));

                // does this user have permission to view the subject data of a specific Release?
                options.AddPolicy(DataSecurityPolicies.CanViewSubjectData.ToString(), policy =>
                    policy.Requirements.Add(new ViewSubjectDataRequirement()));

                // does this user have permission to view a specific PreRelease Summary?
                options.AddPolicy(SecurityPolicies.CanViewSpecificPreReleaseSummary.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificPreReleaseSummaryRequirement()));

                // does this user have permission to resolve a specific Comment?
                options.AddPolicy(SecurityPolicies.CanResolveSpecificComment.ToString(), policy =>
                    policy.Requirements.Add(new ResolveSpecificCommentRequirement()));

                // does this user have permission to update a specific Comment?
                options.AddPolicy(SecurityPolicies.CanUpdateSpecificComment.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificCommentRequirement()));

                // does this user have permission to delete a specific Comment?
                options.AddPolicy(SecurityPolicies.CanDeleteSpecificComment.ToString(), policy =>
                    policy.Requirements.Add(new DeleteSpecificCommentRequirement()));

                // does this user have permission to cancel an ongoing file import?
                options.AddPolicy(SecurityPolicies.CanCancelOngoingImports.ToString(), policy =>
                    policy.Requirements.Add(new CancelSpecificFileImportRequirement()));

                // does this user have permission to view a release's status history?
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
                // does this user have permissions to view the prerelease contacts list?
                options.AddPolicy(SecurityPolicies.CanViewPrereleaseContacts.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.CanViewPrereleaseContacts.ToString()));

                // does this user have permission to assign prerelease contacts to a specific Release?
                options.AddPolicy(SecurityPolicies.CanAssignPrereleaseContactsToSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new AssignPrereleaseContactsToSpecificReleaseRequirement()));

                /**
                 * Taxonomy management
                 */
                // does this user have permission to manage all taxonomy?
                options.AddPolicy(SecurityPolicies.CanManageAllTaxonomy.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ManageAllTaxonomy.ToString()));

                // does this user have permission to create a publication under a specific topic?
                options.AddPolicy(SecurityPolicies.CanCreatePublicationForSpecificTopic.ToString(), policy =>
                    policy.Requirements.Add(new CreatePublicationForSpecificTopicRequirement()));

                /**
                 * Methodology management
                 */
                // does this user have permission to adopt a methodology for a specific Publication?
                options.AddPolicy(SecurityPolicies.CanAdoptMethodologyForSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new AdoptMethodologyForSpecificPublicationRequirement()));

                // does this user have permission to create a methodology for a specific Publication?
                options.AddPolicy(SecurityPolicies.CanCreateMethodologyForSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new CreateMethodologyForSpecificPublicationRequirement()));

                // does this user have permission to drop a link to an adopted Methodology?
                options.AddPolicy(SecurityPolicies.CanDropMethodologyLink.ToString(), policy =>
                    policy.Requirements.Add(new DropMethodologyLinkRequirement()));

                // does this user have permission to manage the external methodology for a specific Publication?
                options.AddPolicy(SecurityPolicies.CanManageExternalMethodologyForSpecificPublication.ToString(),
                    policy =>
                        policy.Requirements.Add(new ManageExternalMethodologyForSpecificPublicationRequirement()));

                options.AddPolicy(SecurityPolicies.CanViewSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificMethodologyRequirement()));

                options.AddPolicy(SecurityPolicies.CanUpdateSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificMethodologyRequirement()));

                // does this user have permission to mark a specific Methodology as Draft?
                options.AddPolicy(SecurityPolicies.CanMarkSpecificMethodologyAsDraft.ToString(), policy =>
                    policy.Requirements.Add(new MethodologyStatusAuthorizationHandlers.MarkSpecificMethodologyAsDraftRequirement()));

                // does this user have permission to approve a specific Methodology?
                options.AddPolicy(SecurityPolicies.CanApproveSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new MethodologyStatusAuthorizationHandlers.ApproveSpecificMethodologyRequirement()));

                // does this user have permission to create an Amendment of a specific Methodology?
                options.AddPolicy(SecurityPolicies.CanMakeAmendmentOfSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new MakeAmendmentOfSpecificMethodologyRequirement()));

                // does this user have permission to cancel a specific Methodology Amendment?
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
            services.AddTransient<IAuthorizationHandler, MethodologyStatusAuthorizationHandlers.MarkSpecificMethodologyAsDraftAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MethodologyStatusAuthorizationHandlers.ApproveSpecificMethodologyAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MakeAmendmentOfSpecificMethodologyAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, DeleteSpecificMethodologyAuthorizationHandler>();
        }
    }
}
