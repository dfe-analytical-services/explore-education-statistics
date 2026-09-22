import os
import shutil
from contextlib import suppress
from pathlib import Path
from typing import Optional

from slack_sdk import WebClient
from tests.libs.logger import get_logger
from tests.libs.ui_test_notification import UiTestNotification

logger = get_logger(__name__)

# Resolved from this file rather than the working directory, as run_tests.py imports this
# module before it changes directory into tests/robot-tests.
DEFAULT_RESULTS_DIRECTORY = Path(__file__).resolve().parents[2] / "test-results"


class SlackService:
    def __init__(self, results_directory: Optional[Path] = None):
        self.slack_app_token = os.getenv("SLACK_APP_TOKEN")
        self.slack_channel = "C070FUXS3GC"  # ui-test-reports
        self.results_directory = results_directory or DEFAULT_RESULTS_DIRECTORY

        if self.slack_app_token is None:
            raise AssertionError("SLACK_APP_TOKEN is not set")

        self.client = WebClient(token=self.slack_app_token)

    def send_notification(self, notification: UiTestNotification) -> bool:
        response = self.client.chat_postMessage(
            channel=self.slack_channel,
            text=notification.title,
            blocks=notification.to_slack_blocks(),
        )

        if response.status_code != 200:
            logger.warning(f"Response wasn't 200, it was {response}")
            return False

        if notification.results_archive_name:
            self._upload_results_archive(notification.results_archive_name, response.data["ts"])

        logger.info("Sent UI test report to Slack")
        return True

    def _upload_results_archive(self, archive_name: str, thread_ts: str) -> None:
        """
        The archive is supplementary to the report that has already been posted, so no
        failure to build or upload it may propagate. Doing so would report the whole run
        as unsent, and leave the pipeline treating a suite that did report as silent.

        The catch is deliberately broad: archiving raises OSError on a large or
        part-written results tree, and the upload raises SlackRequestError and raw urllib
        errors as well as SlackApiError.
        """
        try:
            shutil.make_archive(archive_name.replace(".zip", ""), "zip", self.results_directory)

            self.client.files_upload_v2(
                channel=self.slack_channel, file=archive_name, title=archive_name, thread_ts=thread_ts
            )
        except Exception as ex:
            logger.error(f"Error uploading test report: {ex}")
        finally:
            # The archive is absent when it was building it that failed.
            with suppress(OSError):
                os.remove(archive_name)
