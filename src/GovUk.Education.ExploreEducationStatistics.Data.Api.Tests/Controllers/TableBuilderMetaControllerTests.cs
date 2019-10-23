using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class TableBuilderMetaControllerTests
    {
        private readonly TableBuilderMetaController _controller;

        private const long SubjectId = 1;
        private const long NotFoundId = 2;
        private readonly SubjectMetaQueryContext _queryContext = new SubjectMetaQueryContext
        {
            SubjectId = 1
        };
        
        public TableBuilderMetaControllerTests()
        {
            var logger = new Mock<ILogger<TableBuilderMetaController>>();

            var subjectMetaService = new Mock<ITableBuilderSubjectMetaService>();

            subjectMetaService.Setup(s => s.GetSubjectMeta(SubjectId))
                .Returns(new TableBuilderSubjectMetaViewModel());

            subjectMetaService.Setup(s => s.GetSubjectMeta(_queryContext))
                .Returns(new TableBuilderSubjectMetaViewModel());
            
            subjectMetaService.Setup(s => s.GetSubjectMeta(NotFoundId))
                .Throws(new ArgumentException("Subject does not exist", "subjectId"));
            
            subjectMetaService.Setup(s => s.GetSubjectMeta(It.IsNotIn(_queryContext)))
                .Throws(new ArgumentException("Subject does not exist", "subjectId"));

            _controller = new TableBuilderMetaController(subjectMetaService.Object, logger.Object);
        }

        [Fact]
        public void Get_SubjectMeta_Returns_Ok()
        {
            var result = _controller.GetSubjectMeta(SubjectId);
            Assert.IsAssignableFrom<TableBuilderSubjectMetaViewModel>(result.Value);
        }

        [Fact]
        public void Get_SubjectMeta_Returns_NotFound()
        {
            var result = _controller.GetSubjectMeta(NotFoundId);
            Assert.IsAssignableFrom<NotFoundObjectResult>(result.Result);
        }
        
        [Fact]
        public void Post_SubjectMeta_Returns_Ok()
        {
            var result = _controller.GetSubjectMeta(_queryContext);
            Assert.IsAssignableFrom<TableBuilderSubjectMetaViewModel>(result.Value);
        }
        
        [Fact]
        public void Post_SubjectMeta_Returns_NotFound()
        {
            var queryContextNotFound = new SubjectMetaQueryContext
            {
                SubjectId = 2
            };
            
            var result = _controller.GetSubjectMeta(queryContextNotFound);
            Assert.IsAssignableFrom<NotFoundObjectResult>(result.Result);
        }
    }
}