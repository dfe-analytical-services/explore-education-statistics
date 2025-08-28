using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests.Converters;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Swagger;
using Swashbuckle.AspNetCore.Annotations;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

[JsonConverter(typeof(DataSetQueryLocationJsonConverter))]
[SwaggerSubType(typeof(DataSetQueryLocationId))]
[SwaggerSubType(typeof(DataSetQueryLocationCode))]
[SwaggerSubType(typeof(DataSetQueryLocationLocalAuthorityCode))]
[SwaggerSubType(typeof(DataSetQueryLocationLocalAuthorityOldCode))]
[SwaggerSubType(typeof(DataSetQueryLocationProviderUkprn))]
[SwaggerSubType(typeof(DataSetQueryLocationSchoolLaEstab))]
[SwaggerSubType(typeof(DataSetQueryLocationSchoolUrn))]
public interface IDataSetQueryLocation
{
    /// <summary>
    /// The geographic level of the location.
    /// </summary>
    public string Level { get; init; }

    public GeographicLevel ParsedLevel() => EnumUtil.GetFromEnumValue<GeographicLevel>(Level);

    /// <summary>
    /// The name of the key property for this location.
    /// </summary>
    public string KeyProperty();

    /// <summary>
    /// The value of the key property for this location.
    /// </summary>
    public string KeyValue();

    public string ToLocationString() => $"{Level}|{KeyProperty().ToLowerFirst()}|{KeyValue()}";

    public static IDataSetQueryLocation Parse(string location)
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

    public IValidator CreateValidator();
}

