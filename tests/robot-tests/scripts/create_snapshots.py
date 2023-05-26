import argparse
import base64
import json
import math
import os
from typing import Optional

import requests
from bs4 import BeautifulSoup
from selenium import webdriver
from selenium.common import NoSuchElementException
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from slack_sdk.webhook import WebhookClient

"""
Script to check find statistics, table tool, methodologies and data catalogue snapshots.
By default, the script will run against production

Usage: pipenv run python scripts/create_snapshots.py
Optional flags:
  * --basic-auth-user "username" --basic-auth-pass "password"
  * --public-url "public URL"
  * --validate
  * --ci
  * --slack-webhook-url "slack webhook URL"
"""


class SnapshotService:
    requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)

    def __init__(self, public_url: str, slack_webhook_url: str):
        self.session = requests.Session()
        self.public_url = public_url
        self.slack_webhook_url = slack_webhook_url
        self.timeout = 10
        self.page_size = 10
        self.snapshots = [
            "data_catalogue_snapshot.json",
            "find_statistics_snapshot.json",
            "methodologies_snapshot.json",
            "table_tool_snapshot.json",
        ]

    def _gets_parsed_html_from_page(self, url: str) -> BeautifulSoup:
        response = self.session.get(url, stream=True)
        assert response.status_code == 200, f"Failed to get page: {url}. Status code: {response.status_code}"
        return BeautifulSoup(response.text, "html.parser")

    def _get_create_snapshot_func(self, name: str):
        switcher = {
            "data_catalogue_snapshot.json": self._create_data_catalogue_snapshot,
            "find_statistics_snapshot.json": self._create_find_statistics_snapshot,
            "methodologies_snapshot.json": self._create_methodologies_snapshot,
            "table_tool_snapshot.json": self._create_table_tool_snapshot,
        }

        result = switcher.get(name)

        if result is None:
            raise Exception(f"Invalid snapshot name: {name}")

        return result

    def _send_slack_notification(self, text: Optional[str] = None, blocks=None) -> None:
        webhook = WebhookClient(self.slack_webhook_url)
        response = webhook.send(text=text, blocks=blocks)
        assert (
            response.status_code == 200 and response.body == "ok"
        ), f"Slack notification failed with status code: {response.status_code}"

    def _validate_snapshot(self, name: str) -> bool:
        print(f"Validating snapshot: {name}")
        create_snapshot_func = self._get_create_snapshot_func(name)
        path_to_file = os.path.join(os.getcwd(), "tests/snapshots", name)
        with open(path_to_file, "r") as file:
            snapshot = file.read()
        return snapshot == create_snapshot_func()

    def _write_snapshot_to_file(self, name: str) -> None:
        create_snapshot_func = self._get_create_snapshot_func(name)
        snapshot = create_snapshot_func()

        path = "tests/snapshots"
        if not os.path.exists(path):
            os.makedirs(path)

        path_to_file = os.path.join(os.getcwd(), path, name)

        with open(path_to_file, "w") as file:
            file.write(snapshot)
            file.close()

    def _create_find_statistics_snapshot(self) -> str:
        driver.get(f"{self.public_url}/find-statistics")

        total_results = driver.find_element(By.XPATH, "//*[@data-testid='total-results']").text.split(" ")[0]

        total_pages = math.ceil(int(total_results) / self.page_size)

        # A-Z sort in order to get all the publications
        driver.find_element(By.CSS_SELECTOR, "input[value='title']").click()
        driver.find_element(By.CSS_SELECTOR, "body").click()
        publications = []

        for page in range(total_pages):
            print(f"Adding page {page + 1} of {total_pages} to find statistics snapshot")

            WebDriverWait(driver, self.timeout).until(
                lambda driver: driver.find_element(By.CSS_SELECTOR, "form[id='sortControlsForm']"),
                message=f"Failed to find sort controls form on page {page + 1}",
            )

            WebDriverWait(driver, self.timeout).until(
                lambda driver: driver.find_element(By.XPATH, "//*[@data-testid='publicationsList']"),
                message=f"Failed to find publications list on page {page + 1}",
            )

            publications_list_items = driver.find_elements(By.XPATH, "//*[@data-testid='publicationsList']/li")

            for publication in publications_list_items:
                publication_title = publication.find_element(By.CSS_SELECTOR, "h3").text

                try:
                    publication_summary = publication.find_element(By.CSS_SELECTOR, "p").text
                except NoSuchElementException:
                    publication_summary = ""

                release_type = publication.find_element(By.XPATH, "./dl//dd[@data-testid='release-type']").text

                theme = publication.find_element(By.XPATH, "./dl//dd[@data-testid='theme']").text

                published = publication.find_element(By.XPATH, "./dl//dd[@data-testid='published']").text

                publications.append(
                    {
                        "publication_title": publication_title,
                        "publication_summary": publication_summary,
                        "release_type": release_type,
                        "published": published,
                        "theme": theme,
                    }
                )

                WebDriverWait(driver, self.timeout).until(
                    lambda driver: driver.find_element(By.XPATH, "//*[@data-testid='publicationsList']"),
                    message=f"Failed to find publications list on page {page + 1}",
                )

            try:
                driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
                driver.find_element(By.XPATH, "//*[@data-testid='pagination-next']").click()
            except NoSuchElementException:
                break
        return json.dumps(publications, sort_keys=True, indent=2)

    def _create_table_tool_snapshot(self) -> str:
        driver.get(f"{self.public_url}/data-tables")

        theme_labels = driver.find_elements(By.CSS_SELECTOR, 'label[for^="publicationForm-themes-"]')

        themes = []

        for theme_label in theme_labels:
            theme_label.click()
            theme = {"theme_heading": theme_label.text, "publications": []}

            publication_labels = driver.find_elements(By.CSS_SELECTOR, 'label[for^="publicationForm-publications-"]')

            for publication_label in publication_labels:
                theme["publications"].append(publication_label.text)

            themes.append(theme)

        return json.dumps(themes, sort_keys=True, indent=2)

    def _create_data_catalogue_snapshot(self) -> str:
        driver.get(f"{self.public_url}/data-catalogue")

        theme_labels = driver.find_elements(By.CSS_SELECTOR, 'label[for^="publicationForm-themes-"]')

        themes = []

        for theme_label in theme_labels:
            theme_label.click()
            theme = {"theme_heading": theme_label.text, "publications": []}

            publication_labels = driver.find_elements(By.CSS_SELECTOR, 'label[for^="publicationForm-publications-"]')

            for publication_label in publication_labels:
                theme["publications"].append(publication_label.text)

            themes.append(theme)

        return json.dumps(themes, sort_keys=True, indent=2)

    def _create_methodologies_snapshot(self) -> str:
        parsed_html = self._gets_parsed_html_from_page(f"{self.public_url}/methodology")

        methodologies_accordion = parsed_html.find(id="themes")

        if methodologies_accordion is None:
            return "[]"

        theme_sections = methodologies_accordion.select('[data-testid="accordionSection"]') or []

        themes = []

        for theme_index, theme_html in enumerate(theme_sections):
            theme = {"theme_heading": theme_html.select_one(f"#themes-{theme_index + 1}-heading").string, "topics": []}

            topics = theme_html.select('[id^="topic-details-"]') or []

            for topic_html in topics:
                topic = {"topic_heading": topic_html.select_one('[id^="topic-heading-"]').string, "methodologies": []}

                methodologies = topic_html.select('[id^="methodology-heading-"]') or []

                for methodology_heading in methodologies:
                    topic["methodologies"].append(methodology_heading.string)

                theme["topics"].append(topic)

            themes.append(theme)
        return json.dumps(themes, sort_keys=True, indent=2)

    def validate_snapshots(self) -> None:
        failed = False

        for name in self.snapshots:
            if self._validate_snapshot(name):
                print(f"Snapshot {name} matches for {self.public_url}")
            else:
                print(f"Snapshot {name} does not match for {self.public_url}")
                failed = True

        if failed:
            # At least one snapshot did not match so send a failure message to slack
            self._send_slack_notification(
                blocks=[
                    {
                        "type": "header",
                        "text": {
                            "type": "plain_text",
                            "text": "Snapshots do not match. See pull request for more details",
                        },
                    },
                    {
                        "type": "actions",
                        "elements": [
                            {
                                "type": "button",
                                "style": "danger",
                                "text": {"type": "plain_text", "text": "View pull request"},
                                "url": "https://github.com/dfe-analytical-services/explore-education-statistics/pulls/dfe-sdt",
                            }
                        ],
                    },
                ]
            ) if self.slack_webhook_url else print("Snapshot script complete. Snapshots do not match")

        else:
            print("Snapshot script complete. No differences found.")

    def write_snapshots_to_file(self) -> None:
        for name in self.snapshots:
            self._write_snapshot_to_file(name)


