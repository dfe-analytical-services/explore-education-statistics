using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.Stage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model
{
    public class ReleaseStatusState : INotifyPropertyChanged
    {
        private Stage _content;
        private Stage _files;
        private Stage _data;
        private Stage _publishing;
        public Stage Overall { get; private set; }

        public ReleaseStatusState(Stage content, Stage files, Stage data, Stage publishing, Stage overall)
        {
            _content = content;
            _files = files;
            _data = data;
            _publishing = publishing;
            Overall = overall;
        }

        public ReleaseStatusState(string content, string files, string data, string publishing, string overall) : this(
            Enum.Parse<Stage>(content),
            Enum.Parse<Stage>(files),
            Enum.Parse<Stage>(data),
            Enum.Parse<Stage>(publishing),
            Enum.Parse<Stage>(overall))
        {
        }

        public Stage Content
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

        public Stage Files
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

        public Stage Data
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

        public Stage Publishing
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
            if (Content == Complete && Data == Complete && Files == Complete && Publishing == Complete)
            {
                Overall = Complete;
            }

            if (Content == Failed || Data == Failed || Files == Failed || Publishing == Failed)
            {
                Overall = Failed;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
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