#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Models
{
    // TODO EES-3755 Remove after Permalink snapshot work is complete
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

        public TableBuilderResultViewModel AsTableBuilderResultViewModel()
        {
            return new TableBuilderResultViewModel
            {
                SubjectMeta = SubjectMeta.AsSubjectResultMetaViewModel(),
                Results = Results
            };
        }
    }
}
