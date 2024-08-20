using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

public class FluentValidationPropertyNamesTestsFixture
{
    public FluentValidationPropertyNamesTestsFixture()
    {
        FluentValidation.ValidatorOptions.Global.UseCamelCasePropertyNames();
    }
}
