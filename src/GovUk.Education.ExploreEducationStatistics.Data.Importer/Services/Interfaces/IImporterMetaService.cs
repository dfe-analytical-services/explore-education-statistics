using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Importer.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Importer.Services.Interfaces
{
    public interface IImporterMetaService
    {
        SubjectMeta Import(IEnumerable<string> lines, Subject subject);

        SubjectMeta Get(IEnumerable<string> lines, Subject subject);
    }
}