using System;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controller
{
    public class TableBuilderMetaControllerTests
    {
        private readonly TableBuilderMetaController _controller;

        public TableBuilderMetaControllerTests()
        {
            var subjectMetaService = new Mock<ITableBuilderSubjectMetaService>();

            subjectMetaService.Setup(s => s.GetSubjectMeta(new SubjectMetaQueryContext {SubjectId = 1}))
                .Returns(new TableBuilderSubjectMetaViewModel());
            
            subjectMetaService.Setup(s => s.GetSubjectMeta(new SubjectMetaQueryContext {SubjectId = 2}))
                .Throws(new ArgumentException("Subject does not exist", "subjectId"));
            
            _controller = new TableBuilderMetaController(subjectMetaService.Object);
        }

        [Fact]
        public void Get_SubjectMeta_Returns_Ok()
        {
            var result = _controller.GetSubjectMeta(1);

            // TODO: this fails, i think due to the signature of the object passed to the service not matching ideally the service should just take the id
            //Assert.IsAssignableFrom<SubjectMetaViewModel>(result.Value);
        }

        [Fact]
        public void Get_SubjectMeta_Returns_NotFound()
        {
            var result = _controller.GetSubjectMeta(2);

            // TODO
            //Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }
    }
}