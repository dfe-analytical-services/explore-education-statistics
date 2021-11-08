#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IReleasePermissionService
    {
        Task<Either<ActionResult, List<ManageAccessPageContributorViewModel>>> GetManageAccessPageContributorList(Guid releaseId);
    }
}
