import datetime
import os
import shutil

from bs4 import BeautifulSoup
from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError
from tests.libs.logger import get_logger

logger = get_logger(__name__)

PATH = f"{os.getcwd()}{os.sep}test-results"


class SlackService:
    def __init__(self):
        self.slack_app_token = os.getenv("SLACK_APP_TOKEN")
        self.slack_channel = "C070FUXS3GC" # ui-test-reports

        if self.slack_app_token is None:
            raise AssertionError(f"SLACK_APP_TOKEN is not set")

        self.client = WebClient(token=self.slack_app_token)

    def _build_test_results_attachments(self, env: str, suites_ran: str, suites_failed: [], run_index: int):
        with open(f"{PATH}{os.sep}output.xml", "rb") as report:
            contents = report.read()

        soup = BeautifulSoup(contents, features="xml")

        tests = soup.find("total").find("stat")

        failed_tests = int(tests["fail"])
        passed_tests = int(tests["pass"])

        blocks = [
            {
                "type": "header",
                "text": {
                    "type": "plain_text",
                    "text": f"{':warning:' if suites_failed else ':white_check_mark:'} All results",
                },
            },
            {
                "type": "section",
                "fields": [
                    {"type": "mrkdwn", "text": f"*Environment*\n{env}"},
                    {"type": "mrkdwn", "text": f"*Suite*\n{suites_ran.replace('tests/', '')}"},
                    {"type": "mrkdwn", "text": f"*Total runs*\n{run_index + 1}"},
                    {"type": "mrkdwn", "text": f"*Total test cases*\n{passed_tests + failed_tests}"},
                    {"type": "mrkdwn", "text": f"*Passed test cases*\n{passed_tests}"},
                    {"type": "mrkdwn", "text": f"*Failed test cases*\n{failed_tests}"},
                ],
            },
        ]

        if suites_failed:
            failed_test_suites_list_items = []

            for suite in suites_failed:
                failed_test_suites_list_items.append(
                    {"type": "rich_text_section", "elements": [{"type": "text", "text": suite}]}
                )

            blocks += [
                {"type": "divider"},
                {"type": "section", "text": {"type": "mrkdwn", "text": f"*Failed test suites* ({len(suites_failed)})"}},
                {
                    "type": "rich_text",
                    "elements": [
                        {"type": "rich_text_list", "style": "bullet", "elements": failed_test_suites_list_items}
                    ],
                },
            ]

        return blocks

    def _build_exception_details_attachments(self, env: str, suites_ran: str, run_index: int, ex: Exception):
        ex_stripped = repr(ex).replace("\n", "")
        suites_ran_conditional = "N/A" if not suites_ran else suites_ran.replace("tests/", "")

        return [
            {"type": "header", "text": {"type": "plain_text", "text": ":x: UI test pipeline failure"}},
            {
                "type": "section",
                "fields": [
                    {"type": "mrkdwn", "text": f"*Environment*\n{env}"},
                    {"type": "mrkdwn", "text": f"*Suite*\n{suites_ran_conditional}"},
                    {"type": "mrkdwn", "text": f"*Run number*\n{run_index + 1}"},
                ],
            },
            {"type": "divider"},
            {"type": "section", "text": {"type": "mrkdwn", "text": f"*Error details*\n{ex_stripped}"}},
        ]

    def send_test_report(self, env: str, suites_ran: str, suites_failed: [], run_index: int):
        attachments = self._build_test_results_attachments(env, suites_ran, suites_failed, run_index)

        response = self.client.chat_postMessage(channel=self.slack_channel, text="All results", blocks=attachments)

        if suites_failed:
            date = datetime.datetime.utcnow().strftime("%Y%m%d-%H%M%S")

            report_name = f"UI-test-report-{suites_ran.replace('tests/', '')}-{env}-{date}.zip"

            shutil.make_archive(report_name.replace(".zip", ""), "zip", PATH)
            try:
                self.client.files_upload_v2(
                    channel=self.slack_channel, file=report_name, title=report_name, thread_ts=response.data["ts"]
                )
            except SlackApiError as e:
                logger.error(f"Error uploading test report: {e}")
            os.remove(report_name)
            logger.info("Sent UI test report to #build")

        assert response.status_code == 200, logger.warn(f"Response wasn't 200, it was {response}")

        logger.info("Sent UI test statistics to #build")

    def send_exception_details(self, env: str, suites_ran: str, run_index: int, ex: Exception):
        attachments = self._build_exception_details_attachments(env, suites_ran, run_index, ex)

        response = self.client.chat_postMessage(
            text=":x: UI test pipeline failure", channel=self.slack_channel, blocks=attachments
        )

        assert response.status_code == 200, logger.warn(f"Response wasn't 200, it was {response}")

        logger.info("Sent UI exception details to #build")
