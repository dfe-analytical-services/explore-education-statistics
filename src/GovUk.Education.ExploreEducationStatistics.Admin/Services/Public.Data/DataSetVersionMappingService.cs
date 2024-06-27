using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
                    // collapse into single command
                    string[] baseJsonPathSegments =
                    [
                        "Levels",
                        update.Level.ToString(),
                        "Mappings",
                        update.SourceKey
                    ];

                    await postgreSqlRepository.UpdateJsonbByPath(
                        context: publicDataDbContext,
                        tableName: nameof(PublicDataDbContext.DataSetVersionMappings),
                        idColumnName: "TargetDataSetVersionId",
                        jsonColumnName: nameof(DataSetVersionMapping.LocationMappingPlan),
                        rowId: nextDataSetVersionId,
                        [..baseJsonPathSegments, nameof(Mapping<MappableLocationOption>.Type)],
                        update.Type.ToString(),
                        cancellationToken);

                    await postgreSqlRepository.UpdateJsonbByPath(
                        context: publicDataDbContext,
                        tableName: nameof(PublicDataDbContext.DataSetVersionMappings),
                        idColumnName: "TargetDataSetVersionId",
                        jsonColumnName: nameof(DataSetVersionMapping.LocationMappingPlan),
                        rowId: nextDataSetVersionId,
                        [..baseJsonPathSegments, nameof(Mapping<MappableLocationOption>.CandidateKey)],
                        update.CandidateKey,
                        cancellationToken);
                    
                    var a = await postgreSqlRepository.GetJsonbFromPath<PublicDataDbContext, Guid, LocationOptionMapping>(
                        context: publicDataDbContext,
                        tableName: nameof(PublicDataDbContext.DataSetVersionMappings),
                        idColumnName: "TargetDataSetVersionId",
                        jsonColumnName: nameof(DataSetVersionMapping.LocationMappingPlan),
                        rowId: nextDataSetVersionId,
                        baseJsonPathSegments,
                        cancellationToken);

                    // Should this actually return the JSON fragment at the given JSON path?
                    return new LocationMappingUpdateResult
                    {
                        CandidateKey = update.CandidateKey,
                        Type = update.Type
                    };
                },
                cancellationToken);

        return new BatchMappingUpdatesResponseViewModel { Results = updateResults };
    }
}
