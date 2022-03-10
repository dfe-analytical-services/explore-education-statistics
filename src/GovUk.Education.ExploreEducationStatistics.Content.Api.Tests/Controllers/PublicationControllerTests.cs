#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers
{
    public class PublicationControllerTests
    {
        [Fact]
        public async Task GetPublicationTitle()
        {
            var publicationId = Guid.NewGuid();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(new Publication
                {
                    Id = publicationId,
                    Slug = "publication-a",
                    Title = "Test title"
                });
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var controller = BuildPublicationController(contentDbContext);

                var publicationTitleViewModel = (await controller.GetPublicationTitle("publication-a")).Value;

                Assert.NotNull(publicationTitleViewModel);
                Assert.IsType<PublicationTitleViewModel>(publicationTitleViewModel);
                Assert.Equal(publicationId, publicationTitleViewModel!.Id);
                Assert.Equal("Test title", publicationTitleViewModel.Title);

                MockUtils.VerifyAllMocks();
            }
        }

        [Fact]
        public async Task GetPublicationTitle_NotFound()
        {
            var contentDbContextId = Guid.NewGuid().ToString();
            await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
            var controller = BuildPublicationController(contentDbContext);

            var result = await controller.GetPublicationTitle("missing-publication");

            Assert.IsType<NotFoundResult>(result.Result);

            MockUtils.VerifyAllMocks();
        }

        private static PublicationController BuildPublicationController(
            ContentDbContext? contentDbContext = null
        )
        {
            return new PublicationController(
                contentDbContext != null
                    ? new PersistenceHelper<ContentDbContext>(contentDbContext)
                    : Mock.Of<IPersistenceHelper<ContentDbContext>>()
            );
        }
    }
}
