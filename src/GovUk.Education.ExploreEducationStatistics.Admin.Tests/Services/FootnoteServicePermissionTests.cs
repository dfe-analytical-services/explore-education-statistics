using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services
{
    public class FootnoteServicePermissionTests
    {
        private static readonly Release release = new Release
        {
            Id = Guid.NewGuid()
        };
        
        private static readonly Subject subject = new Subject
        {
            Id = Guid.NewGuid()
        };
        
        private static readonly Footnote footnote = new Footnote
        {
            Id = Guid.NewGuid(),
            Subjects = new List<SubjectFootnote>
            {
                new SubjectFootnote
                {
                    SubjectId = subject.Id 
                }
            }
        };

        private static readonly IReadOnlyCollection<Guid> SubjectIdsList = new List<Guid>
        {
            subject.Id    
        };
        
        private static readonly IReadOnlyCollection<Guid> GuidList = new List<Guid>();
        
        private readonly Task<Either<ActionResult, Release>> _releaseExistsResult 
            = Task.FromResult(new Either<ActionResult, Release>(new Release()));
        
        [Fact]
        public void CreateFootnote()
        {
            AssertSecurityPoliciesChecked(service => service
                .CreateFootnote(
                    release.Id,
                    "", 
                    GuidList, 
                    GuidList, 
                    GuidList, 
                    GuidList, 
                    SubjectIdsList), 
                CanUpdateSpecificRelease);
        }
        
        [Fact]
        public void UpdateFootnote()
        {
            AssertSecurityPoliciesChecked(service => service
                    .UpdateFootnote(
                        release.Id, 
                        footnote.Id,
                        "", 
                        GuidList, 
                        GuidList, 
                        GuidList, 
                        GuidList, 
                        SubjectIdsList), 
                CanUpdateSpecificRelease);
        }
        
        [Fact]
        public void DeleteFootnote()
        {
            AssertSecurityPoliciesChecked(service => service
                    .DeleteFootnote(release.Id, footnote.Id), 
                CanUpdateSpecificRelease);
        }
        
        private void AssertSecurityPoliciesChecked<T>(
            Func<FootnoteService, Task<Either<ActionResult, T>>> protectedAction, params SecurityPolicies[] policies)
        {
            var (
                logger, 
                filterService, 
                filterGroupService, 
                filterItemService,
                indicatorService, 
                subjectService, 
                releaseHelper, 
                userService, 
                footnoteHelper
                ) = Mocks();
            
            using (var context = DbUtils.InMemoryStatisticsDbContext())
            {
                var service = new FootnoteService(
                    context, 
                    logger.Object, 
                    filterService.Object, 
                    filterGroupService.Object, 
                    filterItemService.Object,
                    indicatorService.Object, 
                    subjectService.Object, 
                    releaseHelper.Object, 
                    userService.Object, 
                    footnoteHelper.Object
                );

                PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, release, userService, service, policies);
            }
        }

        private (
            Mock<ILogger<FootnoteService>>,
            Mock<IFilterService>, 
            Mock<IFilterGroupService>,
            Mock<IFilterItemService>,
            Mock<IIndicatorService>,
            Mock<ISubjectService>,
            Mock<IPersistenceHelper<ContentDbContext>>,
            Mock<IUserService>,
            Mock<IPersistenceHelper<StatisticsDbContext>>) Mocks()
        {
            var contentPersistenceHelper = MockUtils.MockPersistenceHelper<ContentDbContext>();
            MockUtils.SetupCall(contentPersistenceHelper, release.Id, release);
            
            return (
                new Mock<ILogger<FootnoteService>>(), 
                new Mock<IFilterService>(), 
                new Mock<IFilterGroupService>(), 
                new Mock<IFilterItemService>(), 
                new Mock<IIndicatorService>(), 
                new Mock<ISubjectService>(), 
                contentPersistenceHelper,
                new Mock<IUserService>(), 
                MockUtils.MockPersistenceHelper<StatisticsDbContext, Footnote>(footnote.Id, footnote));
        }
    }
}