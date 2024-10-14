using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading.Tasks;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeServicePermissionTests
    {
        private readonly Theme _theme = new()
        {
            Id = Guid.NewGuid()
        };

        [Fact]
        public async Task GetThemes_CanViewAllThemes()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new Theme
                    {
                        Title = "Test theme"
                    }
                );

                await context.SaveChangesAsync();
            }

            await PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.RegisteredUser)
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy)
                .AssertSuccess(
                    async userService =>
                    {
                        await using var context = DbUtils.InMemoryApplicationDbContext(contextId);

                        var service = SetupThemeService(userService: userService.Object, contentContext: context);
                        var result = await service.GetThemes();

                        Assert.Single(result.Right);
                        Assert.Equal("Test theme", result.Right[0].Title);

                        return result;
                    }
                );
        }

        [Fact]
        public void GetMyThemes_NoAccessToSystem()
        {
            PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.RegisteredUser, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.GetThemes();
                    }
                );
        }

        [Fact]
        public void CreateTheme()
        {
            PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.CreateTheme(
                            new ThemeSaveViewModel
                            {
                                Title = "Test title",
                                Summary = "Test summary"
                            }
                        );
                    }
                );
        }

        [Fact]
        public void UpdateTheme()
        {
            PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.UpdateTheme(
                            _theme.Id,
                            new ThemeSaveViewModel
                            {
                                Title = "Test title",
                                Summary = "Test summary"
                            }
                        );
                    }
                );
        }

        [Fact]
        public void GetTheme()
        {
            PolicyCheckBuilder()
                .SetupResourceCheck(_theme, SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.GetTheme(_theme.Id);
                    }
                );
        }

        [Fact]
        public void DeleteTheme()
        {
            PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.DeleteTheme(_theme.Id);
                    }
                );
        }

        [Fact]
        public async Task DeleteUITestThemes()
        {
            await PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.DeleteUITestThemes();
                    }
                );
        }

        private static IOptions<AppOptions> DefaultAppOptions()
        {
            return new AppOptions { EnableThemeDeletion = true }.ToOptionsWrapper();
        }

        private ThemeService SetupThemeService(
            ContentDbContext contentContext = null,
            StatisticsDbContext statisticsContext = null,
            IMapper mapper = null,
            PersistenceHelper<ContentDbContext> persistenceHelper = null,
            IUserService userService = null,
            IBlobCacheService cacheService = null,
            IMethodologyService methodologyService = null,
            IReleaseSubjectRepository releaseSubjectRepository = null,
            IReleaseFileService releaseFileService = null,
            IReleaseDataFileService releaseDataFileService = null,
            IReleasePublishingStatusRepository releasePublishingStatusRepository = null,
            IPublishingService publishingService = null)
        {
            return new ThemeService(
                DefaultAppOptions(),
                contentContext ?? new Mock<ContentDbContext>().Object,
                statisticsContext ?? new Mock<StatisticsDbContext>().Object,
                mapper ?? AdminMapper(),
                persistenceHelper ?? MockPersistenceHelper<ContentDbContext, Theme>(_theme.Id, _theme).Object,
                userService ?? AlwaysTrueUserService().Object,
                methodologyService ?? Mock.Of<IMethodologyService>(Strict),
                releaseFileService ?? Mock.Of<IReleaseFileService>(Strict),
                releaseSubjectRepository ?? Mock.Of<IReleaseSubjectRepository>(Strict),
                releaseDataFileService ?? Mock.Of<IReleaseDataFileService>(Strict),
                releasePublishingStatusRepository ?? Mock.Of<IReleasePublishingStatusRepository>(),
                publishingService ?? new Mock<IPublishingService>().Object,
                cacheService ?? Mock.Of<IBlobCacheService>(Strict)
            );
        }
    }
}
