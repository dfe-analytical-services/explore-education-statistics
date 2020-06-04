using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Models.Api
{
    public class CommentViewModel
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        // TODO EES-18 retrieve User name when mapping or LegacyCreatedBy if CreatedById is null
        public string CreatedBy { get; set; }
        public Guid CreatedById { get; set; }
        public DateTime? Updated { get; set; }
    }
}