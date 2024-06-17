using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionMappingService(
    PublicDataDbContext publicDataDbContext)
    : IDataSetVersionMappingService
{
    public async Task<Either<ActionResult, Unit>> CreateMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var liveVersionId = await publicDataDbContext
            .DataSetVersions
            .Where(dsv => dsv.Id == nextDataSetVersionId)
            .Select(dsv => dsv.DataSet.LatestLiveVersionId!.Value)
            .SingleAsync(cancellationToken);

        var mappings = await BuildMappings(
            liveVersionId,
            nextDataSetVersionId,
            cancellationToken);

        publicDataDbContext.DataSetVersionMappings.Add(mappings);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
        return Unit.Instance;
    }

    private async Task<DataSetVersionMapping> BuildMappings(
        Guid sourceVersionId,
        Guid targetVersionId,
        CancellationToken cancellationToken)
    {
        var locationMappings = await CreateLocationMappings(
            sourceVersionId,
            targetVersionId,
            cancellationToken);

        var filterMappings = await CreateFilterMappings(
            sourceVersionId,
            targetVersionId,
            cancellationToken);

        return new DataSetVersionMapping
        {
            SourceDataSetVersionId = sourceVersionId,
            TargetDataSetVersionId = targetVersionId,
            Locations = locationMappings,
            Filters = filterMappings
        };
    }

    private async Task<Locations> CreateLocationMappings(
        Guid sourceVersionId,
        Guid targetVersionId,
        CancellationToken cancellationToken)
    {
        var sourceLocationMeta =
            await GetLocationMeta(cancellationToken, sourceVersionId);

        var targetLocationMeta =
            await GetLocationMeta(cancellationToken, targetVersionId);

        var locationMappings = sourceLocationMeta
            .Select(level => new LocationMappings
            {
                Level    = level.Level,
                Mappings = level
                    .Options
                    .Select(location => new LocationMapping
                    {
                        Source = new LocationOption
                        {
                            Id = location.Id,
                            Label = location.Label,
                        },
                        Type = MappingType.None
                    })
                    .ToList()
            })
            .ToList();

        var locationTargets = targetLocationMeta
            .Select(level =>
            {
                return new LocationTargets
                {
                    Level = level.Level,
                    Options = level
                        .Options
                        .Select(location => new LocationOption { Id = location.Id, Label = location.Label })
                        .ToList()
                };
            })
            .ToList();
        
        return new Locations
        {
            Mappings = locationMappings,
            Targets = locationTargets
        };
    }

    private async Task<Filters> CreateFilterMappings(
        Guid sourceVersionId,
        Guid targetVersionId,
        CancellationToken cancellationToken)
    {
        var sourceFilterMeta = await GetFilterMeta(sourceVersionId, cancellationToken);
        var targetFilterMeta = await GetFilterMeta(targetVersionId, cancellationToken);

        var filterMappings = sourceFilterMeta
            .Select(filter => new FilterMapping
            {
                Source = new Filter {Id = filter.Id, Label = filter.Label},
                Options = filter
                    .Options
                    .Select(option => new FilterOptionMapping
                    {
                        Source = new FilterOption {Id = option.Id, Label = option.Label}
                    })
                    .ToList()
            })
            .ToList();

        var filterTargets = targetFilterMeta
            .Select(filter => new FilterTarget
            {
                Id = filter.Id,
                Label = filter.Label,
                Options = filter
                    .Options
                    .Select(option => new FilterOption {Id = option.Id, Label = option.Label})
                    .ToList()
            })
            .ToList();
        
        var filters = new Filters
        {
            Mappings = filterMappings,
            Targets = filterTargets
        };

        return filters;
    }

    private async Task<List<LocationMeta>> GetLocationMeta(
        CancellationToken cancellationToken,
        Guid dataSetVersionId) =>
        await publicDataDbContext
            .LocationMetas
            .AsNoTracking()
            .Where(meta => meta.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);

    private async Task<List<FilterMeta>> GetFilterMeta(Guid sourceVersionId, CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .FilterMetas
            .AsNoTracking()
            .Where(meta => meta.DataSetVersionId == sourceVersionId)
            .ToListAsync(cancellationToken);
    }
}
