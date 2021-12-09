import os
from alerts import send_slack_alert
from utilities import raise_assertion_error
from scripts.create_snapshots \
    import create_find_statistics_snapshot, create_table_tool_snapshot, create_data_catalogue_snapshot, create_all_methodologies_snapshot
from slack_sdk.webhook import WebhookClient


def validate_find_stats_snapshot():
    with open('tests/snapshots/find_stats_snapshot.json', 'r') as file:
        snapshot = file.read()
    new_snapshot = create_find_statistics_snapshot(os.getenv('PUBLIC_URL'))
    if snapshot != new_snapshot:
        send_slack_alert(f"Find Statistics page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")
        raise_assertion_error(f"Find Statistics page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")


def validate_table_tool_snapshot():
    with open('tests/snapshots/table_tool_snapshot.json', 'r') as file:
        snapshot = file.read()
    new_snapshot = create_table_tool_snapshot(os.getenv('PUBLIC_URL'))
    if snapshot != new_snapshot:
        send_slack_alert(f"Table tool page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")
        raise_assertion_error(f"Table tool page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")
    pass


def validate_data_catalogue_snapshot():
    with open('tests/snapshots/data_catalogue_snapshot.json', 'r') as file:
        snapshot = file.read()
    new_snapshot = create_data_catalogue_snapshot(os.getenv('PUBLIC_URL'))
    if snapshot != new_snapshot:
        send_slack_alert(f"Data catalogue page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")
        raise_assertion_error(f"Data catalogue page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")


def validate_all_methodologies_snapshot():
    with open('tests/snapshots/all_methodologies_snapshot.json', 'r') as file:
        snapshot = file.read()
    new_snapshot = create_all_methodologies_snapshot(os.getenv('PUBLIC_URL'))
    if snapshot != new_snapshot:
        send_slack_alert(f"All methodologies page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")
        raise_assertion_error(
            f"All methodologies page snapshot doesn't match current page on {os.getenv('PUBLIC_URL')}")
