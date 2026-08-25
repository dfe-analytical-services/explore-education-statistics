"""
Notifications sent by the public frontend snapshot validation script.

Each notification is defined once, independently of any chat service, and is
rendered into a Slack Block Kit message and a Teams Adaptive Card, so that both
channels receive the same message.
"""

from dataclasses import dataclass
from typing import Optional

from slack_sdk.webhook import WebhookClient
from tests.libs.logger import get_logger
from tests.libs.teams import TeamsService

logger = get_logger(__name__)

PULL_REQUESTS_URL = "https://github.com/dfe-analytical-services/explore-education-statistics/pulls/dfe-sdt"

# Adaptive Card colours. Slack headings cannot be coloured, so these apply to Teams only.
WARNING = "warning"
ATTENTION = "attention"


@dataclass(frozen=True)
class Notification:
    title: str
    body: str
    colour: str = "default"
    link_url: Optional[str] = None
    link_text: Optional[str] = None

    def _body_with_link(self, link: str) -> str:
        """
        Body text is only ever formatted when we own it, i.e. when it contains a link.
        Bodies without a link may be arbitrary text such as an exception message.
        """
        return self.body.format(link=link) if self.link_url else self.body

    def to_slack_blocks(self) -> list[dict]:
        body = self._body_with_link(f"<{self.link_url}|{self.link_text}>")

        return [
            {"type": "header", "text": {"type": "plain_text", "text": self.title}},
            {
                "type": "section",
                # Links are only recognised in mrkdwn, but leave link-free bodies as
                # plain text so that they cannot be misinterpreted as mrkdwn.
                "text": {"type": "mrkdwn" if self.link_url else "plain_text", "text": body},
            },
        ]

    def to_teams_card(self) -> dict:
        body = self._body_with_link(f"[{self.link_text}]({self.link_url})")

        return {
            "type": "message",
            "attachments": [
                {
                    "contentType": "application/vnd.microsoft.card.adaptive",
                    "content": {
                        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
                        "type": "AdaptiveCard",
                        "version": "1.4",
                        "body": [
                            {
                                "type": "TextBlock",
                                "size": "Large",
                                "weight": "Bolder",
                                "text": self.title,
                                "style": "heading",
                                "color": self.colour,
                                "wrap": True,
                            },
                            {"type": "TextBlock", "text": body, "wrap": True},
                        ],
                    },
                }
            ],
        }


def snapshots_do_not_match() -> Notification:
    return Notification(
        title="⚠️ Snapshots do not match",
        body="See {link} for more details",
        colour=WARNING,
        link_url=PULL_REQUESTS_URL,
        link_text="pull request",
    )


def snapshot_process_failed(message: str) -> Notification:
    return Notification(
        title="❌ Snapshot process failed",
        body=str(message),
        colour=ATTENTION,
    )


class SnapshotNotifier:
    """
    Sends a notification to every channel it has been given a webhook URL for.
    Channels without a webhook URL are skipped, so that the script can be run
    locally without notifying anyone.
    """

    def __init__(self, slack_webhook_url: Optional[str] = None, teams_webhook_url: Optional[str] = None):
        self.slack_webhook_url = slack_webhook_url
        self.teams_service = TeamsService(teams_webhook_url) if teams_webhook_url else None

    def send(self, notification: Notification) -> None:
        # Teams is notified first as it never raises, so a Slack outage cannot
        # stop the message reaching Teams.
        self._send_to_teams(notification)
        self._send_to_slack(notification)

    def _send_to_teams(self, notification: Notification) -> None:
        if not self.teams_service:
            logger.info("No Teams webhook URL was given; skipping Teams notification")
            return

        self.teams_service.send_snapshot_notification(notification.to_teams_card())

    def _send_to_slack(self, notification: Notification) -> None:
        if not self.slack_webhook_url:
            logger.info("No Slack webhook URL was given; skipping Slack notification")
            return

        response = WebhookClient(self.slack_webhook_url).send(
            text=notification.title, blocks=notification.to_slack_blocks()
        )

        assert (
            response.status_code == 200 and response.body == "ok"
        ), f"Slack notification failed with status code: {response.status_code}"
