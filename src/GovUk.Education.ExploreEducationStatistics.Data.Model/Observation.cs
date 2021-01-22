using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model
{
    [Table("ObservationRow")]
    public class Observation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public Guid ObservationId { get; set; }
        public Subject Subject { get; set; }
        public Guid SubjectId { get; set; }
        public GeographicLevel GeographicLevel { get; set; }
        public Location Location { get; set; }
        public Guid LocationId { get; set; }
        public int Year { get; set; }
        public TimeIdentifier TimeIdentifier { get; set; }
        public Dictionary<Guid, string> Measures { get; set; }
        public long CsvRow { get; set; }
        public string FilterItemIds { get; set; }

        public List<string> GetFilterItemIdList()
        {
            if (FilterItemIds.IsNullOrEmpty())
            {
                return new List<string>();
            }

            return FilterItemIds
                .Split(' ', StringSplitOptions.None)
                .Select(s => s.ToLower())
                .OrderBy(l => l)
                .ToList(); 
        }
    }
}