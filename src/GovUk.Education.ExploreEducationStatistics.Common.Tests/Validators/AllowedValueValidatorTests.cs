#nullable enable
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
            var query = new TestClass { Value = value };

            _validator.TestValidate(query).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("value")]
        [InlineData("1")]
        [InlineData("")]
        public void Failure_Invalid(string value)
        {
            var query = new TestClass { Value = value };

            var result = _validator.Validate(query);

            Assert.False(result.IsValid);

            var error = Assert.Single(result.Errors);

            Assert.Equal(nameof(TestClass.Value), error.PropertyName);
            Assert.Equal(ValidationMessages.AllowedValue.Code, error.ErrorCode);
            Assert.Equal(ValidationMessages.AllowedValue.Message, error.ErrorMessage);

            var state = Assert.IsType<AllowedValueValidator.AllowedErrorDetail<string>>(
                error.CustomState
            );

            Assert.Equal(value, state.Value);
            Assert.Equal(AllowedStrings, state.Allowed);
        }
    }
}
