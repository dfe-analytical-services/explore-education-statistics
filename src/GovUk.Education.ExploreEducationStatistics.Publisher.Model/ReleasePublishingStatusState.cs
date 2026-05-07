using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Model;

public class ReleasePublishingStatusState(
    ReleasePublishingStatusFilesStage files,
    ReleasePublishingStatusPublishingStage publishing,
    ReleasePublishingStatusOverallStage overall
) : INotifyPropertyChanged
{
    private ReleasePublishingStatusFilesStage _files = files;
    private ReleasePublishingStatusPublishingStage _publishing = publishing;
    private ReleasePublishingStatusOverallStage _overall = overall;

    public ReleasePublishingStatusState(string files, string publishing, string overall)
        : this(
            Enum.Parse<ReleasePublishingStatusFilesStage>(files),
            Enum.Parse<ReleasePublishingStatusPublishingStage>(publishing),
            Enum.Parse<ReleasePublishingStatusOverallStage>(overall)
        ) { }

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
        if (
            Files == ReleasePublishingStatusFilesStage.Complete
            && Publishing == ReleasePublishingStatusPublishingStage.Complete
        )
        {
            Overall = ReleasePublishingStatusOverallStage.Complete;
        }

        if (
            Files == ReleasePublishingStatusFilesStage.Failed
            || Publishing == ReleasePublishingStatusPublishingStage.Failed
        )
        {
            if (Files == ReleasePublishingStatusFilesStage.Failed)
            {
                Publishing = ReleasePublishingStatusPublishingStage.Cancelled;
            }
            Overall = ReleasePublishingStatusOverallStage.Failed;
        }

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler PropertyChanged;
}

public enum ReleasePublishingStatusFilesStage
{
    Cancelled,
    Complete,
    Failed,
    Queued,
    NotStarted,
    Started,
}

public enum ReleasePublishingStatusOverallStage
{
    Complete,
    Failed,
    Invalid,
    Scheduled,
    Started,
    Superseded,
}

public enum ReleasePublishingStatusPublishingStage
{
    Cancelled,
    Complete,
    Failed,
    Scheduled,
    NotStarted,
    Started,
}
