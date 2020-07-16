using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class MetaControllerTests
    {
        private readonly MetaController _controller;

        private readonly Guid _publicationId = Guid.NewGuid();

        public MetaControllerTests()
        {
            var publicationMetaService = new Mock<IPublicationMetaService>();
            var themeMetaService = new Mock<IThemeMetaService>();

            publicationMetaService
                .Setup(s => s.GetSubjectsForLatestRelease(_publicationId))
                .ReturnsAsync(new PublicationSubjectsMetaViewModel());

            publicationMetaService
                .Setup(s => s.GetSubjectsForLatestRelease(It.Is<Guid>(guid => guid != _publicationId)))
                .ReturnsAsync(new NotFoundResult());

            themeMetaService.Setup(s => s.GetThemes())
                .Returns(new List<ThemeMetaViewModel>
                {
                    new ThemeMetaViewModel()
                });

            _controller = new MetaController(publicationMetaService.Object, themeMetaService.Object);
        }

        [Fact]
        public async Task Get_SubjectsForLatestRelease_Returns_Ok()
        {
            var result = await _controller.GetSubjectsForLatestRelease(_publicationId);
            Assert.IsAssignableFrom<PublicationSubjectsMetaViewModel>(result.Value);
        }

        [Fact]
        public async Task Get_SubjectsForLatestRelease_Returns_NotFound()
        {
            var result = await _controller.GetSubjectsForLatestRelease(Guid.NewGuid());
            Assert.IsAssignableFrom<NotFoundResult>(result.Result);
        }

        [Fact]
        public void Get_Themes_Returns_Ok()
        {
            var result = _controller.GetThemes();

            Assert.IsAssignableFrom<IEnumerable<ThemeMetaViewModel>>(result.Value);
        }
    }
}