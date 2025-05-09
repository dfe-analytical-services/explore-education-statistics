using System;
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class FileNameValidators
{
    public const int MaxFileNameSize = 150;

    public static bool MeetsLengthRequirements(string fileName)
        => fileName.Split('.')[0].Length is > 0 and < MaxFileNameSize;

    public static bool DoesNotContainSpaces(string fileName)
        => !fileName.Contains(' ', StringComparison.Ordinal);

    public static bool DoesNotContainSpecialChars(string fileName)
        => !fileName.Contains('&', StringComparison.Ordinal) &&
           fileName.IndexOfAny(Path.GetInvalidFileNameChars()) == -1;
}
