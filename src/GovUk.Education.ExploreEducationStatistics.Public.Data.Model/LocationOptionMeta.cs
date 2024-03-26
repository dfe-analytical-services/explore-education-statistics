using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public abstract class LocationOptionMeta
{
    public int Id { get; set; }

    public required string Label { get; set; }

    private string Type { get; set; }

    protected string? Code { get; set; }

    protected string? OldCode { get; set; }

    protected string? Urn { get; set; }

    protected string? LaEstab { get; set; }

    protected string? Ukprn { get; set; }

    public List<LocationMeta> Metas { get; set; } = [];

    public List<LocationOptionMetaLink> MetaLinks { get; set; } = [];

    public abstract LocationOptionMetaRow ToRow();

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

            builder
                .Property(o => o.Type)
                .HasMaxLength(10);

            builder.Property(o => o.Code)
                .IsRequired(false);

            builder.Property(o => o.OldCode)
                .IsRequired(false);

            builder.Property(o => o.Urn)
                .IsRequired(false);

            builder.Property(o => o.LaEstab)
                .IsRequired(false);

            builder.Property(o => o.Ukprn)
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
            builder.HasIndex(o => new {
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

     public new required string Code
     {
         get => base.Code ?? string.Empty;
         set => base.Code = value;
     }

     public override LocationOptionMetaRow ToRow() => new()
     {
         Type = TypeValue,
         Label = Label,
         Code = Code
     };
}

public class LocationLocalAuthorityOptionMeta : LocationOptionMeta
{
     public const string TypeValue = "LA";

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

     public override LocationOptionMetaRow ToRow() => new()
     {
         Type = TypeValue,
         Label = Label,
         Code = Code,
         OldCode = OldCode
     };
}

public class LocationProviderOptionMeta : LocationOptionMeta
{
     public const string TypeValue = "PROV";

     public new required string Ukprn
     {
         get => base.Ukprn ?? string.Empty;
         set => base.Ukprn = value;
     }

     public override LocationOptionMetaRow ToRow() => new()
     {
         Type = TypeValue,
         Label = Label,
         Ukprn = Ukprn,
     };
}

public class LocationRscRegionOptionMeta : LocationOptionMeta
{
    public const string TypeValue = "RSC";

    public override LocationOptionMetaRow ToRow() => new()
    {
        Type = TypeValue,
        Label = Label
    };
}

public class LocationSchoolOptionMeta : LocationOptionMeta
{
     public const string TypeValue = "SCH";

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

     public override LocationOptionMetaRow ToRow() => new()
     {
         Type = TypeValue,
         Label = Label,
         Urn = Urn,
         LaEstab = LaEstab
     };
}
