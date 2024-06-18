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
            Locations = locationMappings,
            Filters = filterMappings
        });

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Instance;
    }

    private Locations CreateLocationMappings(
        List<LocationMeta> sourceLocationMeta,
        List<(LocationMeta, List<LocationOptionMetaRow>)> targetLocationMeta)
    {
        var locationMappings = sourceLocationMeta
            .OrderBy(level => level.Level)
            .Select(level => new LocationLevelMappings
            {
                Level = level.Level,
                Mappings = level
                    .Options
                    .OrderBy(location => location.Label)
                    .Select(location => new LocationOptionMapping
                    {
                        Source = new LocationOption
                        {
                            Key = $"Source level {level.Level} location {location.Label}",
                            Label = location.Label,
                        },
                        Type = MappingType.None
                    })
                    .ToList()
            })
            .ToList();

        var locationTargets = targetLocationMeta
            .Select(meta => (
                levelMeta: meta.Item1,
                optionsMeta: meta.Item2))
            .OrderBy(meta => meta.levelMeta.Level)
            .Select(meta => new LocationTargets
            {
                Level = meta.levelMeta.Level,
                Options = meta
                    .optionsMeta
                    .OrderBy(location => location.Label)
                    .Select(location => new LocationOption
                    {
                        Key = $"Target level {meta.levelMeta.Level} location {location.Label}",
                        Label = location.Label
                    })
                    .ToList()
            })
            .ToList();

        return new Locations {Mappings = locationMappings, Targets = locationTargets};
    }

    private Filters CreateFilterMappings(
        List<FilterMeta> sourceFilterMeta,
        List<(FilterMeta, List<FilterOptionMeta>)> targetFilterMeta)
    {
        var filterMappings = sourceFilterMeta
            .Select(filter => new FilterMapping
            {
                Source = new Filter {Key = $"Source filter {filter.Label}", Label = filter.Label},
                Options = filter
                    .Options
                    .Select(option => new FilterOptionMapping
                    {
                        Source = new FilterOption
                        {
                            Key = $"Source filter {filter.Label} option {option.Label}", Label = option.Label
                        }
                    })
                    .ToList()
            })
            .ToList();

        var filterTargets = targetFilterMeta
            .Select(meta => (
                filterMeta: meta.Item1,
                optionsMeta: meta.Item2))
            .Select(meta => new FilterTarget
            {
                Key = $"Target filter {meta.filterMeta.Label}",
                Label = meta.filterMeta.Label,
                Options = meta.optionsMeta
                    .Select(option => new FilterOption
                    {
                        Key = $"Target filter {meta.filterMeta.Label} option {option.Label}", Label = option.Label
                    })
                    .ToList()
            })
            .ToList();

        var filters = new Filters {Mappings = filterMappings, Targets = filterTargets};

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
