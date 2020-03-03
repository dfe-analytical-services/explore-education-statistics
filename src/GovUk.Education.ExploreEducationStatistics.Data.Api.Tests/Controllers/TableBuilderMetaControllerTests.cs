using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta.TableBuilder;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class TableBuilderMetaControllerTests
    {
        private readonly TableBuilderMetaController _controller;

        private static readonly Guid SubjectId = new Guid("3a5cbb72-ccd5-4afd-9a72-0f73d323d192");
        private static readonly Guid NotFoundId = new Guid("9d20d622-b9ec-4199-9d89-57726a8588cb");
        private static readonly Guid NotPublishedId = new Guid("200b8b6e-a033-42ba-9af5-236194ea3c7f");

        private readonly SubjectMetaQueryContext _queryContext = new SubjectMetaQueryContext
        {
            SubjectId = SubjectId
        };

        private readonly SubjectMetaQueryContext _queryContextNotFound = new SubjectMetaQueryContext
        {
            SubjectId = NotFoundId
        };

        private readonly SubjectMetaQueryContext _queryContextNotPublished = new SubjectMetaQueryContext
        {
            SubjectId = NotPublishedId
        };

        public TableBuilderMetaControllerTests()
        {
            var subjectMetaService = new Mock<ITableBuilderSubjectMetaService>();

            subjectMetaService.Setup(s => s.GetSubjectMeta(SubjectId))
                .ReturnsAsync(new TableBuilderSubjectMetaViewModel());

            subjectMetaService.Setup(s => s.GetSubjectMeta(_queryContext))
                .ReturnsAsync(new TableBuilderSubjectMetaViewModel());

            subjectMetaService.Setup(s => s.GetSubjectMeta(NotFoundId)).ReturnsAsync(new NotFoundResult());

            subjectMetaService.Setup(s => s.GetSubjectMeta(NotPublishedId)).ReturnsAsync(new ForbidResult());

            subjectMetaService.Setup(s => s.GetSubjectMeta(
                    It.Is<SubjectMetaQueryContext>(queryContext => queryContext == _queryContextNotFound)))
                .ReturnsAsync(new NotFoundResult());

            subjectMetaService.Setup(s => s.GetSubjectMeta(
                    It.Is<SubjectMetaQueryContext>(queryContext => queryContext == _queryContextNotPublished)))
                .ReturnsAsync(new ForbidResult());

            _controller = new TableBuilderMetaController(subjectMetaService.Object);
        }

        [Fact]
        public async Task Get_SubjectMetaAsync_Returns_Ok()
        {
            var result = await _controller.GetSubjectMetaAsync(SubjectId);
            Assert.IsAssignableFrom<TableBuilderSubjectMetaViewModel>(result.Value);
        }

        [Fact]
        public async Task Get_SubjectMetaAsync_Returns_NotFound()
        {
            var result = await _controller.GetSubjectMetaAsync(NotFoundId);
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Get_SubjectMetaAsync_Returns_Forbidden()
        {
            var result = await _controller.GetSubjectMetaAsync(NotPublishedId);
            Assert.IsAssignableFrom<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task Post_SubjectMetaAsync_Returns_Ok()
        {
            var result = await _controller.GetSubjectMetaAsync(_queryContext);
            Assert.IsAssignableFrom<TableBuilderSubjectMetaViewModel>(result.Value);
        }

        [Fact]
        public async Task Post_SubjectMetaAsync_Returns_NotFound()
        {
            var result = await _controller.GetSubjectMetaAsync(_queryContextNotFound);
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Post_SubjectMetaAsync_Returns_Forbidden()
        {
            var result = await _controller.GetSubjectMetaAsync(_queryContextNotPublished);
            Assert.IsAssignableFrom<ForbidResult>(result.Result);
        }
    }
}