#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class AdminEventRaiserServiceMockBuilder
{
    private readonly Mock<IAdminEventRaiserService> _mock = new(MockBehavior.Strict);
    public IAdminEventRaiserService Build() => _mock.Object;

    public AdminEventRaiserServiceMockBuilder()
    {
        _mock
            .Setup(m => m.OnThemeUpdated(It.IsAny<Theme>()))
            .Returns(Task.CompletedTask);
    }

    public class Asserter(Mock<IAdminEventRaiserService> mock)
    {
        public void ThatOnThemeUpdatedCalled(Func<Theme, bool>? predicate = null) => 
            mock.Verify(m => m.OnThemeUpdated(It.Is<Theme>(t => predicate == null || predicate(t))), Times.Once);
    }
    public Asserter Assert => new(_mock);
}
