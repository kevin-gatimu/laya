namespace Laya.Tests;

/// <summary>
/// Model-free guard for <see cref="SplitLayoutSubset"/>: a misspelt name would otherwise silently
/// shrink the split replay.
/// </summary>
public sealed class SplitLayoutSubsetTests
{
    /// <summary>
    /// The list both SDKs must carry. <c>laya-rust/tests/split_subset_tests.rs</c> asserts the same
    /// literal set, so the two lists cannot drift apart without a test failing.
    /// </summary>
    private static readonly string[] Expected =
    [
        "quickstart",
        "dict_state",
        "list_state",
        "single_option",
        "many_options",
        "batch_one_state",
        "batch_sort_partial_window",
        "batch_sorted",
        "long_punctuation",
        "long_list",
    ];

    [Fact]
    public void SubsetIsTheSharedList() =>
        Assert.Equal(Expected.Order(StringComparer.Ordinal), SplitLayoutSubset.AllNames.Order(StringComparer.Ordinal));

    [Theory]
    [InlineData(LayaCheckpoint.English)]
    [InlineData(LayaCheckpoint.Multilingual)]
    [InlineData(LayaCheckpoint.TypedDecisions)]
    public void EverySubsetNameIsASuccessCaseInTheCommittedGoldens(LayaCheckpoint checkpoint)
    {
        var golden = GoldenData.For(checkpoint);
        Assert.True(golden.Available, $"no committed golden index for {checkpoint}");
        var cases = golden.Index.Where(c => !c.ExpectsError).Select(c => c.Name).ToHashSet(StringComparer.Ordinal);
        var missing = SplitLayoutSubset.AllNames.Where(name => !cases.Contains(name)).ToList();
        Assert.True(missing.Count == 0, $"{checkpoint}: not a success case in index.json: {string.Join(", ", missing)}");
    }
}
