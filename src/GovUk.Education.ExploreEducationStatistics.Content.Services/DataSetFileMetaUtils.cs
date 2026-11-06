using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

internal static class DataSetFileMetaUtils
{
    internal static List<string> GetOrderedFilters(
        List<FilterMeta> filters,
        List<FilterSequenceEntry>? filterSequence
    ) =>
        MetaViewModelBuilderUtils
            .OrderBySequenceOrLabel(
                filters,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry.Id,
                resultSelector: value => value.Value.Label,
                filterSequence
            )
            .ToList();

    internal static List<string> GetOrderedIndicators(
        List<IndicatorMeta> indicators,
        List<IndicatorGroupSequenceEntry>? indicatorGroupSequence
    )
    {
        var indicatorSequence = indicatorGroupSequence?.SelectMany(seq => seq.ChildSequence);
        return MetaViewModelBuilderUtils
            .OrderBySequenceOrLabel(
                indicators,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry,
                resultSelector: value => value.Value.Label,
                indicatorSequence
            )
            .ToList();
    }
}
