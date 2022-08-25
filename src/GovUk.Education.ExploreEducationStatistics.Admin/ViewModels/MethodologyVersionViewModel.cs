#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public record MethodologyVersionViewModel
{
    public Guid Id { get; set; }

    public bool Amendment { get; set; }

    // TODO EES-3596 Remove InternalReleaseNote added to support the old Admin dashboard
    public string? InternalReleaseNote { get; set; }

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
        public bool CanDeleteMethodologyVersion { get; set; }
        public bool CanUpdateMethodologyVersion { get; set; }
        public bool CanApproveMethodologyVersion { get; set; }
        public bool CanMarkMethodologyVersionAsDraft { get; set; }
        public bool CanMakeAmendmentOfMethodology { get; set; }
        public bool CanRemoveMethodologyLink { get; set; }
    }
}
