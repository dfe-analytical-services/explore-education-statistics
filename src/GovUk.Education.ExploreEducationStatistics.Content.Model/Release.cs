using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Release
    {
        [Key] [Required] public Guid Id { get; set; }

        [Required] public string Title { get; set; }

        public string ReleaseName { get; set; }

        public DateTime? Published { get; set; }

        public string Slug { get; set; }

        public string Summary { get; set; }

        public Guid PublicationId { get; set; }

        public Publication Publication { get; set; }
        
        public List<Update> Updates { get; set; }

        public List<ContentSection> Content { get; set; }
        
        public DataBlock KeyStatistics { get; set; }
    }
}