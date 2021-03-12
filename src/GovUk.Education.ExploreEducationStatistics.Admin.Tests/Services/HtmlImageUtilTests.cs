using System;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class HtmlImageUtilTests
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
            Assert.Empty(HtmlImageUtil.GetImages(null));
        }

        [Fact]
        public void GetImages_EmptyContent()
        {
            Assert.Empty(HtmlImageUtil.GetImages(""));
        }

        [Fact]
        public void GetImages_MalformedContent()
        {
            var result = HtmlImageUtil.GetImages("Not Html content");

            Assert.Empty(result);
        }

        [Fact]
        public void GetImages_ContentWithoutImages()
        {
            var result = HtmlImageUtil.GetImages(@"
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
            var result = HtmlImageUtil.GetImages(ContentWithImages);

            Assert.Equal(2, result.Count);
            Assert.Equal(Guid.Parse("eb4e27ac-29bd-4b21-9d76-08d8e53df837"), result[0]);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[1]);
        }

        [Fact]
        public void GetImages_ContentWithMalformedImage()
        {
            var result = HtmlImageUtil.GetImages(@"
    <img src=""/api/methodologies/{methodologyId}/images/not-a-valid-uuid""/>
    <img src=""/api/methodologies/{methodologyId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""/>"
            );

            Assert.Single(result);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[0]);
        }

        [Fact]
        public void GetImages_ContentWithOtherImages()
        {
            var result = HtmlImageUtil.GetImages(@"
    <img src=""some-other-image.png""/>
    <img src=""/images/some-other-image.png""/>
    <img src=""/images/03c51f5d-f2ef-4ed6-9fa2-0842b94bcebb""/>
    <img src=""/api/methodologies/{methodologyId}/images/8205b65b-9fd4-40b9-9d77-08d8e53df837""/>"
            );

            Assert.Single(result);
            Assert.Equal(Guid.Parse("8205b65b-9fd4-40b9-9d77-08d8e53df837"), result[0]);
        }
    }
}
