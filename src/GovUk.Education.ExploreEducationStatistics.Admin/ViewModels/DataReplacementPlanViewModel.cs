#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class DataReplacementPlanViewModel
{
    public IEnumerable<DataBlockReplacementPlanViewModel> DataBlocks { get; init; } = [];
    public IEnumerable<FootnoteReplacementPlanViewModel> Footnotes { get; init; } = [];
    public ReplaceApiDataSetVersionPlanViewModel? ApiDataSetVersionPlan { get; init; }
    public ReplacementPlanMappingViewModel Mapping { get; init; } = new();

    // If the replacement introduces additional filters, existing data blocks become problematic: the data blocks won't
    // contain filter items for the new filters, and will appear valid, but will return no results after the
    // replacement.
    // If the replacement removes filters, then existing data blocks referencing the removed filters' items will be
    // considered invalid and prevent the replacement, so we don't need to handle that case.
    public bool HasDataBlockAndReplacementHasAdditionalFilter =>
        DataBlocks.Any() && Mapping.Filters.Mappings.Count < Mapping.Filters.Candidates.Count;
    public Guid OriginalSubjectId { get; init; }
    public Guid ReplacementSubjectId { get; init; }

    public bool Valid =>
        !HasDataBlockAndReplacementHasAdditionalFilter
        && DataBlocks.All(info => info.Valid)
        && Footnotes.All(info => info.Valid)
        && (ApiDataSetVersionPlan?.Valid ?? true);

    // Trimmed down version of the data replacement plan that
    // only shows full replacement details for invalid items.
    public DataReplacementPlanViewModel ToSummary()
    {
        return new DataReplacementPlanViewModel
        {
            DataBlocks = DataBlocks.Select(block => block.ToSummary()),
            Footnotes = Footnotes.Select(footnote => footnote.ToSummary()),
            ApiDataSetVersionPlan = ApiDataSetVersionPlan,
            OriginalSubjectId = OriginalSubjectId,
            ReplacementSubjectId = ReplacementSubjectId,
            Mapping = Mapping,
        };
    }
}

public class DataBlockReplacementPlanViewModel(
    Guid id,
    string name,
    Dictionary<Guid, FilterReplacementViewModel>? filters = null,
    Dictionary<Guid, IndicatorGroupReplacementViewModel>? indicators = null,
    Dictionary<string, LocationReplacementViewModel>? locations = null,
    TimePeriodRangeReplacementViewModel? timePeriods = null
)
{
    public Guid Id { get; } = id;
    public string Name { get; } = name;
    public Dictionary<Guid, FilterReplacementViewModel> Filters { get; } =
        filters ?? new Dictionary<Guid, FilterReplacementViewModel>();
    public Dictionary<Guid, IndicatorGroupReplacementViewModel> IndicatorGroups { get; } =
        indicators ?? new Dictionary<Guid, IndicatorGroupReplacementViewModel>();
    public Dictionary<string, LocationReplacementViewModel> Locations { get; } =
        locations ?? new Dictionary<string, LocationReplacementViewModel>(); // Key is GeographicLevel
    public TimePeriodRangeReplacementViewModel? TimePeriods { get; } = timePeriods;

    public bool Valid =>
        Filters.All(model => model.Value.Valid)
        && IndicatorGroups.All(model => model.Value.Valid)
        && Locations.Values.All(model => model.Valid)
        && (TimePeriods?.Valid ?? true);

    public DataBlockReplacementPlanViewModel ToSummary()
    {
        return Valid ? new DataBlockReplacementPlanViewModel(Id, Name) : this;
    }
}

