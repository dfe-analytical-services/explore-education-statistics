using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleaseStatus : TableEntity
    {
        public DateTime Created { get; set; }
        public string PublicationSlug { get; set; }
        public DateTime? Publish { get; set; }
        public string ReleaseSlug { get; set; }
        public string ContentStage { get; set; }
        public string DataStage { get; set; }
        public string FilesStage { get; set; }
        public string PublishingStage { get; set; }
        public string Stage { get; set; }
        public string Messages { get; set; }

        public ReleaseStatus()
        {
        }

        public ReleaseStatus(string publicationSlug,
            DateTime? publish,
            Guid releaseId,
            string releaseSlug,
            (Stage Content, Stage Files, Stage Data, Stage Publishing, Stage Overall) stage,
            IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            Created = DateTime.UtcNow;
            PublicationSlug = publicationSlug;
            Publish = publish;
            ReleaseSlug = releaseSlug;
            ContentStage = stage.Content.ToString();
            DataStage = stage.Data.ToString();
            FilesStage = stage.Files.ToString();
            PublishingStage = stage.Publishing.ToString();
            Stage = stage.Overall.ToString();
            Messages = logMessages == null ? null : JsonConvert.SerializeObject(logMessages);

            RowKey = Guid.NewGuid().ToString();
            PartitionKey = releaseId.ToString();
        }

        public Guid Id => Guid.Parse(RowKey);
        public Guid ReleaseId => Guid.Parse(PartitionKey);

        public IEnumerable<ReleaseStatusLogMessage> LogMessages => Messages == null
            ? new List<ReleaseStatusLogMessage>()
            : JsonConvert.DeserializeObject<IEnumerable<ReleaseStatusLogMessage>>(Messages);

        public void AppendLogMessages(IEnumerable<ReleaseStatusLogMessage> logMessages)
        {
            if (logMessages != null)
            {
                Messages = JsonConvert.SerializeObject(LogMessages.Concat(logMessages));
            }
        }
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