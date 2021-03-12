using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    // TODO EES-1991 Rename this or merge it into another service
    public class TestServiceTests
    {
        private const string ContentWithImages = @"
<div class=""dfe-content"">
  <h3>Text block with images</h3>
  <figure class=""image"">
    <img
      alt=""Image alt text""
      src=""/api/methodologies/{methodologyId}/images/eb4e27ac-29bd-4b21-9d76-08d8e53df837""
    />
    <figcaption>Image caption</figcaption>
  </figure>
  <p>More text…</p>
  <figure class=""image"">
    <img
      alt=""Image alt text""
      src=""/api/methodologies/{methodologyId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""
    />
    <figcaption>Image caption</figcaption>
  </figure>
  <p>More text…</p>
</div>
";

        [Fact]
        public void GetImages_NullContent()
        {
            var service = SetupTestService();
            Assert.Empty(service.GetImages(null));
        }

        [Fact]
        public void GetImages_EmptyContent()
        {
            var service = SetupTestService();
            Assert.Empty(service.GetImages(""));
        }

        [Fact]
        public void GetImages_MalformedContent()
        {
            var service = SetupTestService();

            var result = service.GetImages("Not Html content");

            Assert.Empty(result);
        }

        [Fact]
        public void GetImages_ContentWithoutImages()
        {
            var service = SetupTestService();

            var result = service.GetImages(@"
<div class=""dfe-content"">
  <h3>Text block without images</h3>
  <p>This text block has no images.</p>
</div>"
            );

            Assert.Empty(result);
        }

        [Fact]
        public void GetImages_ContentWithImages()
        {
            var service = SetupTestService();

            var result = service.GetImages(ContentWithImages);

            Assert.Equal(2, result.Count);
            Assert.Equal(Guid.Parse("eb4e27ac-29bd-4b21-9d76-08d8e53df837"), result[0]);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[1]);
        }

        [Fact]
        public void GetImages_ContentWithMalformedImage()
        {
            var service = SetupTestService();

            var result = service.GetImages(@"
    <img src=""/api/methodologies/{methodologyId}/images/not-a-valid-uuid""/>
    <img src=""/api/methodologies/{methodologyId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""/>"
            );

            Assert.Single(result);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[0]);
        }

        [Fact]
        public void GetImages_ContentWithOtherImages()
        {
            var service = SetupTestService();

            var result = service.GetImages(@"
    <img src=""some-other-image.png""/>
    <img src=""/images/some-other-image.png""/>
    <img src=""/images/03c51f5d-f2ef-4ed6-9fa2-0842b94bcebb""/>
    <img src=""/api/methodologies/{methodologyId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""/>"
            );

            Assert.Single(result);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[0]);
        }

        private static TestService SetupTestService()
        {
            return new TestService();
        }
    }
}
