using System.Collections.Generic;
using System.Linq;
using DataApi.Controllers;
using DataApi.Models;
using Xunit;

namespace DataApi.Tests.Controller
{
    public class CountryControllerTests
    {
        private CountryController _controller;
        
        public CountryControllerTests()
        {
            var reader = new CsvReader("../../../../../");
            _controller = new CountryController(reader);
        }
        
        [Fact]
        public void List_Countries()
        {
            var result =  _controller.List("schpupnum", null, null, new List<string>());
            
            var items = Assert.IsType<List<GeographicModel>>(result.Value);
            Assert.True(items.Count() > 1);
        }
        
        [Fact]
        public void Get_Country()
        {
            var result = _controller.Get("schpupnum","E92000001", null, null, new List<string>());
            
            var item = Assert.IsType<GeographicModel>(result.Value);
            Assert.Equal("England", item.Country.Name);
        }  
    }
}