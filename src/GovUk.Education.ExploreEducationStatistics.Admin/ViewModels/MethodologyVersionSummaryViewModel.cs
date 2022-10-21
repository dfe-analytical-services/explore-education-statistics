#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record MethodologyVersionSummaryViewModel
{
    public Guid Id { get; set; }

    public bool Amendment { get; set; }

    public DateTime? Published { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public MethodologyStatus Status { get; set; }

    public bool Owned { get; set; }

    public MethodologyVersionPermissions Permissions { get; set; } = new();

    public string Title { get; set; } = string.Empty;

    public Guid MethodologyId { get; set; }

    public Guid? PreviousVersionId { get; set; }

    public record MethodologyVersionPermissions
    {
        public bool CanDeleteMethodology { get; set; }
        public bool CanUpdateMethodology { get; set; }
        public bool CanApproveMethodology { get; set; }
        public bool CanMarkMethodologyAsDraft { get; set; }
        public bool CanMakeAmendmentOfMethodology { get; set; }
        public bool CanRemoveMethodologyLink { get; set; }
    }
}
