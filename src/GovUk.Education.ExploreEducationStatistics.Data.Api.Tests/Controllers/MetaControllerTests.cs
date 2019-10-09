using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using PublicationId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests.Controllers
{
    public class MetaControllerTests
    {
        private readonly MetaController _controller;

        private readonly PublicationId _publicationId = PublicationId.NewGuid();

        public MetaControllerTests()
        {
            var publicationMetaService = new Mock<IPublicationMetaService>();
            var themeMetaService = new Mock<IThemeMetaService>();

            publicationMetaService.Setup(s => s.GetSubjectsForLatestRelease(_publicationId))
                .Returns(new PublicationSubjectsMetaViewModel());

            themeMetaService.Setup(s => s.GetThemes())
                .Returns(new List<ThemeMetaViewModel>
                {
                    new ThemeMetaViewModel()
                });

            _controller = new MetaController(publicationMetaService.Object, themeMetaService.Object);
        }

        [Fact]
        public void Get_SubjectsForLatestRelease_Returns_Ok()
        {
            var result = _controller.GetSubjectsForLatestRelease(_publicationId);

            Assert.IsAssignableFrom<PublicationSubjectsMetaViewModel>(result.Value);
        }

        [Fact]
        public void Get_SubjectsForLatestRelease_Returns_NotFound()
        {
            var result = _controller.GetSubjectsForLatestRelease(new Guid("9ad58d8b-997a-4dba-9255-d0caeae176ab"));

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