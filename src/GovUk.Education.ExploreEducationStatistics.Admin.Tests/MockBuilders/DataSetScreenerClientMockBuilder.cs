#nullable enable
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Screener;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetScreenerClientMockBuilder
{
    private readonly Mock<IDataSetScreenerClient> _mock = new(MockBehavior.Strict);

    private static readonly Expression<Func<IDataSetScreenerClient, Task>> DeleteScreenerProgressAndCompletionFiles =
        m => m.DeleteScreenerProgressAndCompletionFiles(It.IsAny<IList<Guid>>(), It.IsAny<CancellationToken>());

    public DataSetScreenerClientMockBuilder()
    {
        _mock.Setup(DeleteScreenerProgressAndCompletionFiles).Returns(Task.CompletedTask);
    }

    public IDataSetScreenerClient Build() => _mock.Object;

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IDataSetScreenerClient> mock)
    {
        /// <summary>
        /// Asserts that progress and completion report files were deleted for exactly the given data
        /// set ids, and that no other calls were made to the Screener API.
        /// </summary>
        public void DeleteScreenerProgressAndCompletionFilesWasCalled(params Guid[] expectedDataSetIds)
        {
            mock.Verify(
                m =>
                    m.DeleteScreenerProgressAndCompletionFiles(
                        It.Is<IList<Guid>>(dataSetIds => dataSetIds.Order().SequenceEqual(expectedDataSetIds.Order())),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );

            // Guard against the deletion also being requested for any unexpected data sets.
            mock.VerifyNoOtherCalls();
        }

        public void DeleteScreenerProgressAndCompletionFilesWasNotCalled()
        {
            mock.Verify(DeleteScreenerProgressAndCompletionFiles, Times.Never);
        }
    }
}
