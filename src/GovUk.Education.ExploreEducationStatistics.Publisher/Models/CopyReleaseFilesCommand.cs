using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Models
{
    public class CopyReleaseFilesCommand
    {
        public Guid ReleaseId { get; set; }
        public string PublicationSlug { get; set; }
        public string ReleaseSlug { get; set; }
        public DateTime PublishScheduled { get; set; }
        public List<ReleaseFileReference> Files { get; set; }
    }
}