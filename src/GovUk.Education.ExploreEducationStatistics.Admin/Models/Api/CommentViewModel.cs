using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public Guid? IContentBlockId { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public string CommentText { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string State { get; set; }
    }
}