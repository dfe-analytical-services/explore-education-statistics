using System.IO.Compression;

namespace GovUk.Education.ExploreEducationStatistics.Common.Model
{
    public class ArchiveDataSetFile // @MarkFix refactor this in and IDataArchiveFile out
    {
        public ArchiveDataSetFile(
            string dataSetName,
            string dataFileName,
            long dataFileSize,
            string metaFileName,
            long metaFileSize)
        {
            DataSetName = dataSetName;
            DataFileName = dataFileName;
            DataFileSize = dataFileSize;
            MetaFileName = metaFileName;
            MetaFileSize = metaFileSize;
        }

        public string DataSetName;

        private string _dataFileName;
        public string DataFileName
        {
            get { return _dataFileName; }
            set { _dataFileName = value.ToLower(); }
        }

        public long DataFileSize;

        private string _metaFileName;
        public string MetaFileName
        {
            get { return _metaFileName; }
            set { _metaFileName = value.ToLower(); }
        }

        public long MetaFileSize;
    }

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

    public class BulkDataArchiveFile : IDataArchiveFile
    {
        private readonly ZipArchiveEntry _dataFile;
        private readonly ZipArchiveEntry _metaFile;

        public BulkDataArchiveFile(string dataSetName, ZipArchiveEntry dataFile, ZipArchiveEntry metaFile)
        {
            DataSetName = dataSetName;
            _dataFile = dataFile;
            _metaFile = metaFile;
        }

        public string DataSetName;

        public string DataFileName => _dataFile.Name.ToLower();
        public string MetaFileName => _metaFile.Name.ToLower();

        public long DataFileSize => _dataFile.Length;
        public long MetaFileSize => _metaFile.Length;
    }
}
