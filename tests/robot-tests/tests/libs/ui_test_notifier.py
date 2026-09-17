"""
Sends the UI test run report to chat services.
"""

import os
from typing import Optional

from tests.libs.logger import get_logger
from tests.libs.slack import SlackService
from tests.libs.teams import TeamsService
from tests.libs.ui_test_notification import UiTestNotification

logger = get_logger(__name__)


# TODO: Remove slack messages sent when EES DfE Slack Workspace is retired EES-7608
class UiTestNotifier:
    """
    Sends a notification to every channel it has credentials for. Channels without
    credentials are skipped, so that the tests can be run locally without notifying
    anyone.

    Teams is enabled by the presence of a webhook URL, which only a pipeline ever sets.
    Slack is enabled by an explicit argument instead, because its token is read from the
    .env file that every developer has, and a local run must not post to a team channel.
    """

    def __init__(self, slack_service: Optional[SlackService] = None, teams_service: Optional[TeamsService] = None):
        self.slack_service = slack_service
        self.teams_service = teams_service

    @classmethod
    def create(cls, enable_slack: bool, teams_webhook_url: Optional[str] = None) -> "UiTestNotifier":
        return cls(
            slack_service=cls._create_slack_service(enable_slack),
            teams_service=cls._create_teams_service(teams_webhook_url),
        )

    @staticmethod
    def _create_slack_service(enable_slack: bool) -> Optional[SlackService]:
        if not enable_slack:
            return None

        try:
            return SlackService()
        # SlackService raises when SLACK_APP_TOKEN is unset. A missing token for a channel
        # that is being retired must never fail an otherwise passing test run.
        except Exception as ex:
            logger.warning(f"Unable to create the Slack service; skipping Slack notifications: {ex}")
            return None

    @staticmethod
    def _create_teams_service(teams_webhook_url: Optional[str] = None) -> Optional[TeamsService]:
        webhook_url = teams_webhook_url or os.getenv("TEAMS_UI_TESTS_WEBHOOK_URL")

        return TeamsService(webhook_url) if webhook_url else None

    def send(self, notification: UiTestNotification) -> bool:
        """
        Notifying is best effort. Neither channel raises, so a chat service outage cannot
        turn a passing test run into a failing one. Teams is the primary channel, so it
        goes first.

        Returns whether any channel accepted the message, so that a run which reported to
        nobody can be told apart from one that reported, and be picked up by the pipeline.
        """
        teams_sent = self._send_to_teams(notification)
        slack_sent = self._send_to_slack(notification)
        return teams_sent or slack_sent

    def _send_to_teams(self, notification: UiTestNotification) -> bool:
        # Logged when sending rather than when building, so that a notifier which is only
        # ever a stand-in does not report skipping channels it was never asked to use.
        if not self.teams_service:
            logger.info("No Teams webhook URL was given; skipping Teams notifications")
            return False

        try:
            return self.teams_service.send_test_report(notification.to_teams_card())
        # Deliberately broad, and matching Slack below. Teams goes first, so anything it
        # raises would otherwise cost us the Slack message as well.
        except Exception as ex:
            logger.warning(f"Unable to send the UI test report to Teams: {ex}")
            return False

    def _send_to_slack(self, notification: UiTestNotification) -> bool:
        if not self.slack_service:
            logger.info("Slack notifications were not enabled; skipping Slack notifications")
            return False

        try:
            return self.slack_service.send_notification(notification)
        # Deliberately broad. Slack is a secondary channel that is being retired, so no
        # failure to reach it may propagate into the rest of the run.
        except Exception as ex:
            logger.warning(f"Unable to send the UI test report to Slack: {ex}")
            return False
