using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationOptionMetaLink
{
    public required string PublicId { get; set; }
    
    public required int MetaId { get; set; }

    public LocationMeta Meta { get; set; } = null!;

    public required int OptionId { get; set; }

    public LocationOptionMeta Option { get; set; } = null!;

    internal class Config : IEntityTypeConfiguration<LocationOptionMetaLink>
    {
        public void Configure(EntityTypeBuilder<LocationOptionMetaLink> builder)
        {
            builder.HasIndex(o => o.PublicId);
        }
    }
}
