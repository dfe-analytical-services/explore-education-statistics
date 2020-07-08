using System;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class ReleaseFastTrack : TableEntity
    {
        public Guid FastTrackId => Guid.Parse(RowKey);
        public Guid ReleaseId => Guid.Parse(PartitionKey);

        public ReleaseFastTrack()
        {
        }

        public ReleaseFastTrack(Guid releaseId, Guid fastTrackId)
        {
            PartitionKey = releaseId.ToString();
            RowKey = fastTrackId.ToString();
        }
    }
}