using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models
{
    public class ReplacementPlan
    {
        public IEnumerable<DataBlockInfo> DataBlocks { get; set; }
        public IEnumerable<FootnoteInfo> Footnotes { get; set; }

        public ReplacementPlan(IEnumerable<DataBlockInfo> dataBlocks,
            IEnumerable<FootnoteInfo> footnotes)
        {
            DataBlocks = dataBlocks;
            Footnotes = footnotes;
        }

        public bool Valid => DataBlocks.All(info => info.Valid)
                             && Footnotes.All(info => info.Valid);
    }

    public class DataBlockInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<FilterItemReplacementViewModel> FilterItems { get; set; }
        public IEnumerable<IndicatorReplacementViewModel> Indicators { get; set; }
        public Dictionary<string, ObservationalUnitReplacementViewModel> ObservationalUnits { get; set; }
        public TimePeriodReplacementViewModel TimePeriods { get; set; }

        public DataBlockInfo(Guid id,
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

    public class FootnoteInfo
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public IEnumerable<FilterReplacementViewModel> Filters { get; set; }
        public IEnumerable<FilterGroupReplacementViewModel> FilterGroups { get; set; }
        public IEnumerable<FilterItemReplacementViewModel> FilterItems { get; set; }
        public IEnumerable<IndicatorReplacementViewModel> Indicators { get; set; }

        public FootnoteInfo(Guid id,
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
    }

    public abstract class ReplacementViewModel
    {
        public bool Valid { get; set; }
    }
}