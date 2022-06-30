using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleasePublishingStatusState : INotifyPropertyChanged
    {
        private ReleasePublishingStatusContentStage _content;
        private ReleasePublishingStatusFilesStage _files;
        private ReleasePublishingStatusDataStage _data;
        private ReleasePublishingStatusPublishingStage _publishing;
        private ReleasePublishingStatusOverallStage _overall;

        public ReleasePublishingStatusState(
            ReleasePublishingStatusContentStage content,
            ReleasePublishingStatusFilesStage files,
            ReleasePublishingStatusDataStage data,
            ReleasePublishingStatusPublishingStage publishing,
            ReleasePublishingStatusOverallStage overall)
        {
            _content = content;
            _files = files;
            _data = data;
            _publishing = publishing;
            _overall = overall;
        }

        public ReleasePublishingStatusState(string content, string files, string data, string publishing, string overall) : this(
            Enum.Parse<ReleasePublishingStatusContentStage>(content),
            Enum.Parse<ReleasePublishingStatusFilesStage>(files),
            Enum.Parse<ReleasePublishingStatusDataStage>(data),
            Enum.Parse<ReleasePublishingStatusPublishingStage>(publishing),
            Enum.Parse<ReleasePublishingStatusOverallStage>(overall))
        {
        }

        public ReleasePublishingStatusContentStage Content
        {
            get => _content;
            set
            {
                if (value == _content)
                {
                    return;
                }

                _content = value;
                NotifyPropertyChanged();
            }
        }

        public ReleasePublishingStatusFilesStage Files
        {
            get => _files;
            set
            {
                if (value == _files)
                {
                    return;
                }

                _files = value;
                NotifyPropertyChanged();
            }
        }

        public ReleasePublishingStatusDataStage Data
        {
            get => _data;
            set
            {
                if (value == _data)
                {
                    return;
                }

                _data = value;
                NotifyPropertyChanged();
            }
        }

        public ReleasePublishingStatusPublishingStage Publishing
        {
            get => _publishing;
            set
            {
                if (value == _publishing)
                {
                    return;
                }

                _publishing = value;
                NotifyPropertyChanged();
            }
        }

        public ReleasePublishingStatusOverallStage Overall
        {
            get => _overall;
            set
            {
                if (value == _overall)
                {
                    return;
                }

                _overall = value;
                NotifyPropertyChanged();
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (Content == ReleasePublishingStatusContentStage.Complete
                && Data == ReleasePublishingStatusDataStage.Complete
                && Files == ReleasePublishingStatusFilesStage.Complete
                && Publishing == ReleasePublishingStatusPublishingStage.Complete)
            {
                Overall = ReleasePublishingStatusOverallStage.Complete;
            }

            if (Content == ReleasePublishingStatusContentStage.Failed
                || Data == ReleasePublishingStatusDataStage.Failed
                || Files == ReleasePublishingStatusFilesStage.Failed
                || Publishing == ReleasePublishingStatusPublishingStage.Failed)
            {
                if (Content == ReleasePublishingStatusContentStage.Failed
                    || Data == ReleasePublishingStatusDataStage.Failed
                    || Files == ReleasePublishingStatusFilesStage.Failed)
                {
                    Publishing = ReleasePublishingStatusPublishingStage.Cancelled;
                }
                Overall = ReleasePublishingStatusOverallStage.Failed;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public enum ReleasePublishingStatusContentStage
    {
        Cancelled,
        Complete,
        Failed,
        Queued,
        NotStarted,
        Started
    }

    public enum ReleasePublishingStatusDataStage
    {
        Cancelled,
        Complete,
        Failed,
        NotStarted,
    }

    public enum ReleasePublishingStatusFilesStage
    {
        Cancelled,
        Complete,
        Failed,
        Queued,
        NotStarted,
        Started
    }

    public enum ReleasePublishingStatusOverallStage
    {
        Complete,
        Failed,
        Invalid,
        Scheduled,
        Started,
        Superseded
    }

    public enum ReleasePublishingStatusPublishingStage
    {
        Cancelled,
        Complete,
        Failed,
        Scheduled,
        NotStarted,
        Started
    }
}
