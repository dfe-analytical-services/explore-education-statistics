using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class RedirectsCacheServiceMockBuilder
{
    private readonly Mock<IRedirectsCacheService> _mock = new(MockBehavior.Strict);

    public IRedirectsCacheService Build()
    {
        _mock.Setup(m => m.UpdateRedirects()).ReturnsAsync(() => new RedirectsViewModel([], [], []));

        return _mock.Object;
    }
}
