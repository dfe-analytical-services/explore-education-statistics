import json
import os
import shutil

import requests
from bs4 import BeautifulSoup
from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError

PATH = f"{os.getcwd()}{os.sep}test-results"


def _generate_slack_attachments(env: str, suite: str):
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


def _tests_failed():
    with open(f"{PATH}{os.sep}output.xml", "rb") as report:
        contents = report.read()

        soup = BeautifulSoup(contents, features="xml")
        test = soup.find("total").find("stat")

        failed_tests = int(test["fail"])

        if failed_tests > 0:
            return True


def send_slack_report(env: str, suite: str):
    attachments = _generate_slack_attachments(env, suite)

    webhook_url = os.getenv("SLACK_TEST_REPORT_WEBHOOK_URL")
    slack_bot_token = os.getenv("SLACK_BOT_TOKEN")

    assert webhook_url, print("SLACK_TEST_REPORT_WEBHOOK_URL env variable needs to be set")
    assert slack_bot_token, print("SLACK_BOT_TOKEN env variable needs to be set")

    if "hooks.slack.com" not in webhook_url:
        raise Exception(
            f"Invalid slack webhook URL provided: {webhook_url}. Valid URL: https://hooks.slack.com/services/... (https://api.slack.com/messaging/webhooks)"
        )

    if "xoxb-" not in slack_bot_token:
        raise Exception(
            f"Invalid slack bot token provided: {slack_bot_token}. Valid token: xoxb-... (https://api.slack.com/authentication/token-types#bot)"
        )

    response = requests.post(
        url=webhook_url, data=json.dumps({"attachments": attachments}), headers={"Content-Type": "application/json"}
    )
    assert response.status_code == 200, print(f"Response wasn't 200, it was {response}")

    print("Sent UI test statistics to #build")

    if _tests_failed():
        client = WebClient(token=slack_bot_token)

        shutil.make_archive("UI-test-report", "zip", PATH)
        try:
            client.files_upload(
                channels="#build",
                file="UI-test-report.zip",
                title="test-report.zip",
            )
        except SlackApiError as e:
            print(f"Error uploading test report: {e}")
        os.remove("UI-test-report.zip")
        print("Sent UI test report to #build")
