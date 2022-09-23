#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public class LinkViewModel
{
    public Guid Id { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}
