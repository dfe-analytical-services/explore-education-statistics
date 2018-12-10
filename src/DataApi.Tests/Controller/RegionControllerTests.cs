using System.Threading.Tasks;
using DataApi.Controllers;
using DataApi.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DataApi.Tests.Controller
{
    public class RegionControllerTests
    {
        [Fact]
        public void List_Regions()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new RegionController(reader);

            // Act
            var result =  controller.List("schoolpupnum", null, null);
            
            Assert.IsAssignableFrom<ActionResult>(result);
            
            // expect more than one result
        }
        
        [Fact]
        public void Get_Region()
        {
            // Arrange
            var reader = new CsvReader();
            var controller = new RegionController(reader);

            // Act
            var result = controller.Get("schoolpupnum", "E12000001", null, null);
            
            Assert.IsAssignableFrom<ActionResult>(result);
            
            // expect region name to be "North East"
        }
        
    }
}