public class FootnoteReplacementPlanViewModel(
    Guid id,
    string content,
    IEnumerable<FootnoteFilterReplacementViewModel>? filters = null,
    IEnumerable<FootnoteFilterGroupReplacementViewModel>? filterGroups = null,
    IEnumerable<FootnoteFilterItemReplacementViewModel>? filterItems = null,
    Dictionary<Guid, IndicatorGroupReplacementViewModel>? indicatorGroups = null
)
{
    public Guid Id { get; } = id;
    public string Content { get; } = content;
    public IEnumerable<FootnoteFilterReplacementViewModel> Filters { get; } =
        filters ?? new List<FootnoteFilterReplacementViewModel>();
    public IEnumerable<FootnoteFilterGroupReplacementViewModel> FilterGroups { get; } =
        filterGroups ?? new List<FootnoteFilterGroupReplacementViewModel>();
    public IEnumerable<FootnoteFilterItemReplacementViewModel> FilterItems { get; } =
        filterItems ?? new List<FootnoteFilterItemReplacementViewModel>();
    public Dictionary<Guid, IndicatorGroupReplacementViewModel> IndicatorGroups { get; } =
        indicatorGroups ?? new Dictionary<Guid, IndicatorGroupReplacementViewModel>();

    public bool Valid =>
        Filters.All(model => model.Valid)
        && FilterGroups.All(model => model.Valid)
        && FilterItems.All(model => model.Valid)
        && IndicatorGroups.All(model => model.Value.Valid);

    public FootnoteReplacementPlanViewModel ToSummary()
    {
        return Valid ? new FootnoteReplacementPlanViewModel(Id, Content) : this;
    }
}

public class FootnoteFilterReplacementViewModel(Guid id, string label, Guid? target)
    : TargetableReplacementViewModel(id, label, target);

public class FootnoteFilterGroupReplacementViewModel(
    Guid id,
    string label,
    Guid? target,
    Guid filterId,
    string filterLabel
) : TargetableReplacementViewModel(id, label, target)
{
    public Guid FilterId { get; } = filterId;
    public string FilterLabel { get; } = filterLabel;
}

public class FootnoteFilterItemReplacementViewModel(
    Guid id,
    string label,
    Guid? target,
    Guid filterGroupId,
    string filterGroupLabel,
    Guid filterId,
    string filterLabel
) : TargetableReplacementViewModel(id, label, target)
{
    public Guid FilterGroupId { get; } = filterGroupId;
    public string FilterGroupLabel { get; } = filterGroupLabel;
    public Guid FilterId { get; } = filterId;
    public string FilterLabel { get; } = filterLabel;
}

public class FilterReplacementViewModel(
    Guid id,
    Guid? target,
    string label,
    string name,
    Dictionary<Guid, FilterGroupReplacementViewModel> groups
)
{
    public Guid Id { get; } = id;
    public Guid? Target { get; } = target;
    public string Label { get; } = label;
    public string Name { get; } = name;
    public Dictionary<Guid, FilterGroupReplacementViewModel> Groups { get; } = groups;

    public bool Valid => Target.HasValue && Groups.All(group => group.Value.Valid);
}

public class FilterGroupReplacementViewModel(
    Guid id,
    string label,
    Guid? target,
    IEnumerable<FilterItemReplacementViewModel> items
)
{
    public Guid Id { get; } = id;
    public string Label { get; } = label;
    public Guid? Target { get; } = target;
    public IEnumerable<FilterItemReplacementViewModel> Items { get; } = items;

    public bool Valid => Target.HasValue && Items.All(item => item.Valid);
}

public class FilterItemReplacementViewModel(Guid id, string label, Guid? target)
    : TargetableReplacementViewModel(id, label, target);

public class IndicatorReplacementViewModel(Guid id, string label, Guid? target, string name)
    : TargetableReplacementViewModel(id, label, target)
{
    public string Name { get; } = name; // csv column name
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
        IEnumerable<IndicatorReplacementViewModel> indicators
    )
    {
        Id = id;
        Label = label;
        Indicators = indicators;
    }
}

public class LocationAttributeReplacementViewModel : TargetableReplacementViewModel
{
    public string Code { get; }

    public LocationAttributeReplacementViewModel(Guid id, string code, string label, Guid? target)
        : base(id, label, target)
    {
        Code = code;
    }
}

public class LocationReplacementViewModel
{
    public string Label { get; }
    public IEnumerable<LocationAttributeReplacementViewModel> LocationAttributes { get; }
    public bool Valid => LocationAttributes.All(location => location.Valid);

    public LocationReplacementViewModel(
        string label,
        IEnumerable<LocationAttributeReplacementViewModel> locationAttributes
    )
    {
        Label = label;
        LocationAttributes = locationAttributes;
    }
}

