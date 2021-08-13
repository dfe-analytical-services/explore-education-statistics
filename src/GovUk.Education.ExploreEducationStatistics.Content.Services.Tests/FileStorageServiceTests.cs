using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests
{
    public class FileStorageServiceTests
    {
        private class TestModel
        {
            public string Name { get; set; }
        }

        [Fact]
        public async Task GetDeserialized()
        {
            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.GetDeserializedJson<TestModel>(PublicContent, "test-path"))
                .ReturnsAsync(new TestModel
                {
                    Name = "Test"
                });

            var fileStorageService = new FileStorageService(blobStorageService.Object);

            var result = await fileStorageService.GetDeserialized<TestModel>("test-path");

            Assert.True(result.IsRight);
            Assert.Equal("Test", result.Right.Name);
        }

        [Fact]
        public async Task GetDeserialized_NotFound()
        {
            var blobStorageService = new Mock<IBlobStorageService>();

            blobStorageService
                .Setup(s => s.GetDeserializedJson<TestModel>(PublicContent, "test-path"))
                .ThrowsAsync(new FileNotFoundException("Blob not found"));

            var fileStorageService = new FileStorageService(blobStorageService.Object);

            var result = await fileStorageService.GetDeserialized<TestModel>("test-path");

            Assert.True(result.IsLeft);
            Assert.IsType<NotFoundResult>(result.Left);
        }
    }
}
