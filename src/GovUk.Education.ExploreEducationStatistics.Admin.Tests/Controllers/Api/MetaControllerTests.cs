#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class MetaControllerTests
    {
        [Fact]
        public void GetTimeIdentifiersByCategory()
        {
            var controller = new MetaController(new MetaService());

            var result = controller.GetTimeIdentifiersByCategory();
            result.AssertOkResult();
        }
        
    }
}
