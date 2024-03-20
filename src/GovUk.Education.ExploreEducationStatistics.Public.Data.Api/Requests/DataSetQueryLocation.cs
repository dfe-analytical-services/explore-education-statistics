using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

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
public abstract record DataSetQueryLocation
{
    /// <summary>
    /// The geographic level of the location.
    ///
    /// This should be a valid geographic level code e.g. `NAT`, `REG`, `LA`.
    /// </summary>
    public virtual string Level { get; init; } = string.Empty;

    [JsonIgnore]
    public GeographicLevel ParsedLevel => EnumUtil.GetFromEnumValue<GeographicLevel>(Level);

    public abstract string Identifier();

    public static DataSetQueryLocation Parse(string location)
    {
        var parts = location.Split('|');
        var level = parts[0];
        var property = parts[1].ToUpperFirst();
        var value = parts[2];

        if (property == nameof(DataSetQueryLocationId.Id))
        {
            return new DataSetQueryLocationId
            {
                Id = value,
                Level = level
            };
        }

        var parsedLevel = EnumUtil.GetFromEnumValue<GeographicLevel>(parts[0]);

        return parsedLevel switch
        {
            GeographicLevel.LocalAuthority => property switch
            {
                nameof(DataSetQueryLocationLocalAuthority.Code) =>
                    new DataSetQueryLocationLocalAuthority { Code = value },

                nameof(DataSetQueryLocationLocalAuthority.OldCode) =>
                    new DataSetQueryLocationLocalAuthority { OldCode = value },

                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(location),
                    message: $"Invalid {nameof(GeographicLevel.LocalAuthority)} property")
            },
            GeographicLevel.Provider => property switch
            {
                nameof(DataSetQueryLocationProvider.Ukprn) =>
                    new DataSetQueryLocationProvider { Ukprn = value },

                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(location),
                    message: $"Invalid {nameof(GeographicLevel.Provider)} property")
            },
            GeographicLevel.School => property switch
            {
                nameof(DataSetQueryLocationSchool.Urn) =>
                    new DataSetQueryLocationSchool { Urn = value },

                nameof(DataSetQueryLocationSchool) =>
                    new DataSetQueryLocationSchool { LaEstab = value },

                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(location),
                    message: $"Invalid {nameof(GeographicLevel.School)} property")
            },
            _ => property switch
            {
                nameof(DataSetQueryLocationCode.Code) =>
                    new DataSetQueryLocationCode { Code = value, Level = value },

                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(location),
                    message: "Invalid location property")
            }
        };
    }
}

public record DataSetQueryLocationId : DataSetQueryLocation
{
    /// <summary>
    /// The ID of the location. If specified, this will be used
    /// instead of any codes or other types of identifier.
    ///
    /// Every ID is unique to a single location, unlike codes
    /// which may correspond to multiple locations.
    /// </summary>
    public required string Id { get; init; }

    public override required string Level { get; init; }

    public override string Identifier() => Id;
}

public record DataSetQueryLocationCode : DataSetQueryLocation
{
    /// <summary>
    /// The code of the location. This may be an ONS code, or some
    /// other code that identifies the location.
    ///
    /// Note that codes are not necessarily unique to a single location.
    /// Specify the location ID to ensure only a single location is matched.
    /// </summary>
    public required string Code { get; init; }

    public override required string Level { get; init; }

    public override string Identifier() => Code;
}

public record DataSetQueryLocationLocalAuthority : DataSetQueryLocation
{
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

    public override string Level => GeographicLevel.LocalAuthority.GetEnumValue();

    public override string Identifier()
        => Code
           ?? OldCode
           ?? throw new NullReferenceException($"{nameof(Code)} or {nameof(OldCode)} must not be null");
}

public record DataSetQueryLocationProvider : DataSetQueryLocation
{
    /// <summary>
    /// The UKPRN (UK provider reference number) of the provider.
    /// This should be an 8 digit number.
    /// </summary>
    public required string Ukprn { get; init; }

    public override string Level => GeographicLevel.Provider.GetEnumValue();

    public override string Identifier() => Ukprn;
}

public record DataSetQueryLocationSchool : DataSetQueryLocation
{
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

    public override string Level => GeographicLevel.School.GetEnumValue();

    public override string Identifier()
        => Urn
           ?? LaEstab
           ?? throw new NullReferenceException($"{nameof(Urn)} or {nameof(LaEstab)} must not be null");
}
