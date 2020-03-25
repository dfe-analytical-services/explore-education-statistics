using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class TableBuilderControllerTests
    {
        private readonly TableBuilderController _controller;
        private readonly ObservationQueryContext _query = new ObservationQueryContext();

        public TableBuilderControllerTests()
        {
            var tableBuilderService = new Mock<IDataService<TableBuilderResultViewModel>>();

            tableBuilderService.Setup(s => s.Query(_query)).ReturnsAsync(new TableBuilderResultViewModel
            {
                Results = new List<ObservationViewModel>
                {
                    new ObservationViewModel()
                }
            });

            _controller = new TableBuilderController(tableBuilderService.Object);
        }

        [Fact]
        public async Task Query_Post()
        {
            var result = await _controller.Query(_query);
            Assert.IsType<TableBuilderResultViewModel>(result.Value);
        }
    }
}