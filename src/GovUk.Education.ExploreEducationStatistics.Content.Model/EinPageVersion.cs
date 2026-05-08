#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class EinPageVersion
{
    public Guid Id { get; set; }

    public int Version { get; set; }

    public DateTimeOffset? Published { get; set; }

    public DateTimeOffset Created { get; set; }

    public Guid CreatedById { get; set; }

    public DateTimeOffset? Updated { get; set; }

    public Guid? UpdatedById { get; set; }

    public Guid EinPageId { get; set; }
    public EinPage EinPage { get; set; } = null!;

    public List<EinContentSection> Content { get; set; } = new();

    internal class Config : IEntityTypeConfiguration<EinPageVersion>
    {
        public void Configure(EntityTypeBuilder<EinPageVersion> builder)
        {
            builder
                .HasOne(pageVersion => pageVersion.EinPage)
                .WithMany(page => page.PageVersions)
                .HasForeignKey(pageVersion => pageVersion.EinPageId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
