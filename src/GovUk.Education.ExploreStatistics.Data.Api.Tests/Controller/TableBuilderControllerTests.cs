using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Data.Api.Tests.Controller
{
    public class TableBuilderControllerTests
    {
        private readonly TableBuilderController _controller;
        
        private NationalQueryContext _testNationQuery = new NationalQueryContext();
        private LaQueryContext _laQuery = new LaQueryContext();
        private GeographicQueryContext _geographicQuery = new GeographicQueryContext();

        public TableBuilderControllerTests()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();
            var attributeMetaService = new Mock<IAttributeMetaService>();
            var characteristicMetaService = new Mock<ICharacteristicMetaService>();

            tableBuilderService.Setup(s => s.GetNational(_testNationQuery)).Returns(new TableBuilderResult { Result = new List<ITableBuilderData>()});
            tableBuilderService.Setup(s => s.GetLocalAuthority(_laQuery)).Returns(new TableBuilderResult { Result = new List<ITableBuilderData>()});
            tableBuilderService.Setup(s => s.GetGeographic(_geographicQuery)).Returns(new TableBuilderResult { Result = new List<ITableBuilderData>()});
            
            _controller = new TableBuilderController(tableBuilderService.Object, attributeMetaService.Object, characteristicMetaService.Object);
        }

        [Fact]
        public void GetGeographic()
        {
            
        }
        
        [Fact]
        public void PostGeographic()
        {
            var result = _controller.GetGeographic(_geographicQuery);
            
            Assert.IsAssignableFrom<TableBuilderResult>(result.Result);
        }
        
        [Fact]
        public void PostGeographic_Returns_NotFound()
        {
            var result = _controller.GetGeographic(_geographicQuery);
            
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
        
        [Fact]
        public void GetLocalAuthority()
        {
            
        }
        
        [Fact]
        public void PostLocalAuthority()
        {
            var result = _controller.GetLocalAuthority(_laQuery);
            
            Assert.IsAssignableFrom<TableBuilderResult>(result.Result);
        }
        
        [Fact]
        public void PostLocalAuthority_Returns_NotFound()
        {
            var result = _controller.GetLocalAuthority(_laQuery);
            
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
        
        [Fact]
        public void GetNational_GetRequest_Returns_National_Data()
        {
            
        }
        
        [Fact]
        public void GetNational_PostRequest_Returns_National_Data()
        {
            var result = _controller.GetNational(_testNationQuery);
            
            Assert.IsAssignableFrom<TableBuilderResult>(result.Result);
        }

        [Theory]
        [InlineData("af3c435c-df6e-4ab8-aac0-53a0ad03f57e")]
        [InlineData("8fe9c479-1ab5-4894-81cd-9f87882e20ed")]
        public void GetMeta_KnownId_Returns_ActionResult_WithMetaData(string testId)
        {
            var id = new Guid(testId);
            
            var result = _controller.GetMeta(id);
            
            var model = Assert.IsAssignableFrom<ActionResult<PublicationMetaViewModel>>(result);
            
            Assert.Equal(id, model.Value.PublicationId);
            
            // TODO: verify the meta data
        }
        
        [Fact]
        public void GetMeta_UnknownKnownId_Returns_NotFound()
        {
            var result = _controller.GetMeta(new Guid());
            
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}