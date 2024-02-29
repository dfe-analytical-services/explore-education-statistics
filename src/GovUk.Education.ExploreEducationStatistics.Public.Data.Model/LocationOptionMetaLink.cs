using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationOptionMetaLink
{
    public int PublicId { get; set; }

    public required int MetaId { get; set; }

    public LocationMeta Meta { get; set; } = null!;

    public required int OptionId { get; set; }

    public LocationOptionMeta Option { get; set; } = null!;

    internal class Config : IEntityTypeConfiguration<LocationOptionMetaLink>
    {
        public void Configure(EntityTypeBuilder<LocationOptionMetaLink> builder)
        {
            builder.Property(l => l.PublicId)
                .ValueGeneratedOnAdd();

            builder.HasIndex(l => l.PublicId);

            builder.HasIndex(l => new { l.MetaId, l.PublicId })
                .IsUnique();
        }
    }
}
