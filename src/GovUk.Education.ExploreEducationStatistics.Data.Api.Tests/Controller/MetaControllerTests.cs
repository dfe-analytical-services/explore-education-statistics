using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class MetaControllerTests
    {
        private readonly MetaController _controller;

        public MetaControllerTests()
        {
            var publicationMetaService = new Mock<IPublicationMetaService>();

            publicationMetaService.Setup(s => s.GetPublication(new Guid("c102b638-a5ba-4579-aa29-0381f64df344")))
                .Returns(new PublicationSubjectsMetaViewModel());

            publicationMetaService.Setup(s => s.GetPublication(new Guid("31c7bf9c-bc0e-47f3-8341-36d3ee4d113a")))
                .Returns(new PublicationSubjectsMetaViewModel());

            publicationMetaService.Setup(s => s.GetThemes())
                .Returns(new List<ThemeMetaViewModel>
                {
                    new ThemeMetaViewModel()
                });

            _controller = new MetaController(publicationMetaService.Object);
        }

        [Fact]
        public void Get_Publication_Returns_Ok()
        {
            var result = _controller.GetPublication(new Guid("c102b638-a5ba-4579-aa29-0381f64df344"));

            Assert.IsAssignableFrom<PublicationSubjectsMetaViewModel>(result.Value);
        }

        [Fact]
        public void Get_Publication_Returns_NotFound()
        {
            var result = _controller.GetPublication(new Guid("9ad58d8b-997a-4dba-9255-d0caeae176ab"));

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Get_Themes_Returns_Ok()
        {
            var result = _controller.GetThemes();

            Assert.IsAssignableFrom<IEnumerable<ThemeMetaViewModel>>(result.Value);
        }
    }
}