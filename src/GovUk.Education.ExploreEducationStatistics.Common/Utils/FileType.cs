namespace GovUk.Education.ExploreEducationStatistics.Common.Utils
{
    public class FileType
    {
        public byte?[] Header { get; set; }
        public int HeaderOffset { get; set; }
        public string Extension { get; set; }
        public string Mime { get; set; }

        public FileType(byte?[] header, string extension, string mime)
        {
            Header = header;
            Extension = extension;
            Mime = mime;
            HeaderOffset = 0;
        }

        public FileType(byte?[] header, int offset, string extension, string mime)
        {
            Header = null;
            Header = header;
            HeaderOffset = offset;
            Extension = extension;
            Mime = mime;
        }
        
        public override bool Equals(object other)
        {
            if (!(other is FileType)) return false;

            var otherType = (FileType)other;

            if (Extension == otherType.Extension && Mime == otherType.Mime) return true;

            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return Extension;
        }
    }
}