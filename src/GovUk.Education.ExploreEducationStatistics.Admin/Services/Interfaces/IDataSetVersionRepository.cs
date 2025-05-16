using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Semver;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataSetVersionRepository
{
    Task<List<DataSetVersion>> GetDataSetVersions(Guid releaseVersionId);
    
    Task<DataSetVersion> GetDataSetVersion(Guid dataSetVersionId);

    Task<DataSetVersion> GetDataSetVersion(Guid dataSetId, SemVersion version, CancellationToken cancellationToken = default);
}
