using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services
{
    public interface IImporter
    {
        void Import(IEnumerable<string> lines, Release release);
    }
}