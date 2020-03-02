using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class DataControllerTests
    {
        private readonly DataController _controller;

        private readonly ObservationQueryContext _query = new ObservationQueryContext
        {
            SubjectId = Guid.NewGuid()
        };

        public DataControllerTests()
        {
            var (logger, dataService) = Mocks();

            _controller = new DataController(
                dataService.Object, logger.Object
            );
        }

        [Fact]
        public async Task Query_Post()
        {
            var result = await _controller.Query(_query);
            Assert.IsAssignableFrom<ResultWithMetaViewModel>(result.Value);
            Assert.Single(result.Value.Result);
        }

        [Fact]
        public async Task Query_Post_NoResult()
        {
            var result = await _controller.Query(new ObservationQueryContext());
            Assert.IsAssignableFrom<ResultWithMetaViewModel>(result.Value);
            Assert.Empty(result.Value.Result);
        }

        private (
            Mock<ILogger<DataController>>,
            Mock<IDataService<ResultWithMetaViewModel>>) Mocks()
        {
            var dataService = new Mock<IDataService<ResultWithMetaViewModel>>();

            dataService.Setup(s => s.Query(It.IsNotIn(_query))).ReturnsAsync(new ResultWithMetaViewModel());

            dataService.Setup(s => s.Query(_query)).ReturnsAsync(
                new ResultWithMetaViewModel
                {
                    Result = new List<ObservationViewModel>
                    {
                        new ObservationViewModel()
                    }
                });

            return (new Mock<ILogger<DataController>>(), dataService);
        }
    }
}