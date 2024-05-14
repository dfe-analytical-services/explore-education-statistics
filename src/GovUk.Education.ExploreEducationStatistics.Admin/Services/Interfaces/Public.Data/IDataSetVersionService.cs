#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetVersionService
{
    Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(Guid releaseVersionId);

    Task<bool> FileHasVersion(
        Guid releaseFileId,
        CancellationToken cancellationToken = default);
}
