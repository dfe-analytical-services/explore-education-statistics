using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ValidationTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyPublishingStrategy;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyServiceTests
    {
        [Fact]
        public async Task CreateMethodology()
        {
            var publication = new Publication();

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Publications.AddAsync(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var repository = new Mock<IMethodologyRepository>(Strict);

                var service = SetupMethodologyService(
                    context,
                    methodologyRepository: repository.Object);

                var createdMethodology = new Methodology
                {
                    Id = Guid.NewGuid()
                };

                repository
                    .Setup(s => s.CreateMethodologyForPublication(publication.Id))
                    .ReturnsAsync(createdMethodology);

                var result = await service.CreateMethodology(publication.Id);
                VerifyAllMocks(repository);

                var viewModel = result.AssertRight();
                Assert.Equal(createdMethodology.Id, viewModel.Id);
            }
        }

        [Fact]
        public async Task GetSummary()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent(),
                InternalReleaseNote = "Test approval",
                Published = new DateTime(2020, 5, 25),
                PublishingStrategy = Immediately,
                Slug = "pupil-absence-statistics-methodology",
                Status = Approved,
                Title = "Pupil absence statistics: methodology"
            };

            var owningPublication = new Publication
            {
                Title = "Owning publication",
                Methodologies = AsList(
                    new PublicationMethodology
                    {
                        MethodologyParent = methodology.MethodologyParent,
                        Owner = true
                    }
                )
            };

            var adoptingPublication1 = new Publication
            {
                Title = "Test publication",
                Methodologies = AsList(
                    new PublicationMethodology
                    {
                        MethodologyParent = methodology.MethodologyParent,
                        Owner = false
                    }
                )
            };

            var adoptingPublication2 = new Publication
            {
                Title = "Test publication",
                Methodologies = AsList(
                    new PublicationMethodology
                    {
                        MethodologyParent = methodology.MethodologyParent,
                        Owner = false
                    }
                )
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.Publications.AddRangeAsync(owningPublication, adoptingPublication1, adoptingPublication2);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context);

                var viewModel = (await service.GetSummary(methodology.Id)).AssertRight();

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Equal(methodology.InternalReleaseNote, viewModel.InternalReleaseNote);
                Assert.Equal(methodology.Published, viewModel.Published);
                Assert.Equal(methodology.Status, viewModel.Status);
                Assert.Equal(methodology.Title, viewModel.Title);
                
                Assert.Equal(owningPublication.Id, viewModel.Publication.Id);
                Assert.Equal(owningPublication.Title, viewModel.Publication.Title);

                Assert.Equal(2, viewModel.OtherPublications.Count);
                Assert.Equal(adoptingPublication1.Id, viewModel.OtherPublications[0].Id);
                Assert.Equal(adoptingPublication1.Title, viewModel.OtherPublications[0].Title);
                Assert.Equal(adoptingPublication2.Id, viewModel.OtherPublications[1].Id);
                Assert.Equal(adoptingPublication2.Title, viewModel.OtherPublications[1].Title);
            }
        }

        [Fact]
        public async Task UpdateMethodology()
        {
            var publication = new Publication
            {
                Title = "Test publication"
            };

            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(
                        new PublicationMethodology
                        {
                            Publication = publication,
                            Owner = true
                        }
                    )
                },
                PublishingStrategy = Immediately,
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Pupil absence statistics: methodology"
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Title = "Pupil absence statistics (updated): methodology"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodology.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodology.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyRepository: methodologyRepository.Object,
                    publishingService: publishingService.Object);

                var viewModel = (await service.UpdateMethodology(methodology.Id, request)).Right;

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Null(viewModel.InternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
                Assert.Equal(publication.Id, viewModel.Publication.Id);
                Assert.Equal(publication.Title, viewModel.Publication.Title);
                Assert.Empty(viewModel.OtherPublications);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.Null(model.Published);
                Assert.Equal(Draft, model.Status);
                Assert.Equal(Immediately, model.PublishingStrategy);
                Assert.Equal("pupil-absence-statistics-updated-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
            }

            VerifyAllMocks(contentService, imageService, methodologyRepository, publishingService);
        }

        [Fact]
        public async Task UpdateMethodology_MethodologyHasImages()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(
                        new PublicationMethodology
                        {
                            Publication = new Publication
                            {
                                Title = "Test publication"
                            },
                            Owner = true
                        }
                    )
                },
                PublishingStrategy = Immediately,
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Pupil absence statistics: methodology"
            };

            var imageFile1 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = FileType.Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = FileType.Image
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Title = "Pupil absence statistics (updated): methodology"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodology.Id))
                .ReturnsAsync(new List<HtmlBlock>
                {
                    new HtmlBlock
                    {
                        Body = $@"
    <img src=""/api/methodologies/{{methodologyId}}/images/{imageFile1.File.Id}""/>
    <img src=""/api/methodologies/{{methodologyId}}/images/{imageFile2.File.Id}""/>"
                    }
                });

            methodologyRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodology.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyRepository: methodologyRepository.Object,
                    publishingService: publishingService.Object);

                var viewModel = (await service.UpdateMethodology(methodology.Id, request)).Right;

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Null(viewModel.InternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.Null(model.Published);
                Assert.Equal(Draft, model.Status);
                Assert.Equal(Immediately, model.PublishingStrategy);
                Assert.Equal("pupil-absence-statistics-updated-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
            }

            VerifyAllMocks(contentService, imageService, methodologyRepository, publishingService);
        }

        [Fact]
        public async Task UpdateMethodology_MethodologyHasUnusedImages()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(
                        new PublicationMethodology
                        {
                            Publication = new Publication
                            {
                                Title = "Test publication"
                            },
                            Owner = true
                        }
                    )
                },
                PublishingStrategy = Immediately,
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Pupil absence statistics: methodology"
            };

            var imageFile1 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image1.png",
                    Type = FileType.Image
                }
            };

            var imageFile2 = new MethodologyFile
            {
                Methodology = methodology,
                File = new File
                {
                    RootPath = Guid.NewGuid(),
                    Filename = "image2.png",
                    Type = FileType.Image
                }
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Title = "Pupil absence statistics (updated): methodology"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.MethodologyFiles.AddRangeAsync(imageFile1, imageFile2);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodology.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            imageService.Setup(mock =>
                    mock.Delete(methodology.Id, new List<Guid>
                    {
                        imageFile1.File.Id,
                        imageFile2.File.Id
                    }))
                .ReturnsAsync(Unit.Instance);

            methodologyRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodology.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyRepository: methodologyRepository.Object,
                    publishingService: publishingService.Object);

                var viewModel = (await service.UpdateMethodology(methodology.Id, request)).Right;

                imageService.Verify(mock =>
                    mock.Delete(methodology.Id, new List<Guid>
                    {
                        imageFile1.File.Id,
                        imageFile2.File.Id
                    }), Times.Once);

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Null(viewModel.InternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.Null(model.Published);
                Assert.Equal(Draft, model.Status);
                Assert.Equal(Immediately, model.PublishingStrategy);
                Assert.Equal("pupil-absence-statistics-updated-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
            }

            VerifyAllMocks(contentService, imageService, methodologyRepository, publishingService);
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingImmediateStrategy()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(
                        new PublicationMethodology
                        {
                            Publication = new Publication
                            {
                                Title = "Test publication"
                            },
                            Owner = true
                        }
                    )
                },
                PublishingStrategy = Immediately,
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Pupil absence statistics: methodology"
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = Immediately,
                Status = Approved,
                Title = "Pupil absence statistics (updated): methodology"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodology.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            // Methodology is not publicly accessible to begin with when checking if the slug should be updated.
            // Methodology is publicly accessible later after its publishing strategy and status are updated
            methodologyRepository.SetupSequence(mock =>
                    mock.IsPubliclyAccessible(methodology.Id))
                .ReturnsAsync(false)
                .ReturnsAsync(true);

            publishingService.Setup(mock => mock.PublishMethodologyFiles(methodology.Id))
                .ReturnsAsync(Unit.Instance);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyRepository: methodologyRepository.Object,
                    publishingService: publishingService.Object);

                var viewModel = (await service.UpdateMethodology(methodology.Id, request)).Right;

                methodologyRepository.Verify(mock =>
                    mock.IsPubliclyAccessible(methodology.Id), Times.Exactly(2));

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Equal("Test approval", viewModel.InternalReleaseNote);
                Assert.True(viewModel.Published.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(viewModel.Published.Value).Milliseconds, 0, 1500);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.True(model.Published.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Published.Value).Milliseconds, 0, 1500);
                Assert.Equal(Approved, model.Status);
                Assert.Equal(Immediately, model.PublishingStrategy);
                Assert.Equal("pupil-absence-statistics-updated-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
            }

            VerifyAllMocks(contentService, imageService, methodologyRepository, publishingService);
        }

        [Fact]
        public async Task UpdateMethodology_ApprovingUsingWithReleaseStrategy_NonLiveRelease()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(
                        new PublicationMethodology
                        {
                            Publication = new Publication
                            {
                                Title = "Test publication"
                            },
                            Owner = true
                        }
                    )
                },
                PublishingStrategy = Immediately,
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Pupil absence statistics: methodology"
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = "Test approval",
                PublishingStrategy = WithRelease,
                // TODO SOW4 EES-2164 Add a ScheduledWithRelease here when implementing it
                Status = Approved,
                Title = "Pupil absence statistics (updated): methodology"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodology.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            // Methodology is not publicly accessible to begin with when checking if the slug should be updated.
            // Methodology is not publicly accessible later after its publishing strategy and status are updated
            // due to the Release not being live
            methodologyRepository.SetupSequence(mock =>
                    mock.IsPubliclyAccessible(methodology.Id))
                .ReturnsAsync(false)
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyRepository: methodologyRepository.Object,
                    publishingService: publishingService.Object);

                var viewModel = (await service.UpdateMethodology(methodology.Id, request)).Right;

                methodologyRepository.Verify(mock =>
                    mock.IsPubliclyAccessible(methodology.Id), Times.Exactly(2));

                Assert.Equal(methodology.Id, viewModel.Id);
                Assert.Equal("Test approval", viewModel.InternalReleaseNote);
                Assert.Null(viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.Null(model.Published);
                Assert.Equal(Approved, model.Status);
                Assert.Equal(WithRelease, model.PublishingStrategy);
                Assert.Equal("pupil-absence-statistics-updated-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
            }

            VerifyAllMocks(contentService, imageService, methodologyRepository, publishingService);
        }

        [Fact]
        // TODO SOW4 EES-2166 EES-2200:
        // Once amendments are available updating an approved methodology shouldn't be possible
        // but un-approving should be provided the Methodology is not publicly accessible
        public async Task UpdateMethodology_AlreadyPublished()
        {
            var methodology = new Methodology
            {
                MethodologyParent = new MethodologyParent
                {
                    Publications = AsList(
                        new PublicationMethodology
                        {
                            Publication = new Publication
                            {
                                Title = "Test publication"
                            },
                            Owner = true
                        }
                    )
                },
                InternalReleaseNote = "Test approval",
                Published = new DateTime(2020, 5, 25),
                PublishingStrategy = Immediately,
                Slug = "pupil-absence-statistics-methodology",
                Status = Approved,
                Title = "Pupil absence statistics: methodology"
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
                Status = Approved,
                Title = "Pupil absence statistics (updated): methodology"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.Methodologies.AddAsync(methodology);
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodology.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodology.Id))
                .ReturnsAsync(true);

            publishingService.Setup(mock => mock.PublishMethodologyFiles(methodology.Id))
                .ReturnsAsync(Unit.Instance);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyRepository: methodologyRepository.Object,
                    publishingService: publishingService.Object);

                var viewModel = (await service.UpdateMethodology(methodology.Id, request)).Right;

                Assert.Equal(methodology.Id, viewModel.Id);
                // Original release note is not cleared if the update is not altering it
                Assert.Equal(methodology.InternalReleaseNote, viewModel.InternalReleaseNote);
                Assert.Equal(methodology.Published, viewModel.Published);
                Assert.Equal(request.Status, viewModel.Status);
                Assert.Equal(request.Title, viewModel.Title);
            }

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var model = await context.Methodologies.FindAsync(methodology.Id);

                Assert.Equal(methodology.Published, model.Published);
                Assert.Equal(Approved, model.Status);
                Assert.Equal(Immediately, model.PublishingStrategy);
                // Slug remains unchanged
                Assert.Equal("pupil-absence-statistics-methodology", model.Slug);
                Assert.True(model.Updated.HasValue);
                Assert.InRange(DateTime.UtcNow.Subtract(model.Updated.Value).Milliseconds, 0, 1500);
            }

            VerifyAllMocks(contentService, imageService, methodologyRepository, publishingService);
        }

        [Fact]
        public async Task UpdateMethodology_SlugNotUnique()
        {
            var methodology1 = new Methodology
            {
                PublishingStrategy = Immediately,
                Slug = "pupil-absence-statistics-methodology",
                Status = Draft,
                Title = "Methodology title"
            };

            var methodology2 = new Methodology
            {
                InternalReleaseNote = "Test approval",
                Published = new DateTime(2020, 5, 25),
                PublishingStrategy = Immediately,
                Slug = "pupil-exclusion-statistics-methodology",
                Status = Draft,
                Title = "Pupil exclusion statistics: methodology"
            };

            var request = new MethodologyUpdateRequest
            {
                LatestInternalReleaseNote = null,
                PublishingStrategy = Immediately,
                Status = Draft,
                Title = "Pupil exclusion statistics: methodology"
            };

            var contentDbContextId = Guid.NewGuid().ToString();

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                await context.AddRangeAsync(new List<Methodology>
                {
                    methodology1, methodology2
                });
                await context.SaveChangesAsync();
            }

            var contentService = new Mock<IMethodologyContentService>(Strict);
            var imageService = new Mock<IMethodologyImageService>(Strict);
            var methodologyRepository = new Mock<IMethodologyRepository>(Strict);
            var publishingService = new Mock<IPublishingService>(Strict);

            contentService.Setup(mock =>
                    mock.GetContentBlocks<HtmlBlock>(methodology1.Id))
                .ReturnsAsync(new List<HtmlBlock>());

            methodologyRepository.Setup(mock =>
                    mock.IsPubliclyAccessible(methodology1.Id))
                .ReturnsAsync(false);

            await using (var context = InMemoryApplicationDbContext(contentDbContextId))
            {
                var service = SetupMethodologyService(contentDbContext: context,
                    methodologyContentService: contentService.Object,
                    methodologyImageService: imageService.Object,
                    methodologyRepository: methodologyRepository.Object,
                    publishingService: publishingService.Object);

                var result = await service.UpdateMethodology(methodology1.Id, request);

                Assert.True(result.IsLeft);
                AssertValidationProblem(result.Left, SlugNotUnique);
            }

            VerifyAllMocks(contentService, imageService, methodologyRepository, publishingService);
        }

        private static MethodologyService SetupMethodologyService(
            ContentDbContext contentDbContext,
            IPersistenceHelper<ContentDbContext> persistenceHelper = null,
            IMethodologyContentService methodologyContentService = null,
            IMethodologyFileRepository methodologyFileRepository = null,
            IMethodologyRepository methodologyRepository = null,
            IMethodologyImageService methodologyImageService = null,
            IPublishingService publishingService = null,
            IUserService userService = null)
        {
            return new MethodologyService(
                persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
                contentDbContext,
                AdminMapper(),
                methodologyContentService ?? new Mock<IMethodologyContentService>().Object,
                methodologyFileRepository ?? new MethodologyFileRepository(contentDbContext),
                methodologyRepository ?? new Mock<IMethodologyRepository>().Object,
                methodologyImageService ?? new Mock<IMethodologyImageService>().Object,
                publishingService ?? new Mock<IPublishingService>().Object,
                userService ?? AlwaysTrueUserService().Object);
        }
    }
}
