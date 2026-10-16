#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Utils.MetaViewModelBuilderUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

public static class IndicatorsMetaViewModelBuilder
{
    public static Dictionary<string, IndicatorGroupMetaViewModel> BuildIndicatorGroups(
        IEnumerable<IndicatorGroup> values,
        IEnumerable<IndicatorGroupSequenceEntry>? sequence = null,
        IEnumerable<PairedIndicatorEntry>? pairedIndicators = null
    )
    {
        var parentIdsByChildId = pairedIndicators?.ToDictionary(
            pair => pair.ChildIndicatorId,
            pair => pair.ParentIndicatorId
        );

        return OrderAsDictionary(
            values,
            idSelector: filter => filter.Id,
            labelSelector: filter => filter.Label,
            sequenceIdSelector: filterOrdering => filterOrdering.Id,
            resultSelector: (input, index) =>
                new IndicatorGroupMetaViewModel(input, index)
                {
                    Options = BuildIndicators(
                        input.Value.Indicators,
                        input.Sequence?.ChildSequence,
                        parentIdsByChildId
                    ),
                },
            sequence: sequence
        );
    }

    public static List<IndicatorMetaViewModel> BuildIndicators(
        IEnumerable<Indicator> values,
        IEnumerable<Guid>? sequence = null,
        IReadOnlyDictionary<Guid, Guid>? parentIdsByChildId = null
    )
    {
        return OrderBySequenceOrLabel(
                values,
                idSelector: value => value.Id,
                labelSelector: value => value.Label,
                sequenceIdSelector: sequenceEntry => sequenceEntry,
                resultSelector: value => new IndicatorMetaViewModel(value)
                {
                    ParentIndicatorId =
                        parentIdsByChildId is not null
                        && parentIdsByChildId.TryGetValue(value.Value.Id, out var parentId)
                            ? parentId
                            : null,
                },
                sequence
            )
            .ToList();
    }
}
