using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ReleaseFileServiceTests
    {
        [Fact]
        public async Task Delete()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "chart.png",
                    ReleaseFileType = ReleaseFileTypes.Chart,
                    Release = release
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, ancillaryFile.ReleaseFileReference.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.Null(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.ReleaseFileReference.Id));

                // Check that other files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(chartFile.ReleaseFileReference.Id));
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

            var ancillaryFile = new ReleaseFileReference
            {
                Filename = "ancillary.pdf",
                ReleaseFileType = ReleaseFileTypes.Ancillary,
                Release = release
            };

            var releaseFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = ancillaryFile
            };

            var amendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                ReleaseFileReference = ancillaryFile
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
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.Id));
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
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "data.csv",
                    ReleaseFileType = ReleaseFileTypes.Data,
                    Release = release,
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

                var result = await service.Delete(release.Id, dataFile.ReleaseFileReference.Id);

                Assert.True(result.IsLeft);
                ValidationTestUtil.AssertValidationProblem(result.Left, FileTypeInvalid);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(dataFile.ReleaseFileReference.Id));
            }
        }

        [Fact]
        public async Task Delete_ReleaseNotFound()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
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

                var result = await service.Delete(Guid.NewGuid(), ancillaryFile.ReleaseFileReference.Id);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.ReleaseFileReference.Id));
            }
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

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async Task Delete_MultipleFiles()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "chart.png",
                    ReleaseFileType = ReleaseFileTypes.Chart,
                    Release = release
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName,
                        $"{release.Id}/chart/{chartFile.ReleaseFileReference.Id}"))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(release.Id, new List<Guid>
                {
                    ancillaryFile.ReleaseFileReference.Id,
                    chartFile.ReleaseFileReference.Id
                });

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName,
                        $"{release.Id}/chart/{chartFile.ReleaseFileReference.Id}"), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.Null(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.ReleaseFileReference.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.Null(await contentDbContext.ReleaseFileReferences.FindAsync(chartFile.ReleaseFileReference.Id));
            }
        }

        [Fact]
        public async Task Delete_MultipleFilesWithAnInvalidFileType()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
                }
            };

            var dataFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "data.csv",
                    ReleaseFileType = ReleaseFileTypes.Data,
                    Release = release,
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
                    ancillaryFile.ReleaseFileReference.Id,
                    dataFile.ReleaseFileReference.Id
                });

                Assert.True(result.IsLeft);
                ValidationTestUtil.AssertValidationProblem(result.Left, FileTypeInvalid);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that all the files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.ReleaseFileReference.Id));
            }
        }

        [Fact]
        public async Task Delete_MultipleFilesWithReleaseNotFound()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "chart.png",
                    ReleaseFileType = ReleaseFileTypes.Chart,
                    Release = release
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Delete(Guid.NewGuid(), new List<Guid>
                {
                    ancillaryFile.ReleaseFileReference.Id,
                    chartFile.ReleaseFileReference.Id
                });

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the file remains untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.ReleaseFileReference.Id));
            }
        }

        [Fact]
        public async Task Delete_MultipleFilesWithAFileNotFound()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
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
                    ancillaryFile.ReleaseFileReference.Id,
                    // Include an unknown id
                    Guid.NewGuid()
                });

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.ReleaseFileReference.Id));
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

            var ancillaryFile = new ReleaseFileReference
            {
                Filename = "ancillary.pdf",
                ReleaseFileType = ReleaseFileTypes.Ancillary,
                Release = release
            };

            var chartFile = new ReleaseFileReference
            {
                Filename = "chart.png",
                ReleaseFileType = ReleaseFileTypes.Chart,
                Release = amendmentRelease
            };

            var ancillaryReleaseFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = ancillaryFile
            };

            var ancillaryAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                ReleaseFileReference = ancillaryFile
            };

            var chartAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                ReleaseFileReference = chartFile
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
                    mock.DeleteBlob(PrivateFilesContainerName,
                        $"{amendmentRelease.Id}/chart/{chartFile.Id}"))
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
                    mock.DeleteBlob(PrivateFilesContainerName,
                        $"{amendmentRelease.Id}/chart/{chartFile.Id}"), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the ancillary file is unlinked from the amendment
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryAmendmentReleaseFile.Id));

                // Check that the ancillary file and link to the previous version remain untouched
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryReleaseFile.Id));

                // Check that the chart file and link to the amendment are removed
                Assert.Null(await contentDbContext.ReleaseFileReferences.FindAsync(chartFile.Id));
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
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "chart.png",
                    ReleaseFileType = ReleaseFileTypes.Chart,
                    Release = release
                }
            };

            var dataFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "data.csv",
                    ReleaseFileType = ReleaseFileTypes.Data,
                    Release = release,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, dataFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"))
                .Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName,
                        $"{release.Id}/chart/{chartFile.ReleaseFileReference.Id}"))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(release.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName,
                        $"{release.Id}/chart/{chartFile.ReleaseFileReference.Id}"), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryFile.Id));
                Assert.Null(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.ReleaseFileReference.Id));

                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartFile.Id));
                Assert.Null(await contentDbContext.ReleaseFileReferences.FindAsync(chartFile.ReleaseFileReference.Id));

                // Check that data files remain untouched
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(dataFile.Id));
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(dataFile.ReleaseFileReference.Id));
            }
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

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
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
        }

        [Fact]
        public async Task DeleteAll_FileFromAmendment()
        {
            var release = new Release();

            var amendmentRelease = new Release
            {
                PreviousVersionId = release.Id
            };

            var ancillaryFile = new ReleaseFileReference
            {
                Filename = "ancillary.pdf",
                ReleaseFileType = ReleaseFileTypes.Ancillary,
                Release = release
            };

            var chartFile = new ReleaseFileReference
            {
                Filename = "chart.png",
                ReleaseFileType = ReleaseFileTypes.Chart,
                Release = amendmentRelease
            };

            var ancillaryReleaseFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = ancillaryFile
            };

            var ancillaryAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                ReleaseFileReference = ancillaryFile
            };

            var chartAmendmentReleaseFile = new ReleaseFile
            {
                Release = amendmentRelease,
                ReleaseFileReference = chartFile
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile);
                await contentDbContext.AddRangeAsync(ancillaryReleaseFile, ancillaryAmendmentReleaseFile,
                    chartAmendmentReleaseFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName,
                        $"{amendmentRelease.Id}/chart/{chartFile.Id}"))
                .Returns(Task.CompletedTask);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.DeleteAll(amendmentRelease.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.DeleteBlob(PrivateFilesContainerName,
                        $"{amendmentRelease.Id}/chart/{chartFile.Id}"), Times.Once);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                // Check that the ancillary file is unlinked from the amendment
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(ancillaryAmendmentReleaseFile.Id));

                // Check that the ancillary file and link to the previous version remain untouched
                Assert.NotNull(
                    await contentDbContext.ReleaseFileReferences.FindAsync(ancillaryFile.Id));
                Assert.NotNull(await contentDbContext.ReleaseFiles.FindAsync(ancillaryReleaseFile.Id));

                // Check that the chart file and link to the amendment are removed
                Assert.Null(await contentDbContext.ReleaseFileReferences.FindAsync(chartFile.Id));
                Assert.Null(await contentDbContext.ReleaseFiles.FindAsync(chartAmendmentReleaseFile.Id));
            }
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

                var result = await service.ListAll(release.Id, ReleaseFileTypes.Ancillary, ReleaseFileTypes.Chart);

                Assert.True(result.IsRight);
                Assert.Empty(result.Right);
            }
        }

        [Fact]
        public async Task ListAll_ReleaseNotFound()
        {
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            await using (var contentDbContext = InMemoryApplicationDbContext())
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.ListAll(Guid.NewGuid(), ReleaseFileTypes.Ancillary, ReleaseFileTypes.Chart);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async Task ListAll()
        {
            var release = new Release();

            var ancillaryFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
                }
            };

            var chartFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "chart.png",
                    ReleaseFileType = ReleaseFileTypes.Chart,
                    Release = release
                }
            };

            var dataFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "data.csv",
                    ReleaseFileType = ReleaseFileTypes.Data,
                    Release = release,
                    SubjectId = Guid.NewGuid()
                }
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.AddRangeAsync(ancillaryFile, chartFile, dataFile);
                await contentDbContext.SaveChangesAsync();
            }

            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"))
                .ReturnsAsync(true);

            blobStorageService.Setup(mock =>
                    mock.CheckBlobExists(PrivateFilesContainerName,
                        $"{release.Id}/chart/{chartFile.ReleaseFileReference.Id}"))
                .ReturnsAsync(true);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"))
                .ReturnsAsync(new BlobInfo(
                    path: $"{release.Id}/ancillary/ancillary.pdf",
                    size: "10 Kb",
                    contentType: "application/pdf",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>
                    {
                        {BlobInfoExtensions.NameKey, "Ancillary Test File"},
                    },
                    created: null));

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateFilesContainerName, $"{release.Id}/chart/{chartFile.ReleaseFileReference.Id}"))
                .ReturnsAsync(new BlobInfo(
                    path: $"{release.Id}/chart/{chartFile.ReleaseFileReference.Id}",
                    size: "20 Kb",
                    contentType: "image/png",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>
                    {
                        {BlobInfoExtensions.NameKey, "chart.png"}
                    },
                    created: null));

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.ListAll(release.Id, ReleaseFileTypes.Ancillary, ReleaseFileTypes.Chart);

                Assert.True(result.IsRight);

                blobStorageService.Verify(mock =>
                    mock.CheckBlobExists(PrivateFilesContainerName, It.IsAny<string>()), Times.Exactly(2));

                blobStorageService.Verify(mock =>
                    mock.GetBlob(PrivateFilesContainerName, It.IsAny<string>()), Times.Exactly(2));

                var fileInfoList = result.Right.ToList();
                Assert.Equal(2, fileInfoList.Count);

                Assert.Equal(ancillaryFile.ReleaseFileReference.Id, fileInfoList[0].Id);
                Assert.Equal("pdf", fileInfoList[0].Extension);
                Assert.Equal("Ancillary Test File", fileInfoList[0].Name);
                Assert.Equal($"{release.Id}/ancillary/ancillary.pdf", fileInfoList[0].Path);
                Assert.Equal("10 Kb", fileInfoList[0].Size);
                Assert.Equal(ReleaseFileTypes.Ancillary, fileInfoList[0].Type);
                Assert.Equal("ancillary.pdf", fileInfoList[0].FileName);

                Assert.Equal(chartFile.ReleaseFileReference.Id, fileInfoList[1].Id);
                Assert.Equal("", fileInfoList[1].Extension);
                Assert.Equal("chart.png", fileInfoList[1].Name);
                Assert.Equal($"{release.Id}/chart/{chartFile.ReleaseFileReference.Id}", fileInfoList[1].Path);
                Assert.Equal("20 Kb", fileInfoList[1].Size);
                Assert.Equal(ReleaseFileTypes.Chart, fileInfoList[1].Type);
                Assert.Equal(chartFile.ReleaseFileReference.Id.ToString(), fileInfoList[1].FileName);
            }
        }

        [Fact]
        public async Task ListPublicFilesPreview()
        {
            // TODO EES-1490
        }

        [Fact]
        public async Task Stream()
        {
            var release = new Release();

            var releaseFile = new ReleaseFile
            {
                Release = release,
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
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
                    mock.GetBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"))
                .ReturnsAsync(blob);

            blobStorageService.Setup(mock =>
                    mock.DownloadToStream(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf",
                        It.IsAny<MemoryStream>()))
                .ReturnsAsync(new MemoryStream());

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object);

                var result = await service.Stream(release.Id, releaseFile.ReleaseFileReference.Id);

                Assert.True(result.IsRight);

                blobStorageService.Verify(
                    mock => mock.GetBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf"),
                    Times.Once());

                blobStorageService.Verify(
                    mock => mock.DownloadToStream(PrivateFilesContainerName, $"{release.Id}/ancillary/ancillary.pdf",
                        It.IsAny<MemoryStream>()), Times.Once());

                Assert.Equal("application/pdf", result.Right.ContentType);
                Assert.Equal("ancillary.pdf", result.Right.FileDownloadName);
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
                ReleaseFileReference = new ReleaseFileReference
                {
                    Filename = "ancillary.pdf",
                    ReleaseFileType = ReleaseFileTypes.Ancillary,
                    Release = release
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

                var result = await service.Stream(Guid.NewGuid(), releaseFile.ReleaseFileReference.Id);

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
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

                Assert.True(result.IsLeft);
                Assert.IsType<NotFoundResult>(result.Left);
            }
        }

        [Fact]
        public async Task UploadAncillary()
        {
            const string fileName = "ancillary.pdf";
            const string uploadName = "Ancillary Test File";

            var release = new Release();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }

            var formFile = CreateFormFileMock(fileName).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                mock.UploadFile(PrivateFilesContainerName,
                    $"{release.Id}/ancillary/{fileName}",
                    formFile,
                    It.Is<IBlobStorageService.UploadFileOptions>(options =>
                        options.MetaValues[BlobInfoExtensions.NameKey] == uploadName)
                )).Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/{fileName}"))
                .ReturnsAsync(new BlobInfo(
                    path: $"{release.Id}/ancillary/{fileName}",
                    size: "10 Kb",
                    contentType: "application/pdf",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>
                    {
                        {BlobInfoExtensions.NameKey, uploadName}
                    },
                    created: null));

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateUploadFileType(formFile, ReleaseFileTypes.Ancillary))
                .ReturnsAsync(Unit.Instance);

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileForUpload(release.Id, formFile, ReleaseFileTypes.Ancillary, false))
                .ReturnsAsync(Unit.Instance);

            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileUploadName(uploadName))
                .ReturnsAsync(Unit.Instance);

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);

                var result = await service.UploadAncillary(release.Id, formFile, uploadName);

                Assert.True(result.IsRight);

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateUploadFileType(formFile, ReleaseFileTypes.Ancillary), Times.Once);

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(release.Id, formFile, ReleaseFileTypes.Ancillary,
                        false), Times.Once);

                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileUploadName(uploadName), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateFilesContainerName,
                        $"{release.Id}/ancillary/{fileName}",
                        formFile,
                        It.Is<IBlobStorageService.UploadFileOptions>(options =>
                            options.MetaValues[BlobInfoExtensions.NameKey] == uploadName)
                    ), Times.Once);

                blobStorageService.Verify(mock =>
                    mock.GetBlob(PrivateFilesContainerName, $"{release.Id}/ancillary/{fileName}"), Times.Once);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal("pdf", result.Right.Extension);
                Assert.Equal(uploadName, result.Right.Name);
                Assert.Equal($"{release.Id}/ancillary/{fileName}", result.Right.Path);
                Assert.Equal("10 Kb", result.Right.Size);
                Assert.Equal(ReleaseFileTypes.Ancillary, result.Right.Type);
                Assert.Equal(fileName, result.Right.FileName);
            }

            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var file = await contentDbContext.ReleaseFileReferences.SingleOrDefaultAsync(f =>
                    f.ReleaseId == release.Id
                    && f.Filename == fileName
                    && f.ReleaseFileType == ReleaseFileTypes.Ancillary
                );

                Assert.NotNull(file);

                Assert.NotNull(await contentDbContext.ReleaseFiles.SingleOrDefaultAsync(rf =>
                    rf.ReleaseId == release.Id
                    && rf.ReleaseFileReferenceId == file.Id
                ));
            }
        }

        [Fact]
        public async Task UploadChart()
        {
            var fileGuid = Guid.NewGuid();
            const string fileName = "chart.png";
        
            var release = new Release();
        
            var contentDbContextId = Guid.NewGuid().ToString();
        
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                await contentDbContext.AddAsync(release);
                await contentDbContext.SaveChangesAsync();
            }
        
            var formFile = CreateFormFileMock(fileName).Object;
            var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
            var fileUploadsValidatorService = new Mock<IFileUploadsValidatorService>(MockBehavior.Strict);

            blobStorageService.Setup(mock =>
                mock.UploadFile(PrivateFilesContainerName,
                    It.Is<string>(path => path.Contains($"{release.Id}/chart/")),
                    formFile,
                    It.Is<IBlobStorageService.UploadFileOptions>(options =>
                        options.MetaValues[BlobInfoExtensions.NameKey] == fileName)
                )).Returns(Task.CompletedTask);

            blobStorageService.Setup(mock =>
                    mock.GetBlob(PrivateFilesContainerName,
                        It.Is<string>(path => path.Contains($"{release.Id}/chart/"))))
                .ReturnsAsync(new BlobInfo(
                    $"{release.Id}/chart/{fileGuid}",
                    size: "20 Kb",
                    contentType: "image/png",
                    contentLength: 0L,
                    meta: new Dictionary<string, string>
                    {
                        {BlobInfoExtensions.NameKey, fileName}
                    },
                    created: null));
        
            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateUploadFileType(formFile, ReleaseFileTypes.Chart))
                .ReturnsAsync(Unit.Instance);
        
            fileUploadsValidatorService.Setup(mock =>
                    mock.ValidateFileForUpload(release.Id, formFile, ReleaseFileTypes.Chart, false))
                .ReturnsAsync(Unit.Instance);
        
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupReleaseFileService(contentDbContext: contentDbContext,
                    blobStorageService: blobStorageService.Object,
                    fileUploadsValidatorService: fileUploadsValidatorService.Object);
        
                var result = await service.UploadChart(release.Id, formFile);
        
                Assert.True(result.IsRight);
        
                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateUploadFileType(formFile, ReleaseFileTypes.Chart), Times.Once);
        
                fileUploadsValidatorService.Verify(mock =>
                    mock.ValidateFileForUpload(release.Id, formFile, ReleaseFileTypes.Chart,
                        false), Times.Once);
        
                blobStorageService.Verify(mock =>
                    mock.UploadFile(PrivateFilesContainerName,
                        It.Is<string>(path => path.Contains($"{release.Id}/chart/")),
                        formFile,
                        It.Is<IBlobStorageService.UploadFileOptions>(options =>
                            options.MetaValues[BlobInfoExtensions.NameKey] == fileName)
                    ), Times.Once);
        
                blobStorageService.Verify(mock =>
                    mock.GetBlob(PrivateFilesContainerName,
                        It.Is<string>(path => path.Contains($"{release.Id}/chart/"))), Times.Once);

                Assert.True(result.Right.Id.HasValue);
                Assert.Equal(string.Empty, result.Right.Extension);
                Assert.Equal(fileName, result.Right.Name);
                Assert.Equal($"{release.Id}/chart/{fileGuid}", result.Right.Path);
                Assert.Equal("20 Kb", result.Right.Size);
                Assert.Equal(ReleaseFileTypes.Chart, result.Right.Type);
                Assert.Equal(fileGuid.ToString(), result.Right.FileName);
            }
            
            await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
            {
                var file = await contentDbContext.ReleaseFileReferences.SingleOrDefaultAsync(f =>
                    f.ReleaseId == release.Id
                    && f.Filename == fileName
                    && f.ReleaseFileType == ReleaseFileTypes.Chart
                );

                Assert.NotNull(file);

                Assert.NotNull(await contentDbContext.ReleaseFiles.SingleOrDefaultAsync(rf =>
                    rf.ReleaseId == release.Id
                    && rf.ReleaseFileReferenceId == file.Id
                ));
            }
        }

        private static Mock<IFormFile> CreateFormFileMock(string fileName)
        {
            var formFile = new Mock<IFormFile>();

            formFile.SetupGet(f => f.FileName)
                .Returns(fileName);

            return formFile;
        }

        private static ReleaseFileService SetupReleaseFileService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> contentPersistenceHelper = null,
            IBlobStorageService blobStorageService = null,
            IFileRepository fileRepository = null,
            IFileUploadsValidatorService fileUploadsValidatorService = null,
            IReleaseFileRepository releaseFileRepository = null,
            IUserService userService = null)
        {
            return new ReleaseFileService(
                contentDbContext ?? new Mock<ContentDbContext>().Object,
                contentPersistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                blobStorageService ?? new Mock<IBlobStorageService>().Object,
                fileRepository ?? new FileRepository(contentDbContext),
                fileUploadsValidatorService ?? new Mock<IFileUploadsValidatorService>().Object,
                releaseFileRepository ?? new ReleaseFileRepository(contentDbContext),
                userService ?? MockUtils.AlwaysTrueUserService().Object
            );
        }
    }
}