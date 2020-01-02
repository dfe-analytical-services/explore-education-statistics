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

        private static readonly Guid SubjectId = new Guid("3a5cbb72-ccd5-4afd-9a72-0f73d323d192");
        private static readonly Guid NotFoundId =  new Guid("9d20d622-b9ec-4199-9d89-57726a8588cb");
        private readonly SubjectMetaQueryContext _queryContext = new SubjectMetaQueryContext
        {
            SubjectId = SubjectId
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
                SubjectId = NotFoundId
            };
            
            var result = _controller.GetSubjectMeta(queryContextNotFound);
            Assert.IsAssignableFrom<NotFoundObjectResult>(result.Result);
        }
    }
}