using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders.Services;

public class DataSetPublishingServiceMockBuilder
{
    private readonly Mock<IDataSetPublishingService> _mock = new(MockBehavior.Strict);
    public IDataSetPublishingService Build() => _mock.Object;
    public Asserter Assert => new(_mock);
    public DataSetPublishingServiceMockBuilder()
    {
        _mock
            .Setup(m => m.PublishDataSets(It.IsAny<Guid[]>()))
            .Returns(Task.CompletedTask);
    }

    public class Asserter(Mock<IDataSetPublishingService> mock)
    {
        public void DataSetsWerePublished(params Guid[] expected)
        {
            mock.Verify(m => m.PublishDataSets(It.Is<Guid[]>(
                actual => actual.OrderBy(g => g).SequenceEqual(expected.OrderBy(g => g)))));
        }
    }
}
