#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels
{
    public class ReleaseAncillaryFileUploadViewModel
    {
        [Required]
        public string Title { get; set; } = null!;

        [Required]
        public string Summary { get; set; } = null!;

        [Required]
        public IFormFile File { get; set; } = null!;
    }

    public class ReleaseFileUpdateViewModel
    {
        public string? Title { get; set; }

        public string? Summary { get; set; }
    }
}