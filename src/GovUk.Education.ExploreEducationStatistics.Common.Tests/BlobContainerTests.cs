using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests
{
    public class BlobContainerTests
    {
        [Fact]
        public void Name()
        {
            var container = new BlobContainer("container");
            Assert.Equal("container", container.Name);
            Assert.Equal("container", container.EmulatedName);
        }
    }

    public class PublicBlobContainerTests
    {
        [Fact]
        public void Name()
        {
            var container = new PublicBlobContainer("container");
            Assert.Equal("container", container.Name);
            Assert.Equal("public-container", container.EmulatedName);
        }
    }

    public class PrivateBlobContainerTests
    {
        [Fact]
        public void Test()
        {
            var container = new PrivateBlobContainer("container");
            Assert.Equal("container", container.Name);
            Assert.Equal("private-container", container.EmulatedName);
        }
    }
}
