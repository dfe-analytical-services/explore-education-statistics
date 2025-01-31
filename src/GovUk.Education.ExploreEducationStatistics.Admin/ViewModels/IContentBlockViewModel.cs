#nullable enable
using System;
using System.Collections.Generic;
using JsonKnownTypes;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

[JsonConverter(typeof(JsonKnownTypesConverter<IContentBlockViewModel>))]
[JsonDiscriminator(Name = "type")]
public interface IContentBlockViewModel
{
    Guid Id { get; init; }

    List<CommentViewModel> Comments { get; init; }

    int Order { get; init; }

    DateTimeOffset? Locked { get; init; }

    DateTimeOffset? LockedUntil { get; init; }

    UserDetailsViewModel? LockedBy { get; init; }
}