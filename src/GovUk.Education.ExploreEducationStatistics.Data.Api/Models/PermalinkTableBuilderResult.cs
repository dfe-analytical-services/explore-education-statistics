#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    public class PermalinkTableBuilderResult
    {
        public PermalinkResultSubjectMeta SubjectMeta { get; init; }

        public List<ObservationViewModel> Results { get; init; } = new();

        public PermalinkTableBuilderResult()
        {
        }

        public PermalinkTableBuilderResult(TableBuilderResultViewModel tableBuilderResult)
        {
            SubjectMeta = new PermalinkResultSubjectMeta(tableBuilderResult.SubjectMeta);
            Results = tableBuilderResult.Results.ToList();
        }
    }
}
