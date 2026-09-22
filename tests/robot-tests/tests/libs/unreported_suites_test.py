import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from scripts.send_teams_unreported_suites import SUITES, UNKNOWN_STATUS, find_unreported_suites, send_unreported_suites
from tests.libs.run_results import record_notification_sent
from tests.libs.ui_test_notification import unreported_suites

BUILD_ID = "123"

# Markers are scoped to the run that wrote them, so reading and writing must agree on it.
ALL_SUCCEEDED = {
    "BUILD_BUILDID": BUILD_ID,
    **{status_environment_variable: "Succeeded" for _, _, status_environment_variable in SUITES},
}


class FindUnreportedSuitesTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.artifacts_directory = Path(self.temporary_directory.name)

    def tearDown(self):
        self.temporary_directory.cleanup()

    def _mark_reported(self, *artifact_names: str):
        with patch.dict("os.environ", {"BUILD_BUILDID": BUILD_ID}):
            for artifact_name in artifact_names:
                record_notification_sent(self.artifacts_directory / artifact_name)

    def test_finds_nothing_when_every_suite_reported(self):
        self._mark_reported(*[artifact_name for _, artifact_name, _ in SUITES])

        with patch.dict("os.environ", ALL_SUCCEEDED, clear=True):
            self.assertEqual((), find_unreported_suites(self.artifacts_directory, SUITES))

    def test_finds_the_suite_whose_artifact_has_no_marker(self):
        self._mark_reported(*[artifact_name for _, artifact_name, _ in SUITES if artifact_name != "test-results-admin"])

        with patch.dict("os.environ", {**ALL_SUCCEEDED, "UI_TEST_ADMIN_RESULT": "Canceled"}, clear=True):
            self.assertEqual((("Admin", "Canceled"),), find_unreported_suites(self.artifacts_directory, SUITES))

    def test_ignores_a_failing_suite_that_reported_itself(self):
        """
        A suite whose tests failed reports itself from the runner, so the pipeline must
        not report it a second time.
        """
        self._mark_reported(*[artifact_name for _, artifact_name, _ in SUITES])

        with patch.dict("os.environ", {**ALL_SUCCEEDED, "UI_TEST_ADMIN_RESULT": "Failed"}, clear=True):
            self.assertEqual((), find_unreported_suites(self.artifacts_directory, SUITES))

    def test_reports_an_unknown_status_when_the_job_result_is_missing(self):
        with patch.dict("os.environ", {}, clear=True):
            unreported = find_unreported_suites(self.artifacts_directory, SUITES)

        self.assertEqual(len(SUITES), len(unreported))
        self.assertTrue(all(status == UNKNOWN_STATUS for _, status in unreported))


class SendUnreportedSuitesTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.artifacts_directory = Path(self.temporary_directory.name)

    def tearDown(self):
        self.temporary_directory.cleanup()

    @patch("scripts.send_teams_unreported_suites.TeamsService")
    def test_sends_nothing_when_every_suite_reported(self, teams_service_mock):
        with patch.dict("os.environ", {"BUILD_BUILDID": BUILD_ID}):
            for _, artifact_name, _ in SUITES:
                record_notification_sent(self.artifacts_directory / artifact_name)

        with patch.dict("os.environ", ALL_SUCCEEDED, clear=True):
            self.assertFalse(send_unreported_suites(self.artifacts_directory, "dev"))

        teams_service_mock.return_value.send_test_report.assert_not_called()

    @patch("scripts.send_teams_unreported_suites.TeamsService")
    def test_sends_one_card_listing_the_suites_that_did_not_report(self, teams_service_mock):
        teams_service_mock.return_value.send_test_report.return_value = True

        with patch.dict("os.environ", {**ALL_SUCCEEDED, "UI_TEST_SEED_DATA_RESULT": "Canceled"}, clear=True):
            self.assertTrue(send_unreported_suites(self.artifacts_directory, "preprod"))

        teams_service_mock.return_value.send_test_report.assert_called_once()
        card = teams_service_mock.return_value.send_test_report.call_args.args[0]
        content = card["attachments"][0]["content"]
        facts = content["body"][1]["facts"]
        self.assertEqual("preprod", next(fact["value"] for fact in facts if fact["title"] == "Environment"))
        self.assertEqual(str(len(SUITES)), next(fact["value"] for fact in facts if fact["title"] == "Suites"))
        self.assertIn("Seed data — job status: Canceled", content["body"][3]["text"])


class UnreportedSuitesNotificationTests(unittest.TestCase):
    def setUp(self):
        self.notification = unreported_suites("preprod", (("Admin", "Canceled"), ("Public", "Failed")))

    def test_title_names_the_environment(self):
        self.assertEqual("❌ UI test suites did not report on preprod", self.notification.title)

    def test_slack_blocks_list_the_suites(self):
        listed = self.notification.to_slack_blocks()[4]["elements"][0]["elements"]

        self.assertEqual("Admin — job status: Canceled", listed[0]["elements"][0]["text"])
        self.assertEqual("Public — job status: Failed", listed[1]["elements"][0]["text"])

    def test_teams_card_lists_the_same_suites(self):
        body = self.notification.to_teams_card()["attachments"][0]["content"]["body"]

        self.assertEqual("Suites that did not report (2)", body[2]["text"])
        self.assertEqual("- Admin — job status: Canceled\n- Public — job status: Failed", body[3]["text"])


if __name__ == "__main__":
    unittest.main()
