"""
Reports the UI test suites that never reported themselves.

Every suite that runs reports its own result from inside the test runner, so this covers
only the suites that never got that far: a job that was cancelled, lost its agent, timed
out, or failed before the runner started. Without it those failures are silent.
"""

import argparse
import os
import sys
from pathlib import Path

ROBOT_TESTS_DIRECTORY = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROBOT_TESTS_DIRECTORY))

from tests.libs import azure_pipelines  # noqa: E402
from tests.libs.logger import get_logger  # noqa: E402
from tests.libs.run_results import notification_was_sent  # noqa: E402
from tests.libs.teams import TeamsService  # noqa: E402
from tests.libs.ui_test_notification import unreported_suites  # noqa: E402

logger = get_logger(__name__)

UNKNOWN_STATUS = "unknown"

SUITES = [
    ("Public", "test-results-public", "UI_TEST_PUBLIC_RESULT"),
    ("Publish and amend", "test-results-admin-and-public-2", "UI_TEST_PUBLISH_AND_AMEND_RESULT"),
    ("Admin", "test-results-admin", "UI_TEST_ADMIN_RESULT"),
    ("Admin and public", "test-results-admin-public", "UI_TEST_ADMIN_AND_PUBLIC_RESULT"),
    ("Public API", "test-results-admin-public-api", "UI_TEST_PUBLIC_API_RESULT"),
    ("Seed data", "test-results-seed-data", "UI_TEST_SEED_DATA_RESULT"),
]


def find_unreported_suites(artifacts_directory: Path, suites: list) -> tuple[tuple[str, str], ...]:
    return tuple(
        (name, os.getenv(status_environment_variable) or UNKNOWN_STATUS)
        for name, artifact_name, status_environment_variable in suites
        if not notification_was_sent(artifacts_directory / artifact_name)
    )


def send_unreported_suites(artifacts_directory: Path, environment: str) -> bool:
    unreported = find_unreported_suites(artifacts_directory, SUITES)

    if not unreported:
        logger.info("Every UI test suite reported its own result; there is nothing to report")
        return False

    notification = unreported_suites(environment, unreported, azure_pipelines.current_results_url())
    return TeamsService().send_test_report(notification.to_teams_card())


def main():
    parser = argparse.ArgumentParser(description="Report the UI test suites that never reported themselves")
    parser.add_argument("--artifacts-dir", type=Path, required=True, help="directory containing downloaded artifacts")
    parser.add_argument("--environment", default="dev", help="environment tested by the UI test pipeline")
    args = parser.parse_args()

    try:
        send_unreported_suites(args.artifacts_dir, args.environment)
    except Exception as ex:
        # Notification reporting must never change the result of the UI test pipeline.
        logger.warning(f"Unable to report the UI test suites that did not report: {ex}")


if __name__ == "__main__":
    main()
