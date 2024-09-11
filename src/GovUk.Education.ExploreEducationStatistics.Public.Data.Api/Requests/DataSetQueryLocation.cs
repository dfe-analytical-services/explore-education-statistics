using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests.Converters;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Swashbuckle.AspNetCore.Annotations;

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
[JsonConverter(typeof(DataSetQueryLocationJsonConverter))]
[SwaggerSubType(typeof(DataSetQueryLocationId))]
[SwaggerSubType(typeof(DataSetQueryLocationCode))]
[SwaggerSubType(typeof(DataSetQueryLocationLocalAuthorityCode))]
[SwaggerSubType(typeof(DataSetQueryLocationLocalAuthorityOldCode))]
[SwaggerSubType(typeof(DataSetQueryLocationProviderUkprn))]
[SwaggerSubType(typeof(DataSetQueryLocationSchoolLaEstab))]
[SwaggerSubType(typeof(DataSetQueryLocationSchoolUrn))]
public abstract record DataSetQueryLocation
{
    /// <summary>
    /// The geographic level of the location.
    ///
    /// This should be a valid geographic level code e.g. `NAT`, `REG`, `LA`.
    /// </summary>
    /// <example>NAT</example>
    [SwaggerEnum(typeof(GeographicLevel))]
    public virtual string Level { get; protected init; } = string.Empty;

    public GeographicLevel ParsedLevel() => EnumUtil.GetFromEnumValue<GeographicLevel>(Level);

    /// <summary>
    /// The name of the key property for this location.
    /// </summary>
    [JsonIgnore]
    public abstract string KeyProperty { get; }

    /// <summary>
    /// The value of the key property for this location.
    /// </summary>
    [JsonIgnore]
    public abstract string KeyValue { get; }

    public string ToLocationString() => $"{Level}|{KeyProperty.ToLowerFirst()}|{KeyValue}";

    public static DataSetQueryLocation Parse(string location)
    {
        var parts = location.Split('|');
        var level = parts[0];
        var property = parts[1].ToUpperFirst();
        var value = parts[2];

        var parsedLevel = EnumUtil.GetFromEnumValue<GeographicLevel>(level);

        if (property == nameof(DataSetQueryLocationId.Id))
        {
            return new DataSetQueryLocationId
            {
                Id = value,
                Level = level
            };
        }

        return parsedLevel switch
        {
            GeographicLevel.LocalAuthority => property switch
            {
                nameof(DataSetQueryLocationLocalAuthorityCode.Code) =>
                    new DataSetQueryLocationLocalAuthorityCode { Code = value },

                nameof(DataSetQueryLocationLocalAuthorityOldCode.OldCode) =>
                    new DataSetQueryLocationLocalAuthorityOldCode { OldCode = value },

                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(location),
                    message: $"Invalid {nameof(GeographicLevel.LocalAuthority)} property")
            },
            GeographicLevel.Provider => property switch
            {
                nameof(DataSetQueryLocationProviderUkprn.Ukprn) =>
                    new DataSetQueryLocationProviderUkprn { Ukprn = value },

                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(location),
                    message: $"Invalid {nameof(GeographicLevel.Provider)} property")
            },
            GeographicLevel.School => property switch
            {
                nameof(DataSetQueryLocationSchoolUrn.Urn) =>
                    new DataSetQueryLocationSchoolUrn { Urn = value },

                nameof(DataSetQueryLocationSchoolLaEstab.LaEstab) =>
                    new DataSetQueryLocationSchoolLaEstab { LaEstab = value },

                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(location),
                    message: $"Invalid {nameof(GeographicLevel.School)} property")
            },
            _ => property switch
            {
                nameof(DataSetQueryLocationCode.Code) =>
                    new DataSetQueryLocationCode { Code = value, Level = level },

                _ => throw new ArgumentOutOfRangeException(
                    paramName: nameof(location),
                    message: "Invalid location property")
            }
        };
    }

    public abstract IValidator CreateValidator();

    public class BaseValidator<T> : AbstractValidator<T> where T : DataSetQueryLocation
    {
        protected BaseValidator()
        {
            RuleFor(l => l.Level)
                .NotEmpty()
                .AllowedValue(GeographicLevelUtils.OrderedCodes);
        }
    }
}

