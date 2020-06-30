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
    public class TableBuilderControllerTests
    {
        private readonly TableBuilderController _controller;
        private readonly Guid _releaseId = new Guid("03730cff-22d5-446c-8971-68921e933b50");

        private readonly ObservationQueryContext _query = new ObservationQueryContext
        {
            SubjectId = Guid.NewGuid()
        };

        public TableBuilderControllerTests()
        {
            var (logger, tableBuilderService) = Mocks();
            
            _controller = new TableBuilderController(tableBuilderService.Object, logger.Object);
        }

        [Fact]
        public async Task Query_Post()
        {
            var result = await _controller.Query(_releaseId, _query);
            Assert.IsAssignableFrom<TableBuilderResultViewModel>(result.Value);
            Assert.Single(result.Value.Results);
        }
        
        [Fact]
        public async Task Query_Post_NoResult()
        {
            var result = await _controller.Query(_releaseId, new ObservationQueryContext());
            Assert.IsAssignableFrom<TableBuilderResultViewModel>(result.Value);
            Assert.Empty(result.Value.Results);
        }
        
        private (
            Mock<ILogger<TableBuilderController>>,
            Mock<ITableBuilderService>) Mocks()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService.Setup(s => s.Query(_releaseId, It.IsNotIn(_query))).ReturnsAsync(
                new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>()
                });

            tableBuilderService.Setup(s => s.Query(_releaseId, _query)).ReturnsAsync(
                new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>
                    {
                        new ObservationViewModel()
                    }
                });

            return (
                new Mock<ILogger<TableBuilderController>>(),
                tableBuilderService);
        }
    }
}