public class TimePeriodRangeReplacementViewModel
{
    public TimePeriodReplacementViewModel Start { get; }
    public TimePeriodReplacementViewModel End { get; }

    public bool Valid => Start.Valid && End.Valid;

    public TimePeriodRangeReplacementViewModel(TimePeriodReplacementViewModel start, TimePeriodReplacementViewModel end)
    {
        Start = start;
        End = end;
    }
}

public class TimePeriodReplacementViewModel : ReplacementViewModel
{
    [JsonConverter(typeof(StringEnumConverter))]
    public TimeIdentifier Code { get; }

    public int Year { get; }
    public string Label => TimePeriodLabelFormatter.Format(Year, Code);

    public TimePeriodReplacementViewModel(bool valid, TimeIdentifier code, int year)
        : base(valid)
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

public record ReplacementPlanMappingViewModel
{
    public ReplacementPlanIndicatorMappingsViewModel Indicators { get; init; } = null!;

    public ReplacementPlanLocationMappingsViewModel Locations { get; init; } = null!;

    public ReplacementPlanFilterMappingsViewModel Filters { get; init; } = null!;

    public static ReplacementPlanMappingViewModel FromModel(DataSetMapping mapping)
    {
        var indicatorMappings = ReplacementPlanIndicatorMappingsViewModel.FromModel(mapping);
        var locationMappings = ReplacementPlanLocationMappingsViewModel.FromModel(mapping);
        var filterMappings = ReplacementPlanFilterMappingsViewModel.FromModel(mapping);

        return new ReplacementPlanMappingViewModel
        {
            Indicators = indicatorMappings,
            Locations = locationMappings,
            Filters = filterMappings,
        };
    }
}

public record ReplacementPlanIndicatorMappingsViewModel
{
    // Key is original indicator id
    public Dictionary<Guid, ReplacementPlanIndicatorMappingViewModel> Mappings { get; init; } = null!;

    // Key is replacement indicator id
    public Dictionary<Guid, ReplacementPlanIndicatorViewModel> Candidates { get; init; } = null!;

    public static ReplacementPlanIndicatorMappingsViewModel FromModel(DataSetMapping mapping)
    {
        var indicatorMappings = mapping.IndicatorMappings.Values.ToDictionary(
            indicatorMap => indicatorMap.OriginalId,
            indicatorMap => new ReplacementPlanIndicatorMappingViewModel
            {
                Source = new ReplacementPlanIndicatorViewModel
                {
                    Id = indicatorMap.OriginalId,
                    Name = indicatorMap.OriginalColumnName,
                    Label = indicatorMap.OriginalLabel,
                },
                Type = indicatorMap.Status,
                CandidateKey = indicatorMap.ReplacementId,
            }
        );
        var indicatorCandidates = mapping // candidates are all possible replacement indicators
            .IndicatorMappings.Values.Where(indicatorMap => indicatorMap.ReplacementId != null)
            .Select(indicatorMap => new
            {
                Id = indicatorMap.ReplacementId!.Value,
                ColumnName = indicatorMap.ReplacementColumnName!,
                Label = indicatorMap.ReplacementLabel!,
            })
            .Concat(
                mapping.UnmappedReplacementIndicators.Select(unmappedIndicator => new
                {
                    unmappedIndicator.Id,
                    unmappedIndicator.ColumnName,
                    unmappedIndicator.Label,
                })
            )
            .ToDictionary(
                x => x.Id,
                x => new ReplacementPlanIndicatorViewModel
                {
                    Id = x.Id,
                    Name = x.ColumnName,
                    Label = x.Label,
                }
            );
        return new ReplacementPlanIndicatorMappingsViewModel
        {
            Mappings = indicatorMappings,
            Candidates = indicatorCandidates,
        };
    }
}

public record ReplacementPlanIndicatorMappingViewModel
{
    public ReplacementPlanIndicatorViewModel Source { get; init; } = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public MapStatus Type { get; init; }
    public Guid? CandidateKey { get; init; } // replacement indicator id
}

public record ReplacementPlanIndicatorViewModel
{
    public Guid Id { get; init; }
    public string Name { get; init; } = ""; // csv column name
    public string Label { get; init; } = "";
}

public record ReplacementPlanLocationMappingsViewModel
{
    // Key is original location id
    public Dictionary<Guid, ReplacementPlanLocationMappingViewModel> Mappings { get; init; } = new();

