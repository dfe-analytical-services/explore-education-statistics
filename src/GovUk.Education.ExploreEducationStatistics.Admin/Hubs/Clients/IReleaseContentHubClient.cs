#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;

public interface IReleaseContentHubClient
{
    Task ContentBlockLocked(ReleaseContentBlockLockViewModel lockViewModel);

    Task ContentBlockUnlocked(ReleaseContentBlockUnlockViewModel unlockViewModel);

    Task ContentBlockUpdated(IContentBlockViewModel viewModel);
}