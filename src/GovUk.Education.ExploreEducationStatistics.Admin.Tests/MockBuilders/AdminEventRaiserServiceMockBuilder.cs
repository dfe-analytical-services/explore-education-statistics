#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Events;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class AdminEventRaiserServiceMockBuilder
{
    private readonly Mock<IAdminEventRaiserService> _mock = new(MockBehavior.Strict);
    private readonly List<OnPublicationLatestPublishedReleaseVersionChangedArgs> _onPublicationLatestPublishedReleaseVersionChangedInvocations = new();
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
        
        _mock
            .Setup(m => m.OnPublicationChanged(It.IsAny<Publication>()))
            .Returns(Task.CompletedTask);
        
        _mock
            .Setup(m => m.OnPublicationLatestPublishedReleaseVersionChanged(
                It.IsAny<Publication>(),
                It.IsAny<Guid?>()))
            .Callback((Publication publication, Guid? oldLatestPublishedReleaseVersionId) => 
                _onPublicationLatestPublishedReleaseVersionChangedInvocations
                    .Add(new OnPublicationLatestPublishedReleaseVersionChangedArgs(publication, oldLatestPublishedReleaseVersionId)))
            .Returns(Task.CompletedTask);
    }

    private record OnPublicationLatestPublishedReleaseVersionChangedArgs(Publication Publication, Guid? OldLatestPublishedReleaseVersionId);
    
    public class Asserter(AdminEventRaiserServiceMockBuilder mockBuilder)
    {
        public void ThatOnThemeUpdatedRaised(Func<Theme, bool>? predicate = null) => 
            mockBuilder._mock.Verify(m => m.OnThemeUpdated(It.Is<Theme>(t => predicate == null || predicate(t))), Times.Once);

        public void OnReleaseSlugChangedWasRaised(
            Guid? expectedReleaseId = null,
            string? expectedNewReleaseSlug = null,
            Guid? expectedPublicationId = null,
            string? expectedPublicationSlug = null) => 
            mockBuilder._mock.Verify(m => m.OnReleaseSlugChanged(
                It.Is<Guid>(releaseId => expectedReleaseId == null || releaseId == expectedReleaseId), 
                It.Is<string>(newReleaseSlug => expectedNewReleaseSlug == null || newReleaseSlug == expectedNewReleaseSlug), 
                It.Is<Guid>(publicationId => expectedPublicationId == null || publicationId == expectedPublicationId), 
                It.Is<string>(publicationSlug => expectedPublicationSlug == null || publicationSlug == expectedPublicationSlug)),
                Times.Once);

        public void OnReleaseSlugChangedWasNotRaised() => 
            mockBuilder._mock.Verify(OnReleaseSlugChanged, Times.Never);

        public void OnPublicationChangedWasRaised(Publication? publication = null) =>
            mockBuilder._mock.Verify(m => m.OnPublicationChanged(It.Is<Publication>(p => 
                publication == null 
                || new PublicationChangedEventDto(p) == new PublicationChangedEventDto(publication))), Times.Once);

        public void OnPublicationLatestPublishedReleaseVersionChangedWasRaised(
            Publication? publication = null,
            Guid? previousReleaseVersionId = null)
        {
            var expectedEvent = new PublicationLatestPublishedReleaseVersionChangedEventDto(
                publication, 
                previousReleaseVersionId);

            Xunit.Assert.Single(
                mockBuilder._onPublicationLatestPublishedReleaseVersionChangedInvocations, 
                inv =>
                    new PublicationLatestPublishedReleaseVersionChangedEventDto(
                        inv.Publication,
                        inv.OldLatestPublishedReleaseVersionId)
                    == expectedEvent);
        }
            

        public void OnPublicationChangedWasNotRaised()
        {
            mockBuilder._mock.Verify(m => m.OnPublicationChanged(It.IsAny<Publication>()), Times.Never);
            mockBuilder._mock.Verify(m => m.OnPublicationLatestPublishedReleaseVersionChanged(
                It.IsAny<Publication>(),
                It.IsAny<Guid?>()), 
                Times.Never);
        }
    }
    public Asserter Assert => new(this);
}
