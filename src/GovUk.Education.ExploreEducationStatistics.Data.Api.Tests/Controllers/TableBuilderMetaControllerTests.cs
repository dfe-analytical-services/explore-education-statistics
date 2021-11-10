using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class TableBuilderMetaControllerTests
    {
        private readonly TableBuilderMetaController _controller;

        private static readonly Guid SubjectId = Guid.NewGuid();
        private static readonly SubjectMetaQueryContext QueryContext = new SubjectMetaQueryContext
        {
            SubjectId = SubjectId
        };

        [Fact]
        public async Task Get_SubjectMetaAsync_Returns_Ok()
        {
            var subjectMetaViewModel = new SubjectMetaViewModel();
            
            var (controller, mocks) = BuildControllerAndMocks();

            mocks
                .subjectMetaService
                .Setup(s => s.GetSubjectMeta(SubjectId))
                .ReturnsAsync(subjectMetaViewModel);

            mocks
                .cacheKeyService
                .Setup(s => s.CreateCacheKeyForSubjectMeta(SubjectId))
                .ReturnsAsync(new SubjectMetaCacheKey("publication", "release", SubjectId));
            
            var result = await controller.GetSubjectMetaAsync(SubjectId);
            VerifyAllMocks(mocks);

            result.AssertOkResult(subjectMetaViewModel);
        }

        [Fact]
        public async Task Get_SubjectMetaAsync_Returns_NotFound()
        {
            var (controller, mocks) = BuildControllerAndMocks();

            mocks
                .subjectMetaService
                .Setup(s => s.GetSubjectMeta(SubjectId))
                .ReturnsAsync(new NotFoundResult());

            mocks
                .cacheKeyService
                .Setup(s => s.CreateCacheKeyForSubjectMeta(SubjectId))
                .ReturnsAsync(new SubjectMetaCacheKey("publication", "release", SubjectId));
            
            var result = await controller.GetSubjectMetaAsync(SubjectId);
            VerifyAllMocks(mocks);
            
            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task Post_SubjectMetaAsync_Returns_Ok()
        {
            var subjectMetaViewModel = new SubjectMetaViewModel();

            var (controller, mocks) = BuildControllerAndMocks();

            mocks
                .subjectMetaService
                .Setup(s => s.GetSubjectMeta(QueryContext))
                .ReturnsAsync(subjectMetaViewModel);

            var result = await controller.GetSubjectMetaAsync(QueryContext);
            VerifyAllMocks(mocks);

            result.AssertOkResult(subjectMetaViewModel);
        }

        [Fact]
        public async Task Post_SubjectMetaAsync_Returns_NotFound()
        {
            var (controller, mocks) = BuildControllerAndMocks();

            mocks
                .subjectMetaService
                .Setup(s => s.GetSubjectMeta(QueryContext))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetSubjectMetaAsync(QueryContext);
            VerifyAllMocks(mocks);
            
            result.AssertNotFoundResult();
        }

        private (
            TableBuilderMetaController controller,
            (
            Mock<ISubjectMetaService> subjectMetaService,
            Mock<ICacheKeyService> cacheKeyService
            ) mocks
            ) BuildControllerAndMocks()
        {
            var subjectMetaService = new Mock<ISubjectMetaService>(Strict);
            var cacheKeyService = new Mock<ICacheKeyService>(Strict);
            var controller = new TableBuilderMetaController(subjectMetaService.Object, cacheKeyService.Object);
            
            return (
                controller,
                (
                    subjectMetaService,
                    cacheKeyService
                )
            );
        }
    }
}