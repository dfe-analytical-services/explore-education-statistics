using System;
using System.Collections.Generic;
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
        public string FilesStage { get; set; }
        public string DataStage { get; set; }
        public string Stage { get; set; }
        public string Messages { get; set; }

        public ReleaseStatus()
        {
        }

        public ReleaseStatus(string publicationSlug,
            DateTime? publish,
            Guid releaseId,
            string releaseSlug,
            (Stage Content, Stage Files, Stage Data, Stage Overall) stage,
            IEnumerable<string> messages)
        {
            Created = DateTime.UtcNow;
            PublicationSlug = publicationSlug;
            Publish = publish;
            ReleaseSlug = releaseSlug;
            ContentStage = stage.Content.ToString();
            FilesStage = stage.Files.ToString();
            DataStage = stage.Data.ToString();
            Stage = stage.Overall.ToString();
            Messages = JsonConvert.SerializeObject(messages);

            RowKey = Guid.NewGuid().ToString();
            PartitionKey = releaseId.ToString();
        }
        
        public Guid Id => Guid.Parse(RowKey);
        public Guid ReleaseId => Guid.Parse(PartitionKey);
        public IEnumerable<string> MessageList => JsonConvert.DeserializeObject<IEnumerable<string>>(Messages);
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