using System;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseFastTrack : TableEntity
    {
        public Guid FastTrackId => Guid.Parse(RowKey);
        public Guid ReleaseId => Guid.Parse(PartitionKey);
        public string HighlightName { get; set; }

        public ReleaseFastTrack()
        {
        }

        public ReleaseFastTrack(Guid releaseId, Guid fastTrackId, string highlightName)
        {
            PartitionKey = releaseId.ToString();
            RowKey = fastTrackId.ToString();
            HighlightName = highlightName;
        }
    }
}