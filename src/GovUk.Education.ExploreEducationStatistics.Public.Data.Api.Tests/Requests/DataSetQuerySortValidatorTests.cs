using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Requests;

public class DataSetQuerySortValidatorTests
{
    private readonly DataSetQuerySort.Validator _validator = new();

    public static readonly TheoryData<string, string> ValidSorts = new()
    {
        { "timePeriod", "Asc" },
        { "timePeriod", "Desc" },
        { "geographicLevel", "Asc" },
        { "geographicLevel", "Desc" },
        { "locations|REG", "Desc" },
        { "locations|LA", "Desc" },
        { "characteristic", "Asc" },
        { "characteristic", "Desc" },
        { "school_type", "Asc" },
        { "school_type", "Desc" },
        { "ssa_tier_1", "Asc" },
        { "ssa_tier_2", "Desc" },
    };

    [Theory]
    [MemberData(nameof(ValidSorts))]
    public void Success(string field, string direction)
    {
        var sort = new DataSetQuerySort { Field = field, Direction = direction };

        _validator.TestValidate(sort).ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\n")]
    public void Failure_EmptyField(string field)
    {
        var sort = new DataSetQuerySort { Field = field, Direction = "Asc" };

        var result = _validator.TestValidate(sort);

        result
            .ShouldHaveValidationErrorFor(s => s.Field)
            .WithErrorCode(FluentValidationKeys.NotEmptyValidator)
            .Only();
    }

    [Fact]
    public void Failure_InvalidFieldLength()
    {
        var sort = new DataSetQuerySort { Field = new string('a', 41), Direction = "Asc" };

        var result = _validator.TestValidate(sort);

        result
            .ShouldHaveValidationErrorFor(s => s.Field)
            .WithErrorCode(FluentValidationKeys.MaximumLengthValidator)
            .WithMessageArgument("MaxLength", 40)
            .Only();
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("asc")]
    [InlineData("desc")]
    public void Failure_InvalidDirection(string direction)
    {
        var sort = new DataSetQuerySort { Field = "timePeriod", Direction = direction };

        var result = _validator.TestValidate(sort);

        result
            .ShouldHaveValidationErrorFor(s => s.Direction)
            .WithErrorCode(ValidationMessages.AllowedValue.Code)
            .WithErrorMessage(ValidationMessages.AllowedValue.Message)
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
