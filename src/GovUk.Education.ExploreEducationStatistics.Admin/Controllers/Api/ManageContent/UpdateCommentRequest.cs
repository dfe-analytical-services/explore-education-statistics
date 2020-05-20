using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    public class UpdateCommentRequest
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public string CommentText { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public CommentState State { get; set; }
    }
}