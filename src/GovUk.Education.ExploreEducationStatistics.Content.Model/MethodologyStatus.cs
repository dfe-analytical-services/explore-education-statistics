#nullable enable

using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyStatus : ICreatedTimestamp<DateTime?>
    {
        public Guid Id { get; set; }

        public Guid MethodologyVersionId { get; set; }

        public MethodologyVersion MethodologyVersion { get; set; } = null!;

        public string? InternalReleaseNote { get; set; }

        public MethodologyApprovalStatus ApprovalStatus { get; set; }

        public DateTime? Created { get; set; }

        public Guid? CreatedById { get; set; }

        public User? CreatedBy { get; set; }
    }
}
