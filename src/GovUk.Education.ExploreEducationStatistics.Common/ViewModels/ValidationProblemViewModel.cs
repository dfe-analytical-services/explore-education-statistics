#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

/// <summary>
/// A validation problem that has occurred with the API request.
/// This type of response will be composed of one or more errors
/// relating to the request and its properties.
/// </summary>
public class ValidationProblemViewModel : ValidationProblemDetails
{
    /// <inheritdoc cref="ValidationProblemDetails.Status" />
    public new int Status => StatusCodes.Status400BadRequest;

    /// <summary>
    /// Additional errors that are related to the validation problem.
    /// </summary>
    public new IReadOnlyList<ErrorViewModel> Errors { get; set; } = new List<ErrorViewModel>();

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
        };

        viewModel.Extensions.AddRange(problemDetails.Extensions);

        return viewModel;
    }
}
