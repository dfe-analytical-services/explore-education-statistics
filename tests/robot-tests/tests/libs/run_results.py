"""
Reads the Robot Framework reports produced by a UI test run.

Totals are read from the merged report, but failures are read from the final run
attempt's report. Merging marks a test that failed in one attempt and passed in a
later one as PASSED, and reports the tests that followed a failure as SKIPPED, so
the merged report cannot say what was still failing when the run finished.
"""

import re
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from typing import Optional
from xml.etree import ElementTree

DEFINITELY = "Definitely"
DEFINITELY_NOT = "Definitely not"
LIKELY = "Likely"
UNLIKELY = "Unlikely"

# Written into the results folder, and so into the published pipeline artifact, once a
# test run has reported itself. It is what lets the pipeline tell a suite that reported
# from one that never got far enough to, such as a job that was cancelled or lost its
# agent, so that only the latter is reported by the pipeline fallback.
NOTIFICATION_MARKER_FILENAME = "chat-notification-sent"


def record_notification_sent(results_directory: Path) -> None:
    results_directory.mkdir(parents=True, exist_ok=True)
    (results_directory / NOTIFICATION_MARKER_FILENAME).touch()


def notification_was_sent(results_directory: Path) -> bool:
    return (results_directory / NOTIFICATION_MARKER_FILENAME).is_file()


@dataclass(frozen=True)
class RunTotals:
    passed: int = 0
    failed: int = 0
    skipped: int = 0

    @property
    def total(self) -> int:
        return self.passed + self.failed + self.skipped


def read_totals(report_path: Path) -> RunTotals:
    report = ElementTree.parse(report_path)
    total = report.find("./statistics/total/stat")

    if total is None:
        raise ValueError(f"Unable to find total statistics in {report_path}")

    return RunTotals(
        passed=int(total.attrib["pass"]),
        failed=int(total.attrib["fail"]),
        skipped=int(total.attrib["skip"]),
    )


def _get_run_number(run_directory: Path) -> Optional[int]:
    match = re.fullmatch(r"run-(\d+)", run_directory.name)
    return int(match.group(1)) if match else None


def find_run_directories(results_directory: Path) -> list[tuple[int, Path]]:
    """
    Every run attempt's reports, ordered by run number. A "run-0" folder is present
    when rerunning the failed suites of a previous execution, so the run numbers are
    not necessarily a count of the attempts made by this execution.
    """
    run_directories = [
        (run_number, run_directory)
        for run_directory in results_directory.glob("run-*")
        if (run_number := _get_run_number(run_directory)) is not None
    ]

    return sorted(run_directories, key=lambda run: run[0])


def flakiness(merged: RunTotals, final: RunTotals, run_attempts: int) -> str:
    """
    Genuine bugs fail every attempt, so they show as failures in both the merged report
    and the final attempt's report. Flaky tests can be spotted either by the run finally
    passing after more than one attempt, or by the two reports disagreeing on the number
    of failures.
    """
    if final.failed == 0:
        return DEFINITELY if run_attempts > 1 else DEFINITELY_NOT

    return LIKELY if merged.failed != final.failed else UNLIKELY


@dataclass(frozen=True)
class RunResults:
    environment: str
    suite_label: str
    run_attempts: int
    merged: RunTotals
    final: RunTotals
    failed_suites: tuple[str, ...] = field(default=())

    @property
    def flakiness(self) -> str:
        return flakiness(self.merged, self.final, self.run_attempts)

    @property
    def attach_archive(self) -> bool:
        """
        Archive the reports when there is something to investigate, i.e. a failure or a
        rerun that identifies a flaky test.
        """
        return bool(self.failed_suites) or self.run_attempts > 1

    @property
    def archive_name(self) -> str:
        suite = re.sub(r"[^A-Za-z0-9._-]+", "-", self.suite_label).strip("-")
        date = datetime.now(timezone.utc).strftime("%Y%m%d-%H%M%S")
        return f"UI-test-report-{suite}-{self.environment}-{date}.zip"


def read_run_results(
    results_directory: Path,
    environment: str,
    suite_label: str,
    run_attempts: int,
    failed_suites: tuple[str, ...] = (),
) -> RunResults:
    run_directories = find_run_directories(results_directory)

    if not run_directories:
        raise ValueError(f"Unable to find test run reports in {results_directory}")

    _, final_run_directory = run_directories[-1]

    return RunResults(
        environment=environment,
        suite_label=suite_label,
        run_attempts=run_attempts,
        merged=read_totals(results_directory / "output.xml"),
        final=read_totals(final_run_directory / "output.xml"),
        failed_suites=tuple(failed_suites),
    )
