#nullable enable

using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseStatus : ICreatedTimestamp<DateTime?>
    {
        public Guid Id { get; set; }

        public Guid ReleaseId { get; set; }

        public Release Release { get; set; } = null!;

        public string? InternalReleaseNote { get; set; }

        // TODO EES-4058 Remove unused NotifySubscribers after EES-4056
        public bool NotifySubscribers { get; set; }

        // TODO EES-4058 Remove unused NotifiedOn after EES-4056
        public DateTime? NotifiedOn { get; set; }

        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public DateTime? Created { get; set; }

        public Guid? CreatedById { get; set; }

        public User? CreatedBy { get; set; }
    }
}
