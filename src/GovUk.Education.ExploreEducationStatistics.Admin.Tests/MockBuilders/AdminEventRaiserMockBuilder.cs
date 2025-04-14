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

public class AdminEventRaiserMockBuilder
{
    private readonly Mock<IAdminEventRaiser> _mock = new(MockBehavior.Strict);
    private readonly List<InvokeArguments> _invocations = new();
    private static readonly Expression<Func<IAdminEventRaiser, Task>> OnReleaseSlugChanged =  
        m => m.OnReleaseSlugChanged(
            It.IsAny<Guid>(), 
            It.IsAny<string>(), 
            It.IsAny<Guid>(), 
            It.IsAny<string>());

    public IAdminEventRaiser Build() => _mock.Object;

    public AdminEventRaiserMockBuilder()
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
            .Setup(m => m.OnPublicationLatestPublishedReleaseReordered(
                It.IsAny<Publication>(),
                It.IsAny<Guid>()))
            .Callback((Publication publication, Guid oldLatestPublishedReleaseVersionId) => 
                _invocations
                    .Add(new InvokeArguments(publication, oldLatestPublishedReleaseVersionId)))
            .Returns(Task.CompletedTask);
    }

    private record InvokeArguments(Publication Publication, Guid OldLatestPublishedReleaseVersionId);
    
    public class Asserter(AdminEventRaiserMockBuilder mockBuilder)
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
                || new PublicationChangedEvent(p) == new PublicationChangedEvent(publication))), Times.Once);

        public void OnPublicationLatestPublishedReleaseReorderedWasRaised(
            Publication publication,
            Guid previousReleaseVersionId)
        {
            var expectedEvent = new PublicationLatestPublishedReleaseReorderedEvent(
                publication, 
                previousReleaseVersionId);

            Xunit.Assert.Single(
                mockBuilder._invocations, 
                inv =>
                    new PublicationLatestPublishedReleaseReorderedEvent(
                        inv.Publication,
                        inv.OldLatestPublishedReleaseVersionId)
                    == expectedEvent);
        }
            

        public void OnPublicationChangedWasNotRaised()
        {
            mockBuilder._mock.Verify(m => m.OnPublicationChanged(It.IsAny<Publication>()), Times.Never);
            mockBuilder._mock.Verify(m => m.OnPublicationLatestPublishedReleaseReordered(
                It.IsAny<Publication>(),
                It.IsAny<Guid>()), 
                Times.Never);
        }
    }
    public Asserter Assert => new(this);
}
