import json
import os
from typing import Optional
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen

from tests.libs.logger import get_logger

logger = get_logger(__name__)


class TeamsService:
    def __init__(self, webhook_url: Optional[str] = None):
        self.webhook_url = webhook_url or os.getenv("TEAMS_UI_TESTS_WEBHOOK_URL")

    def _post_card(self, card: dict, description: str) -> bool:
        if not self.webhook_url:
            logger.warning(f"Teams webhook URL is not set; skipping {description}")
            return False

        try:
            request = Request(
                self.webhook_url,
                data=json.dumps(card).encode("utf-8"),
                headers={"Content-Type": "application/json"},
                method="POST",
            )
            with urlopen(request, timeout=30) as response:
                if not 200 <= response.status < 300:
                    logger.warning(f"Teams webhook returned HTTP {response.status}")
                    return False
        except (HTTPError, URLError, TimeoutError, ValueError) as ex:
            logger.warning(f"Unable to send {description} to Teams: {ex}")
            return False

        logger.info(f"Sent {description} to Teams")
        return True

    def send_test_report(self, card: dict) -> bool:
        return self._post_card(card, "UI test report")

    def send_snapshot_notification(self, card: dict) -> bool:
        return self._post_card(card, "snapshot notification")
