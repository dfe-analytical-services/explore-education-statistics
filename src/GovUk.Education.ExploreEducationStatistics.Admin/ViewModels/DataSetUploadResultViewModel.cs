#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record DataSetUploadResultViewModel
{
    public string Title { get; set; } = string.Empty;

    public string DataFileName { get; set; } = string.Empty;

    public Guid DataFileId { get; set; }

    public long DataFileSize { get; set; }

    public string MetaFileName { get; set; } = string.Empty;

    public Guid MetaFileId { get; set; }

    public long MetaFileSize { get; set; }

    public Guid? ReplacingFileId { get; set; }
}
