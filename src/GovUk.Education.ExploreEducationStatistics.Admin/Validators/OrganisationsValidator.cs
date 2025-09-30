#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using LinqToDB.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public class OrganisationsValidator(ContentDbContext context) : IOrganisationsValidator
{
    /// <summary>
    /// Validates a list of organisation id's by checking for existing organisations matching those id's.
    /// Returns either a list of organisations or a list of validation errors.
    /// </summary>
    /// <param name="organisationIds">An array of organisation id's to validate.</param>
    /// <param name="path">An optional path to a property in a request that the error relates to include in error details for context.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>
    /// An <see cref="Either{TLeft, TRight}"/> containing:
    /// - A list of <see cref="Organisation"/> objects if all id's are valid.
    /// - A Bad Request <see cref="ActionResult"/> if any id's are invalid.
    /// </returns>
    public async Task<Either<ActionResult, Organisation[]>> ValidateOrganisations(
        Guid[]? organisationIds,
        string? path = null,
        CancellationToken cancellationToken = default)
    {
        if (organisationIds.IsNullOrEmpty())
        {
            return Array.Empty<Organisation>();
        }

        var organisations = await context.Organisations
            .Where(o => organisationIds.Contains(o.Id))
            .Distinct()
            .ToArrayAsync(cancellationToken: cancellationToken);

        var organisationNotFoundErrors = organisationIds
            .Except(organisations.Select(o => o.Id))
            .Select(organisationId => new ErrorViewModel
            {
                Code = ValidationMessages.OrganisationNotFound.Code,
                Message = ValidationMessages.OrganisationNotFound.Message,
                Path = path,
                Detail = new InvalidErrorDetail<Guid>(organisationId)
            })
            .ToArray();

        if (organisationNotFoundErrors.Length > 0)
        {
            return Common.Validators.ValidationUtils.ValidationResult(organisationNotFoundErrors);
        }

        return organisations;
    }
}
