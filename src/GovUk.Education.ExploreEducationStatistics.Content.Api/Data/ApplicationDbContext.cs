using System;
using System.Collections.Generic;
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
        public DbSet<Link> Links { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Theme>().HasData(
                new Theme() { Id = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Title = "Schools", Summary = "Lorem ipsum dolor sit amet.", Slug = "schools" },
                new Theme() { Id = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), Title = "Social Care", Summary = "Lorem ipsum dolor sit amet.", Slug = "schools" },
                new Theme() { Id = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), Title = "16+", Summary = "Lorem ipsum dolor sit amet.", Slug = "16+" }
                );

            modelBuilder.Entity<Topic>().HasData(
                new Topic() { Id = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Title = "Absence and exclusions", Summary = "Pupil absence, permanent and fixed period exlusions", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "absence-and-exclusions"},
                new Topic() { Id = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Title = "School & pupil numbers", Summary = "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "school-and-pupil-numbers"},
                new Topic() { Id = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"), Title = "Capacity and admissions", Summary = "School capacity, Admission appeals", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "capacity-admissions"},
                new Topic() { Id = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), Title = "Results", Summary = "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "results"},
                new Topic() { Id = new Guid("66ff5e67-36cf-4210-9ad2-632baeb4eca7"), Title = "School finance", Summary = "Schools, pupils and their characteristics, SEN and EHC plans, SEN in England", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "school-finance"},
                new Topic() { Id = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"), Title = "Teacher Numbers", Summary = "School capacity, Admission appeals", ThemeId = new Guid("cc8e02fd-5599-41aa-940d-26bca68eab53"), Slug = "teacher-numbers"},

                new Topic() { Id = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"), Title = "Number of Children", Summary = "Lorem ipsum dolor sit amet.", ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), Slug = "number-of-children" },
                new Topic() { Id = new Guid("3bef5b2b-76a1-4be1-83b1-a3269245c610"), Title = "Vulnerable Children", Summary = "Lorem ipsum dolor sit amet.", ThemeId = new Guid("6412a76c-cf15-424f-8ebc-3a530132b1b3"), Slug = "vulnerable-children" },
                
                new Topic() { Id = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), Title = "Further Education", Summary = "Lorem ipsum dolor sit amet.", ThemeId = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), Slug = "further-education" },
                new Topic() { Id = new Guid("4c658598-450b-4493-b972-8812acd154a7"), Title = "Higher Education", Summary = "Lorem ipsum dolor sit amet.", ThemeId = new Guid("bc08839f-2970-4f34-af2d-29608a48082f"), Slug = "higher-education" }
            );

            modelBuilder.Entity<Publication>().HasData(
                // Absence and exclusions
                new Publication()
                {
                    Id = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), 
                    Title = "Pupil absence in schools in England", 
                    Summary = "Overall absence, Authorised absence, Unauthorised absence, Persistence absence", 
                    TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), 
                    Slug = "pupil-absence-in-schools-in-england",
                    NextUpdate = new DateTime(2018, 3, 22),
                },
                new Publication() { Id = new Guid("bf2b4284-6b84-46b0-aaaa-a2e0a23be2a9"), Title = "Permanent and fixed period exclusions", Summary = "Permanent exclusions, fixed period exclusions", TopicId = new Guid("1003fa5c-b60a-4036-a178-e3a69a81b852"), Slug = "permanent-and-fixed-period-exclusions" },
                
                // School and pupil numbers
                new Publication() { Id = new Guid("a91d9e05-be82-474c-85ae-4913158406d0"), Title = "Schools, pupils and their characteristics", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("22c52d89-88c0-44b5-96c4-042f1bde6ddd"), Slug = "schools-pupils-and-their-characteristics" },

                // Capacity Admissions
                new Publication() { Id = new Guid("d04142bd-f448-456b-97bc-03863143836b"), Title = "School capacity", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"), Slug = "school-capacity" },
                new Publication() { Id = new Guid("a20ea465-d2d0-4fc1-96ee-6b2ca4e0520e"), Title = "Admission appeals in England", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("734820b7-f80e-45c3-bb92-960edcc6faa5"), Slug = "admission-appeals-in-England" },
                
                // Results
                new Publication() { Id = new Guid("526dea0e-abf3-476e-9ca4-9dbd9b101bc8"), Title = "Early years foundation stage profile results", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), Slug = "early-years-foundation-stage-profile-results" },
                new Publication() { Id = new Guid("9674ac24-649a-400c-8a2c-871793d9cd7a"), Title = "Phonics screening check and KS1 assessments", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), Slug = "phonics-screening-check-and-ks1-assessments" },
                new Publication() { Id = new Guid("a4b22113-47d3-48fc-b2da-5336c801a31f"), Title = "KS2 statistics", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), Slug = "ks2-statistics" },
                new Publication() { Id = new Guid("bfdcaae1-ce6b-4f63-9b2b-0a1f3942887f"), Title = "KS4 statistics", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("17b2e32c-ed2f-4896-852b-513cdf466769"), Slug = "ks4-statistics" },
                
                // Teacher Numbers
                new Publication() { Id = new Guid("8b2c1269-3495-4f89-83eb-524fc0b6effc"), Title = "School workforce", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"), Slug = "school-workforce" },
                new Publication() { Id = new Guid("fe94b33d-0419-4fac-bf73-28299d5e4247"), Title = "Initial teacher training performance profiles", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("d5288137-e703-43a1-b634-d50fc9785cb9"), Slug = "initial-teacher-training-performance-profiles" },
                
                // Number of Children
                new Publication() { Id = new Guid("bd781dc5-cfc7-4543-b8d7-a3a7b3606b3d"), Title = "Children in need", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"), Slug = "children-in-need" },
                new Publication() { Id = new Guid("143c672b-18d7-478b-a6e7-b843c9b3fd42"), Title = "Looked after children", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("0b920c62-ff67-4cf1-89ec-0c74a364e6b4"), Slug = "looked-after-children" },
                
                // Further Education 
                new Publication() { Id = new Guid("70902b3c-0bb4-457d-b40a-2a959cdc7d00"), Title = "16 to 18 school performance", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), Slug = "16-to-18-school-performance" },
                new Publication() { Id = new Guid("d0e56978-c944-4b12-9156-bfe50c94c2a0"), Title = "Destination of leavers", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), Slug = "destination-of-leavers" },
                new Publication() { Id = new Guid("ad81ebdd-2bbc-47e8-a32c-f396d6e2bb72"), Title = "Further education and skills", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), Slug = "further-education-and-skills" },
                new Publication() { Id = new Guid("201cb72d-ef35-4680-ade7-b09a8dca9cc1"), Title = "Apprenticeship and levy statistics", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), Slug = "apprenticeship-and-levy-statistics" },
                new Publication() { Id = new Guid("7bd128a3-ae7f-4e1b-984e-d1b795c61630"), Title = "Apprenticeships and traineeships", Summary = "Lorem ipsum dolor sit amet.", TopicId = new Guid("6a0f4dce-ae62-4429-834e-dd67cee32860"), Slug = "apprenticeships-and-traineeships" }
            );

            modelBuilder.Entity<Release>().HasData(
                new Release()
                {
                    Id = new Guid("4fa4fe8e-9a15-46bb-823f-49bf8e0cdec5"), 
                    Title = "Pupil absence data and statistics for schools in England", 
                    ReleaseName = "2016-17",
                    PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), 
                    Published = new DateTime(2018,3,22),
                    Summary = "<p class=\"govuk-body\"> This service helps parents, specialists and the public find different kinds of pupil absence facts and figures for state-funded schools.</p><p class=\"govuk-body\">It allows you to find out about, view and download overall, authorised and unauthorised absence data and statistics going back to 2006/07 on the following levels:</p>"
                }
            );
            
            modelBuilder.Entity<Link>().HasData(
                new Link { PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Id = new Guid("8693c112-225e-4e09-80c2-820cb307bc58"), Description = "2015 to 2016", Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2015-to-2016"},
                new Link { PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Id = new Guid("45bc02ff-de90-489b-b78e-cdc7db662353"), Description = "2014 to 2015", Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015"},
                new Link { PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Id = new Guid("82292fe7-1545-44eb-a094-80c5064701a7"), Description = "2013 to 2014", Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014"},
                new Link { PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Id = new Guid("6907625d-0c2e-4fd8-8e96-aedd85b2ff97"), Description = "2012 to 2013", Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013"},
                new Link { PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Id = new Guid("a538e57a-da5e-4a2c-a89e-b74dbae0c30b"), Description = "2011 to 2012", Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics"},
                new Link { PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Id = new Guid("18b24d60-c56e-44f0-8baa-6db4c6e7deee"), Description = "2010 to 2011", Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2010-to-2011"},
                new Link { PublicationId = new Guid("cbbd299f-8297-44bc-92ac-558bcf51f8ad"), Id = new Guid("c5444f5a-6ba5-4c80-883c-6bca0d8a9eb5"), Description = "2009 to 2010", Url = "https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-including-pupil-characteristics-academic-year-2009-to-2010"}
            );
        }
    }
}
