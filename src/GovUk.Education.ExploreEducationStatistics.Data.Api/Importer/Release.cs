using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class Release
    {
        public Guid PublicationId { get; set; }
        public Guid ReleaseId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public DataCsvFilename[] Filenames { get; set; }
    }
}