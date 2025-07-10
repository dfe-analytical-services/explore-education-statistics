using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Requests;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly AnalyticsManagerMockBuilder _analyticsManager = new();

    private AnalyticsController GetSut() => new(
        _analyticsManager.Build(),
        new NullLogger<AnalyticsController>());

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task
        GivenValidRecordTableToolDownloadRequestBindingModel_WhenCallToRecordDownload_ThenInformationPassedToAnalyticsManager()
    {
        // ARRANGE
        var bindingModel = new RecordTableToolDownloadRequestBindingModelBuilder().Build();
        var sut = GetSut();

        // ACT
        var response = await sut.RecordDownload(bindingModel);
        
        // ASSERT
        response.AssertOkResult();
        var expected = bindingModel.ToModel();
        _analyticsManager.Assert.RequestAdded(expected);
    }
}
