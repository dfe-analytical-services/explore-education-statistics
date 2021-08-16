#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces
{
    public interface IDataGuidanceFileWriter
    {
        Task<Stream> WriteToStream(Stream stream, Release release, IEnumerable<Guid>? subjectIds = null);
    }
}