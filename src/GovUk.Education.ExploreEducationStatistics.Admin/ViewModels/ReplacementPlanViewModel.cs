using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReplacementPlanViewModel
    {
        public IEnumerable<DataBlockReplacementPlanViewModel> DataBlocks { get; }
        public IEnumerable<FootnoteReplacementPlanViewModel> Footnotes { get; }

        public ReplacementPlanViewModel(IEnumerable<DataBlockReplacementPlanViewModel> dataBlocks,
            IEnumerable<FootnoteReplacementPlanViewModel> footnotes)
        {
            DataBlocks = dataBlocks;
            Footnotes = footnotes;
        }

        public bool Valid => DataBlocks.All(info => info.Valid)
                             && Footnotes.All(info => info.Valid);
    }

    public class DataBlockReplacementPlanViewModel
    {
        public Guid Id { get; }
        public string Name { get; }
        public IEnumerable<FilterItemReplacementViewModel> FilterItems { get; }
        public IEnumerable<IndicatorReplacementViewModel> Indicators { get; }
        public Dictionary<string, ObservationalUnitReplacementViewModel> ObservationalUnits { get; }
        public TimePeriodReplacementViewModel TimePeriods { get; }

        public DataBlockReplacementPlanViewModel(Guid id,
            string name,
            IEnumerable<FilterItemReplacementViewModel> filterItems,
            IEnumerable<IndicatorReplacementViewModel> indicators,
            Dictionary<string, ObservationalUnitReplacementViewModel> observationalUnits,
            TimePeriodReplacementViewModel timePeriods)
        {
            Id = id;
            Name = name;
            FilterItems = filterItems;
            Indicators = indicators;
            ObservationalUnits = observationalUnits;
            TimePeriods = timePeriods;
        }

        public bool Valid => FilterItems.All(model => model.Valid)
                             && Indicators.All(model => model.Valid)
                             && ObservationalUnits.Values.All((model => model.Valid))
                             && TimePeriods.Valid;
    }

    public class FootnoteReplacementPlanViewModel
    {
        public Guid Id { get; }
        public string Content { get; }
        public IEnumerable<FilterReplacementViewModel> Filters { get; }
        public IEnumerable<FilterGroupReplacementViewModel> FilterGroups { get; }
        public IEnumerable<FilterItemReplacementViewModel> FilterItems { get; }
        public IEnumerable<IndicatorReplacementViewModel> Indicators { get; }

        public FootnoteReplacementPlanViewModel(Guid id,
            string content,
            IEnumerable<FilterReplacementViewModel> filters,
            IEnumerable<FilterGroupReplacementViewModel> filterGroups,
            IEnumerable<FilterItemReplacementViewModel> filterItems,
            IEnumerable<IndicatorReplacementViewModel> indicators)
        {
            Id = id;
            Content = content;
            Filters = filters;
            FilterGroups = filterGroups;
            FilterItems = filterItems;
            Indicators = indicators;
        }

        public bool Valid => Filters.All(model => model.Valid)
                             && FilterGroups.All(model => model.Valid)
                             && FilterItems.All(model => model.Valid)
                             && Indicators.All(model => model.Valid);
    }

    public class FilterReplacementViewModel : TargetableReplacementViewModel
    {
        public string Name { get; set; }
    }

    public class FilterGroupReplacementViewModel : TargetableReplacementViewModel
    {
        public string FilterLabel { get; set; }
    }

    public class FilterItemReplacementViewModel : TargetableReplacementViewModel
    {
    }

    public class IndicatorReplacementViewModel : TargetableReplacementViewModel
    {
        public string Name { get; set; }
    }

    public class ObservationalUnitReplacementViewModel : ReplacementViewModel
    {
        public IEnumerable<string> Matched { get; set; }
        public IEnumerable<string> Unmatched { get; set; }
        public new bool Valid => !Unmatched.Any();
    }

    public class TimePeriodReplacementViewModel : ReplacementViewModel
    {
        public TimePeriodQuery Query;
    }

    public abstract class TargetableReplacementViewModel
    {
        public Guid Id { get; set; }
        public string Label { get; set; }
        public Guid? Target { get; set; }
        public bool Valid => Target.HasValue;

        [JsonIgnore]
        public Guid TargetValue
        {
            get
            {
                if (!Target.HasValue)
                {
                    throw new InvalidOperationException ($"{nameof(Target)} does not have a value");
                }
                return Target.Value;
            }
        }
    }

    public abstract class ReplacementViewModel
    {
        public bool Valid { get; set; }
    }
}