    // Key is replacement location id
    public Dictionary<Guid, ReplacementPlanLocationViewModel> Candidates { get; init; } = new();

    public static ReplacementPlanLocationMappingsViewModel FromModel(DataSetMapping mapping)
    {
        var locationMappings = mapping.LocationMappings.Values.ToDictionary(
            locationMap => locationMap.OriginalId,
            locationMap => new ReplacementPlanLocationMappingViewModel
            {
                Source = new ReplacementPlanLocationViewModel
                {
                    Id = locationMap.OriginalId,
                    Code = locationMap.OriginalCode,
                    Name = locationMap.OriginalName,
                },
                Type = locationMap.Status,
                CandidateKey = locationMap.ReplacementId,
            }
        );
        var locationCandidates = mapping // candidates are all possible replacement locations
            .LocationMappings.Values.Where(locationMap => locationMap.ReplacementId != null)
            .Select(locationMap => new
            {
                Id = locationMap.ReplacementId!.Value,
                Code = locationMap.ReplacementCode!,
                Name = locationMap.ReplacementName!,
            })
            .Concat(
                mapping.UnmappedReplacementLocations.Select(unmappedLocation => new
                {
                    unmappedLocation.Id,
                    unmappedLocation.Code,
                    unmappedLocation.Name,
                })
            )
            .ToDictionary(
                x => x.Id,
                x => new ReplacementPlanLocationViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                }
            );

        return new ReplacementPlanLocationMappingsViewModel
        {
            Mappings = locationMappings,
            Candidates = locationCandidates,
        };
    }
}

public record ReplacementPlanLocationMappingViewModel
{
    public ReplacementPlanLocationViewModel Source { get; init; } = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public MapStatus Type { get; init; }

    public Guid? CandidateKey { get; init; } // replacement location id
}

public record ReplacementPlanLocationViewModel
{
    public Guid Id { get; init; }
    public string Code { get; init; } = "";
    public string Name { get; init; } = "";
}

public record ReplacementPlanFilterMappingsViewModel
{
    // Key is original filter id
    public Dictionary<Guid, ReplacementPlanFilterMappingViewModel> Mappings { get; init; } = new();

    // Key is replacement filter id
    public Dictionary<Guid, ReplacementPlanFilterViewModel> Candidates { get; init; } = new();

    public static ReplacementPlanFilterMappingsViewModel FromModel(DataSetMapping mapping)
    {
        var filterMappings = mapping.FilterMappings.Values.ToDictionary(
            filterMap => filterMap.OriginalId,
            filterMap => new ReplacementPlanFilterMappingViewModel
            {
                Source = new ReplacementPlanFilterViewModel
                {
                    Id = filterMap.OriginalId,
                    Name = filterMap.OriginalColumnName,
                    Label = filterMap.OriginalLabel,
                },
                CandidateKey = filterMap.ReplacementId,
                Type = filterMap.Status,
                FilterGroups = ReplacementPlanFilterGroupMappingsViewModel.FromModel(mapping, filterMap),
            }
        );
        // candidates are the complete replacement-side catalogue, whether currently claimed by a mapping or not
        var filterCandidates = mapping.ReplacementFilters.ToDictionary(
            replacementFilter => replacementFilter.Id,
            replacementFilter => new ReplacementPlanFilterViewModel
            {
                Id = replacementFilter.Id,
                Label = replacementFilter.Label,
                Name = replacementFilter.ColumnName,
            }
        );

        return new ReplacementPlanFilterMappingsViewModel { Mappings = filterMappings, Candidates = filterCandidates };
    }
}

public record ReplacementPlanFilterMappingViewModel
{
    public ReplacementPlanFilterViewModel Source { get; init; } = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public MapStatus Type { get; init; }

    public Guid? CandidateKey { get; init; } // replacement filter id

    public ReplacementPlanFilterGroupMappingsViewModel FilterGroups { get; init; } = null!;
}

public record ReplacementPlanFilterViewModel
{
    public Guid Id { get; init; }
    public string Label { get; init; } = "";
    public string Name { get; init; } = ""; // csv column name
}

