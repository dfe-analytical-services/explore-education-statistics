#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Comment : ICreatedUpdatedTimestamps<DateTime, DateTime?>
    {
        public Guid Id { get; set; }
        public ContentBlock ContentBlock { get; set; } = null!;
        public Guid ContentBlockId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public User CreatedBy { get; set; } = null!;
        public Guid? CreatedById { get; set; }
        [Obsolete] public string? LegacyCreatedBy { get; set; }
        public DateTime? Updated { get; set; }
        public DateTime? Resolved { get; set; }
        public User ResolvedBy { get; set; } = null!;
        public Guid? ResolvedById { get; set; }
    }
}
