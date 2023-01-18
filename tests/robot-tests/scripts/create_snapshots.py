import argparse
import base64
import json
import math
import os

import requests
from bs4 import BeautifulSoup
from selenium import webdriver
from selenium.common import NoSuchElementException
from selenium.webdriver.chrome.options import Options
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait

"""
Script to check find statistics, table tool, methodologies and data catalogue snapshots.
By default, the script will run against production

Usage: pipenv run python scripts/create_snapshots.py
Optional flags:
  * --basic-auth-user "username" --basic-auth-pass "password"
  * --public-url "public URL"
"""


class SnapshotService:
    requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)

    def __init__(self, public_url, driver):
        self.session = requests.Session()
        self.driver = driver
        self.public_url = public_url
        self.timeout = 10
        self.page_size = 10

    def _gets_parsed_html_from_page(self, url: str) -> BeautifulSoup:
        response = self.session.get(url, stream=True)
        assert response.status_code == 200, f"Requests response wasn't 200!\nResponse: {response}"
        return BeautifulSoup(response.text, "html.parser")

    def _write_to_file(self, name: str, snapshot: list) -> None:
        path = "tests/snapshots"
        if not os.path.exists(path):
            os.makedirs(path)

        path_to_file = os.path.join(os.getcwd(), path, name)

        with open(path_to_file, "w") as file:
            file.write(snapshot)

    def create_find_statistics_snapshot(self) -> None:
        driver = self.driver
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
                lambda driver: driver.find_element(By.CSS_SELECTOR, "form[id='sortControlsForm']")
            )

            WebDriverWait(driver, self.timeout).until(
                lambda driver: driver.find_element(By.XPATH, "//*[@data-testid='publicationsList']")
            )

            publications_list = driver.find_element(By.XPATH, "//*[@data-testid='publicationsList']")

            all_publications = publications_list.find_elements(By.CSS_SELECTOR, "li")

            for publication in all_publications:
                publication.click()

                publication_details = publication.find_element(By.CSS_SELECTOR, "dl")

                publication_title = publication.find_element(By.CSS_SELECTOR, "h3").text

                try:
                    publication_summary = publication.find_element(By.CSS_SELECTOR, "p").text
                except NoSuchElementException:
                    publication_summary = ""

                release_type = publication_details.find_element(
                    By.XPATH, "//*[dl]//div//dd[@data-testid='release-type']"
                ).text

                theme = publication_details.find_element(By.XPATH, "//*[dl]//div//dd[@data-testid='theme']").text

                published = publication_details.find_element(
                    By.XPATH, "//*[dl]//div//dd[@data-testid='published']"
                ).text

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
                    lambda driver: driver.find_element(By.XPATH, "//*[@data-testid='publicationsList']")
                )

            try:
                driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
                driver.find_element(By.XPATH, "//*[@data-testid='pagination-next']").click()
            except NoSuchElementException:
                self._write_to_file("find_statistics_snapshot.json", json.dumps(publications, sort_keys=True, indent=2))
                break

    def create_table_tool_snapshot(self) -> None:
        driver = self.driver
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

        self._write_to_file("table_tool_snapshot.json", json.dumps(themes, sort_keys=True, indent=2))

    def create_data_catalogue_snapshot(self) -> None:
        driver = self.driver
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

        self._write_to_file("data_catalogue_snapshot.json", json.dumps(themes, sort_keys=True, indent=2))

    def create_methodologies_snapshot(self) -> None:
        parsed_html = self._gets_parsed_html_from_page(f"{self.public_url}/methodology")

        methodologies_accordion = parsed_html.find(id="themes")

        if methodologies_accordion is None:
            return []

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
        self._write_to_file("methodologies_snapshot.json", json.dumps(themes, sort_keys=True, indent=2))


if __name__ == "__main__":
    from get_webdriver import get_webdriver

    ap = argparse.ArgumentParser(
        prog=f"python {os.path.basename(__file__)}", description="To create snapshots of specific public frontend pages"
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
        dest="basic_auth_pass",
        help="Password for basic auth",
        type=str,
        required=False,
    )

    args = ap.parse_args()

    assert os.path.basename(os.getcwd()) == "robot-tests", "Must run from the robot-tests directory!"

    get_webdriver("latest")
    chrome_options = Options()
    chrome_options.add_argument("--headless")
    driver = webdriver.Chrome(options=chrome_options)

    if args.basic_auth_user and args.basic_auth_pass:
        driver.execute_cdp_cmd("Network.enable", {})
        token = base64.b64encode(f"{args.basic_auth_user}:{args.basic_auth_pass}".encode())
        driver.execute_cdp_cmd("Network.setExtraHTTPHeaders", {"headers": {"Authorization": f"Basic {token.decode()}"}})

    snapshot_service = SnapshotService(args.public_url, driver)

    snapshot_service.create_find_statistics_snapshot()
    snapshot_service.create_table_tool_snapshot()
    snapshot_service.create_data_catalogue_snapshot()
    snapshot_service.create_methodologies_snapshot()
    driver.quit()
