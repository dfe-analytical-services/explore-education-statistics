using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using ValidationErrorMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationErrorMessages;

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
        [InlineData("Invalid")]
        [InlineData("NAT|id")]
        [InlineData("NAT|code")]
        [InlineData("NAT|code|")]
        [InlineData("|NAT|code")]
        [InlineData("nat|code|12345")]
        [InlineData("la|code|12345")]
        public void Failure_InvalidFormat(string location)
        {
            var testObj = new TestClass { Location = location };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Location), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.LocationFormat.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationFormat.Single, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.FormatErrorDetail>(error.CustomState);

            Assert.Equal([location], state.Invalid);
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
            Assert.Equal(ValidationErrorMessages.LocationAllowedLevel.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationAllowedLevel.Single, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.AllowedLevelErrorDetail>(error.CustomState);

            Assert.Equal([location], state.Invalid);
            Assert.Equal(EnumUtil.GetEnumValues<GeographicLevel>().Order(), state.Allowed);
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
            Assert.Equal(ValidationErrorMessages.LocationAllowedProperty.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationAllowedProperty.Single, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.AllowedPropertyErrorDetail>(error.CustomState);

            var invalid = Assert.Single(state.Invalid);

            Assert.Equal(location, invalid.Value);
            Assert.Equal(allowedProperties, invalid.Allowed);
        }

        [Theory]
        [InlineData("NAT|id|99999999999999999999999999")]
        [InlineData("NAT|code|99999999999999999999999999")]
        [InlineData("LA|code|99999999999999999999999999")]
        [InlineData("LA|oldCode|99999999999999999999999999")]
        [InlineData("RSC|id|99999999999999999999999999")]
        [InlineData("SCH|urn|99999999999999999999999999")]
        [InlineData("SCH|laEstab|99999999999999999999999999")]
        [InlineData("PROV|ukprn|99999999999999999999999999")]
        public void Failure_InvalidValueLength(string location)
        {
            var testObj = new TestClass { Location = location };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Location), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.LocationMaxValueLength.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationMaxValueLength.Single, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.MaxValueLengthDetail>(error.CustomState);

            Assert.Equal([location], state.Invalid);
            Assert.Equal(25, state.MaxLength);
        }
    }

    public class OnlyLocationStringsTests : LocationStringValidatorsTests
    {
        private class TestClass
        {
            public required IEnumerable<string> Locations { get; init; }
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.Locations).OnlyLocationStrings();
            }
        }

        private readonly Validator _validator = new();

        [Theory]
        [InlineData("NAT|id|12345", "NAT|code|E92000001")]
        [InlineData("REG|id|12345", "REG|code|E12000003")]
        [InlineData("LA|id|12345", "LA|code|E08000019", "LA|oldCode|373")]
        [InlineData("SCH|id|12345", "SCH|urn|107029", "SCH|laEstab|3732060")]
        [InlineData("PROV|id|12345", "PROV|ukprn|10066874")]
        public void Success(params string[] locations)
        {
            var testObj = new TestClass { Locations = locations };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("Invalid", "NAT|id", "NAT|code")]
        [InlineData("NAT|code", "NAT|code|", "|NAT|code")]
        [InlineData("nat|code|12345", "la|code|12345")]
        public void Failure_InvalidFormats(params string[] locations)
        {
            var testObj = new TestClass { Locations = locations };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Locations), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.LocationFormat.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationFormat.Plural, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.FormatErrorDetail>(error.CustomState);

            Assert.Equal(locations, state.Invalid);
        }

        [Theory]
        [InlineData("NA|id|12345", "NA|code|12345")]
        [InlineData("NT|id|12345", "LAA|code|12345", "LADD|code|12345")]
        public void Failure_InvalidLevels(params string[] locations)
        {
            var testObj = new TestClass { Locations = locations };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Locations), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.LocationAllowedLevel.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationAllowedLevel.Plural, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.AllowedLevelErrorDetail>(error.CustomState);

            Assert.Equal(locations, state.Invalid);
            Assert.Equal(EnumUtil.GetEnumValues<GeographicLevel>().Order(), state.Allowed);
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
            var testObj = new TestClass { Locations = [location] };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Locations), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.LocationAllowedProperty.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationAllowedProperty.Plural, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.AllowedPropertyErrorDetail>(error.CustomState);

            var invalid = Assert.Single(state.Invalid);

            Assert.Equal(location, invalid.Value);
            Assert.Equal(allowedProperties, invalid.Allowed);
        }

        [Theory]
        [InlineData("NAT|urn|12345", "REG|ukprn|12345", "RSC|code|12345")]
        [InlineData("SCH|code|12345", "SCH|ukprn|12345")]
        [InlineData("PROV|urn|12345", "PROV|code|12345")]
        public void Failure_InvalidProperties(params string[] locations)
        {
            var testObj = new TestClass { Locations = locations };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Locations), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.LocationAllowedProperty.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationAllowedProperty.Plural, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.AllowedPropertyErrorDetail>(error.CustomState);

            Assert.All(state.Invalid, item => Assert.Contains(item.Value, locations));
        }

        [Theory]
        [InlineData("NAT|id|99999999999999999999999999", "NAT|code|99999999999999999999999999")]
        [InlineData("LA|code|99999999999999999999999999", "LA|oldCode|99999999999999999999999999")]
        [InlineData("RSC|id|99999999999999999999999999")]
        [InlineData(
            "SCH|urn|99999999999999999999999999",
            "SCH|laEstab|99999999999999999999999999",
            "PROV|ukprn|99999999999999999999999999"
        )]
        public void Failure_InvalidValueLength(params string[] locations)
        {
            var testObj = new TestClass { Locations = locations };

            var result = _validator.Validate(testObj);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Locations), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.LocationMaxValueLength.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.LocationMaxValueLength.Plural, error.ErrorMessage);

            var state = Assert.IsType<LocationStringValidators.MaxValueLengthDetail>(error.CustomState);

            Assert.Equal(locations, state.Invalid);
            Assert.Equal(25, state.MaxLength);
        }
    }
}
