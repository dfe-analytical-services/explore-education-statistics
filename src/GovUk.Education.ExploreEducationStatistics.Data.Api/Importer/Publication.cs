using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class Publication
    {
        public Guid PublicationId { get; set; }
        public string Name { get; set; }
        public Release[] Releases { get; set; }
    }
}