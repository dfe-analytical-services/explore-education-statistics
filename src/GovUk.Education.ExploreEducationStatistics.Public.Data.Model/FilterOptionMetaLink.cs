using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionMetaLink
{
    public int PublicId { get; set; }

    public required int MetaId { get; set; }

    public FilterMeta Meta { get; set; } = null!;

    public required int OptionId { get; set; }

    public FilterOptionMeta Option { get; set; } = null!;

    internal class Config : IEntityTypeConfiguration<FilterOptionMetaLink>
    {
        public void Configure(EntityTypeBuilder<FilterOptionMetaLink> builder)
        {
            builder.Property(l => l.PublicId)
                .ValueGeneratedOnAdd();

            builder.HasIndex(l => l.PublicId);

            builder.HasIndex(l => new { l.MetaId, l.PublicId })
                .IsUnique();
        }
    }
}
