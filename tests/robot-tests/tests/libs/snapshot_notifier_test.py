import unittest
from unittest.mock import MagicMock, patch
from urllib.error import URLError

from tests.libs.snapshot_notification import snapshots_do_not_match
from tests.libs.snapshot_notifier import SnapshotNotifier


class SnapshotNotifierTests(unittest.TestCase):
    def setUp(self):
        self.notification = snapshots_do_not_match()

    @patch("tests.libs.snapshot_notifier.WebhookClient")
    @patch("tests.libs.snapshot_notifier.TeamsService")
    def test_sends_to_both_channels(self, teams_service_mock, webhook_client_mock):
        webhook_client_mock.return_value.send.return_value = MagicMock(status_code=200, body="ok")

        SnapshotNotifier(
            slack_webhook_url="https://example.com/slack",
            teams_webhook_url="https://example.com/teams",
        ).send(self.notification)

        teams_service_mock.assert_called_once_with("https://example.com/teams")
        teams_service_mock.return_value.send_snapshot_notification.assert_called_once_with(
            self.notification.to_teams_card()
        )
        webhook_client_mock.assert_called_once_with("https://example.com/slack")
        webhook_client_mock.return_value.send.assert_called_once_with(
            text=self.notification.title, blocks=self.notification.to_slack_blocks()
        )

    @patch("tests.libs.snapshot_notifier.WebhookClient")
    @patch("tests.libs.snapshot_notifier.TeamsService")
    def test_skips_channels_without_a_webhook_url(self, teams_service_mock, webhook_client_mock):
        SnapshotNotifier().send(self.notification)

        teams_service_mock.assert_not_called()
        webhook_client_mock.assert_not_called()

    @patch("tests.libs.snapshot_notifier.WebhookClient")
    @patch("tests.libs.snapshot_notifier.TeamsService")
    def test_notifies_teams_even_when_slack_returns_an_error(self, teams_service_mock, webhook_client_mock):
        webhook_client_mock.return_value.send.return_value = MagicMock(status_code=500, body="error")

        self._notifier().send(self.notification)

        teams_service_mock.return_value.send_snapshot_notification.assert_called_once()

    @patch("tests.libs.snapshot_notifier.WebhookClient")
    @patch("tests.libs.snapshot_notifier.TeamsService")
    def test_notifies_teams_even_when_slack_is_unreachable(self, teams_service_mock, webhook_client_mock):
        webhook_client_mock.return_value.send.side_effect = URLError("connection refused")

        self._notifier().send(self.notification)

        teams_service_mock.return_value.send_snapshot_notification.assert_called_once()

    @patch("tests.libs.snapshot_notifier.WebhookClient")
    @patch("tests.libs.snapshot_notifier.TeamsService")
    def test_slack_failure_does_not_stop_the_snapshot_run(self, _teams_service_mock, webhook_client_mock):
        """
        A Slack outage must never propagate, as it would abandon the snapshot
        run before the updated snapshots are written and a pull request raised.
        """
        webhook_client_mock.return_value.send.side_effect = URLError("connection refused")

        self.assertIsNone(self._notifier().send(self.notification))

    def _notifier(self) -> SnapshotNotifier:
        return SnapshotNotifier(
            slack_webhook_url="https://example.com/slack",
            teams_webhook_url="https://example.com/teams",
        )
