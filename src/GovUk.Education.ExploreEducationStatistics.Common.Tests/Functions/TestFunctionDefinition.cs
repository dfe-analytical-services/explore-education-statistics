using System.Collections.Immutable;
using Microsoft.Azure.Functions.Worker;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Functions;

public class TestFunctionDefinition : FunctionDefinition
{
    public override ImmutableArray<FunctionParameter> Parameters { get; } = [];

    public override string PathToAssembly => string.Empty;

    public override string EntryPoint => string.Empty;

    public override string Id => string.Empty;

    public override string Name => string.Empty;

    public override IImmutableDictionary<string, BindingMetadata> InputBindings { get; } =
        new Dictionary<string, BindingMetadata>().ToImmutableDictionary();

    public override IImmutableDictionary<string, BindingMetadata> OutputBindings { get; } =
        new Dictionary<string, BindingMetadata>().ToImmutableDictionary();
}
