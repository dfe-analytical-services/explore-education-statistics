import json
import os
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Optional
from urllib.error import HTTPError, URLError
from urllib.parse import quote
from urllib.request import Request, urlopen
from xml.etree import ElementTree

from tests.libs.logger import get_logger

logger = get_logger(__name__)

PASSED = "passed"
FAILED = "failed"
FLAKY = "flaky"
UNAVAILABLE = "results unavailable"

SUCCESSFUL_JOB_STATUSES = {"succeeded", "succeededwithissues"}


@dataclass(frozen=True)
class SuiteDefinition:
    name: str
    artifact_name: str
    dependency_status: str


@dataclass(frozen=True)
class SuiteResult:
    name: str
    artifact_name: str
    dependency_status: str
    passed: int = 0
    failed: int = 0
    skipped: int = 0
    run_attempts: int = 0
    final_failures: int = 0
    classification: str = UNAVAILABLE

    @property
    def total(self) -> int:
        return self.passed + self.failed + self.skipped


def _read_totals(report_path: Path) -> tuple[int, int, int]:
    report = ElementTree.parse(report_path)
    total = report.find("./statistics/total/stat")

    if total is None:
        raise ValueError(f"Unable to find total statistics in {report_path}")

    return int(total.attrib["pass"]), int(total.attrib["fail"]), int(total.attrib["skip"])


def _get_run_number(run_directory: Path) -> Optional[int]:
    match = re.fullmatch(r"run-(\d+)", run_directory.name)
    return int(match.group(1)) if match else None


def collect_suite_result(artifacts_directory: Path, suite: SuiteDefinition) -> SuiteResult:
    artifact_directory = artifacts_directory / suite.artifact_name
    merged_report_path = artifact_directory / "output.xml"

    try:
        passed, failed, skipped = _read_totals(merged_report_path)
        run_directories = [
            (run_number, run_directory)
            for run_directory in artifact_directory.glob("run-*")
            if (run_number := _get_run_number(run_directory)) is not None
        ]

        if not run_directories:
            raise ValueError(f"Unable to find test run reports in {artifact_directory}")

        _, final_run_directory = max(run_directories, key=lambda run: run[0])
        _, final_failures, _ = _read_totals(final_run_directory / "output.xml")
    except (ElementTree.ParseError, OSError, KeyError, TypeError, ValueError) as ex:
        logger.warning(f'Unable to read results for "{suite.name}": {ex}')
        return SuiteResult(
            name=suite.name,
            artifact_name=suite.artifact_name,
            dependency_status=suite.dependency_status,
        )

    dependency_succeeded = suite.dependency_status.lower() in SUCCESSFUL_JOB_STATUSES

    if final_failures > 0 or suite.dependency_status.lower() == "failed":
        classification = FAILED
    elif not dependency_succeeded:
        classification = UNAVAILABLE
    elif len(run_directories) > 1:
        classification = FLAKY
    else:
        classification = PASSED

    return SuiteResult(
        name=suite.name,
        artifact_name=suite.artifact_name,
        dependency_status=suite.dependency_status,
        passed=passed,
        failed=failed,
        skipped=skipped,
        run_attempts=len(run_directories),
        final_failures=final_failures,
        classification=classification,
    )


def collect_suite_results(artifacts_directory: Path, suites: list[SuiteDefinition]) -> list[SuiteResult]:
    return [collect_suite_result(artifacts_directory, suite) for suite in suites]


def get_pipeline_artifacts_url(collection_uri: str, project: str, build_id: str) -> Optional[str]:
    if not collection_uri or not project or not build_id:
        return None

    encoded_project = quote(project, safe="")
    encoded_build_id = quote(build_id, safe="")
    return (
        f"{collection_uri.rstrip('/')}/{encoded_project}/_build/results"
        f"?buildId={encoded_build_id}&view=artifacts&pathAsName=false&type=publishedArtifacts"
    )


