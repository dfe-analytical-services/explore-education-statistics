using GovUk.Education.ExploreEducationStatistics.Common.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class LocationMeta : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public required Guid DataSetVersionId { get; set; }

    public DataSetVersion DataSetVersion { get; set; } = null!;

    public abstract GeographicLevel Level { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    internal class Config : IEntityTypeConfiguration<LocationMeta>
    {
        public void Configure(EntityTypeBuilder<LocationMeta> builder)
        {
            builder
                .HasDiscriminator<string>("Type")
                .HasValue<LocationDefaultMeta>("Default")
                .HasValue<LocationLocalAuthorityMeta>("LocalAuthority")
                .HasValue<LocationProviderMeta>("Provider")
                .HasValue<LocationRscRegionMeta>("RscRegion")
                .HasValue<LocationSchoolMeta>("School");

            builder
                .Property("Type")
                .HasMaxLength(50);

            builder
                .Property(m => m.Level)
                .HasMaxLength(5)
                .HasConversion(new EnumToEnumValueConverter<GeographicLevel>());

            builder.HasIndex("Type", nameof(DataSetVersionId));

            builder
                .HasIndex(m => new {m.Level, m.DataSetVersionId})
                .IsUnique();
        }
    }
}

public interface ILocationMetaWithOptions<TOptionMeta> where TOptionMeta : LocationOptionMetaBase
{
    public List<TOptionMeta> Options { get; set; }
}

public class LocationDefaultMeta : LocationMeta, ILocationMetaWithOptions<LocationOptionMeta>
{
    public override required GeographicLevel Level { get; set; }

    public List<LocationOptionMeta> Options { get; set; } = [];

    internal new class Config : IEntityTypeConfiguration<LocationDefaultMeta>
    {
        public void Configure(EntityTypeBuilder<LocationDefaultMeta> builder)
        {
            builder.OwnsMany(m => m.Options, m =>
            {
                m.ToJson("Options");
            });
        }
    }
}

public class LocationLocalAuthorityMeta : LocationMeta, ILocationMetaWithOptions<LocationLocalAuthorityOptionMeta>
{
    public override GeographicLevel Level
    {
        get => GeographicLevel.LocalAuthority;
        set { }
    }

    public List<LocationLocalAuthorityOptionMeta> Options { get; set; } = [];

    internal new class Config : IEntityTypeConfiguration<LocationLocalAuthorityMeta>
    {
        public void Configure(EntityTypeBuilder<LocationLocalAuthorityMeta> builder)
        {
            builder.OwnsMany(m => m.Options, m =>
            {
                m.ToJson("LocalAuthorityOptions");
            });
        }
    }
}

public class LocationProviderMeta : LocationMeta, ILocationMetaWithOptions<LocationProviderOptionMeta>
{
    public override GeographicLevel Level
    {
        get => GeographicLevel.Provider;
        set { }
    }

    public List<LocationProviderOptionMeta> Options { get; set; } = [];

    internal new class Config : IEntityTypeConfiguration<LocationProviderMeta>
    {
        public void Configure(EntityTypeBuilder<LocationProviderMeta> builder)
        {
            builder.OwnsMany(m => m.Options, m =>
            {
                m.ToJson("ProviderOptions");
            });
        }
    }
}

public class LocationRscRegionMeta : LocationMeta, ILocationMetaWithOptions<LocationRscRegionOptionMeta>
{
    public override GeographicLevel Level
    {
        get => GeographicLevel.RscRegion;
        set { }
    }
    public List<LocationRscRegionOptionMeta> Options { get; set; } = [];

    internal new class Config : IEntityTypeConfiguration<LocationRscRegionMeta>
    {
        public void Configure(EntityTypeBuilder<LocationRscRegionMeta> builder)
        {
            builder.OwnsMany(m => m.Options, m =>
            {
                m.ToJson("RscRegionOptions");
            });
        }
    }
}

public class LocationSchoolMeta : LocationMeta, ILocationMetaWithOptions<LocationSchoolOptionMeta>
{
    public override GeographicLevel Level
    {
        get => GeographicLevel.School;
        set { }
    }

    public List<LocationSchoolOptionMeta> Options { get; set; } = [];

    internal new class Config : IEntityTypeConfiguration<LocationSchoolMeta>
    {
        public void Configure(EntityTypeBuilder<LocationSchoolMeta> builder)
        {
            builder.OwnsMany(m => m.Options, m =>
            {
                m.ToJson("SchoolOptions");
            });
        }
    }
}
