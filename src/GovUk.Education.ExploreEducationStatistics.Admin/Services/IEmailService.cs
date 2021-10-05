#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public interface IEmailService
    {
        Either<ActionResult, Unit> SendEmail(
            string email,
            string templateId,
            Dictionary<string, dynamic> values);
    }
}
