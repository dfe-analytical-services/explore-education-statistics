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
        public async Task AdoptMethodology_Returns_Ok()
        {
            var publicationId = Guid.NewGuid();
            var methodologyId = Guid.NewGuid();

            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.AdoptMethodology(publicationId, methodologyId))
                .ReturnsAsync(Unit.Instance);

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.AdoptMethodology(publicationId, methodologyId);

            VerifyAllMocks(methodologyService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task CreateMethodology_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.CreateMethodology(_id))
                .ReturnsAsync(new MethodologyVersionViewModel());

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.CreateMethodology(_id);

            VerifyAllMocks(methodologyService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task DropMethodology_Returns_Ok()
        {
            var publicationId = Guid.NewGuid();
            var methodologyId = Guid.NewGuid();

            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.DropMethodology(publicationId, methodologyId))
                .ReturnsAsync(Unit.Instance);

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.DropMethodology(publicationId, methodologyId);

            VerifyAllMocks(methodologyService);

            result.AssertNoContent();
        }

        [Fact]
        public async Task GetAdoptableMethodologies_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.GetAdoptableMethodologies(_id))
                .ReturnsAsync(ListOf(new MethodologyVersionViewModel()));

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.GetAdoptableMethodologies(_id);

            VerifyAllMocks(methodologyService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task GetMethodology_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.GetMethodology(_id))
                .ReturnsAsync(new MethodologyVersionViewModel());

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.GetMethodology(_id);

            VerifyAllMocks(methodologyService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task GetMethodology_Returns_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.GetMethodology(_id))
                .ReturnsAsync(new NotFoundResult());

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.GetMethodology(_id);

            VerifyAllMocks(methodologyService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task GetUnpublishedReleasesUsingMethodology_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.GetUnpublishedReleasesUsingMethodology(_id))
                .ReturnsAsync(ListOf(new IdTitleViewModel()));

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.GetUnpublishedReleasesUsingMethodology(_id);

            VerifyAllMocks(methodologyService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task GetUnpublishedReleasesUsingMethodology_Returns_NotFound()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.GetUnpublishedReleasesUsingMethodology(_id))
                .ReturnsAsync(new NotFoundResult());

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.GetUnpublishedReleasesUsingMethodology(_id);

            VerifyAllMocks(methodologyService);

            result.AssertNotFoundResult();
        }

        [Fact]
        public async Task ListMethodologies_Returns_Ok()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.ListMethodologies(_id))
                .ReturnsAsync(ListOf(new MethodologyVersionSummaryViewModel()));

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.ListMethodologies(_id);

            VerifyAllMocks(methodologyService);

            result.AssertOkResult();
        }

        [Fact]
        public async Task UpdateMethodology_Returns_Ok()
        {
            var request = new MethodologyUpdateRequest();

            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.UpdateMethodology(_id, request))
                .ReturnsAsync(new MethodologyVersionViewModel());

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.UpdateMethodology(_id, request);

            VerifyAllMocks(methodologyService);

            result.AssertOkResult();
        }

        [Fact]
        public async void CreateMethodologyAmendment_Returns_Ok()
        {
            var methodologyAmendmentService = new Mock<IMethodologyAmendmentService>(Strict);

            methodologyAmendmentService
                .Setup(s => s.CreateMethodologyAmendment(_id))
                .ReturnsAsync(new MethodologyVersionViewModel());

            var controller =
                SetupMethodologyController(methodologyAmendmentService: methodologyAmendmentService.Object);

            var result = await controller.CreateMethodologyAmendment(_id);

            VerifyAllMocks(methodologyAmendmentService);

            result.AssertOkResult();
        }

        [Fact]
        public async void DeleteMethodologyVersion_Returns_NoContent()
        {
            var methodologyService = new Mock<IMethodologyService>(Strict);

            methodologyService
                .Setup(s => s.DeleteMethodologyVersion(_id, false))
                .ReturnsAsync(Unit.Instance);

            var controller = SetupMethodologyController(methodologyService.Object);

            var result = await controller.DeleteMethodologyVersion(_id);

            VerifyAllMocks(methodologyService);

            result.AssertNoContent();
        }

        private static MethodologyController SetupMethodologyController(
            IMethodologyService? methodologyService = null,
            IMethodologyAmendmentService? methodologyAmendmentService = null
        )
        {
            return new(
                methodologyService ?? Mock.Of<IMethodologyService>(Strict),
                methodologyAmendmentService ?? Mock.Of<IMethodologyAmendmentService>(Strict)
            );
        }
    }
}
