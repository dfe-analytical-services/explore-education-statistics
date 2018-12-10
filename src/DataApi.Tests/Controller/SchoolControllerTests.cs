using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataApi.Controllers;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DataApi.Tests.Controller
{
    public class SchoolControllerTests
    {
        private readonly SchoolController _controller;
        
        public SchoolControllerTests()
        {
            var reader = new CsvReader("../../../../../");
            _controller = new SchoolController(reader);
        }
        
        [Fact]
        public void List_Schools()
        {
            var result =  _controller.List("schpupnum", null, null, null);
            
            var items = Assert.IsType<List<GeographicModel>>(result.Value);
            Assert.True(items.Count() > 1);
        }
        
        [Fact]
        public void Get_School()
        {
            var result = _controller.Get("schpupnum", "2013614", null);
            
            var item = Assert.IsType<GeographicModel>(result.Value);
            Assert.Equal("England", item.Country.Name);
            Assert.Equal("Inner London", item.Region.Name);
            Assert.Equal("City of London", item.LocalAuthority.Name);
        }
    }
}