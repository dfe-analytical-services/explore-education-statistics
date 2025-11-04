#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record LocationMappingUpdateResponseViewModel
    : MappingUpdateResponseViewModel<LocationOptionMapping, MappableLocationOption>
{
    [JsonConverter(typeof(StringEnumConverter))]
    public GeographicLevel Level { get; init; }
}

public record FilterOptionMappingUpdateResponseViewModel
    : MappingUpdateResponseViewModel<FilterOptionMapping, MappableFilterOption>
{
    public string FilterKey { get; init; } = string.Empty;
}

public record BatchLocationMappingUpdatesResponseViewModel
    : BatchMappingUpdatesResponseViewModel<
        LocationMappingUpdateResponseViewModel,
        LocationOptionMapping,
        MappableLocationOption
    >;

public record BatchFilterOptionMappingUpdatesResponseViewModel
    : BatchMappingUpdatesResponseViewModel<
        FilterOptionMappingUpdateResponseViewModel,
        FilterOptionMapping,
        MappableFilterOption
    >;

public abstract record MappingUpdateResponseViewModel<TMapping, TMappableElement>
    where TMapping : Mapping<TMappableElement>
    where TMappableElement : MappableElement
{
    public string SourceKey { get; init; } = string.Empty;

    public TMapping Mapping { get; init; } = null!;
}

public abstract record BatchMappingUpdatesResponseViewModel<TMappingUpdateResponse, TMapping, TMappableElement>
    where TMappingUpdateResponse : MappingUpdateResponseViewModel<TMapping, TMappableElement>
    where TMapping : Mapping<TMappableElement>
    where TMappableElement : MappableElement
{
    public List<TMappingUpdateResponse> Updates { get; init; } = [];
}
