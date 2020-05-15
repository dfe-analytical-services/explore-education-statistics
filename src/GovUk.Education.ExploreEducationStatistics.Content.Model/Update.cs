using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model
{
    public class Update
    {
        [Key] [Required] public Guid Id { get; set; }

        public Guid ReleaseId { get; set; }
        
        public Release Release { get; set; }

        [Required] public DateTime On { get; set; }

        [Required] public string Reason { get; set; }

        public Update Clone(CreateClonedContext ctx)
        {
            var copy = MemberwiseClone() as Update;
            copy.Id = Guid.NewGuid();
            ctx.OldToNewIdUpdateMappings.Add(this, copy);
            
            copy.Release = ctx.NewRelease;
            copy.ReleaseId = ctx.NewRelease.Id;
            return copy;
        }
    }
}