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
            RegisterConversionsLevel(modelBuilder);
            RegisterConversionsSchoolType(modelBuilder);
            RegisterConversionsSchool(modelBuilder);
            RegisterConversionsAttributes(modelBuilder);
            RegisterConversionsCountry(modelBuilder);
            RegisterConversionsLocalAuthority(modelBuilder);
            RegisterConversionsRegion(modelBuilder);
            RegisterConversionsCharacteristic(modelBuilder);
        }

        private static void RegisterConversionsLevel(ModelBuilder modelBuilder)
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

        private static void RegisterConversionsSchoolType(ModelBuilder modelBuilder)
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

        private static void RegisterConversionsSchool(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.School)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<School>(v));
        }

        private static void RegisterConversionsAttributes(ModelBuilder modelBuilder)
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

        private static void RegisterConversionsCountry(ModelBuilder modelBuilder)
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

        private static void RegisterConversionsLocalAuthority(ModelBuilder modelBuilder)
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

        private static void RegisterConversionsRegion(ModelBuilder modelBuilder)
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

        private static void RegisterConversionsCharacteristic(ModelBuilder modelBuilder)
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
            
            modelBuilder.Entity<CharacteristicDataNational>()
                .Property(data => data.CharacteristicName)
                .HasComputedColumnSql("JSON_VALUE(Characteristic, '$.characteristic_1')");

            modelBuilder.Entity<CharacteristicDataNational>()
                .HasIndex(data => data.CharacteristicName);
            
            modelBuilder.Entity<CharacteristicDataLa>()
                .Property(data => data.CharacteristicName)
                .HasComputedColumnSql("JSON_VALUE(Characteristic, '$.characteristic_1')");

            modelBuilder.Entity<CharacteristicDataLa>()
                .HasIndex(data => data.CharacteristicName);
        }
    }
}