#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public class ContactSaveRequest : IValidatableObject
{
    [Required] public string TeamName { get; set; } = string.Empty;

    [Required, EmailAddress] public string TeamEmail { get; set; } = string.Empty;

    [Required] public string ContactName { get; set; } = string.Empty;

    public string? ContactTelNo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        if (ContactTelNo != null)
        {
            if (ContactTelNo.Length < 8)
            {
                results.Add(new ValidationResult(
                    "Contact telephone must have a minimum length of '8'."));
            }

            if (!Regex.IsMatch(ContactTelNo, @"^[0-9\s]*$"))
            {
                results.Add(new ValidationResult(
                    "Contact telephone must only contain numbers and spaces"));
            }

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
