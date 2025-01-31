#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators
{
    public static class EmailValidator
    {
        public static Either<ActionResult, List<string>> ValidateEmailAddresses(IEnumerable<string> input)
        {
            var emails = input
                .Where(email => !email.IsNullOrWhitespace())
                .Select(line => line.Trim())
                .Distinct()
                .ToList();

            var emailAddressAttribute = new EmailAddressAttribute();
            if (emails.Any(email => !emailAddressAttribute.IsValid(email)))
            {
                return ValidationActionResult(InvalidEmailAddress);
            }

            return emails;
        }
    }
}
