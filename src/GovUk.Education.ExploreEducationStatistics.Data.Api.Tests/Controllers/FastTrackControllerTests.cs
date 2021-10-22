using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class FastTrackControllerTests
    {
        private readonly Guid fastTrackId = Guid.NewGuid();

        [Fact]
        public async Task Get_FastTrack()
        {
            var fastTrackViewModel = new FastTrackViewModel();

            var (controller, mocks) = BuildControllerAndMocks();
            
            mocks
                .fastTrackService
                .Setup(s => s.GetFastTrackAndResults(fastTrackId))
                .ReturnsAsync(fastTrackViewModel);

            mocks
                .cacheKeyService
                .Setup(s => s.CreateCacheKeyForFastTrackResults(fastTrackId))
                .ReturnsAsync(new FastTrackResultsCacheKey("publication", "release", fastTrackId));
            
            var result = await controller.Get(fastTrackId.ToString());
            VerifyAllMocks(mocks);
            
            result.AssertOkResult(fastTrackViewModel);
        }

        [Fact]
        public async Task Get_NotFound()
        {
            var (controller, mocks) = BuildControllerAndMocks();
            
            mocks
                .fastTrackService
                .Setup(s => s.GetFastTrackAndResults(fastTrackId))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.Get("InvalidGuid");
            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task Get_InvalidId()
        {
            var (controller, mocks) = BuildControllerAndMocks();
            var result = await controller.Get("InvalidGuid");
            result.AssertNotFoundResult();
        }
        
        private (
            FastTrackController controller,
            (
                Mock<IFastTrackService> fastTrackService,
                Mock<ICacheKeyService> cacheKeyService
            ) mocks
            ) BuildControllerAndMocks()
        {
            var fastTrackService = new Mock<IFastTrackService>(Strict);
            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            var controller = new FastTrackController(fastTrackService.Object, cacheKeyService.Object);
            
            return (
                controller,
                (
                    fastTrackService,
                    cacheKeyService
                )
            );
        }
    }
}
