using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Meta;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.SetCommandTimeout(int.MaxValue);
        }

        public DbSet<IndicatorMeta> IndicatorMeta { get; set; }
        public DbSet<CharacteristicMeta> CharacteristicMeta { get; set; }
        public DbSet<Release> Release { get; set; }
        public DbSet<Subject> Subject { get; set; }
        public DbSet<LevelComposite> Level { get; set; }
        public DbSet<GeographicData> GeographicData { get; set; }
        public DbSet<CharacteristicData> CharacteristicData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureUnit(modelBuilder);
            ConfigureIndicatorMeta(modelBuilder);
            ConfigureCharacteristicMeta(modelBuilder);
            ConfigurePublication(modelBuilder);
            ConfigureTimePeriod(modelBuilder);
            ConfigureLevel(modelBuilder);
            ConfigureSchoolType(modelBuilder);
            ConfigureSchool(modelBuilder);
            ConfigureCharacteristic(modelBuilder);
            ConfigureIndicators(modelBuilder);
            ConfigureCountry(modelBuilder);
            ConfigureLocalAuthority(modelBuilder);
            ConfigureRegion(modelBuilder);
        }

        private static void ConfigureUnit(ModelBuilder modelBuilder)
        {
            var unitConverter = new EnumToStringConverter<Unit>();

            modelBuilder.Entity<IndicatorMeta>()
                .Property(data => data.Unit)
                .HasConversion(unitConverter);
        }

        private static void ConfigureIndicatorMeta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IndicatorMeta>()
                .HasOne(indicatorMeta => indicatorMeta.Subject)
                .WithMany(subject => subject.Indicators);
        }

        private static void ConfigureCharacteristicMeta(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacteristicMeta>()
                .HasOne(characteristicMeta => characteristicMeta.Subject)
                .WithMany(subject => subject.Characteristics);
        }

        private static void ConfigurePublication(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Release>()
                .HasIndex(data => data.PublicationId);
        }

        private static void ConfigureTimePeriod(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.TimePeriod);

            modelBuilder.Entity<CharacteristicData>()
                .HasIndex(data => data.TimePeriod);
        }

        private static void ConfigureLevel(ModelBuilder modelBuilder)
        {
            var levelConverter = new EnumToStringConverter<Level>();

            modelBuilder.Entity<LevelComposite>()
                .Property(data => data.Level)
                .HasConversion(levelConverter);

            modelBuilder.Entity<LevelComposite>()
                .HasIndex(data => data.Level);

            modelBuilder.Entity<GeographicData>()
                .HasOne(data => data.Level)
                .WithMany();

            modelBuilder.Entity<CharacteristicData>()
                .HasOne(data => data.Level)
                .WithMany();
        }

        private static void ConfigureSchoolType(ModelBuilder modelBuilder)
        {
            var schoolTypeConverter = new EnumToStringConverter<SchoolType>();

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.SchoolType)
                .HasConversion(schoolTypeConverter);

            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.SchoolType);

            modelBuilder.Entity<CharacteristicData>()
                .Property(data => data.SchoolType)
                .HasConversion(schoolTypeConverter);

            modelBuilder.Entity<CharacteristicData>()
                .HasIndex(data => data.SchoolType);
        }
        
        private static void ConfigureSchool(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.School)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<School>(v));

            modelBuilder.Entity<GeographicData>()
                .Property(data => data.SchoolLaEstab)
                .HasComputedColumnSql("JSON_VALUE(School, '$.laestab')");

            modelBuilder.Entity<GeographicData>()
                .HasIndex(data => data.SchoolLaEstab);
        }

        private static void ConfigureIndicators(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeographicData>()
                .Property(data => data.Indicators)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));

            modelBuilder.Entity<CharacteristicData>()
                .Property(data => data.Indicators)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Dictionary<string, string>>(v));
        }

        private static void ConfigureCountry(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LevelComposite>()
                .OwnsOne(level => level.Country,
                    builder => builder.HasIndex(country => country.Code));
        }

        private static void ConfigureLocalAuthority(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LevelComposite>()
                .OwnsOne(level => level.LocalAuthority,
                    builder => builder.HasIndex(localAuthority => localAuthority.Code));
        }

        private static void ConfigureRegion(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LevelComposite>()
                .OwnsOne(level => level.Region,
                    builder => builder.HasIndex(region => region.Code));
        }

        private static void ConfigureCharacteristic(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CharacteristicData>()
                .Property(data => data.Characteristic)
                .HasConversion(
                    v => JsonConvert.SerializeObject(v),
                    v => JsonConvert.DeserializeObject<Characteristic>(v));

            modelBuilder.Entity<CharacteristicData>()
                .Property(data => data.CharacteristicName)
                .HasComputedColumnSql("JSON_VALUE(Characteristic, '$.characteristic_breakdown')");

            modelBuilder.Entity<CharacteristicData>()
                .HasIndex(data => data.CharacteristicName);
        }
    }
}