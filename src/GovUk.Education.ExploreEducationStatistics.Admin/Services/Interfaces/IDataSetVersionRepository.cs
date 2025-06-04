using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetVersionRepository
{
    Task<List<DataSetVersion>> GetDataSetVersions(Guid releaseVersionId);
}
