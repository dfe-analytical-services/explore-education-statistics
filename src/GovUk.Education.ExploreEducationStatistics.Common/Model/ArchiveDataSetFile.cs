namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public class ArchiveDataSetFile
{
    public ArchiveDataSetFile(
        string dataSetFileName,
        string dataFileName,
        long dataFileSize,
        string metaFileName,
        long metaFileSize)
    {
        DataSetFileName = dataSetFileName;
        DataFileName = dataFileName;
        DataFileSize = dataFileSize;
        MetaFileName = metaFileName;
        MetaFileSize = metaFileSize;
    }

    public string DataSetFileName;

    private string _dataFileName;
    public string DataFileName
    {
        get => _dataFileName;
        set => _dataFileName = value.ToLower();
    }

    public long DataFileSize;

    private string _metaFileName;
    public string MetaFileName
    {
        get => _metaFileName;
        set => _metaFileName = value.ToLower();
    }

    public long MetaFileSize;
}