/// <summary>
/// A location (identified by an ID) to filter results by.
///
/// Note that location IDs are guaranteed to be unique to a single location
/// unlike location codes (which may correspond to multiple locations).
/// </summary>
public record DataSetQueryLocationId : IDataSetQueryLocation
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
    public required string Level { get; init; }

    public string KeyProperty() => nameof(Id);

    public string KeyValue() => Id;

    public IValidator CreateValidator() => new Validator();

    public class Validator : AbstractValidator<DataSetQueryLocationId>
    {
        public Validator()
        {
            RuleFor(l => l.Level)
                .AllowedValue(GeographicLevelUtils.OrderedCodes);

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
public record DataSetQueryLocationCode : IDataSetQueryLocation
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
    public required string Level { get; init; }

    public string KeyProperty() => nameof(Code);

    public string KeyValue() => Code;

    public IValidator CreateValidator() => new Validator();

    public class Validator : AbstractValidator<DataSetQueryLocationCode>
    {
        public Validator()
        {
            RuleFor(l => l.Level)
                .AllowedValue(GeographicLevelUtils.OrderedCodes);

            RuleFor(l => l.Code)
                .NotEmpty()
                .MaximumLength(30);
        }
    }
}

/// <summary>
/// A local authority (identified by its ONS code) to filter results by.
/// </summary>
public record DataSetQueryLocationLocalAuthorityCode : IDataSetQueryLocation
{
    /// <summary>
    /// The ONS code of the local authority. This is typically 9 characters
    /// in the standard ONS format for local authorities e.g. `E08000019`,
    /// but may be a combination of multiple codes e.g. `E08000019 / E08000020`.
    /// </summary>
    /// <example>E08000019</example>
    public required string Code { get; init; }

    /// <summary>
    /// The geographic level of the local authority. Must be set to `LA`.
    /// </summary>
    /// <example>LA</example>
    [Required]
    [SwaggerEnum(typeof(GeographicLevel))]
    public string Level { get; init; } = GeographicLevel.LocalAuthority.GetEnumValue();

    public string KeyProperty() => nameof(Code);

    public string KeyValue() => Code;

    public IValidator CreateValidator() => new Validator();

    public class Validator : AbstractValidator<DataSetQueryLocationLocalAuthorityCode>
    {
        public Validator()
        {
            RuleFor(l => l.Level)
                .AllowedValue([GeographicLevel.LocalAuthority.GetEnumValue()]);

            RuleFor(l => l.Code)
                .NotEmpty()
                .MaximumLength(30);
        }
    }
}

/// <summary>
/// A local authority (identified by its old code) to filter results by.
/// </summary>
public record DataSetQueryLocationLocalAuthorityOldCode : IDataSetQueryLocation
{
    /// <summary>
    /// The old code (previously the LEA code) of the local authority.
    /// This is typically a 3-digit number e.g. `318`, but may be a
    /// combination of multiple codes e.g. `318 / 319`.
    /// </summary>
    /// <example>373</example>
    public required string OldCode { get; init; }

    /// <summary>
    /// The geographic level of the local authority. Must be set to `LA`.
    /// </summary>
    /// <example>LA</example>
    [SwaggerEnum(typeof(GeographicLevel))]
    public string Level { get; init; } = GeographicLevel.LocalAuthority.GetEnumValue();

    public string KeyProperty() => nameof(OldCode);

    public string KeyValue() => OldCode;

    public IValidator CreateValidator() => new Validator();

    public class Validator : AbstractValidator<DataSetQueryLocationLocalAuthorityOldCode>
    {
        public Validator()
        {
            RuleFor(l => l.Level)
                .AllowedValue([GeographicLevel.LocalAuthority.GetEnumValue()]);

            RuleFor(l => l.OldCode)
                .NotEmpty()
                .MaximumLength(20);
        }
    }
}

/// <summary>
/// A provider (identified by its UKPRN) to filter results by.
/// </summary>
public record DataSetQueryLocationProviderUkprn : IDataSetQueryLocation
{
    /// <summary>
    /// The UKPRN (UK provider reference number) of the provider.
    /// This is typically an 8-digit number.
    /// </summary>
    /// <example>123454678</example>
    public required string Ukprn { get; init; }

    /// <summary>
    /// The geographic level of the provider. Must be set to `PROV`.
    /// </summary>
    /// <example>PROV</example>
    [Required]
    [SwaggerEnum(typeof(GeographicLevel))]
    public string Level { get; init; } = GeographicLevel.Provider.GetEnumValue();

    public string KeyProperty() => nameof(Ukprn);

    public string KeyValue() => Ukprn;

    public IValidator CreateValidator() => new Validator();

    public class Validator : AbstractValidator<DataSetQueryLocationProviderUkprn>
    {
        public Validator()
        {
            RuleFor(l => l.Level)
                .AllowedValue([GeographicLevel.Provider.GetEnumValue()]);

            RuleFor(l => l.Ukprn)
                .NotEmpty()
                .MaximumLength(20);
        }
    }
}

/// <summary>
/// A school (identified by its URN) to filter results by.
/// </summary>
public record DataSetQueryLocationSchoolUrn : IDataSetQueryLocation
{
    /// <summary>
    /// The URN (unique reference number) of the school.
    /// This is typically a 6-digit number.
    /// </summary>
    /// <example>123456</example>
    public required string Urn { get; init; }

    /// <summary>
    /// The geographic level of the school. Must be set to `SCH`.
    /// </summary>
    /// <example>SCH</example>
    [Required]
    [SwaggerEnum(typeof(GeographicLevel))]
    public string Level { get; init; } = GeographicLevel.School.GetEnumValue();

    public string KeyProperty() => nameof(Urn);

    public string KeyValue() => Urn;

    public IValidator CreateValidator() => new Validator();

    public class Validator : AbstractValidator<DataSetQueryLocationSchoolUrn>
    {
        public Validator()
        {
            RuleFor(l => l.Level)
                .AllowedValue([GeographicLevel.School.GetEnumValue()]);

            RuleFor(l => l.Urn)
                .NotEmpty()
                .MaximumLength(20);
        }
    }
}

/// <summary>
/// A school (identified by its LAESTAB) to filter results by.
/// </summary>
public record DataSetQueryLocationSchoolLaEstab : IDataSetQueryLocation
{
    /// <summary>
    /// The LAESTAB (local authority establishment number) of the school.
    /// This is typically a 7-digit number.
    /// </summary>
    /// <example>1234567</example>
    public required string LaEstab { get; init; }

    /// <summary>
    /// The geographic level of the location. Must be set to `SCH`.
    /// </summary>
    /// <example>SCH</example>
    [Required]
    public string Level { get; init; } = GeographicLevel.School.GetEnumValue();

    public string KeyProperty() => nameof(LaEstab);

    public string KeyValue() => LaEstab;

    public IValidator CreateValidator() => new Validator();

    public class Validator : AbstractValidator<DataSetQueryLocationSchoolLaEstab>
    {
        public Validator()
        {
            RuleFor(l => l.Level)
                .AllowedValue([GeographicLevel.School.GetEnumValue()]);

            RuleFor(l => l.LaEstab)
                .NotEmpty()
                .MaximumLength(20);
        }
    }
}
