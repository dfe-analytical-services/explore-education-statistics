#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Org.BouncyCastle.Asn1.X509;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetUploadMockBuilder(TimeProvider? timeProvider = null)
{
    private Guid? _releaseVersionId;
    private string? _screenerResult;
    private List<DataScreenerTestResult>? _testResults;
    private TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

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
            Created = _timeProvider.GetUtcNow().AddDays(-1).Date,
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
                Passed = _screenerResult == "Passed",
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
            ScreenerProgress = new DataSetScreenerProgress
            {
                PercentageComplete = 100,
                Stage = "Stage 1",
                Completed = true,
                Passed = _screenerResult == "Passed",
                LastUpdated = _timeProvider.GetUtcNow(),
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
            Status = nameof(DataSetUploadStatus.PENDING_IMPORT),
            UploadedBy = "test.user",
            MetaFileName = "data.meta.csv",
            MetaFileSize = "123 B",
            ReplacingFileId = null,
            Created = DateTime.UtcNow,
            ScreenerResult = new()
            {
                OverallResult = "Passed",
                TestResults =
                [
                    new()
                    {
                        TestFunctionName = "TestFunction1",
                        Notes = "Test 1 passed",
                        Stage = "1",
                        Result = nameof(TestResult.PASS),
                    },
                    new()
                    {
                        TestFunctionName = "TestFunction2",
                        Notes = "Test 2 passed",
                        Stage = "2",
                        Result = nameof(TestResult.PASS),
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
