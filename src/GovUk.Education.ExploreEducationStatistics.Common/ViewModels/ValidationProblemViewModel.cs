#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

/// <summary>
/// A validation problem that has occurred with the API request.
/// This type of response will be composed of one or more errors
/// relating to the request and its properties.
/// </summary>
public record ValidationProblemViewModel : ProblemDetailsViewModel
{
    /// <inheritdoc cref="ProblemDetailsViewModel.Status" />
    public new int Status => StatusCodes.Status400BadRequest;

    /// <summary>
    /// The errors relating to the validation problem.
    /// </summary>
    public IReadOnlyList<ErrorViewModel> Errors { get; set; } = new List<ErrorViewModel>();

    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public ValidationProblemDetails? OriginalDetails { get; init; }

    public static ValidationProblemViewModel Create(ValidationProblemDetails problemDetails)
    {
        var errors = problemDetails.Errors
            .SelectMany(errorEntry =>
                errorEntry.Value.Select(
                    message => new ErrorViewModel
                    {
                        Path = errorEntry.Key.IsNullOrEmpty() ? null : errorEntry.Key,
                        Message = message
                    }
                )
            )
            .ToList();

        return Create(problemDetails, errors);
    }

    public static ValidationProblemViewModel Create(
        ValidationProblemDetails problemDetails,
        IEnumerable<ErrorViewModel> errors)
    {
        var viewModel = new ValidationProblemViewModel
        {
            Title = problemDetails.Title,
            Type = problemDetails.Type,
            Detail = problemDetails.Detail,
            Instance = problemDetails.Instance,
            Errors = errors.ToList(),
            OriginalDetails = problemDetails,
        };

        viewModel.Extensions.AddRange(problemDetails.Extensions);

        return viewModel;
    }
}
