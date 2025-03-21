using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models;

public class DataSetDto
{
    public string DataFileName { get; set; }
    public long DataFileSize { get; set; }
    public MemoryStream DataFileStream { get; set; }

    public string MetaFileName { get; set; }
    public long MetaFileSize { get; set; }
    public MemoryStream MetaFileStream { get; set; }
}
