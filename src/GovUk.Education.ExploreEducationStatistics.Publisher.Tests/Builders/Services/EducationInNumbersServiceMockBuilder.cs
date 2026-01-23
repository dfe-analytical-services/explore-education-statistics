using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class EducationInNumbersServiceMockBuilder
{
    private readonly Mock<IEducationInNumbersService> _mock = new(MockBehavior.Strict);

    public IEducationInNumbersService Build() => _mock.Object;

    public Asserter Assert => new(_mock);

    public EducationInNumbersServiceMockBuilder()
    {
        _mock.Setup(m => m.UpdateEinTiles(It.IsAny<Guid[]>())).Returns(Task.CompletedTask);
    }

    public class Asserter(Mock<IEducationInNumbersService> mock)
    {
        public void UpdateEinTilesCalled()
        {
            mock.Verify(m => m.UpdateEinTiles(It.IsAny<Guid[]>()), Times.Once);
        }
    }
}