/// <summary>
/// A location (identified by an ID) to filter results by.
///
/// Note that location IDs are guaranteed to be unique to a single location
/// unlike location codes (which may correspond to multiple locations).
/// </summary>
public record DataSetQueryLocationId : DataSetQueryLocation
{
    /// <summary>
    /// The ID of the location. If specified, this will be used
    /// instead of any codes or other types of identifier.
    /// </summary>
    /// <example>2tYX</example>
    public required string Id { get; init; }

    /// <summary>
    /// The geographic level of the location.
    ///
    /// This should be a valid geographic level code e.g. `NAT`, `REG`, `LA`.
    /// </summary>
    /// <example>NAT</example>
    [SwaggerEnum(typeof(GeographicLevel))]
    public new required string Level
    {
        get => base.Level;
        init => base.Level = value;
    }

    public override string KeyProperty => nameof(Id);

    public override string KeyValue => Id;

    public override IValidator CreateValidator() => new Validator();

    public class Validator : BaseValidator<DataSetQueryLocationId>
    {
        public Validator()
        {
            RuleFor(l => l.Id)
                .NotEmpty()
                .MaximumLength(10);
        }
    }
}

/// <summary>
/// A location (identified by a code) to filter results by.
///
/// Note that location codes may correspond to multiple locations in the same geographic level.
/// </summary>
public record DataSetQueryLocationCode : DataSetQueryLocation
{
    /// <summary>
    /// The code of the location. This may be an ONS code, or some
    /// other code that identifies the location.
    /// </summary>
    /// <example>E12000003</example>
    public required string Code { get; init; }

    /// <summary>
    /// The geographic level of the location.
    ///
    /// This should be a valid geographic level code e.g. `NAT`, `REG`, `LA`.
    /// </summary>
    /// <example>NAT</example>
    [SwaggerEnum(typeof(GeographicLevel))]
    public new required string Level
    {
        get => base.Level;
        init => base.Level = value;
    }

    public override string KeyProperty => nameof(Code);

    public override string KeyValue => Code;

    public override IValidator CreateValidator() => new Validator();

    public class Validator : BaseValidator<DataSetQueryLocationCode>
    {
        public Validator()
        {
            RuleFor(l => l.Code)
                .NotEmpty()
                .MaximumLength(25);
        }
    }
}

/// <summary>
/// A local authority (identified by its ONS code) to filter results by.
/// </summary>
public record DataSetQueryLocationLocalAuthorityCode : DataSetQueryLocation
{
    /// <summary>
    /// The ONS code of the local authority. This should be 9 characters
    /// in the standard ONS format for local authorities (e.g. `E08000019`).
    /// It can be a combination of two codes (e.g. `E09000021 / E09000027`).
    /// </summary>
    /// <example>E08000019</example>
    public required string Code { get; init; }

    /// <summary>
    /// The geographic level of the local authority. Must be set to `LA`.
    /// </summary>
    /// <example>LA</example>
    [Required]
    [SwaggerSchema(ReadOnly = false)]
    public override string Level { get; protected init; } = GeographicLevel.LocalAuthority.GetEnumValue();

    public override string KeyProperty => nameof(Code);

    public override string KeyValue => Code;

    public override IValidator CreateValidator() => new Validator();

    public class Validator : BaseValidator<DataSetQueryLocationLocalAuthorityCode>
    {
        public Validator()
        {
            RuleFor(l => l.Code)
                .NotEmpty()
                .MaximumLength(25);
        }
    }
}

/// <summary>
/// A local authority (identified by its old code) to filter results by.
/// </summary>
public record DataSetQueryLocationLocalAuthorityOldCode : DataSetQueryLocation
{
    /// <summary>
    /// The old code (previously the LEA code) of the local authority.
    /// This should be a 3 digit number (e.g. `318`) or be
    /// a combination of two codes (e.g. `314 / 318`).
    /// </summary>
    /// <example>373</example>
    public required string OldCode { get; init; }

