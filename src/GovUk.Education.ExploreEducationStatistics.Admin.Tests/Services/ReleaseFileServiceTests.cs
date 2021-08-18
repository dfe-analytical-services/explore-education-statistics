#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseFileServiceTests
    {
        private readonly User _user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com"
        };

        [Fact]
        public async Task Delete()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "chart.png",
                    Type = Chart
                }
            };

            var imageFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    Type = Image
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, ancillaryFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.Null(
                    await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                // Check that other files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(chartFile.File.Id));
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_FileFromAmendment()
        {
            var release = new Release();

            var amendmentRelease = new Release
            {
                PreviousVersionId = release.Id
            };

            var ancillaryFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary,
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = ancillaryFile
            };

            var amendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = ancillaryFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, amendmentRelease);
                await contentDbContext.AddRangeAsync(ancillaryFile);
                await contentDbContext.AddRangeAsync(releaseFile, amendmentReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(amendmentRelease.Id, ancillaryFile.Id);

                Assert.True(result.IsRight);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file is unlinked from the amendment
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseFile.Id));

                // Check that the file and link to the previous version remain untouched
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseFile.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_InvalidFileType()
        {
            var release = new Release();

            var dataFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(dataFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, dataFile.File.Id);

                result.AssertBadRequest(FileTypeInvalid);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_ReleaseNotFound()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(ancillaryFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(Guid.NewGuid(), ancillaryFile.File.Id);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_FileNotFound()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_MultipleFiles()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "chart.png",
                    Type = Chart
                }
            };

            var imageFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    Type = Image
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, new List<Guid>
                {
                    ancillaryFile.File.Id,
                    chartFile.File.Id,
                    imageFile.File.Id
                });

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(chartFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_MultipleFilesWithAnInvalidFileType()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var dataFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, dataFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, new List<Guid>
                {
                    ancillaryFile.File.Id,
                    dataFile.File.Id
                });

                result.AssertBadRequest(FileTypeInvalid);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that all the files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_MultipleFilesWithReleaseNotFound()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "chart.png",
                    Type = Chart
                }
            };

            var imageFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    Type = Image
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(Guid.NewGuid(), new List<Guid>
                {
                    ancillaryFile.File.Id,
                    chartFile.File.Id,
                    imageFile.File.Id
                });

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that all the files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(chartFile.File.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_MultipleFilesWithAFileNotFound()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, new List<Guid>
                {
                    ancillaryFile.File.Id,
                    // Include an unknown id
                    Guid.NewGuid()
                });

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Delete_MultipleFilesWithAFileFromAmendment()
        {
            var release = new Release();

            var amendmentRelease = new Release
            {
                PreviousVersionId = release.Id
            };

            var ancillaryFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            };

            var chartFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart
            };

            var ancillaryReleaseFile = new ReleaseFile
            {
                Release = release,
                File = ancillaryFile
            };

            var ancillaryAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = ancillaryFile
            };

            var chartAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = chartFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, amendmentRelease);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile);
                await contentDbContext.AddRangeAsync(ancillaryReleaseFile, ancillaryAmendmentReleaseFile,
                    chartAmendmentReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(amendmentRelease.Id, new List<Guid>
                {
                    ancillaryFile.Id,
                    chartFile.Id
                });

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the ancillary file is unlinked from the amendment
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryAmendmentReleaseFile.Id));

                // Check that the ancillary file and link to the previous version remain untouched
                Assert.NotNull(
                    await contentDbContext.Files.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryReleaseFile.Id));

                // Check that the chart file and link to the amendment are removed
                Assert.Null(await contentDbContext.Files.FindAsync(chartFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartAmendmentReleaseFile.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task DeleteAll()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "chart.png",
                    Type = Chart
                }
            };

            var dataFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };

            var imageFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    Type = Image
                }
            };

            var dataGuidanceFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data-guidance.txt",
                    Type = DataGuidance
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, dataFile, imageFile, dataGuidanceFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, dataGuidanceFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(release.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, dataGuidanceFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(chartFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(dataGuidanceFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(dataGuidanceFile.File.Id));

                // Check that data files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.File.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task DeleteAll_ReleaseNotFound()
        {
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task DeleteAll_NoFiles()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(release.Id);

                Assert.True(result.IsRight);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task DeleteAll_FileFromAmendment()
        {
            var release = new Release();

            var amendmentRelease = new Release
            {
                PreviousVersionId = release.Id
            };

            var ancillaryFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "ancillary.pdf",
                Type = Ancillary
            };

            var chartFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "chart.png",
                Type = Chart
            };

            var dataGuidanceFile = new File
            {
                RootPath = Guid.NewGuid(),
                Filename = "data-guidance.txt",
                Type = DataGuidance,
            };

            var ancillaryReleaseFile = new ReleaseFile
            {
                Release = release,
                File = ancillaryFile
            };

            var ancillaryAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = ancillaryFile
            };

            var chartAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = chartFile
            };

            var dataGuidanceAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                File = dataGuidanceFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, amendmentRelease);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, dataGuidanceFile);
                await contentDbContext.AddRangeAsync(
                    ancillaryReleaseFile,
                    ancillaryAmendmentReleaseFile,
                    chartAmendmentReleaseFile,
                    dataGuidanceAmendmentReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, dataGuidanceFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(amendmentRelease.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the ancillary file is unlinked from the amendment
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryAmendmentReleaseFile.Id));

                // Check that the ancillary file and link to the previous version remain untouched
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryReleaseFile.Id));

                // Check that the chart file and link to the amendment are removed
                Assert.Null(await contentDbContext.Files.FindAsync(chartFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartAmendmentReleaseFile.Id));

                // Check that the data guidance file and link to the amendment are removed
                Assert.Null(await contentDbContext.Files.FindAsync(dataGuidanceFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(dataGuidanceAmendmentReleaseFile.Id));
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task ListAll_NoFiles()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.ListAll(release.Id, Ancillary, Chart);

                Assert.True(result.IsRight);
                Assert.Empty(result.Right);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task ListAll_ReleaseNotFound()
        {
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.ListAll(Guid.NewGuid(), Ancillary, Chart);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task ListAll()
        {
            var release = new Release();

            var ancillaryFile1 = new ReleaseFile
            {
                Release = release,
                Name = "Ancillary Test File 1",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary_1.pdf",
                    Type = Ancillary,
                    CreatedBy = new User
                    {
                        Email = "ancillary1@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var ancillaryFile2 = new ReleaseFile
            {
                Release = release,
                Name = "Ancillary Test File 2",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Ancillary 2.pdf",
                    Type = Ancillary,
                    CreatedBy = new User
                    {
                        Email = "ancillary2@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "chart.png",
                    Type = Chart,
                    CreatedBy = new User
                    {
                        Email = "chart@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var dataFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid(),
                    CreatedBy = new User
                    {
                        Email = "dataFile@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var imageFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    Type = Image,
                    CreatedBy = new User
                    {
                        Email = "image@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile1, ancillaryFile2,
                    chartFile, dataFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PrivateReleaseFiles, It.IsIn(
                        ancillaryFile1.Path(),
                        ancillaryFile2.Path(),
                        chartFile.Path(),
                        imageFile.Path())))
                .ReturnsAsync(true);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles, ancillaryFile1.Path()))
                .ReturnsAsync(new BlobInfo(
                    path: ancillaryFile1.Path(),
                    size: "10 Kb",
                    contentType: "application/pdf",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles, ancillaryFile2.Path()))
                .ReturnsAsync(new BlobInfo(
                    path: ancillaryFile2.Path(),
                    size: "10 Kb",
                    contentType: "application/pdf",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles, chartFile.Path()))
                .ReturnsAsync(new BlobInfo(
                    path: chartFile.Path(),
                    size: "20 Kb",
                    contentType: "image/png",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles, imageFile.Path()))
                .ReturnsAsync(new BlobInfo(
                    path: imageFile.Path(),
                    size: "30 Kb",
                    contentType: "image/png",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.ListAll(release.Id, Ancillary, Chart, Image);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.CheckBlobExists(PrivateReleaseFiles, It.IsIn(
                        ancillaryFile1.Path(),
                        ancillaryFile2.Path(),
                        chartFile.Path(),
                        imageFile.Path())), Times.Exactly(4));

                blobStorageService.Verify(mock =>
                    mock.GetBlob(PrivateReleaseFiles, It.IsIn(
                        ancillaryFile1.Path(),
                        ancillaryFile2.Path(),
                        chartFile.Path(),
                        imageFile.Path())), Times.Exactly(4));

                var fileInfoList = result.Right.ToList();
                Assert.Equal(4, fileInfoList.Count);

                Assert.Equal(ancillaryFile1.File.Id, fileInfoList[0].Id);
                Assert.Equal("pdf", fileInfoList[0].Extension);
                Assert.Equal("ancillary_1.pdf", fileInfoList[0].FileName);
                Assert.Equal("Ancillary Test File 1", fileInfoList[0].Name);
                Assert.Equal("10 Kb", fileInfoList[0].Size);
                Assert.Equal(Ancillary, fileInfoList[0].Type);

                Assert.Equal(ancillaryFile2.File.Id, fileInfoList[1].Id);
                Assert.Equal("pdf", fileInfoList[1].Extension);
                Assert.Equal("Ancillary 2.pdf", fileInfoList[1].FileName);
                Assert.Equal("Ancillary Test File 2", fileInfoList[1].Name);
                Assert.Equal("10 Kb", fileInfoList[1].Size);
                Assert.Equal(Ancillary, fileInfoList[1].Type);

                Assert.Equal(chartFile.File.Id, fileInfoList[2].Id);
                Assert.Equal("png", fileInfoList[2].Extension);
                Assert.Equal("chart.png", fileInfoList[2].FileName);
                Assert.Equal("chart.png", fileInfoList[2].Name);
                Assert.Equal("20 Kb", fileInfoList[2].Size);
                Assert.Equal(Chart, fileInfoList[2].Type);

                Assert.Equal(imageFile.File.Id, fileInfoList[3].Id);
                Assert.Equal("png", fileInfoList[3].Extension);
                Assert.Equal("image.png", fileInfoList[3].FileName);
                Assert.Equal("image.png", fileInfoList[3].Name);
                Assert.Equal("30 Kb", fileInfoList[3].Size);
                Assert.Equal(Image, fileInfoList[3].Type);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetAncillaryFiles()
        {
            var release = new Release();

            var ancillaryFile1 = new ReleaseFile
            {
                Release = release,
                Name = "Ancillary Test File 1",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary_1.pdf",
                    Type = Ancillary,
                    CreatedBy = new User
                    {
                        Email = "ancillary1@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var ancillaryFile2 = new ReleaseFile
            {
                Release = release,
                Name = "Ancillary Test File 2",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Ancillary 2.pdf",
                    Type = Ancillary,
                    CreatedBy = new User
                    {
                        Email = "ancillary2@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "chart.png",
                    Type = Chart,
                    CreatedBy = new User
                    {
                        Email = "chart@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var dataFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid(),
                    CreatedBy = new User
                    {
                        Email = "dataFile@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var imageFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.png",
                    Type = Image,
                    CreatedBy = new User
                    {
                        Email = "image@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile1, ancillaryFile2,
                    chartFile, dataFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PrivateReleaseFiles, It.IsIn(
                        ancillaryFile1.Path(),
                        ancillaryFile2.Path())))
                .ReturnsAsync(true);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles, ancillaryFile1.Path()))
                .ReturnsAsync(new BlobInfo(
                    path: ancillaryFile1.Path(),
                    size: "10 Kb",
                    contentType: "application/pdf",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles, ancillaryFile2.Path()))
                .ReturnsAsync(new BlobInfo(
                    path: ancillaryFile2.Path(),
                    size: "10 Kb",
                    contentType: "application/pdf",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.GetAncillaryFiles(release.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.CheckBlobExists(PrivateReleaseFiles, It.IsIn(
                        ancillaryFile1.Path(), ancillaryFile2.Path())),
                        Times.Exactly(2));

                blobStorageService.Verify(mock =>
                    mock.GetBlob(PrivateReleaseFiles, It.IsIn(
                        ancillaryFile1.Path(), ancillaryFile2.Path())),
                        Times.Exactly(2));

                var fileInfoList = result.Right.ToList();
                Assert.Equal(2, fileInfoList.Count);

                Assert.Equal(ancillaryFile1.File.Id, fileInfoList[0].Id);
                Assert.Equal("pdf", fileInfoList[0].Extension);
                Assert.Equal("ancillary_1.pdf", fileInfoList[0].FileName);
                Assert.Equal("Ancillary Test File 1", fileInfoList[0].Name);
                Assert.Equal("10 Kb", fileInfoList[0].Size);
                Assert.Equal(Ancillary, fileInfoList[0].Type);
                Assert.Equal(ancillaryFile1.File.Created, fileInfoList[0].Created);
                Assert.Equal(ancillaryFile1.File.CreatedBy.Email, fileInfoList[0].UserName);

                Assert.Equal(ancillaryFile2.File.Id, fileInfoList[1].Id);
                Assert.Equal("pdf", fileInfoList[1].Extension);
                Assert.Equal("Ancillary 2.pdf", fileInfoList[1].FileName);
                Assert.Equal("Ancillary Test File 2", fileInfoList[1].Name);
                Assert.Equal("10 Kb", fileInfoList[1].Size);
                Assert.Equal(Ancillary, fileInfoList[1].Type);
                Assert.Equal(ancillaryFile2.File.Created, fileInfoList[1].Created);
                Assert.Equal(ancillaryFile2.File.CreatedBy.Email, fileInfoList[1].UserName);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetFile()
        {
            var releaseFile = new ReleaseFile
            {
                Release = new Release(),
                Name = "Test PDF File",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService
                .Setup(mock => mock.CheckBlobExists(PrivateReleaseFiles, releaseFile.Path()))
                .ReturnsAsync(true);
            blobStorageService
                .Setup(mock => mock.GetBlob(PrivateReleaseFiles, releaseFile.Path()))
                .ReturnsAsync(
                    new BlobInfo(
                        path: null,
                        size: "93 Kb",
                        contentType: "application/pdf",
                        contentLength: 0L,
                        meta: null,
                        created: null
                    )
                );

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.GetFile(releaseFile.ReleaseId, releaseFile.FileId);
                Assert.True(result.IsRight);

                var fileInfo = result.Right;
                Assert.Equal(releaseFile.Name, fileInfo.Name);
                Assert.Equal(releaseFile.FileId, fileInfo.Id);
                Assert.Equal("pdf", fileInfo.Extension);
                Assert.Equal("ancillary.pdf", fileInfo.FileName);
                Assert.Equal("93 Kb", fileInfo.Size);
                Assert.Equal(Ancillary, fileInfo.Type);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetFile_NoRelease()
        {
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.GetFile(Guid.NewGuid(), Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetFile_NoReleaseFile()
        {
            var release = new Release();
            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.GetFile(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task GetFile_NoBlob()
        {
            var releaseFile = new ReleaseFile
            {
                Release = new Release(),
                Name = "Test PDF File",
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PrivateReleaseFiles, releaseFile.Path()))
                .ReturnsAsync(false);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.GetFile(releaseFile.ReleaseId, releaseFile.FileId);
                var fileInfo = result.AssertRight();

                Assert.Equal("Test PDF File", fileInfo.Name);
                Assert.Equal(releaseFile.FileId, fileInfo.Id);
                Assert.Equal("pdf", fileInfo.Extension);
                Assert.Equal(releaseFile.File.Filename, fileInfo.FileName);
                Assert.Equal("0.00 B", fileInfo.Size);
                Assert.Equal(releaseFile.File.Type, fileInfo.Type);
            }
            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream()
        {
            var release = new Release();

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blob = new BlobInfo(
                path: null,
                size: null,
                contentType: "application/pdf",
                contentLength: 0L,
                meta: null,
                created: null);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles, releaseFile.Path()))
                .ReturnsAsync(blob);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PrivateReleaseFiles, releaseFile.Path(),
                        It.IsAny<MemoryStream>(), null))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(
                    mock => mock.GetBlob(PrivateReleaseFiles, releaseFile.Path()),
                    Times.Once());

                blobStorageService.Verify(
                    mock =>
                        mock.DownloadToStream(PrivateReleaseFiles, releaseFile.Path(),
                        It.IsAny<MemoryStream>(), null),
                    Times.Once());

                Assert.Equal("application/pdf", result.Right.ContentType);
                Assert.Equal("ancillary.pdf", result.Right.FileDownloadName);
                Assert.IsType<MemoryStream>(result.Right.FileStream);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_MixedCaseFilename()
        {
            var release = new Release();

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "Ancillary 1.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            var blob = new BlobInfo(
                path: null,
                size: null,
                contentType: "application/pdf",
                contentLength: 0L,
                meta: null,
                created: null);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles, releaseFile.Path()))
                .ReturnsAsync(blob);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PrivateReleaseFiles, releaseFile.Path(),
                        It.IsAny<MemoryStream>(), null))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(
                    mock => mock.GetBlob(PrivateReleaseFiles, releaseFile.Path()),
                    Times.Once());

                blobStorageService.Verify(
                    mock =>
                        mock.DownloadToStream(PrivateReleaseFiles, releaseFile.Path(),
                        It.IsAny<MemoryStream>(), null),
                    Times.Once());

                Assert.Equal("application/pdf", result.Right.ContentType);
                Assert.Equal("Ancillary 1.pdf", result.Right.FileDownloadName);
                Assert.IsType<MemoryStream>(result.Right.FileStream);
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_ReleaseNotFound()
        {
            var release = new Release();

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(Guid.NewGuid(), releaseFile.File.Id);

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Stream_FileNotFound()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }

            MockUtils.VerifyAllMocks(blobStorageService);
        }

        [Fact]
        public async Task Update()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Test PDF File",
                File = new File
                {
                    RootPath = release.Id,
                    Filename = "test.pdf",
                    Type = Ancillary,
                    Created = new DateTime(),
                    CreatedBy = new User
                    {
                        Email = "test@test.com"
                    }
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.Update(
                    releaseFile.ReleaseId,
                    releaseFile.FileId,
                    new ReleaseFileUpdateViewModel
                    {
                        Title = "New file title",
                        Summary = "New file summary"
                    }
                );

                Assert.True(result.IsRight);
                Assert.IsType<Unit>(result.Right);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var updatedReleaseFile = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                    rf.ReleaseId == releaseFile.ReleaseId
                    && rf.FileId == releaseFile.FileId);

                Assert.Equal("New file title", updatedReleaseFile.Name);
                Assert.Equal("New file summary", updatedReleaseFile.Summary);
            }
        }

        [Fact]
        public async Task Update_OnlyName()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Old file name",
                Summary = "Old file summary",
                File = new File
                {
                    RootPath = release.Id,
                    Filename = "test.pdf"
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.Update(
                    releaseFile.ReleaseId,
                    releaseFile.FileId,
                    new ReleaseFileUpdateViewModel
                    {
                        Title = "New file title",
                    }
                );

                Assert.True(result.IsRight);
                Assert.IsType<Unit>(result.Right);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var updatedReleaseFile = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                    rf.ReleaseId == releaseFile.ReleaseId
                    && rf.FileId == releaseFile.FileId);

                Assert.Equal("New file title", updatedReleaseFile.Name);
                Assert.Equal("Old file summary", updatedReleaseFile.Summary);
            }
        }

        [Fact]
        public async Task Update_OnlySummary()
        {
            var release = new Release
            {
                Id = Guid.NewGuid(),
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                Name = "Old file title",
                Summary = "Old file summary",
                File = new File
                {
                    RootPath = release.Id,
                    Filename = "test.pdf",
                }
            };

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.Update(
                    releaseFile.ReleaseId,
                    releaseFile.FileId,
                    new ReleaseFileUpdateViewModel
                    {
                        Summary = "New file summary",
                    }
                );

                Assert.True(result.IsRight);
                Assert.IsType<Unit>(result.Right);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var updatedReleaseFile = await contentDbContext.ReleaseFiles.FirstAsync(rf =>
                    rf.ReleaseId == releaseFile.ReleaseId
                    && rf.FileId == releaseFile.FileId);

                Assert.Equal("Old file title", updatedReleaseFile.Name);
                Assert.Equal("New file summary", updatedReleaseFile.Summary);
            }
        }

        [Fact]
        public async Task Update_NoRelease()
        {
            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext);

                var result = await service.Update(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new ReleaseFileUpdateViewModel
                    {
                        Title = "New file title",
                    }
                );

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async Task Update_NoReleaseFile()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
            {
                var service = SetupReleaseFileService(contentDbContext);

                var result = await service.Update(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    new ReleaseFileUpdateViewModel
                    {
                        Title = "New file title",
                    }
                );

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async Task UploadAncillary()
        {
            const string filename = "ancillary.pdf";

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var formFile = CreateFormFileMock(filename).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(release.Id, Ancillary))),
                    formFile,
                    null
                )).Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, Ancillary)))))
                .ReturnsAsync(new BlobInfo(
                    path: "ancillary/file/path",
                    size: "10 Kb",
                    contentType: "application/pdf",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileForUpload(formFile, Ancillary))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.UploadAncillary(
                    release.Id,
                    new ReleaseAncillaryFileUploadViewModel
                    {
                        Title = "Test name",
                        Summary = "Test summary",
                        File = formFile
                    }
                );

                var fileInfo = result.AssertRight();

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(formFile, Ancillary), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, Ancillary))),
                        formFile,
                        null
                    ), Times.Once);

                blobStorageService.Verify(mock =>
                        mock.GetBlob(PrivateReleaseFiles,
                            It.Is<string>(path =>
                                path.Contains(FilesPath(release.Id, Ancillary)))),
                    Times.Once);

                Assert.True(fileInfo.Id.HasValue);
                Assert.Equal("pdf", fileInfo.Extension);
                Assert.Equal("ancillary.pdf", fileInfo.FileName);
                Assert.Equal("Test name", fileInfo.Name);
                Assert.Equal("Test summary", fileInfo.Summary);
                Assert.Equal("10 Kb", fileInfo.Size);
                Assert.Equal(Ancillary, fileInfo.Type);
                Assert.Equal("test@test.com", fileInfo.UserName);
                Assert.InRange(DateTime.UtcNow.Subtract(fileInfo.Created.GetValueOrDefault()).Milliseconds, 0, 1500);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseFile = await contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .SingleOrDefaultAsync(rf =>
                    rf.ReleaseId == release.Id
                    && rf.File.Filename == filename
                    && rf.File.Type == Ancillary
                );

                Assert.NotNull(releaseFile);
                Assert.InRange(DateTime.UtcNow.Subtract(releaseFile.File.Created.GetValueOrDefault()).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, releaseFile.File.CreatedById);
            }

            MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService);
        }

        [Fact]
        public async Task UploadChart()
        {
            const string filename = "chart.png";

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var formFile = CreateFormFileMock(filename).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(release.Id, Chart))),
                    formFile,
                    null
                )).Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, Chart)))))
                .ReturnsAsync(new BlobInfo(
                    path: "chart/file/path",
                    size: "20 Kb",
                    contentType: "image/png",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>(),
                    created: null));

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileForUpload(formFile, Chart))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.UploadChart(release.Id, formFile);

                Assert.True(result.IsRight);

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(formFile, Chart), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, Chart))),
                        formFile,
                        null
                    ), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.GetBlob(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, Chart)))),
                    Times.Once);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal("png", result.Right.Extension);
                Assert.Equal("chart.png", result.Right.FileName);
                Assert.Equal("chart.png", result.Right.Name);
                Assert.Equal("20 Kb", result.Right.Size);
                Assert.Equal(Chart, result.Right.Type);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var releaseFile = await contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .SingleOrDefaultAsync(rf =>
                        rf.ReleaseId == release.Id
                        && rf.File.Filename == filename
                        && rf.File.Type == Chart
                    );

                Assert.NotNull(releaseFile);
                Assert.InRange(DateTime.UtcNow.Subtract(releaseFile.File.Created.Value).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, releaseFile.File.CreatedById);
            }

            MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService);
        }

        private ReleaseFileService SetupReleaseFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IBlobStorageService? blobStorageService = null,
            IFileRepository? fileRepository = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IUserService? userService = null)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new ReleaseFileService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                fileRepository ?? new FileRepository(contentDbContext),
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
            );
        }
    }
}
