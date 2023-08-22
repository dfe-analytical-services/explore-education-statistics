using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using IdentityModel;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles.RoleNames;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils
{
    public static class ClaimsPrincipalUtils
    {
        public static ClaimsPrincipal AuthenticatedUser()
        {
            return CreateClaimsPrincipal(
                Guid.NewGuid(), 
                SecurityClaim(ApplicationAccessGranted));
        }
        
        public static ClaimsPrincipal BauUser()
        {
            // Give the BAU User the "BAU User" role, plus every Claim from the SecurityClaimTypes enum.
            var claims = 
                ListOf(RoleClaim(RoleNames.BauUser))
                .Concat(EnumUtil.GetEnumValues<SecurityClaimTypes>().Select(SecurityClaim));
                
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
                    SecurityClaim(CanViewPrereleaseContacts));
        }
        
        public static ClaimsPrincipal PreReleaseUser()
        {
            return CreateClaimsPrincipal(
                Guid.NewGuid(),
                RoleClaim(PrereleaseUser),
                SecurityClaim(ApplicationAccessGranted),
                SecurityClaim(PrereleasePagesAccessGranted));

        }
        
        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId)
        {
            return CreateClaimsPrincipal(userId, new Claim[] { });
        }

        public static ClaimsPrincipal CreateClaimsPrincipal(Guid userId, params Claim[] additionalClaims)
        {
            var identity = new ClaimsIdentity(
                new List<Claim>(), 
                "TestAuthenticationType", 
                JwtClaimTypes.Name, 
                JwtClaimTypes.Role);
            
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
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
        private static Claim SecurityClaim(SecurityClaimTypes type)
        {
            return new Claim(type.ToString(), "");
        }
        
        /// <summary>
        /// Create a Claim representing a Global Role (i.e. an AspNetUserRoles assignment).
        /// </summary>
        private static Claim RoleClaim(string roleName)
        {
            return new Claim(JwtClaimTypes.Role, roleName);
        }
    }
}