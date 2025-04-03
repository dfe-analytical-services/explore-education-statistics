#nullable enable
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class AdminEventRaiserServiceMockBuilder
{
    private readonly Mock<IAdminEventRaiserService> _mock = new(MockBehavior.Strict);
    
    private static readonly Expression<Func<IAdminEventRaiserService, Task>> OnReleaseSlugChanged =  
        m => m.OnReleaseSlugChanged(
            It.IsAny<Guid>(), 
            It.IsAny<string>(), 
            It.IsAny<Guid>(), 
            It.IsAny<string>());

    public IAdminEventRaiserService Build() => _mock.Object;

    public AdminEventRaiserServiceMockBuilder()
    {
        _mock
            .Setup(m => m.OnThemeUpdated(It.IsAny<Theme>()))
            .Returns(Task.CompletedTask);
        
        _mock
            .Setup(OnReleaseSlugChanged)
            .Returns(Task.CompletedTask);
    }

    public class Asserter(Mock<IAdminEventRaiserService> mock)
    {
        public void ThatOnThemeUpdatedRaised(Func<Theme, bool>? predicate = null) => 
            mock.Verify(m => m.OnThemeUpdated(It.Is<Theme>(t => predicate == null || predicate(t))), Times.Once);

        public void OnReleaseSlugChangedWasRaised(
            Guid? expectedReleaseId = null,
            string? expectedNewReleaseSlug = null,
            Guid? expectedPublicationId = null,
            string? expectedPublicationSlug = null) => 
            mock.Verify(m => m.OnReleaseSlugChanged(
                It.Is<Guid>(releaseId => expectedReleaseId == null || releaseId == expectedReleaseId), 
                It.Is<string>(newReleaseSlug => expectedNewReleaseSlug == null || newReleaseSlug == expectedNewReleaseSlug), 
                It.Is<Guid>(publicationId => expectedPublicationId == null || publicationId == expectedPublicationId), 
                It.Is<string>(publicationSlug => expectedPublicationSlug == null || publicationSlug == expectedPublicationSlug)),
                Times.Once);

        public void OnReleaseSlugChangedWasNotRaised() => 
            mock.Verify(OnReleaseSlugChanged, Times.Never);
    }
    public Asserter Assert => new(_mock);
}
