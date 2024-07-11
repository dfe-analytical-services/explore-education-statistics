namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public class ArchiveDataSetFile(
    string title,
    string dataFilename,
    string metaFilename,
    long dataFileSize = 1048576,
    long metaFileSize = 1024)
{
    public string Title { get; } = title;

    public string DataFilename { get; } = dataFilename;

    public string MetaFilename { get; } = metaFilename;

    public long DataFileSize { get; } = dataFileSize;

    public long MetaFileSize { get; } = metaFileSize;
}
