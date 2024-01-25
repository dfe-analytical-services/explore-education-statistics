using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services;

internal class DataSetService : IDataSetService
{
    private readonly PublicDataDbContext _publicDataDbContext;

    public DataSetService(PublicDataDbContext publicDataDbContext)
    {
        _publicDataDbContext = publicDataDbContext;
    }

    public async Task<Either<ActionResult, PaginatedDataSetViewModel>> ListDataSets(
        int page,
        int pageSize,
        Guid publicationId)
    {
        throw new NotImplementedException();
    }
}
