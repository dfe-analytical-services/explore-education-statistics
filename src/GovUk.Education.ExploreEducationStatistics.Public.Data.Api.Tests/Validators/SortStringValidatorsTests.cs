using FluentValidation;
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Validators;

public class SortStringValidatorsTests
{
    public class SortStringTests : SortStringValidatorsTests
    {
        private class TestClass
        {
            public required string Sort { get; init; }
        }

        private class Validator : AbstractValidator<TestClass>
        {
            public Validator()
            {
                RuleFor(t => t.Sort).SortString();
            }
        }

        private readonly Validator _validator = new();

        [Theory]
        [InlineData("timePeriod|Asc")]
        [InlineData("timePeriod|Desc")]
        [InlineData("geographicLevel|Asc")]
        [InlineData("geographicLevel|Desc")]
        [InlineData("locations|REG|Desc")]
        [InlineData("locations|LA|Desc")]
        [InlineData("characteristic|Asc")]
        [InlineData("characteristic|Desc")]
        [InlineData("school_type|Asc")]
        [InlineData("school_type|Desc")]
        [InlineData("ssa_tier_1|Asc")]
        [InlineData("ssa_tier_2|Desc")]
        public void Success(string sort)
        {
            var testObj = new TestClass { Sort = sort };

            _validator.TestValidate(testObj).ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData(null)]
        public void Failure_Empty(string? sort)
        {
            var testObj = new TestClass { Sort = sort! };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Sort)
                .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
                .Only();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("Asc")]
        [InlineData("timePeriod:Asc")]
        [InlineData("locations|||Asc")]
        [InlineData("locations| ||Asc")]
        [InlineData("locations| | |Asc")]
        [InlineData("locations|LA||Asc")]
        [InlineData("locations|LA| |Asc")]
        [InlineData("locations|LA|Asc|")]
        [InlineData("locations|LA| Asc|")]
        public void Failure_InvalidFormat(string sort)
        {
            var testObj = new TestClass { Sort = sort };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Sort)
                .WithErrorCode(ValidationMessages.SortFormat.Code)
                .WithErrorMessage(ValidationMessages.SortFormat.Message)
                .WithCustomState<FormatErrorDetail>(s => s.Value == sort)
                .WithCustomState<FormatErrorDetail>(s => s.ExpectedFormat == "{field}|{direction}")
                .Only();
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("  ")]
        [InlineData("\n")]
        public void Failure_EmptyField(string field)
        {
            var testObj = new TestClass { Sort = $"{field}|Asc" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Sort)
                .WithErrorCode(ValidationMessages.SortFieldNotEmpty.Code)
                .WithErrorMessage(ValidationMessages.SortFieldNotEmpty.Message)
                .WithMessageArgument("Property", "field")
                .Only();
        }

        [Theory]
        [InlineData("Asc")]
        [InlineData("Desc")]
        public void Failure_InvalidFieldLength(string direction)
        {
            var testObj = new TestClass { Sort = $"{new string('a', 41)}|{direction}" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Sort)
                .WithErrorCode(ValidationMessages.SortFieldMaxLength.Code)
                .WithMessageArgument("MaxLength", 40)
                .WithMessageArgument("Property", "field")
                .Only();
        }

        [Theory]
        [InlineData("Invalid")]
        [InlineData("asc")]
        [InlineData("desc")]
        public void Failure_InvalidDirection(string direction)
        {
            var testObj = new TestClass { Sort = $"timePeriod|{direction}" };

            var result = _validator.TestValidate(testObj);

            result
                .ShouldHaveValidationErrorFor(t => t.Sort)
                .WithErrorCode(ValidationMessages.SortDirection.Code)
                .WithErrorMessage(ValidationMessages.SortDirection.Message)
                .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                    s.Value == direction
                )
                .WithCustomState<AllowedValueValidator.AllowedErrorDetail<string>>(s =>
                    s.Allowed.SequenceEqual(
                        [SortDirection.Asc.ToString(), SortDirection.Desc.ToString()]
                    )
                )
                .Only();
        }
    }
}
