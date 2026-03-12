#nullable enable
using Generator.Equals;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

[Equatable]
public partial class Organisation : ICreatedUpdatedTimestamps<DateTimeOffset, DateTimeOffset?>
{
    public Guid Id { get; set; }

    public required string Title { get; set; }

    public required string Url { get; set; }

    public DateTimeOffset Created { get; set; }

    public DateTimeOffset? Updated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the organisation is a government department or organisation that should use the Government Information Service (GIS) logo format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// GIS logos use a common format which should be constructed from markup, whereas non-GIS logos may use an image file.
    /// </para>
    /// </remarks>
    public bool UseGISLogo { get; set; }

    /// <summary>
    /// GIS logos contain a horizontal line which may vary in colour depending on the department or organisation. This property identifies the colour of the line as a hex code (e.g. the DfE uses #0b0c0c).
    /// </summary>
    public string? GISLogoHexCode { get; set; }

    //public string? LogoBlobStorageUri { get; set; }

    //public string? LogoBase64EncodedString { get; set; }

    //public Uri? LogoUri { get; set; }

    internal class Config : IEntityTypeConfiguration<Organisation>
    {
        public void Configure(EntityTypeBuilder<Organisation> builder)
        {
            builder.Property(o => o.Title).HasMaxLength(100);

            builder.Property(o => o.Url).HasMaxLength(1024);

            builder.HasIndex(o => o.Title).IsUnique();
        }
    }
}
