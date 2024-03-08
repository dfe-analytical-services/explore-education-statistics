#nullable enable
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

/// <summary>
/// Provides details of any invalid items that caused a validation error.
/// </summary>
/// <typeparam name="T">The invalid item type</typeparam>
public record InvalidItemsErrorDetail<T>(IEnumerable<T> Invalid);
