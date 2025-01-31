using System;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils
{
    public class MultiWriteStreamTests
    {
        [Fact]
        public async Task Write()
        {
            await using var stream1 = new MemoryStream();
            await using var stream2 = new MemoryStream();

            var multiWriteStream = new MultiWriteStream(stream1, stream2);

            multiWriteStream.WriteText("Test text");

            stream1.Position = 0;
            stream2.Position = 0;

            Assert.Equal("Test text", stream1.ReadToEnd());
            Assert.Equal("Test text", stream2.ReadToEnd());
        }

        [Fact]
        public async Task Read_Throws()
        {
            await using var stream1 = new MemoryStream();
            await using var stream2 = new MemoryStream();

            var multiWriteStream = new MultiWriteStream(stream1, stream2);

            var buffer = new byte[] {};

            Assert.Throws<NotSupportedException>(() => multiWriteStream.Read(buffer, 0, 0));
        }

        [Fact]
        public async Task Seek_Throws()
        {
            await using var stream1 = new MemoryStream();
            await using var stream2 = new MemoryStream();

            var multiWriteStream = new MultiWriteStream(stream1, stream2);

            Assert.Throws<NotSupportedException>(() => multiWriteStream.Seek(0, SeekOrigin.Begin));
        }

        [Fact]
        public async Task Position_Throws()
        {
            await using var stream1 = new MemoryStream();
            await using var stream2 = new MemoryStream();

            var multiWriteStream = new MultiWriteStream(stream1, stream2);

            Assert.Throws<NotSupportedException>(() => { multiWriteStream.Position = 0; });
        }
    }
}