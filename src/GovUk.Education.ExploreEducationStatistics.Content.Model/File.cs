#nullable enable
using System;
using System.Collections.Generic;
using Generator.Equals;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

[Equatable]
public partial class File : ICreatedTimestamp<DateTime?>
{
    public Guid Id { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public Guid RootPath { get; set; }

    public Guid? SubjectId { get; set; }

    public string Filename { get; set; } = string.Empty;

    public FileType Type { get; set; }

    public Guid? DataSetFileId { get; set; }

    public int? DataSetFileVersion { get; set; }

    [UnorderedEquality]
    public List<DataSetFileVersionGeographicLevel> DataSetFileVersionGeographicLevels { get; set; } = [];

    public DataSetFileMeta? DataSetFileMeta { get; set; }

    public List<DataSetFileFilterHierarchy>? FilterHierarchies { get; set; }

    public Guid? ReplacedById { get; set; }

    public File? ReplacedBy { get; set; }

    public Guid? ReplacingId { get; set; }

    public File? Replacing { get; set; }

    public Guid? SourceId { get; set; }

    public File? Source { get; set; }

    public long ContentLength { get; set; }

    public DateTime? Created { get; set; }

    public User? CreatedBy { get; set; }

    public Guid? CreatedById { get; set; }
}
