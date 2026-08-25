import unittest
from unittest.mock import MagicMock, patch

from tests.libs.snapshot_notifications import (
    PULL_REQUESTS_URL,
    SnapshotNotifier,
    snapshot_process_failed,
    snapshots_do_not_match,
)


def _teams_card_body(notification) -> list[dict]:
    return notification.to_teams_card()["attachments"][0]["content"]["body"]


class SnapshotsDoNotMatchTests(unittest.TestCase):
    def setUp(self):
        self.notification = snapshots_do_not_match()

    def test_slack_blocks_contain_title_and_link(self):
        header, section = self.notification.to_slack_blocks()

        self.assertEqual("⚠️ Snapshots do not match", header["text"]["text"])
        self.assertEqual("mrkdwn", section["text"]["type"])
        self.assertEqual(f"See <{PULL_REQUESTS_URL}|pull request> for more details", section["text"]["text"])

    def test_teams_card_contains_same_title_and_link(self):
        title, body = _teams_card_body(self.notification)

        self.assertEqual("⚠️ Snapshots do not match", title["text"])
        self.assertEqual("warning", title["color"])
        self.assertEqual(f"See [pull request]({PULL_REQUESTS_URL}) for more details", body["text"])


class SnapshotProcessFailedTests(unittest.TestCase):
    def setUp(self):
        self.notification = snapshot_process_failed("Something went wrong")

    def test_slack_blocks_contain_title_and_message(self):
        header, section = self.notification.to_slack_blocks()

        self.assertEqual("❌ Snapshot process failed", header["text"]["text"])
        self.assertEqual("plain_text", section["text"]["type"])
        self.assertEqual("Something went wrong", section["text"]["text"])

    def test_teams_card_contains_same_title_and_message(self):
        title, body = _teams_card_body(self.notification)

        self.assertEqual("❌ Snapshot process failed", title["text"])
        self.assertEqual("attention", title["color"])
        self.assertEqual("Something went wrong", body["text"])

    def test_message_is_not_formatted_when_it_has_no_link(self):
        notification = snapshot_process_failed("Unexpected token {foo} in response")

        _, section = notification.to_slack_blocks()
        self.assertEqual("Unexpected token {foo} in response", section["text"]["text"])

    def test_message_accepts_an_exception(self):
        notification = snapshot_process_failed(ValueError("bad value"))

        _, section = notification.to_slack_blocks()
        self.assertEqual("bad value", section["text"]["text"])


class SnapshotNotifierTests(unittest.TestCase):
    def setUp(self):
        self.notification = snapshots_do_not_match()

    @patch("tests.libs.snapshot_notifications.WebhookClient")
    @patch("tests.libs.snapshot_notifications.TeamsService")
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

    @patch("tests.libs.snapshot_notifications.WebhookClient")
    @patch("tests.libs.snapshot_notifications.TeamsService")
    def test_skips_channels_without_a_webhook_url(self, teams_service_mock, webhook_client_mock):
        SnapshotNotifier().send(self.notification)

        teams_service_mock.assert_not_called()
        webhook_client_mock.assert_not_called()

    @patch("tests.libs.snapshot_notifications.WebhookClient")
    @patch("tests.libs.snapshot_notifications.TeamsService")
    def test_notifies_teams_even_when_slack_fails(self, teams_service_mock, webhook_client_mock):
        webhook_client_mock.return_value.send.return_value = MagicMock(status_code=500, body="error")

        notifier = SnapshotNotifier(
            slack_webhook_url="https://example.com/slack",
            teams_webhook_url="https://example.com/teams",
        )

        with self.assertRaises(AssertionError):
            notifier.send(self.notification)

        teams_service_mock.return_value.send_snapshot_notification.assert_called_once()
