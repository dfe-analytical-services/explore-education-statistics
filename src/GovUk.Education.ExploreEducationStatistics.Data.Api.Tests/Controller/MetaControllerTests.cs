using System;
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

            publicationMetaService.Setup(s => s.GetPublicationMeta(new Guid("c102b638-a5ba-4579-aa29-0381f64df344")))
                .Returns(new PublicationMetaViewModel());
            
            publicationMetaService.Setup(s => s.GetPublicationMeta(new Guid("31c7bf9c-bc0e-47f3-8341-36d3ee4d113a")))
                .Returns(new PublicationMetaViewModel());
            
            _controller = new MetaController(publicationMetaService.Object);
        }

        [Fact]
        public void Get_PublicationMeta_Returns_Ok()
        {
            var result = _controller.GetPublicationMeta(new Guid("c102b638-a5ba-4579-aa29-0381f64df344"));

            Assert.IsAssignableFrom<PublicationMetaViewModel>(result.Value);
        }

        [Fact]
        public void Get_PublicationMeta_Returns_NotFound()
        {
            var result = _controller.GetPublicationMeta(new Guid("9ad58d8b-997a-4dba-9255-d0caeae176ab"));

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
    }
}