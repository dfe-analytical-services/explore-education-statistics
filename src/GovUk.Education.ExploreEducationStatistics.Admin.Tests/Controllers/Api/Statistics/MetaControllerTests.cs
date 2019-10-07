using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Api.Statistics;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Statistics
{
    public class MetaControllerTests
    {
        private readonly MetaController _controller;

        private readonly ReleaseId _releaseId = ReleaseId.NewGuid();

        public MetaControllerTests()
        {
            var releaseMetaService = new Mock<IReleaseMetaService>();

            releaseMetaService.Setup(s => s.GetSubjects(_releaseId))
                .Returns(new ReleaseSubjectsMetaViewModel());

            _controller = new MetaController(releaseMetaService.Object);
        }

        [Fact]
        public void Get_SubjectsForRelease_Returns_Ok()
        {
            var result = _controller.GetSubjectsForRelease(_releaseId);

            Assert.IsAssignableFrom<ReleaseSubjectsMetaViewModel>(result.Value);
        }

        [Fact]
        public void Get_SubjectsForRelease_Returns_NotFound()
        {
            var result = _controller.GetSubjectsForRelease(new ReleaseId("17226c96-9285-4f3e-9eb9-0babc5e1bd1a"));

            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
    }
}