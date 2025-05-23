#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record ZipDataSetFile(
    string Title,
    string DataFilename,
    string MetaFilename,
    long DataFileSize = 1048576,
    long MetaFileSize = 1024,
    File? ReplacingFile = null);
