#nullable enable
using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;
using ValidationMessages = GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationMessages;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests;

public class PairedIndicatorsUpdateRequestTests
{
    private readonly PairedIndicatorsUpdateRequest.Validator _validator = new();

    [Fact]
    public void WhenPairsAreValid_ValidationPasses()
    {
        var request = new PairedIndicatorsUpdateRequest
        {
            PairedIndicators = [Pair(Guid.NewGuid(), Guid.NewGuid()), Pair(Guid.NewGuid(), Guid.NewGuid())],
        };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void WhenPairsAreEmpty_ValidationPasses()
    {
        var request = new PairedIndicatorsUpdateRequest { PairedIndicators = [] };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void WhenIndicatorIdsAreEmpty_ValidationFails()
    {
        var request = new PairedIndicatorsUpdateRequest { PairedIndicators = [Pair(Guid.Empty, Guid.Empty)] };

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor("PairedIndicators[0].ParentIndicatorId")
            .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
        result
            .ShouldHaveValidationErrorFor("PairedIndicators[0].ChildIndicatorId")
            .WithErrorCode(FluentValidationKeys.NotEmptyValidator);
    }

    [Fact]
    public void WhenPairIsNull_ValidationFails()
    {
        var request = new PairedIndicatorsUpdateRequest
        {
            PairedIndicators = [Pair(Guid.NewGuid(), Guid.NewGuid()), null!],
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor("PairedIndicators[1]").WithErrorCode(FluentValidationKeys.NotNullValidator);
    }

    [Fact]
    public void WhenIndicatorIsPairedWithItself_ValidationFails()
    {
        var indicatorId = Guid.NewGuid();
        var request = new PairedIndicatorsUpdateRequest { PairedIndicators = [Pair(indicatorId, indicatorId)] };

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor("PairedIndicators[0].ChildIndicatorId")
            .WithErrorCode(ValidationMessages.PairedIndicatorCannotPairWithItself.Code);
        result.ShouldNotHaveValidationErrorFor(r => r.PairedIndicators);
    }

    [Fact]
    public void WhenChildHasMultipleParents_ValidationFails()
    {
        var childId = Guid.NewGuid();
        var request = new PairedIndicatorsUpdateRequest
        {
            PairedIndicators = [Pair(Guid.NewGuid(), childId), Pair(Guid.NewGuid(), childId)],
        };

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(r => r.PairedIndicators)
            .WithErrorCode(ValidationMessages.PairedIndicatorChildHasMultipleParents.Code);
    }

    [Fact]
    public void WhenParentHasMultipleChildren_ValidationFails()
    {
        var parentId = Guid.NewGuid();
        var request = new PairedIndicatorsUpdateRequest
        {
            PairedIndicators = [Pair(parentId, Guid.NewGuid()), Pair(parentId, Guid.NewGuid())],
        };

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(r => r.PairedIndicators)
            .WithErrorCode(ValidationMessages.PairedIndicatorParentHasMultipleChildren.Code);
    }

    [Fact]
    public void WhenPairsAreChained_ValidationFails()
    {
        var indicator1Id = Guid.NewGuid();
        var indicator2Id = Guid.NewGuid();
        var indicator3Id = Guid.NewGuid();
        var request = new PairedIndicatorsUpdateRequest
        {
            PairedIndicators = [Pair(indicator1Id, indicator2Id), Pair(indicator2Id, indicator3Id)],
        };

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(r => r.PairedIndicators)
            .WithErrorCode(ValidationMessages.PairedIndicatorsCannotBeChained.Code);
    }

    [Fact]
    public void WhenPairIsReversed_ValidationFails()
    {
        var indicator1Id = Guid.NewGuid();
        var indicator2Id = Guid.NewGuid();
        var request = new PairedIndicatorsUpdateRequest
        {
            PairedIndicators = [Pair(indicator1Id, indicator2Id), Pair(indicator2Id, indicator1Id)],
        };

        var result = _validator.TestValidate(request);

        result
            .ShouldHaveValidationErrorFor(r => r.PairedIndicators)
            .WithErrorCode(ValidationMessages.PairedIndicatorsCannotBeChained.Code);
    }

    private static PairedIndicatorRequest Pair(Guid parentIndicatorId, Guid childIndicatorId) =>
        new() { ParentIndicatorId = parentIndicatorId, ChildIndicatorId = childIndicatorId };
}
