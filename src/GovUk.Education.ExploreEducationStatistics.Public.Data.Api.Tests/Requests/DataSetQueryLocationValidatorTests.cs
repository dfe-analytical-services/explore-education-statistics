using FluentValidation;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetQueryLocationValidatorTests
{
    public static readonly TheoryData<IDataSetQueryLocation> ValidLocations = new()
    {
        new DataSetQueryLocationId { Level = "NAT", Id = "12345" },
        new DataSetQueryLocationCode { Level = "NAT", Code = "12345" },
        new DataSetQueryLocationId { Level = "REG", Id = "12345" },
        new DataSetQueryLocationCode { Level = "REG", Code = "12345" },
        new DataSetQueryLocationId { Level = "LA", Id = "12345" },
        new DataSetQueryLocationLocalAuthorityCode { Code = "12345" },
        new DataSetQueryLocationLocalAuthorityCode { Code = "E08000019" },
        new DataSetQueryLocationLocalAuthorityCode { Code = "E09000021 / E09000027" },
        new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "373" },
        new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "314 / 318" },
        new DataSetQueryLocationId { Level = "LA", Id = "12345" },
        new DataSetQueryLocationSchoolUrn { Urn = "107029" },
        new DataSetQueryLocationSchoolLaEstab { LaEstab = "3732060" },
        new DataSetQueryLocationId { Level = "PROV", Id = "12345" },
        new DataSetQueryLocationProviderUkprn { Ukprn = "3732060" },
    };

    [Theory]
    [MemberData(nameof(ValidLocations))]
    public void Success(IDataSetQueryLocation location)
    {
        var validator = location.CreateValidator();

        var result = validator.Validate(new ValidationContext<IDataSetQueryLocation>(location));

        Assert.Empty(result.Errors);
    }

    public static readonly TheoryData<IDataSetQueryLocation> InvalidLocationLevels = new()
    {
        new DataSetQueryLocationId { Level = "NA", Id = "12345" },
        new DataSetQueryLocationCode { Level = "NA", Code = "12345" },
        new DataSetQueryLocationId { Level = "NT", Id = "12345" },
        new DataSetQueryLocationCode { Level = "LAA", Code = "12345" },
        new DataSetQueryLocationCode { Level = "LADD", Code = "12345" },
    };

    [Theory]
    [MemberData(nameof(InvalidLocationLevels))]
    public void Failure_InvalidLevel(IDataSetQueryLocation location)
    {
        var validator = location.CreateValidator();

        var result = validator.Validate(new ValidationContext<IDataSetQueryLocation>(location));

        var error = Assert.Single(result.Errors);

        Assert.Equal(nameof(IDataSetQueryLocation.Level), error.PropertyName);
        Assert.Equal(ValidationMessages.AllowedValue.Code, error.ErrorCode);
        Assert.Equal(ValidationMessages.AllowedValue.Message, error.ErrorMessage);

        var state = Assert.IsType<AllowedValueValidator.AllowedErrorDetail<string>>(
            error.CustomState
        );

        Assert.Equal(location.Level, state.Value);
        Assert.Equal(GeographicLevelUtils.OrderedCodes, state.Allowed);
    }

    public static readonly TheoryData<IDataSetQueryLocation> EmptyLocationValues = new()
    {
        new DataSetQueryLocationId { Level = "NAT", Id = "" },
        new DataSetQueryLocationId { Level = "NAT", Id = "  " },
        new DataSetQueryLocationCode { Level = "NAT", Code = "" },
        new DataSetQueryLocationId { Level = "LA", Id = "" },
        new DataSetQueryLocationLocalAuthorityCode { Code = "" },
        new DataSetQueryLocationLocalAuthorityOldCode { OldCode = "" },
        new DataSetQueryLocationId { Level = "RSC", Id = "" },
        new DataSetQueryLocationId { Level = "SCH", Id = "" },
        new DataSetQueryLocationSchoolUrn { Urn = "" },
        new DataSetQueryLocationSchoolLaEstab { LaEstab = "" },
        new DataSetQueryLocationId { Level = "PROV", Id = "" },
        new DataSetQueryLocationProviderUkprn { Ukprn = "" },
    };

    [Theory]
    [MemberData(nameof(EmptyLocationValues))]
    public void Failure_EmptyValue(IDataSetQueryLocation location)
    {
        var validator = location.CreateValidator();

        var result = validator.Validate(new ValidationContext<IDataSetQueryLocation>(location));

        var error = Assert.Single(result.Errors);

        Assert.Equal(FluentValidationKeys.NotEmptyValidator, error.ErrorCode);

        Assert.Equal(location.KeyProperty(), error.PropertyName);
        Assert.Equal(location.KeyValue(), error.AttemptedValue);
    }

    public static readonly TheoryData<IDataSetQueryLocation> InvalidLocationValueLengths = new()
    {
        new DataSetQueryLocationId { Level = "NAT", Id = new string('X', 11) },
        new DataSetQueryLocationCode { Level = "NAT", Code = new string('X', 31) },
        new DataSetQueryLocationId { Level = "LA", Id = new string('X', 11) },
        new DataSetQueryLocationLocalAuthorityCode { Code = new string('X', 31) },
        new DataSetQueryLocationLocalAuthorityOldCode { OldCode = new string('X', 21) },
        new DataSetQueryLocationId { Level = "RSC", Id = new string('X', 11) },
        new DataSetQueryLocationId { Level = "SCH", Id = new string('X', 11) },
        new DataSetQueryLocationSchoolUrn { Urn = new string('X', 21) },
        new DataSetQueryLocationSchoolLaEstab { LaEstab = new string('X', 21) },
        new DataSetQueryLocationId { Level = "PROV", Id = new string('X', 11) },
        new DataSetQueryLocationProviderUkprn { Ukprn = new string('X', 21) },
    };

    [Theory]
    [MemberData(nameof(InvalidLocationValueLengths))]
    public void Failure_InvalidValueLength(IDataSetQueryLocation location)
    {
        var validator = location.CreateValidator();

        var result = validator.Validate(new ValidationContext<IDataSetQueryLocation>(location));

        var error = Assert.Single(result.Errors);

        Assert.Equal(FluentValidationKeys.MaximumLengthValidator, error.ErrorCode);

        Assert.Equal(location.KeyProperty(), error.PropertyName);

        Assert.Equal(location.KeyValue(), error.AttemptedValue);
        Assert.Equal(
            location.KeyValue().Length - 1,
            error.FormattedMessagePlaceholderValues["MaxLength"]
        );
    }
}
