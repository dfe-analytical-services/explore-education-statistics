using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetVersionService
{
    Task<Either<ActionResult, List<DataSetVersionStatusViewModel>>> ListStatusesForReleaseVersion(
        Guid releaseVersionId);
}
