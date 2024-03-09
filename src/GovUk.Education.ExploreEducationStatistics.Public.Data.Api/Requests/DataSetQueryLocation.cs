using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

/// <summary>
/// A location to filter results by.
///
/// Locations can be specified by an ID or one of its codes (depending
/// on the location type). Note that whilst most codes are usually unique
/// to a single location, they may also be used for multiple locations.
///
/// When using codes, you may get more results than expected so it's recommended
/// to use IDs where possible to ensure only a single location is matched.
/// </summary>
public record DataSetQueryLocation
{
    /// <summary>
    /// The ID of the location. If specified, this will be used
    /// instead of any codes or other types of identifier.
    ///
    /// Every ID is unique to a single location, unlike codes
    /// which may correspond to multiple locations.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// The geographic level of the location.
    /// </summary>
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public GeographicLevel Level { get; init; }
}

public record DataSetQueryLocationCoded : DataSetQueryLocation
{
    /// <summary>
    /// The code of the location. This may be an ONS code, or some
    /// other code that identifies the location.
    ///
    /// Note that codes are not necessarily unique to a single location.
    /// Specify the location ID to ensure only a single location is matched.
    /// </summary>
    public string? Code { get; init; }
}

public record DataSetQueryLocationLocalAuthority : DataSetQueryLocation
{
    public new GeographicLevel Level => GeographicLevel.LocalAuthority;

    /// <summary>
    /// The ONS code of the local authority. This should be 9 characters
    /// in the standard ONS format for local authorities (e.g. `E08000019`).
    /// It can be a combination of two codes (e.g. `E09000021 / E09000027`).
    /// </summary>
    public string? Code { get; init; }

    /// <summary>
    /// The old code (previously the LEA code) of the local authority.
    /// This should be a 3 digit number (e.g. `318`) or be
    /// a combination of two codes (e.g. `314 / 318`).
    /// </summary>
    public string? OldCode { get; init; }
}

public record DataSetQueryLocationProvider : DataSetQueryLocation
{
    public new GeographicLevel Level => GeographicLevel.Provider;

    /// <summary>
    /// The UKPRN (UK provider reference number) of the provider.
    /// This should be an 8 digit number.
    /// </summary>
    public string? Ukprn { get; init; }
}

public record DataSetQueryLocationRscRegion : DataSetQueryLocation
{
    public new GeographicLevel Level => GeographicLevel.RscRegion;

    /// <summary>
    /// The ID of the RSC region.
    /// </summary>
    public new required string Id { get; init; }
}

public record DataSetQueryLocationSchool : DataSetQueryLocation
{
    public new GeographicLevel Level => GeographicLevel.School;

    /// <summary>
    /// The URN (unique reference number) of the school.
    /// This should be a 6 digit number.
    /// </summary>
    public string? Urn { get; init; }

    /// <summary>
    /// The LAESTAB (local authority establishment number) of the school.
    /// This should be a 7 digit number.
    /// </summary>
    public string? LaEstab { get; init; }
}
