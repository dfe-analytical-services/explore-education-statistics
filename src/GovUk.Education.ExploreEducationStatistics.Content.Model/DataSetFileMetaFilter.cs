#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileMetaFilter
{
    public required Guid DataSetFileId;

    public required Guid FilterId;

    public required string Label { get; set; } = "";

    public string? Hint { get; set; } = "";

    public required string ColumnName { get; set; } = "";
}
