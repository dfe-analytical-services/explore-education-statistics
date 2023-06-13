#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record FeaturedTableViewModel
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string? Description { get; set; }

    public int Order { get; set; }

    public Guid DataBlockId { get; set; }

    public DataBlock DataBlock { get; set; } = null!;
}

public record FeaturedTableBasicViewModel
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }
}
