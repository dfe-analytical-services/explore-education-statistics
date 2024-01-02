#nullable enable
using System;
using System.IO;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

public record DataSetSearchResultViewModel
{
    public Guid FileId { get; init; }

    public string Filename { get; init; } = string.Empty;

    public string FileSize { get; init; } = string.Empty;

    public string FileExtension => Path.GetExtension(Filename).TrimStart('.');

    public string Title { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public IdTitleViewModel Theme { get; init; } = null!;

    public IdTitleViewModel Publication { get; init; } = null!;

    public IdTitleViewModel Release { get; init; } = null!;

    public bool LatestData { get; init; }

    public DateTime Published { get; init; }
}
