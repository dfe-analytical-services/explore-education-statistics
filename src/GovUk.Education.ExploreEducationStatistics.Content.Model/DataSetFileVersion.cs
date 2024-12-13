#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileVersion : ICreatedTimestamp<DateTime?>
{
    public Guid Id { get; set; }

    public Guid DataSetFileId { get; set; }

    public int Version { get; set; }

    public Guid RootPath { get; set; }

    public Guid SubjectId { get; set; }

    public string DataFilename { get; set; } = string.Empty;

    public string MetaFilename { get; set; } = string.Empty;

    public long DataContentLength { get; set; }

    public long MetaContentLength { get; set; }

    // public string DataContentType // @MarkFix probably don't need this?
    // public string MetaContentType // @MarkFix probably don't need this?

    public DataSetFileMeta? DataSetFileMeta { get; set; }

    public Guid? ReplacedById { get; set; }

    public DataSetFileVersion? ReplacedBy { get; set; }

    public DataSetFileVersion? Replacing { get; set; }

    public Guid? SourceZipFileId { get; set; }

    public File? SourceZipFile { get; set; }


    public DateTime? Created { get; set; }

    public User? CreatedBy { get; set; }

    public Guid? CreatedById { get; set; }
}
