using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
{
    public class SubmitSpecificReleaseToHigherReviewRequirement : IAuthorizationRequirement
    {}
    
    public class SubmitSpecificReleaseToHigherReviewCanSubmitAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
        SubmitSpecificReleaseToHigherReviewRequirement>
    {
        public SubmitSpecificReleaseToHigherReviewCanSubmitAllReleasesAuthorizationHandler() 
            : base(SecurityClaimTypes.SubmitAllReleasesToHigherReview) {}
    }

    public class SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<SubmitSpecificReleaseToHigherReviewRequirement>
    {
        public SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler(ContentDbContext context) : base(context)
        {}
    }
}