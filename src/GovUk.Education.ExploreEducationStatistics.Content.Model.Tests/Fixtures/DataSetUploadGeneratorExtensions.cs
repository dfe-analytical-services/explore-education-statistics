using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

public static class DataSetUploadGeneratorExtensions
{
    public static Generator<DataSetUpload> DefaultDataSetUpload(this DataFixture fixture) =>
        fixture.Generator<DataSetUpload>().WithDefaults();

    public static Generator<DataSetUpload> WithDefaults(this Generator<DataSetUpload> generator) =>
        generator.ForInstance(d => d.SetDefaults());

    public static InstanceSetters<DataSetUpload> SetDefaults(this InstanceSetters<DataSetUpload> setters) =>
        setters
            .SetDefault(d => d.Id)
            .SetDefault(d => d.ReleaseVersionId)
            .SetDefault(d => d.DataSetTitle)
            .SetDefault(d => d.DataFileId)
            .SetDefault(d => d.DataFileName)
            .SetDefault(d => d.DataFileSizeInBytes)
            .SetDefault(d => d.MetaFileId)
            .SetDefault(d => d.MetaFileName)
            .SetDefault(d => d.MetaFileSizeInBytes)
            .SetDefault(d => d.Status)
            .SetDefault(d => d.Created)
            .SetDefault(d => d.UploadedBy);

    public static Generator<DataSetUpload> WithId(this Generator<DataSetUpload> generator, Guid id) =>
        generator.ForInstance(s => s.SetId(id));

    public static Generator<DataSetUpload> WithReleaseVersionId(
        this Generator<DataSetUpload> generator,
        Guid releaseVersionId
    ) => generator.ForInstance(s => s.SetReleaseVersionId(releaseVersionId));

    public static Generator<DataSetUpload> WithDataSetTitle(this Generator<DataSetUpload> generator, string title) =>
        generator.ForInstance(s => s.SetDataSetTitle(title));

    public static Generator<DataSetUpload> WithDataFileId(this Generator<DataSetUpload> generator, Guid dataFileId) =>
        generator.ForInstance(s => s.SetDataFileId(dataFileId));

    public static Generator<DataSetUpload> WithDataFileName(
        this Generator<DataSetUpload> generator,
        string dataFileName
    ) => generator.ForInstance(s => s.SetDataFileName(dataFileName));

    public static Generator<DataSetUpload> WithDataFileSizeInBytes(
        this Generator<DataSetUpload> generator,
        long size
    ) => generator.ForInstance(s => s.SetDataFileSizeInBytes(size));

    public static Generator<DataSetUpload> WithMetaFileId(this Generator<DataSetUpload> generator, Guid metaFileId) =>
        generator.ForInstance(s => s.SetMetaFileId(metaFileId));

    public static Generator<DataSetUpload> WithMetaFileName(
        this Generator<DataSetUpload> generator,
        string metaFileName
    ) => generator.ForInstance(s => s.SetMetaFileName(metaFileName));

    public static Generator<DataSetUpload> WithMetaFileSizeInBytes(
        this Generator<DataSetUpload> generator,
        long size
    ) => generator.ForInstance(s => s.SetMetaFileSizeInBytes(size));

    public static Generator<DataSetUpload> WithReplacingFileId(
        this Generator<DataSetUpload> generator,
        Guid? replacingFileId
    ) => generator.ForInstance(s => s.SetReplacingFileId(replacingFileId));

    public static Generator<DataSetUpload> WithStatus(
        this Generator<DataSetUpload> generator,
        DataSetUploadStatus status
    ) => generator.ForInstance(s => s.SetStatus(status));

    public static Generator<DataSetUpload> WithScreenerResult(
        this Generator<DataSetUpload> generator,
        DataSetScreenerResponse? screenerResult
    ) => generator.ForInstance(s => s.SetScreenerResult(screenerResult));

