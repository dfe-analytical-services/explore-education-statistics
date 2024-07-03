#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record LocationMappingUpdateResponseViewModel : MappingUpdateResponse<LocationOptionMapping, MappableLocationOption>
{
    public GeographicLevel Level { get; init; }
}

public record FilterOptionMappingUpdateResponseViewModel : MappingUpdateResponse<FilterOptionMapping, MappableFilterOption>
{
    public string FilterKey { get; init; } = string.Empty;
}

public record BatchLocationMappingUpdatesResponseViewModel : BatchMappingUpdatesResponseViewModel
    <LocationMappingUpdateResponseViewModel, LocationOptionMapping, MappableLocationOption>;

public record BatchFilterOptionMappingUpdatesResponseViewModel : BatchMappingUpdatesResponseViewModel
    <FilterOptionMappingUpdateResponseViewModel, FilterOptionMapping, MappableFilterOption>;

public abstract record MappingUpdateResponse<TMapping, TMappableElement>
    where TMapping : Mapping<TMappableElement>
    where TMappableElement : MappableElement
{
    public string SourceKey { get; init; } = string.Empty;

    public TMapping Mapping { get; init; } = null!;
}

public abstract record BatchMappingUpdatesResponseViewModel<TMappingUpdateResponse, TMapping, TMappableElement>
    where TMappingUpdateResponse : MappingUpdateResponse<TMapping, TMappableElement>
    where TMapping : Mapping<TMappableElement>
    where TMappableElement : MappableElement
{
    public List<TMappingUpdateResponse> Updates { get; init; } = [];
}
