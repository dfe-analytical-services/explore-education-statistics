using FluentValidation.TestHelper;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System.Text;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Validators;

public class DataSetDtoValidatorTests
{
    private readonly DataSetDto.Validator _validator;

    public DataSetDtoValidatorTests()
    {
        _validator = new();
    }

    [Fact]
    public async Task DataSetDto_Valid_ReturnsNoErrors()
    {
        // Arrange
        var dto = await new DataSetDtoBuilder().Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(Skip = "TODO (EES-6201): Flakey test requires investigation")]
    public async Task DataSetDto_ReleaseVersionMissing_ReturnsExpectedError()
    {
        // Arrange
        var dto = await new DataSetDtoBuilder()
            .WhereReleaseVersionIdIsEmpty()
            .Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.ReleaseVersionId)
            .WithErrorMessage("'Release Version Id' must not be empty.")
            .WithErrorCode("NotEmptyValidator");
    }

    [Fact]
    public async Task DataSetDto_TitleMissing_ReturnsExpectedError()
    {
        // Arrange
        var dto = await new DataSetDtoBuilder()
            .WhereTitleIs("")
            .Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.Title)
            .WithErrorMessage("Data set title cannot be empty")
            .WithErrorCode("DataSetTitleCannotBeEmpty");
    }

    [Fact]
    public async Task DataSetDto_TitleTooLong_ReturnsExpectedError()
    {
        // Arrange
        var title = new string('a', 121);

        var dto = await new DataSetDtoBuilder()
            .WhereTitleIs(title)
            .Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.Title)
            .WithErrorMessage($"Title '{title}' must be 120 characters or less")
            .WithErrorCode("DataSetTitleTooLong");
    }

    [Fact]
    public async Task DataSetDto_DataFileIsEmpty_ReturnsExpectedError()
    {
        // Arrange
        var file = new FileDto { FileName = "data.csv", FileSize = 0, FileStream = new System.IO.MemoryStream() };

        var dto = await new DataSetDtoBuilder()
            .WhereDataFileIs(file)
            .Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.DataFile)
            .WithErrorMessage($"File 'data.csv' either empty or not found.")
            .WithErrorCode("FileSizeMustNotBeZero");
    }

    [Fact]
    public async Task DataSetDto_DataFileHasWrongExtension_ReturnsExpectedError()
    {
        // Arrange
        var file = new FileDto { FileName = "data.jpg", FileSize = 4, FileStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes("test")) };

        var dto = await new DataSetDtoBuilder()
            .WhereDataFileIs(file)
            .Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.DataFile)
            .WithErrorMessage("File name 'data.jpg' must end in '.csv'.")
            .WithErrorCode("FileNameMustEndDotCsv");
    }

    [Fact]
    public async Task DataSetDto_MetaFileIsEmpty_ReturnsExpectedError()
    {
        // Arrange
        var file = new FileDto { FileName = "data.meta.csv", FileSize = 0, FileStream = new System.IO.MemoryStream() };

        var dto = await new DataSetDtoBuilder()
            .WhereDataFileIs(file)
            .Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.DataFile)
            .WithErrorMessage($"File 'data.meta.csv' either empty or not found.")
            .WithErrorCode("FileSizeMustNotBeZero");
    }

    [Fact]
    public async Task DataSetDto_MetaFileHasWrongExtension_ReturnsExpectedError()
    {
        // Arrange
        var file = new FileDto { FileName = "data.meta.jpg", FileSize = 4, FileStream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes("test")) };

        var dto = await new DataSetDtoBuilder()
            .WhereMetaFileIs(file)
            .Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.MetaFile)
            .WithErrorMessage("Meta file 'data.meta.jpg' must end in '.meta.csv'.")
            .WithErrorCode("MetaFileNameMustEndDotMetaDotCsv");
    }

    [Fact]
    public async Task DataSetDto_ReplacingFileIsNotADataFile_ReturnsExpectedError()
    {
        // Arrange
        var file = new File { Type = FileType.Ancillary };

        var dto = await new DataSetDtoBuilder()
            .WhereReplacingFileIs(file)
            .Build();

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.ReplacingFile)
            .WithErrorMessage("Replacing file should be of type 'Data'.")
            .WithErrorCode("InvalidFileTypeForReplacement");
    }
}
