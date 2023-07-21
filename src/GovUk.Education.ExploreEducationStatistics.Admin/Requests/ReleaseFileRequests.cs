#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record ReleaseDataFileUpdateRequest
{
    public string? Title { get; set; }

    public string? Summary { get; set; }
}

public record ReleaseAncillaryFileUploadRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required] public string Summary { get; set; } = string.Empty;

    [Required]
    public IFormFile File { get; set; } = null!;
}

public record ReleaseAncillaryFileUpdateRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Summary { get; set; } = string.Empty;

    public IFormFile? File { get; set; }
}
