#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetUploadMockBuilder
{
    private Guid? _releaseVersionId;
    private string? _screenerResult;
    private List<DataScreenerTestResult>? _testResults;

    public DataSetUpload BuildInitialEntity()
    {
        return new DataSetUpload
        {
            ReleaseVersionId = _releaseVersionId ?? Guid.NewGuid(),
            DataSetTitle = "Test Data Set",
            DataFileId = Guid.NewGuid(),
            DataFileName = "data.csv",
            DataFileSizeInBytes = 434,
            MetaFileId = Guid.NewGuid(),
            MetaFileName = "meta.data.csv",
            MetaFileSizeInBytes = 157,
            Status = DataSetUploadStatus.SCREENING,
            UploadedBy = "test@test.com",
            Created = DateTime.UtcNow,
            ReplacingFileId = null,
        };
    }

    public DataSetUpload BuildScreenedEntity()
    {
        return new DataSetUpload
        {
            ReleaseVersionId = _releaseVersionId ?? Guid.NewGuid(),
            DataSetTitle = "Test Data Set",
            DataFileId = Guid.NewGuid(),
            DataFileName = "data.csv",
            DataFileSizeInBytes = 434,
            MetaFileId = Guid.NewGuid(),
            MetaFileName = "meta.data.csv",
            MetaFileSizeInBytes = 157,
            Status =
                _screenerResult == "Failed" ? DataSetUploadStatus.FAILED_SCREENING : DataSetUploadStatus.PENDING_IMPORT,
            UploadedBy = "test@test.com",
            Created = DateTime.UtcNow,
            ScreenerResult = new DataSetScreenerResponse
            {
                OverallResult = _screenerResult ?? "Passed",
                TestResults =
                    _testResults
                    ??
                    [
                        new()
                        {
                            TestFunctionName = "TestFunction1",
                            Notes = "Test 1 passed",
                            Stage = "Passed",
                            Result = TestResult.PASS,
                            GuidanceUrl = "http://example.com/guidance1",
                        },
                        new()
                        {
                            TestFunctionName = "TestFunction2",
                            Notes = "Test 2 passed",
                            Stage = "Passed",
                            Result = TestResult.PASS,
                            GuidanceUrl = "http://example.com/guidance2",
                        },
                    ],
            },
            ReplacingFileId = null,
        };
    }

    public static DataSetUploadViewModel BuildViewModel()
    {
        return new DataSetUploadViewModel
        {
            Id = Guid.NewGuid(),
            DataSetTitle = "Data set title",
            DataFileSize = "123 Kb",
            DataFileName = "data.csv",
            Status = DataSetUploadStatus.PENDING_IMPORT.ToString(),
            UploadedBy = "test.user",
            MetaFileName = "data.meta.csv",
            MetaFileSize = "123 B",
            ReplacingFileId = null,
            Created = DateTime.UtcNow,
            ScreenerResult = new()
            {
                Message = "Screener result message",
                OverallResult = "Passed",
                TestResults =
                [
                    new()
                    {
                        TestFunctionName = "TestFunction1",
                        Notes = "Test 1 passed",
                        Stage = "1",
                        Result = TestResult.PASS.ToString(),
                    },
                    new()
                    {
                        TestFunctionName = "TestFunction2",
                        Notes = "Test 2 passed",
                        Stage = "2",
                        Result = TestResult.PASS.ToString(),
                    },
                ],
            },
            PublicApiCompatible = true,
        };
    }

    public DataSetUploadMockBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
    }

    public DataSetUploadMockBuilder WithFailingTests()
    {
        _screenerResult = "Failed";
        _testResults =
        [
            new()
            {
                TestFunctionName = "TestFunction1",
                Notes = "Test 1 failed",
                Stage = "PreScreening1",
                Result = TestResult.FAIL,
                GuidanceUrl = "http://example.com/guidance1",
            },
            new()
            {
                TestFunctionName = "TestFunction2",
                Notes = "Test 2 failed",
                Stage = "PreScreening1",
                Result = TestResult.FAIL,
                GuidanceUrl = "http://example.com/guidance2",
            },
        ];

        return this;
    }

    public DataSetUploadMockBuilder WithWarningTests()
    {
        _screenerResult = "Passed";
        _testResults =
        [
            new()
            {
                TestFunctionName = "TestFunction1",
                Notes = "Test 1 passed with a warning",
                Stage = "Passed",
                Result = TestResult.WARNING,
                GuidanceUrl = "http://example.com/guidance1",
            },
            new()
            {
                TestFunctionName = "TestFunction2",
                Notes = "Test 2 passed with a warning",
                Stage = "Passed",
                Result = TestResult.WARNING,
                GuidanceUrl = "http://example.com/guidance2",
            },
        ];

        return this;
    }
}
