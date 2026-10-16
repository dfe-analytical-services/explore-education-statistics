#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests;

public class ReleaseUpdateRequestValidatorTests
{
    private readonly ReleaseUpdateRequest.Validator _validator = new();

    [Fact]
    public void WhenLabelIsOver20Characters_ValidationFails()
    {
        var result = _validator.TestValidate(new ReleaseUpdateRequest { Label = new string('a', 21) });

        result.ShouldHaveValidationErrorFor(r => r.Label).WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("aaaaaaaaaaaaaaaaaaaa")]
    public void WhenLabelIsNullOrUpTo20Characters_ValidationPasses(string? label)
    {
        var result = _validator.TestValidate(new ReleaseUpdateRequest { Label = label });

        result.ShouldNotHaveAnyValidationErrors();
    }
}
