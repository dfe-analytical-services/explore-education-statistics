namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Models
{
    using System;

    public class Publication
    {
        public Guid PublicationId { get; set; }

        public string Name { get; set; }

        public Release[] Releases { get; set; }
    }
}