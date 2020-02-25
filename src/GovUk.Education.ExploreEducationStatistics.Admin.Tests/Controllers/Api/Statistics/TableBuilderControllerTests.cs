using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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

        private readonly ObservationQueryContext _query = new ObservationQueryContext
        {
            SubjectId = ReleaseId.NewGuid()
        };

        private readonly ReleaseId _releaseId = ReleaseId.NewGuid();

        public TableBuilderControllerTests()
        {
            var (logger, tableBuilderService, persistenceHelper, userService) = Mocks();
            
            _controller = new TableBuilderController(
                tableBuilderService.Object, logger.Object, persistenceHelper.Object, userService.Object
            );
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
            Mock<IDataService<TableBuilderResultViewModel>>, 
            Mock<IPersistenceHelper<ContentDbContext>>, 
            Mock<IUserService>) Mocks()
        {
            var tableBuilderService = new Mock<IDataService<TableBuilderResultViewModel>>();

            tableBuilderService.Setup(s => s.Query(It.IsNotIn(_query), _releaseId)).ReturnsAsync(
                new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>()
                });

            tableBuilderService.Setup(s => s.Query(_query, _releaseId)).ReturnsAsync(
                new TableBuilderResultViewModel
                {
                    Results = new List<ObservationViewModel>
                    {
                        new ObservationViewModel()
                    }
                });

            return (
                new Mock<ILogger<TableBuilderController>>(),
                tableBuilderService,
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(), 
                MockUtils.AlwaysTrueUserService());
        }
    }
}