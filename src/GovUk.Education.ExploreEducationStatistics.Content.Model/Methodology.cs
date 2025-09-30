﻿#nullable enable
using System.ComponentModel.DataAnnotations;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Methodology
{
    public Guid Id { get; set; }

    public List<PublicationMethodology> Publications { get; set; } = new();

    [Required]
    public string OwningPublicationTitle { get; set; } = null!;

    [Required]
    public string OwningPublicationSlug { get; set; } = null!;

    public List<MethodologyVersion> Versions { get; set; } = new();

    public Guid? LatestPublishedVersionId { get; set; }

    public MethodologyVersion? LatestPublishedVersion { get; set; }

    public PublicationMethodology OwningPublication()
    {
        if (Publications.IsNullOrEmpty())
        {
            throw new ArgumentException(
                "Methodology must be hydrated with Publications to get the owning publication"
            );
        }

        return Publications.Single(pm => pm.Owner);
    }

    public MethodologyVersion LatestVersion()
    {
        if (Versions.IsNullOrEmpty())
        {
            throw new ArgumentException(
                "Methodology must be hydrated with Versions to get the latest version"
            );
        }

        return Versions.Single(mv => IsLatestVersionOfMethodology(mv.Id));
    }

    public bool IsLatestVersionOfMethodology(Guid methodologyVersionId)
    {
        if (Versions == null)
        {
            throw new ArgumentException(
                "Methodology must be hydrated with Versions to test the latest version"
            );
        }

        return Versions.Exists(mv => mv.Id == methodologyVersionId)
            && Versions.All(mv => mv.PreviousVersionId != methodologyVersionId);
    }
}
