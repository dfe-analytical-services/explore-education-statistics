#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public class DataBlockSummaryViewModel
{
    public List<Guid> DataKeys { get; set; } = new();

    public List<string> DataSummary { get; set; } = new();

    public List<string> DataDefinition { get; set; } = new();

    public List<string> DataDefinitionTitle { get; set; } = new();
}