if __name__ == "__main__":
    from get_webdriver import get_webdriver

    ap = argparse.ArgumentParser(
        prog=f"pipenv run python {os.path.basename(__file__)}",
        description="To create snapshots of specific public frontend pages",
    )

    ap.add_argument(
        "--public-url",
        dest="public_url",
        default="https://explore-education-statistics.service.gov.uk",
        nargs="?",
        help="URL of public frontend you wish to create snapshots for",
        type=str,
        required=False,
    )

    ap.add_argument(
        "--basic-auth-user",
        dest="basic_auth_user",
        help="Username for basic auth",
        type=str,
        required=False,
    )

    ap.add_argument(
        "--basic-auth-pass",
        dest="basic_auth_password",
        help="Password for basic auth",
        type=str,
        required=False,
    )

    ap.add_argument(
        "--validate",
        dest="validate",
        action="store_true",
        help="Compares the latest snapshots to the existing versions and does not write any snapshot files",
        required=False,
    )

    ap.add_argument(
        "--ci",
        dest="ci",
        action="store_true",
        help="Signifies that the test is running in a CI environment, which will make the test validate snapshots and write any snapshot files which have changed",
        required=False,
    )

    ap.add_argument(
        "--slack-webhook-url",
        dest="slack_webhook_url",
        default=None,
        help="URL for Slack webhook to send notifications when snapshot differences are found during validation",
        type=str,
        required=False,
    )

    ap.add_argument(
        "-v",
        "--visual",
        dest="visual",
        action="store_true",
        help="display browser window that the tests run in",
        required=False,
    )

    args = ap.parse_args()

    assert os.path.basename(os.getcwd()) == "robot-tests", "Must run from the robot-tests directory!"

    chrome_options = Options()

    if not args.visual:
        chrome_options.add_argument("--headless")

    get_webdriver("latest")

    driver = webdriver.Chrome(options=chrome_options)

    if args.basic_auth_user and args.basic_auth_password:
        driver.execute_cdp_cmd("Network.enable", {})
        token = base64.b64encode(f"{args.basic_auth_user}:{args.basic_auth_password}".encode())
        driver.execute_cdp_cmd("Network.setExtraHTTPHeaders", {"headers": {"Authorization": f"Basic {token.decode()}"}})

    snapshot_service = SnapshotService(public_url=args.public_url, slack_webhook_url=args.slack_webhook_url)

    if args.ci:
        snapshot_service.validate_snapshots()
        snapshot_service.write_snapshots_to_file()

    if args.validate:
        snapshot_service.validate_snapshots()

    driver.quit()
