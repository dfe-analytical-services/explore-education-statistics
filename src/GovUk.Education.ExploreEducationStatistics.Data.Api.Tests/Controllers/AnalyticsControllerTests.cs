using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using FluentValidation.Results;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers;

public class AnalyticsControllerTests
{
    private readonly AnalyticsManagerMockBuilder _analyticsManager = new();

    private AnalyticsController GetSut() => new(
        _analyticsManager.Build());

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    public class TableBuilderTests : AnalyticsControllerTests
    {
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
            response.AssertAccepted();
            var expected = bindingModel.ToModel();
            _analyticsManager.Assert.RequestAdded(expected);
        }
    }

    public class PermalinkTests : AnalyticsControllerTests
    {
        [Fact]
        public async Task
            GivenValidRecordPermalinkTableDownloadRequestBindingModel_WhenCallToRecordDownload_ThenInformationPassedToAnalyticsManager()
        {
            // ARRANGE
            var bindingModel = new RecordPermalinkTableDownloadRequestBindingModelBuilder().Build();
            var sut = GetSut();

            // ACT
            var response = await sut.RecordDownload(bindingModel);

            // ASSERT
            response.AssertAccepted();
            var expected = bindingModel.ToModel();
            _analyticsManager.Assert.RequestAdded(expected);
        }
    }
}
