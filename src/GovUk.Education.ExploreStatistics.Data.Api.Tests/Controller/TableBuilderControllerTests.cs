using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
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

            var subject = new Subject
            {
                Id = 1,
                Release = new Release(DateTime.Now, new Guid("8fe9c479-1ab5-4894-81cd-9f87882e20ed"))
            };

            subjectService
                .Setup(s => s.Find(It.Is<long>(l => l == 1), It.IsAny<List<Expression<Func<Subject, object>>>>()))
                .Returns(subject);

            subjectService
                .Setup(s => s.GetSubjectMetas(new Guid("8fe9c479-1ab5-4894-81cd-9f87882e20ed")))
                .Returns(new List<SubjectMetaViewModel>
                {
                    new SubjectMetaViewModel
                    {
                        Id = 1,
                        Label = "Geographic levels"
                    },
                    new SubjectMetaViewModel
                    {
                        Id = 2,
                        Label = "Local authority characteristics"
                    },
                    new SubjectMetaViewModel
                    {
                        Id = 3,
                        Label = "National characteristics"
                    }
                });

            subjectService
                .Setup(s => s.GetIndicatorMetas(subject.Id))
                .Returns(new Dictionary<string, IEnumerable<IndicatorMetaViewModel>>
                {
                    {
                        "Exclusion fields",
                        new List<IndicatorMetaViewModel>
                        {
                            new IndicatorMetaViewModel {Name = "num_schools", Label = "Number of schools", Unit = ""}
                        }
                    }
                });

            subjectService
                .Setup(s => s.GetCharacteristicMetas(subject.Id))
                .Returns(new Dictionary<string, IEnumerable<CharacteristicMetaViewModel>>
                {
                    {
                        "Total",
                        new List<CharacteristicMetaViewModel>
                        {
                            new CharacteristicMetaViewModel {Name = "Total", Label = "All pupils"}
                        }
                    }
                });

            _controller = new TableBuilderController(
                tableBuilderService.Object,
                releaseService.Object,
                subjectService.Object
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

        [Theory]
        [InlineData("8fe9c479-1ab5-4894-81cd-9f87882e20ed")]
        public void GetMeta_KnownId_Returns_ActionResult_WithSubjectMetaData(string testId)
        {
            var id = new Guid(testId);

            var result = _controller.GetMeta(id);

            var model = Assert.IsAssignableFrom<ActionResult<PublicationSubjectsMetaViewModel>>(result);

            Assert.Equal(id, model.Value.PublicationId);

            // TODO: verify the meta data
        }

        [Theory]
        [InlineData("335043a6-e7d3-4573-8910-0f8eead36edb")]
        public void GetMeta_UnknownKnownId_Returns_NotFound(string testId)
        {
            var id = new Guid(testId);

            var result = _controller.GetMeta(id);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Theory]
        [InlineData(1)]
        public void GetSubjectMeta_KnownId_Returns_ActionResult_WithMetaData(long testId)
        {
            var result = _controller.GetSubjectMeta(testId);

            var model = Assert.IsAssignableFrom<ActionResult<PublicationMetaViewModel>>(result);

            // TODO: verify the meta data
        }

        [Theory]
        [InlineData(2)]
        public void GetSubjectMeta_UnknownKnownId_Returns_NotFound(long testId)
        {
            var result = _controller.GetSubjectMeta(testId);

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}