using System.IO.Compression;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public interface IDataArchiveFile
    {
        public string DataFileName { get; }
        public string MetaFileName { get; }

        public long DataFileSize { get; }
        public long MetaFileSize { get; }
    }

    public class DataArchiveFile : IDataArchiveFile
    {
        private readonly ZipArchiveEntry _dataFile;
        private readonly ZipArchiveEntry _metaFile;

        public DataArchiveFile()
        {
        }

        public DataArchiveFile(ZipArchiveEntry dataFile, ZipArchiveEntry metaFile)
        {
            _dataFile = dataFile;
            _metaFile = metaFile;
        }

        public string DataFileName => _dataFile.Name.ToLower();
        public string MetaFileName => _metaFile.Name.ToLower();

        public long DataFileSize => _dataFile.Length;
        public long MetaFileSize => _metaFile.Length;
    }
}