#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public record ReleaseStatusViewModel
    {
        public Guid ReleaseStatusId { get; set; }

        public string? InternalReleaseNote { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public DateTime? Created { get; set; }

        public string? CreatedByEmail { get; set; }

        public int ReleaseVersion { get; set; }
    }
}
