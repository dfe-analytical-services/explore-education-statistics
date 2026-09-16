import unittest
from unittest.mock import patch

from tests.libs.ui_test_notification import UiTestNotification
from tests.libs.ui_test_notifier import UiTestNotifier, logger

WEBHOOK_URL = "https://example.com/teams"


class UiTestNotifierTests(unittest.TestCase):
    def setUp(self):
        self.notification = UiTestNotification(title="✅ UI tests on dev — admin")

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_sends_to_both_channels(self, teams_service_mock, slack_service_mock):
        UiTestNotifier.create(enable_slack=True, teams_webhook_url=WEBHOOK_URL).send(self.notification)

        teams_service_mock.assert_called_once_with(WEBHOOK_URL)
        teams_service_mock.return_value.send_test_report.assert_called_once_with(self.notification.to_teams_card())
        slack_service_mock.return_value.send_notification.assert_called_once_with(self.notification)

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_skips_teams_without_a_webhook_url(self, teams_service_mock, slack_service_mock):
        with patch.dict("os.environ", {}, clear=True):
            UiTestNotifier.create(enable_slack=True).send(self.notification)

        teams_service_mock.assert_not_called()
        slack_service_mock.return_value.send_notification.assert_called_once()

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_takes_the_teams_webhook_url_from_the_environment(self, teams_service_mock, _slack_service_mock):
        with patch.dict("os.environ", {"TEAMS_UI_TESTS_WEBHOOK_URL": WEBHOOK_URL}, clear=True):
            UiTestNotifier.create(enable_slack=False).send(self.notification)

        teams_service_mock.assert_called_once_with(WEBHOOK_URL)

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_skips_slack_when_notifications_are_not_enabled(self, teams_service_mock, slack_service_mock):
        UiTestNotifier.create(enable_slack=False, teams_webhook_url=WEBHOOK_URL).send(self.notification)

        slack_service_mock.assert_not_called()
        teams_service_mock.return_value.send_test_report.assert_called_once()

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_skips_both_channels_for_a_local_run(self, teams_service_mock, slack_service_mock):
        with patch.dict("os.environ", {}, clear=True):
            self.assertIsNone(UiTestNotifier.create(enable_slack=False).send(self.notification))

        teams_service_mock.assert_not_called()
        slack_service_mock.assert_not_called()

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_notifies_teams_even_when_slack_fails(self, teams_service_mock, slack_service_mock):
        slack_service_mock.return_value.send_notification.side_effect = Exception("connection refused")

        UiTestNotifier.create(enable_slack=True, teams_webhook_url=WEBHOOK_URL).send(self.notification)

        teams_service_mock.return_value.send_test_report.assert_called_once()

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_building_a_notifier_reports_nothing(self, _teams_service_mock, _slack_service_mock):
        """
        The runner builds a stand-in notifier before it can know whether Slack is enabled,
        so building one must not claim that a channel is being skipped.
        """
        with patch.dict("os.environ", {}, clear=True):
            with self.assertNoLogs("tests.libs.ui_test_notifier", level="INFO"):
                UiTestNotifier.create(enable_slack=False)

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_reports_the_skipped_channels_when_sending(self, _teams_service_mock, _slack_service_mock):
        with patch.dict("os.environ", {}, clear=True):
            with self.assertLogs("tests.libs.ui_test_notifier", level="INFO") as logs:
                UiTestNotifier.create(enable_slack=False).send(self.notification)

        self.assertEqual(2, len(logs.records))

    @patch("tests.libs.ui_test_notifier.SlackService")
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_does_not_report_skipping_a_channel_it_used(self, _teams_service_mock, _slack_service_mock):
        with self.assertLogs("tests.libs.ui_test_notifier", level="INFO") as logs:
            UiTestNotifier.create(enable_slack=True, teams_webhook_url=WEBHOOK_URL).send(self.notification)
            logger.info("nothing was skipped")

        self.assertEqual(["nothing was skipped"], [record.getMessage() for record in logs.records])

    @patch("tests.libs.ui_test_notifier.SlackService", side_effect=AssertionError("SLACK_APP_TOKEN is not set"))
    @patch("tests.libs.ui_test_notifier.TeamsService")
    def test_missing_slack_token_does_not_fail_the_test_run(self, teams_service_mock, _slack_service_mock):
        """
        A missing or rotated SLACK_APP_TOKEN must not turn a passing UI test run into a
        failing one, as it did when the runner constructed SlackService directly inside
        the try block that reports pipeline failures.
        """
        notifier = UiTestNotifier.create(enable_slack=True, teams_webhook_url=WEBHOOK_URL)

        self.assertIsNone(notifier.send(self.notification))
        teams_service_mock.return_value.send_test_report.assert_called_once()


if __name__ == "__main__":
    unittest.main()
