#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record PermalinkViewModel
{
    public Guid Id { get; init; }

    public DateTime Created { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public PermalinkStatus Status { get; init; }

    public dynamic Table { get; init; }
}
