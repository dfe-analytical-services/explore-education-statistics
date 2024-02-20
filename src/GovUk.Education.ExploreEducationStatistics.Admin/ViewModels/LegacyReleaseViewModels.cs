using System;
using System.ComponentModel.DataAnnotations;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public record LegacyReleaseViewModel
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        public int Order { get; set; }
    }

    public record CombinedReleaseViewModel
    {
        public Guid Id { get; set; }

        public string Description { get; set; }

        public bool IsLegacy { get; set; }

        public bool IsDraft { get; set; }

        public bool IsAmendment { get; set; }

        public string Url { get; set; }

        public int Order { get; set; }
    }

    public class LegacyReleaseCreateViewModel
    {
        [Required]
        public string Description { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }

        [Required]
        public Guid PublicationId { get; set; }
    }

    public class LegacyReleaseUpdateViewModel
    {
        [Required]
        public string Description { get; set; }

        [Required]
        [Url]
        public string Url { get; set; }

        [Required]
        public Guid PublicationId { get; set; }
    }

    public class CombinedReleaseUpdateOrderViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public int Order { get; set; }

        public bool IsLegacy { get; set; }

        public bool IsAmendment { get; set; }
    }
}
