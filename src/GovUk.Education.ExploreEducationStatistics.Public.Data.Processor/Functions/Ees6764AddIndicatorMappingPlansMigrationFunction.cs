using GovUk.Education.ExploreEducationStatistics.Common.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.DuckDb;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Services.Interfaces;
using InterpolatedSql.Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Functions;

// TODO EES-6993 - remove this Function when migrations have run against all environments.
public class Ees6764AddIndicatorMappingPlansMigrationFunction(
    PublicDataDbContext publicDataDbContext,
    IIndicatorMetaRepository indicatorRepository,
    IDataSetVersionPathResolver dataSetVersionPathResolver,
    ILogger<Ees6764AddIndicatorMappingPlansMigrationFunction> logger
)
{
    private const int BatchSize = 50;

    [Function(nameof(AddIndicatorMappingsToExistingDataSetVersionMappings))]
    [Produces("application/json")]
    public async Task<IActionResult> AddIndicatorMappingsToExistingDataSetVersionMappings(
#pragma warning disable IDE0060
        [HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequest request,
        bool dryRun = true
    )
#pragma warning restore IDE0060
    {
        var existingMappings = await publicDataDbContext
            .DataSetVersionMappings.Where(m => m.IndicatorMappingPlan == null)
            .Take(BatchSize)
            .ToListAsync();

        var updatedMappings = new List<IndicatorMappingPlanUpdate>();

        await foreach (var mapping in existingMappings.ToAsyncEnumerable())
        {
            updatedMappings.Add(await AddIndicatorMappingToDataSetVersionMapping(mapping, dryRun));
        }

        return new OkObjectResult(updatedMappings);
    }

    private async Task<IndicatorMappingPlanUpdate> AddIndicatorMappingToDataSetVersionMapping(
        DataSetVersionMapping mapping,
        bool dryRun
    )
    {
        try
        {
            var indicatorMappingPlan = new IndicatorMappingPlan();

            var sourceDataSetVersionIndicators = await publicDataDbContext
                .IndicatorMetas.Where(indicator => indicator.DataSetVersionId == mapping.SourceDataSetVersionId)
                .ToListAsync();

            var targetDataSetVersion = await publicDataDbContext.DataSetVersions.SingleAsync(dsv =>
                dsv.Id == mapping.TargetDataSetVersionId
            );

            await using var duckDbConnection = DuckDbConnection.CreateFileConnection(
                dataSetVersionPathResolver.DuckDbPath(targetDataSetVersion)
            );
            duckDbConnection.Open();

            var targetDataSetVersionIndicators = await indicatorRepository.ReadIndicatorMetas(
                duckDbConnection: duckDbConnection,
                dataSetVersion: targetDataSetVersion,
                allowedColumns: await GetAllowedColumns(
                    duckDbConnection: duckDbConnection,
                    dataSetVersion: targetDataSetVersion
                )
            );

            DataSetVersionStatus[] postMappingStatuses =
            [
                DataSetVersionStatus.Finalising,
                DataSetVersionStatus.Published,
                DataSetVersionStatus.Withdrawn,
            ];

            var mappingsAlreadyComplete = postMappingStatuses.Contains(targetDataSetVersion.Status);

            // If mappings have already been completed, we will simulate that the user has correctly confirmed manually
            // that all non-auto-mappable Indicators are indeed not mappable by using "ManualNone". Otherwise, if the user
            // is still able to work on the Indicator mappings, mark them as having been automatically unmapped with
            // AutoNone and allow the user to either confirm this or find an appropriate mapping.
            var mappingTypeForNonAutoMappedIndicators = mappingsAlreadyComplete
                ? MappingType.ManualNone
                : MappingType.AutoNone;

            sourceDataSetVersionIndicators.ForEach(sourceIndicator =>
            {
                var matchingTargetIndicator = targetDataSetVersionIndicators.SingleOrDefault(targetIndicator =>
                    targetIndicator.Column == sourceIndicator.Column
                );

                indicatorMappingPlan.Mappings[sourceIndicator.Column] = new IndicatorMapping
                {
                    Source = new MappableIndicator { Label = sourceIndicator.Label },
                    PublicId = sourceIndicator.PublicId,
                    Type =
                        matchingTargetIndicator != null
                            ? MappingType.AutoMapped
                            : mappingTypeForNonAutoMappedIndicators,
                    CandidateKey =
                        matchingTargetIndicator != null
                            ? MappingKeyGenerators.IndicatorMeta(matchingTargetIndicator)
                            : null,
                };
            });

            targetDataSetVersionIndicators.ForEach(targetIndicator =>
            {
                indicatorMappingPlan.Candidates[MappingKeyGenerators.IndicatorMeta(targetIndicator)] =
                    new MappableIndicator { Label = targetIndicator.Label };
            });

            MappingType[] completedIndividualMappingTypes =
            [
                MappingType.AutoMapped,
                MappingType.ManualNone,
                MappingType.ManualMapped,
            ];

            var indicatorMappingsComplete = indicatorMappingPlan.Mappings.Values.All(indicatorMapping =>
                completedIndividualMappingTypes.Contains(indicatorMapping.Type)
            );

            var plan = new IndicatorMappingPlanUpdate(
                SourceDataSetVersionId: mapping.SourceDataSetVersionId,
                TargetDataSetVersionId: mapping.TargetDataSetVersionId,
                SourceIndicators: sourceDataSetVersionIndicators
                    .Select(i => new Indicator(Column: i.Column, Label: i.Label))
                    .OrderBy(i => i.Column)
                    .ToList(),
                TargetIndicators: targetDataSetVersionIndicators
                    .Select(i => new Indicator(Column: i.Column, Label: i.Label))
                    .OrderBy(i => i.Column)
                    .ToList(),
                IndicatorMappingPlan: indicatorMappingPlan,
                IndicatorMappingsComplete: indicatorMappingsComplete,
                ErrorMessage: null
            );

            if (!dryRun)
            {
                try
                {
                    mapping.IndicatorMappingsComplete = indicatorMappingsComplete;
                    mapping.IndicatorMappingPlan = indicatorMappingPlan;

                    publicDataDbContext.Update(mapping);
                    await publicDataDbContext.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    logger.LogError(
                        e,
                        "Error saving IndicatorMappingPlan for SourceDataSetVersionId {SourceDataSetVersionId} "
                            + "and TargetDataSetVersionId {TargetDataSetVersionId}",
                        mapping.SourceDataSetVersionId,
                        mapping.TargetDataSetVersionId
                    );

                    return plan with
                    {
                        ErrorMessage = e.Message,
                    };
                }
            }

            return plan;
        }
        catch (Exception e)
        {
            logger.LogError(
                e,
                "Error creating IndicatorMappingPlan for SourceDataSetVersionId {SourceDataSetVersionId} "
                    + "and TargetDataSetVersionId {TargetDataSetVersionId}",
                mapping.SourceDataSetVersionId,
                mapping.TargetDataSetVersionId
            );

            return new IndicatorMappingPlanUpdate(
                SourceDataSetVersionId: mapping.SourceDataSetVersionId,
                TargetDataSetVersionId: mapping.TargetDataSetVersionId,
                SourceIndicators: [],
                TargetIndicators: [],
                IndicatorMappingPlan: new IndicatorMappingPlan(),
                IndicatorMappingsComplete: false,
                ErrorMessage: e.Message
            );
        }
    }

    private async Task<HashSet<string>> GetAllowedColumns(
        DuckDbConnection duckDbConnection,
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default
    )
    {
        var columns = (
            await duckDbConnection
                .SqlBuilder(
                    $"""
                    DESCRIBE SELECT *
                    FROM read_csv(
                        '{dataSetVersionPathResolver.CsvDataPath(dataSetVersion):raw}',
                        {DuckDbConstants.ReadCsvOptions:raw}
                    )
                    """
                )
                .QueryAsync<(string ColumnName, string ColumnType)>(cancellationToken: cancellationToken)
        ).Select(row => row.ColumnName).ToList();

        return [.. columns];
    }
}

// ReSharper disable NotAccessedPositionalProperty.Global
public record IndicatorMappingPlanUpdate(
    Guid SourceDataSetVersionId,
    Guid TargetDataSetVersionId,
    List<Indicator> SourceIndicators,
    List<Indicator> TargetIndicators,
    IndicatorMappingPlan IndicatorMappingPlan,
    bool IndicatorMappingsComplete,
    string? ErrorMessage
);

public record Indicator(string Column, string Label);
