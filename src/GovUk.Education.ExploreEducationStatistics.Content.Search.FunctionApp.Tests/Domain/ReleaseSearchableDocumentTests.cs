using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Domain;

public abstract class ReleaseSearchableDocumentTests
{
    public class EqualsTests : ReleaseSearchableDocumentTests
    {
        [Fact]
        public void PublishingOrganisationsWithSameContent_AreEqual()
        {
            // Verify that equality compares publishing organisations by value rather than reference.
            var first = new ReleaseSearchableDocumentBuilder()
                .WithPublishingOrganisations(
                    new PublishingOrganisation
                    {
                        Id = new Guid("7cbcfe03-9f7e-478a-8512-a1a5e0ca793b"),
                        Title = "Department for Education",
                    },
                    new PublishingOrganisation
                    {
                        Id = new Guid("7c3252c6-6e94-4a34-8762-ff52aae5c0c0"),
                        Title = "Ofsted",
                    }
                )
                .Build();

            var second = new ReleaseSearchableDocumentBuilder()
                .WithPublishingOrganisations(
                    new PublishingOrganisation
                    {
                        Id = new Guid("7cbcfe03-9f7e-478a-8512-a1a5e0ca793b"),
                        Title = "Department for Education",
                    },
                    new PublishingOrganisation
                    {
                        Id = new Guid("7c3252c6-6e94-4a34-8762-ff52aae5c0c0"),
                        Title = "Ofsted",
                    }
                )
                .Build();

            Assert.Equal(first, second);
        }

        [Fact]
        public void PublishingOrganisationsWithDifferentContent_AreNotEqual()
        {
            var first = new ReleaseSearchableDocumentBuilder()
                .WithPublishingOrganisations(
                    new PublishingOrganisation
                    {
                        Id = new Guid("7cbcfe03-9f7e-478a-8512-a1a5e0ca793b"),
                        Title = "Department for Education",
                    }
                )
                .Build();

            var second = new ReleaseSearchableDocumentBuilder()
                .WithPublishingOrganisations(
                    new PublishingOrganisation
                    {
                        Id = new Guid("7c3252c6-6e94-4a34-8762-ff52aae5c0c0"),
                        Title = "Ofsted",
                    }
                )
                .Build();

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void PublishingOrganisationsWithDifferentOrder_AreNotEqual()
        {
            var first = new ReleaseSearchableDocumentBuilder()
                .WithPublishingOrganisations(
                    new PublishingOrganisation
                    {
                        Id = new Guid("7cbcfe03-9f7e-478a-8512-a1a5e0ca793b"),
                        Title = "Department for Education",
                    },
                    new PublishingOrganisation
                    {
                        Id = new Guid("7c3252c6-6e94-4a34-8762-ff52aae5c0c0"),
                        Title = "Ofsted",
                    }
                )
                .Build();

            var second = new ReleaseSearchableDocumentBuilder()
                .WithPublishingOrganisations(
                    new PublishingOrganisation
                    {
                        Id = new Guid("7c3252c6-6e94-4a34-8762-ff52aae5c0c0"),
                        Title = "Ofsted",
                    },
                    new PublishingOrganisation
                    {
                        Id = new Guid("7cbcfe03-9f7e-478a-8512-a1a5e0ca793b"),
                        Title = "Department for Education",
                    }
                )
                .Build();

            Assert.NotEqual(first, second);
        }
    }
}
