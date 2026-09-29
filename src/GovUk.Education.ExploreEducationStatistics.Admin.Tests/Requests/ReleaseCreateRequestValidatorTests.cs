#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests;

public class ReleaseCreateRequestValidatorTests
{
    private readonly ReleaseCreateRequest.Validator _validator = new();

    [Fact]
    public void WhenRequestIsValid_ValidationPasses()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void WhenTypeIsNull_ValidationFails()
    {
        var result = _validator.TestValidate(ValidRequest() with { Type = null });

        result.ShouldHaveValidationErrorFor(r => r.Type).WithErrorCode(FluentValidationKeys.NotNullValidator);
    }

    [Fact]
    public void WhenTypeIsExperimentalStatistics_ValidationFails()
    {
        var result = _validator.TestValidate(ValidRequest() with { Type = ReleaseType.ExperimentalStatistics });

        result
            .ShouldHaveValidationErrorFor(r => r.Type)
            .WithErrorCode(FluentValidationKeys.NotEqualValidator)
            .WithMessageArgument("ComparisonValue", ReleaseType.ExperimentalStatistics);
    }

    [Theory]
    [InlineData(999)]
    [InlineData(10000)]
    public void WhenYearIsOutOfRange_ValidationFails(int year)
    {
        var result = _validator.TestValidate(ValidRequest() with { Year = year });

        result.ShouldHaveValidationErrorFor(r => r.Year).WithErrorCode(FluentValidationKeys.InclusiveBetweenValidator);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(9999)]
    public void WhenYearIsInRange_ValidationPasses(int year)
    {
        var result = _validator.TestValidate(ValidRequest() with { Year = year });

        result.ShouldNotHaveValidationErrorFor(r => r.Year);
    }

    [Fact]
    public void WhenLabelIsOver20Characters_ValidationFails()
    {
        var result = _validator.TestValidate(ValidRequest() with { Label = new string('a', 21) });

        result.ShouldHaveValidationErrorFor(r => r.Label).WithErrorCode(FluentValidationKeys.MaximumLengthValidator);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("aaaaaaaaaaaaaaaaaaaa")]
    public void WhenLabelIsNullOrUpTo20Characters_ValidationPasses(string? label)
    {
        var result = _validator.TestValidate(ValidRequest() with { Label = label });

        result.ShouldNotHaveValidationErrorFor(r => r.Label);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void WhenPublishingOrganisationsHasOneToThreeValues_ValidationPasses(int count)
    {
        var result = _validator.TestValidate(
            ValidRequest() with
            {
                PublishingOrganisations = [.. MockUtils.GenerateGuids(count)],
            }
        );

        result.ShouldNotHaveValidationErrorFor(r => r.PublishingOrganisations);
    }

    [Fact]
    public void WhenPublishingOrganisationsHasMoreThanThreeValues_ValidationFails()
    {
        var result = _validator.TestValidate(
            ValidRequest() with
            {
                PublishingOrganisations = [.. MockUtils.GenerateGuids(4)],
            }
        );

        result
            .ShouldHaveValidationErrorFor(r => r.PublishingOrganisations)
            .WithErrorCode(FluentValidationKeys.LessThanOrEqualValidator);
    }

    private static ReleaseCreateRequest ValidRequest() =>
        new()
        {
            PublicationId = Guid.NewGuid(),
            Type = ReleaseType.OfficialStatistics,
            TimePeriodCoverage = TimeIdentifier.AcademicYear,
            Year = 2020,
            Label = "initial",
            PublishingOrganisations = [Guid.NewGuid()],
        };
}