def _get_pipeline_status(results: list[SuiteResult]) -> tuple[str, str, str]:
    if any(result.classification in {FAILED, UNAVAILABLE} for result in results):
        return "❌", "attention", "UI test pipeline completed with failures"
    if any(result.classification == FLAKY for result in results):
        return "⚠️", "warning", "UI test pipeline completed with flaky tests"
    return "✅", "good", "UI test pipeline passed"


def _format_attention_result(result: SuiteResult) -> str:
    if result.classification == UNAVAILABLE:
        detail = f"job status: {result.dependency_status or 'unknown'}"
    elif result.classification == FLAKY:
        detail = f"passed after {result.run_attempts} attempts"
    elif result.final_failures == 0:
        detail = f"job status: {result.dependency_status or 'unknown'}"
    else:
        detail = f"{result.final_failures} final failures after {result.run_attempts} attempts"

    return f"- **{result.name}** — {result.classification}; `{result.artifact_name}`; {detail}"


def build_pipeline_report_card(
    environment: str,
    results: list[SuiteResult],
    artifacts_url: Optional[str],
) -> dict:
    status_emoji, status_color, title = _get_pipeline_status(results)
    attention_results = [result for result in results if result.classification != PASSED]

    facts = [
        {"title": "Environment", "value": environment},
        {"title": "Test suites", "value": str(len(results))},
        {"title": "Total test cases", "value": str(sum(result.total for result in results))},
        {"title": "Passed test cases", "value": str(sum(result.passed for result in results))},
        {"title": "Failed test cases", "value": str(sum(result.failed for result in results))},
        {"title": "Skipped test cases", "value": str(sum(result.skipped for result in results))},
        {
            "title": "Flaky suites",
            "value": str(sum(result.classification == FLAKY for result in results)),
        },
        {
            "title": "Unavailable results",
            "value": str(sum(result.classification == UNAVAILABLE for result in results)),
        },
    ]

    body = [
        {
            "type": "TextBlock",
            "size": "Large",
            "weight": "Bolder",
            "text": f"{status_emoji} {title}",
            "style": "heading",
            "color": status_color,
            "wrap": True,
        },
        {"type": "FactSet", "facts": facts},
    ]

    if attention_results:
        body.extend(
            [
                {
                    "type": "TextBlock",
                    "weight": "Bolder",
                    "text": "Failed, flaky or unavailable suites",
                    "separator": True,
                    "spacing": "Medium",
                },
                {
                    "type": "TextBlock",
                    "text": "\n".join(_format_attention_result(result) for result in attention_results),
                    "wrap": True,
                },
            ]
        )

    content = {
        "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
        "type": "AdaptiveCard",
        "version": "1.4",
        "body": body,
    }

    if artifacts_url:
        content["actions"] = [
            {
                "type": "Action.OpenUrl",
                "title": "View pipeline artifacts",
                "url": artifacts_url,
            }
        ]

    return {
        "type": "message",
        "attachments": [
            {
                "contentType": "application/vnd.microsoft.card.adaptive",
                "content": content,
            }
        ],
    }


class TeamsService:
    def __init__(self, webhook_url: Optional[str] = None):
        self.webhook_url = webhook_url or os.getenv("TEAMS_UI_TESTS_WEBHOOK_URL")

    def _post_card(self, card: dict, description: str) -> bool:
        if not self.webhook_url:
            logger.warning(f"Teams webhook URL is not set; skipping {description}")
            return False

        try:
            request = Request(
                self.webhook_url,
                data=json.dumps(card).encode("utf-8"),
                headers={"Content-Type": "application/json"},
                method="POST",
            )
            with urlopen(request, timeout=30) as response:
                if not 200 <= response.status < 300:
                    logger.warning(f"Teams webhook returned HTTP {response.status}")
                    return False
        except (HTTPError, URLError, TimeoutError, ValueError) as ex:
            logger.warning(f"Unable to send {description} to Teams: {ex}")
            return False

        logger.info(f"Sent {description} to Teams")
        return True

    def send_pipeline_report(self, card: dict) -> bool:
        return self._post_card(card, "UI test pipeline report")

    def send_snapshot_notification(self, card: dict) -> bool:
        return self._post_card(card, "snapshot notification")
