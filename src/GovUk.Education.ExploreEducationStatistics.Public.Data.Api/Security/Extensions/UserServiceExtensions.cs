using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security.Extensions;

public static class UserServiceExtensions
{
    public static Task<Either<ActionResult, DataSet>> CheckCanViewDataSet(
        this IUserService userService,
        DataSet dataSet
    )
    {
        return userService.CheckPolicy(dataSet, PublicDataSecurityPolicies.CanViewDataSet);
    }

    public static Task<Either<ActionResult, DataSetVersion>> CheckCanQueryDataSetVersion(
        this IUserService userService,
        DataSetVersion dataSetVersion
    )
    {
        return userService.CheckPolicy(
            dataSetVersion,
            PublicDataSecurityPolicies.CanQueryDataSetVersion
        );
    }

    public static Task<Either<ActionResult, DataSetVersion>> CheckCanViewDataSetVersion(
        this IUserService userService,
        DataSetVersion dataSetVersion
    )
    {
        return userService.CheckPolicy(
            dataSetVersion,
            PublicDataSecurityPolicies.CanViewDataSetVersion
        );
    }
}
