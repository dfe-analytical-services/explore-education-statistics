using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Utils;

public class RandomUtils
{
    private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    public static string GenerateRandomAlphanumericString(int length)
    {
        var random = new Random();
        var randomChars = new char[length];
        for (var i = 0; i < length; i++)
        {
            randomChars[i] = Chars[random.Next(Chars.Length)];
        }

        return new string(randomChars);
    }
}
