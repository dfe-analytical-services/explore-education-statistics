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
    public async Task<Either<ActionResult, BatchMappingUpdatesResponseViewModel>> ApplyBatchMappingUpdates(
        Guid nextDataSetVersionId,
        BatchLocationMappingUpdatesRequest request,
        CancellationToken cancellationToken = default)
    {
        // checking for existence of things
        // permissions

        var updateResults = await request
            .Updates
            .ToAsyncEnumerable()
            .ToDictionaryAwaitAsync(
                keySelector: update => ValueTask.FromResult(update.SourceKey),
                elementSelector: async update =>
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
                            update.Level.ToString(),
                            "Mappings",
                            update.SourceKey
                        ]
                    };
                    
                    return await postgreSqlRepository.UpdateJsonbByPath(
                        publicDataDbContext,
                        updateJsonRequest,
                        (LocationOptionMapping mapping) =>
                        {
                            mapping.Type = update.Type;
                            mapping.CandidateKey = update.CandidateKey;
                        },
                        cancellationToken);
                },
                cancellationToken);

        return new BatchMappingUpdatesResponseViewModel { Results = updateResults };
    }
}
