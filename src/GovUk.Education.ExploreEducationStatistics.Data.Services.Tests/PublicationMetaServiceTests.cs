using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Mappings;
using Moq;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Tests
{
    public class PublicationMetaServiceTests
    {
        [Fact]
        public void GetSubjectsForLatestRelease()
        {
            var publicationId = Guid.NewGuid();
            var releaseId = Guid.NewGuid();

            var subject1 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Subject one"
            };

            var subject2 = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Subject two"
            };

            var (releaseService,
                subjectService) = Mocks();

            releaseService.Setup(s => s.GetLatestRelease(publicationId)).Returns(releaseId);
            subjectService.Setup(s =>
                    s.FindMany(It.IsAny<Expression<Func<Subject, bool>>>(),
                        It.IsAny<List<Expression<Func<Subject, object>>>>()))
                .Returns(new List<Subject>
                {
                    subject1, subject2
                }.AsQueryable);

            var service = new PublicationMetaService(releaseService.Object, subjectService.Object,
                MapperUtils.MapperForProfile<DataServiceMappingProfiles>());

            var result = service.GetSubjectsForLatestRelease(publicationId);
            var subjects = result.Subjects.ToList();

            Assert.Equal(publicationId, result.PublicationId);
            Assert.Equal(2, subjects.Count);
            Assert.Equal(subject1.Id, subjects[0].Id);
            Assert.Equal(subject1.Name, subjects[0].Label);
            Assert.Equal(subject2.Id, subjects[1].Id);
            Assert.Equal(subject2.Name, subjects[1].Label);
        }

        private static (Mock<IReleaseService>,
            Mock<ISubjectService>) Mocks()
        {
            return (
                new Mock<IReleaseService>(),
                new Mock<ISubjectService>());
        }
    }
}