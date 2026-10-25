//! Model-free guard for `common::split_subset`: a misspelt name would otherwise silently shrink the
//! split replay. Needs only the committed goldens, never an ONNX artifact.

mod common;

use common::engines::CHECKPOINTS;
use common::golden_data::CheckpointGoldenData;
use common::split_subset::NAMES;

/// The list both SDKs must carry. `laya-dotnet/tests/Laya.Tests/SplitLayoutSubsetTests.cs` asserts
/// the same literal set, so the two lists cannot drift apart without a test failing.
const EXPECTED: [&str; 10] = [
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

#[test]
fn subset_is_the_shared_list() {
    let mut want = EXPECTED.to_vec();
    let mut got = NAMES.to_vec();
    want.sort_unstable();
    got.sort_unstable();
    assert_eq!(want, got);
}

#[test]
fn every_subset_name_is_a_success_case_in_the_committed_goldens() {
    for checkpoint in CHECKPOINTS {
        let golden = CheckpointGoldenData::for_checkpoint(checkpoint.subdir());
        assert!(
            golden.available(),
            "no committed golden index for '{}'",
            checkpoint.subdir()
        );
        let cases: Vec<String> = golden
            .index()
            .into_iter()
            .filter(|c| !c.expects_error)
            .map(|c| c.name)
            .collect();
        let missing: Vec<&&str> = NAMES
            .iter()
            .filter(|n| !cases.iter().any(|c| c == **n))
            .collect();
        assert!(
            missing.is_empty(),
            "{}: not a success case in index.json: {missing:?}",
            checkpoint.subdir()
        );
    }
}
