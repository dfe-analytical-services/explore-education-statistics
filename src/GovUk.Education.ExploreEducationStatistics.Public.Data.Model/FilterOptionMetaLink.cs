using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionMetaLink
{
    public required string PublicId { get; set; }

    public required int MetaId { get; set; }

    public FilterMeta Meta { get; set; } = null!;

    public required int OptionId { get; set; }

    public FilterOptionMeta Option { get; set; } = null!;

    internal class Config : IEntityTypeConfiguration<FilterOptionMetaLink>
    {
        public void Configure(EntityTypeBuilder<FilterOptionMetaLink> builder)
        {
            builder.Property(l => l.PublicId).HasMaxLength(10);

            builder.HasIndex(l => new { l.MetaId, l.PublicId }).IsUnique();
        }
    }
}
