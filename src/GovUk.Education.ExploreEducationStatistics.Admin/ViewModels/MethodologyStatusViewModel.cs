#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record MethodologyStatusViewModel
{
    public Guid MethodologyStatusId { get; set; }

    public string? InternalReleaseNote { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public MethodologyApprovalStatus ApprovalStatus { get; set; }

    public DateTime? Created { get; set; }

    public string? CreatedByEmail { get; set; }

    public int MethodologyVersion { get; set; }
}
