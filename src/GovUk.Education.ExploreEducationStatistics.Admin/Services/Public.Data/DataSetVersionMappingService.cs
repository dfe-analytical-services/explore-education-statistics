using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public class DataSetVersionMappingService(
    IPostgreSqlRepository postgreSqlRepository,
    PublicDataDbContext publicDataDbContext)
    : IDataSetVersionMappingService
{
    public async Task<Either<ActionResult, BatchLocationMappingUpdatesResponseViewModel>> ApplyBatchMappingUpdates(
        Guid nextDataSetVersionId,
        BatchLocationMappingUpdatesRequest request,
        CancellationToken cancellationToken = default)
    {
        // checking for existence of things
        // permissions

        var updateResults = await request
            .Updates
            .ToAsyncEnumerable()
            .SelectAwait(async updateRequest =>
            {
                var updateJsonRequest = new JsonbPathRequest<Guid>
                {
                    TableName = nameof(PublicDataDbContext.DataSetVersionMappings),
                    IdColumnName = "TargetDataSetVersionId",
                    JsonbColumnName = nameof(DataSetVersionMapping.LocationMappingPlan),
                    RowId = nextDataSetVersionId,
                    PathSegments =
                    [
                        "Levels",
                        updateRequest.Level.ToString(),
                        "Mappings",
                        updateRequest.SourceKey
                    ]
                };

                var updatedMapping = await postgreSqlRepository.UpdateJsonbByPath(
                    publicDataDbContext,
                    updateJsonRequest,
                    (LocationOptionMapping mapping) =>
                    {
                        mapping.Type = updateRequest.Type;
                        mapping.CandidateKey = updateRequest.CandidateKey;
                    },
                    cancellationToken);

                return new LocationMappingUpdateResponse
                {
                    Level = updateRequest.Level,
                    SourceKey = updateRequest.SourceKey,
                    Mapping = updatedMapping
                };
            })
            .ToListAsync(cancellationToken);

        return new BatchLocationMappingUpdatesResponseViewModel { Updates = updateResults };
    }
}
