using System.Threading.Tasks;
using DataApi.Controllers;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DataApi.Tests.Controller
{
    public class SchoolControllerTests
    {
        [Fact]
        public void List_Schools()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new SchoolController(reader);

            // Act
            var result =  controller.List("schoolpupnum", null, null);
            
            Assert.IsAssignableFrom<ActionResult>(result);
            
            // expect more than one result
        }
        
        [Fact]
        public void Get_School()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new SchoolController(reader);

            // Act
            var result = controller.Get("schoolpupnum", "2013614");
            
            Assert.IsAssignableFrom<ActionResult>(result);
            
            // expect region to be "Inner London"
            // expect la name to be "City of London"
        }
        
    }
}