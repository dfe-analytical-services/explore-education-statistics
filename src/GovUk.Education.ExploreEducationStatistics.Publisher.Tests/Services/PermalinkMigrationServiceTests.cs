#nullable enable
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using Moq;
using Xunit;
using static Azure.Storage.Blobs.Models.BlobsModelFactory;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

/// <summary>
/// TODO EES-3755 Remove after Permalink snapshot migration work is complete
/// </summary>
public class PermalinkMigrationServiceTests
{
    private readonly Guid _releaseId = Guid.NewGuid();
    private readonly Guid _subjectId = Guid.NewGuid();
    private readonly DateTime _created = DateTime.UtcNow;
    private const string PublicationTitle = "Test publication";
    private const string DataSetTitle = "Test data set";

    [Fact]
    public async Task MigratePermalink()
    {
        var permalinkId = Guid.NewGuid();

        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""Created"": ""{_created.ToString("O")}"",
            ""Configuration"": {{
                ""TableHeaders"": {{
                    ""ColumnGroups"": [],
                    ""Columns"": [],
                    ""RowGroups"": [],
                    ""Rows"": []
                }}
            }},
            ""FullTable"": {{
                ""SubjectMeta"": {{
                    ""PublicationName"": ""{PublicationTitle}"",
                    ""SubjectName"": ""{DataSetTitle}"",
                    ""Filters"": {{
                        ""Filter1"": {{
                          ""Options"": {{
                            ""Filter1Group1"": {{
                              ""Options"": [
                                {{
                                  ""Label"": ""Filter 1 Group 1 Item 1""
                                }},
                                {{
                                  ""Label"": ""Filter 1 Group 1 Item 2""
                                }}
                              ]
                            }},
                            ""Filter1Group2"": {{
                              ""Options"": [
                                {{
                                  ""Label"": ""Filter 1 Group 2 Item 1""
                                }},
                                {{
                                  ""Label"": ""Filter 1 Group 1 Item 2""
                                }}
                              ]
                            }}
                          }}
                        }},
                        ""Filter2"": {{
                          ""Options"": {{
                            ""Filter2Group1"": {{
                              ""Options"": [
                                {{
                                  ""Label"": ""Filter 2 Group 1 Item 1""
                                }},
                                {{
                                  ""Label"": ""Filter 2 Group 1 Item 2""
                                }}
                              ]
                            }}
                          }}
                        }}
                    }},
                    ""Footnotes"": [
                        {{
                            ""Id"": """"
                        }},
                        {{
                            ""Id"": """"
                        }}
                    ],
                    ""Indicators"": [
                        {{
                            ""Id"": """"
                        }},
                        {{
                            ""Id"": """"
                        }},
                        {{
                            ""Id"": """"
                        }}
                    ],
                    ""LocationsHierarchical"": {{
                        ""localAuthority"": [
                            {{
                                ""Label"": ""North East"",
                                ""Value"": ""E12000001"",
                                ""Level"": ""region"",
                                ""Options"": [
                                    {{
                                        ""Id"": ""7decfe81-30c3-4896-a664-54831689052e"",
                                        ""Label"": ""Newcastle upon Tyne"",
                                        ""Value"": ""E08000021""
                                    }},
                                    {{
                                        ""Id"": ""e0dd2816-cd98-497f-bc63-167e2083b86c"",
                                        ""Label"": ""North Tyneside"",
                                        ""Value"": ""E08000022""
                                    }}
                                ]
                            }},
                            {{
                                ""Level"": ""region"",
                                ""Label"": ""North West"",
                                ""Value"": ""E12000002"",
                                ""Options"": [
                                    {{
                                        ""Id"": ""c1edd525-a606-4cf2-846c-85ea00b7ef60"",
                                        ""Label"": ""Bolton"",
                                        ""Value"": ""E08000001""
                                    }},
                                    {{
                                        ""Id"": ""ff72e20c-2bc1-4d87-8aba-a2b9e4f2df23"",
                                        ""Label"": ""Bury"",
                                        ""Value"": ""E08000002""
                                    }}
                                ]
                            }}
                        ],
                        ""country"": [
                            {{
                                ""Id"": ""376f9a26-dc39-4db3-bb19-0549e59d322a"",
                                ""Label"": ""England"",
                                ""Value"": ""E92000001""
                            }}
                        ]
                    }},
                    ""TimePeriodRange"": [
                        {{
                            ""Code"": ""AY"",
                            ""Label"": ""2016/17"",
                            ""Year"": 2016
                        }},
                        {{
                            ""Code"": ""AY"",
                            ""Label"": ""2017/18"",
                            ""Year"": 2017
                        }}
                    ]
                }},
                ""Results"": [
                    {{
                        ""Id"": """"
                    }},
                    {{
                        ""Id"": """"
                    }},
                    {{
                        ""Id"": """"
                    }},
                    {{
                        ""Id"": """"
                    }},
                    {{
                        ""Id"": """"
                    }}
                ]
            }},
            ""Query"": {{
                ""SubjectId"": ""{_subjectId}""
            }}
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId,
                Created = _created,
                PublicationTitle = PublicationTitle,
                DataSetTitle = DataSetTitle,
                ReleaseId = _releaseId,
                SubjectId = _subjectId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);

            // Existing fields should not be touched
            Assert.Equal(permalinkId, saved.Id);
            Assert.Equal(_created, saved.Created);
            Assert.Equal(PublicationTitle, saved.PublicationTitle);
            Assert.Equal(DataSetTitle, saved.DataSetTitle);
            Assert.Equal(_releaseId, saved.ReleaseId);
            Assert.Equal(_subjectId, saved.SubjectId);

            // Content length should be set
            Assert.True(saved.LegacyContentLength > 0);

            // Statistics about the Permalink should be set
            Assert.Equal(6, saved.CountFilterItems);
            Assert.Equal(2, saved.CountFootnotes);
            Assert.Equal(3, saved.CountIndicators);
            Assert.Equal(5, saved.CountLocations);
            Assert.Equal(5, saved.CountObservations);
            Assert.Equal(2, saved.CountTimePeriods);
            Assert.True(saved.LegacyHasConfigurationHeaders);
        }
    }

