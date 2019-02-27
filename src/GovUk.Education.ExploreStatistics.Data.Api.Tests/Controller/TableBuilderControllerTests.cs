using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Data.Api.Tests.Controller
{
    public class TableBuilderControllerTests
    {

        private TableBuilderController _controller;

        public TableBuilderControllerTests()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();
            var attributeMetaService = new Mock<IAttributeMetaService>();
            var characteristicMetaService = new Mock<ICharacteristicMetaService>();
            
            _controller = new TableBuilderController(tableBuilderService.Object, attributeMetaService.Object, characteristicMetaService.Object);
        }

        [Fact]
        public void GetGeographic()
        {
            
        }
        
        [Fact]
        public void PostGeographic()
        {
            
        }
        
        [Fact]
        public void GetLocalAuthority()
        {
            
        }
        
        [Fact]
        public void PostLocalAuthority()
        {
            
        }
        
        [Fact]
        public void GetNational()
        {
            
        }
        
        [Fact]
        public void PostNational()
        {
            
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
    }
}