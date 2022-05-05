using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Utils.StatisticsDbUtils;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FootnoteServicePermissionTests
    {
        private static readonly Release Release = new Release
        {
            Id = Guid.NewGuid()
        };

        private static readonly Subject Subject = new Subject
        {
            Id = Guid.NewGuid()
        };

        private static readonly Footnote Footnote = new Footnote
        {
            Id = Guid.NewGuid(),
            Subjects = new List<SubjectFootnote>
            {
                new SubjectFootnote
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
        public void CreateFootnote()
        {
            AssertSecurityPoliciesChecked(
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
                CanUpdateSpecificRelease
            );
        }

        [Fact]
        public void UpdateFootnote()
        {
            AssertSecurityPoliciesChecked(
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
                CanUpdateSpecificRelease
            );
        }

        [Fact]
        public void DeleteFootnote()
        {
            AssertSecurityPoliciesChecked(
                service => service
                    .DeleteFootnote(Release.Id, Footnote.Id),
                CanUpdateSpecificRelease
            );
        }

        private void AssertSecurityPoliciesChecked<T>(
            Func<FootnoteService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
        {
            var (
                _,
                releaseHelper,
                userService,
                dataBlockService,
                footnoteService,
                footnoteHelper,
                guidGenerator
                ) = Mocks();

            using var context = InMemoryStatisticsDbContext();
            var service = new FootnoteService(
                context,
                releaseHelper.Object,
                userService.Object,
                dataBlockService.Object,
                footnoteService.Object,
                footnoteHelper.Object,
                guidGenerator.Object
            );

            PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, Release, userService, service, policies);
        }

        private (
            Mock<ILogger<FootnoteService>>,
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IUserService>,
            Mock<IDataBlockService>,
            Mock<IFootnoteRepository>,
            Mock<IPersistenceHelper<StatisticsDbContext>>,
            Mock<IGuidGenerator>) Mocks()
        {
            var contentPersistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall(contentPersistenceHelper, Release.Id, Release);

            return (
                new Mock<ILogger<FootnoteService>>(),
                contentPersistenceHelper,
                new Mock<IUserService>(),
                new Mock<IDataBlockService>(),
                new Mock<IFootnoteRepository>(),
                MockUtils.MockPersistenceHelper<StatisticsDbContext, Footnote>(Footnote.Id, Footnote),
                new Mock<IGuidGenerator>());
        }
    }
}
