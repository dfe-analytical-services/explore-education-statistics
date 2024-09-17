using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionChangeService(PublicDataDbContext publicDataDbContext)
    : IDataSetVersionChangeService
{
    public async Task CreateChanges(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var nextVersion = await publicDataDbContext
            .DataSetVersions
            .Include(dsv => dsv.DataSet)
            .SingleAsync(dsv => dsv.Id == nextDataSetVersionId, cancellationToken);

        var previousVersionId = nextVersion.DataSet.LatestLiveVersionId!.Value;

        await publicDataDbContext.RequireTransaction(async () =>
        {
            await CreateFilterChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);

            await CreateLocationChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);

            await CreateGeographicLevelChange(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);

            await CreateIndicatorChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);

            await CreateTimePeriodChanges(
                previousVersionId: previousVersionId,
                nextVersionId: nextVersion.Id,
                cancellationToken: cancellationToken);
        });
    }

    private async Task CreateFilterChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldFilterMetas = await GetFilterMetas(previousVersionId, cancellationToken);
        var newFilterMetas = await GetFilterMetas(nextVersionId, cancellationToken);

        var mappingPlan = await publicDataDbContext
            .DataSetVersionMappings
            .AsNoTracking()
            .Where(dsvm => dsvm.TargetDataSetVersionId == nextVersionId)
            .Select(dsvm => dsvm.FilterMappingPlan)
            .SingleAsync(cancellationToken);

        var filterMetaDeletionsAndChanges = mappingPlan.Mappings
            // Do not include changes for keys that have been mapped to the exact same candidate key (no renaming)    
            .Where(kv => kv.Key != kv.Value.CandidateKey)
            .Select(kv =>
            {
                var source = oldFilterMetas[kv.Key].FilterMeta;
                var target = string.IsNullOrEmpty(kv.Value.CandidateKey)
                    ? null
                    : newFilterMetas[kv.Value.CandidateKey].FilterMeta;

                return new FilterMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousStateId = source.Id,
                    CurrentStateId = target?.Id
                };
            })
            .ToList();

        var filterMetaAdditions = mappingPlan.Candidates
            .Keys
            .Except(mappingPlan.Mappings.Select(m => m.Value.CandidateKey!))
            .Select(newCandidateKey =>
            {
                var target = newFilterMetas[newCandidateKey].FilterMeta;
                return new FilterMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousStateId = null,
                    CurrentStateId = target.Id
                };
            })
            .ToList();

        var filterOptionMetaDeletionsAndChanges = mappingPlan.Mappings
            // Don't create a change for any filter options which have had their entire filter deleted
            .Where(kv => !string.IsNullOrEmpty(kv.Value.CandidateKey))
            .SelectMany(
                fm => fm.Value.OptionMappings,
                (fm, fom) => new
                {
                    FilterMapping = fm,
                    OptionMapping = fom
                })
            // Do not include changes for keys that have been mapped to the exact same candidate key (no renaming)
            .Where(a => a.OptionMapping.Key != a.OptionMapping.Value.CandidateKey)
            .Select(a =>
            {
                var source = oldFilterMetas[a.FilterMapping.Key].OptionLinks[a.OptionMapping.Key];
                var target = string.IsNullOrEmpty(a.OptionMapping.Value.CandidateKey)
                    ? null
                    : newFilterMetas[a.FilterMapping.Value.CandidateKey!]
                        .OptionLinks[a.OptionMapping.Value.CandidateKey];
                return new FilterOptionMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousState = FilterOptionMetaChange.State.Create(source),
                    CurrentState = target != null ? FilterOptionMetaChange.State.Create(target) : null
                };
            })
            .ToList();

        var filterOptionMetaAdditions = mappingPlan.Candidates
            .SelectMany(
                fc => fc.Value.Options,
                (fc, foc) => new
                {
                    FilterCandidateKey = (string?)fc.Key,
                    OptionCandidateKey = (string?)foc.Key
                })
            .Except(
                mappingPlan.Mappings
                    .SelectMany(
                        fm => fm.Value.OptionMappings,
                        (fm, fom) => new
                        {
                            FilterCandidateKey = fm.Value.CandidateKey,
                            OptionCandidateKey = fom.Value.CandidateKey
                        }
                    ))
            .Select(a =>
            {
                var target = newFilterMetas[a.FilterCandidateKey!].OptionLinks[a.OptionCandidateKey!];
                return new FilterOptionMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousState = null,
                    CurrentState = FilterOptionMetaChange.State.Create(target)
                };
            });

        publicDataDbContext.FilterMetaChanges.AddRange([.. filterMetaDeletionsAndChanges, .. filterMetaAdditions]);
        publicDataDbContext.FilterOptionMetaChanges.AddRange([
            .. filterOptionMetaDeletionsAndChanges, .. filterOptionMetaAdditions
        ]);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateLocationChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldLocationMetas = await GetLocationMetas(previousVersionId, cancellationToken);
        var newLocationMetas = await GetLocationMetas(nextVersionId, cancellationToken);

        var mappingPlan = await publicDataDbContext
            .DataSetVersionMappings
            .AsNoTracking()
            .Where(dsvm => dsvm.TargetDataSetVersionId == nextVersionId)
            .Select(dsvm => dsvm.LocationMappingPlan)
            .SingleAsync(cancellationToken);

        var locationMetaChanges = mappingPlan.Levels
            // Only include adds and deletes for location levels
            .Where(locationGroupMappings => locationGroupMappings.Value.Mappings.Count == 0 ||
                                            locationGroupMappings.Value.Candidates.Count == 0)
            .Select(locationGroupMappings =>
            {
                var source = locationGroupMappings.Value.Mappings.Count == 0
                    ? null
                    : oldLocationMetas[locationGroupMappings.Key].LocationMeta;
                var target = locationGroupMappings.Value.Candidates.Count == 0
                    ? null
                    : newLocationMetas[locationGroupMappings.Key].LocationMeta;
                return new LocationMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousStateId = source?.Id,
                    CurrentStateId = target?.Id
                };
            })
            .ToList();

        var locationOptionMetaDeletionsAndChanges = mappingPlan.Levels
            // Don't create a change for any location options which have had their entire location group deleted
            .Where(locationGroupMappings => locationGroupMappings.Value.Candidates.Count > 0)
            .SelectMany(
                lm => lm.Value.Mappings,
                (lm, lom) => new
                {
                    LocationGroup = lm.Key,
                    OptionMapping = lom
                })
            // Do not include changes for keys that have been mapped to the exact same candidate key (no renaming)
            .Where(a => a.OptionMapping.Key != a.OptionMapping.Value.CandidateKey)
            .Select(a =>
            {
                var level = a.LocationGroup;
                var source = oldLocationMetas[level].OptionLinks[a.OptionMapping.Key];
                var target = string.IsNullOrEmpty(a.OptionMapping.Value.CandidateKey)
                    ? null
                    : newLocationMetas[level].OptionLinks[a.OptionMapping.Value.CandidateKey];
                return new LocationOptionMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousState = LocationOptionMetaChange.State.Create(source),
                    CurrentState = target != null ? LocationOptionMetaChange.State.Create(target) : null
                };
            })
            .ToList();

        var locationOptionMetaAdditions = mappingPlan.Levels
            .SelectMany(
                lm => lm.Value.Candidates,
                (lm, loc) => new
                {
                    LocationGroup = lm.Key,
                    OptionCandidateKey = (string?)loc.Key
                })
            .Except(
                mappingPlan.Levels
                    .SelectMany(
                        lm => lm.Value.Mappings,
                        (lm, lom) => new
                        {
                            LocationGroup = lm.Key,
                            OptionCandidateKey = lom.Value.CandidateKey
                        }
                    ))
            .Select(a =>
            {
                var target = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionCandidateKey!];
                return new LocationOptionMetaChange
                {
                    DataSetVersionId = nextVersionId,
                    PreviousState = null,
                    CurrentState = LocationOptionMetaChange.State.Create(target)
                };
            });

        publicDataDbContext.LocationMetaChanges.AddRange(locationMetaChanges);
        publicDataDbContext.LocationOptionMetaChanges.AddRange([
            .. locationOptionMetaDeletionsAndChanges, .. locationOptionMetaAdditions
        ]);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateGeographicLevelChange(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldGeographicLevelMeta = await GetGeographicLevelMeta(previousVersionId, cancellationToken);
        var newGeographicLevelMeta = await GetGeographicLevelMeta(nextVersionId, cancellationToken);

        var levelsAreEqual =
            newGeographicLevelMeta.Levels.Order().SequenceEqual(oldGeographicLevelMeta.Levels.Order());

        if (!levelsAreEqual)
        {
            publicDataDbContext.GeographicLevelMetaChanges.Add(new GeographicLevelMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = oldGeographicLevelMeta.Id,
                CurrentStateId = newGeographicLevelMeta.Id
            });
            await publicDataDbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task CreateIndicatorChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldIndicatorMetas = await GetIndicatorMetas(previousVersionId, cancellationToken);
        var newIndicatorMetas = await GetIndicatorMetas(nextVersionId, cancellationToken);

        var indicatorMetaDeletions = oldIndicatorMetas
            .Except(newIndicatorMetas, IndicatorMeta.PublicIdComparer)
            .OrderBy(indicatorMeta => indicatorMeta.Label)
            .Select(indicatorMeta => new IndicatorMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = indicatorMeta.Id,
                CurrentStateId = null
            })
            .ToList();

        var indicatorMetaAdditions = newIndicatorMetas
            .Except(oldIndicatorMetas, IndicatorMeta.PublicIdComparer)
            .OrderBy(indicatorMeta => indicatorMeta.Label)
            .Select(indicatorMeta => new IndicatorMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = null,
                CurrentStateId = indicatorMeta.Id
            })
            .ToList();

        publicDataDbContext.IndicatorMetaChanges.AddRange([.. indicatorMetaDeletions, .. indicatorMetaAdditions]);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CreateTimePeriodChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldTimePeriodMetas = await GetTimePeriodMetas(previousVersionId, cancellationToken);
        var newTimePeriodMetas = await GetTimePeriodMetas(nextVersionId, cancellationToken);

        var timePeriodMetaDeletions = oldTimePeriodMetas
            .Except(newTimePeriodMetas, TimePeriodMeta.CodePeriodComparer)
            .OrderBy(timePeriodMeta => timePeriodMeta.Period)
            .ThenBy(timePeriodMeta => timePeriodMeta.Code)
            .Select(timePeriodMeta => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = timePeriodMeta.Id,
                CurrentStateId = null
            })
            .ToList();

        var timePeriodMetaAdditions = newTimePeriodMetas
            .Except(oldTimePeriodMetas, TimePeriodMeta.CodePeriodComparer)
            .OrderBy(timePeriodMeta => timePeriodMeta.Period)
            .ThenBy(timePeriodMeta => timePeriodMeta.Code)
            .Select(timePeriodMeta => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = null,
                CurrentStateId = timePeriodMeta.Id
            })
            .ToList();

        publicDataDbContext.TimePeriodMetaChanges.AddRange([.. timePeriodMetaDeletions, .. timePeriodMetaAdditions]);
        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async
        Task<Dictionary<string, (FilterMeta FilterMeta, Dictionary<string, FilterOptionMetaLink> OptionLinks)>>
        GetFilterMetas(
            Guid dataSetVersionId,
            CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .FilterMetas
            .Include(m => m.OptionLinks)
            .ThenInclude(l => l.Option)
            .Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToDictionaryAsync(
                MappingKeyGenerators.Filter,
                m => (
                    FilterMeta: m,
                    OptionLinks: m.OptionLinks.ToDictionary(l => MappingKeyGenerators.FilterOptionMetaLink(l))
                ),
                cancellationToken);
    }

    private async Task<GeographicLevelMeta> GetGeographicLevelMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .GeographicLevelMetas
            .AsNoTracking()
            .SingleAsync(m => m.DataSetVersionId == dataSetVersionId, cancellationToken);
    }

    private async Task<IReadOnlyList<IndicatorMeta>> GetIndicatorMetas(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .IndicatorMetas
            .Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<GeographicLevel,
        (LocationMeta LocationMeta, Dictionary<string, LocationOptionMetaLink> OptionLinks)>> GetLocationMetas(
        Guid previousVersionId, CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .LocationMetas
            .Include(lm => lm.OptionLinks)
            .ThenInclude(oml => oml.Option)
            .Where(lm => lm.DataSetVersionId == previousVersionId)
            .ToDictionaryAsync(
                lm => lm.Level,
                lm =>
                (
                    LocationMeta: lm,
                    OptionLinks: lm.OptionLinks.ToDictionary(l => MappingKeyGenerators.LocationOptionMetaLink(l))
                ),
                cancellationToken);
    }

    private async Task<IReadOnlyList<TimePeriodMeta>> GetTimePeriodMetas(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .TimePeriodMetas
            .Where(tpm => tpm.DataSetVersionId == dataSetVersionId)
            .ToListAsync(cancellationToken);
    }
}
