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

        [Fact]
        public void GetMeta()
        {
            var result = _controller.GetMeta(new Guid()); // PublicationId
            
            var model = Assert.IsAssignableFrom<ActionResult<PublicationMetaViewModel>>(result);
        }
        
        
    }
}