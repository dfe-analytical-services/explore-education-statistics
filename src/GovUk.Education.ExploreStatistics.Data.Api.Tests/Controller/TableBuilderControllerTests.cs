using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.TableBuilder;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels.Meta;
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
            var attributeMetaService = new Mock<IAttributeMetaService>();
            var characteristicMetaService = new Mock<ICharacteristicMetaService>();
            var mapper = new Mock<IMapper>();

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

            var attributeMeta = new AttributeMeta
            {
                Name = "num_schools",
                Label = "Number of schools",
                Group = "Exclusion fields",
                KeyIndicator = false,
                Unit = Unit.Number
            };

            mapper.Setup(m => m.Map<AttributeMetaViewModel>(attributeMeta))
                .Returns(new AttributeMetaViewModel());

            attributeMetaService.Setup(s => s.Get(new Guid("8fe9c479-1ab5-4894-81cd-9f87882e20ed")))
                .Returns(new List<AttributeMeta>
                {
                    attributeMeta
                });

            var characteristicMeta = new CharacteristicMeta
            {
                Name = "Total",
                Label = "All pupils",
                Group = "Total"
            };

            mapper.Setup(m => m.Map<CharacteristicViewModel>(characteristicMeta))
                .Returns(new CharacteristicViewModel());

            characteristicMetaService.Setup(s => s.Get(new Guid("8fe9c479-1ab5-4894-81cd-9f87882e20ed")))
                .Returns(new List<CharacteristicMeta>
                {
                    characteristicMeta
                });

            _controller = new TableBuilderController(
                tableBuilderService.Object,
                attributeMetaService.Object,
                characteristicMetaService.Object,
                mapper.Object
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
        public void GetMeta_KnownId_Returns_ActionResult_WithMetaData(string testId)
        {
            var id = new Guid(testId);

            var result = _controller.GetMeta(id);

            var model = Assert.IsAssignableFrom<ActionResult<PublicationMetaViewModel>>(result);

            Assert.Equal(id, model.Value.PublicationId);

            // TODO: verify the meta data
        }

        [Fact]
        [InlineData("335043a6-e7d3-4573-8910-0f8eead36edb")]
        public void GetMeta_UnknownKnownId_Returns_NotFound()
        {
            var result = _controller.GetMeta(new Guid());

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}