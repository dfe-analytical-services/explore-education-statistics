using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class SchoolControllerTests
    {
        private readonly SchoolController _controller;
        
        public SchoolControllerTests()
        {
            var reader = new CsvReader("../../../../../src/GovUk.Education.ExploreEducationStatistics.Data.Api");
            _controller = new SchoolController(reader);
        }
        
        [Fact]
        public void List_Schools()
        {
            var result =  _controller.List("schpupnum", null, null, new List<string>());
            
            var items = Assert.IsType<List<GeographicModel>>(result.Value);
            Assert.True(items.Count() > 1);
        }
        
        [Fact]
        public void Get_School()
        {
            var result = _controller.Get("schpupnum", "2013614", null, new List<string>());
            
            var item = Assert.IsType<GeographicModel>(result.Value);
            Assert.Equal("England", item.Country.Name);
            Assert.Equal("Inner London", item.Region.Name);
            Assert.Equal("City of London", item.LocalAuthority.Name);
        }
    }
}