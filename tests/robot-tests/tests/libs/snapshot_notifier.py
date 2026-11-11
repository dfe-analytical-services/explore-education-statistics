"""
Sends the public frontend snapshot validation notifications to chat services.
"""

from typing import Optional

from tests.libs.logger import get_logger
from tests.libs.snapshot_notification import SnapshotNotification
from tests.libs.teams import TeamsService

logger = get_logger(__name__)


class SnapshotNotifier:
    """
    Sends a notification to Teams when it has been given a webhook URL. Without one,
    notifications are skipped, so that the script can be run locally without notifying
    anyone.
    """

    def __init__(self, teams_webhook_url: Optional[str] = None):
        self.teams_service = TeamsService(teams_webhook_url) if teams_webhook_url else None

    def send(self, notification: SnapshotNotification) -> None:
        if not self.teams_service:
            logger.info("No Teams webhook URL was given; skipping Teams notification")
            return

        self.teams_service.send_snapshot_notification(notification.to_teams_card())
