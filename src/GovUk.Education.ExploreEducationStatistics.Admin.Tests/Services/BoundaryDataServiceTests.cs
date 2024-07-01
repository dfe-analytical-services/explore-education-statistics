#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Moq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;
public class BoundaryDataServiceTests
{
    private readonly Mock<IBoundaryLevelRepository> _boundaryLevelRepository;
    private readonly Mock<IBoundaryDataRepository> _boundaryDataRepository;
    private readonly BoundaryDataService _sut;

    public BoundaryDataServiceTests()
    {
        _boundaryLevelRepository = new Mock<IBoundaryLevelRepository>(MockBehavior.Strict);
        _boundaryDataRepository = new Mock<IBoundaryDataRepository>(MockBehavior.Strict);
        _sut = new(_boundaryLevelRepository.Object, _boundaryDataRepository.Object);
    }

    [Fact]
    public async Task ProcessGeoJson_ReturnsOk()
    {
        // Arrange
        // Act
        // Assert
    }
}
