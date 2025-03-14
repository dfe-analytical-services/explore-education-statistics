using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class ContentServiceBuilder
{
    private readonly Mock<IContentService> _mock = new(MockBehavior.Strict);
    public Asserter Assert => new(_mock);

    public IContentService Build() => _mock.Object;

    public ContentServiceBuilder()
    {
        _mock
            .Setup(m => m.DeletePreviousVersionsDownloadFiles(It.IsAny<IReadOnlyList<Guid>>()))
            .Returns(Task.CompletedTask);
        
        _mock
            .Setup(m => m.DeletePreviousVersionsContent(It.IsAny<IReadOnlyList<Guid>>()))
            .Returns(Task.CompletedTask);
        
        _mock
            .Setup(m => m.UpdateCachedTaxonomyBlobs())
            .Returns(Task.CompletedTask);
    }

    public class Asserter(Mock<IContentService> mock)
    {
        public void DeletePreviousVersionsDownloadFilesCalled(params Guid[] expectedReleaseVersionIds)
        {
            mock.Verify(m => m.DeletePreviousVersionsDownloadFiles(It.Is<IReadOnlyList<Guid>>(actual => expectedReleaseVersionIds.OrderBy(g => g).SequenceEqual(actual.OrderBy(g => g)))), Times.Once);
        }
        
        public void DeletePreviousVersionsContentCalled(params Guid[] expectedReleaseVersionIds)
        {
            mock.Verify(m => m.DeletePreviousVersionsContent(It.Is<IReadOnlyList<Guid>>(actual => expectedReleaseVersionIds.OrderBy(g => g).SequenceEqual(actual.OrderBy(g => g)))), Times.Once);
        }

        public void UpdateCachedTaxonomyBlobsCalled()
        {
            mock.Verify(m => m.UpdateCachedTaxonomyBlobs(), Times.Once);
        }
    }
}
