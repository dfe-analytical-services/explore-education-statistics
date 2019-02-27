//using System.Collections.Generic;
//using System.Linq;
//using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
//using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
//using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
//using Xunit;
//
//namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
//{
//    public class RegionControllerTests
//    {
//        private readonly RegionController _controller;
//        
//        public RegionControllerTests()
//        {
//            var reader = new CsvReader("../../../../../src/GovUk.Education.ExploreEducationStatistics.Data.Api");
//            _controller = new RegionController(reader);
//        }
//        
//        [Fact]
//        public void List_Regions()
//        {
//            var result =  _controller.List("schpupnum", null, null, new List<string>());
//            
//            var items = Assert.IsType<List<GeographicModel>>(result.Value);
//            Assert.True(items.Count() > 1);
//        }
//        
//        [Fact]
//        public void Get_Region()
//        {
//            var result = _controller.Get("schpupnum", "E12000001", null, null, new List<string>());
//            
//            var item = Assert.IsType<GeographicModel>(result.Value);
//            Assert.Equal("England", item.Country.Name);
//            Assert.Equal("North East", item.Region.Name);
//        }
//        
//    }
//}