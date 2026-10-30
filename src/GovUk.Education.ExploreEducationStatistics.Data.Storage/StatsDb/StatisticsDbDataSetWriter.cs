#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Storage.StatsDb;

public class StatisticsDbDataSetWriter(
    Guid subjectId,
    StatisticsDbContext context,
    IDatabaseHelper databaseHelper,
    ImporterLocationService importerLocationService,
    IObservationBatchImporter observationBatchImporter
) : IStorageDataSetWriter
{
    public Guid SubjectId { get; } = subjectId;

    public Task WriteFiltersAndIndicators(
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        CancellationToken cancellationToken = default
    )
    {
        return databaseHelper.DoInTransaction(
            context,
            async ctxDelegate =>
            {
                await ctxDelegate.Filter.AddRangeAsync(filters, cancellationToken);
                await ctxDelegate.Indicator.AddRangeAsync(indicators, cancellationToken);
                await ctxDelegate.SaveChangesAsync(cancellationToken);
            }
        );
    }

    public Task WriteFilterItems(
        IReadOnlyList<FilterItem> filterItems,
        IReadOnlyDictionary<Guid, FilterItem?> autoSelectFilterItemsByFilterId,
        CancellationToken cancellationToken = default
    )
    {
        var filterIds = autoSelectFilterItemsByFilterId.Keys.ToList();

        return databaseHelper.DoInTransaction(
            context,
            async ctxDelegate =>
            {
                // We don't add filter groups explicitly, as they are included in `filterItems`
                await ctxDelegate.FilterItem.AddRangeAsync(filterItems, cancellationToken);

                var filters = ctxDelegate.Filter.Where(f => filterIds.Contains(f.Id)).ToList();
                foreach (var filter in filters)
                {
                    var filterItem = autoSelectFilterItemsByFilterId[filter.Id];
                    filter.AutoSelectFilterItemId = filterItem?.Id;
                    filter.AutoSelectFilterItemLabel = filterItem?.Label;
                }

                await ctxDelegate.SaveChangesAsync(cancellationToken);
            }
        );
    }

    public Task WriteLocations(IReadOnlyCollection<Location> locations, CancellationToken cancellationToken = default)
    {
        // In the StatsDB, Locations are shared between data sets - i.e. one location can belong to multiple data sets.
        //
        // Add any new Locations that are being introduced by this import process exclusively.  Other concurrent
        // import processes reaching this stage will wait before adding their own new Locations, if any.
        //
        // This ensures that we do not end up with duplicate Locations being added by separate processes, or let one
        // process continue on to the next stage of import before all Locations that it relies on have been
        // persisted successfully by whichever import process introduced it first.
        //
        // This also lets us take advantage of the performance gains of saving new Locations in a single batch
        // rather than in separate SaveChanges() calls, which introduces quite a penalty.
        return databaseHelper.ExecuteWithExclusiveLock(
            context,
            "Importer_AddNewLocations",
            ctxDelegate => importerLocationService.CreateIfNotExistsAndCache(ctxDelegate, locations.ToList())
        );
    }

    public Guid GetLocationId(Location location)
    {
        return importerLocationService.Get(location).Id;
    }

    public Task WriteObservations(
        IReadOnlyList<Observation> observations,
        CancellationToken cancellationToken = default
    )
    {
        return databaseHelper.DoInTransaction(
            context,
            ctxDelegate => observationBatchImporter.ImportObservationBatch(ctxDelegate, observations)
        );
    }

    public Task<long> CountObservations(CancellationToken cancellationToken = default)
    {
        return context.Observation.LongCountAsync(o => o.SubjectId == SubjectId, cancellationToken);
    }

    public async Task<List<(Guid ParentFilterItemId, Guid ChildFilterItemId)>> ListFilterItemRelationships(
        Guid parentFilterId,
        Guid childFilterId,
        CancellationToken cancellationToken = default
    )
    {
        var pairs = await context
            .FilterItem.AsNoTracking()
            .Where(fi => fi.FilterGroup.Filter.SubjectId == SubjectId)
            .Where(fi => fi.FilterGroup.FilterId == parentFilterId)
            .SelectMany(parentFilterItem =>
                context
                    .ObservationFilterItem.AsNoTracking()
                    .Where(childOfi =>
                        childOfi.FilterId == childFilterId
                        && context.ObservationFilterItem.Any(parentOfi =>
                            childOfi.ObservationId == parentOfi.ObservationId
                            && parentOfi.FilterItemId == parentFilterItem.Id
                        )
                    )
                    .Select(childOfi => new
                    {
                        ParentFilterItemId = parentFilterItem.Id,
                        ChildFilterItemId = childOfi.FilterItem.Id,
                    })
                    .ToList()
            )
            .Distinct()
            .ToListAsync(cancellationToken);

        return pairs.Select(pair => (pair.ParentFilterItemId, pair.ChildFilterItemId)).ToList();
    }

    public async Task<List<GeographicLevel>> ListGeographicLevels(CancellationToken cancellationToken = default)
    {
        return await context
            .Observation.AsNoTracking()
            .Where(o => o.SubjectId == SubjectId)
            .Select(observation => observation.Location.GeographicLevel)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
