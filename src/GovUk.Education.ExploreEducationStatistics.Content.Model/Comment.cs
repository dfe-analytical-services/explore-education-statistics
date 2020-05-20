using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid? IContentBlockId { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public string CommentText { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public CommentState State { get; set; }
    }
}