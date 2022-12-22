#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using static GovUk.Education.ExploreEducationStatistics.Data.Services.Utils.MetaViewModelBuilderUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Utils;

public static class IndicatorsMetaViewModelBuilder
{
    public static Dictionary<string, IndicatorGroupMetaViewModel> BuildIndicatorGroups(
        IEnumerable<IndicatorGroup> values,
        IEnumerable<IndicatorGroupSequenceEntry>? sequence = null)
    {
        return OrderAsDictionary(values,
            idSelector: filter => filter.Id,
            labelSelector: filter => filter.Label,
            sequenceIdSelector: filterOrdering => filterOrdering.Id,
            resultSelector: (input, index) => new IndicatorGroupMetaViewModel(input, index)
            {
                Options = BuildIndicators(input.Value.Indicators, input.Sequence?.ChildSequence)
            },
            sequence: sequence
        );
    }

    public static List<IndicatorMetaViewModel> BuildIndicators(IEnumerable<Indicator> values,
        IEnumerable<Guid>? sequence = null)
    {
        return OrderAsList(values,
            idSelector: value => value.Id,
            labelSelector: value => value.Label,
            sequenceIdSelector: sequenceEntry => sequenceEntry,
            resultSelector: value => new IndicatorMetaViewModel(value),
            sequence
        );
    }
}
