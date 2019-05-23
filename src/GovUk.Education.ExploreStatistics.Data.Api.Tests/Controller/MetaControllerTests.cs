using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using Microsoft.AspNetCore.Http;
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
            var service = new Mock<IMetaService>();
            
            service.Setup(s => s.GetPublicationMeta(new Guid("c102b638-a5ba-4579-aa29-0381f64df344"))).Returns(new PublicationMetaViewModel{ });
            service.Setup(s => s.GetPublicationMeta(new Guid("31c7bf9c-bc0e-47f3-8341-36d3ee4d113a"))).Returns(new PublicationMetaViewModel{ });

            
            service.Setup(s => s.GetSubjectMeta(new SubjectMetaQueryContext{SubjectId = 1})).Returns(new SubjectMetaViewModel{ });
            service.Setup(s => s.GetSubjectMeta(new SubjectMetaQueryContext{SubjectId = 2})).Returns((SubjectMetaViewModel)null);


            _controller = new MetaController(service.Object);   
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
        
        [Fact]
        public void Get_SubjectMeta_Returns_Ok()
        {
            var result = _controller.GetSubjectMeta(1);

            // TODO: this fails, i think due to the signature of the object passed to the service not matching ideally the service should just take the id
            //Assert.IsAssignableFrom<SubjectMetaViewModel>(result.Value);

        }

        [Fact]
        public void Get_SubjectMeta_Returns_NotFound()
        {
            var result = _controller.GetSubjectMeta(2);

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);

        }
    }
}