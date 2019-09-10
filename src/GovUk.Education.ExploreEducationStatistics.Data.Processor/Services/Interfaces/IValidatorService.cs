using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IValidatorService
    {
        List<string> Validate(ImportMessage message, SubjectData subjectData);
    }
}