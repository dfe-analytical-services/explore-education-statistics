using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class MakeAmendmentOfSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, Release>
    {
        public MakeAmendmentOfSpecificReleaseAuthorizationHandler(ContentDbContext context) : base(
            new CanMakeAmendmentOfAllReleasesAuthorizationHandler(context),
            new HasEditorRoleOnReleaseAuthorizationHandler(context))
        {
            
        }

        public class CanMakeAmendmentOfAllReleasesAuthorizationHandler :
            EntityAuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, Release>
        {
            public CanMakeAmendmentOfAllReleasesAuthorizationHandler(ContentDbContext context)
                : base(ctx =>
                    ctx.Entity.Live
                    && IsLatestVersionOfRelease(context, ctx.Entity)
                    && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.MakeAmendmentsOfAllReleases)) {}
        }

        public class HasEditorRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement>
        {
            public HasEditorRoleOnReleaseAuthorizationHandler(ContentDbContext context)
                : base(context, ctx =>
                    ctx.Release.Live
                    && IsLatestVersionOfRelease(context, ctx.Release)
                    && ContainsEditorRole(ctx.Roles))
            {}
        }

        private static bool IsLatestVersionOfRelease(ContentDbContext context, Release release)
        {
            var releases = context.Releases.AsNoTracking().Where(r => r.PublicationId == release.PublicationId);
            return IsLatestVersionOfRelease(releases, release.Id);
        }

        private static bool IsLatestVersionOfRelease(IEnumerable<Release> releases, Guid releaseId)
        {
            return !releases.Any(r => r.PreviousVersionId == releaseId && r.Id != releaseId);
        }
    }
}