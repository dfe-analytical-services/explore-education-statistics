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
        self.client = WebClient(token=self.slack_bot_token)

        for env_var in [self.slack_bot_token, self.report_webhook_url]:
            if env_var is None:
                raise AssertionError(f"{env_var} is not set")

    def _build_attachments(self, env: str, suite: str):
        with open(f"{PATH}{os.sep}output.xml", "rb") as report:
            contents = report.read()

        soup = BeautifulSoup(contents, features="xml")

        test = soup.find("total").find("stat")

        failed_tests = int(test["fail"])
        passed_tests = int(test["pass"])

        failed_tests_field = ({},)

        if failed_tests > 0:
            failed_tests_field = {"title": "Failed tests", "value": failed_tests}
        return [
            {
                "pretext": "All results",
                "color": "danger" if failed_tests else "good",
                "mrkdwn_in": ["pretext"],
                "fields": [
                    {"title": "Environment", "value": env},
                    {"title": "Suite", "value": suite.replace("tests/", "")},
                    {"title": "Total test cases", "value": passed_tests + failed_tests},
                    failed_tests_field,
                ],
            }
        ]

    def _tests_failed(self):
        with open(f"{PATH}{os.sep}output.xml", "rb") as report:
            contents = report.read()

            soup = BeautifulSoup(contents, features="xml")
            test = soup.find("total").find("stat")

            failed_tests = int(test["fail"])

            if failed_tests > 0:
                return True

    def send_test_report(self, env: str, suite: str):
        attachments = self._build_attachments(env, suite)

        webhook_url = self.report_webhook_url
        slack_bot_token = self.slack_bot_token

        response = requests.post(
            url=webhook_url, data=json.dumps({"attachments": attachments}), headers={"Content-Type": "application/json"}
        )
        assert response.status_code == 200, logger.warn(f"Response wasn't 200, it was {response}")

        logger.info("Sent UI test statistics to #build")

        if self._tests_failed():
            client = WebClient(token=slack_bot_token)

            shutil.make_archive("UI-test-report", "zip", PATH)
            try:
                client.files_upload(
                    channels="#build",
                    file="UI-test-report.zip",
                    title="test-report.zip",
                )
            except SlackApiError as e:
                logger.error(f"Error uploading test report: {e}")
            os.remove("UI-test-report.zip")
            logger.info("Sent UI test report to #build")
