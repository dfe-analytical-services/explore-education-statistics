#nullable enable
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EinPage
{
    public Guid Id { get; set; }

    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Slug { get; set; } = null;

    [MaxLength(2047)]
    public string Description { get; set; } = string.Empty;

    public int Order { get; set; }

    public Guid? LatestVersionId { get; set; }
    public EinPageVersion? LatestVersion { get; set; } = null!;

    public Guid? LatestPublishedVersionId { get; set; }
    public EinPageVersion? LatestPublishedVersion { get; set; }

    public List<EinPageVersion> PageVersions { get; set; } = new();

    internal class Config : IEntityTypeConfiguration<EinPage>
    {
        public void Configure(EntityTypeBuilder<EinPage> builder)
        {
            builder
                .HasOne(p => p.LatestVersion)
                .WithMany()
                .HasForeignKey(p => p.LatestVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(p => p.LatestPublishedVersion)
                .WithMany()
                .HasForeignKey(p => p.LatestPublishedVersionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
