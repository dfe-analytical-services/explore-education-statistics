using System;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleaseStatus : TableEntity
    {
        public DateTime Created { get; set; }
        public string PublicationSlug { get; set; }
        public DateTime Publish { get; set; }
        public string ReleaseSlug { get; set; }
        public string ContentStage { get; set; }
        public string FilesStage { get; set; }
        public string DataStage { get; set; }
        public string Stage { get; set; }

        public ReleaseStatus()
        {
        }

        public ReleaseStatus(string publicationSlug,
            DateTime publish,
            Guid releaseId,
            string releaseSlug,
            Stage contentStage,
            Stage filesStage,
            Stage dataStage,
            Stage stage)
        {
            Created = DateTime.UtcNow;
            PublicationSlug = publicationSlug;
            Publish = publish;
            ReleaseSlug = releaseSlug;
            ContentStage = contentStage.ToString();
            FilesStage = filesStage.ToString();
            DataStage = dataStage.ToString();
            Stage = stage.ToString();

            RowKey = Guid.NewGuid().ToString();
            PartitionKey = releaseId.ToString();
        }

        public Guid Id => Guid.Parse(RowKey);
        public Guid ReleaseId => Guid.Parse(PartitionKey);
    }

    public enum Stage
    {
        Cancelled,
        Complete,
        Failed,
        Invalid,
        Queued,
        Scheduled,
        Started
    }
}