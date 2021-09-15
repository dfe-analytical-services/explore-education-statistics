#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Methodology;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.PermissionTestUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.Methodologies
{
    public class MethodologyNoteServicePermissionTests
    {
        [Fact]
        public async Task AddNote()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid()
            };

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(methodologyVersion, CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyNoteService(
                            persistenceHelper: MockPersistenceHelper<ContentDbContext, MethodologyVersion>(
                                methodologyVersion.Id, methodologyVersion).Object,
                            userService: userService.Object);
                        return service.AddNote(
                            methodologyVersion.Id,
                            new MethodologyNoteAddRequest());
                    }
                );
        }

        [Fact]
        public async Task DeleteNote()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid()
            };

            var methodologyNote = new MethodologyNote
            {
                Id = Guid.NewGuid(),
                MethodologyVersion = methodologyVersion
            };

            var persistenceHelper = new Mock<IPersistenceHelper<ContentDbContext>>(Strict);
            SetupCall(persistenceHelper, methodologyNote);

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(methodologyVersion, CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyNoteService(
                            persistenceHelper: persistenceHelper.Object,
                            userService: userService.Object);
                        return service.DeleteNote(
                            methodologyVersion.Id,
                            methodologyNote.Id);
                    }
                );
        }

        [Fact]
        public async Task UpdateNote()
        {
            var methodologyVersion = new MethodologyVersion
            {
                Id = Guid.NewGuid()
            };

            var methodologyNote = new MethodologyNote
            {
                Id = Guid.NewGuid(),
                MethodologyVersion = methodologyVersion
            };

            var persistenceHelper = new Mock<IPersistenceHelper<ContentDbContext>>(Strict);
            SetupCall(persistenceHelper, methodologyNote);

            await PolicyCheckBuilder<SecurityPolicies>()
                .SetupResourceCheckToFail(methodologyVersion, CanUpdateSpecificMethodology)
                .AssertForbidden(
                    userService =>
                    {
                        var service = SetupMethodologyNoteService(
                            persistenceHelper: persistenceHelper.Object,
                            userService: userService.Object);
                        return service.UpdateNote(
                            methodologyVersion.Id,
                            methodologyNote.Id,
                            new MethodologyNoteUpdateRequest());
                    }
                );
        }

        private static MethodologyNoteService SetupMethodologyNoteService(
            IPersistenceHelper<ContentDbContext> persistenceHelper,
            IMethodologyNoteRepository? methodologyNoteRepository = null,
            IUserService? userService = null)
        {
            return new(
                AdminMapper(),
                persistenceHelper,
                methodologyNoteRepository ?? Mock.Of<IMethodologyNoteRepository>(Strict),
                userService ?? Mock.Of<IUserService>(Strict));
        }
    }
}
