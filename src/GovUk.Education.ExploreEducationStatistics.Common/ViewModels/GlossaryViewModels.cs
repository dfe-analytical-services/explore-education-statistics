#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

public class GlossaryEntryViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Slug { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;
}

public class GlossaryCategoryViewModel
{
    public string Heading { get; init; } = string.Empty;

    public List<GlossaryEntryViewModel> Entries { get; init; } = new();
}
