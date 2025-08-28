namespace GovUk.Education.ExploreEducationStatistics.Common.Validators.ErrorDetails;

/// <summary>
/// Provides details of items that could not be found.
/// </summary>
/// <param name="Items">The items that could not be found.</param>
/// <typeparam name="T">The type of each item.</typeparam>
public record NotFoundItemsErrorDetail<T>(IEnumerable<T> Items);