    /// <summary>
    /// The geographic level of the local authority. Must be set to `LA`.
    /// </summary>
    /// <example>LA</example>
    [Required]
    [SwaggerSchema(ReadOnly = false)]
    public override string Level { get; protected init; } = GeographicLevel.LocalAuthority.GetEnumValue();

    public override string KeyProperty => nameof(OldCode);

    public override string KeyValue => OldCode;

    public override IValidator CreateValidator() => new Validator();

    public class Validator : BaseValidator<DataSetQueryLocationLocalAuthorityOldCode>
    {
        public Validator()
        {
            RuleFor(l => l.OldCode)
                .NotEmpty()
                .MaximumLength(10);
        }
    }
}

/// <summary>
/// A provider (identified by its UKPRN) to filter results by.
/// </summary>
public record DataSetQueryLocationProviderUkprn : DataSetQueryLocation
{
    /// <summary>
    /// The UKPRN (UK provider reference number) of the provider.
    /// This should be an 8 digit number.
    /// </summary>
    /// <example>123454678</example>
    public required string Ukprn { get; init; }

    /// <summary>
    /// The geographic level of the provider. Must be set to `PROV`.
    /// </summary>
    /// <example>PROV</example>
    [Required]
    [SwaggerSchema(ReadOnly = false)]
    public override string Level { get; protected init; } = GeographicLevel.Provider.GetEnumValue();

    public override string KeyProperty => nameof(Ukprn);

    public override string KeyValue => Ukprn;

    public override IValidator CreateValidator() => new Validator();

    public class Validator : BaseValidator<DataSetQueryLocationProviderUkprn>
    {
        public Validator()
        {
            RuleFor(l => l.Ukprn)
                .NotEmpty()
                .MaximumLength(8);
        }
    }
}

/// <summary>
/// A school (identified by its URN) to filter results by.
/// </summary>
public record DataSetQueryLocationSchoolUrn : DataSetQueryLocation
{
    /// <summary>
    /// The URN (unique reference number) of the school.
    /// This should be a 6 digit number.
    /// </summary>
    /// <example>123456</example>
    public required string Urn { get; init; }

    /// <summary>
    /// The geographic level of the school. Must be set to `SCH`.
    /// </summary>
    /// <example>SCH</example>
    [Required]
    [SwaggerSchema(ReadOnly = false)]
    public override string Level { get; protected init; } = GeographicLevel.School.GetEnumValue();

    public override string KeyProperty => nameof(Urn);

    public override string KeyValue => Urn;

    public override IValidator CreateValidator() => new Validator();

    public class Validator : BaseValidator<DataSetQueryLocationSchoolUrn>
    {
        public Validator()
        {
            RuleFor(l => l.Urn)
                .NotEmpty()
                .MaximumLength(6);
        }
    }
}

/// <summary>
/// A school (identified by its LAESTAB) to filter results by.
/// </summary>
public record DataSetQueryLocationSchoolLaEstab : DataSetQueryLocation
{
    /// <summary>
    /// The LAESTAB (local authority establishment number) of the school.
    /// This should be a 7 digit number.
    /// </summary>
    /// <example>1234567</example>
    public required string LaEstab { get; init; }

    /// <summary>
    /// The geographic level of the location. Must be set to `SCH`.
    /// </summary>
    /// <example>SCH</example>
    [Required]
    [SwaggerSchema(ReadOnly = false)]
    public override string Level { get; protected init; } = GeographicLevel.School.GetEnumValue();

    public override string KeyProperty => nameof(LaEstab);

    public override string KeyValue => LaEstab;

    public override IValidator CreateValidator() => new Validator();

    public class Validator : BaseValidator<DataSetQueryLocationSchoolLaEstab>
    {
        public Validator()
        {
            RuleFor(l => l.LaEstab)
                .NotEmpty()
                .MaximumLength(7);
        }
    }
}
