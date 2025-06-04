#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

// TODO: Remove?
public record DataSetUploadResultViewModel
{
    public string Title { get; set; } = string.Empty;

    public required string DataFileName { get; set; }

    public required string DataFilePath { get; set; }

    public Guid DataFileId { get; set; }

    public long DataFileSize { get; set; }

    public required string MetaFileName { get; set; }

    public required string MetaFilePath { get; set; }

    public Guid MetaFileId { get; set; }

    public long MetaFileSize { get; set; }

    public Guid? ReplacingFileId { get; set; }

    public DataSetScreenerResult? ScreenerResult { get; set; }
}
