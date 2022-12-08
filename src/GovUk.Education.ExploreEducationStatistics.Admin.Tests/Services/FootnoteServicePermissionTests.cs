#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FootnoteServicePermissionTests
    {
        private static readonly Release Release = new()
        {
            Id = Guid.NewGuid()
        };

        private static readonly Subject Subject = new()
        {
            Id = Guid.NewGuid()
        };

        private static readonly Footnote Footnote = new()
        {
            Id = Guid.NewGuid(),
            Subjects = new List<SubjectFootnote>
            {
                new()
                {
                    SubjectId = Subject.Id
                }
            }
        };

        private static readonly IReadOnlyCollection<Guid> SubjectIdsList = new List<Guid>
        {
            Subject.Id
        };

        private static readonly IReadOnlyCollection<Guid> GuidList = new List<Guid>();

        [Fact]
        public async Task CopyFootnotes_ViewSourceRelease()
        {
            var (
                contentPersistenceHelper,
                dataBlockService,
                footnoteService,
                statisticsPersistenceHelper
                ) = Mocks();

            var sourceRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var destinationRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            SetupCall(contentPersistenceHelper, sourceRelease.Id, sourceRelease);
            SetupCall(contentPersistenceHelper, destinationRelease.Id, destinationRelease);

            await PermissionTestUtils.PolicyCheckBuilder<ContentSecurityPolicies>()
                .SetupResourceCheck(sourceRelease, ContentSecurityPolicies.CanViewSpecificRelease, false)
                .AssertForbidden(
                    async userService =>
                    {
                        userService
                            .Setup(s => s.MatchesPolicy(destinationRelease, SecurityPolicies.CanUpdateSpecificRelease))
                            .ReturnsAsync(true);

                        var service = new FootnoteService(
                            InMemoryStatisticsDbContext(),
                            contentPersistenceHelper.Object,
                            userService.Object,
                            dataBlockService.Object,
                            footnoteService.Object,
                            statisticsPersistenceHelper.Object
                        );

                        return await service.CopyFootnotes(sourceRelease.Id, destinationRelease.Id);
                    }
                );
        }

        [Fact]
        public async Task CopyFootnotes_UpdateDestinationRelease()
        {
            var (
                contentPersistenceHelper,
                dataBlockService,
                footnoteService,
                statisticsPersistenceHelper
                ) = Mocks();

            var sourceRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            var destinationRelease = new Release
            {
                Id = Guid.NewGuid()
            };

            SetupCall(contentPersistenceHelper, sourceRelease.Id, sourceRelease);
            SetupCall(contentPersistenceHelper, destinationRelease.Id, destinationRelease);

            await PermissionTestUtils.PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheck(destinationRelease, SecurityPolicies.CanUpdateSpecificRelease, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = new FootnoteService(
                            InMemoryStatisticsDbContext(),
                            contentPersistenceHelper.Object,
                            userService.Object,
                            dataBlockService.Object,
                            footnoteService.Object,
                            statisticsPersistenceHelper.Object
                        );

                        return await service.CopyFootnotes(sourceRelease.Id, destinationRelease.Id);
                    }
                );
        }

        [Fact]
        public async Task CreateFootnote()
        {
            await AssertSecurityPolicyChecked(
                service => service
                    .CreateFootnote(
                        Release.Id,
                        "",
                        GuidList,
                        GuidList,
                        GuidList,
                        GuidList,
                        SubjectIdsList
                    ),
                Release,
                SecurityPolicies.CanUpdateSpecificRelease
            );
        }

        [Fact]
        public async Task DeleteFootnote()
        {
            await AssertSecurityPolicyChecked(
                service => service
                    .DeleteFootnote(Release.Id, Footnote.Id),
                Release,
                SecurityPolicies.CanUpdateSpecificRelease
            );
        }

        [Fact]
        public async Task GetFootnote()
        {
            await AssertSecurityPolicyChecked(
                service => service
                    .GetFootnote(Release.Id, Footnote.Id),
                Release,
                ContentSecurityPolicies.CanViewSpecificRelease
            );
        }

        [Fact]
        public async Task GetFootnotes()
        {
            await AssertSecurityPolicyChecked(
                service => service
                    .GetFootnotes(Release.Id),
                Release,
                ContentSecurityPolicies.CanViewSpecificRelease
            );
        }

        [Fact]
        public async Task UpdateFootnote()
        {
            await AssertSecurityPolicyChecked(
                service => service
                    .UpdateFootnote(
                        Release.Id,
                        Footnote.Id,
                        "",
                        GuidList,
                        GuidList,
                        GuidList,
                        GuidList,
                        SubjectIdsList
                    ),
                Release,
                SecurityPolicies.CanUpdateSpecificRelease
            );
        }

        private static Task AssertSecurityPolicyChecked<T, TResource, TPolicy>(
            Func<FootnoteService, Task<Either<ActionResult, T>>> protectedAction,
            TResource resource,
            TPolicy policy) where TPolicy : Enum
        {
            var (
                contentPersistenceHelper,
                dataBlockService,
                footnoteService,
                statisticsPersistenceHelper
                ) = Mocks();

            return PermissionTestUtils.PolicyCheckBuilder<TPolicy>()
                .SetupResourceCheck(resource, policy, false)
                .AssertForbidden(
                    async userService =>
                    {
                        var service = new FootnoteService(
                            InMemoryStatisticsDbContext(),
                            contentPersistenceHelper.Object,
                            userService.Object,
                            dataBlockService.Object,
                            footnoteService.Object,
                            statisticsPersistenceHelper.Object
                        );

                        return await protectedAction.Invoke(service);
                    }
                );
        }

        private static (
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IDataBlockService>,
            Mock<IFootnoteRepository>,
            Mock<IPersistenceHelper<StatisticsDbContext>>) Mocks()
        {
            return (
                MockPersistenceHelper<ContentDbContext, Release>(Release.Id, Release),
                new Mock<IDataBlockService>(),
                new Mock<IFootnoteRepository>(),
                MockPersistenceHelper<StatisticsDbContext, Footnote>(Footnote.Id, Footnote));
        }
    }
}
