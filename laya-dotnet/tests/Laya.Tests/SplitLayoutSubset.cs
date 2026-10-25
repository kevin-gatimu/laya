namespace Laya.Tests;

/// <summary>
/// The golden cases replayed on the split ONNX layout (<c>encoder.onnx</c> + <c>head.onnx</c>).
/// The fused layout still replays every case.
/// </summary>
/// <remarks>
/// <para>
/// Rationale: the split replay re-ran every golden a second time for a property that is about
/// artifact loading rather than batch semantics (maintainer's remark on PR #1359,
/// https://github.com/NandhaKishorM/laya/pull/1359#issuecomment-6400726102). What is specific to the
/// split layout is the encoder-to-head handoff (hidden states, marker positions, masks), so the
/// subset keeps every question type (choice, score, noul), both batch shapes (one row, and a
/// padded multi-row batch of states of different widths), a max_len-truncated row, the marker
/// axis at its smallest and largest, and long-document windows. Every checkpoint replays all of
/// them, with the same tolerances as the fused replay.
/// </para>
/// <para>
/// Cases outside the subset are left out of the theory data of the split tests, never skipped, so
/// the skip guard keeps its meaning; a subset case whose split artifacts are missing still skips
/// in <see cref="AllEnginesFixture.RequireSplit"/> and so still fails the guard. The Rust SDK
/// keeps the identical list in <c>laya-rust/tests/common/split_subset.rs</c>.
/// </para>
/// </remarks>
internal static class SplitLayoutSubset
{
    /// <summary>Golden case names (the same in every checkpoint's index), each with the reason it is kept.</summary>
    private static readonly HashSet<string> Names = new(StringComparer.Ordinal)
    {
        // Standard predict cases.
        "quickstart",                 // 4 rows, all three question types, different option counts: marker axis padding and masks
        "dict_state",                 // single row, score question
        "list_state",                 // single row, noul question
        "single_option",              // single row, choice question, marker axis padded to the graph's minimum of 2
        "many_options",               // single row, 40 markers: the widest marker axis through the head

        // Batch cases.
        "batch_one_state",            // one state, three question types: the smallest batch shape
        "batch_sort_partial_window",  // sort + chunking: a padded run of states of different widths, then a partial run
        "batch_sorted",               // a state far over max_len truncated to the cap (a max_len-wide run) next to short ones

        // Long-document cases.
        "long_punctuation",           // non-overlapping windows chunked across runs, with a partial last run
        "long_list",                  // overlapping windows (stride < window) aggregated across one run
    };

    /// <summary>Every name in the subset.</summary>
    public static IReadOnlyCollection<string> AllNames => Names;

    /// <summary>Whether <paramref name="info"/> is replayed on the split layout.</summary>
    public static bool Contains(GoldenCaseInfo info) => Names.Contains(info.Name);
}
