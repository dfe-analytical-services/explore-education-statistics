import os

from scripts.create_snapshots import (
    create_all_methodologies_snapshot,
    create_data_catalogue_snapshot,
    create_find_statistics_snapshot,
    create_table_tool_snapshot,
)
from scripts.get_webdriver import get_webdriver
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from tests.libs.slack import SlackService
from utilities import raise_assertion_error

get_webdriver("latest")
chrome_options = Options()
chrome_options.add_argument("--headless")
driver = webdriver.Chrome(options=chrome_options)


slack_service = SlackService()


def validate_find_stats_snapshot():
    with open("tests/snapshots/find_stats_snapshot.json", "r") as file:
        snapshot = file.read()
    new_snapshot = create_find_statistics_snapshot(os.getenv("PUBLIC_URL"))
    if snapshot != new_snapshot:
        slack_service.send_snapshot_alert(
            f"Find Statistics page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}"
        )
        raise_assertion_error(f"Find Statistics page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")


def validate_table_tool_snapshot():
    with open("tests/snapshots/table_tool_snapshot.json", "r") as file:
        snapshot = file.read()
    new_snapshot = create_table_tool_snapshot(os.getenv("PUBLIC_URL"), driver)
    if snapshot != new_snapshot:
        slack_service.send_snapshot_alert(
            f"Table tool page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}"
        )
        raise_assertion_error(f"Table tool page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")
    pass


def validate_data_catalogue_snapshot():
    with open("tests/snapshots/data_catalogue_snapshot.json", "r") as file:
        snapshot = file.read()
    new_snapshot = create_data_catalogue_snapshot(os.getenv("PUBLIC_URL"), driver)
    if snapshot != new_snapshot:
        slack_service.send_snapshot_alert(
            f"Data catalogue page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}"
        )
        raise_assertion_error(f"Data catalogue page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")


def validate_all_methodologies_snapshot():
    with open("tests/snapshots/all_methodologies_snapshot.json", "r") as file:
        snapshot = file.read()
    new_snapshot = create_all_methodologies_snapshot(os.getenv("PUBLIC_URL"))
    if snapshot != new_snapshot:
        slack_service.send_snapshot_alert(
            f"All methodologies page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}"
        )
        raise_assertion_error(
            f"All methodologies page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}"
        )
