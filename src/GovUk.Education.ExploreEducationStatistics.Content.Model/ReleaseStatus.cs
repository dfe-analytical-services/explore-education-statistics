#nullable enable

using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseStatus
    {
        public Guid Id { get; set; }

        public Guid ReleaseId { get; set; }

        public Release Release { get; set; } = null!;

        public string InternalReleaseNote { get; set; } = null!;

        public bool NotifySubscribers { get; set; }

        public DateTime? NotifiedOn { get; set; }

        public ReleaseApprovalStatus ApprovalStatus { get; set; }

        public DateTime? Created { get; set; }

        public Guid? CreatedById { get; set; }

        public User? CreatedBy { get; set; }
    }
}
