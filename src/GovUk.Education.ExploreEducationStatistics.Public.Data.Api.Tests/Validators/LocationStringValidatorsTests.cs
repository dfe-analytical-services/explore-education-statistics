using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
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

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Location), error.PropertyName);
            Assert.Equal(FluentValidationKeys.NotEmptyValidator, error.ErrorCode);
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("NAT")]
        [InlineData("id")]
        [InlineData("code")]
        [InlineData("NAT|id")]
        [InlineData("NAT|code")]
        [InlineData("NAT |code|")]
        [InlineData("|NAT|code")]
        [InlineData("NAT | code | 12345")]
        [InlineData(" NAT | code | 12345 ")]
        [InlineData("NAT|code | 12345")]
        [InlineData("NAT | code|12345")]
        [InlineData(" NAT|code|12345 ")]
        [InlineData(" NAT|code|12345")]
        [InlineData("NAT|code|12345 ")]
        [InlineData("nat|code|12345")]
        [InlineData("la|code|12345")]
        [InlineData("LA|code|E09000021  / E09000027")]
        [InlineData("LA|code|E09000021 /  E09000027")]
        public void Failure_InvalidFormat(string location)
        {
            var testObj = new TestClass { Location = location };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Location), error.PropertyName);
            Assert.Equal(ValidationMessages.LocationFormat.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.LocationFormat.Message, error.ErrorMessage);

            var state = Assert.IsType<FormatErrorDetail>(error.CustomState);

            Assert.Equal(location, state.Value);
            Assert.Equal("{level}|{property}|{value}", state.ExpectedFormat);
        }

        [Theory]
        [InlineData("NA|id|12345")]
        [InlineData("NA|code|12345")]
        [InlineData("NT|id|12345")]
        [InlineData("LAA|code|12345")]
        [InlineData("LADD|code|12345")]
        public void Failure_InvalidLevel(string location)
        {
            var testObj = new TestClass { Location = location };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Location), error.PropertyName);
            Assert.Equal(ValidationMessages.LocationAllowedLevel.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.LocationAllowedLevel.Message, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.AllowedLevelErrorDetail>(error.CustomState);

            Assert.Equal(location, $"{state.Value.Level}|{state.Value.Property}|{state.Value.Value}");
            Assert.Equal(EnumUtil.GetEnumValues<GeographicLevel>().Order(), state.AllowedLevels);
        }

        [Theory]
        [InlineData("NAT|urn|12345", "id", "code")]
        [InlineData("REG|ukprn|12345", "id", "code")]
        [InlineData("RSC|code|12345", "id")]
        [InlineData("SCH|code|12345", "id", "urn", "laEstab")]
        [InlineData("SCH|ukprn|12345", "id", "urn", "laEstab")]
        [InlineData("PROV|urn|12345", "id", "ukprn")]
        [InlineData("PROV|code|12345", "id", "ukprn")]
        public void Failure_InvalidProperty(string location, params string[] allowedProperties)
        {
            var testObj = new TestClass { Location = location };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Location), error.PropertyName);
            Assert.Equal(ValidationMessages.LocationAllowedProperty.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.LocationAllowedProperty.Message, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.AllowedPropertyErrorDetail>(error.CustomState);

            Assert.Equal(location, $"{state.Value.Level}|{state.Value.Property}|{state.Value.Value}");
            Assert.Equal(allowedProperties, state.AllowedProperties);
        }

        [Theory]
        [InlineData("NAT|id", 10)]
        [InlineData("NAT|code", 25)]
        [InlineData("LA|id", 10)]
        [InlineData("LA|code", 25)]
        [InlineData("LA|oldCode", 10)]
        [InlineData("RSC|id", 10)]
        [InlineData("SCH|id", 10)]
        [InlineData("SCH|urn", 6)]
        [InlineData("SCH|laEstab", 7)]
        [InlineData("PROV|id", 10)]
        [InlineData("PROV|ukprn", 8)]
        public void Failure_InvalidValueLength(string levelProperty, int expectedMaxLength)
        {
            var location = $"{levelProperty}|{new string('X', expectedMaxLength + 1)}";
            var testObj = new TestClass { Location = location };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Location), error.PropertyName);
            Assert.Equal(ValidationMessages.LocationMaxValueLength.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.LocationMaxValueLength.Message, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.MaxValueLengthErrorDetail>(error.CustomState);

            Assert.Equal(location, $"{state.Value.Level}|{state.Value.Property}|{state.Value.Value}");
            Assert.Equal(expectedMaxLength, state.MaxValueLength);
        }
    }
}
