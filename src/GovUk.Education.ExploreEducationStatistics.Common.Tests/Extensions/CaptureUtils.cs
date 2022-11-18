using System;
using System.IO;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

public static class CaptureUtils
{
    public static CaptureMatch<Stream> CaptureStreamAsArrayOfLines(Action<string[]> action)
    {
        return new CaptureMatch<Stream>(stream =>
        {
            using var reader = new StreamReader(stream);
            stream.Position = 0;
            var contents = reader.ReadToEnd().Split(Environment.NewLine);
            action.Invoke(contents);
        });
    }
}