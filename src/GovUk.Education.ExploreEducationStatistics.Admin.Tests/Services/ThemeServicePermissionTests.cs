#nullable enable
using System;
using System.Threading.Tasks;
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Methodologies;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model.Database;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class ThemeServicePermissionTests
    {
        private readonly Theme _theme = new() { Id = Guid.NewGuid() };

        [Fact]
        public async Task GetThemes_CanViewAllThemes()
        {
            var contextId = Guid.NewGuid().ToString();

            await using (var context = DbUtils.InMemoryApplicationDbContext(contextId))
            {
                context.Add(
                    new Theme { Title = "Test theme" }
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

                        var service = SetupThemeService(userService: userService.Object, contentDbContext: context);
                        var result = await service.GetThemes();

                        Assert.Single(result.Right);
                        Assert.Equal("Test theme", result.Right[0].Title);

                        return result;
                    }
                );
        }

        [Fact]
        public async Task GetMyThemes_NoAccessToSystem()
        {
            await PolicyCheckBuilder()
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
        public async Task CreateTheme()
        {
            await PolicyCheckBuilder()
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
        public async Task UpdateTheme()
        {
            await PolicyCheckBuilder()
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
        public async Task GetTheme()
        {
            await PolicyCheckBuilder()
                .SetupCheck(SecurityPolicies.CanManageAllTaxonomy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = SetupThemeService(userService: userService.Object);

                        return await service.GetTheme(_theme.Id);
                    }
                );
        }

        [Fact]
        public async Task DeleteTheme()
        {
            await PolicyCheckBuilder()
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
        public async Task DeleteUiTestThemes()
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
            ContentDbContext? contentDbContext = null,
            PublicDataDbContext? publicDataDbContext = null,
            IMapper? mapper = null,
            PersistenceHelper<ContentDbContext>? persistenceHelper = null,
            IUserService? userService = null,
            IMethodologyService? methodologyService = null,
            IPublishingService? publishingService = null,
            IReleaseVersionService? releaseVersionService = null,
            IAdminEventRaiser? adminEventRaiser = null)
        {
            var publicContext = publicDataDbContext ?? Mock.Of<PublicDataDbContext>();
            var contentContext = contentDbContext ?? Mock.Of<ContentDbContext>();

            return new ThemeService(
                appOptions: DefaultAppOptions(),
                contentDbContext: contentContext,
                dataSetVersionRepository: new DataSetVersionRepository(
                    contentDbContext: contentContext,
                    publicDataDbContext: publicContext),
                mapper ?? AdminMapper(),
                persistenceHelper ?? MockPersistenceHelper<ContentDbContext, Theme>(_theme.Id, _theme).Object,
                userService ?? AlwaysTrueUserService().Object,
                methodologyService ?? Mock.Of<IMethodologyService>(Strict),
                publishingService ?? Mock.Of<IPublishingService>(Strict),
                releaseVersionService ?? Mock.Of<IReleaseVersionService>(Strict),
                adminEventRaiser ?? new AdminEventRaiserMockBuilder().Build()
            );
        }
    }
}
