#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataSetFileMetaIndicator
{
    public Guid DataSetFileId;

    public Guid IndicatorId;

    public string Label { get; set; } = "";

    public string ColumnName { get; set; } = "";
}
