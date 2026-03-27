#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Requests;

public class ReleaseCreateRequestValidatorTests
{
    [Fact]
    public void Validate_ZeroPublishingOrganisations_ReturnsUnit()
    {
        var result = ReleaseCreateRequestValidator.Validate(new ReleaseCreateRequest { PublishingOrganisations = [] });

        result.AssertRight();
    }

    [Fact]
    public void Validate_NullPublishingOrganisations_ReturnsUnit()
    {
        var result = ReleaseCreateRequestValidator.Validate(new());

        result.AssertRight();
    }

    [Fact]
    public void Validate_ValidNumberOfPublishingOrganisations_ReturnsUnit()
    {
        var result = ReleaseCreateRequestValidator.Validate(
            new ReleaseCreateRequest { PublishingOrganisations = [.. MockUtils.GenerateGuids(3)] }
        );

        result.AssertRight();
    }

    [Fact]
    public void Validate_TooManyPublishingOrganisations_ReturnsBadRequest()
    {
        var result = ReleaseCreateRequestValidator.Validate(
            new ReleaseCreateRequest { PublishingOrganisations = [.. MockUtils.GenerateGuids(4)] }
        );

        result.AssertBadRequest(ValidationErrorMessages.PublishingOrganisationsLimitExceeded);
    }
}
