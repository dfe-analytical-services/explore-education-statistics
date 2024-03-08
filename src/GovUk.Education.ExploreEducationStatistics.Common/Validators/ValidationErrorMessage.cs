#nullable enable
namespace GovUk.Education.ExploreEducationStatistics.Common.Validators;

public record ValidationErrorMessage(string Code, string Single, string Plural = "");
