#nullable enable
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetVersionRepository
{
    Task<DataSetVersion?> GetByReleaseFileId(
        Guid releaseFileId,
        CancellationToken cancellationToken = default);
}
