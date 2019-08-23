using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Debug;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class DebugControllerTests
    {
        private readonly DebugController _controller;

        public DebugControllerTests()
        {
            var filterService = new Mock<IFilterService>();
            var indicatorService = new Mock<IIndicatorService>();
            var locationService = new Mock<ILocationService>();
            var observationService = new Mock<IObservationService>();
            var releaseService = new Mock<IReleaseService>();
            var schoolService = new Mock<ISchoolService>();
            var subjectService = new Mock<ISubjectService>();

            filterService.Setup(g => g.Count()).Returns(Task.FromResult(100));
            indicatorService.Setup(g => g.Count()).Returns(Task.FromResult(200));
            locationService.Setup(g => g.Count()).Returns(Task.FromResult(300));
            observationService.Setup(g => g.Count()).Returns(Task.FromResult(400));
            releaseService.Setup(g => g.Count()).Returns(Task.FromResult(500));
            schoolService.Setup(g => g.Count()).Returns(Task.FromResult(600));
            subjectService.Setup(g => g.Count()).Returns(Task.FromResult(700));

            _controller = new DebugController(
                filterService.Object,
                indicatorService.Object,
                locationService.Object,
                observationService.Object,
                releaseService.Object,
                schoolService.Object,
                subjectService.Object
            );
        }

        [Fact]
        public async void DebugReport()
        {
            var result = _controller.GetReport();

            Assert.IsType<Task<ActionResult<DebugReport>>>(result);

            var actionResult = await result;

            Assert.Equal(100, actionResult.Value.FilterCount);
            Assert.Equal(200, actionResult.Value.IndicatorCount);
            Assert.Equal(300, actionResult.Value.LocationCount);
            Assert.Equal(400, actionResult.Value.ObservationCount);
            Assert.Equal(500, actionResult.Value.ReleaseCount);
            Assert.Equal(600, actionResult.Value.SchoolCount);
            Assert.Equal(700, actionResult.Value.SubjectCount);
        }
    }
}