    [Fact]
    public async Task MigratePermalink_LegacyLocations()
    {
        var permalinkId = Guid.NewGuid();

        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""FullTable"": {{
                ""SubjectMeta"": {{
                    ""Locations"": [
                        {{
                            ""Level"": ""localAuthority"",
                            ""Label"": ""Blackpool"",
                            ""Value"": ""E06000009""
                        }},
                        {{
                            ""Level"": ""localAuthority"",
                            ""Label"": ""Derby"",
                            ""Value"": ""E06000015""
                        }},
                        {{
                            ""Level"": ""localAuthority"",
                            ""Label"": ""Nottingham"",
                            ""Value"": ""E06000018""
                        }}
                    ]
                }}
            }}
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId,
                Created = _created,
                PublicationTitle = PublicationTitle,
                DataSetTitle = DataSetTitle,
                ReleaseId = _releaseId,
                SubjectId = _subjectId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);

            Assert.Equal(3, saved.CountLocations);
        }
    }

    [Fact]
    public async Task MigratePermalink_HasNoContent()
    {
        var permalinkId = Guid.NewGuid();

        // Create a permalink with nothing but Id
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}""
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var permalink = new Permalink
            {
                Id = permalinkId,
                Created = _created,
                PublicationTitle = PublicationTitle,
                DataSetTitle = DataSetTitle,
                ReleaseId = _releaseId,
                SubjectId = _subjectId
            };

            await contentDbContext.Permalinks.AddAsync(permalink);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);

            // Existing fields should not be touched
            Assert.Equal(permalinkId, saved.Id);
            Assert.Equal(_created, saved.Created);
            Assert.Equal(PublicationTitle, saved.PublicationTitle);
            Assert.Equal(DataSetTitle, saved.DataSetTitle);
            Assert.Equal(_releaseId, saved.ReleaseId);
            Assert.Equal(_subjectId, saved.SubjectId);

            // Content length should be set
            Assert.True(saved.LegacyContentLength > 0);

            // Statistics about the Permalink should be set but they are all zero/false
            Assert.Equal(0, saved.CountFilterItems);
            Assert.Equal(0, saved.CountFootnotes);
            Assert.Equal(0, saved.CountIndicators);
            Assert.Equal(0, saved.CountLocations);
            Assert.Equal(0, saved.CountObservations);
            Assert.Equal(0, saved.CountTimePeriods);
            Assert.False(saved.LegacyHasConfigurationHeaders);
        }
    }

    [Fact]
    public async Task MigratePermalink_HasNullConfiguration()
    {
        var permalinkId = Guid.NewGuid();

        // Create a permalink with a null Configuration element
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""Configuration"": null
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);

            Assert.False(saved.LegacyHasConfigurationHeaders);
        }
    }

    [Fact]
    public async Task MigratePermalink_HasConfigurationWithNoTableHeaders()
    {
        var permalinkId = Guid.NewGuid();

        // Create a permalink with an empty Configuration element
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""Configuration"": {{
            }}
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);

            Assert.False(saved.LegacyHasConfigurationHeaders);
        }
    }

    [Fact]
    public async Task MigratePermalink_HasConfigurationWithNullTableHeaders()
    {
        var permalinkId = Guid.NewGuid();

        // Create a permalink with a Configuration element with null TableHeaders
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""Configuration"": {{
                ""TableHeaders"": null
            }}
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);

            Assert.False(saved.LegacyHasConfigurationHeaders);
        }
    }

    [Fact]
    public async Task MigratePermalink_HasFullTableWithNoContent()
    {
        var permalinkId = Guid.NewGuid();

        // Create a permalink with an empty FullTable element
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""FullTable"": {{
            }}
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);
            
            Assert.Equal(0, saved.CountFilterItems);
            Assert.Equal(0, saved.CountFootnotes);
            Assert.Equal(0, saved.CountIndicators);
            Assert.Equal(0, saved.CountLocations);
            Assert.Equal(0, saved.CountObservations);
            Assert.Equal(0, saved.CountTimePeriods);
        }
    }

    [Fact]
    public async Task MigratePermalink_HasNullFullTable()
    {
        var permalinkId = Guid.NewGuid();

        // Create a permalink with an empty FullTable element
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""FullTable"": null
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);
            
            Assert.Equal(0, saved.CountFilterItems);
            Assert.Equal(0, saved.CountFootnotes);
            Assert.Equal(0, saved.CountIndicators);
            Assert.Equal(0, saved.CountLocations);
            Assert.Equal(0, saved.CountObservations);
            Assert.Equal(0, saved.CountTimePeriods);
        }
    }

    [Fact]
    public async Task MigratePermalink_HasSubjectMetaWithNoContent()
    {
        var permalinkId = Guid.NewGuid();

        // Create a permalink with an empty SubjectMeta element
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""FullTable"": {{
                ""SubjectMeta"": {{
                }},
                ""Results"": [
                    {{
                        ""Id"": """"
                    }}
                ]
            }}
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);
            
            Assert.Equal(0, saved.CountFilterItems);
            Assert.Equal(0, saved.CountFootnotes);
            Assert.Equal(0, saved.CountIndicators);
            Assert.Equal(0, saved.CountLocations);
            Assert.Equal(1, saved.CountObservations);
            Assert.Equal(0, saved.CountTimePeriods);
        }
    }

    [Fact]
    public async Task MigratePermalink_HasNullSubjectMeta()
    {
        var permalinkId = Guid.NewGuid();

        // Create a permalink with an empty SubjectMeta element
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}"",
            ""FullTable"": {{
                ""SubjectMeta"": null,
                ""Results"": [
                    {{
                        ""Id"": """"
                    }}
                ]
            }}
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.Permalinks.AddAsync(new Permalink
            {
                Id = permalinkId
            });
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = SetupService(contentDbContext: contentDbContext,
                blobServiceClient: blobServiceClient.Object
            );

            await service.MigratePermalink(permalinkId);
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var saved = contentDbContext.Permalinks.SingleOrDefault(permalink => permalink.Id == permalinkId);
            Assert.NotNull(saved);
            
            Assert.Equal(0, saved.CountFilterItems);
            Assert.Equal(0, saved.CountFootnotes);
            Assert.Equal(0, saved.CountIndicators);
            Assert.Equal(0, saved.CountLocations);
            Assert.Equal(1, saved.CountObservations);
            Assert.Equal(0, saved.CountTimePeriods);
        }
    }

    [Fact]
    public async Task MigratePermalink_PermalinkNotFoundInDatabase()
    {
        var permalinkId = Guid.NewGuid();
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{permalinkId}""
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        await using var contentDbContext = InMemoryContentDbContext();

        var service = SetupService(contentDbContext: contentDbContext,
            blobServiceClient: blobServiceClient.Object
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.MigratePermalink(permalinkId));

        Assert.Equal($"Permalink with id {permalinkId} not found", exception.Message);
    }

    [Fact]
    public async Task MigratePermalink_PermalinkNotFoundInBlobStorage()
    {
        var permalinkId = Guid.NewGuid();

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId,
            permalinkAsJson: null);

        await using var contentDbContext = InMemoryContentDbContext();
        await contentDbContext.Permalinks.AddAsync(new Permalink
        {
            Id = permalinkId
        });
        await contentDbContext.SaveChangesAsync();

        var service = SetupService(contentDbContext: contentDbContext,
            blobServiceClient: blobServiceClient.Object
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.MigratePermalink(permalinkId));

        Assert.Equal($"Blob not found for permalink {permalinkId}", exception.Message);
    }

    [Fact]
    public async Task MigratePermalink_PermalinkInBlobStorageHasUnexpectedId()
    {
        var permalinkId = Guid.NewGuid();
        var otherId = Guid.NewGuid();

        // Create a permalink with a different id
        var permalinkAsJson = $@"
        {{
            ""Id"": ""{otherId}""
        }}";

        var blobServiceClient = MockBlobServiceClientForPermalink(permalinkId, permalinkAsJson);

        await using var contentDbContext = InMemoryContentDbContext();
        await contentDbContext.Permalinks.AddAsync(new Permalink
        {
            Id = permalinkId
        });
        await contentDbContext.SaveChangesAsync();

        var service = SetupService(contentDbContext: contentDbContext,
            blobServiceClient: blobServiceClient.Object
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await service.MigratePermalink(permalinkId));

        Assert.Equal($"Blob does not match expected Permalink id {permalinkId}", exception.Message);
    }

    private static Mock<BlobServiceClient> MockBlobServiceClientForPermalink(Guid permalinkId,
        string? permalinkAsJson = null)
    {
        var exists = permalinkAsJson != null;
        var contentLength = exists ? Encoding.UTF8.GetBytes(permalinkAsJson!).Length : 0;

        var blobClient = MockBlobClient(permalinkId.ToString(), contentLength, exists);
        var blobContainerClient = MockBlobContainerClient(Permalinks.Name, blobClient);

        if (exists)
        {
            blobClient.Setup(s => s.OpenReadAsync(0, null, null, default))
                .ReturnsAsync(permalinkAsJson!.ToStream());
        }

        return MockBlobServiceClient(blobContainerClient);
    }

    private static Mock<BlobClient> MockBlobClient(string name,
        int contentLength,
        bool exists = true)
    {
        var blobClient = new Mock<BlobClient>(Strict);

        blobClient.SetupGet(client => client.Name)
            .Returns(name);

        blobClient.Setup(client => client.ExistsAsync(default))
            .ReturnsAsync(Response.FromValue(exists, null!));

        if (exists)
        {
            var blobProperties = BlobProperties(
                contentLength: contentLength
            );

            blobClient.Setup(client => client.GetPropertiesAsync(null, default))
                .ReturnsAsync(Response.FromValue(blobProperties, null!));
        }

        return blobClient;
    }

    private static Mock<BlobContainerClient> MockBlobContainerClient(string containerName,
        params Mock<BlobClient>[] blobClients)
    {
        var blobContainerClient = new Mock<BlobContainerClient>(Strict);

        blobContainerClient
            .SetupGet(client => client.Name)
            .Returns(containerName);

        foreach (var blobClient in blobClients)
        {
            blobContainerClient.Setup(client => client.GetBlobClient(blobClient.Object.Name))
                .Returns(blobClient.Object);
        }

        return blobContainerClient;
    }

    private static Mock<BlobServiceClient> MockBlobServiceClient(
        params Mock<BlobContainerClient>[] blobContainerClients)
    {
        var blobServiceClient = new Mock<BlobServiceClient>(Strict);

        foreach (var blobContainerClient in blobContainerClients)
        {
            blobServiceClient.Setup(client => client.GetBlobContainerClient(blobContainerClient.Object.Name))
                .Returns(blobContainerClient.Object);
        }

        return blobServiceClient;
    }

    private static PermalinkMigrationService SetupService(
        BlobServiceClient blobServiceClient,
        ContentDbContext? contentDbContext = null)
    {
        return new PermalinkMigrationService(
            contentDbContext ?? Mock.Of<ContentDbContext>(),
            blobServiceClient
        );
    }
}
