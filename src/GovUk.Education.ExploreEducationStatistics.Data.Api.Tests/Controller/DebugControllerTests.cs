using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
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
            var result = await _controller.GetReport();

            Assert.NotNull(result);

            var debugReport = result.Value;

            Assert.Equal(100, debugReport.FilterCount);
            Assert.Equal(200, debugReport.IndicatorCount);
            Assert.Equal(300, debugReport.LocationCount);
            Assert.Equal(400, debugReport.ObservationCount);
            Assert.Equal(500, debugReport.ReleaseCount);
            Assert.Equal(600, debugReport.SchoolCount);
            Assert.Equal(700, debugReport.SubjectCount);
        }
    }
}