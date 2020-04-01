using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleaseStatusState : INotifyPropertyChanged
    {
        private ReleaseStatusContentStage _content;
        private ReleaseStatusFilesStage _files;
        private ReleaseStatusDataStage _data;
        private ReleaseStatusPublishingStage _publishing;
        public ReleaseStatusOverallStage ReleaseStatusOverall { get; private set; }

        public ReleaseStatusState(ReleaseStatusContentStage content,
            ReleaseStatusFilesStage files,
            ReleaseStatusDataStage data,
            ReleaseStatusPublishingStage publishing,
            ReleaseStatusOverallStage releaseStatusOverall)
        {
            _content = content;
            _files = files;
            _data = data;
            _publishing = publishing;
            ReleaseStatusOverall = releaseStatusOverall;
        }

        public ReleaseStatusState(string content, string files, string data, string publishing, string overall) : this(
            Enum.Parse<ReleaseStatusContentStage>(content),
            Enum.Parse<ReleaseStatusFilesStage>(files),
            Enum.Parse<ReleaseStatusDataStage>(data),
            Enum.Parse<ReleaseStatusPublishingStage>(publishing),
            Enum.Parse<ReleaseStatusOverallStage>(overall))
        {
        }

        public ReleaseStatusContentStage Content
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

        public ReleaseStatusFilesStage Files
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

        public ReleaseStatusDataStage Data
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

        public ReleaseStatusPublishingStage Publishing
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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (Content == ReleaseStatusContentStage.Complete
                && Data == ReleaseStatusDataStage.Complete
                && Files == ReleaseStatusFilesStage.Complete
                && Publishing == ReleaseStatusPublishingStage.Complete)
            {
                ReleaseStatusOverall = ReleaseStatusOverallStage.Complete;
            }

            if (Content == ReleaseStatusContentStage.Failed
                || Data == ReleaseStatusDataStage.Failed 
                || Files == ReleaseStatusFilesStage.Failed
                || Publishing == ReleaseStatusPublishingStage.Failed)
            {
                if (Content == ReleaseStatusContentStage.Failed
                    || Data == ReleaseStatusDataStage.Failed 
                    || Files == ReleaseStatusFilesStage.Failed)
                {
                    Publishing = ReleaseStatusPublishingStage.Cancelled;
                }
                ReleaseStatusOverall = ReleaseStatusOverallStage.Failed;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public enum ReleaseStatusContentStage
    {
        Cancelled,
        Complete,
        Failed,
        Queued,
        NotStarted,
        Started
    }
    
    public enum ReleaseStatusDataStage
    {
        Cancelled,
        Complete,
        Failed,
        Queued,
        NotStarted,
        Started
    }
    
    public enum ReleaseStatusFilesStage
    {
        Cancelled,
        Complete,
        Failed,
        Queued,
        NotStarted,
        Started
    }
    
    public enum ReleaseStatusOverallStage
    {
        Complete,
        Failed,
        Invalid,
        Scheduled,
        Started
    }
    
    public enum ReleaseStatusPublishingStage
    {
        Cancelled,
        Complete,
        Failed,
        Scheduled,
        NotStarted,
        Started
    }
}