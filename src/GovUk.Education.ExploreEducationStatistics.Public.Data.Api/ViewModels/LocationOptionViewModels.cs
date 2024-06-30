using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;

/// <summary>
/// A location option that can be used to filter a data set.
/// </summary>
[JsonConverter(typeof(LocationOptionViewModelJsonConverter))]
public abstract record LocationOptionViewModel
{
    /// <summary>
    /// The ID of the location.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// The human-readable label of the location.
    /// </summary>
    public required string Label { get; init; }
}

/// <summary>
/// A location option that can be identified by code and used to filter a data set.
/// </summary>
public record LocationCodedOptionViewModel : LocationOptionViewModel
{
    /// <summary>
    /// The code of the location.
    /// </summary>
    public required string Code { get; init; }
}

/// <summary>
/// A location option for a local authority that can be used to filter a data set.
/// </summary>
public record LocationLocalAuthorityOptionViewModel : LocationOptionViewModel
{
    /// <summary>
    /// The ONS code of the local authority.
    /// </summary>
    public required string Code { get; init; }

    /// <summary>
    /// The old code (previously the LEA code) of the local authority.
    /// </summary>
    public required string OldCode { get; init; }
}

/// <summary>
/// A location option for a provider that can be used to filter a data set.
/// </summary>
public record LocationProviderOptionViewModel : LocationOptionViewModel
{
    /// <summary>
    /// The UKPRN (UK provider reference number) of the provider.
    /// </summary>
    public required string Ukprn { get; init; }
}

/// <summary>
/// A location option for an RSC region that can be used to filter a data set.
/// </summary>
public record LocationRscRegionOptionViewModel : LocationOptionViewModel;

/// <summary>
/// A location option for a school that can be used to filter a data set.
/// </summary>
public record LocationSchoolOptionViewModel : LocationOptionViewModel
{
    /// <summary>
    /// The URN (unique reference number) of the school.
    /// </summary>
    public required string Urn { get; init; }

    /// <summary>
    /// The LAESTAB (local authority establishment number) of the school.
    /// </summary>
    public required string LaEstab { get; init; }
}
