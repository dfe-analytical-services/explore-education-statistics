using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Meta;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AttributeMeta> AttributeMeta { get; set; }
        public DbSet<CharacteristicMeta> CharacteristicMeta { get; set; }
        public DbSet<GeographicData> GeographicData { get; set; }
        public DbSet<CharacteristicDataNational> CharacteristicDataNational { get; set; }
        public DbSet<CharacteristicDataLa> CharacteristicDataLa { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            registerConversionsLevel(modelBuilder);
            registerConversionsSchoolType(modelBuilder);
            registerConversionsSchool(modelBuilder);
            registerConversionsAttributes(modelBuilder);
            registerConversionsCountry(modelBuilder);
            registerConversionsLocalAuthority(modelBuilder);
            registerConversionsRegion(modelBuilder);
            registerConversionsCharacteristic(modelBuilder);
        }

        private void registerConversionsLevel(ModelBuilder modelBuilder)
        {
            var levelConverter = new EnumToStringConverter<Level>();

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Level)
                .HasConversion(levelConverter);

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Level)
                .HasConversion(levelConverter);

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Level)
                .HasConversion(levelConverter);
        }

        private void registerConversionsSchoolType(ModelBuilder modelBuilder)
        {
            var schoolTypeConverter = new EnumToStringConverter<SchoolType>();

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.SchoolType)
                .HasConversion(schoolTypeConverter);

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.SchoolType)
                .HasConversion(schoolTypeConverter);

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.SchoolType)
                .HasConversion(schoolTypeConverter);
        }

        private void registerConversionsSchool(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.School)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<School>(v));
        }

        private void registerConversionsAttributes(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Attributes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Attributes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Attributes)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
        }

        private void registerConversionsCountry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Country)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Country>(v));

            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Country)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Country>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Country)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Country>(v));
        }

        private void registerConversionsLocalAuthority(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.LocalAuthority)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<LocalAuthority>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.LocalAuthority)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<LocalAuthority>(v));
        }

        private void registerConversionsRegion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Region)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Region>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Region)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Region>(v));
        }

        private void registerConversionsCharacteristic(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.Characteristic)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Characteristic>(v));

            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.Characteristic)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Characteristic>(v));
        }
    }
}