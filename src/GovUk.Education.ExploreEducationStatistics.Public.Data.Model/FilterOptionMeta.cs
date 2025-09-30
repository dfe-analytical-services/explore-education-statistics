using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class FilterOptionMeta
{
    public int Id { get; set; }

    public required string Label { get; set; }

    public List<FilterMeta> Metas { get; set; } = [];

    public List<FilterOptionMetaLink> MetaLinks { get; set; } = [];

    internal class Config : IEntityTypeConfiguration<FilterOptionMeta>
    {
        public void Configure(EntityTypeBuilder<FilterOptionMeta> builder)
        {
            builder.Property(m => m.Label).HasMaxLength(120);
        }
    }
}
