using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Builders;

public class DataSetPublishingServiceBuilder
{
    private readonly Mock<IDataSetPublishingService> _mock = new(MockBehavior.Strict);
    public IDataSetPublishingService Build() => _mock.Object;
    
    public DataSetPublishingServiceBuilder()
    {
        _mock
            .Setup(m => m.PublishDataSets(It.IsAny<Guid[]>()))
            .Returns(Task.CompletedTask);
    }
}
