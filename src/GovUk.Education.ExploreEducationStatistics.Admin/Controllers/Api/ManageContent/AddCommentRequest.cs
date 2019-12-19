using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.ManageContent
{
    public class AddCommentRequest
    {
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public string CommentText { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedOn { get; set; }
        public string State { get; set; }
    }
}