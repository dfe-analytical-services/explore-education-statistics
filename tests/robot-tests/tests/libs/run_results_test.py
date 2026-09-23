import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch
from xml.etree import ElementTree

from tests.libs.run_results import (
    DEFINITELY,
    DEFINITELY_NOT,
    LIKELY,
    LOCAL_RUN_ID,
    UNLIKELY,
    RunResults,
    RunTotals,
    current_run_id,
    find_run_directories,
    notification_was_sent,
    read_run_results,
    read_totals,
    record_notification_sent,
)


def _write_report(path: Path, passed: int, failed: int, skipped: int):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        "<robot><statistics><total>"
        f'<stat pass="{passed}" fail="{failed}" skip="{skipped}" />'
        "</total></statistics></robot>",
        encoding="utf-8",
    )


class ReadTotalsTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.results_directory = Path(self.temporary_directory.name)

    def tearDown(self):
        self.temporary_directory.cleanup()

    def test_reads_the_totals_from_a_report(self):
        _write_report(self.results_directory / "output.xml", 8, 2, 1)

        totals = read_totals(self.results_directory / "output.xml")

        self.assertEqual((8, 2, 1, 11), (totals.passed, totals.failed, totals.skipped, totals.total))

    def test_raises_for_a_report_without_statistics(self):
        (self.results_directory / "output.xml").write_text("<robot />", encoding="utf-8")

        with self.assertRaises(ValueError):
            read_totals(self.results_directory / "output.xml")

    def test_raises_for_a_malformed_report(self):
        (self.results_directory / "output.xml").write_text("not xml", encoding="utf-8")

        with self.assertRaises(ElementTree.ParseError):
            read_totals(self.results_directory / "output.xml")


class FindRunDirectoriesTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.results_directory = Path(self.temporary_directory.name)

    def tearDown(self):
        self.temporary_directory.cleanup()

    def test_orders_run_directories_numerically(self):
        for run_number in (10, 2, 1):
            (self.results_directory / f"run-{run_number}").mkdir()

        run_numbers = [run_number for run_number, _ in find_run_directories(self.results_directory)]

        self.assertEqual([1, 2, 10], run_numbers)

    def test_ignores_directories_that_are_not_run_folders(self):
        (self.results_directory / "run-1").mkdir()
        (self.results_directory / "downloads").mkdir()
        (self.results_directory / "run-abc").mkdir()

        self.assertEqual(1, len(find_run_directories(self.results_directory)))

    def test_returns_nothing_when_there_are_no_run_folders(self):
        self.assertEqual([], find_run_directories(self.results_directory))


class FlakinessTests(unittest.TestCase):
    def _results(self, merged: RunTotals, final: RunTotals, run_attempts: int) -> RunResults:
        return RunResults(
            environment="dev",
            suite_label="admin",
            run_attempts=run_attempts,
            merged=merged,
            final=final,
        )

    def test_definitely_flaky_when_a_rerun_finally_passed(self):
        results = self._results(RunTotals(9, 1, 0), RunTotals(10, 0, 0), run_attempts=2)

        self.assertEqual(DEFINITELY, results.flakiness)

    def test_definitely_not_flaky_for_a_single_clean_run(self):
        results = self._results(RunTotals(10, 0, 0), RunTotals(10, 0, 0), run_attempts=1)

        self.assertEqual(DEFINITELY_NOT, results.flakiness)

    def test_likely_flaky_when_the_reports_disagree_on_failures(self):
        results = self._results(RunTotals(8, 3, 0), RunTotals(9, 1, 0), run_attempts=2)

        self.assertEqual(LIKELY, results.flakiness)

    def test_unlikely_flaky_when_the_reports_agree_on_failures(self):
        results = self._results(RunTotals(8, 2, 0), RunTotals(8, 2, 0), run_attempts=2)

        self.assertEqual(UNLIKELY, results.flakiness)


class ArchiveTests(unittest.TestCase):
    def _results(self, run_attempts: int = 1, failed_suites: tuple = (), suite_label: str = "admin") -> RunResults:
        return RunResults(
            environment="dev",
            suite_label=suite_label,
            run_attempts=run_attempts,
            merged=RunTotals(10, 0, 0),
            final=RunTotals(10, 0, 0),
            failed_suites=failed_suites,
        )

    def test_archives_when_a_suite_failed(self):
        self.assertTrue(self._results(failed_suites=("tests/admin/bau/a.robot",)).attach_archive)

    def test_archives_when_more_than_one_attempt_was_needed(self):
        self.assertTrue(self._results(run_attempts=2).attach_archive)

    def test_does_not_archive_a_clean_single_run(self):
        self.assertFalse(self._results().attach_archive)

    def test_archive_name_replaces_characters_that_are_awkward_in_a_filename(self):
        name = self._results(suite_label="all tests").archive_name

        self.assertTrue(name.startswith("UI-test-report-all-tests-dev-"), name)
        self.assertTrue(name.endswith(".zip"), name)


class NotificationMarkerTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.results_directory = Path(self.temporary_directory.name)

    def tearDown(self):
        self.temporary_directory.cleanup()

    def test_no_marker_until_a_notification_is_recorded(self):
        self.assertFalse(notification_was_sent(self.results_directory))

    def test_records_that_a_notification_was_sent(self):
        record_notification_sent(self.results_directory)

        self.assertTrue(notification_was_sent(self.results_directory))

    def test_creates_the_results_directory_when_the_run_never_made_one(self):
        results_directory = self.results_directory / "test-results"

        record_notification_sent(results_directory)

        self.assertTrue(notification_was_sent(results_directory))

    def test_recording_twice_is_harmless(self):
        record_notification_sent(self.results_directory)
        record_notification_sent(self.results_directory)

        self.assertTrue(notification_was_sent(self.results_directory))

    def test_ignores_a_marker_left_by_a_previous_run(self):
        """
        The agents are self-hosted and the artifacts are downloaded into a workspace that
        can outlive a run, so a stale marker must not be mistaken for this run's and
        suppress its alert.
        """
        with patch.dict("os.environ", {"BUILD_BUILDID": "100"}, clear=True):
            record_notification_sent(self.results_directory)

        with patch.dict("os.environ", {"BUILD_BUILDID": "101"}, clear=True):
            self.assertFalse(notification_was_sent(self.results_directory))

    def test_recognises_a_marker_from_the_same_run(self):
        with patch.dict("os.environ", {"BUILD_BUILDID": "100"}, clear=True):
            record_notification_sent(self.results_directory)

            self.assertTrue(notification_was_sent(self.results_directory))

    def test_falls_back_to_the_release_id_when_there_is_no_build_id(self):
        with patch.dict("os.environ", {"RELEASE_RELEASEID": "6362"}, clear=True):
            self.assertEqual("6362", current_run_id())

    def test_identifies_a_local_run(self):
        with patch.dict("os.environ", {}, clear=True):
            self.assertEqual(LOCAL_RUN_ID, current_run_id())


class ReadRunResultsTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.results_directory = Path(self.temporary_directory.name)

    def tearDown(self):
        self.temporary_directory.cleanup()

    def _read(self, run_attempts: int = 1) -> RunResults:
        return read_run_results(
            results_directory=self.results_directory,
            environment="preprod",
            suite_label="admin",
            run_attempts=run_attempts,
            failed_suites=("tests/admin/bau/a.robot",),
        )

    def test_reads_totals_from_the_merged_report_and_failures_from_the_final_run(self):
        _write_report(self.results_directory / "output.xml", 10, 3, 2)
        _write_report(self.results_directory / "run-1" / "output.xml", 10, 3, 2)
        _write_report(self.results_directory / "run-2" / "output.xml", 14, 1, 0)

        results = self._read(run_attempts=2)

        self.assertEqual((10, 3, 2), (results.merged.passed, results.merged.failed, results.merged.skipped))
        self.assertEqual(1, results.final.failed)
        self.assertEqual("preprod", results.environment)
        self.assertEqual(("tests/admin/bau/a.robot",), results.failed_suites)

    def test_uses_the_highest_numbered_run_directory(self):
        _write_report(self.results_directory / "output.xml", 10, 0, 0)
        _write_report(self.results_directory / "run-2" / "output.xml", 8, 2, 0)
        _write_report(self.results_directory / "run-10" / "output.xml", 10, 0, 0)

        self.assertEqual(0, self._read().final.failed)

    def test_reports_the_attempts_it_was_given_rather_than_counting_run_folders(self):
        """
        Rerunning the failed suites of a previous execution copies that execution's
        results into a "run-0" folder, so the folders outnumber this execution's attempts.
        """
        _write_report(self.results_directory / "output.xml", 10, 0, 0)
        _write_report(self.results_directory / "run-0" / "output.xml", 8, 2, 0)
        _write_report(self.results_directory / "run-1" / "output.xml", 10, 0, 0)

        self.assertEqual(1, self._read(run_attempts=1).run_attempts)

    def test_raises_when_there_are_no_run_folders(self):
        _write_report(self.results_directory / "output.xml", 10, 0, 0)

        with self.assertRaises(ValueError):
            self._read()


if __name__ == "__main__":
    unittest.main()
