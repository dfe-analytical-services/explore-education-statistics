using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels
{
    public class ResultViewModel
    {
        public Guid? PublicationId { get; set; }
        public Guid? ReleaseId { get; set; }
        public long? SubjectId { get; set; }
        public DateTime? ReleaseDate { get; set; }

        public IEnumerable<ObservationViewModel> Result { get; set; }

        public ResultViewModel()
        {
            Result = new List<ObservationViewModel>();
        }
    }
}