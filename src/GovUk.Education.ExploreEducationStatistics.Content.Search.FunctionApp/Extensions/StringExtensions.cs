using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Decodes a Base64 encoded string back to its original representation.
    /// </summary>
    /// <param name="input">The Base64 encoded string to be decoded. If null, the method returns null.</param>
    /// <returns>
    /// The original string decoded from the Base64 input, or null if the input is null.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? FromBase64String(this string? input)
    {
        if (input == null)
        {
            return null;
        }

        var bytes = Convert.FromBase64String(input);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Converts a string to its Base64 encoded representation.
    /// </summary>
    /// <param name="input">The input string to be encoded. If null, the method returns null.</param>
    /// <returns>
    /// A Base64 encoded string representation of the input, or null if the input is null.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? ToBase64String(this string? input)
    {
        if (input == null)
        {
            return null;
        }

        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Converts a string to its Base64 encoded representation.
    /// Removes all leading and trailing white-space characters from the input string before encoding.
    /// </summary>
    /// <param name="input">The input string to be trimmed and encoded. If null, the method returns null.</param>
    /// <returns>
    /// A Base64 encoded string representation of the trimmed input, or null if the input is null.
    /// </returns>
    [return: NotNullIfNotNull(nameof(input))]
    public static string? TrimAndBase64Encode(this string? input) => input?.Trim().ToBase64String();
}
