using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    public class Footnote
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public ICollection<IndicatorFootnote> Indicators { get; set; }
        public ICollection<FilterFootnote> Filters { get; set; }
        public ICollection<FilterGroupFootnote> FilterGroups { get; set; }
        public ICollection<FilterItemFootnote> FilterItems { get; set; }
        public ICollection<SubjectFootnote> Subjects { get; set; }
        
        public Release Release { get; set; }
        
        public Guid ReleaseId { get; set; }
    }
}