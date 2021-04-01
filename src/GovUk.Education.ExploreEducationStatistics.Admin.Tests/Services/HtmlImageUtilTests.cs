using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class HtmlImageUtilTests
    {
        private const string ContentWithMethodologyImages = @"
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

        private const string ContentWithReleaseImages = @"
<div class=""dfe-content"">
  <h3>Text block with images</h3>
  <figure class=""image"">
    <img
      alt=""Image alt text""
      src=""/api/releases/{releaseId}/images/eb4e27ac-29bd-4b21-9d76-08d8e53df837""
    />
    <figcaption>Image caption</figcaption>
  </figure>
  <p>More text…</p>
  <figure class=""image"">
    <img
      alt=""Image alt text""
      src=""/api/releases/{releaseId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""
    />
    <figcaption>Image caption</figcaption>
  </figure>
  <p>More text…</p>
</div>
";

        [Fact]
        public void GetMethodologyImages_NullContent()
        {
            Assert.Empty(HtmlImageUtil.GetMethodologyImages(null));
        }

        [Fact]
        public void GetMethodologyImages_EmptyContent()
        {
            Assert.Empty(HtmlImageUtil.GetMethodologyImages(""));
        }

        [Fact]
        public void GetMethodologyImages_MalformedContent()
        {
            var result = HtmlImageUtil.GetMethodologyImages("Not Html content");

            Assert.Empty(result);
        }

        [Fact]
        public void GetMethodologyImages_ContentWithoutImages()
        {
            var result = HtmlImageUtil.GetMethodologyImages(@"
<div class=""dfe-content"">
  <h3>Text block without images</h3>
  <p>This text block has no images.</p>
</div>"
            );

            Assert.Empty(result);
        }

        [Fact]
        public void GetMethodologyImages_ContentWithImages()
        {
            var result = HtmlImageUtil.GetMethodologyImages(ContentWithMethodologyImages);

            Assert.Equal(2, result.Count);
            Assert.Equal(Guid.Parse("eb4e27ac-29bd-4b21-9d76-08d8e53df837"), result[0]);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[1]);
        }

        [Fact]
        public void GetMethodologyImages_ContentWithMalformedImage()
        {
            var result = HtmlImageUtil.GetMethodologyImages(@"
    <img src=""/api/methodologies/{methodologyId}/images/not-a-valid-uuid""/>
    <img src=""/api/methodologies/{methodologyId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""/>"
            );

            Assert.Single(result);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[0]);
        }

        [Fact]
        public void GetMethodologyImages_ContentWithOtherImages()
        {
            var result = HtmlImageUtil.GetMethodologyImages(@"
    <img src=""some-other-image.png""/>
    <img src=""/images/some-other-image.png""/>
    <img src=""/images/03c51f5d-f2ef-4ed6-9fa2-0842b94bcebb""/>
    <img src=""/api/methodologies/{methodologyId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""/>"
            );

            Assert.Single(result);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[0]);
        }

        [Fact]
        public void GetReleaseImages_NullContent()
        {
            Assert.Empty(HtmlImageUtil.GetReleaseImages(null));
        }

        [Fact]
        public void GetReleaseImages_EmptyContent()
        {
            Assert.Empty(HtmlImageUtil.GetReleaseImages(""));
        }

        [Fact]
        public void GetReleaseImages_MalformedContent()
        {
            var result = HtmlImageUtil.GetReleaseImages("Not Html content");

            Assert.Empty(result);
        }

        [Fact]
        public void GetReleaseImages_ContentWithoutImages()
        {
            var result = HtmlImageUtil.GetReleaseImages(@"
<div class=""dfe-content"">
  <h3>Text block without images</h3>
  <p>This text block has no images.</p>
</div>"
            );

            Assert.Empty(result);
        }

        [Fact]
        public void GetReleaseImages_ContentWithImages()
        {
            var result = HtmlImageUtil.GetReleaseImages(ContentWithReleaseImages);

            Assert.Equal(2, result.Count);
            Assert.Equal(Guid.Parse("eb4e27ac-29bd-4b21-9d76-08d8e53df837"), result[0]);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[1]);
        }

        [Fact]
        public void GetReleaseImages_ContentWithMalformedImage()
        {
            var result = HtmlImageUtil.GetReleaseImages(@"
    <img src=""/api/releases/{releaseId}/images/not-a-valid-uuid""/>
    <img src=""/api/releases/{releaseId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""/>"
            );

            Assert.Single(result);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[0]);
        }

        [Fact]
        public void GetReleaseImages_ContentWithOtherImages()
        {
            var result = HtmlImageUtil.GetReleaseImages(@"
    <img src=""some-other-image.png""/>
    <img src=""/images/some-other-image.png""/>
    <img src=""/images/03c51f5d-f2ef-4ed6-9fa2-0842b94bcebb""/>
    <img src=""/api/releases/{releaseId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""/>"
            );

            Assert.Single(result);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[0]);
        }
    }
}
