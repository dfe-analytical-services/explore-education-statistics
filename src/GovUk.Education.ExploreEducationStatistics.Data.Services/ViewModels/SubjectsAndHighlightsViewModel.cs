using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels
{
    public class SubjectsAndHighlightsViewModel
    {
        public List<TableHighlightViewModel> Highlights { get; set; }

        public List<SubjectViewModel> Subjects { get; set; }
    }
}