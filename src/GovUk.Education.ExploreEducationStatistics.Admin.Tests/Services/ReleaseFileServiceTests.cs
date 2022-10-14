#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockFormTestUtils;
using static Moq.MockBehavior;
using File = GovUk.Education.ExploreEducationStatistics.Content.Model.File;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseFileServiceTests : IDisposable
    {
        private readonly List<string> _filePaths = new();

        public void Dispose()
        {
            // Cleanup any files that have been
            // written to the filesystem.
            _filePaths.ForEach(System.IO.File.Delete);
        }

        private readonly User _user = new()
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, ancillaryFile.File.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, amendmentRelease);
                await contentDbContext.AddRangeAsync(ancillaryFile);
                await contentDbContext.AddRangeAsync(releaseFile, amendmentReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(amendmentRelease.Id, ancillaryFile.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Check that the file is unlinked from the amendment
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(amendmentReleaseFile.Id));

                // Check that the file and link to the previous version remain untouched
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(releaseFile.Id));
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(dataFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.Delete(release.Id, dataFile.File.Id);

                result.AssertBadRequest(FileTypeInvalid);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.File.Id));
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(ancillaryFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(Guid.NewGuid(), ancillaryFile.File.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));
            }
        }

        [Fact]
        public async Task Delete_FileNotFound()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.Delete(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, new List<Guid>
                {
                    ancillaryFile.File.Id,
                    chartFile.File.Id,
                    imageFile.File.Id
                });

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, ancillaryFile.Path()), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, imageFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(chartFile.File.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
                Assert.Null(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, dataFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, new List<Guid>
                {
                    ancillaryFile.File.Id,
                    dataFile.File.Id
                });

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertBadRequest(FileTypeInvalid);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Check that all the files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(dataFile.File.Id));
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(Guid.NewGuid(), new List<Guid>
                {
                    ancillaryFile.File.Id,
                    chartFile.File.Id,
                    imageFile.File.Id
                });

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Check that all the files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(chartFile.File.Id));

                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(imageFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(imageFile.File.Id));
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, new List<Guid>
                {
                    ancillaryFile.File.Id,
                    // Include an unknown id
                    Guid.NewGuid()
                });

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                // Check that the files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.Files.FindAsync(ancillaryFile.File.Id));
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddRangeAsync(release, amendmentRelease);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile);
                await contentDbContext.AddRangeAsync(ancillaryReleaseFile, ancillaryAmendmentReleaseFile,
                    chartAmendmentReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(amendmentRelease.Id, new List<Guid>
                {
                    ancillaryFile.Id,
                    chartFile.Id
                });

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, dataFile, imageFile, dataGuidanceFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(release.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
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
        }

        [Fact]
        public async Task DeleteAll_ReleaseNotFound()
        {
            await using var contentDbContext = InMemoryContentDbContext();

            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.DeleteAll(Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task DeleteAll_NoFiles()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(release.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
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

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, dataGuidanceFile.Path()))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(amendmentRelease.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateReleaseFiles, chartFile.Path()), Times.Once);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
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
        }

        [Fact]
        public async Task ListAll_NoFiles()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.ListAll(release.Id, Ancillary, Chart);

                Assert.True(result.IsRight);
                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task ListAll_ReleaseNotFound()
        {
            await using var contentDbContext = InMemoryContentDbContext();

            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.ListAll(Guid.NewGuid(), Ancillary, Chart);

            result.AssertNotFound();
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
                    ContentLength = 10240,
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
                    ContentLength = 10240,
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
                    ContentLength = 20480,
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
                    ContentLength = 30720,
                    Type = Image,
                    CreatedBy = new User
                    {
                        Email = "image@test.com"
                    },
                    Created = DateTime.UtcNow
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile1, ancillaryFile2,
                    chartFile, dataFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.ListAll(release.Id, Ancillary, Chart, Image);

                Assert.True(result.IsRight);

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
                Assert.Equal("", fileInfoList[2].Name);
                Assert.Equal("20 Kb", fileInfoList[2].Size);
                Assert.Equal(Chart, fileInfoList[2].Type);

                Assert.Equal(imageFile.File.Id, fileInfoList[3].Id);
                Assert.Equal("png", fileInfoList[3].Extension);
                Assert.Equal("image.png", fileInfoList[3].FileName);
                Assert.Equal("", fileInfoList[3].Name);
                Assert.Equal("30 Kb", fileInfoList[3].Size);
                Assert.Equal(Image, fileInfoList[3].Type);
            }
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
                    ContentLength = 10240,
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
                    ContentLength = 10240,
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile1, ancillaryFile2,
                    chartFile, dataFile, imageFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.GetAncillaryFiles(release.Id);

                Assert.True(result.IsRight);

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
                    ContentLength = 10240,
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.GetFile(releaseFile.ReleaseId, releaseFile.FileId);
                Assert.True(result.IsRight);

                var fileInfo = result.Right;
                Assert.Equal(releaseFile.Name, fileInfo.Name);
                Assert.Equal(releaseFile.FileId, fileInfo.Id);
                Assert.Equal("pdf", fileInfo.Extension);
                Assert.Equal("ancillary.pdf", fileInfo.FileName);
                Assert.Equal("10 Kb", fileInfo.Size);
                Assert.Equal(Ancillary, fileInfo.Type);
            }
        }

        [Fact]
        public async Task GetFile_NoRelease()
        {
            await using var contentDbContext = InMemoryContentDbContext();

            var service = SetupReleaseFileService(contentDbContext: contentDbContext);

            var result = await service.GetFile(Guid.NewGuid(), Guid.NewGuid());

            result.AssertNotFound();
        }

        [Fact]
        public async Task GetFile_NoReleaseFile()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.GetFile(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }
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
                    ContentType = "application/pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PrivateReleaseFiles, releaseFile.Path(),
                        It.IsAny<MemoryStream>(), null))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                blobStorageService.Verify(
                    mock =>
                        mock.DownloadToStream(PrivateReleaseFiles, releaseFile.Path(),
                        It.IsAny<MemoryStream>(), null),
                    Times.Once());

                Assert.Equal("application/pdf", result.Right.ContentType);
                Assert.Equal("ancillary.pdf", result.Right.FileDownloadName);
                Assert.IsType<MemoryStream>(result.Right.FileStream);
            }
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
                    ContentType = "application/pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PrivateReleaseFiles, releaseFile.Path(),
                        It.IsAny<MemoryStream>(), null))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                blobStorageService.Verify(
                    mock =>
                        mock.DownloadToStream(PrivateReleaseFiles, releaseFile.Path(),
                        It.IsAny<MemoryStream>(), null),
                    Times.Once());

                Assert.Equal("application/pdf", result.Right.ContentType);
                Assert.Equal("Ancillary 1.pdf", result.Right.FileDownloadName);
                Assert.IsType<MemoryStream>(result.Right.FileStream);
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.Stream(Guid.NewGuid(), releaseFile.File.Id);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Stream_FileNotFound()
        {
            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext);

                var result = await service.Stream(release.Id, Guid.NewGuid());

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task Stream_BlobDoesNotExist()
        {
            var release = new Release();

            var releaseFile = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    ContentType = "application/pdf",
                    Type = Ancillary
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService.SetupDownloadToStreamNotFound(PrivateReleaseFiles, releaseFile.Path());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.File.Id);

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task ZipFilesToStream_ValidFileTypes()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
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
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };
            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService
                .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile1.Path(), true);
            blobStorageService
                .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile2.Path(), true);
            blobStorageService
                .SetupDownloadToStream(PrivateReleaseFiles, releaseFile1.Path(), "Test data blob");
            blobStorageService
                .SetupDownloadToStream(PrivateReleaseFiles, releaseFile2.Path(), "Test ancillary blob");

            var subjectIds = releaseFiles
                .Where(rf => rf.File.SubjectId.HasValue)
                .Select(rf => rf.File.SubjectId.GetValueOrDefault())
                .ToList();

            var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(Strict);

            dataGuidanceFileWriter
                .Setup(
                    s => s.WriteToStream(
                        It.IsAny<Stream>(),
                        It.Is<Release>(r => r.Id == release.Id),
                        It.Is<IEnumerable<Guid>>(
                            ids => ids.All(id => subjectIds.Contains(id))
                        )
                    )
                )
                .Returns<Stream, Release, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
                .Callback<Stream, Release, IEnumerable<Guid>?>(
                    (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
                );

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var path = GenerateZipFilePath();
                var stream = System.IO.File.OpenWrite(path);

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataGuidanceFileWriter: dataGuidanceFileWriter.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    outputStream: stream,
                    fileIds: fileIds
                );

                MockUtils.VerifyAllMocks(blobStorageService, dataGuidanceFileWriter);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Equal(3, zip.Entries.Count);
                Assert.Equal("data/data.csv", zip.Entries[0].FullName);
                Assert.Equal("Test data blob", zip.Entries[0].Open().ReadToEnd());

                Assert.Equal("supporting-files/ancillary.pdf", zip.Entries[1].FullName);
                Assert.Equal("Test ancillary blob", zip.Entries[1].Open().ReadToEnd());

                // Data guidance is generated if there is at least one data file
                Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[2].FullName);
                Assert.Equal("Test data guidance blob", zip.Entries[2].Open().ReadToEnd());
            }
        }

        [Fact]
        public async Task ZipFilesToStream_DataGuidanceForMultipleDataFiles()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data-1.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data-2.csv",
                    Type = FileType.Data,
                    SubjectId = Guid.NewGuid()
                }
            };
            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService
                .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile1.Path(), true);
            blobStorageService
                .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile2.Path(), true);
            blobStorageService
                .SetupDownloadToStream(PrivateReleaseFiles, releaseFile1.Path(), "Test data 1 blob");
            blobStorageService
                .SetupDownloadToStream(PrivateReleaseFiles, releaseFile2.Path(), "Test data 2 blob");

            var subjectIds = releaseFiles
                .Where(rf => rf.File.SubjectId.HasValue)
                .Select(rf => rf.File.SubjectId.GetValueOrDefault())
                .ToList();

            var dataGuidanceFileWriter = new Mock<IDataGuidanceFileWriter>(Strict);

            dataGuidanceFileWriter
                .Setup(
                    s => s.WriteToStream(
                        It.IsAny<Stream>(),
                        It.Is<Release>(r => r.Id == release.Id),
                        It.Is<IEnumerable<Guid>>(
                            ids => ids.All(id => subjectIds.Contains(id))
                        )
                    )
                )
                .Returns<Stream, Release, IEnumerable<Guid>?>((stream, _, _) => Task.FromResult(stream))
                .Callback<Stream, Release, IEnumerable<Guid>?>(
                    (stream, _, _) => { stream.WriteText("Test data guidance blob"); }
                );

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var path = GenerateZipFilePath();
                var stream = System.IO.File.OpenWrite(path);

                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    dataGuidanceFileWriter: dataGuidanceFileWriter.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    outputStream: stream,
                    fileIds: fileIds
                );

                MockUtils.VerifyAllMocks(blobStorageService, dataGuidanceFileWriter);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Equal(3, zip.Entries.Count);
                Assert.Equal("data/data-1.csv", zip.Entries[0].FullName);
                Assert.Equal("Test data 1 blob", zip.Entries[0].Open().ReadToEnd());

                Assert.Equal("data/data-2.csv", zip.Entries[1].FullName);
                Assert.Equal("Test data 2 blob", zip.Entries[1].Open().ReadToEnd());

                // Data guidance is generated if there is at least one data file
                Assert.Equal("data-guidance/data-guidance.txt", zip.Entries[2].FullName);
                Assert.Equal("Test data guidance blob", zip.Entries[2].Open().ReadToEnd());
            }
        }

        [Fact]
        public async Task ZipFilesToStream_OrderedAlphabetically()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-2.pdf",
                    Type = Ancillary,
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-3.pdf",
                    Type = Ancillary
                }
            };
            var releaseFile3 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "test-1.pdf",
                    Type = Ancillary
                }
            };
            var releaseFiles = ListOf(releaseFile1, releaseFile2, releaseFile3);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = System.IO.File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            blobStorageService
                .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile1.Path(), true);
            blobStorageService
                .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile2.Path(), true);
            blobStorageService
                .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile3.Path(), true);
            blobStorageService
                .SetupDownloadToStream(PrivateReleaseFiles, releaseFile1.Path(), "Test 2 blob");
            blobStorageService
                .SetupDownloadToStream(PrivateReleaseFiles, releaseFile2.Path(), "Test 3 blob");
            blobStorageService
                .SetupDownloadToStream(PrivateReleaseFiles, releaseFile3.Path(), "Test 1 blob");

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    outputStream: stream,
                    fileIds: fileIds
                );

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Equal(3, zip.Entries.Count);
                Assert.Equal("supporting-files/test-1.pdf", zip.Entries[0].FullName);
                Assert.Equal("Test 1 blob", zip.Entries[0].Open().ReadToEnd());

                Assert.Equal("supporting-files/test-2.pdf", zip.Entries[1].FullName);
                Assert.Equal("Test 2 blob", zip.Entries[1].Open().ReadToEnd());

                Assert.Equal("supporting-files/test-3.pdf", zip.Entries[2].FullName);
                Assert.Equal("Test 3 blob", zip.Entries[2].Open().ReadToEnd());
            }
        }

        [Fact]
        public async Task ZipFilesToStream_FiltersInvalidFileTypes()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.meta.csv",
                    Type = Metadata,
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.zip",
                    Type = DataZip
                }
            };
            var releaseFile3 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "chart.jpg",
                    Type = Chart
                }
            };
            var releaseFile4 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image.jpg",
                    Type = Image
                }
            };

            var releaseFiles = ListOf(releaseFile1, releaseFile2, releaseFile3, releaseFile4);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = System.IO.File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    outputStream: stream,
                    fileIds: fileIds
                );

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                Assert.Empty(zip.Entries);
            }
        }

        [Fact]
        public async Task ZipFilesToStream_FiltersFilesNotInBlobStorage()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "data.pdf",
                    Type = FileType.Data,
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary.pdf",
                    Type = Ancillary
                }
            };

            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = System.IO.File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            // Files do not exist in blob storage
            blobStorageService.SetupCheckBlobExists(PrivateReleaseFiles, releaseFile1.Path(), false);
            blobStorageService.SetupCheckBlobExists(PrivateReleaseFiles, releaseFile2.Path(), false);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    outputStream: stream,
                    fileIds: fileIds
                );

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                Assert.Empty(zip.Entries);
            }
        }

        [Fact]
        public async Task ZipFilesToStream_FiltersFilesForOtherReleases()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            // Files are for other releases
            var releaseFile1 = new ReleaseFile
            {
                Release = new Release(),
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-1.pdf",
                    Type = Ancillary,
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = new Release(),
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-2.pdf",
                    Type = Ancillary
                }
            };

            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = System.IO.File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    outputStream: stream,
                    fileIds: fileIds
                );

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                Assert.Empty(zip.Entries);
            }
        }

        [Fact]
        public async Task ZipFilesToStream_Empty()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = System.IO.File.OpenWrite(path);

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = ListOf(Guid.NewGuid(), Guid.NewGuid());
                var result = await service.ZipFilesToStream(release.Id, stream, fileIds);

                MockUtils.VerifyAllMocks(blobStorageService);

                Assert.True(result.IsRight);

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Empty(zip.Entries);
            }
        }

        [Fact]
        public async Task ZipFilesToStream_Cancelled()
        {
            var release = new Release
            {
                Publication = new Publication
                {
                    Slug = "publication-slug"
                },
                Slug = "release-slug"
            };

            var releaseFile1 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-1.pdf",
                    Type = Ancillary
                }
            };
            var releaseFile2 = new ReleaseFile
            {
                Release = release,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "ancillary-2.pdf",
                    Type = Ancillary
                }
            };

            var releaseFiles = ListOf(releaseFile1, releaseFile2);

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.Releases.AddAsync(release);
                await contentDbContext.ReleaseFiles.AddRangeAsync(releaseFiles);
                await contentDbContext.SaveChangesAsync();
            }

            var path = GenerateZipFilePath();
            var stream = System.IO.File.OpenWrite(path);

            var tokenSource = new CancellationTokenSource();

            var blobStorageService = new Mock<IBlobStorageService>(Strict);

            // After the first file has completed, we cancel the request
            // to prevent the next file from being fetched.
            blobStorageService
                .SetupCheckBlobExists(PrivateReleaseFiles, releaseFile1.Path(), true);
            blobStorageService
                .SetupDownloadToStream(
                    container: PrivateReleaseFiles,
                    path: releaseFile1.Path(),
                    blobText: "Test ancillary blob",
                    cancellationToken: tokenSource.Token)
                .Callback(() => tokenSource.Cancel());

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(
                    contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var fileIds = releaseFiles.Select(file => file.FileId).ToList();

                var result = await service.ZipFilesToStream(
                    releaseId: release.Id,
                    outputStream: stream,
                    fileIds: fileIds,
                    cancellationToken: tokenSource.Token
                );

                MockUtils.VerifyAllMocks(blobStorageService);

                result.AssertRight();

                using var zip = ZipFile.OpenRead(path);

                // Entries are sorted alphabetically
                Assert.Single(zip.Entries);
                Assert.Equal("supporting-files/ancillary-1.pdf", zip.Entries[0].FullName);
                Assert.Equal("Test ancillary blob", zip.Entries[0].Open().ReadToEnd());
            }
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

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
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

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var updatedReleaseFile = await contentDbContext.ReleaseFiles
                    .AsQueryable()
                    .FirstAsync(rf =>
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

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
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

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var updatedReleaseFile = await contentDbContext.ReleaseFiles
                    .AsQueryable()
                    .FirstAsync(rf =>
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

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(releaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
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

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                var updatedReleaseFile = await contentDbContext.ReleaseFiles
                    .AsQueryable()
                    .FirstAsync(rf =>
                        rf.ReleaseId == releaseFile.ReleaseId
                        && rf.FileId == releaseFile.FileId);

                Assert.Equal("Old file title", updatedReleaseFile.Name);
                Assert.Equal("New file summary", updatedReleaseFile.Summary);
            }
        }

        [Fact]
        public async Task Update_NoRelease()
        {
            await using var contentDbContext = InMemoryContentDbContext();
            var service = SetupReleaseFileService(contentDbContext);

            var result = await service.Update(
                Guid.NewGuid(),
                Guid.NewGuid(),
                new ReleaseFileUpdateViewModel
                {
                    Title = "New file title",
                }
            );

            result.AssertNotFound();
        }

        [Fact]
        public async Task Update_NoReleaseFile()
        {
            var release = new Release();

            var contextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contextId))
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

                result.AssertNotFound();
            }
        }

        [Fact]
        public async Task UploadAncillary()
        {
            const string filename = "ancillary.pdf";

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var formFile = CreateFormFileMock(filename, "application/pdf").Object;
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);

            blobStorageService.Setup(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(release.Id, Ancillary))),
                    formFile
                )).Returns(Task.CompletedTask);

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileForUpload(formFile, Ancillary))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
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

                MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService);

                var fileInfo = result.AssertRight();

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(formFile, Ancillary), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, Ancillary))),
                        formFile
                    ), Times.Once);

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

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var releaseFile = await contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .SingleOrDefaultAsync(rf =>
                    rf.ReleaseId == release.Id
                    && rf.File.Filename == filename
                    && rf.File.Type == Ancillary
                );

                Assert.NotNull(releaseFile);
                var file = releaseFile!.File;

                Assert.Equal(10240, file.ContentLength);
                Assert.Equal("application/pdf", file.ContentType);
                Assert.InRange(DateTime.UtcNow.Subtract(file.Created.GetValueOrDefault()).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, file.CreatedById);
            }
        }

        [Fact]
        public async Task UploadChart()
        {
            const string filename = "chart.png";

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var formFile = CreateFormFileMock(filename, "image/png").Object;
            var blobStorageService = new Mock<IBlobStorageService>(Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(Strict);

            blobStorageService.Setup(mock =>
                mock.UploadFile(PrivateReleaseFiles,
                    It.Is<string>(path =>
                        path.Contains(FilesPath(release.Id, Chart))),
                    formFile
                )).Returns(Task.CompletedTask);

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileForUpload(formFile, Chart))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.UploadChart(release.Id, formFile);

                MockUtils.VerifyAllMocks(blobStorageService, fileUploadsValidatorService);

                Assert.True(result.IsRight);

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(formFile, Chart), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateReleaseFiles,
                        It.Is<string>(path =>
                            path.Contains(FilesPath(release.Id, Chart))),
                        formFile
                    ), Times.Once);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal("png", result.Right.Extension);
                Assert.Equal("chart.png", result.Right.FileName);
                Assert.Equal("", result.Right.Name);
                Assert.Equal("10 Kb", result.Right.Size);
                Assert.Equal(Chart, result.Right.Type);
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var releaseFile = await contentDbContext.ReleaseFiles
                    .Include(rf => rf.File)
                    .SingleOrDefaultAsync(rf =>
                        rf.ReleaseId == release.Id
                        && rf.File.Filename == filename
                        && rf.File.Type == Chart
                    );

                Assert.NotNull(releaseFile);
                var file = releaseFile!.File;

                Assert.Equal(10240, file.ContentLength);
                Assert.Equal("image/png", file.ContentType);
                Assert.InRange(DateTime.UtcNow.Subtract(file.Created!.Value).Milliseconds, 0, 1500);
                Assert.Equal(_user.Id, file.CreatedById);
            }
        }

        private string GenerateZipFilePath()
        {
            var path = Path.GetTempPath() + Guid.NewGuid() + ".zip";
            _filePaths.Add(path);

            return path;
        }

        private ReleaseFileService SetupReleaseFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext>? contentPersistenceHelper = null,
            IBlobStorageService? blobStorageService = null,
            IFileRepository? fileRepository = null,
            IFileUploadsValidatorService? fileUploadsValidatorService = null,
            IReleaseFileRepository? releaseFileRepository = null,
            IDataGuidanceFileWriter? dataGuidanceFileWriter = null,
            IUserService? userService = null)
        {
            contentDbContext.Users.Add(_user);
            contentDbContext.SaveChanges();

            return new ReleaseFileService(
                contentDbContext,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? Mock.Of<IBlobStorageService>(Strict),
                fileRepository ?? new FileRepository(contentDbContext),
                fileUploadsValidatorService ?? Mock.Of<IFileUploadsValidatorService>(Strict),
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                dataGuidanceFileWriter ?? Mock.Of<IDataGuidanceFileWriter>(Strict),
                userService ?? MockUtils.AlwaysTrueUserService(_user.Id).Object
            );
        }
    }
}
