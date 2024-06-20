using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionMappingService(
    IDataSetMetaService dataSetMetaService,
    PublicDataDbContext publicDataDbContext)
    : IDataSetVersionMappingService
{
    public async Task<Either<ActionResult, Unit>> CreateMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var liveVersion = (await publicDataDbContext
            .DataSetVersions
            .Where(dsv => dsv.Id == nextDataSetVersionId)
            .Select(dsv => dsv.DataSet.LatestLiveVersion)
            .SingleAsync(cancellationToken))!;

        var nextVersion = await publicDataDbContext
            .DataSetVersions
            .SingleAsync(dsv => dsv.Id == nextDataSetVersionId, cancellationToken);

        var nextVersionMeta = await dataSetMetaService.ReadDataSetVersionMetaForMappings(
            dataSetVersionId: nextDataSetVersionId,
            cancellationToken);

        var sourceLocationMeta =
            await GetLocationMeta(liveVersion.Id, cancellationToken);

        var locationMappings = CreateLocationMappings(
            sourceLocationMeta,
            nextVersionMeta.Locations);

        var sourceFilterMeta =
            await GetFilterMeta(liveVersion.Id, cancellationToken);

        var filterMappings = CreateFilterMappings(
            sourceFilterMeta,
            nextVersionMeta.Filters);

        nextVersion.MetaSummary = nextVersionMeta.MetaSummary;

        publicDataDbContext.DataSetVersionMappings.Add(new DataSetVersionMapping
        {
            SourceDataSetVersionId = liveVersion.Id,
            TargetDataSetVersionId = nextDataSetVersionId,
            LocationMappingPlan = locationMappings,
            FilterMappingPlan = filterMappings
        });

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Instance;
    }

    private LocationMappingPlan CreateLocationMappings(
        List<LocationMeta> sourceLocationMeta,
        IDictionary<LocationMeta, List<LocationOptionMetaRow>> targetLocationMeta)
    {
        var locationMappings = sourceLocationMeta
            .OrderBy(level => level.Level)
            .Select(level => new LocationLevelMappingPlan
            {
                Level = level.Level,
                Mappings = level
                    .Options
                    .OrderBy(location => location.Label)
                    .Select(location => new LocationOptionMapping
                    {
                        Source = new LocationOption
                        {
                            Key = $"Source location :: {level.Level} :: {location.Label}",
                            Label = location.Label,
                        },
                        Type = MappingType.None
                    })
                    .ToList()
            })
            .ToList();

        var locationTargets = targetLocationMeta
            .Select(meta => (
                levelMeta: meta.Key,
                optionsMeta: meta.Value))
            .OrderBy(meta => meta.levelMeta.Level)
            .Select(meta => new LocationLevelMappingCandidates
            {
                Level = meta.levelMeta.Level,
                Candidates = meta
                    .optionsMeta
                    .OrderBy(location => location.Label)
                    .Select(location => new LocationOption
                    {
                        Key = $"Target location :: {meta.levelMeta.Level} :: {location.Label}",
                        Label = location.Label
                    })
                    .ToList()
            })
            .ToList();

        return new LocationMappingPlan
        {
            Mappings = locationMappings,
            Candidates = locationTargets
        };
    }

    private FilterMappingPlan CreateFilterMappings(
        List<FilterMeta> sourceFilterMeta,
        IDictionary<FilterMeta, List<FilterOptionMeta>> targetFilterMeta)
    {
        var filterMappings = sourceFilterMeta
            .Select(filter => new FilterMapping
            {
                Source = new Filter
                {
                    Key = $"Source filter :: {filter.Label}",
                    Label = filter.Label
                },
                Options = filter
                    .Options
                    .Select(option => new FilterOptionMapping
                    {
                        Source = new FilterOption
                        {
                            Key = $"Source filter option :: {filter.Label} :: {option.Label}",
                            Label = option.Label
                        }
                    })
                    .ToList()
            })
            .ToList();

        var filterTargets = targetFilterMeta
            .Select(meta => (
                filterMeta: meta.Key,
                optionsMeta: meta.Value))
            .Select(meta => new FilterMappingCandidate
            {
                Key = $"Target filter :: {meta.filterMeta.Label}",
                Label = meta.filterMeta.Label,
                Options = meta.optionsMeta
                    .Select(option => new FilterOption
                    {
                        Key = $"Target filter option :: {meta.filterMeta.Label} :: {option.Label}",
                        Label = option.Label
                    })
                    .ToList()
            })
            .ToList();

        var filters = new FilterMappingPlan
        {
            Mappings = filterMappings,
            Candidates = filterTargets
        };

        return filters;
    }

    private async Task<List<LocationMeta>> GetLocationMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .LocationMetas
            .AsNoTracking()
            .Include(levelMeta => levelMeta.Options)
            .Where(meta => meta.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);
    }

    private async Task<List<FilterMeta>> GetFilterMeta(Guid sourceVersionId, CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .FilterMetas
            .AsNoTracking()
            .Include(filterMeta => filterMeta.Options)
            .Where(meta => meta.DataSetVersionId == sourceVersionId)
            .ToListAsync(cancellationToken);
    }
}
