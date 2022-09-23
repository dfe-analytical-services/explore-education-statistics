#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public class ContentSectionViewModel
{
    public Guid Id { get; set; }

    public int Order { get; set; }

    public string Heading { get; set; } = string.Empty;

    public string Caption { get; set; } = string.Empty;

    public List<IContentBlockViewModel> Content { get; set; } = new();
}
