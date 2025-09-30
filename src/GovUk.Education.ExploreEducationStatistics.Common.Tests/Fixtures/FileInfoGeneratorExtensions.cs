using GovUk.Education.ExploreEducationStatistics.Common.Model;
using FileInfo = GovUk.Education.ExploreEducationStatistics.Common.Model.FileInfo;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

public static class FileInfoGeneratorExtensions
{
    public static Generator<FileInfo> DefaultFileInfo(this DataFixture fixture) =>
        fixture.Generator<FileInfo>().WithDefaults();

    public static Generator<FileInfo> WithDefaults(this Generator<FileInfo> generator) =>
        generator.ForInstance(f => f.SetDefaults());

    public static Generator<FileInfo> WithName(this Generator<FileInfo> generator, string name) =>
        generator.ForInstance(f => f.SetName(name));

    public static Generator<FileInfo> WithSummary(
        this Generator<FileInfo> generator,
        string summary
    ) => generator.ForInstance(f => f.SetSummary(summary));

    public static Generator<FileInfo> WithFileName(
        this Generator<FileInfo> generator,
        string fileName
    ) => generator.ForInstance(f => f.SetFileName(fileName));

    public static Generator<FileInfo> WithSize(this Generator<FileInfo> generator, string size) =>
        generator.ForInstance(f => f.SetSize(size));

    public static Generator<FileInfo> WithType(this Generator<FileInfo> generator, FileType type) =>
        generator.ForInstance(f => f.SetType(type));

    public static Generator<FileInfo> WithUserName(
        this Generator<FileInfo> generator,
        string userName
    ) => generator.ForInstance(f => f.SetUserName(userName));

    public static InstanceSetters<FileInfo> SetDefaults(this InstanceSetters<FileInfo> setters) =>
        setters
            .SetDefault(f => f.Id)
            .SetDefault(f => f.Name)
            .SetDefault(f => f.Summary)
            .SetDefault(f => f.FileName)
            .Set(f => f.FileName, (_, f) => $"{f.FileName}.txt")
            .Set(f => f.Size, faker => $"{faker.Random.Number()} KB")
            .Set(f => f.Type, FileType.Ancillary)
            .SetDefault(f => f.Created);

    public static InstanceSetters<FileInfo> SetName(
        this InstanceSetters<FileInfo> setters,
        string name
    ) => setters.Set(f => f.Name, name);

    public static InstanceSetters<FileInfo> SetSummary(
        this InstanceSetters<FileInfo> setters,
        string summary
    ) => setters.Set(f => f.Summary, summary);

    public static InstanceSetters<FileInfo> SetFileName(
        this InstanceSetters<FileInfo> setters,
        string fileName
    ) => setters.Set(f => f.FileName, fileName);

    public static InstanceSetters<FileInfo> SetSize(
        this InstanceSetters<FileInfo> setters,
        string size
    ) => setters.Set(f => f.Size, size);

    public static InstanceSetters<FileInfo> SetType(
        this InstanceSetters<FileInfo> setters,
        FileType type
    ) => setters.Set(f => f.Type, type);

    public static InstanceSetters<FileInfo> SetUserName(
        this InstanceSetters<FileInfo> setters,
        string userName
    ) => setters.Set(f => f.UserName, userName);
}
