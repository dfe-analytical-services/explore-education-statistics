#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

/// <summary>
/// Provides details about a value that was incorrectly formatted.
/// </summary>
/// <param name="Value">The invalid value</param>
/// <param name="ExpectedFormat">The format that was expected</param>
public record FormatErrorDetail(string Value, string ExpectedFormat) : InvalidErrorDetail<string>(Value);
