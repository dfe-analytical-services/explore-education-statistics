"""
Sends the public frontend snapshot validation notifications to chat services.
"""

from typing import Optional

from slack_sdk.webhook import WebhookClient
from tests.libs.logger import get_logger
from tests.libs.snapshot_notification import SnapshotNotification
from tests.libs.teams import TeamsService

logger = get_logger(__name__)


# TODO: Remove slack messages sent when EES DfE Slack Workspace is retired EES-7608
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
        """
        Notifying is best effort. Neither channel raises, so an outage cannot
        abandon the snapshot run part way through and leave the updated
        snapshots unwritten. Teams is the primary channel, so it goes first.
        """
        self._send_to_teams(notification)
        self._send_to_slack(notification)

    def _send_to_teams(self, notification: SnapshotNotification) -> bool:
        if not self.teams_service:
            logger.info("No Teams webhook URL was given; skipping Teams notification")
            return False

        return self.teams_service.send_snapshot_notification(notification.to_teams_card())

    def _send_to_slack(self, notification: SnapshotNotification) -> bool:
        if not self.slack_webhook_url:
            logger.info("No Slack webhook URL was given; skipping Slack notification")
            return False

        try:
            response = WebhookClient(self.slack_webhook_url).send(
                text=notification.title, blocks=notification.to_slack_blocks()
            )
        # Deliberately broad. Slack is a secondary channel that is being retired,
        # so no failure to reach it may propagate into the rest of the run.
        except Exception as ex:
            logger.warning(f"Unable to send snapshot notification to Slack: {ex}")
            return False

        if response.status_code != 200 or response.body != "ok":
            logger.warning(f"Slack webhook returned HTTP {response.status_code}")
            return False

        logger.info("Sent snapshot notification to Slack")
        return True
