using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class MethodologyFile
    {
        public Guid Id { get; set; }

        public Methodology Methodology { get; set; }

        public Guid MethodologyId { get; set; }

        public File File { get; set; }

        public Guid FileId { get; set; }
    }
}
