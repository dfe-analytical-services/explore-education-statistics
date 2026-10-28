import unittest
from unittest.mock import patch

from tests.libs.ui_test_notification import UiTestNotification
from tests.libs.ui_test_notifier import UiTestNotifier, logger

WEBHOOK_URL = "https://example.com/teams"


class UiTestNotifierTests(unittest.TestCase):
    def setUp(self):
        self.notification = UiTestNotification(title="✅ UI tests on dev — admin")

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_sends_to_teams(self, teams_service_mock):
        UiTestNotifier.create(teams_webhook_url=WEBHOOK_URL).send(self.notification)

        teams_service_mock.assert_called_once_with(WEBHOOK_URL)
        teams_service_mock.return_value.send_test_report.assert_called_once_with(self.notification.to_teams_card())

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_takes_the_teams_webhook_url_from_the_environment(self, teams_service_mock):
        with patch.dict("os.environ", {"TEAMS_UI_TESTS_WEBHOOK_URL": WEBHOOK_URL}, clear=True):
            UiTestNotifier.create().send(self.notification)

        teams_service_mock.assert_called_once_with(WEBHOOK_URL)

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_skips_teams_for_a_local_run(self, teams_service_mock):
        with patch.dict("os.environ", {}, clear=True):
            self.assertFalse(UiTestNotifier.create().send(self.notification))

        teams_service_mock.assert_not_called()

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_reports_delivery_when_teams_accepted(self, teams_service_mock):
        teams_service_mock.return_value.send_test_report.return_value = True

        self.assertTrue(UiTestNotifier.create(teams_webhook_url=WEBHOOK_URL).send(self.notification))

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_reports_no_delivery_when_teams_rejected(self, teams_service_mock):
        teams_service_mock.return_value.send_test_report.return_value = False

        self.assertFalse(UiTestNotifier.create(teams_webhook_url=WEBHOOK_URL).send(self.notification))

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_teams_failure_does_not_fail_the_test_run(self, teams_service_mock):
        teams_service_mock.return_value.send_test_report.side_effect = Exception("boom")

        self.assertFalse(UiTestNotifier.create(teams_webhook_url=WEBHOOK_URL).send(self.notification))

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_building_a_notifier_reports_nothing(self, _teams_service_mock):
        """
        The runner builds a stand-in notifier before the arguments are parsed, so building
        one must not claim that a channel is being skipped.
        """
        with patch.dict("os.environ", {}, clear=True):
            with self.assertNoLogs("tests.libs.ui_test_notifier", level="INFO"):
                UiTestNotifier.create()

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_reports_skipping_teams_when_sending(self, _teams_service_mock):
        with patch.dict("os.environ", {}, clear=True):
            with self.assertLogs("tests.libs.ui_test_notifier", level="INFO") as logs:
                UiTestNotifier.create().send(self.notification)

        self.assertEqual(1, len(logs.records))

    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_does_not_report_skipping_a_channel_it_used(self, _teams_service_mock):
        with self.assertLogs("tests.libs.ui_test_notifier", level="INFO") as logs:
            UiTestNotifier.create(teams_webhook_url=WEBHOOK_URL).send(self.notification)
            logger.info("nothing was skipped")

        self.assertEqual(["nothing was skipped"], [record.getMessage() for record in logs.records])


if __name__ == "__main__":
    unittest.main()
