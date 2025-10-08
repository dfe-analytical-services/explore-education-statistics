#nullable enable
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public record ZipDataSetFile(
    string Title,
    string DataFilename,
    string MetaFilename,
    long DataFileSize = 1048576,
    long MetaFileSize = 1024,
    File? ReplacingFile = null
);
