using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleasePublishingStatus : TableEntity
    {
        public DateTime Created { get; set; }
        public string PublicationSlug { get; set; }
        public DateTime? Publish { get; set; }
        public string ReleaseSlug { get; set; }
        public string ContentStage { get; set; }
        public string FilesStage { get; set; }
        public string PublishingStage { get; set; }
        public string OverallStage { get; set; }
        public bool Immediate { get; set; }
        public string Messages { get; set; }
        private ReleasePublishingStatusState _state;

        public ReleasePublishingStatus()
        {
        }

        public ReleasePublishingStatus(string publicationSlug,
            DateTime? publish,
            Guid releaseId,
            Guid releaseStatusId,
            string releaseSlug,
            ReleasePublishingStatusState state,
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage> logMessages = null)
        {
            RowKey = releaseStatusId.ToString();
            PartitionKey = releaseId.ToString();
            Created = DateTime.UtcNow;
            PublicationSlug = publicationSlug;
            Publish = publish;
            ReleaseSlug = releaseSlug;
            Immediate = immediate;
            Messages = logMessages == null ? null : JsonConvert.SerializeObject(logMessages);
            State = state;
        }

        public Guid Id => Guid.Parse(RowKey);
        public Guid ReleaseId => Guid.Parse(PartitionKey);

        public ReleasePublishingStatusState State
        {
            get
            {
                if (_state == null)
                {
                    _state = new ReleasePublishingStatusState(ContentStage, FilesStage, PublishingStage, OverallStage);
                    _state.PropertyChanged += StateChangedCallback;
                }

                return _state;
            }
            set
            {
                ContentStage = value.Content.ToString();
                FilesStage = value.Files.ToString();
                PublishingStage = value.Publishing.ToString();
                OverallStage = value.Overall.ToString();

                _state = new ReleasePublishingStatusState(value.Content, value.Files, value.Publishing,
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

            OverallStage = _state.Overall.ToString();
        }

        public bool AllStagesPriorToPublishingComplete()
        {
            return State.Content == ReleasePublishingStatusContentStage.Complete &&
                   State.Files == ReleasePublishingStatusFilesStage.Complete;
        }
    }
}
