using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreStatistics.Data.Api.Tests.Controller
{
    public class TableBuilderControllerTests
    {
        private readonly TableBuilderController _controller;

        private readonly NationalQueryContext _testNationalQuery = new NationalQueryContext();
        private readonly LaQueryContext _laQuery = new LaQueryContext();
        private readonly GeographicQueryContext _geographicQuery = new GeographicQueryContext();

        public TableBuilderControllerTests()
        {
            var tableBuilderService = new Mock<ITableBuilderService>();
            var releaseService = new Mock<IReleaseService>();
            var subjectService = new Mock<ISubjectService>();

            tableBuilderService.Setup(s => s.GetNational(It.IsNotIn(_testNationalQuery))).Returns(
                new TableBuilderResult
                {
                    Result = new List<ITableBuilderData>()
                });

            tableBuilderService.Setup(s => s.GetNational(_testNationalQuery)).Returns(
                new TableBuilderResult
                {
                    Result = new List<ITableBuilderData>
                    {
                        new TableBuilderCharacteristicData()
                    }
                });

            tableBuilderService.Setup(s => s.GetLocalAuthority(It.IsNotIn(_laQuery))).Returns(
                new TableBuilderResult
                {
                    Result = new List<ITableBuilderData>()
                });

            tableBuilderService.Setup(s => s.GetLocalAuthority(_laQuery)).Returns(
                new TableBuilderResult
                {
                    Result = new List<ITableBuilderData>
                    {
                        new TableBuilderCharacteristicData()
                    }
                });

            tableBuilderService.Setup(s => s.GetGeographic(It.IsNotIn(_geographicQuery))).Returns(
                new TableBuilderResult
                {
                    Result = new List<ITableBuilderData>()
                });

            tableBuilderService.Setup(s => s.GetGeographic(_geographicQuery)).Returns(
                new TableBuilderResult
                {
                    Result = new List<ITableBuilderData>
                    {
                        new TableBuilderGeographicData()
                    }
                });

            _controller = new TableBuilderController(
                releaseService.Object,
                subjectService.Object,
                tableBuilderService.Object
            );
        }

        [Fact]
        public void GetGeographic_Post()
        {
            var result = _controller.GetGeographic(_geographicQuery);
            Assert.IsAssignableFrom<TableBuilderResult>(result.Value);
        }

        [Fact]
        public void GetGeographic_Post_NoResult_Returns_NotFound()
        {
            var result = _controller.GetGeographic(new GeographicQueryContext());
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetLocalAuthority_Post()
        {
            var result = _controller.GetLocalAuthority(_laQuery);
            Assert.IsAssignableFrom<TableBuilderResult>(result.Value);
        }

        [Fact]
        public void GetLocalAuthority_Post_NoResult_Returns_NotFound()
        {
            var result = _controller.GetLocalAuthority(new LaQueryContext());
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void GetNational_Post()
        {
            var result = _controller.GetNational(_testNationalQuery);
            Assert.IsAssignableFrom<TableBuilderResult>(result.Value);
        }

        [Fact]
        public void GetNational_Post_NoResult_Returns_NotFound()
        {
            var result = _controller.GetNational(new NationalQueryContext());
            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}