using CSharpFunctionalExtensions.HttpResults;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Errors;
using Microsoft.AspNetCore.Http.HttpResults;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.ErrorMappers;

public class ResourceNotFoundErrorMapper : IResultErrorMapper<ResourceNotFoundError, ProblemHttpResult>
{
    public ProblemHttpResult Map(ResourceNotFoundError resourceNotFoundError) =>
        resourceNotFoundError switch
        {
            PublicationNotFoundError error => NotFoundResult(
                "Publication not found",
                $"The publication '{error.PublicationSlug}' could not be found."
            ),
            ReleaseNotFoundError error => NotFoundResult(
                "Release not found",
                $"The release '{error.ReleaseSlug}' could not found in publication '{error.PublicationSlug}'."
            ),
            _ => TypedResults.Problem(),
        };

    private static ProblemHttpResult NotFoundResult(string title, string detail) =>
        TypedResults.Problem(
            title: title,
            detail: detail,
            statusCode: StatusCodes.Status404NotFound,
            type: ProblemDetailsMappingProvider.FindMapping(StatusCodes.Status404NotFound).Type
        );
}
