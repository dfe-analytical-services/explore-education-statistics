import os

from scripts.create_snapshots import SnapshotService
from scripts.get_webdriver import get_webdriver
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from tests.libs.slack import SlackService
from utilities import raise_assertion_error

get_webdriver("latest")
chrome_options = Options()
chrome_options.add_argument("--headless")
driver = webdriver.Chrome(options=chrome_options)

public_url = os.getenv("PUBLIC_URL")

slack_service = SlackService()
snapshot_service = SnapshotService(public_url=public_url, driver=driver)


def validate_find_stats_snapshot():
    with open("tests/snapshots/find_statistics_snapshot.json", "r") as file:
        snapshot = file.read()
    new_snapshot = snapshot_service.create_find_statistics_snapshot()
    if snapshot != new_snapshot:
        slack_service.send_snapshot_alert(f"Find Statistics page snapshot doesn't match current page on {public_url}")
        raise_assertion_error(f"Find Statistics page snapshot doesn't match current page on {public_url}")


def validate_table_tool_snapshot():
    with open("tests/snapshots/table_tool_snapshot.json", "r") as file:
        snapshot = file.read()
    new_snapshot = snapshot_service.create_table_tool_snapshot()
    if snapshot != new_snapshot:
        slack_service.send_snapshot_alert(f"Table tool page snapshot doesn't match current page on {public_url}")
        raise_assertion_error(f"Table tool page snapshot doesn't match current page on {public_url}")
    pass


def validate_data_catalogue_snapshot():
    with open("tests/snapshots/data_catalogue_snapshot.json", "r") as file:
        snapshot = file.read()
    new_snapshot = snapshot_service.create_data_catalogue_snapshot()
    if snapshot != new_snapshot:
        slack_service.send_snapshot_alert(
            f"Data catalogue page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}"
        )
        raise_assertion_error(f"Data catalogue page snapshot doesn't match current page on {public_url}")


def validate_all_methodologies_snapshot():
    with open("tests/snapshots/methodologies_snapshot.json", "r") as file:
        snapshot = file.read()
    new_snapshot = snapshot_service.create_methodologies_snapshot()
    if snapshot != new_snapshot:
        slack_service.send_snapshot_alert(f"Methodologies page snapshot doesn't match current page on {public_url}")
        raise_assertion_error(f"Methodologies page snapshot doesn't match current page on {public_url}")
