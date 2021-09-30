#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    /// <summary>
    /// A stream implementation that wraps and writes to multiple target streams.
    /// <para>
    /// It should be noted that this stream is only intended for writing,
    /// and is not intended to be seekable or readable.
    /// </para>
    /// </summary>
    public class MultiWriteStream : Stream
    {
        private readonly List<Stream> _streams;

        public MultiWriteStream(params Stream[] streams)
        {
            _streams = streams.ToList();
        }

        public override ValueTask DisposeAsync()
        {
            try
            {
                foreach (var stream in _streams)
                {
                    stream.Dispose();
                }

                return default;
            }
            catch (Exception exc)
            {
                return new ValueTask(Task.FromException(exc));
            }
        }

        public override void Flush()
        {
            foreach (var stream in _streams)
            {
                stream.Flush();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            foreach (var stream in _streams)
            {
                stream.SetLength(value);
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            foreach (var stream in _streams)
            {
                if (stream.CanWrite)
                {
                    stream.Write(buffer, offset, count);
                }
            }
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => _streams.Any(x => x.CanWrite);

        public override long Length => -1;

        public override long Position
        {
            get => -1;
            set => throw new NotSupportedException();
        }
    }
}