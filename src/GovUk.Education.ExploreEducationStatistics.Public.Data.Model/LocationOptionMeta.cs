using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class LocationOptionMeta
{
    public int Id { get; set; }

    public required string Label { get; set; }

    protected abstract string Type { get; set; }

    protected string? Code { get; set; }

    protected string? OldCode { get; set; }

    protected string? Urn { get; set; }

    protected string? LaEstab { get; set; }

    protected string? Ukprn { get; set; }

    public List<LocationMeta> Metas { get; set; } = [];

    public List<LocationOptionMetaLink> MetaLinks { get; set; } = [];

    public LocationOptionMetaRow ToRow() => new()
    {
        Id = Id,
        Type = Type,
        Label = Label,
        Code = Code,
        OldCode = OldCode,
        Urn = Urn,
        LaEstab = LaEstab,
        Ukprn = Ukprn
    };

    internal class Config : IEntityTypeConfiguration<LocationOptionMeta>
    {
        public void Configure(EntityTypeBuilder<LocationOptionMeta> builder)
        {
            builder
                .HasDiscriminator<string>(o => o.Type)
                .HasValue<LocationCodedOptionMeta>(LocationCodedOptionMeta.TypeValue)
                .HasValue<LocationLocalAuthorityOptionMeta>(LocationLocalAuthorityOptionMeta.TypeValue)
                .HasValue<LocationProviderOptionMeta>(LocationProviderOptionMeta.TypeValue)
                .HasValue<LocationRscRegionOptionMeta>(LocationRscRegionOptionMeta.TypeValue)
                .HasValue<LocationSchoolOptionMeta>(LocationSchoolOptionMeta.TypeValue);

            builder.Property(m => m.Label)
                .HasMaxLength(120);

            builder.Property(o => o.Type)
                .HasMaxLength(10);

            builder.Property(o => o.Code)
                .HasMaxLength(30)
                .IsRequired(false);

            builder.Property(o => o.OldCode)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(o => o.Urn)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(o => o.LaEstab)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.Property(o => o.Ukprn)
                .HasMaxLength(20)
                .IsRequired(false);

            builder.HasIndex(o => o.Type);
            builder.HasIndex(o => o.Code);
            builder.HasIndex(o => o.OldCode);
            builder.HasIndex(o => o.Urn);
            builder.HasIndex(o => o.LaEstab);
            builder.HasIndex(o => o.Ukprn);

            // This index is primarily for providing a uniqueness constraint
            // preventing duplicates making it into the table by accident.
            // Unfortunately, it comes at the cost of essentially doubling the table's
            // size, but it's worth having for now (can remove it later if needed).
            builder.HasIndex(o => new
            {
                o.Type,
                o.Label,
                o.Code,
                o.OldCode,
                o.Urn,
                o.LaEstab,
                o.Ukprn
            }, $"IX_{nameof(PublicDataDbContext.LocationOptionMetas)}_All")
                .IsUnique()
                .AreNullsDistinct(false);
        }
    }
}

public class LocationCodedOptionMeta : LocationOptionMeta
{
    public const string TypeValue = "CODE";

    protected override string Type { get; set; } = TypeValue;

    public new required string Code
    {
        get => base.Code ?? string.Empty;
        set => base.Code = value;
    }
}

public class LocationLocalAuthorityOptionMeta : LocationOptionMeta
{
    public const string TypeValue = "LA";

    protected override string Type { get; set; } = TypeValue;

    public new required string Code
    {
        get => base.Code ?? string.Empty;
        set => base.Code = value;
    }

    public new required string OldCode
    {
        get => base.OldCode ?? string.Empty;
        set => base.OldCode = value;
    }
}

public class LocationProviderOptionMeta : LocationOptionMeta
{
    public const string TypeValue = "PROV";

    protected override string Type { get; set; } = TypeValue;

    public new required string Ukprn
    {
        get => base.Ukprn ?? string.Empty;
        set => base.Ukprn = value;
    }
}

public class LocationRscRegionOptionMeta : LocationOptionMeta
{
    public const string TypeValue = "RSC";

    protected override string Type { get; set; } = TypeValue;
}

public class LocationSchoolOptionMeta : LocationOptionMeta
{
    public const string TypeValue = "SCH";

    protected override string Type { get; set; } = TypeValue;

    public new required string Urn
    {
        get => base.Urn ?? string.Empty;
        set => base.Urn = value;
    }

    public new required string LaEstab
    {
        get => base.LaEstab ?? string.Empty;
        set => base.LaEstab = value;
    }
}
