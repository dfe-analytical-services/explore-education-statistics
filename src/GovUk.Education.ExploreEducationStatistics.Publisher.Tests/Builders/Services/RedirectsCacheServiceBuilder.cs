using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class RedirectsCacheServiceBuilder
{
    private readonly Mock<IRedirectsCacheService> _mock = new(MockBehavior.Strict);
    public IRedirectsCacheService Build() => _mock.Object;
    
    public Asserter Assert => new(_mock);

    public RedirectsCacheServiceBuilder()
    {
        _mock
            .Setup(m => m.UpdateRedirects())
            .ReturnsAsync(new Either<ActionResult, RedirectsViewModel>(new NoOpResult()));
    }

    public class Asserter(Mock<IRedirectsCacheService> mock)
    {
        public void UpdateRedirectsCalled()
        {
            mock.Verify(m => m.UpdateRedirects(), Times.Once);
        }
    }
}
