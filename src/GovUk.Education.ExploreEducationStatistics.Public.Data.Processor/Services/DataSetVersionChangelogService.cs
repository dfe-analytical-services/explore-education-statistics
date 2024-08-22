using AngleSharp.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Profiling.Internal;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

internal class DataSetVersionChangelogService(PublicDataDbContext publicDataDbContext)
    : IDataSetVersionChangelogService
{
    public async Task<Either<ActionResult, Unit>> GenerateChangelog(
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

        var oldFilterMetas = await publicDataDbContext
            .FilterMetas
            .Include(fm => fm.OptionLinks)
            .ThenInclude(fm => fm.Option)
            .Where(fm => fm.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId)
            .ToDictionaryAsync(
                MappingKeyFunctions.FilterKeyGenerator,
                fm => new
                {
                    FilterMeta = fm,
                    OptionLinks = fm.OptionLinks.ToDictionary(foml => MappingKeyFunctions.FilterOptionKeyGenerator(foml.Option))
                },
                cancellationToken);

        var newFilterMetas = await publicDataDbContext
            .FilterMetas
            .Include(fm => fm.OptionLinks)
            .ThenInclude(fm => fm.Option)
            .Where(fm => fm.DataSetVersionId == nextDataSetVersionId)
            .ToDictionaryAsync(
                MappingKeyFunctions.FilterKeyGenerator,
                fm => new
                {
                    FilterMeta = fm,
                    OptionLinks = fm.OptionLinks.ToDictionary(foml => MappingKeyFunctions.FilterOptionKeyGenerator(foml.Option))
                },
                cancellationToken);

        var filterMetaChanges = mappings.FilterMappingPlan.Mappings
            .Where(kv => kv.Key != kv.Value.CandidateKey) // DO NOT INCLUDE CHANGELOG ENTRIES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
            .Select(kv => new FilterMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
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
                DataSetVersionId = nextDataSetVersionId,
                PreviousStateId = null,
                CurrentStateId = newFilterMetas[newCandidateKey!].FilterMeta.Id
            })
            .ToList();

        nextVersion.FilterMetaChanges = [.. filterMetaChanges, .. filterMetaAdditions];

        var filterOptionMetaChanges = mappings.FilterMappingPlan.Mappings
            .Where(fm => fm.Value.CandidateKey.HasValue()) // DON'T CREATE A CHANGELOG ENTRY FOR ANY FILTER OPTIONS WHICH HAVE HAD THEIR ENTIRE FILTER DELETED
            .SelectMany(
                fm => fm.Value.OptionMappings,
                (fm, fom) => new { FilterMapping = fm, OptionMapping = fom })
            .Where(a => a.OptionMapping.Key != a.OptionMapping.Value.CandidateKey) // DO NOT INCLUDE CHANGELOG ENTRIES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
            .Select(a => new FilterOptionMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
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
                DataSetVersionId = nextDataSetVersionId,
                PreviousState = null,
                CurrentState = new FilterOptionMetaChange.State
                {
                    MetaId = newFilterMetas[a.FilterCandidateKey!].OptionLinks[a.OptionCandidateKey!].MetaId,
                    OptionId = newFilterMetas[a.FilterCandidateKey!].OptionLinks[a.OptionCandidateKey!].OptionId,
                    PublicId = newFilterMetas[a.FilterCandidateKey!].OptionLinks[a.OptionCandidateKey!].PublicId,
                }
            });

        nextVersion.FilterOptionMetaChanges = [.. filterOptionMetaChanges, .. filterOptionMetaAdditions];



        // JUST THINKING.... DO WE WANT TO REMOVE CHANGELOG ENTRIES FOR MAPPINGS THAT MAP TO THE SAME ID AS BEFORE? IS THAT HOW IT EVEN WORKS? 
        // yes, I think we don't want to include these! If candidate key = original key then we don't include it in the changelog
        // just looked at the code and I think the candidate key can be the same as the dictionary key, if they have been mapped
        // filter key = filter public ID
        // filter option key = option label
        // filter candidate key = filter public ID
        // filter option candidate key = option label
        // location option key
        // location option candidate key = $"{option.Label} :: {option.GetRowKeyPretty()}"



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
                    OptionLinks = lm.OptionLinks.ToDictionary(loml => MappingKeyFunctions.LocationOptionMetaKeyGenerator(loml.Option))
                },
                cancellationToken);

        var newLocationMetas = await publicDataDbContext
            .LocationMetas
            .Include(lm => lm.OptionLinks)
            .ThenInclude(lm => lm.Option)
            .Where(lm => lm.DataSetVersionId == nextDataSetVersionId)
            .ToDictionaryAsync(
                lm => lm.Level,
                lm => new
                {
                    LocationMeta = lm,
                    OptionLinks = lm.OptionLinks.ToDictionary(loml => MappingKeyFunctions.LocationOptionMetaKeyGenerator(loml.Option))
                },
                cancellationToken);

        nextVersion.LocationMetaChanges = mappings.LocationMappingPlan
            .Levels
            .Where(locationGroupMappings => !locationGroupMappings.Value.Mappings.Any() || !locationGroupMappings.Value.Candidates.Any()) // ONLY INCLUDE ADDS AND DELETES FOR LOCATION LEVELS
            .Select(locationGroupMappings => new LocationMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
                PreviousStateId = locationGroupMappings.Value.Mappings.Any()
                    ? oldLocationMetas[locationGroupMappings.Key].LocationMeta.Id
                    : null,
                CurrentStateId = locationGroupMappings.Value.Candidates.Any()
                    ? newLocationMetas[locationGroupMappings.Key].LocationMeta.Id
                    : null,
            })
            .ToList();

        // check for duplicates. if a group has been deleted, don't need a changelog entry for the old options beneath it. BUT bear in mind, we still want to account for options which have been deleted, but their parent hasn't

        var locationOptionMetaDeletionsAndChanges = mappings.LocationMappingPlan
            .Levels
            .Where(locationGroupMappings => locationGroupMappings.Value.Candidates.Any()) // DON'T CREATE A CHANGELOG ENTRY FOR ANY LOCATION OPTIONS WHICH HAVE HAD THEIR ENTIRE LOCATION GROUP DELETED
            .SelectMany(
                lm => lm.Value.Mappings,
                (lm, lom) => new { LocationGroup = lm.Key, OptionMapping = lom })
            .Where(a => a.OptionMapping.Key != a.OptionMapping.Value.CandidateKey) // DO NOT INCLUDE CHANGELOG ENTRIES FOR KEYS THAT HAVE BEEN MAPPED TO THE EXACT SAME CANDIDATE KEY (NO RENAMING)
            .Select(a => new LocationOptionMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
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
                DataSetVersionId = nextDataSetVersionId,
                PreviousState = null,
                CurrentState = new LocationOptionMetaChange.State
                {
                    MetaId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionCandidateKey!].MetaId,
                    OptionId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionCandidateKey!].OptionId,
                    PublicId = newLocationMetas[a.LocationGroup].OptionLinks[a.OptionCandidateKey!].PublicId,
                }
            });

        nextVersion.LocationOptionMetaChanges = [.. locationOptionMetaDeletionsAndChanges, .. locationOptionMetaAdditions];




        var oldGeographicLevelMetas = await publicDataDbContext
            .GeographicLevelMetas
            .SingleAsync(lm => lm.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId, cancellationToken);

        var newGeographicLevelMetas = await publicDataDbContext
            .GeographicLevelMetas
            .SingleAsync(lm => lm.DataSetVersionId == nextDataSetVersionId, cancellationToken);

        nextVersion.GeographicLevelMetaChange = newGeographicLevelMetas.Levels.Order().SequenceEqual(oldGeographicLevelMetas.Levels.Order()) // DO WE HAVE AN EXTENSION METHOD FOR THIS?
            ?
            null
            : new GeographicLevelMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
                PreviousStateId = oldGeographicLevelMetas.Id,
                CurrentStateId = newGeographicLevelMetas.Id
            };




        var oldIndicatorMetaIdsByPublicId = await publicDataDbContext
            .IndicatorMetas
            .Where(im => im.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId)
            .ToDictionaryAsync(
                im => im.PublicId,
                im => im.Id,
                cancellationToken);

        var newIndicatorMetaIdsByPublicId = await publicDataDbContext
            .IndicatorMetas
            .Where(im => im.DataSetVersionId == nextDataSetVersionId)
            .ToDictionaryAsync(
                im => im.PublicId,
                im => im.Id,
                cancellationToken);

        var indicatorMetaDeletions = oldIndicatorMetaIdsByPublicId
            .Keys
            .Except(newIndicatorMetaIdsByPublicId.Keys)
            .Select(publicId => new IndicatorMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
                PreviousStateId = oldIndicatorMetaIdsByPublicId[publicId],
                CurrentStateId = null
            })
            .ToList();

        var indicatorMetaAdditions = newIndicatorMetaIdsByPublicId
            .Keys
            .Except(oldIndicatorMetaIdsByPublicId.Keys)
            .Select(publicId => new IndicatorMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
                PreviousStateId = null,
                CurrentStateId = newIndicatorMetaIdsByPublicId[publicId]
            })
            .ToList();

        nextVersion.IndicatorMetaChanges = [.. indicatorMetaDeletions, .. indicatorMetaAdditions];





        var oldTimePeriodMetaIdsByCodeAndPeriod = await publicDataDbContext
            .TimePeriodMetas
            .Where(im => im.DataSetVersionId == nextVersion.DataSet.LatestLiveVersionId)
            .ToDictionaryAsync(
                im => (im.Code, im.Period),
                im => im.Id,
                cancellationToken);

        var newTimePeriodMetaIdsByCodeAndPeriod = await publicDataDbContext
            .TimePeriodMetas
            .Where(im => im.DataSetVersionId == nextDataSetVersionId)
            .ToDictionaryAsync(
                im => (im.Code, im.Period),
                im => im.Id,
                cancellationToken);

        var timePeriodMetaDeletions = oldTimePeriodMetaIdsByCodeAndPeriod
            .Keys
            .Except(newTimePeriodMetaIdsByCodeAndPeriod.Keys)
            .Select(codeAndPeriod => new TimePeriodMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
                PreviousStateId = oldTimePeriodMetaIdsByCodeAndPeriod[codeAndPeriod],
                CurrentStateId = null
            })
            .ToList();

        var timePeriodMetaAdditions = newTimePeriodMetaIdsByCodeAndPeriod
            .Keys
            .Except(oldTimePeriodMetaIdsByCodeAndPeriod.Keys)
            .Select(codeAndPeriod => new TimePeriodMetaChange
            {
                DataSetVersionId = nextDataSetVersionId,
                PreviousStateId = null,
                CurrentStateId = newTimePeriodMetaIdsByCodeAndPeriod[codeAndPeriod]
            })
            .ToList();

        nextVersion.TimePeriodMetaChanges = [.. timePeriodMetaDeletions, .. timePeriodMetaAdditions];


        await publicDataDbContext.SaveChangesAsync();

        return Unit.Instance;
    }
}
