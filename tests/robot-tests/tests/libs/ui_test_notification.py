"""
Notifications sent by the UI test runner at the end of a test run.

Each notification is defined once, independently of any chat service, and is
rendered into a Slack Block Kit message and a Teams Adaptive Card, so that both
channels receive the same message.
"""

from dataclasses import dataclass
from typing import Optional

from tests.libs.run_results import RunResults

# Adaptive Card colours. Slack headings cannot be coloured, so these apply to Teams only.
GOOD = "good"
WARNING = "warning"
ATTENTION = "attention"

# Failing suite sources are absolute paths on the build agent, and a bad run can produce
# dozens of them. Teams rejects oversized cards, so the list is shortened and capped.
MAX_LISTED_SUITES = 15
SUITE_SOURCE_SEGMENTS = 2


def suite_label(tests: str) -> str:
    """
    The suite argument is a path such as "tests/admin", or the default "tests/" when
    every suite is run, which would otherwise leave the label empty.
    """
    return tests.replace("tests/", "").strip("/") or "all tests"


def shorten_suite_sources(sources: tuple[str, ...]) -> tuple[str, ...]:
    shortened = ["/".join(source.replace("\\", "/").split("/")[-SUITE_SOURCE_SEGMENTS:]) for source in sources]

    if len(shortened) <= MAX_LISTED_SUITES:
        return tuple(shortened)

    return tuple(shortened[:MAX_LISTED_SUITES] + [f"…and {len(shortened) - MAX_LISTED_SUITES} more"])


@dataclass(frozen=True)
class UiTestNotification:
    title: str
    # Slack renders at most 10 fields in a section, so keep this under that limit.
    facts: tuple[tuple[str, str], ...] = ()
    details_title: Optional[str] = None
    details: tuple[str, ...] = ()
    body_title: Optional[str] = None
    body: Optional[str] = None
    colour: str = "default"
    link_url: Optional[str] = None
    link_text: Optional[str] = None
    # Slack uploads the test results archive beneath the message when this is set. Teams
    # incoming webhooks cannot accept files, so the Teams card links to the pipeline
    # artifacts instead.
    results_archive_name: Optional[str] = None

    def to_slack_blocks(self) -> list[dict]:
        blocks: list[dict] = [
            {"type": "header", "text": {"type": "plain_text", "text": self.title}},
            {
                "type": "section",
                "fields": [{"type": "mrkdwn", "text": f"*{name}*\n{value}"} for name, value in self.facts],
            },
        ]

        if self.details:
            blocks += [
                {"type": "divider"},
                {
                    "type": "section",
                    "text": {"type": "mrkdwn", "text": f"*{self.details_title}* ({len(self.details)})"},
                },
                {
                    "type": "rich_text",
                    "elements": [
                        {
                            "type": "rich_text_list",
                            "style": "bullet",
                            "elements": [
                                {"type": "rich_text_section", "elements": [{"type": "text", "text": detail}]}
                                for detail in self.details
                            ],
                        }
                    ],
                },
            ]

        if self.body:
            blocks += [
                {"type": "divider"},
                {"type": "section", "text": {"type": "mrkdwn", "text": f"*{self.body_title}*\n{self.body}"}},
            ]

        return blocks

    def to_teams_card(self) -> dict:
        body: list[dict] = [
            {
                "type": "TextBlock",
                "size": "Large",
                "weight": "Bolder",
                "text": self.title,
                "style": "heading",
                "color": self.colour,
                "wrap": True,
            },
            {"type": "FactSet", "facts": [{"title": name, "value": value} for name, value in self.facts]},
        ]

        if self.details:
            body += [
                {
                    "type": "TextBlock",
                    "weight": "Bolder",
                    "text": f"{self.details_title} ({len(self.details)})",
                    "separator": True,
                    "spacing": "Medium",
                },
                {"type": "TextBlock", "text": "\n".join(f"- {detail}" for detail in self.details), "wrap": True},
            ]

        if self.body:
            body += [
                {
                    "type": "TextBlock",
                    "weight": "Bolder",
                    "text": self.body_title,
                    "separator": True,
                    "spacing": "Medium",
                },
                {"type": "TextBlock", "text": self.body, "wrap": True},
            ]

        content = {
            "$schema": "http://adaptivecards.io/schemas/adaptive-card.json",
            "type": "AdaptiveCard",
            "version": "1.4",
            "body": body,
        }

        if self.link_url:
            content["actions"] = [{"type": "Action.OpenUrl", "title": self.link_text, "url": self.link_url}]

        return {
            "type": "message",
            "attachments": [
                {
                    "contentType": "application/vnd.microsoft.card.adaptive",
                    "content": content,
                }
            ],
        }


def ui_test_report(results: RunResults, artifacts_url: Optional[str] = None) -> UiTestNotification:
    failed = results.final.failed > 0

    return UiTestNotification(
        title=f"{'⚠️' if failed else '✅'} UI tests on {results.environment} — {results.suite_label}",
        colour=WARNING if failed else GOOD,
        facts=(
            ("Environment", results.environment),
            ("Suite", results.suite_label),
            ("Total run attempts", str(results.run_attempts)),
            ("Total test cases", str(results.merged.total)),
            ("Passed test cases", str(results.merged.passed)),
            ("Failed test cases", str(results.final.failed)),
            ("Skipped test cases", str(results.merged.skipped)),
            ("Flaky tests?", results.flakiness),
        ),
        details_title="Failed test suites",
        details=shorten_suite_sources(results.failed_suites),
        link_url=artifacts_url,
        link_text="View test results" if artifacts_url else None,
        results_archive_name=results.archive_name if results.attach_archive else None,
    )


def unreported_suites(
    environment: str,
    suites: tuple[tuple[str, str], ...],
    artifacts_url: Optional[str] = None,
) -> UiTestNotification:
    """
    Sent by the pipeline for suites that never reported themselves, which the test runner
    cannot report because it did not get far enough to run.
    """
    return UiTestNotification(
        title=f"❌ UI test suites did not report on {environment}",
        colour=ATTENTION,
        facts=(
            ("Environment", environment),
            ("Suites", str(len(suites))),
        ),
        details_title="Suites that did not report",
        details=tuple(f"{name} — job status: {status}" for name, status in suites),
        link_url=artifacts_url,
        link_text="View test results" if artifacts_url else None,
    )


def ui_test_exception(
    environment: str,
    suite_label: str,
    run_attempts: int,
    ex: Exception,
    artifacts_url: Optional[str] = None,
) -> UiTestNotification:
    return UiTestNotification(
        title=f"❌ UI test run failed on {environment} — {suite_label}",
        colour=ATTENTION,
        facts=(
            ("Environment", environment),
            ("Suite", suite_label),
            ("Run number", str(run_attempts)),
        ),
        body_title="Error details",
        body=repr(ex).replace("\n", ""),
        link_url=artifacts_url,
        link_text="View test results" if artifacts_url else None,
    )