    public static Generator<DataSetUpload> WithScreenerProgress(
        this Generator<DataSetUpload> generator,
        DataSetScreenerProgress? screenerProgress
    ) => generator.ForInstance(s => s.SetScreenerProgress(screenerProgress));

    public static Generator<DataSetUpload> WithCreated(this Generator<DataSetUpload> generator, DateTime created) =>
        generator.ForInstance(s => s.SetCreated(created));

    public static Generator<DataSetUpload> WithUploadedBy(this Generator<DataSetUpload> generator, string uploadedBy) =>
        generator.ForInstance(s => s.SetUploadedBy(uploadedBy));

    public static InstanceSetters<DataSetUpload> SetId(this InstanceSetters<DataSetUpload> setters, Guid id) =>
        setters.Set(d => d.Id, id);

    public static InstanceSetters<DataSetUpload> SetReleaseVersionId(
        this InstanceSetters<DataSetUpload> setters,
        Guid releaseVersionId
    ) => setters.Set(d => d.ReleaseVersionId, releaseVersionId);

    public static InstanceSetters<DataSetUpload> SetDataSetTitle(
        this InstanceSetters<DataSetUpload> setters,
        string title
    ) => setters.Set(d => d.DataSetTitle, title);

    public static InstanceSetters<DataSetUpload> SetDataFileId(
        this InstanceSetters<DataSetUpload> setters,
        Guid dataFileId
    ) => setters.Set(d => d.DataFileId, dataFileId);

    public static InstanceSetters<DataSetUpload> SetDataFileName(
        this InstanceSetters<DataSetUpload> setters,
        string dataFileName
    ) => setters.Set(d => d.DataFileName, dataFileName);

    public static InstanceSetters<DataSetUpload> SetDataFileSizeInBytes(
        this InstanceSetters<DataSetUpload> setters,
        long size
    ) => setters.Set(d => d.DataFileSizeInBytes, size);

    public static InstanceSetters<DataSetUpload> SetMetaFileId(
        this InstanceSetters<DataSetUpload> setters,
        Guid metaFileId
    ) => setters.Set(d => d.MetaFileId, metaFileId);

    public static InstanceSetters<DataSetUpload> SetMetaFileName(
        this InstanceSetters<DataSetUpload> setters,
        string metaFileName
    ) => setters.Set(d => d.MetaFileName, metaFileName);

    public static InstanceSetters<DataSetUpload> SetMetaFileSizeInBytes(
        this InstanceSetters<DataSetUpload> setters,
        long size
    ) => setters.Set(d => d.MetaFileSizeInBytes, size);

    public static InstanceSetters<DataSetUpload> SetReplacingFileId(
        this InstanceSetters<DataSetUpload> setters,
        Guid? replacingFileId
    ) => setters.Set(d => d.ReplacingFileId, replacingFileId);

    public static InstanceSetters<DataSetUpload> SetStatus(
        this InstanceSetters<DataSetUpload> setters,
        DataSetUploadStatus status
    ) => setters.Set(d => d.Status, status);

    public static InstanceSetters<DataSetUpload> SetScreenerResult(
        this InstanceSetters<DataSetUpload> setters,
        DataSetScreenerResponse? screenerResult
    ) => setters.Set(d => d.ScreenerResult, screenerResult);

    public static InstanceSetters<DataSetUpload> SetScreenerProgress(
        this InstanceSetters<DataSetUpload> setters,
        DataSetScreenerProgress? screenerProgress
    ) => setters.Set(d => d.ScreenerProgress, screenerProgress);

    public static InstanceSetters<DataSetUpload> SetCreated(
        this InstanceSetters<DataSetUpload> setters,
        DateTime created
    ) => setters.Set(d => d.Created, created);

    public static InstanceSetters<DataSetUpload> SetUploadedBy(
        this InstanceSetters<DataSetUpload> setters,
        string uploadedBy
    ) => setters.Set(d => d.UploadedBy, uploadedBy);
}
