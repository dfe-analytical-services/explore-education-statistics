#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class PermalinkControllerTests
    {
        [Fact]
        public async Task Get_InvalidIdReturnsNotFound()
        {
            var result = await BuildPermalinkController().Get("NotAGuid");
            result.AssertNotFoundResult();
        }

        private static PermalinkController BuildPermalinkController(
            IPermalinkService? permalinkService = null
        )
        {
            return new(
                permalinkService ?? Mock.Of<IPermalinkService>(MockBehavior.Strict)
            );
        }
    }
}
