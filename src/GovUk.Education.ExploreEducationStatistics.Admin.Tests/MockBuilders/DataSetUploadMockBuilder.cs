#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;

public class DataSetUploadMockBuilder
{
    private Guid? _releaseVersionId;

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
            Status = DataSetUploadStatus.PENDING_IMPORT,
            UploadedBy = "test@test.com",
            Created = DateTime.UtcNow,
            ScreenerResult = new DataSetScreenerResponse
            {
                OverallResult = ScreenerResult.Passed,
                Message = "Screener passed",
                TestResults =
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

    public DataSetUploadMockBuilder WithReleaseVersionId(Guid releaseVersionId)
    {
        _releaseVersionId = releaseVersionId;
        return this;
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
}
