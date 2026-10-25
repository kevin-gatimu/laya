//! The golden cases replayed on the split ONNX layout (`encoder.onnx` + `head.onnx`); the fused
//! layout still replays every case.
//!
//! Rationale: the split replay re-ran every golden a second time for a property that is about
//! artifact loading rather than batch semantics (maintainer's remark on PR #1359,
//! <https://github.com/NandhaKishorM/laya/pull/1359#issuecomment-6400726102>). What is specific to
//! the split layout is the encoder-to-head handoff (hidden states, marker positions, masks), so
//! the subset keeps every question type (choice, score, noul), both batch shapes (one row, and a
//! padded multi-row batch of states of different widths), a max_len-truncated row, the marker axis
//! at its smallest and largest, and long-document windows. Every checkpoint replays all of them,
//! with the same tolerances as the fused replay.
//!
//! Cases outside the subset are left out of the split loops, never skipped, so the skip guard keeps
//! its meaning; a subset case whose split artifacts are missing still prints the `skip:` line from
//! [`super::engines::load_split`] and so still fails the guard. The .NET SDK keeps the identical
//! list in `laya-dotnet/tests/Laya.Tests/SplitLayoutSubset.cs`.

use super::golden_data::GoldenCaseInfo;

/// Golden case names (the same in every checkpoint's index), each with the reason it is kept.
pub const NAMES: [&str; 10] = [
    // Standard predict cases.
    // 4 rows, all three question types, different option counts: marker axis padding and masks
    "quickstart",
    // single row, score question
    "dict_state",
    // single row, noul question
    "list_state",
    // single row, choice question, marker axis padded to the graph's minimum of 2
    "single_option",
    // single row, 40 markers: the widest marker axis through the head
    "many_options",
    // Batch cases.
    // one state, three question types: the smallest batch shape
    "batch_one_state",
    // sort + chunking: a padded run of states of different widths, then a partial run
    "batch_sort_partial_window",
    // a state far over max_len truncated to the cap (a max_len-wide run) next to short ones
    "batch_sorted",
    // Long-document cases.
    // non-overlapping windows chunked across runs, with a partial last run
    "long_punctuation",
    // overlapping windows (stride < window) aggregated across one run
    "long_list",
];

/// Whether `case` is replayed on the split layout.
pub fn contains(case: &GoldenCaseInfo) -> bool {
    NAMES.contains(&case.name.as_str())
}

/// The fused layout replays every case.
pub fn all(_case: &GoldenCaseInfo) -> bool {
    true
}
