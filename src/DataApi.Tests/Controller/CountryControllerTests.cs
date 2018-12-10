using System.Threading.Tasks;
using DataApi.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DataApi.Tests.Controller
{
    public class CountryControllerTests
    {
        [Fact]
        public void List_Countries()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new CountryController(reader);

            // Act
            var result =  controller.List("schoolpupnum", null, null);
            
            Assert.IsAssignableFrom<ActionResult>(result);
        }
        
        [Fact]
        public void Get_Country()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new CountryController(reader);

            // Act
            var result = controller.Get("schoolpupnum", 0, "E92000001", null, null);
            
            Assert.IsAssignableFrom<ActionResult>(result);
        }
        
    }
}