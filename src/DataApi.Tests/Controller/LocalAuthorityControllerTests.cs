using System.Threading.Tasks;
using DataApi.Controllers;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DataApi.Tests.Controller
{
    public class LocalAuthorityControllerTests
    {
        [Fact]
        public void List_LAs()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new LocalAuthorityController(reader);

            // Act
            var result =  controller.List("schoolpupnum", null, null);
            
            Assert.IsAssignableFrom<ActionResult>(result);
            
            // expect more than one result
        }
        
        [Fact]
        public void Get_LA_New_Code()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new LocalAuthorityController(reader);

            // Act
            var result = controller.Get("schoolpupnum", "E09000001", null, null);
            
            Assert.IsAssignableFrom<ActionResult>(result);
            
            // expect  name to be "City of London"
        }
        
        [Fact]
        public void Get_LA_Old_Code()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new LocalAuthorityController(reader);

            // Act
            var result = controller.Get("schoolpupnum", "201", null, null);
            
            Assert.IsAssignableFrom<ActionResult>(result);
            
            // expect  name to be "City of London"
        }
        
    }
}