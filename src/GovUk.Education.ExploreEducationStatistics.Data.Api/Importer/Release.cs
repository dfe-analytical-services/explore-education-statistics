using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class Release
    {
        public Guid PublicationId { get; set; }
        public int ReleaseId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public String Name { get; set; }
        public DataCsvFilename[] Filenames { get; set; }
    }
}