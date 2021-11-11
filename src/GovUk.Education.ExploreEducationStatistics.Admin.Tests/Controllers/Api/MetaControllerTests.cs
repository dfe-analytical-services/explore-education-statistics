using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api
{
    public class MetaControllerTests
    {
        [Fact]
        public void Get_TimeIdentifiersByCategory_Returns_Ok()
        {
            var metaService = new MetaService(null /* do not need */);
            var controller =
                new MetaController(metaService);

            // Method under test
            var result = controller.GetTimeIdentifiersByCategory();
            Assert.IsAssignableFrom<List<TimeIdentifierCategoryModel>>(result.Value);
        }
        
        
        [Fact]
        public void Get_ReleaseTypes_Returns_Ok()
        {
            var metaService = new Mock<IMetaService>();
            metaService.Setup(s => s.GetReleaseTypes()).Returns(
                new List<ReleaseType>
                {
                    new ReleaseType()
                    {
                        Title = "Ad Hoc Statistics"
                    }
                }
            );
            var controller = new MetaController(metaService.Object);
            
            // Method under test
            var result = controller.GetReleaseTypes();
            Assert.IsAssignableFrom<List<ReleaseType>>(result.Value);
        }
    }
}