using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class TableBuilderControllerTests
    {
        private readonly TableBuilderController _controller;

        private readonly ObservationQueryContext _query = new ObservationQueryContext();

        public TableBuilderControllerTests()
        {
            var tableBuilderService = new Mock<IDataService<TableResultViewModel>>();

            tableBuilderService.Setup(s => s.Query(It.IsNotIn(_query))).Returns(
                new TableResultViewModel
                {
                    Result = new List<ObservationViewModel>()
                });

            tableBuilderService.Setup(s => s.Query(_query)).Returns(
                new TableResultViewModel
                {
                    Result = new List<ObservationViewModel>
                    {
                        new ObservationViewModel()
                    }
                });

            _controller = new TableBuilderController(
                tableBuilderService.Object
            );
        }

        [Fact]
        public void Query_Post()
        {
            var result = _controller.Query(_query);
            Assert.IsAssignableFrom<ResultViewModel>(result.Value);
        }
    }
}