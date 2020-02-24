using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Controllers.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
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
        private static readonly Release Release = new Release
        {
            Id = Guid.NewGuid()
        };
        
        private static readonly Subject Subject = new Subject
        {
            Id = Guid.NewGuid(),
            ReleaseId = Release.Id
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
            AssertSecurityPoliciesChecked(service => service
                .CreateFootnote(
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
                        Footnote.Id,
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
                    .DeleteFootnote(Footnote.Id), 
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
                context.Add(Subject);
                context.SaveChanges();
                
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

                PermissionTestUtil.AssertSecurityPoliciesChecked(protectedAction, Release, userService, service, policies);
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
            return (
                new Mock<ILogger<FootnoteService>>(), 
                new Mock<IFilterService>(), 
                new Mock<IFilterGroupService>(), 
                new Mock<IFilterItemService>(), 
                new Mock<IIndicatorService>(), 
                new Mock<ISubjectService>(), 
                MockUtils.MockPersistenceHelper<ContentDbContext, Release>(Release.Id, Release),
                new Mock<IUserService>(), 
                MockUtils.MockPersistenceHelper<StatisticsDbContext, Footnote>(Footnote.Id, Footnote));
        }
    }
}