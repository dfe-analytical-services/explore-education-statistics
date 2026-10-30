"""
Sends the UI test run report to chat services.
"""

import os
from typing import Optional

from tests.libs.logger import get_logger
from tests.libs.teams import TeamsService
from tests.libs.ui_test_notification import UiTestNotification

logger = get_logger(__name__)


class UiTestNotifier:
    """
    Sends a notification to Teams when it has a webhook URL. Without one, notifications are
    skipped, so that the tests can be run locally without notifying anyone. Only a pipeline
    ever sets the webhook URL.
    """

    def __init__(self, teams_service: Optional[TeamsService] = None):
        self.teams_service = teams_service

    @classmethod
    def create(cls, teams_webhook_url: Optional[str] = None) -> "UiTestNotifier":
        return cls(teams_service=cls._create_teams_service(teams_webhook_url))

    @staticmethod
    def _create_teams_service(teams_webhook_url: Optional[str] = None) -> Optional[TeamsService]:
        webhook_url = teams_webhook_url or os.getenv("TEAMS_UI_TESTS_WEBHOOK_URL")

        return TeamsService(webhook_url) if webhook_url else None

    def send(self, notification: UiTestNotification) -> bool:
        """
        Notifying is best effort and never raises, so a chat service outage cannot turn a
        passing test run into a failing one.

        Returns whether Teams accepted the message, so that a run which reported to nobody
        can be told apart from one that reported, and be picked up by the pipeline.
        """
        # Logged when sending rather than when building, so that a notifier which is only
        # ever a stand-in does not report skipping a channel it was never asked to use.
        if not self.teams_service:
            logger.info("No Teams webhook URL was given; skipping Teams notifications")
            return False

        try:
            return self.teams_service.send_test_report(notification.to_teams_card())
        except Exception as ex:
            logger.warning(f"Unable to send the UI test report to Teams: {ex}")
            return False
