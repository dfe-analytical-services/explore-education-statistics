using System;
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class FileNameValidators
{
    public const int MaxFileNameSize = 150;

    public static bool MeetsLengthRequirements(string filename)
        => filename.Length is > 0 and < MaxFileNameSize;

    public static bool DoesNotContainSpaces(string filename)
        => !filename.Contains(' ', StringComparison.Ordinal);

    public static bool DoesNotContainSpecialChars(string filename)
        => !filename.Contains('&', StringComparison.Ordinal) &&
           filename.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
}
