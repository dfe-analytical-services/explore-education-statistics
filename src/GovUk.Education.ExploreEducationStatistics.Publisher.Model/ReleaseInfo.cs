using System;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleaseInfo : TableEntity
    {
        public DateTime Created { get; set; }
        public string PublicationSlug { get; set; }
        public DateTime PublishScheduled { get; set; }
        public Guid ReleaseId { get; set; }
        public string ReleaseSlug { get; set; }
        public int ReleaseContentStage { get; set; }
        public int ReleaseFilesStage { get; set; }
        public int ReleaseDataStage { get; set; }
        public string Status { get; set; }

        public ReleaseInfo()
        {
        }

        public ReleaseInfo(string publicationSlug,
            DateTime publishScheduled,
            Guid releaseId,
            string releaseSlug,
            ReleaseInfoStatus status)
        {
            Created = DateTime.UtcNow;
            PublicationSlug = publicationSlug;
            PublishScheduled = publishScheduled;
            ReleaseId = releaseId;
            ReleaseSlug = releaseSlug;
            Status = status.ToString();

            RowKey = Guid.NewGuid().ToString();
            PartitionKey = releaseId.ToString();
        }
    }
}