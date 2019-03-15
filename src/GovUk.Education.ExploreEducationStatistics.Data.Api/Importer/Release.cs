using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Importer
{
    public class Release
    {
        public Guid PublicationId { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Name { get; set; }
        public DataSet[] DataSets { get; set; }
    }
}