using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.TableBuilder;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Data.Api.Tests.Controller
{
    public class TableBuilderControllerTests
    {
        private readonly TableBuilderController _controller;

        private readonly ObservationQueryContext _query = new ObservationQueryContext();

        public TableBuilderControllerTests()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();

            tableBuilderService.Setup(s => s.Query(It.IsNotIn(_query))).Returns(
                new TableBuilderResultViewModel
                {
                    Result = new List<TableBuilderObservationViewModel>()
                });

            tableBuilderService.Setup(s => s.Query(_query)).Returns(
                new TableBuilderResultViewModel
                {
                    Result = new List<TableBuilderObservationViewModel>
                    {
                        new TableBuilderObservationViewModel()
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
            Assert.IsAssignableFrom<TableBuilderResultViewModel>(result.Value);
        }

        [Fact]
        public void Query_Post_NoResult_Returns_NotFound()
        {
            var result = _controller.Query(new ObservationQueryContext());
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
    }
}