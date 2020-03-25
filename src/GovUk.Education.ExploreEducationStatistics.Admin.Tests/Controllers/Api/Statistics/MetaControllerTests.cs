using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class MetaControllerTests
    {
        private readonly MetaController _controller;

        private readonly ReleaseId _releaseId = ReleaseId.NewGuid();

        public MetaControllerTests()
        {
            var (releaseMetaService, persistenceHelper, userService) = Mocks();
            _controller = new MetaController(releaseMetaService.Object, persistenceHelper.Object, userService.Object);
        }

        [Fact]
        public void Get_SubjectsForRelease_Returns_Ok()
        {
            var result = _controller.GetSubjectsForRelease(_releaseId);

            Assert.IsAssignableFrom<OkObjectResult>(result.Result.Result);
            Assert.IsAssignableFrom<ReleaseSubjectsMetaViewModel>(((OkObjectResult) result.Result.Result).Value);
        }

        [Fact]
        public void Get_SubjectsForRelease_Returns_NotFound()
        {
            var result = _controller.GetSubjectsForRelease(new ReleaseId("17226c96-9285-4f3e-9eb9-0babc5e1bd1a"));

            Assert.IsAssignableFrom<NotFoundResult>(result.Result.Result);
        }
        
        private (
            Mock<IReleaseMetaService>, 
            Mock<IPersistenceHelper<ContentDbContext>>, 
            Mock<IUserService>) Mocks()
        {
            var releaseMetaService = new Mock<IReleaseMetaService>();

            releaseMetaService.Setup(s => s.GetSubjects(_releaseId))
                .Returns(new List<IdLabel>
                {
                    new IdLabel(ReleaseId.NewGuid(), "Absence by characteristic")
                });

            return (
                releaseMetaService,
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(), 
                MockUtils.AlwaysTrueUserService());
        }
    }
}