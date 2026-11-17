#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class MethodologyCacheServiceMockBuilder
{
    private readonly Mock<IMethodologyCacheService> _mock = new(MockBehavior.Strict);

    public MethodologyCacheServiceMockBuilder()
    {
        _mock
            .Setup(m => m.UpdateSummariesTree())
            .ReturnsAsync(new Either<ActionResult, List<AllMethodologiesThemeViewModel>>([]));
    }

    public IMethodologyCacheService Build() => _mock.Object;

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IMethodologyCacheService> mock)
    {
        public void SummariesTreeUpdated() => mock.Verify(m => m.UpdateSummariesTree(), Times.Once);

        public void SummariesTreeNotUpdated() => mock.Verify(m => m.UpdateSummariesTree(), Times.Never);
    }
}
