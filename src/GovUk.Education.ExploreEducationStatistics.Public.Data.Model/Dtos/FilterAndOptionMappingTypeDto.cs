using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Dtos;

public record FilterAndOptionMappingTypeDto(MappingType FilterMappingType, MappingType OptionMappingType);

internal class Config : IEntityTypeConfiguration<FilterAndOptionMappingTypeDto>
{
    public void Configure(EntityTypeBuilder<FilterAndOptionMappingTypeDto> builder)
    {
        builder.Property(dto => dto.FilterMappingType)
            .IsRequired()
            .HasColumnType("text")
            .HasConversion(new EnumToStringConverter<MappingType>());

        builder.Property(dto => dto.OptionMappingType)
            .IsRequired()
            .HasColumnType("text")
            .HasConversion(new EnumToStringConverter<MappingType>());
    }
}
