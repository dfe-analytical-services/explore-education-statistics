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
    StatisticsDbDataSet dataSet,
    StatisticsDbContext context,
    IDatabaseHelper databaseHelper,
    ImporterLocationService importerLocationService,
    IObservationBatchImporter observationBatchImporter
) : IStorageDataSetWriter
{
    public Guid SubjectId => dataSet.SubjectId;

    public IStorageDataSet DataSet => dataSet;

    public Task<bool> HasIndicators(CancellationToken cancellationToken = default)
    {
        return context.IndicatorGroup.AnyAsync(group => group.SubjectId == SubjectId, cancellationToken);
    }

    public Task WriteIndicators(IReadOnlyList<Indicator> indicators, CancellationToken cancellationToken = default)
    {
        return databaseHelper.DoInTransaction(
            context,
            async ctxDelegate =>
            {
                // We don't add indicator groups explicitly, as they are included in `indicators`
                await ctxDelegate.Indicator.AddRangeAsync(indicators, cancellationToken);
                await ctxDelegate.SaveChangesAsync(cancellationToken);
            }
        );
    }

    public Task<bool> HasFilters(CancellationToken cancellationToken = default)
    {
        return context.Filter.AnyAsync(filter => filter.SubjectId == SubjectId, cancellationToken);
    }

    public Task WriteFilters(IReadOnlyList<Filter> filters, CancellationToken cancellationToken = default)
    {
        return databaseHelper.DoInTransaction(
            context,
            async ctxDelegate =>
            {
                // We don't add filter groups and items explicitly, as they are included in `filters`
                await ctxDelegate.Filter.AddRangeAsync(filters, cancellationToken);
                await ctxDelegate.SaveChangesAsync(cancellationToken);
            }
        );
    }

    public Task WriteLocations(IReadOnlyCollection<Location> locations, CancellationToken cancellationToken = default)
    {
        // In the StatsDB, Locations are shared between data sets - i.e. one location can belong to multiple data sets.
        // So we add new Locations from this import exclusively - other concurrent import processes reaching this
        // stage will wait before adding their own new Locations.
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

    public Task StartObservations(
        IReadOnlyList<Filter> filters,
        IReadOnlyList<Indicator> indicators,
        long? resumeFromCsvRow,
        CancellationToken cancellationToken = default
    )
    {
        return Task.CompletedTask;
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

    public Task Complete(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task CleanUp(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
