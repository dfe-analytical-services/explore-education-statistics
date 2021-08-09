#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Methodologies
{
    public class MethodologyControllerTests
    {
        private readonly Guid _id = Guid.NewGuid();

        [Fact]
        public async Task CreateMethodology_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyService
                .Setup(s => s.CreateMethodology(_id))
                .ReturnsAsync(new MethodologySummaryViewModel());

            var controller = new MethodologyController(methodologyService.Object, methodologyAmendmentService.Object);

            // Method under test
            var result = await controller.CreateMethodology(_id);
            result.AssertOkResult();

            VerifyAllMocks(methodologyService, methodologyAmendmentService);
        }

        [Fact]
        public async Task GetMethodologySummary_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyService
                .Setup(s => s.GetSummary(_id))
                .ReturnsAsync(new MethodologySummaryViewModel());

            var controller = new MethodologyController(methodologyService.Object, methodologyAmendmentService.Object);

            // Method under test
            var result = await controller.GetMethodologySummary(_id);
            result.AssertOkResult();

            VerifyAllMocks(methodologyService, methodologyAmendmentService);
        }

        [Fact]
        public async Task GetMethodologySummary_Returns_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyService
                .Setup(s => s.GetSummary(_id))
                .ReturnsAsync(new NotFoundResult());

            var controller = new MethodologyController(methodologyService.Object, methodologyAmendmentService.Object);

            // Method under test
            var result = await controller.GetMethodologySummary(_id);
            result.AssertNotFoundResult();

            VerifyAllMocks(methodologyService, methodologyAmendmentService);
        }

        [Fact]
        public async Task GetUnpublishedReleasesUsingMethodology_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyService
                .Setup(s => s.GetUnpublishedReleasesUsingMethodology(_id))
                .ReturnsAsync(AsList(new TitleAndIdViewModel()));

            var controller = new MethodologyController(methodologyService.Object, methodologyAmendmentService.Object);

            var result = await controller.GetUnpublishedReleasesUsingMethodology(_id);
            result.AssertOkResult();

            VerifyAllMocks(methodologyService, methodologyAmendmentService);
        }

        [Fact]
        public async Task GetUnpublishedReleasesUsingMethodology_Returns_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyService
                .Setup(s => s.GetUnpublishedReleasesUsingMethodology(_id))
                .ReturnsAsync(new NotFoundResult());

            var controller = new MethodologyController(methodologyService.Object, methodologyAmendmentService.Object);

            var result = await controller.GetUnpublishedReleasesUsingMethodology(_id);
            result.AssertNotFoundResult();

            VerifyAllMocks(methodologyService, methodologyAmendmentService);
        }

        [Fact]
        public async Task UpdateMethodology_Returns_Ok()
        {
            var request = new MethodologyUpdateRequest();

            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyService
                .Setup(s => s.UpdateMethodology(_id, request))
                .ReturnsAsync(new Either<ActionResult, MethodologySummaryViewModel>(new MethodologySummaryViewModel()));

            var controller = new MethodologyController(methodologyService.Object, methodologyAmendmentService.Object);

            // Method under test
            var result = await controller.UpdateMethodology(_id, request);
            result.AssertOkResult();

            VerifyAllMocks(methodologyService, methodologyAmendmentService);
        }

        [Fact]
        public async void CreateMethodologyAmendment_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyAmendmentService
                .Setup(s => s.CreateMethodologyAmendment(_id))
                .ReturnsAsync(new Either<ActionResult, MethodologySummaryViewModel>(new MethodologySummaryViewModel()));

            var controller = new MethodologyController(methodologyService.Object, methodologyAmendmentService.Object);

            // Method under test
            var result = await controller.CreateMethodologyAmendment(_id);
            result.AssertOkResult();

            VerifyAllMocks(methodologyService, methodologyAmendmentService);
        }

        [Fact]
        public async void DeleteMethodologyAmendment_Returns_NoContent()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyService
                .Setup(s => s.DeleteMethodology(_id, false))
                .ReturnsAsync(new Either<ActionResult, Unit>(Unit.Instance));

            var controller = new MethodologyController(methodologyService.Object, methodologyAmendmentService.Object);

            // Method under test
            var result = await controller.DeleteMethodology(_id);
            result.AssertNoContentResult();

            VerifyAllMocks(methodologyService, methodologyAmendmentService);
        }
    }
}
