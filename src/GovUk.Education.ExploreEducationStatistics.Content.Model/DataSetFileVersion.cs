#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileVersion : ICreatedTimestamp<DateTime?>
{
    public Guid Id { get; set; } // @MarkFix was File.Id

    public Guid DataSetFileId { get; set; }

    public DataSetFile DataSetFile { get; set; } = null!;

    public int Version { get; set; }

    public Guid? SubjectId { get; set; }

    public Guid DataFileId { get; set; }

    public File DataFile { get; set; } = null!;

    public Guid MetaFileId { get; set; }

    public File MetaFile { get; set; } = null!;

    public List<DataSetFileVersionGeographicLevel> DataSetFileVersionGeographicLevels { get; set; } = [];

    public DataSetFileMeta? DataSetFileMeta { get; set; }

    public List<DataSetFileFilterHierarchy>? FilterHierarchies { get; set; }

    public Guid? ReplacedById { get; set; }

    public File? ReplacedBy { get; set; }

    public Guid? ReplacingId { get; set; }

    public File? Replacing { get; set; }

    public Guid? SourceId { get; set; }

    public File? Source { get; set; }

    public DateTime? Created { get; set; }

    public User? CreatedBy { get; set; }

    public Guid? CreatedById { get; set; }
}
