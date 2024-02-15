using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class LocationOptionMeta
{
    public int Id { get; set; }

    public required string Label { get; set; }

    public List<LocationMeta> Metas { get; set; } = [];

    public List<LocationOptionMetaLink> MetaLinks { get; set; } = [];

    internal class Config : IEntityTypeConfiguration<LocationOptionMeta>
    {
        public void Configure(EntityTypeBuilder<LocationOptionMeta> builder)
        {
            builder
                .HasDiscriminator<string>("Type")
                .HasValue<LocationCodedOptionMeta>("Coded")
                .HasValue<LocationLocalAuthorityOptionMeta>("LA")
                .HasValue<LocationProviderOptionMeta>("PROV")
                .HasValue<LocationRscRegionOptionMeta>("RSC")
                .HasValue<LocationSchoolOptionMeta>("SCH");

            builder
                .Property("Type")
                .HasMaxLength(5);

            builder.HasIndex("Type");
        }
    }
}

public class LocationCodedOptionMeta : LocationOptionMeta
{
    public required string Code { get; set; }

    internal class Config : IEntityTypeConfiguration<LocationCodedOptionMeta>
    {
        public void Configure(EntityTypeBuilder<LocationCodedOptionMeta> builder)
        {
            builder.HasIndex(o => o.Code);
        }
    }
}

public class LocationLocalAuthorityOptionMeta : LocationCodedOptionMeta
{
    public required string OldCode { get; set; }

    internal class Config : IEntityTypeConfiguration<LocationLocalAuthorityOptionMeta>
    {
        public void Configure(EntityTypeBuilder<LocationLocalAuthorityOptionMeta> builder)
        {
            builder.HasIndex(o => o.OldCode);
        }
    }
}

public class LocationProviderOptionMeta : LocationOptionMeta
{
    public required string Ukprn { get; set; }

    internal class Config : IEntityTypeConfiguration<LocationProviderOptionMeta>
    {
        public void Configure(EntityTypeBuilder<LocationProviderOptionMeta> builder)
        {
            builder.HasIndex(o => o.Ukprn);
        }
    }
}

public class LocationRscRegionOptionMeta : LocationOptionMeta;

public class LocationSchoolOptionMeta : LocationOptionMeta
{
    public required string Urn { get; set; }

    public required string LaEstab { get; set; }

    internal class Config : IEntityTypeConfiguration<LocationSchoolOptionMeta>
    {
        public void Configure(EntityTypeBuilder<LocationSchoolOptionMeta> builder)
        {
            builder.HasIndex(o => o.Urn);
            builder.HasIndex(o => o.LaEstab);
        }
    }
}
