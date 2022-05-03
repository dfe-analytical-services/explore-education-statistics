#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
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
        private static readonly Guid SubjectId = Guid.NewGuid();

        private static readonly ObservationQueryContext QueryContext = new()
        {
            SubjectId = SubjectId
        };

        [Fact]
        public async Task GetSubjectMeta()
        {
            var subjectMetaViewModel = new SubjectMetaViewModel();

            var (controller, subjectMetaService) = BuildControllerAndMocks();

            subjectMetaService
                .Setup(s => s.GetCachedSubjectMeta(SubjectId))
                .ReturnsAsync(subjectMetaViewModel);

            var result = await controller.GetSubjectMeta(SubjectId);
            VerifyAllMocks(subjectMetaService);

            result.AssertOkResult(subjectMetaViewModel);
        }

        [Fact]
        public async Task GetSubjectMeta_NotFound()
        {
            var (controller, subjectMetaService) = BuildControllerAndMocks();

            subjectMetaService
                .Setup(s => s.GetCachedSubjectMeta(SubjectId))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetSubjectMeta(SubjectId);
            VerifyAllMocks(subjectMetaService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task Post_GetSubjectMeta()
        {
            var subjectMetaViewModel = new SubjectMetaViewModel();
            var cancellationToken = new CancellationTokenSource().Token;

            var (controller, subjectMetaService) = BuildControllerAndMocks();

            subjectMetaService
                .Setup(s => s.GetSubjectMeta(QueryContext, cancellationToken))
                .ReturnsAsync(subjectMetaViewModel);

            var result = await controller.GetSubjectMeta(QueryContext, cancellationToken);
            VerifyAllMocks(subjectMetaService);

            result.AssertOkResult(subjectMetaViewModel);
        }

        [Fact]
        public async Task Post_GetSubjectMeta_NotFound()
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var (controller, subjectMetaService) = BuildControllerAndMocks();

            subjectMetaService
                .Setup(s => s.GetSubjectMeta(QueryContext, cancellationToken))
                .ReturnsAsync(new NotFoundResult());

            var result = await controller.GetSubjectMeta(QueryContext, cancellationToken);
            VerifyAllMocks(subjectMetaService);

            result.AssertNotFoundResult();
        }

        private static (
            TableBuilderMetaController controller,
            Mock<ISubjectMetaService> subjectMetaService)
            BuildControllerAndMocks()
        {
            var subjectMetaService = new Mock<ISubjectMetaService>(Strict);
            var controller = new TableBuilderMetaController(subjectMetaService.Object);

            return (controller, subjectMetaService);
        }
    }
}
