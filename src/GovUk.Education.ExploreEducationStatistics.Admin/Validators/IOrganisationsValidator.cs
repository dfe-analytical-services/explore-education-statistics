﻿#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public interface IOrganisationsValidator
{
    Task<Either<ActionResult, Organisation[]>> ValidateOrganisations(
        Guid[]? organisationIds,
        string? path = null,
        CancellationToken cancellationToken = default);
}
