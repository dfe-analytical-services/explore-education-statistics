using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public static class AuthorizationHandlerUtil
    {
        private static readonly List<ReleaseRole> EditorRoles = new List<ReleaseRole>
        {
            ReleaseRole.Contributor,
            ReleaseRole.Approver,
            ReleaseRole.Lead
        };
        
        private static readonly List<ReleaseRole> ApproverRoles = new List<ReleaseRole>
        {
            ReleaseRole.Approver,
        };

        public static bool ContainsEditorRole(IEnumerable<ReleaseRole> roles)
        {
            return EditorRoles.Intersect(roles).Any();
        }

        public static bool ContainsApproverRole(IEnumerable<ReleaseRole> roles)
        {
            return ApproverRoles.Intersect(roles).Any();
        }
    }
}