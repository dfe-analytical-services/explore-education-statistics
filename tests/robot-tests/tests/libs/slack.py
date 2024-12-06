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
        self.slack_channel = "C070FUXS3GC"  # ui-test-reports

        if self.slack_app_token is None:
            raise AssertionError(f"SLACK_APP_TOKEN is not set")

        self.client = WebClient(token=self.slack_app_token)

    def _build_test_results_attachments(self, env: str, suites_ran: str, suites_failed: [], number_of_test_runs: int):
        try:
            # Gather high-level passing test count from the merged report from all run attempts.
            with open(f"{PATH}{os.sep}output.xml", "rb") as report:
                contents = report.read()
                soup = BeautifulSoup(contents, features="xml")
                tests = soup.find("total").find("stat")
                passed_tests = int(tests["pass"])
                skipped_tests = int(tests["skip"])
                failed_tests_in_merged_report = int(tests["fail"])

            # Gather final failing test counts from the final run attempt's report. We do this so as to most
            # accurately report what was failing right at the end of the process. It's not safe to do this from
            # the merged report because failures in one run attempt that then succeed in another attempt will be
            # marked as PASSED during the merge, and subsequent test following a failure are only reported as
            # SKIPPED, thus potentially suggesting that no tests were actually left as failing at the end.
            with open(f"{PATH}{os.sep}run-{number_of_test_runs}{os.sep}output.xml", "rb") as report:
                contents = report.read()
                soup = BeautifulSoup(contents, features="xml")
                tests = soup.find("total").find("stat")
                failed_tests_in_final_run = int(tests["fail"])

        except AttributeError as e:
            raise Exception("Error parsing the XML report") from e

        total_tests_count = passed_tests + failed_tests_in_merged_report + skipped_tests

        # Whilst genuine bugs that fail a step every time will always show as FAILED in the merged test report AND
        # the final run report, this is not always true of flaky / intermittent test failures, which can be identified
        # in part by an unequal number of failures in the merged test report versus that of the final run attempt. It can
        # also be identified by the fact that the tests finally all passed but the number of attempts was greater than 1.
        flaky_test_message = (
            "Definitely"
            if failed_tests_in_final_run == 0 and number_of_test_runs > 1
            else "Definitely not"
            if failed_tests_in_final_run == 0 and number_of_test_runs == 1
            else "Likely"
            if failed_tests_in_merged_report != failed_tests_in_final_run
            else "Unlikely"
        )

        blocks = [
            {
                "type": "header",
                "text": {
                    "type": "plain_text",
                    "text": f"{':warning:' if failed_tests_in_final_run > 0 else ':white_check_mark:'} All results",
                },
            },
            {
                "type": "section",
                "fields": [
                    {"type": "mrkdwn", "text": f"*Environment*\n{env}"},
                    {"type": "mrkdwn", "text": f"*Suite*\n{suites_ran.replace('tests/', '')}"},
                    {"type": "mrkdwn", "text": f"*Total run attempts*\n{number_of_test_runs}"},
                    {"type": "mrkdwn", "text": f"*Total test cases*\n{total_tests_count}"},
                    {"type": "mrkdwn", "text": f"*Passed test cases*\n{passed_tests}"},
                    {"type": "mrkdwn", "text": f"*Failed test cases*\n{failed_tests_in_final_run}"},
                    {"type": "mrkdwn", "text": f"*Skipped test cases*\n{skipped_tests}"},
                    {"type": "mrkdwn", "text": f"*Flaky tests?*\n{flaky_test_message}"},
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

    def _build_exception_details_attachments(self, env: str, suites_ran: str, number_of_test_runs: int, ex: Exception):
        ex_stripped = repr(ex).replace("\n", "")
        suites_ran_conditional = "N/A" if not suites_ran else suites_ran.replace("tests/", "")

        return [
            {"type": "header", "text": {"type": "plain_text", "text": ":x: UI test pipeline failure"}},
            {
                "type": "section",
                "fields": [
                    {"type": "mrkdwn", "text": f"*Environment*\n{env}"},
                    {"type": "mrkdwn", "text": f"*Suite*\n{suites_ran_conditional}"},
                    {"type": "mrkdwn", "text": f"*Run number*\n{number_of_test_runs}"},
                ],
            },
            {"type": "divider"},
            {"type": "section", "text": {"type": "mrkdwn", "text": f"*Error details*\n{ex_stripped}"}},
        ]

    def send_test_report(self, env: str, suites_ran: str, suites_failed: [], number_of_test_runs: int):
        attachments = self._build_test_results_attachments(env, suites_ran, suites_failed, number_of_test_runs)

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

        assert response.status_code == 200, logger.warning(f"Response wasn't 200, it was {response}")

        logger.info("Sent UI test statistics to #build")

    def send_exception_details(self, env: str, suites_ran: str, number_of_test_runs: int, ex: Exception):
        attachments = self._build_exception_details_attachments(env, suites_ran, number_of_test_runs, ex)

        response = self.client.chat_postMessage(
            text=":x: UI test pipeline failure", channel=self.slack_channel, blocks=attachments
        )

        assert response.status_code == 200, logger.warning(f"Response wasn't 200, it was {response}")

        logger.info("Sent UI exception details to #build")
