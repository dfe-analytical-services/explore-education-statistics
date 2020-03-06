using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IValidatorService
    {
        Tuple<List<string>,int, int> ValidateAndCountRows(SubjectData subjectData);
    }
}