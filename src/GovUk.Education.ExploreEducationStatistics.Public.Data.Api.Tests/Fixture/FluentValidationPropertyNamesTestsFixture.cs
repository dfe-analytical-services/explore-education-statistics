using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

public class FluentValidationPropertyNamesTestsFixture
{
    public FluentValidationPropertyNamesTestsFixture()
    {
        FluentValidation.ValidatorOptions.Global.UseCamelCasePropertyNames();
    }
}
