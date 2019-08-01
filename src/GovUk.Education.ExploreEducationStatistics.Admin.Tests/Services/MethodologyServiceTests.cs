using System;
using System.Collections.Generic;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class MethodologyServiceTests
    {
        [Fact]
        public void GetTopicMethodologies()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext("TopicMethodologies"))
            {
                var methodologies = new List<Methodology>
                {
                    new Methodology
                    {
                        Id = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"),
                        Title = "Methodology 1"
                    },
                    new Methodology
                    {
                        Id = new Guid("ebeb2b2d-fc6b-4734-9420-4e4dd37816ba"),
                        Title = "Methodology 2",
                    },
                };

                var topics = new Topic
                {
                    Id = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"),
                };

                var publications = new List<Publication>
                {
                    new Publication
                    {
                        Id = new Guid("847b7c45-edce-4e9d-aeea-f04251896aae"),
                        MethodologyId = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"), // Methodology 1
                        TopicId = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"),
                        Title = "Publication 1"
                    },
                    new Publication
                    {
                        Id = new Guid("b940394c-6e5c-48be-bb1e-541ebc7648c8"),
                        MethodologyId = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"), // Methodology 1
                        TopicId = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"),
                        Title = "Publication 2"
                    },
                    new Publication
                    {
                        Id = new Guid("90924bc3-5066-46f3-ac94-9eccd1c89799"),
                        MethodologyId = new Guid("ebeb2b2d-fc6b-4734-9420-4e4dd37816ba"), // Methodology 2
                        TopicId = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"),
                        Title = "Publication 3"
                    },
                };
                context.AddRange(methodologies);
                context.AddRange(topics);
                context.AddRange(publications);
                context.SaveChanges();
            }

            using (var context = DbUtils.InMemoryApplicationDbContext("TopicMethodologies"))
            {
                // Method under test
                var topicMethodologies = new MethodologyService(context, MapperForProfile<MappingProfiles>()).GetTopicMethodologiesAsync(new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"));
                Assert.Contains(topicMethodologies.Result, m => m.Id == new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"));
                Assert.Contains(topicMethodologies.Result, m => m.Id == new Guid("ebeb2b2d-fc6b-4734-9420-4e4dd37816ba"));
                Assert.Equal(2, topicMethodologies.Result.Count); // Check we don't have duplicates
            }
        }

        [Fact]
        public void GetMethodologies()
        {
            using (var context = DbUtils.InMemoryApplicationDbContext("Get"))
            {
                var methodologies = new List<Methodology>
                {
                    new Methodology
                    {
                        Id = new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46"),
                        Title = "Methodology 1"
                    },
                    new Methodology
                    {
                        Id = new Guid("ebeb2b2d-fc6b-4734-9420-4e4dd37816ba"),
                        Title = "Methodology 2",
                    },
                };
                
                context.AddRange(methodologies);
                context.SaveChanges();
            }
            
            using (var context = DbUtils.InMemoryApplicationDbContext("Get"))
            {
                
                
                // Method under test
                var methodologies = new MethodologyService(context, MapperForProfile<MappingProfiles>()).ListAsync();
                Assert.Contains(methodologies.Result, m => m.Id == new Guid("d5ed05f4-8364-4682-b6fe-7dde181d6c46") && m.Title == "Methodology 1");
                Assert.Contains(methodologies.Result, m => m.Id == new Guid("ebeb2b2d-fc6b-4734-9420-4e4dd37816ba") && m.Title == "Methodology 2");
            }
        }

    }
}