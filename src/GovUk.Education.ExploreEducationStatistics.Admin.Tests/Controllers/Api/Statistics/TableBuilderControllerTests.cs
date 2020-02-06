using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;
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
            var (logger, tableBuilderService, persistenceHelper, userService) = Mocks();
            
            _controller = new TableBuilderController(
                tableBuilderService.Object, logger.Object, persistenceHelper.Object, userService.Object
            );
        }

        [Fact]
        public void Query_Post()
        {
            var result = _controller.Query(_releaseId, _query);
            Assert.IsAssignableFrom<OkObjectResult>(result.Result.Result);
            Assert.IsAssignableFrom<TableBuilderResultViewModel>(((OkObjectResult) result.Result.Result).Value);
        }
        
        private (
            Mock<ILogger<TableBuilderController>>,
            Mock<IDataService<TableBuilderResultViewModel>>, 
            Mock<IPersistenceHelper<ContentDbContext>>, 
            Mock<IUserService>) Mocks()
        {
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

            return (
                new Mock<ILogger<TableBuilderController>>(),
                tableBuilderService,
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(), 
                MockUtils.AlwaysTrueUserService());
        }
    }
}