using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class MethodologyControllerTests
    {
        [Fact]
        public async Task Get()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService.Setup(
                    s =>
                        s.GetDeserialized<MethodologyViewModel>("methodology/methodologies/test-slug.json"
                    )
                )
                .ReturnsAsync(
                    new MethodologyViewModel
                    {
                        Content = new List<ContentSectionViewModel>
                        {
                            new ContentSectionViewModel()
                        },
                        Annexes = new List<ContentSectionViewModel>
                        {
                            new ContentSectionViewModel()
                        }
                    }
                );

            var controller = new MethodologyController(fileStorageService.Object);

            var result = await controller.Get("test-slug");
            var methodologyViewModel = result.Value;

            Assert.IsType<MethodologyViewModel>(methodologyViewModel);

            Assert.Single(methodologyViewModel.Content);
            Assert.Single(methodologyViewModel.Annexes);
        }

        [Fact]
        public async Task Get_NotFound()
        {
            var fileStorageService = new Mock<IFileStorageService>();

            fileStorageService
                .Setup(s => s.GetDeserialized<MethodologyViewModel>(It.IsAny<string>()))
                .ReturnsAsync(new NotFoundResult());

            var controller = new MethodologyController(fileStorageService.Object);

            var result = await controller.Get("unknown-slug");

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}