using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles.RoleNames;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils
{
    public static class ClaimsPrincipalUtils
    {
        public static ClaimsPrincipal VerifiedButNotAuthorizedByIdentityProviderUser()
        {
            return CreateClaimsPrincipal(
                Guid.NewGuid(),
                ScopeClaim("some-other-scope"));
        }

        public static ClaimsPrincipal VerifiedByIdentityProviderUser()
        {
            return CreateClaimsPrincipal(
                Guid.NewGuid(),
                ScopeClaim(SecurityScopes.AccessAdminApiScope));
        }

        public static ClaimsPrincipal AuthenticatedUser(params Claim[] additionalClaims)
        {
            return CreateClaimsPrincipal(
                Guid.NewGuid(),
                [
                    SecurityClaim(ApplicationAccessGranted),
                    ScopeClaim(SecurityScopes.AccessAdminApiScope),
                    ..additionalClaims
                ]);
        }

        public static ClaimsPrincipal BauUser()
        {
            // Give the BAU User the "BAU User" role, plus every Claim from the SecurityClaimTypes enum.
            var claims =
                ListOf(RoleClaim(RoleNames.BauUser))
                .Concat(EnumUtil.GetEnums<SecurityClaimTypes>().Select(SecurityClaim))
                .Append(ScopeClaim(SecurityScopes.AccessAdminApiScope));

            return CreateClaimsPrincipal(
                Guid.NewGuid(),
                claims.ToArray());
        }

        public static ClaimsPrincipal AnalystUser()
        {
            return CreateClaimsPrincipal(
                Guid.NewGuid(),
                RoleClaim(Analyst),
                SecurityClaim(ApplicationAccessGranted),
                SecurityClaim(AnalystPagesAccessGranted),
                SecurityClaim(PrereleasePagesAccessGranted),
                SecurityClaim(CanViewPrereleaseContacts),
                ScopeClaim(SecurityScopes.AccessAdminApiScope));
        }

        public static ClaimsPrincipal PreReleaseUser()
        {
            return CreateClaimsPrincipal(
                Guid.NewGuid(),
                RoleClaim(PrereleaseUser),
                SecurityClaim(ApplicationAccessGranted),
                SecurityClaim(PrereleasePagesAccessGranted),
                ScopeClaim(SecurityScopes.AccessAdminApiScope));
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId)
        {
            return CreateClaimsPrincipal(userId, new Claim[] { });
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params Claim[] additionalClaims)
        {
            var identity = new ClaimsIdentity(
                claims: new List<Claim>(),
                authenticationType: JwtBearerDefaults.AuthenticationScheme,
                nameType: EesClaimTypes.Name,
                roleType: EesClaimTypes.Role);

            identity.AddClaim(new Claim(EesClaimTypes.LocalUserId, userId.ToString()));
            identity.AddClaims(additionalClaims);
            var user = new ClaimsPrincipal(identity);
            return user;
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params SecurityClaimTypes[] additionalClaims)
        {
            return CreateClaimsPrincipal(userId,
                additionalClaims.Select(c => new Claim(c.ToString(), "")).ToArray());
        }

        /// <summary>
        /// Create a Claim representing a SecurityClaimTypes enum value.
        /// </summary>
        public static Claim SecurityClaim(SecurityClaimTypes type)
        {
            return new Claim(type.ToString(), "");
        }

        /// <summary>
        /// Create a Claim representing a Global Role (i.e. an AspNetUserRoles assignment).
        /// </summary>
        public static Claim RoleClaim(string roleName)
        {
            return new Claim(EesClaimTypes.Role, roleName);
        }

        /// <summary>
        /// Create a Claim representing a scope that is requested when requesting an Access
        /// Token from the Identity Provider.
        /// </summary>
        private static Claim ScopeClaim(string scope)
        {
            return new Claim(EesClaimTypes.SupportedMsalScope, scope);
        }
    }
}
