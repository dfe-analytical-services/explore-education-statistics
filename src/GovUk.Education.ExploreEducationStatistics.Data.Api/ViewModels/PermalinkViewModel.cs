#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;

public record PermalinkViewModel
{
    public Guid Id { get; set; }

    public DateTime Created { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public PermalinkStatus Status { get; set; }

    public dynamic Table { get; set; }
}
