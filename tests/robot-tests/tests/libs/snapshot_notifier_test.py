import unittest
from unittest.mock import patch

from tests.libs.snapshot_notification import snapshots_do_not_match
from tests.libs.snapshot_notifier import SnapshotNotifier


class SnapshotNotifierTests(unittest.TestCase):
    def setUp(self):
        self.notification = snapshots_do_not_match()

    @patch("tests.libs.snapshot_notifier.TeamsService")
    def test_sends_to_teams(self, teams_service_mock):
        SnapshotNotifier(teams_webhook_url="https://example.com/teams").send(self.notification)

        teams_service_mock.assert_called_once_with("https://example.com/teams")
        teams_service_mock.return_value.send_snapshot_notification.assert_called_once_with(
            self.notification.to_teams_card()
        )

    @patch("tests.libs.snapshot_notifier.TeamsService")
    def test_skips_teams_without_a_webhook_url(self, teams_service_mock):
        SnapshotNotifier().send(self.notification)

        teams_service_mock.assert_not_called()
