"""
Sends the public frontend snapshot validation notifications to chat services.
"""

from typing import Optional

from slack_sdk.webhook import WebhookClient
from tests.libs.logger import get_logger
from tests.libs.snapshot_notification import SnapshotNotification
from tests.libs.teams import TeamsService

logger = get_logger(__name__)


class SnapshotNotifier:
    """
    Sends a notification to every channel it has been given a webhook URL for.
    Channels without a webhook URL are skipped, so that the script can be run
    locally without notifying anyone.
    """

    def __init__(self, slack_webhook_url: Optional[str] = None, teams_webhook_url: Optional[str] = None):
        self.slack_webhook_url = slack_webhook_url
        self.teams_service = TeamsService(teams_webhook_url) if teams_webhook_url else None

    def send(self, notification: SnapshotNotification) -> None:
        # Teams is notified first as it never raises, so a Slack outage cannot
        # stop the message reaching Teams.
        self._send_to_teams(notification)
        self._send_to_slack(notification)

    def _send_to_teams(self, notification: SnapshotNotification) -> None:
        if not self.teams_service:
            logger.info("No Teams webhook URL was given; skipping Teams notification")
            return

        self.teams_service.send_snapshot_notification(notification.to_teams_card())

    def _send_to_slack(self, notification: SnapshotNotification) -> None:
        if not self.slack_webhook_url:
            logger.info("No Slack webhook URL was given; skipping Slack notification")
            return

        response = WebhookClient(self.slack_webhook_url).send(
            text=notification.title, blocks=notification.to_slack_blocks()
        )

        assert (
            response.status_code == 200 and response.body == "ok"
        ), f"Slack notification failed with status code: {response.status_code}"
