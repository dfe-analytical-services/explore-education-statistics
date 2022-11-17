#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using JsonKnownTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

[JsonKnownThisType(nameof(EmbedBlockLink))]
public class EmbedBlockLinkViewModel : IContentBlockViewModel
{
    public Guid Id { get; init; }

    public int Order { get; init; }

    public string Title { get; set; }

    public string Url { get; set; }

    public List<CommentViewModel> Comments { get; init; } = new();

    public DateTimeOffset? Locked { get; init; }

    public DateTimeOffset? LockedUntil { get; init; }

    public UserDetailsViewModel? LockedBy { get; init; }
}
