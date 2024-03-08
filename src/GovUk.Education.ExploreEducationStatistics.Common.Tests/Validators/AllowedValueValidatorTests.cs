#nullable enable
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;

public abstract class AllowedValueValidatorTests
{
    private static readonly IList<string> AllowedStrings = ListOf("value-1", "value-2", "value-3");

    public class AllowedValueTests
    {
        private class TestClass
        {
            public string Value { get; init; } = string.Empty;
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.Value).AllowedValue(AllowedStrings);
            }
        }

        private readonly Validator _validator = new();

        [Theory]
        [InlineData("value-1")]
        [InlineData("value-2")]
        [InlineData("value-3")]
        public void Success(string value)
        {
            var query = new TestClass
            {
                Value = value
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }


        [Theory]
        [InlineData("Invalid")]
        [InlineData("value")]
        [InlineData("1")]
        [InlineData("")]
        public void Failure_Invalid(string value)
        {
            var query = new TestClass
            {
                Value = value
            };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Value), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Single, error.ErrorMessage);

            var state = Assert.IsType<AllowedValueValidator.ErrorDetail<string>>(error.CustomState);

            Assert.Equal(ListOf(value), state.Invalid);
            Assert.Equal(AllowedStrings, state.Allowed);
        }
    }

    public class OnlyAllowedValuesTests
    {
        private class TestClass
        {
            public IList<string> Values { get; init; } = new List<string>();
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.Values).OnlyAllowedValues(AllowedStrings);
            }
        }

        private readonly Validator _validator = new();


        [Theory]
        [InlineData("value-1", "value-2", "value-3")]
        [InlineData("value-2", "value-1")]
        [InlineData("value-3")]
        public void Success(params string[] values)
        {
            var query = new TestClass
            {
                Values = values
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("Invalid1", "Invalid2")]
        [InlineData("value-", "value", "1")]
        [InlineData("")]
        public void Failure_NoneAllowed(params string[] geographicLevels)
        {
            var query = new TestClass
            {
                Values = geographicLevels
            };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Values), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Plural, error.ErrorMessage);

            var state = Assert.IsType<AllowedValueValidator.ErrorDetail<string>>(error.CustomState);

            Assert.Equal(geographicLevels, state.Invalid);
            Assert.Equal(AllowedStrings, state.Allowed);
        }

        [Fact]
        public void Failure_SomeAllowed()
        {
            var query = new TestClass
            {
                Values = ListOf("value-2", "value-1", "", "value", "Invalid", "1", "value-", "value-3")
            };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Values), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Plural, error.ErrorMessage);

            var state = Assert.IsType<AllowedValueValidator.ErrorDetail<string>>(error.CustomState);

            Assert.Equal(ListOf("", "value", "Invalid", "1", "value-"), state.Invalid);
            Assert.Equal(AllowedStrings, state.Allowed);
        }
    }

    public class OnlyAllowedReadOnlyValuesTests
    {
        private class TestClass
        {
            public IList<string> Values { get; init; } = new List<string>();
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.Values).OnlyAllowedValues(AllowedStrings);
            }
        }

        private readonly Validator _validator = new();


        [Theory]
        [InlineData("value-1", "value-2", "value-3")]
        [InlineData("value-2", "value-1")]
        [InlineData("value-3")]
        public void Success(params string[] values)
        {
            var query = new TestClass
            {
                Values = values
            };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("Invalid1", "Invalid2")]
        [InlineData("value-", "value", "1")]
        [InlineData("")]
        public void Failure_NoneAllowed(params string[] geographicLevels)
        {
            var query = new TestClass
            {
                Values = geographicLevels
            };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Values), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Plural, error.ErrorMessage);

            var state = Assert.IsType<AllowedValueValidator.ErrorDetail<string>>(error.CustomState);

            Assert.Equal(geographicLevels, state.Invalid);
            Assert.Equal(AllowedStrings, state.Allowed);
        }

        [Fact]
        public void Failure_SomeAllowed()
        {
            var query = new TestClass
            {
                Values = ListOf("value-2", "value-1", "", "value", "Invalid", "1", "value-", "value-3")
            };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Values), error.PropertyName);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Code, error.ErrorCode);
            Assert.Equal(ValidationErrorMessages.AllowedValue.Plural, error.ErrorMessage);

            var state = Assert.IsType<AllowedValueValidator.ErrorDetail<string>>(error.CustomState);

            Assert.Equal(ListOf("", "value", "Invalid", "1", "value-"), state.Invalid);
            Assert.Equal(AllowedStrings, state.Allowed);
        }
    }
}
