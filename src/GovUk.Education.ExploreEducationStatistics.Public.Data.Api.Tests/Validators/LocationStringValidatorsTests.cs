using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ErrorDetails;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Validators;

public class LocationStringValidatorsTests
{
    public class LocationStringTests : LocationStringValidatorsTests
    {
        private class TestClass
        {
            public required string Location { get; init; }
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.Location).LocationString();
            }
        }

        private readonly Validator _validator = new();

        [Theory]
        [InlineData("NAT|id|12345")]
        [InlineData("NAT|code|E92000001")]
        [InlineData("REG|id|12345")]
        [InlineData("REG|code|E12000003")]
        [InlineData("LA|id|12345")]
        [InlineData("LA|code|E08000019")]
        [InlineData("LA|code|E09000021 / E09000027")]
        [InlineData("LA|oldCode|373")]
        [InlineData("LA|oldCode|314 / 318")]
        [InlineData("SCH|id|12345")]
        [InlineData("SCH|urn|107029")]
        [InlineData("SCH|laEstab|3732060")]
        [InlineData("PROV|id|12345")]
        [InlineData("PROV|ukprn|10066874")]
        public void Success(string location)
        {
            var testObj = new TestClass { Location = location };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Failure_Empty(string? location)
        {
            var testObj = new TestClass { Location = location! };

            _validator
                .TestValidate(testObj)
                .ShouldHaveValidationErrorFor(t => t.Location)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("NAT")]
        [InlineData("id")]
        [InlineData("code")]
        [InlineData("NAT|id")]
        [InlineData("NAT|code")]
        public void Failure_InvalidFormat(string location)
        {
            var testObj = new TestClass { Location = location };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Location)
                .WithErrorCode(ValidationMessages.LocationFormat.Code)
                .WithErrorMessage(ValidationMessages.LocationFormat.Message)
                .WithCustomState<FormatErrorDetail>(s => s.Value == location)
                .WithCustomState<FormatErrorDetail>(s => s.ExpectedFormat == "{level}|{property}|{value}")
                .Only();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("NA")]
        [InlineData("NT")]
        [InlineData("LAA")]
        [InlineData("LADD")]
        [InlineData("nat")]
        [InlineData("lad")]
        [InlineData("0")]
        [InlineData("1")]
        public void Failure_InvalidLevel(string invalidLevel)
        {
            var testObj = new TestClass { Location = $"{invalidLevel}|id|12345" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Location)
                .WithErrorCode(ValidationMessages.LocationAllowedLevel.Code)
                .WithErrorMessage(ValidationMessages.LocationAllowedLevel.Message)
                .WithCustomState<LocationAllowedLevelErrorDetail>(s => s.Value == invalidLevel)
                .WithCustomState<LocationAllowedLevelErrorDetail>(s =>
                    s.AllowedLevels.SequenceEqual(EnumUtil.GetEnumValues<GeographicLevel>().Order())
                )
                .Only();
        }

        [Theory]
        [InlineData("NAT", "invalid", "id", "code")]
        [InlineData("NAT", "urn", "id", "code")]
        [InlineData("REG", "invalid", "id", "code")]
        [InlineData("REG", "ukprn", "id", "code")]
        [InlineData("RSC", "invalid", "id")]
        [InlineData("RSC", "code", "id")]
        [InlineData("SCH", "invalid", "id", "urn", "laEstab")]
        [InlineData("SCH", "code", "id", "urn", "laEstab")]
        [InlineData("SCH", "ukprn", "id", "urn", "laEstab")]
        [InlineData("PROV", "invalid", "id", "ukprn")]
        [InlineData("PROV", "urn", "id", "ukprn")]
        [InlineData("PROV", "code", "id", "ukprn")]
        public void Failure_InvalidProperty(string level, string invalidProperty, params string[] allowedProperties)
        {
            var testObj = new TestClass { Location = $"{level}|{invalidProperty}|12345" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Location)
                .WithErrorCode(ValidationMessages.LocationAllowedProperty.Code)
                .WithErrorMessage(ValidationMessages.LocationAllowedProperty.Message)
                .WithCustomState<LocationAllowedPropertyErrorDetail>(s => s.Value == invalidProperty)
                .WithCustomState<LocationAllowedPropertyErrorDetail>(s =>
                    s.AllowedProperties.SequenceEqual(allowedProperties)
                )
                .Only();
        }

        [Theory]
        [InlineData("NAT", "id", " ")]
        [InlineData("REG", "code", "  ")]
        [InlineData("REG", "code", "\n")]
        [InlineData("LA", "code", " ")]
        [InlineData("LA", "oldCode", " ")]
        [InlineData("SCH", "urn", " ")]
        [InlineData("SCH", "laEstab", " ")]
        [InlineData("PROV", "ukprn", " ")]
        public void Failure_NotEmptyValue(string level, string property, string value)
        {
            var testObj = new TestClass { Location = $"{level}|{property}|{value}" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Location)
                .WithErrorCode(ValidationMessages.LocationValueNotEmpty.Code)
                .WithAttemptedValue(value)
                .WithMessageArgument("Property", property)
                .Only();
        }

        public static readonly TheoryData<IDataSetQueryLocation> InvalidLocationValueLengths =
            DataSetQueryLocationValidatorTests.InvalidLocationValueLengths;

        [Theory]
        [MemberData(nameof(InvalidLocationValueLengths))]
        public void Failure_InvalidValueLength(IDataSetQueryLocation location)
        {
            var testObj = new TestClass { Location = location.ToLocationString() };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Location)
                .WithErrorCode(ValidationMessages.LocationValueMaxLength.Code)
                .WithAttemptedValue(location.KeyValue())
                .WithMessageArgument("Property", location.KeyProperty().ToLowerFirst())
                .WithMessageArgument("MaxLength", location.KeyValue().Length - 1)
                .Only();
        }
    }
}
