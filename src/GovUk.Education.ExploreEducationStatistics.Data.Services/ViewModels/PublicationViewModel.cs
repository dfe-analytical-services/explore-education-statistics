using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class PublicationViewModel
    {
        public Guid Id { get; set; }
        public IEnumerable<IdLabel> Highlights { get; set; }
        public IEnumerable<IdLabel> Subjects { get; set; }
    }
}