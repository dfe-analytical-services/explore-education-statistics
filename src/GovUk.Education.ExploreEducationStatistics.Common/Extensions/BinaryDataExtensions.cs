using System;

namespace GovUk.Education.ExploreEducationStatistics.Common.Extensions;

public static class BinaryDataExtensions
{
    /// <summary>
    /// Workaround for bug in BinaryData where BinaryData.Empty.ToString() throws a NullReferenceException
    /// See <see ref="https://github.com/dotnet/runtime/pull/68349"/>
    /// </summary>
    /// <param name="binaryData"></param>
    /// <returns></returns>
    public static string ToStringSafe(this BinaryData binaryData) => 
        binaryData.ToMemory().Length == 0 
            ? string.Empty 
            : binaryData.ToString();
}
