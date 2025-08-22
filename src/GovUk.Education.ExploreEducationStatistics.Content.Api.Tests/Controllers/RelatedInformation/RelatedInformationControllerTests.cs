using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.RelatedInformation;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers.RelatedInformation.Dtos;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Content.Services.RelatedInformation.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers.RelatedInformation;

public abstract class RelatedInformationControllerTests
{
    private readonly RelatedInformationServiceMockBuilder _relatedInformationService = new();

    public class GetRelatedInformationForReleaseTests : RelatedInformationControllerTests
    {
        [Fact]
        public async Task GetRelatedInformationForRelease_ReturnsExpectedOrganisations()
        {
            // Arrange
            const string publicationSlug = "test-publication";
            const string releaseSlug = "test-release";
            RelatedInformationDto[] relatedInformation =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Related information 1",
                    Url = "https://example.com/related-1"
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Related information 2",
                    Url = "https://example.com/related-2"
                }
            ];
            _relatedInformationService.WhereHasRelatedInformation(relatedInformation);

            var sut = BuildController();

            // Act
            var result = await sut.GetRelatedInformationForRelease(
                publicationSlug: publicationSlug,
                releaseSlug: releaseSlug,
                CancellationToken.None);

            // Assert
            result.AssertOkResult(relatedInformation);
        }
    }

    private RelatedInformationController BuildController()
    {
        return new RelatedInformationController(
            _relatedInformationService.Build()
        );
    }
}
