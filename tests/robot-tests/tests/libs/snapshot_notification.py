"""
Notifications sent by the public frontend snapshot validation script.

Each notification is defined once, independently of any chat service, and is
rendered into a Teams Adaptive Card.
"""

from dataclasses import dataclass
from typing import Optional

PULL_REQUESTS_URL = "https://github.com/dfe-analytical-services/explore-education-statistics/pulls/dfe-sdt"

# Adaptive Card colours.
WARNING = "warning"
ATTENTION = "attention"


@dataclass(frozen=True)
class SnapshotNotification:
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


def snapshots_do_not_match() -> SnapshotNotification:
    return SnapshotNotification(
        title="⚠️ Snapshots do not match",
        body="See {link} for more details",
        colour=WARNING,
        link_url=PULL_REQUESTS_URL,
        link_text="pull request",
    )


def snapshot_process_failed(message: str) -> SnapshotNotification:
    return SnapshotNotification(
        title="❌ Snapshot process failed",
        body=str(message),
        colour=ATTENTION,
    )
