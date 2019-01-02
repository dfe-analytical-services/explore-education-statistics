using System;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Models;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Swagger;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Theme> Themes { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Release> Releases { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Theme>().HasData(
                new Theme() { Id = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Title = "Schools", Slug = "schools" },
                new Theme() { Id = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), Title = "Social Care", Slug = "schools" },
                new Theme() { Id = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), Title = "16+", Slug = "16+" }
                );
            
            modelBuilder.Entity<Topic>().HasData(
                new Topic() { Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Absence and Exclusions", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "absence-and-exclusions"},
                new Topic() { Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "School and Pupil Numbers", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "school-and-pupil-numbers"},
                new Topic() { Id = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"), Title = "Capacity Admissions", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "capacity-admissions"},
                new Topic() { Id = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), Title = "Results", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "results"},
                new Topic() { Id = new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"), Title = "School Finance", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "school-finance"},
                new Topic() { Id = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"), Title = "Teacher Numbers", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "teacher-numbers"},

                new Topic() { Id = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"), Title = "Number of Children", ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), Slug = "number-of-children" },
                new Topic() { Id = new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"), Title = "Vulnerable Children", ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), Slug = "vulnerable-children" },
                
                new Topic() { Id = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), Title = "Further Education", ThemeId = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), Slug = "further-education" },
                new Topic() { Id = new Guid("4c658598-450b-4493-b972-8812acd154a7"), Title = "Higher Education", ThemeId = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), Slug = "higher-education" }

            );

            modelBuilder.Entity<Publication>().HasData(
                new Publication() { Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Title = "Pupil absence in schools in England", TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Slug = "pupil-absence-in-schools-in-england" },
                new Publication() { Id = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), Title = "Permanent and fixed period exclusions", TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Slug = "permanent-and-fixed-period-exclusions" }

            );

            modelBuilder.Entity<Release>().HasData(
                new Release() { Id = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), Title = "2016-17", PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Published = new DateTime(2018,3,22)}
            );
        }
    }
}
