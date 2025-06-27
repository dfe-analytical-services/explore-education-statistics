using System;
using System.IO;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Validators;

public static class FileNameValidators
{
    public const int MaxFileNameSize = 150;

    public static bool MeetsLengthRequirements(string fileName)
    {
        var fileNamePart = Path.GetFileNameWithoutExtension(fileName);

        return fileNamePart.Length is > 0 and <= MaxFileNameSize;
    }

    public static bool ContainsSpaces(string fileName)
        => fileName.Contains(' ', StringComparison.Ordinal);

    public static bool ContainsSpecialChars(string fileName)
        => fileName.Contains('&', StringComparison.Ordinal) ||
           fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1;
}
