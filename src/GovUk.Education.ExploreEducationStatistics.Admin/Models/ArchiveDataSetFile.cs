#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record ArchiveDataSetFile(
    string Title,
    string DataFilename,
    string MetaFilename,
    long DataFileSize = 1048576,
    long MetaFileSize = 1024);
