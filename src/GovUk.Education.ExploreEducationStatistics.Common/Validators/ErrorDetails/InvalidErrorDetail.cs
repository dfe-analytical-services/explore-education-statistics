namespace GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

/// <summary>
/// Provides details of an invalid value that caused a validation error.
/// </summary>
/// <param name="Value">The invalid value</param>
/// <typeparam name="T">The invalid value's type</typeparam>
public record InvalidErrorDetail<T>(T Value);
