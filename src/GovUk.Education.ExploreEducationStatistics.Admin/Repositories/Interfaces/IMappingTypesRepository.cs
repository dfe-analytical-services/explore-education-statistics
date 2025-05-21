using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Interfaces;

public interface IMappingTypesRepository
{
    public Task<List<LocationMappingTypes>> GetLocationOptionMappingTypes(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken = default);

    public Task<List<FilterMappingTypes>> GetFilterOptionMappingTypes(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken = default);

    public Task<bool> HasDeletionMajorVersionChangesAsync(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken = default);
}
