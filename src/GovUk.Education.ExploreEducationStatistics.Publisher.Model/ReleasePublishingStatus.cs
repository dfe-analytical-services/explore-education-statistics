using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Azure;
using Azure.Data.Tables;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleasePublishingStatus : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public DateTime Created { get; set; }
        public string PublicationSlug { get; set; }
        public DateTime? Publish { get; set; }
        public string ReleaseSlug { get; set; }
        public string ContentStage { get; set; } // @MarkFix change type of stages? I think so if it works
        public string FilesStage { get; set; }
        public string PublishingStage { get; set; }
        public ReleasePublishingStatusOverallStage OverallStage { get; set; }
        public bool Immediate { get; set; }
        public string Messages { get; set; }
        private ReleasePublishingStatusState _state;

        public ReleasePublishingStatus()
        {
        }

        public ReleasePublishingStatus(
            Guid releaseVersionId,
            Guid releaseStatusId,
            string publicationSlug,
            DateTime? publish,
            string releaseSlug,
            ReleasePublishingStatusState state,
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage> logMessages = null)
        {
            PartitionKey = releaseVersionId.ToString();
            RowKey = releaseStatusId.ToString();
            Created = DateTime.UtcNow;
            PublicationSlug = publicationSlug;
            Publish = publish;
            ReleaseSlug = releaseSlug;
            Immediate = immediate;
            Messages = logMessages == null ? null : JsonConvert.SerializeObject(logMessages);
            State = state;
        }

        public ReleasePublishingStatus(
            ReleasePublishingStatusOld oldStatus,
            IEnumerable<ReleasePublishingStatusLogMessage> logMessages = null)
        {
            PartitionKey = oldStatus.PartitionKey; // releaseVersionId
            RowKey = oldStatus.RowKey; // releaseStatusId
            Created = DateTime.UtcNow;
            PublicationSlug = oldStatus.PublicationSlug;
            Publish = oldStatus.Publish;
            ReleaseSlug = oldStatus.ReleaseSlug;
            Immediate = oldStatus.Immediate;
            Messages = logMessages == null ? null : JsonConvert.SerializeObject(logMessages);
            State = oldStatus.State;
        }

        public Guid Id => Guid.Parse(RowKey);
        public Guid ReleaseVersionId => Guid.Parse(PartitionKey);

        public ReleasePublishingStatusState State
        {
            get
            {
                if (_state == null)
                {
                    _state = new ReleasePublishingStatusState(ContentStage, FilesStage, PublishingStage, OverallStage.ToString());
                    _state.PropertyChanged += StateChangedCallback;
                }

                return _state;
            }
            set
            {
                ContentStage = value.Content.ToString();
                FilesStage = value.Files.ToString();
                PublishingStage = value.Publishing.ToString();
                OverallStage = value.Overall;

                _state = new ReleasePublishingStatusState(value.Content,
                    value.Files,
                    value.Publishing,
                    value.Overall);
                _state.PropertyChanged += StateChangedCallback;
            }
        }

        public IEnumerable<ReleasePublishingStatusLogMessage> LogMessages => Messages == null
            ? new List<ReleasePublishingStatusLogMessage>()
            : JsonConvert.DeserializeObject<IEnumerable<ReleasePublishingStatusLogMessage>>(Messages);

        public void AppendLogMessage(ReleasePublishingStatusLogMessage logMessage)
        {
            if (logMessage != null)
            {
                Messages = JsonConvert.SerializeObject(LogMessages.Append(logMessage));
            }
        }

        public void AppendLogMessages(IEnumerable<ReleasePublishingStatusLogMessage> logMessages)
        {
            if (logMessages != null)
            {
                Messages = JsonConvert.SerializeObject(LogMessages.Concat(logMessages));
            }
        }

        private void StateChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ReleasePublishingStatusState.Content):
                    ContentStage = _state.Content.ToString();
                    break;
                case nameof(ReleasePublishingStatusState.Files):
                    FilesStage = _state.Files.ToString();
                    break;
                case nameof(ReleasePublishingStatusState.Publishing):
                    PublishingStage = _state.Publishing.ToString();
                    break;
            }

            OverallStage = _state.Overall;
        }

        public bool AllStagesPriorToPublishingComplete()
        {
            return State.Content == ReleasePublishingStatusContentStage.Complete &&
                   State.Files == ReleasePublishingStatusFilesStage.Complete;
        }

        public ReleasePublishingKey AsTableRowKey()
        {
            return new ReleasePublishingKey(ReleaseVersionId, Id);
        }
    }

    public record ReleasePublishingKey(Guid ReleaseVersionId, Guid ReleaseStatusId);
}
