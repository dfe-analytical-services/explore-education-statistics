using GovUk.Education.ExploreEducationStatistics.Common.Model;
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

        var mappings = await publicDataDbContext
            .DataSetVersionMappings
            .AsNoTracking()
            .SingleAsync(dsvm => dsvm.TargetDataSetVersionId == nextDataSetVersionId, cancellationToken);

        (nextVersion.FilterMetaChanges, nextVersion.FilterOptionMetaChanges) = await CreateFilterChanges(
            previousVersionId: previousVersionId,
            nextVersionId: nextVersion.Id,
            mappings: mappings,
            cancellationToken: cancellationToken);

        (nextVersion.LocationMetaChanges, nextVersion.LocationOptionMetaChanges) = await CreateLocationChanges(
            previousVersionId: previousVersionId,
            nextVersionId: nextVersion.Id,
            mappings: mappings,
            cancellationToken: cancellationToken);

        nextVersion.GeographicLevelMetaChange = await CreateGeographicLevelChanges(
            previousVersionId: previousVersionId,
            nextVersionId: nextVersion.Id,
            cancellationToken: cancellationToken);

        nextVersion.IndicatorMetaChanges = await CreateIndicatorChanges(
            previousVersionId: previousVersionId,
            nextVersionId: nextVersion.Id,
            cancellationToken: cancellationToken);

        nextVersion.TimePeriodMetaChanges = await CreateTimePeriodChanges(
            previousVersionId: previousVersionId,
            nextVersionId: nextVersion.Id,
            cancellationToken: cancellationToken);

        await publicDataDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<(List<FilterMetaChange> filterMetaChanges, List<FilterOptionMetaChange> filterOptionMetaChanges)>
        CreateFilterChanges(
            Guid previousVersionId,
            Guid nextVersionId,
            DataSetVersionMapping mappings,
            CancellationToken cancellationToken)
    {
        var oldFilterMetas = await GetFilterMetas(previousVersionId, cancellationToken);
        var newFilterMetas = await GetFilterMetas(nextVersionId, cancellationToken);

        var filterMetaDeletionsAndChanges = mappings.FilterMappingPlan.Mappings
            // DO NOT INCLUDE CHANGES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
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

        var filterMetaAdditions = mappings.FilterMappingPlan.Candidates
            .Keys
            .Except(mappings.FilterMappingPlan.Mappings.Select(m => m.Value.CandidateKey!))
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

        var filterOptionMetaDeletionsAndChanges = mappings.FilterMappingPlan.Mappings
            // DON'T CREATE A CHANGE FOR ANY FILTER OPTIONS WHICH HAVE HAD THEIR ENTIRE FILTER DELETED
            .Where(kv => !string.IsNullOrEmpty(kv.Value.CandidateKey))
            .SelectMany(
                fm => fm.Value.OptionMappings,
                (fm, fom) => new
                {
                    FilterMapping = fm,
                    OptionMapping = fom
                })
            // DO NOT INCLUDE CHANGES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
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

        var filterOptionMetaAdditions = mappings.FilterMappingPlan.Candidates
            .SelectMany(
                fc => fc.Value.Options,
                (fc, foc) => new
                {
                    FilterCandidateKey = (string?)fc.Key,
                    OptionCandidateKey = (string?)foc.Key
                })
            .Except(
                mappings.FilterMappingPlan.Mappings
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

        return (
            filterMetaChanges: [.. filterMetaDeletionsAndChanges, .. filterMetaAdditions],
            filterOptionMetaChanges: [.. filterOptionMetaDeletionsAndChanges, .. filterOptionMetaAdditions]
        );
    }

    private async
        Task<(List<LocationMetaChange> locationMetaChanges, List<LocationOptionMetaChange> locationOptionMetaChanges)>
        CreateLocationChanges(
            Guid previousVersionId,
            Guid nextVersionId,
            DataSetVersionMapping mappings,
            CancellationToken cancellationToken)
    {
        var oldLocationMetas = await GetLocationMetas(previousVersionId, cancellationToken);
        var newLocationMetas = await GetLocationMetas(nextVersionId, cancellationToken);

        var locationMetaChanges = mappings.LocationMappingPlan
            .Levels
            // ONLY INCLUDE ADDS AND DELETES FOR LOCATION LEVELS
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

        var locationOptionMetaDeletionsAndChanges = mappings.LocationMappingPlan
            .Levels
            // DON'T CREATE A CHANGE FOR ANY LOCATION OPTIONS WHICH HAVE HAD THEIR ENTIRE LOCATION GROUP DELETED
            .Where(locationGroupMappings => locationGroupMappings.Value.Candidates.Count > 0)
            .SelectMany(
                lm => lm.Value.Mappings,
                (lm, lom) => new
                {
                    LocationGroup = lm.Key,
                    OptionMapping = lom
                })
            // DO NOT INCLUDE CHANGES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
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

        var locationOptionMetaAdditions = mappings.LocationMappingPlan
            .Levels
            .SelectMany(
                lm => lm.Value.Candidates,
                (lm, loc) => new
                {
                    LocationGroup = lm.Key,
                    OptionCandidateKey = (string?)loc.Key
                })
            .Except(
                mappings.LocationMappingPlan
                    .Levels
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

        return (
            locationMetaChanges,
            locationOptionMetaChanges: [.. locationOptionMetaDeletionsAndChanges, .. locationOptionMetaAdditions]
        );
    }

    private async Task<GeographicLevelMetaChange?> CreateGeographicLevelChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldGeographicLevelMetas = await GetGeographicLevelMeta(previousVersionId, cancellationToken);
        var newGeographicLevelMetas = await GetGeographicLevelMeta(nextVersionId, cancellationToken);

        var levelsAreEqual =
            newGeographicLevelMetas.Levels.Order().SequenceEqual(oldGeographicLevelMetas.Levels.Order());

        return levelsAreEqual
            ? null
            : new GeographicLevelMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = oldGeographicLevelMetas.Id,
                CurrentStateId = newGeographicLevelMetas.Id
            };
    }

    private async Task<List<IndicatorMetaChange>> CreateIndicatorChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldIndicatorMetaIdsByPublicId = await GetIndicatorMetas(previousVersionId, cancellationToken);
        var newIndicatorMetaIdsByPublicId = await GetIndicatorMetas(nextVersionId, cancellationToken);

        var indicatorMetaDeletions = oldIndicatorMetaIdsByPublicId
            .Keys
            .Except(newIndicatorMetaIdsByPublicId.Keys)
            .Select(publicId => new IndicatorMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = oldIndicatorMetaIdsByPublicId[publicId].Id,
                CurrentStateId = null
            })
            .ToList();

        var indicatorMetaAdditions = newIndicatorMetaIdsByPublicId
            .Keys
            .Except(oldIndicatorMetaIdsByPublicId.Keys)
            .Select(publicId => new IndicatorMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = null,
                CurrentStateId = newIndicatorMetaIdsByPublicId[publicId].Id
            })
            .ToList();

        return [.. indicatorMetaDeletions, .. indicatorMetaAdditions];
    }

    private async Task<List<TimePeriodMetaChange>> CreateTimePeriodChanges(
        Guid previousVersionId,
        Guid nextVersionId,
        CancellationToken cancellationToken)
    {
        var oldTimePeriodMetaIdsByCodeAndPeriod = await GetTimePeriodMetas(previousVersionId, cancellationToken);
        var newTimePeriodMetaIdsByCodeAndPeriod = await GetTimePeriodMetas(nextVersionId, cancellationToken);

        var timePeriodMetaDeletions = oldTimePeriodMetaIdsByCodeAndPeriod
            .Keys
            .Except(newTimePeriodMetaIdsByCodeAndPeriod.Keys)
            .Select(codeAndPeriod => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = oldTimePeriodMetaIdsByCodeAndPeriod[codeAndPeriod].Id,
                CurrentStateId = null
            })
            .ToList();

        var timePeriodMetaAdditions = newTimePeriodMetaIdsByCodeAndPeriod
            .Keys
            .Except(oldTimePeriodMetaIdsByCodeAndPeriod.Keys)
            .Select(codeAndPeriod => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersionId,
                PreviousStateId = null,
                CurrentStateId = newTimePeriodMetaIdsByCodeAndPeriod[codeAndPeriod].Id
            })
            .ToList();

        return [.. timePeriodMetaDeletions, .. timePeriodMetaAdditions];
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

    private async Task<Dictionary<string, IndicatorMeta>> GetIndicatorMetas(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .IndicatorMetas
            .Where(m => m.DataSetVersionId == dataSetVersionId)
            .ToDictionaryAsync(m => m.PublicId, cancellationToken);
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

    private async Task<Dictionary<(TimeIdentifier Code, string Period), TimePeriodMeta>> GetTimePeriodMetas(
        Guid dataSetVersionId,
        CancellationToken cancellationToken)
    {
        return await publicDataDbContext
            .TimePeriodMetas
            .Where(tpm => tpm.DataSetVersionId == dataSetVersionId)
            .ToDictionaryAsync(m => (m.Code, m.Period), cancellationToken);
    }
}
