using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions
{
    public class StreamExtensionTests
    {
        public class ComputeMd5Hash
        {
            [Fact]
            public void ReturnsHashString()
            {
                var stream = "Test stream".ToStream();

                Assert.Equal("8fa78f2f3709f68caf0839b73c121ee6", stream.ComputeMd5Hash());
            }

            [Fact]
            public void ResetsStreamPosition()
            {
                var stream = "Test stream".ToStream();

                Assert.Equal(0, stream.Position);

                stream.ComputeMd5Hash();

                Assert.Equal(0, stream.Position);
            }
        }
    }
}