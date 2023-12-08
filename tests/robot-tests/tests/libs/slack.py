import datetime
import json
import os
import shutil

import requests
from bs4 import BeautifulSoup
from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError
from tests.libs.logger import get_logger

logger = get_logger(__name__)

PATH = f"{os.getcwd()}{os.sep}test-results"


class SlackService:
    def __init__(self):
        self.slack_bot_token = os.getenv("SLACK_BOT_TOKEN")
        self.report_webhook_url = os.getenv("SLACK_TEST_REPORT_WEBHOOK_URL")

        if self.slack_bot_token is None:
            raise AssertionError(f"SLACK_BOT_TOKEN is not set")

        if self.report_webhook_url is None:
            raise AssertionError(f"SLACK_TEST_REPORT_WEBHOOK_URL is not set")

        self.client = WebClient(token=self.slack_bot_token)

    def _build_test_results_attachments(self, env: str, suites_ran: str, suites_failed: [], run_index: int):
        with open(f"{PATH}{os.sep}output.xml", "rb") as report:
            contents = report.read()

        soup = BeautifulSoup(contents, features="xml")

        tests = soup.find("total").find("stat")

        failed_tests = int(tests["fail"])
        passed_tests = int(tests["pass"])

        failed_test_suites_field = ({},)

        if suites_failed:
            failed_test_suites_field = {"title": "Failed test suites", "value": "\n".join(suites_failed)}
        return [
            {
                "pretext": "All results",
                "color": "danger" if suites_failed else "good",
                "fields": [
                    {"title": "Environment", "value": env},
                    {"title": "Suite", "value": suites_ran.replace("tests/", "")},
                    {"title": "Total runs", "value": run_index + 1},
                    {"title": "Total test cases", "value": passed_tests + failed_tests},
                    {"title": "Passed test cases", "value": passed_tests},
                    {"title": "Failed test cases", "value": failed_tests},
                    {"title": "Failed test suites", "value": len(suites_failed)},
                    failed_test_suites_field,
                ],
            }
        ]

    def _build_exception_details_attachments(self, env: str, suites_ran: str, run_index: int, ex: Exception):
        return [
            {
                "pretext": "UI test pipeline failure",
                "color": "danger",
                "fields": [
                    {"title": "Environment", "value": env},
                    {"title": "Suite", "value": suites_ran.replace("tests/", "")},
                    {"title": "Run number", "value": run_index + 1},
                    {"title": "Error encountered", "value": f"{ex}"},
                ],
            }
        ]

    def send_test_report(self, env: str, suites_ran: str, suites_failed: [], run_index: int):
        attachments = self._build_test_results_attachments(env, suites_ran, suites_failed, run_index)

        webhook_url = self.report_webhook_url
        slack_bot_token = self.slack_bot_token

        response = requests.post(
            url=webhook_url, data=json.dumps({"attachments": attachments}), headers={"Content-Type": "application/json"}
        )
        assert response.status_code == 200, logger.warn(f"Response wasn't 200, it was {response}")

        logger.info("Sent UI test statistics to #build")

        if suites_failed:
            client = WebClient(token=slack_bot_token)

            date = datetime.datetime.utcnow().strftime("%Y%m%d-%H%M%S")

            report_name = f"UI-test-report-{suites_ran.replace('tests/', '')}-{env}-{date}.zip"

            shutil.make_archive(report_name.replace(".zip", ""), "zip", PATH)
            try:
                client.files_upload(channels="#build", file=report_name, title=report_name)
            except SlackApiError as e:
                logger.error(f"Error uploading test report: {e}")
            os.remove(report_name)
            logger.info("Sent UI test report to #build")

    def send_exception_details(self, env: str, suites_ran: str, run_index: int, ex: Exception):
        attachments = self._build_exception_details_attachments(env, suites_ran, run_index, ex)

        webhook_url = self.report_webhook_url

        response = requests.post(
            url=webhook_url, data=json.dumps({"attachments": attachments}), headers={"Content-Type": "application/json"}
        )
        assert response.status_code == 200, logger.warn(f"Response wasn't 200, it was {response}")

        logger.info("Sent UI exception details to #build")
