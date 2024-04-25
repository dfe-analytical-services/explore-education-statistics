using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetVersionService
{
    Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(Guid releaseVersionId);
}
