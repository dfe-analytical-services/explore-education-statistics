﻿#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class DataReplacementPlanViewModel
    {
        public IEnumerable<DataBlockReplacementPlanViewModel> DataBlocks { get; }
        public IEnumerable<FootnoteReplacementPlanViewModel> Footnotes { get; }
        public Guid OriginalSubjectId { get; }
        public Guid ReplacementSubjectId { get; }

        public DataReplacementPlanViewModel(
            IEnumerable<DataBlockReplacementPlanViewModel> dataBlocks,
            IEnumerable<FootnoteReplacementPlanViewModel> footnotes,
            Guid originalSubjectId,
            Guid replacementSubjectId)
        {
            DataBlocks = dataBlocks;
            Footnotes = footnotes;
            OriginalSubjectId = originalSubjectId;
            ReplacementSubjectId = replacementSubjectId;
        }

        public bool Valid => DataBlocks.All(info => info.Valid)
                             && Footnotes.All(info => info.Valid);

        /**
         * Trimmed down version of the data replacement plan that
         * only shows full replacement details for invalid items.
         */
        public DataReplacementPlanViewModel ToSummary()
        {
            return new DataReplacementPlanViewModel(
                DataBlocks.Select(block => block.ToSummary()),
                Footnotes.Select(footnote => footnote.ToSummary()),
                OriginalSubjectId,
                ReplacementSubjectId
            );
        }
    }

    public class DataBlockReplacementPlanViewModel
    {
        public Guid Id { get; }
        public string Name { get; }
        public Dictionary<Guid, FilterReplacementViewModel> Filters { get; }
        public Dictionary<Guid, IndicatorGroupReplacementViewModel> IndicatorGroups { get; }
        public Dictionary<string, LocationReplacementViewModel> Locations { get; }
        public TimePeriodRangeReplacementViewModel? TimePeriods { get; }

        public DataBlockReplacementPlanViewModel(
            Guid id,
            string name,
            Dictionary<Guid, FilterReplacementViewModel>? filters = null,
            Dictionary<Guid, IndicatorGroupReplacementViewModel>? indicators = null,
            Dictionary<string, LocationReplacementViewModel>? locations = null,
            TimePeriodRangeReplacementViewModel? timePeriods = null)
        {
            Id = id;
            Name = name;
            Filters = filters ?? new Dictionary<Guid, FilterReplacementViewModel>();
            IndicatorGroups = indicators ?? new Dictionary<Guid, IndicatorGroupReplacementViewModel>();
            Locations = locations ?? new Dictionary<string, LocationReplacementViewModel>();
            TimePeriods = timePeriods;
        }

        public bool Valid => Filters.All(model => model.Value.Valid)
                             && IndicatorGroups.All(model => model.Value.Valid)
                             && Locations.Values.All(model => model.Valid)
                             && (TimePeriods?.Valid ?? true);

        public DataBlockReplacementPlanViewModel ToSummary()
        {
            return Valid ? new DataBlockReplacementPlanViewModel(Id, Name) : this;
        }
    }

    public class FootnoteReplacementPlanViewModel
    {
        public Guid Id { get; }
        public string Content { get; }
        public IEnumerable<FootnoteFilterReplacementViewModel> Filters { get; }
        public IEnumerable<FootnoteFilterGroupReplacementViewModel> FilterGroups { get; }
        public IEnumerable<FootnoteFilterItemReplacementViewModel> FilterItems { get; }
        public Dictionary<Guid, IndicatorGroupReplacementViewModel> IndicatorGroups { get; }

        public FootnoteReplacementPlanViewModel(
            Guid id,
            string content,
            IEnumerable<FootnoteFilterReplacementViewModel>? filters = null,
            IEnumerable<FootnoteFilterGroupReplacementViewModel>? filterGroups = null,
            IEnumerable<FootnoteFilterItemReplacementViewModel>? filterItems = null,
            Dictionary<Guid, IndicatorGroupReplacementViewModel>? indicatorGroups = null)
        {
            Id = id;
            Content = content;
            Filters = filters ?? new List<FootnoteFilterReplacementViewModel>();
            FilterGroups = filterGroups ?? new List<FootnoteFilterGroupReplacementViewModel>();
            FilterItems = filterItems ?? new List<FootnoteFilterItemReplacementViewModel>();
            IndicatorGroups = indicatorGroups ?? new Dictionary<Guid, IndicatorGroupReplacementViewModel>();
        }

        public bool Valid => Filters.All(model => model.Valid)
                             && FilterGroups.All(model => model.Valid)
                             && FilterItems.All(model => model.Valid)
                             && IndicatorGroups.All(model => model.Value.Valid);

        public FootnoteReplacementPlanViewModel ToSummary()
        {
            return Valid ? new FootnoteReplacementPlanViewModel(Id, Content) : this;
        }
    }

    public class FootnoteFilterReplacementViewModel : TargetableReplacementViewModel
    {
        public FootnoteFilterReplacementViewModel(Guid id, string label, Guid? target) : base(id, label, target)
        {
        }
    }

    public class FootnoteFilterGroupReplacementViewModel : TargetableReplacementViewModel
    {
        public Guid FilterId { get; }
        public string FilterLabel { get; }

        public FootnoteFilterGroupReplacementViewModel(
            Guid id,
            string label,
            Guid? target,
            Guid filterId,
            string filterLabel) : base(
            id,
            label,
            target
        )
        {
            FilterId = filterId;
            FilterLabel = filterLabel;
        }
    }

    public class FootnoteFilterItemReplacementViewModel : TargetableReplacementViewModel
    {
        public Guid FilterGroupId { get; }
        public string FilterGroupLabel { get; }
        public Guid FilterId { get; }
        public string FilterLabel { get; }

        public FootnoteFilterItemReplacementViewModel(
            Guid id,
            string label,
            Guid? target,
            Guid filterGroupId,
            string filterGroupLabel,
            Guid filterId,
            string filterLabel) : base(id, label, target)
        {
            FilterGroupId = filterGroupId;
            FilterGroupLabel = filterGroupLabel;
            FilterId = filterId;
            FilterLabel = filterLabel;
        }
    }

    public class FilterReplacementViewModel : TargetableReplacementViewModel
    {
        public string Name { get; }
        public Dictionary<Guid, FilterGroupReplacementViewModel> Groups { get; }

        public FilterReplacementViewModel(
            Guid id,
            string label,
            Guid? target,
            string name,
            Dictionary<Guid, FilterGroupReplacementViewModel> groups) : base(id, label, target)
        {
            Name = name;
            Groups = groups;
        }
    }

    public class FilterGroupReplacementViewModel : TargetableReplacementViewModel
    {
        public IEnumerable<FilterItemReplacementViewModel> Filters { get; }

        public FilterGroupReplacementViewModel(
            Guid id,
            string label,
            Guid? target,
            IEnumerable<FilterItemReplacementViewModel> filters) : base(id, label, target)
        {
            Filters = filters;
        }
    }

    public class FilterItemReplacementViewModel : TargetableReplacementViewModel
    {
        public FilterItemReplacementViewModel(Guid id, string label, Guid? target) : base(id, label, target)
        {
        }
    }

    public class IndicatorReplacementViewModel : TargetableReplacementViewModel
    {
        public string Name { get; }

        public IndicatorReplacementViewModel(Guid id, string label, Guid? target, string name) : base(id, label, target)
        {
            Name = name;
        }
    }

    public class IndicatorGroupReplacementViewModel
    {
        public Guid Id { get; }
        public string Label { get; }
        public IEnumerable<IndicatorReplacementViewModel> Indicators { get; }

        public bool Valid => Indicators.All(indicator => indicator.Valid);

        public IndicatorGroupReplacementViewModel(
            Guid id,
            string label,
            IEnumerable<IndicatorReplacementViewModel> indicators)
        {
            Id = id;
            Label = label;
            Indicators = indicators;
        }
    }

    public class ObservationalUnitReplacementViewModel
    {
        public string Code { get; }
        public string Label { get; }
        public string Target { get; }

        public bool Valid => !string.IsNullOrEmpty(Target);

        public ObservationalUnitReplacementViewModel(string code, string label, string target)
        {
            Code = code;
            Label = label;
            Target = target;
        }
    }

    public class LocationReplacementViewModel
    {
        public string Label { get; }
        public IEnumerable<ObservationalUnitReplacementViewModel> ObservationalUnits { get; }
        public bool Valid => ObservationalUnits.All(location => location.Valid);

        [JsonIgnore] public bool Any => ObservationalUnits.Any();

        public LocationReplacementViewModel(
            string label,
            IEnumerable<ObservationalUnitReplacementViewModel> observationalUnits)
        {
            Label = label;
            ObservationalUnits = observationalUnits;
        }
    }

    public class TimePeriodRangeReplacementViewModel
    {
        public TimePeriodReplacementViewModel Start { get; }
        public TimePeriodReplacementViewModel End { get; }

        public bool Valid => Start.Valid && End.Valid;

        public TimePeriodRangeReplacementViewModel(
            TimePeriodReplacementViewModel start,
            TimePeriodReplacementViewModel end)
        {
            Start = start;
            End = end;
        }
    }

    public class TimePeriodReplacementViewModel : ReplacementViewModel
    {
        [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
        public TimeIdentifier Code { get; }

        public int Year { get; }
        public string Label => TimePeriodLabelFormatter.Format(Year, Code);

        public TimePeriodReplacementViewModel(bool valid, TimeIdentifier code, int year) : base(valid)
        {
            Code = code;
            Year = year;
        }
    }

    public abstract class TargetableReplacementViewModel
    {
        public Guid Id { get; }
        public string Label { get; }
        public Guid? Target { get; }
        public bool Valid => Target.HasValue;

        [JsonIgnore]
        public Guid TargetValue
        {
            get
            {
                if (!Target.HasValue)
                {
                    throw new InvalidOperationException($"{nameof(Target)} does not have a value");
                }

                return Target.Value;
            }
        }

        protected TargetableReplacementViewModel(Guid id, string label, Guid? target)
        {
            Id = id;
            Label = label;
            Target = target;
        }
    }

    public abstract class ReplacementViewModel
    {
        public bool Valid { get; }

        protected ReplacementViewModel(bool valid)
        {
            Valid = valid;
        }
    }
}