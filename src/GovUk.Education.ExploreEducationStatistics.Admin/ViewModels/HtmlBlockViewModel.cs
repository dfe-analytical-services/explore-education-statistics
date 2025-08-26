﻿#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

[JsonKnownThisType(nameof(HtmlBlock))]
public record HtmlBlockViewModel : IContentBlockViewModel
{
    public Guid Id { get; init; }

    public List<CommentViewModel> Comments { get; init; } = new();

    public int Order { get; init; }

    public string Body { get; set; } = string.Empty;

    public DateTimeOffset? Locked { get; init; }

    public DateTimeOffset? LockedUntil { get; init; }

    public UserDetailsViewModel? LockedBy { get; init; }
}