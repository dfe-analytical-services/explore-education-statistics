using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public string OverallStage { get; set; }
        public string Messages { get; set; }
        private ReleaseStatusState _state;

        public ReleaseStatus()
        {
        }

        public ReleaseStatus(string publicationSlug,
            DateTime? publish,
            Guid releaseId,
            string releaseSlug,
            ReleaseStatusState state,
            IEnumerable<ReleaseStatusLogMessage> logMessages = null)
        {
            Created = DateTime.UtcNow;
            PublicationSlug = publicationSlug;
            Publish = publish;
            ReleaseSlug = releaseSlug;
            Messages = logMessages == null ? null : JsonConvert.SerializeObject(logMessages);
            State = state;
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = releaseId.ToString();
        }

        public Guid Id => Guid.Parse(RowKey);
        public Guid ReleaseId => Guid.Parse(PartitionKey);

        public ReleaseStatusState State
        {
            get
            {
                if (_state == null)
                {
                    _state = new ReleaseStatusState(ContentStage, FilesStage, DataStage, PublishingStage, OverallStage);
                    _state.PropertyChanged += StateChangedCallback;
                }

                return _state;
            }
            set
            {
                ContentStage = value.Content.ToString();
                DataStage = value.Data.ToString();
                FilesStage = value.Files.ToString();
                PublishingStage = value.Publishing.ToString();
                OverallStage = value.Overall.ToString();

                _state = new ReleaseStatusState(value.Content, value.Files, value.Data, value.Publishing,
                    value.Overall);
                _state.PropertyChanged += StateChangedCallback;
            }
        }

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

        private void StateChangedCallback(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ReleaseStatusState.Content):
                    ContentStage = _state.Content.ToString();
                    break;
                case nameof(ReleaseStatusState.Data):
                    DataStage = _state.Data.ToString();
                    break;
                case nameof(ReleaseStatusState.Files):
                    FilesStage = _state.Files.ToString();
                    break;
                case nameof(ReleaseStatusState.Publishing):
                    PublishingStage = _state.Publishing.ToString();
                    break;
            }
            
            OverallStage = _state.Overall.ToString();
        }
    }
}