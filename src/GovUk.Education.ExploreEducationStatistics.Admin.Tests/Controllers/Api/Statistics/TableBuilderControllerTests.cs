using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class TableBuilderControllerTests
    {
        private readonly TableBuilderController _controller;

        private readonly ObservationQueryContext _query = new ObservationQueryContext();

        private readonly ReleaseId _releaseId = ReleaseId.NewGuid();

        public TableBuilderControllerTests()
        {
            var logger = new Mock<ILogger<TableBuilderController>>();
            
            var tableBuilderService = new Mock<IDataService<TableBuilderResultViewModel>>();

            tableBuilderService.Setup(s => s.Query(It.IsNotIn(_query), null)).Returns(
                new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>()
                });

            tableBuilderService.Setup(s => s.Query(_query, _releaseId)).Returns(
                new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>
                    {
                        new ObservationViewModel()
                    }
                });

            _controller = new TableBuilderController(
                tableBuilderService.Object, logger.Object
            );
        }

        [Fact]
        public void Query_Post()
        {
            var result = _controller.Query(_releaseId, _query);
            Assert.IsAssignableFrom<TableBuilderResultViewModel>(result.Value);
        }
    }
}