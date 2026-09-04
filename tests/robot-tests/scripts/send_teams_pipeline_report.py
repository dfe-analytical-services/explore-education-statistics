import argparse
import os
import sys
from pathlib import Path

ROBOT_TESTS_DIRECTORY = Path(__file__).resolve().parent.parent
sys.path.insert(0, str(ROBOT_TESTS_DIRECTORY))

from tests.libs.logger import get_logger  # noqa: E402
from tests.libs.teams import (  # noqa: E402
    SuiteDefinition,
    TeamsService,
    build_pipeline_report_card,
    collect_suite_results,
    get_pipeline_artifacts_url,
)

logger = get_logger(__name__)

SUITES = [
    ("Public", "test-results-public", "UI_TEST_PUBLIC_RESULT"),
    ("Publish and amend", "test-results-admin-and-public-2", "UI_TEST_PUBLISH_AND_AMEND_RESULT"),
    ("Admin", "test-results-admin", "UI_TEST_ADMIN_RESULT"),
    ("Admin and public", "test-results-admin-public", "UI_TEST_ADMIN_AND_PUBLIC_RESULT"),
    ("Public API", "test-results-admin-public-api", "UI_TEST_PUBLIC_API_RESULT"),
    ("Seed data", "test-results-seed-data", "UI_TEST_SEED_DATA_RESULT"),
]


def send_pipeline_report(artifacts_directory: Path, environment: str) -> bool:
    suites = [
        SuiteDefinition(
            name=name,
            artifact_name=artifact_name,
            dependency_status=os.getenv(status_environment_variable, "unknown"),
        )
        for name, artifact_name, status_environment_variable in SUITES
    ]
    results = collect_suite_results(artifacts_directory, suites)
    artifacts_url = get_pipeline_artifacts_url(
        collection_uri=os.getenv("SYSTEM_TEAMFOUNDATIONCOLLECTIONURI", ""),
        project=os.getenv("SYSTEM_TEAMPROJECT", ""),
        build_id=os.getenv("BUILD_BUILDID", ""),
    )
    card = build_pipeline_report_card(environment, results, artifacts_url)
    return TeamsService().send_pipeline_report(card)


def main():
    parser = argparse.ArgumentParser(description="Send the completed UI test pipeline report to Teams")
    parser.add_argument("--artifacts-dir", type=Path, required=True, help="directory containing downloaded artifacts")
    parser.add_argument("--environment", default="dev", help="environment tested by the UI test pipeline")
    args = parser.parse_args()

    try:
        send_pipeline_report(args.artifacts_dir, args.environment)
    except Exception as ex:
        # Notification reporting must never change the result of the UI test pipeline.
        logger.warning(f"Unable to create UI test report for Teams: {ex}")


if __name__ == "__main__":
    main()