public record ReplacementPlanFilterGroupMappingsViewModel
{
    // Key is original filter group id
    public Dictionary<Guid, ReplacementPlanFilterGroupMappingViewModel> Mappings { get; init; } = new();

    // Key is replacement filter group id
    public Dictionary<Guid, ReplacementPlanFilterGroupViewModel> Candidates { get; init; } = new();

    public static ReplacementPlanFilterGroupMappingsViewModel FromModel(
        DataSetMapping mapping,
        FilterMapping filterMapping
    )
    {
        var filterGroupMappings = filterMapping.FilterGroupMappings.Values.ToDictionary(
            groupMap => groupMap.OriginalId,
            groupMap => new ReplacementPlanFilterGroupMappingViewModel
            {
                Source = new ReplacementPlanFilterGroupViewModel
                {
                    Id = groupMap.OriginalId,
                    Label = groupMap.OriginalLabel,
                },
                CandidateKey = groupMap.ReplacementId,
                Type = groupMap.Status,
                FilterItems = ReplacementPlanFilterItemMappingsViewModel.FromModel(mapping, groupMap),
            }
        );
        // candidates are the replacement filter's own children, whether currently claimed by a mapping or not
        var filterGroupCandidates = mapping
            .ReplacementFilterGroups.Where(replacementGroup => replacementGroup.FilterId == filterMapping.ReplacementId)
            .ToDictionary(
                replacementGroup => replacementGroup.Id,
                replacementGroup => new ReplacementPlanFilterGroupViewModel
                {
                    Id = replacementGroup.Id,
                    Label = replacementGroup.Label,
                }
            );

        return new ReplacementPlanFilterGroupMappingsViewModel
        {
            Mappings = filterGroupMappings,
            Candidates = filterGroupCandidates,
        };
    }
}

public record ReplacementPlanFilterGroupMappingViewModel
{
    public ReplacementPlanFilterGroupViewModel Source { get; init; } = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public MapStatus Type { get; init; }

    public Guid? CandidateKey { get; init; } // replacement filter group id

    public ReplacementPlanFilterItemMappingsViewModel FilterItems { get; init; } = null!;
}

public record ReplacementPlanFilterGroupViewModel
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
}

public record ReplacementPlanFilterItemMappingsViewModel
{
    // Key is original filter item id
    public Dictionary<Guid, ReplacementPlanFilterItemMappingViewModel> Mappings { get; init; } = new();

    // Key is replacement filter item id
    public Dictionary<Guid, ReplacementPlanFilterItemViewModel> Candidates { get; init; } = new();

    public static ReplacementPlanFilterItemMappingsViewModel FromModel(
        DataSetMapping mapping,
        FilterGroupMapping groupMapping
    )
    {
        var itemMappings = groupMapping.FilterItemMappings.Values.ToDictionary(
            itemMap => itemMap.OriginalId,
            itemMap => new ReplacementPlanFilterItemMappingViewModel
            {
                Source = new ReplacementPlanFilterItemViewModel
                {
                    Id = itemMap.OriginalId,
                    Label = itemMap.OriginalLabel,
                },
                CandidateKey = itemMap.ReplacementId,
                Type = itemMap.Status,
            }
        );
        // candidates are the replacement group's own children, whether currently claimed by a mapping or not
        var itemCandidates = mapping
            .ReplacementFilterItems.Where(replacementItem =>
                replacementItem.FilterGroupId == groupMapping.ReplacementId
            )
            .ToDictionary(
                replacementItem => replacementItem.Id,
                replacementItem => new ReplacementPlanFilterItemViewModel
                {
                    Id = replacementItem.Id,
                    Label = replacementItem.Label,
                }
            );

        return new ReplacementPlanFilterItemMappingsViewModel { Mappings = itemMappings, Candidates = itemCandidates };
    }
}

public record ReplacementPlanFilterItemMappingViewModel
{
    public ReplacementPlanFilterItemViewModel Source { get; init; } = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public MapStatus Type { get; init; }

    public Guid? CandidateKey { get; init; } // replacement filter item id
}

public record ReplacementPlanFilterItemViewModel
{
    public Guid Id { get; set; }
    public string Label { get; set; } = "";
}
