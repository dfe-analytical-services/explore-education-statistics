#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetUploadMockBuilder
{
    private Guid? _releaseVersionId;
    private ScreenerResult? _screenerResult;
    private List<DataScreenerTestResult>? _testResults;

    public DataSetUpload BuildEntity()
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
            Status = _screenerResult == ScreenerResult.Failed
                ? DataSetUploadStatus.FAILED_SCREENING
                : DataSetUploadStatus.PENDING_IMPORT,
            UploadedBy = "test@test.com",
            Created = DateTime.UtcNow,
            ScreenerResult = new DataSetScreenerResponse
            {
                OverallResult = _screenerResult ?? ScreenerResult.Passed,
                Message = "Screening complete",
                TestResults = _testResults ??
                [
                    new()
                    {
                        TestFunctionName = "TestFunction1",
                        Notes = "Test 1 passed",
                        Stage = Stage.Passed,
                        Result = TestResult.PASS,
                    },
                    new()
                    {
                        TestFunctionName = "TestFunction2",
                        Notes = "Test 2 passed",
                        Stage = Stage.Passed,
                        Result = TestResult.PASS,
                    },
                ]
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
                OverallResult = ScreenerResult.Passed.ToString(),
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
                ]
            }
        };
    }

    public DataSetUploadMockBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
    }

    public DataSetUploadMockBuilder WithFailingTests()
    {
        _screenerResult = ScreenerResult.Failed;
        _testResults =
        [
            new()
            {
                TestFunctionName = "TestFunction1",
                Notes = "Test 1 failed",
                Stage = Stage.PreScreening1,
                Result = TestResult.FAIL,
            },
            new()
            {
                TestFunctionName = "TestFunction2",
                Notes = "Test 2 failed",
                Stage = Stage.PreScreening1,
                Result = TestResult.FAIL,
            },
        ];

        return this;
    }

    public DataSetUploadMockBuilder WithWarningTests()
    {
        _screenerResult = ScreenerResult.Passed;
        _testResults =
        [
            new()
            {
                TestFunctionName = "TestFunction1",
                Notes = "Test 1 passed with a warning",
                Stage = Stage.Passed,
                Result = TestResult.WARNING,
            },
            new()
            {
                TestFunctionName = "TestFunction2",
                Notes = "Test 2 passed with a warning",
                Stage = Stage.Passed,
                Result = TestResult.WARNING,
            },
        ];

        return this;
    }
}
