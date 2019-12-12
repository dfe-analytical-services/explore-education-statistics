using System;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleaseStatus : TableEntity
    {
        public DateTime Created { get; set; }
        public string PublicationSlug { get; set; }
        public DateTime Publish { get; set; }
        public Guid ReleaseId { get; set; }
        public string ReleaseSlug { get; set; }
        public string? ContentStage { get; set; }
        public string? FilesStage { get; set; }
        public string? DataStage { get; set; }
        public string Stage { get; set; }

        public ReleaseStatus()
        {
        }

        public ReleaseStatus(string publicationSlug,
            DateTime publish,
            Guid releaseId,
            string releaseSlug,
            Stage stage)
        {
            Created = DateTime.UtcNow;
            PublicationSlug = publicationSlug;
            Publish = publish;
            ReleaseId = releaseId;
            ReleaseSlug = releaseSlug;
            Stage = stage.ToString();

            RowKey = Guid.NewGuid().ToString();
            PartitionKey = releaseId.ToString();
        }

        public Guid Id => Guid.Parse(RowKey);
    }

    public enum Stage
    {
        Invalid,
        Started,
        Scheduled,
        Complete,
        Failed
    }
}