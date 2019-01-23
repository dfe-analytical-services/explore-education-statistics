//using System.Collections.Generic;
//using System.Linq;
//using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
//using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
//using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
//using Xunit;
//
//namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
//{
//    public class LocalAuthorityControllerTests
//    {
//        private readonly LocalAuthorityController _controller;
//        
//        public LocalAuthorityControllerTests()
//        {
//            var reader = new CsvReader("../../../../../src/GovUk.Education.ExploreEducationStatistics.Data.Api");
//            _controller = new LocalAuthorityController(reader);
//        }
//        
//        [Fact]
//        public void List_LAs()
//        {
//            var result =  _controller.List("schpupnum", null, null, new List<string>());
//            
//            var items = Assert.IsType<List<GeographicModel>>(result.Value);
//            Assert.True(items.Count() > 1);
//        }
//        
//        [Fact]
//        public void Get_LA_New_Code()
//        {
//            var result = _controller.Get("schpupnum", "E09000001", null, null, new List<string>());
//            
//            var item = Assert.IsType<GeographicModel>(result.Value);
//            Assert.Equal("England", item.Country.Name);
//            Assert.Equal("Inner London", item.Region.Name);
//            Assert.Equal("City of London", item.LocalAuthority.Name);
//        }
//        
//        [Fact]
//        public void Get_LA_Old_Code()
//        {
//            var result = _controller.Get("schpupnum", "201", null, null, new List<string>());
//            
//            var item = Assert.IsType<GeographicModel>(result.Value);
//            Assert.Equal("England", item.Country.Name);
//            Assert.Equal("Inner London", item.Region.Name);
//            Assert.Equal("City of London", item.LocalAuthority.Name);
//        }
//    }
//}