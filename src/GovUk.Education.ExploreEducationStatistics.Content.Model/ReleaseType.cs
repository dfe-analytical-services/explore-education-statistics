using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class ReleaseType
    {
        public Guid Id { get; set; }

        [Required] public string Title { get; set; }
    }
}