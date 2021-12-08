import os
from slack_sdk import WebClient
from slack_sdk.errors import SlackApiError
import shutil
import json
import requests
from bs4 import BeautifulSoup

PATH = f'{os.getcwd()}{os.sep}test-results'


def _generate_slack_attachments(env: str):
    with open(f'{PATH}{os.sep}output.xml', 'rb') as report:
        contents = report.read()

    soup = BeautifulSoup(contents, 'lxml')
    test = soup.find('total').find('stat')

    failed_tests = int(test['fail'])
    passed_tests = int(test['pass'])

    return [
        {
            "pretext": "All results",
            "color": "danger" if failed_tests else "good",
            "mrkdwn_in": [
                "pretext"
            ],
            "fields": [
                {
                    "title": "Environment",
                    "value": env
                },
                {
                    "title": "Total test cases",
                    "value": passed_tests + failed_tests
                },
                {
                    "title": "Passed tests",
                    "value": passed_tests
                },
                {
                    "title": "Failed tests",
                    "value": failed_tests if failed_tests else "0"
                },
                {
                    "title": "Results",
                    "value": "Failed" if failed_tests else "Passed"
                },

            ]
        }
    ]


def send_slack_report(env: str):
    attachments = _generate_slack_attachments(env)
    data = {"attachments": attachments}

    webhook_url = os.getenv('SLACK_TEST_REPORT_WEBHOOK_URL')
    slack_bot_token = os.getenv('SLACK_BOT_TOKEN')

    assert webhook_url, print("SLACK_TEST_REPORT_WEBHOOK_URL env variable needs to bet set")
    assert slack_bot_token, print("SLACK_BOT_TOKEN env variable needs to bet set")

    response = requests.post(
        url=webhook_url,
        data=json.dumps(data), headers={'Content-Type': 'application/json'}
    )
    assert response.status_code == 200, print(f"Response wasn't 200, it was {response}")

    print("Sent UI test statistics to #build")

    client = WebClient(token=slack_bot_token)
    print('Sending UI test report to #build')

    shutil.make_archive('UI-test-report', 'zip', PATH)
    try:
        client.files_upload(
            channels='#build',
            file='UI-test-report.zip',
            title='test-report.zip',
        )
    except SlackApiError as e:
        print(f'Error uploading test report: {e}')
    os.remove('UI-test-report.zip')
