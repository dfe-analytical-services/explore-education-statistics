import argparse
import tempfile
import unittest
from pathlib import Path
from unittest.mock import MagicMock, patch

import run_tests
from tests.libs.run_results import notification_was_sent


def _args(environment: str = "dev", tests: str = "tests/admin") -> argparse.Namespace:
    return argparse.Namespace(env=environment, tests=tests)


class SendTestReportTests(unittest.TestCase):
    """
    The runner must always report the outcome of a run somewhere, and must only record that
    it reported when a channel actually accepted the message.
    """

    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.results_directory = Path(self.temporary_directory.name)
        self.notifier = MagicMock()
        self.notifier.send.return_value = True

        results_folder = patch.object(run_tests, "main_results_folder", str(self.results_directory))
        results_folder.start()
        self.addCleanup(results_folder.stop)

    def tearDown(self):
        self.temporary_directory.cleanup()

    def _write_report(self, path: Path, passed: int, failed: int, skipped: int):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(
            "<robot><statistics><total>"
            f'<stat pass="{passed}" fail="{failed}" skip="{skipped}" />'
            "</total></statistics></robot>",
            encoding="utf-8",
        )

    def _write_readable_results(self):
        self._write_report(self.results_directory / "output.xml", 10, 0, 0)
        self._write_report(self.results_directory / "run-1" / "output.xml", 10, 0, 0)

    @patch("run_tests.time.sleep")
    def test_sends_the_test_report_when_the_results_can_be_read(self, _sleep_mock):
        self._write_readable_results()

        run_tests._send_test_report(self.notifier, _args(), [], 1)

        notification = self.notifier.send.call_args.args[0]
        self.assertEqual("✅ UI tests on dev — admin", notification.title)

    @patch("run_tests.time.sleep")
    def test_reports_a_failure_to_report_when_the_results_cannot_be_read(self, _sleep_mock):
        """
        Unreadable results used to leave the run reporting to nobody at all, which is worse
        than reporting that it could not build its report.
        """
        run_tests._send_test_report(self.notifier, _args(), [], 1)

        notification = self.notifier.send.call_args.args[0]
        self.assertEqual("❌ UI test run failed on dev — admin", notification.title)
        self.assertIn("Unable to find test run reports", notification.body)

    @patch("run_tests.time.sleep")
    def test_does_not_fail_the_run_when_the_report_cannot_be_built(self, _sleep_mock):
        self.assertIsNone(run_tests._send_test_report(self.notifier, _args(), [], 1))

    @patch("run_tests.time.sleep")
    def test_records_that_it_reported_when_a_channel_accepted(self, _sleep_mock):
        self._write_readable_results()

        run_tests._send_test_report(self.notifier, _args(), [], 1)

        self.assertTrue(notification_was_sent(self.results_directory))

    @patch("run_tests.run_results.record_notification_sent", side_effect=OSError("no space left on device"))
    @patch("run_tests.time.sleep")
    def test_a_failure_to_record_does_not_fail_a_passing_run(self, _sleep_mock, _record_mock):
        """
        Recording that we reported is bookkeeping. Letting it raise would put an otherwise
        passing run into the exception handler, which would then send a second
        notification claiming the run had failed.
        """
        self._write_readable_results()

        self.assertIsNone(run_tests._send_test_report(self.notifier, _args(), [], 1))

        self.notifier.send.assert_called_once()

    @patch("run_tests.time.sleep")
    def test_does_not_record_that_it_reported_when_no_channel_accepted(self, _sleep_mock):
        """
        The marker is what tells the pipeline a suite reported itself, so a run that reached
        nobody must leave it unwritten for the fallback job to pick up.
        """
        self._write_readable_results()
        self.notifier.send.return_value = False

        run_tests._send_test_report(self.notifier, _args(), [], 1)

        self.assertFalse(notification_was_sent(self.results_directory))


if __name__ == "__main__":
    unittest.main()
