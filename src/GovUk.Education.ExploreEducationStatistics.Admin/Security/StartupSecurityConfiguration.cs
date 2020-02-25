using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

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

                // does this user have permissions to access analyst pages?
                options.AddPolicy(SecurityPolicies.CanAccessAnalystPages.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AnalystPagesAccessGranted.ToString()));

                // does this user have permissions to access prerelease pages?
                options.AddPolicy(SecurityPolicies.CanAccessPrereleasePages.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.PrereleasePagesAccessGranted.ToString()));
                
                // does this user have permissions to invite and manage all users on the system?
                options.AddPolicy(SecurityPolicies.CanManageUsersOnSystem.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ManageAnyUser.ToString()));
                
                // does this user have permissions to manage all methodologies on the system?
                options.AddPolicy(SecurityPolicies.CanManageMethodologiesOnSystem.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.ManageAnyMethodology.ToString()));

                /**
                 * Publication management
                 */
                // does this user have permission to create a publication under a specific topic?
                options.AddPolicy(SecurityPolicies.CanCreatePublicationForSpecificTopic.ToString(), policy =>
                    policy.Requirements.Add(new CreatePublicationForSpecificTopicRequirement()));

                // does this user have permission to view a specific Publication?
                options.AddPolicy(SecurityPolicies.CanViewSpecificPublication.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificPublicationRequirement()));

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
                options.AddPolicy(SecurityPolicies.CanViewSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificReleaseRequirement()));

                // does this user have permission to update a specific Release?
                options.AddPolicy(SecurityPolicies.CanUpdateSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificReleaseRequirement()));

                // does this user have permission to mark a specific Release as a Draft?
                options.AddPolicy(SecurityPolicies.CanMarkSpecificReleaseAsDraft.ToString(), policy =>
                    policy.Requirements.Add(new MarkSpecificReleaseAsDraftRequirement()));

                // does this user have permission to submit a specific Release to Higher Review?
                options.AddPolicy(SecurityPolicies.CanSubmitSpecificReleaseToHigherReview.ToString(), policy =>
                    policy.Requirements.Add(new SubmitSpecificReleaseToHigherReviewRequirement()));

                // does this user have permission to approve a specific Release?
                options.AddPolicy(SecurityPolicies.CanApproveSpecificRelease.ToString(), policy =>
                    policy.Requirements.Add(new ApproveSpecificReleaseRequirement()));
                
                // does this user have permission to view the subject data of a specific Release?
                options.AddPolicy(DataSecurityPolicies.CanViewSubjectData.ToString(), policy => 
                    policy.Requirements.Add(new CanViewSubjectDataRequirement()));

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
                 * Topic / Theme management
                 */
                // does this user have permission to view all Topics across the application?
                options.AddPolicy(SecurityPolicies.CanViewAllTopics.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllTopics.ToString()));

                // does this user have permission to view a specific Theme?
                options.AddPolicy(SecurityPolicies.CanViewSpecificTheme.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificThemeRequirement()));

                /**
                 * Methodology management
                 */
                // does this user have permission to create a methodology?
                options.AddPolicy(SecurityPolicies.CanCreateMethodologies.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.CreateAnyMethodology.ToString()));
                
                // does this user have permissions to view all methodologies on the system?
                options.AddPolicy(SecurityPolicies.CanViewAllMethodologies.ToString(), policy =>
                    policy.RequireClaim(SecurityClaimTypes.AccessAllMethodologies.ToString()));

                // does this user have permission to view a specific Methodology?
                options.AddPolicy(SecurityPolicies.CanViewSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new ViewSpecificMethodologyRequirement()));

                // does this user have permissions to update all methodologies on the system?
                options.AddPolicy(SecurityPolicies.CanUpdateSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new UpdateSpecificMethodologyRequirement()));

                // does this user have permission to mark a specific Methodology as Draft?
                options.AddPolicy(SecurityPolicies.CanMarkSpecificMethodologyAsDraft.ToString(), policy =>
                    policy.Requirements.Add(new MarkSpecificMethodologyAsDraftRequirement()));

                // does this user have permission to approve a specific Methodology?
                options.AddPolicy(SecurityPolicies.CanApproveSpecificMethodology.ToString(), policy =>
                    policy.Requirements.Add(new ApproveSpecificMethodologyRequirement()));
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
            services
                .AddTransient<IAuthorizationHandler, CreatePublicationForSpecificTopicCanCreateForAnyTopicAuthorizationHandler
                >();

            /**
             * Release management
             */
            services.AddTransient<IAuthorizationHandler, ViewSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MarkSpecificReleaseAsDraftAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, SubmitSpecificReleaseToHigherReviewAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ApproveSpecificReleaseAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CreateReleaseForSpecificPublicationAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, CanViewSubjectDataAuthorizationHandler>();

            /**
             * Pre Release management
             */
            services.AddTransient<IAuthorizationHandler, AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler>();
            
            /**
             * Topic / Theme management
             */
            services.AddTransient<IAuthorizationHandler, ViewSpecificThemeAuthorizationHandler>();

            /**
             * Methodology management
             */
            services.AddTransient<IAuthorizationHandler, ViewSpecificMethodologyAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, UpdateSpecificMethodologyAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, MarkSpecificMethodologyAsDraftAuthorizationHandler>();
            services.AddTransient<IAuthorizationHandler, ApproveSpecificMethodologyAuthorizationHandler>();
        }
    }
}