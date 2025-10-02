#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetVersionServiceMockBuilder
{
    private readonly Mock<IDataSetVersionService> _mock = new(MockBehavior.Strict);

    public IDataSetVersionService Build() => _mock.Object;

    public DataSetVersionServiceMockBuilder()
    {
        _mock
            .Setup(m =>
                m.UpdateVersionsForReleaseVersion(
                    It.IsAny<Guid>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);
    }
}
