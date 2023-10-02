#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common.Validators;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class ContactSaveRequest : IValidatableObject
{
    [Required] public string TeamName { get; set; } = string.Empty;

    [Required, EmailAddress] public string TeamEmail { get; set; } = string.Empty;

    [Required] public string ContactName { get; set; } = string.Empty;

    [PhoneNumber] public string? ContactTelNo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        if (ContactTelNo != null)
        {
            if (Regex.IsMatch(ContactTelNo,
                    @"^\s*0\s*3\s*7\s*0\s*0\s*0\s*0\s*2\s*2\s*8\s*8\s*$"))
            {
                results.Add(new ValidationResult(
                    "Contact telephone cannot be DfE Enquiries number"));
            }
        }

        return results;
    }
}
