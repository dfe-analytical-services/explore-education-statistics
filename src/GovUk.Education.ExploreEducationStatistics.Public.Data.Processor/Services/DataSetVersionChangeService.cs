using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling.Internal;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionChangeService(PublicDataDbContext publicDataDbContext)
    : IDataSetVersionChangeService
{
    public async Task<Either<ActionResult, Unit>> GenerateChanges(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default)
    {
        var nextVersion = await publicDataDbContext
            .DataSetVersions
            .Include(dsv => dsv.DataSet)
            .ThenInclude(ds => ds.LatestLiveVersion)
            .SingleAsync(dsv => dsv.Id == nextDataSetVersionId, cancellationToken);

        var mappings = await publicDataDbContext
            .DataSetVersionMappings
            .SingleAsync(dsvm => dsvm.TargetDataSetVersionId == nextDataSetVersionId, cancellationToken);

        var (filterMetaChanges, filterOptionMetaChanges) = await GenerateFilterChanges(
            nextVersion: nextVersion,
            mappings: mappings,
            cancellationToken: cancellationToken);

        nextVersion.FilterMetaChanges = filterMetaChanges;
        nextVersion.FilterOptionMetaChanges = filterOptionMetaChanges;

        var (locationMetaChanges, locationOptionMetaChanges) = await GenerateLocationChanges(
            nextVersion: nextVersion,
            mappings: mappings,
            cancellationToken: cancellationToken);

        nextVersion.LocationMetaChanges = locationMetaChanges;
        nextVersion.LocationOptionMetaChanges = locationOptionMetaChanges;

        var geographicLevelMetaChange = await GenerateGeographicLevelChanges(
            nextVersion: nextVersion,
            cancellationToken: cancellationToken);

        nextVersion.GeographicLevelMetaChange = geographicLevelMetaChange;

        var indicatorMetaChanges = await GenerateIndicatorChanges(
            nextVersion: nextVersion,
            cancellationToken: cancellationToken);

        nextVersion.IndicatorMetaChanges = indicatorMetaChanges;

        var timePeriodMetaChanges = await GenerateTimePeriodChanges(
            nextVersion: nextVersion,
            cancellationToken: cancellationToken);

        nextVersion.TimePeriodMetaChanges = timePeriodMetaChanges;

        await publicDataDbContext.SaveChangesAsync(cancellationToken);

        return Unit.Instance;
    }

    private async Task<(List<FilterMetaChange> filterMetaChanges, List<FilterOptionMetaChange> filterOptionMetaChanges)> GenerateFilterChanges(
        DataSetVersion nextVersion, 
        DataSetVersionMapping mappings, 
        CancellationToken cancellationToken)
    {
        var oldFilterMetas = await publicDataDbContext
            .FilterMetas
            .Include(fm => fm.OptionLinks)
            .ThenInclude(fm => fm.Option)
            .Where(fm => fm.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId)
            .ToDictionaryAsync(
                MappingKeyGenerators.Filter,
                fm => new
                {
                    FilterMeta = fm,
                    OptionLinks = fm.OptionLinks.ToDictionary(l => MappingKeyGenerators.FilterOptionMetaLink(l))
                },
                cancellationToken);

        var newFilterMetas = await publicDataDbContext
            .FilterMetas
            .Include(fm => fm.OptionLinks)
            .ThenInclude(fm => fm.Option)
            .Where(fm => fm.DataSetVersionId == nextVersion.Id)
            .ToDictionaryAsync(
                MappingKeyGenerators.Filter,
                fm => new
                {
                    FilterMeta = fm,
                    OptionLinks = fm.OptionLinks.ToDictionary(l => MappingKeyGenerators.FilterOptionMetaLink(l))
                },
                cancellationToken);

        var filterMetaDeletionsAndChanges = mappings.FilterMappingPlan.Mappings
            .Where(kv => kv.Key != kv.Value.CandidateKey) // DO NOT INCLUDE CHANGES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
            .Select(kv => new FilterMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousStateId = oldFilterMetas[kv.Key].FilterMeta.Id,
                CurrentStateId = kv.Value.CandidateKey.HasValue()
                    ? newFilterMetas[kv.Value.CandidateKey].FilterMeta.Id
                    : null
            })
            .ToList();

        var filterMetaAdditions = mappings.FilterMappingPlan.Candidates
            .Keys
            .Except(mappings.FilterMappingPlan.Mappings.Select(m => m.Value.CandidateKey))
            .Select(newCandidateKey => new FilterMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousStateId = null,
                CurrentStateId = newFilterMetas[newCandidateKey!].FilterMeta.Id
            })
            .ToList();

        var filterOptionMetaDeletionsAndChanges = mappings.FilterMappingPlan.Mappings
            .Where(fm => fm.Value.CandidateKey.HasValue()) // DON'T CREATE A CHANGE FOR ANY FILTER OPTIONS WHICH HAVE HAD THEIR ENTIRE FILTER DELETED
            .SelectMany(
                fm => fm.Value.OptionMappings,
                (fm, fom) => new { FilterMapping = fm, OptionMapping = fom })
            .Where(a => a.OptionMapping.Key != a.OptionMapping.Value.CandidateKey) // DO NOT INCLUDE CHANGES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
            .Select(a => new FilterOptionMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousState = new FilterOptionMetaChange.State
                {
                    MetaId = oldFilterMetas[a.FilterMapping.Key].OptionLinks[a.OptionMapping.Key].MetaId,
                    OptionId = oldFilterMetas[a.FilterMapping.Key].OptionLinks[a.OptionMapping.Key].OptionId,
                    PublicId = oldFilterMetas[a.FilterMapping.Key].OptionLinks[a.OptionMapping.Key].PublicId
                },
                CurrentState = a.OptionMapping.Value.CandidateKey.HasValue()
                    ? new FilterOptionMetaChange.State
                    {
                        MetaId = newFilterMetas[a.FilterMapping.Value.CandidateKey!].OptionLinks[a.OptionMapping.Value.CandidateKey].MetaId,
                        OptionId = newFilterMetas[a.FilterMapping.Value.CandidateKey!].OptionLinks[a.OptionMapping.Value.CandidateKey].OptionId,
                        PublicId = newFilterMetas[a.FilterMapping.Value.CandidateKey!].OptionLinks[a.OptionMapping.Value.CandidateKey].PublicId,
                    }
                    : null
            })
            .ToList();

        var filterOptionMetaAdditions = mappings.FilterMappingPlan.Candidates
            .SelectMany(
                fc => fc.Value.Options,
                (fc, foc) => new { FilterCandidateKey = (string?)fc.Key, OptionCandidateKey = (string?)foc.Key })
            .Except(
                mappings.FilterMappingPlan.Mappings
                .SelectMany(
                    fm => fm.Value.OptionMappings,
                    (fm, fom) => new { FilterCandidateKey = fm.Value.CandidateKey, OptionCandidateKey = fom.Value.CandidateKey }
                    ))
            .Select(a => new FilterOptionMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousState = null,
                CurrentState = new FilterOptionMetaChange.State
                {
                    MetaId = newFilterMetas[a.FilterCandidateKey!].OptionLinks[a.OptionCandidateKey!].MetaId,
                    OptionId = newFilterMetas[a.FilterCandidateKey!].OptionLinks[a.OptionCandidateKey!].OptionId,
                    PublicId = newFilterMetas[a.FilterCandidateKey!].OptionLinks[a.OptionCandidateKey!].PublicId,
                }
            });

        List<FilterMetaChange> filterMetaChanges = [.. filterMetaDeletionsAndChanges, .. filterMetaAdditions];
        List<FilterOptionMetaChange> filterOptionMetaChanges = [.. filterOptionMetaDeletionsAndChanges, .. filterOptionMetaAdditions];

        return (filterMetaChanges, filterOptionMetaChanges);
    }

    private async Task<(List<LocationMetaChange> locationMetaChanges, List<LocationOptionMetaChange> locationOptionMetaChanges)> GenerateLocationChanges(
        DataSetVersion nextVersion,
        DataSetVersionMapping mappings,
        CancellationToken cancellationToken)
    {
        var oldLocationMetas = await publicDataDbContext
            .LocationMetas
            .Include(lm => lm.OptionLinks)
            .ThenInclude(lm => lm.Option)
            .Where(lm => lm.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId)
            .ToDictionaryAsync(
                lm => lm.Level,
                lm => new
                {
                    LocationMeta = lm,
                    OptionLinks = lm.OptionLinks.ToDictionary(l => MappingKeyGenerators.LocationOptionMetaLink(l))
                },
                cancellationToken);

        var newLocationMetas = await publicDataDbContext
            .LocationMetas
            .Include(lm => lm.OptionLinks)
            .ThenInclude(lm => lm.Option)
            .Where(lm => lm.DataSetVersionId == nextVersion.Id)
            .ToDictionaryAsync(
                lm => lm.Level,
                lm => new
                {
                    LocationMeta = lm,
                    OptionLinks = lm.OptionLinks.ToDictionary(l => MappingKeyGenerators.LocationOptionMetaLink(l))
                },
                cancellationToken);

        var locationMetaChanges = mappings.LocationMappingPlan
            .Levels
            .Where(locationGroupMappings => !locationGroupMappings.Value.Mappings.Any() || !locationGroupMappings.Value.Candidates.Any()) // ONLY INCLUDE ADDS AND DELETES FOR LOCATION LEVELS
            .Select(locationGroupMappings => new LocationMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousStateId = locationGroupMappings.Value.Mappings.Any()
                    ? oldLocationMetas[locationGroupMappings.Key].LocationMeta.Id
                    : null,
                CurrentStateId = locationGroupMappings.Value.Candidates.Any()
                    ? newLocationMetas[locationGroupMappings.Key].LocationMeta.Id
                    : null,
            })
            .ToList();

        var locationOptionMetaDeletionsAndChanges = mappings.LocationMappingPlan
            .Levels
            .Where(locationGroupMappings => locationGroupMappings.Value.Candidates.Any()) // DON'T CREATE A CHANGE FOR ANY LOCATION OPTIONS WHICH HAVE HAD THEIR ENTIRE LOCATION GROUP DELETED
            .SelectMany(
                lm => lm.Value.Mappings,
                (lm, lom) => new { LocationGroup = lm.Key, OptionMapping = lom })
            .Where(a => a.OptionMapping.Key != a.OptionMapping.Value.CandidateKey) // DO NOT INCLUDE CHANGES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
            .Select(a => new LocationOptionMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousState = new LocationOptionMetaChange.State
                {
                    MetaId = oldLocationMetas[a.LocationGroup].OptionLinks[a.OptionMapping.Key].MetaId,
                    OptionId = oldLocationMetas[a.LocationGroup].OptionLinks[a.OptionMapping.Key].OptionId,
                    PublicId = oldLocationMetas[a.LocationGroup].OptionLinks[a.OptionMapping.Key].PublicId,
                },
                CurrentState = a.OptionMapping.Value.CandidateKey.HasValue()
                    ? new LocationOptionMetaChange.State
                    {
                        MetaId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionMapping.Value.CandidateKey].MetaId,
                        OptionId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionMapping.Value.CandidateKey].OptionId,
                        PublicId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionMapping.Value.CandidateKey].PublicId,
                    }
                    : null
            })
            .ToList();

        var locationOptionMetaAdditions = mappings.LocationMappingPlan
            .Levels
            .SelectMany(
                lm => lm.Value.Candidates,
                (lm, loc) => new { LocationGroup = lm.Key, OptionCandidateKey = (string?)loc.Key })
            .Except(
                mappings.LocationMappingPlan
                .Levels
                .SelectMany(
                    lm => lm.Value.Mappings,
                    (lm, lom) => new { LocationGroup = lm.Key, OptionCandidateKey = lom.Value.CandidateKey }
                    ))
            .Select(a => new LocationOptionMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousState = null,
                CurrentState = new LocationOptionMetaChange.State
                {
                    MetaId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionCandidateKey!].MetaId,
                    OptionId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionCandidateKey!].OptionId,
                    PublicId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionCandidateKey!].PublicId,
                }
            });

        List<LocationOptionMetaChange> locationOptionMetaChanges = [.. locationOptionMetaDeletionsAndChanges, .. locationOptionMetaAdditions];

        return (locationMetaChanges, locationOptionMetaChanges);
    }

    private async Task<GeographicLevelMetaChange?> GenerateGeographicLevelChanges(
        DataSetVersion nextVersion,
        CancellationToken cancellationToken)
    {
        var oldGeographicLevelMetas = await publicDataDbContext
            .GeographicLevelMetas
            .SingleAsync(lm => lm.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId, cancellationToken);

        var newGeographicLevelMetas = await publicDataDbContext
            .GeographicLevelMetas
            .SingleAsync(lm => lm.DataSetVersionId == nextVersion.Id, cancellationToken);

        return newGeographicLevelMetas.Levels.Order().SequenceEqual(oldGeographicLevelMetas.Levels.Order()) // DO WE HAVE AN EXTENSION METHOD FOR THIS?
            ?
            null
            : new GeographicLevelMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousStateId = oldGeographicLevelMetas.Id,
                CurrentStateId = newGeographicLevelMetas.Id
            };
    }

    private async Task<List<IndicatorMetaChange>> GenerateIndicatorChanges(
        DataSetVersion nextVersion,
        CancellationToken cancellationToken)
    {
        var oldIndicatorMetaIdsByPublicId = await publicDataDbContext
            .IndicatorMetas
            .Where(im => im.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId)
            .ToDictionaryAsync(
                im => im.PublicId,
                im => im.Id,
                cancellationToken);

        var newIndicatorMetaIdsByPublicId = await publicDataDbContext
            .IndicatorMetas
            .Where(im => im.DataSetVersionId == nextVersion.Id)
            .ToDictionaryAsync(
                im => im.PublicId,
                im => im.Id,
                cancellationToken);

        var indicatorMetaDeletions = oldIndicatorMetaIdsByPublicId
            .Keys
            .Except(newIndicatorMetaIdsByPublicId.Keys)
            .Select(publicId => new IndicatorMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousStateId = oldIndicatorMetaIdsByPublicId[publicId],
                CurrentStateId = null
            })
            .ToList();

        var indicatorMetaAdditions = newIndicatorMetaIdsByPublicId
            .Keys
            .Except(oldIndicatorMetaIdsByPublicId.Keys)
            .Select(publicId => new IndicatorMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousStateId = null,
                CurrentStateId = newIndicatorMetaIdsByPublicId[publicId]
            })
            .ToList();

        return [.. indicatorMetaDeletions, .. indicatorMetaAdditions];
    }

    private async Task<List<TimePeriodMetaChange>> GenerateTimePeriodChanges(
        DataSetVersion nextVersion,
        CancellationToken cancellationToken)
    {
        var oldTimePeriodMetaIdsByCodeAndPeriod = await publicDataDbContext
            .TimePeriodMetas
            .Where(im => im.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId)
            .ToDictionaryAsync(
                im => (im.Code, im.Period),
                im => im.Id,
                cancellationToken);

        var newTimePeriodMetaIdsByCodeAndPeriod = await publicDataDbContext
            .TimePeriodMetas
            .Where(im => im.DataSetVersionId == nextVersion.Id)
            .ToDictionaryAsync(
                im => (im.Code, im.Period),
                im => im.Id,
                cancellationToken);

        var timePeriodMetaDeletions = oldTimePeriodMetaIdsByCodeAndPeriod
            .Keys
            .Except(newTimePeriodMetaIdsByCodeAndPeriod.Keys)
            .Select(codeAndPeriod => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousStateId = oldTimePeriodMetaIdsByCodeAndPeriod[codeAndPeriod],
                CurrentStateId = null
            })
            .ToList();

        var timePeriodMetaAdditions = newTimePeriodMetaIdsByCodeAndPeriod
            .Keys
            .Except(oldTimePeriodMetaIdsByCodeAndPeriod.Keys)
            .Select(codeAndPeriod => new TimePeriodMetaChange
            {
                DataSetVersionId = nextVersion.Id,
                PreviousStateId = null,
                CurrentStateId = newTimePeriodMetaIdsByCodeAndPeriod[codeAndPeriod]
            })
            .ToList();

        return [.. timePeriodMetaDeletions, .. timePeriodMetaAdditions];
    }
}