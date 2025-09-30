using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Repositories.Public.Data.Interfaces;

public interface IMappingTypesRepository
{
    public Task<List<LocationMappingTypes>> GetLocationOptionMappingTypes(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken = default);

    public Task<List<FilterMappingTypes>> GetFilterOptionMappingTypes(
        Guid targetDataSetVersionId,
        CancellationToken cancellationToken = default);
}
