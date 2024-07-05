namespace GovUk.Education.ExploreEducationStatistics.Common.Model;

public class ArchiveDataSetFile
{
    public ArchiveDataSetFile(
        string title,
        string dataFilename,
        long dataFileSize,
        string metaFilename,
        long metaFileSize)
    {
        Title = title;
        DataFilename = dataFilename;
        DataFileSize = dataFileSize;
        MetaFilename = metaFilename;
        MetaFileSize = metaFileSize;
    }

    private string _title;
    public string Title
    {
        get => _title;
        set => _title = value.Trim();
    }

    private string _dataFilename;
    public string DataFilename
    {
        get => _dataFilename;
        set => _dataFilename = value.ToLower();
    }

    public long DataFileSize;

    private string _metaFilename;
    public string MetaFilename
    {
        get => _metaFilename;
        set => _metaFilename = value.ToLower();
    }

    public long MetaFileSize;
}
