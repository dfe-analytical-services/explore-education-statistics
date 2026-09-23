import unittest

from tests.libs.run_results import RunResults, RunTotals
from tests.libs.ui_test_notification import (
    ATTENTION,
    GOOD,
    MAX_LISTED_SUITES,
    UNKNOWN_SUITE,
    WARNING,
    shorten_suite_sources,
    suite_label,
    ui_test_exception,
    ui_test_report,
)

ARTIFACTS_URL = "https://dev.azure.com/example/project/_build/results?buildId=123&view=artifacts"

# The labels the report has always shown. Pinned so that neither channel can drift.
EXPECTED_FACT_TITLES = [
    "Environment",
    "Suite",
    "Total run attempts",
    "Total test cases",
    "Passed test cases",
    "Failed test cases",
    "Skipped test cases",
    "Flaky tests?",
]


def _teams_content(notification) -> dict:
    return notification.to_teams_card()["attachments"][0]["content"]


def _slack_fields(notification) -> list[str]:
    return [field["text"] for field in notification.to_slack_blocks()[1]["fields"]]


def _results(
    failed: int = 0,
    run_attempts: int = 1,
    failed_suites: tuple = (),
    environment: str = "preprod",
    suite_label_value: str = "admin",
) -> RunResults:
    return RunResults(
        environment=environment,
        suite_label=suite_label_value,
        run_attempts=run_attempts,
        merged=RunTotals(passed=10, failed=failed, skipped=2),
        final=RunTotals(passed=10, failed=failed, skipped=2),
        failed_suites=failed_suites,
    )


class SuiteLabelTests(unittest.TestCase):
    def test_strips_the_tests_prefix(self):
        self.assertEqual("admin", suite_label("tests/admin"))

    def test_labels_the_default_suite_as_all_tests(self):
        self.assertEqual("all tests", suite_label("tests/"))


class ShortenSuiteSourcesTests(unittest.TestCase):
    def test_keeps_only_the_last_two_path_segments(self):
        sources = ("/home/vsts/work/1/s/tests/robot-tests/tests/admin/bau/publication.robot",)

        self.assertEqual(("bau/publication.robot",), shorten_suite_sources(sources))

    def test_handles_windows_separators(self):
        sources = (r"C:\repo\tests\robot-tests\tests\admin\bau\publication.robot",)

        self.assertEqual(("bau/publication.robot",), shorten_suite_sources(sources))

    def test_labels_a_suite_with_no_source(self):
        """
        Robot reports a suite element with no source attribute as None, which must not take
        out the whole notification.
        """
        sources = ("tests/admin/bau/publication.robot", None)

        self.assertEqual(("bau/publication.robot", UNKNOWN_SUITE), shorten_suite_sources(sources))

    def test_caps_the_list_and_counts_the_remainder(self):
        sources = tuple(f"tests/admin/suite-{index}.robot" for index in range(MAX_LISTED_SUITES + 3))

        shortened = shorten_suite_sources(sources)

        self.assertEqual(MAX_LISTED_SUITES + 1, len(shortened))
        self.assertEqual("…and 3 more", shortened[-1])


class UiTestReportTests(unittest.TestCase):
    def setUp(self):
        self.notification = ui_test_report(_results(), ARTIFACTS_URL)

    def test_slack_blocks_contain_the_expected_facts(self):
        self.assertEqual(
            [f"*{title}*" for title in EXPECTED_FACT_TITLES],
            [field.split("\n")[0] for field in _slack_fields(self.notification)],
        )

    def test_teams_card_contains_the_same_facts(self):
        facts = _teams_content(self.notification)["body"][1]["facts"]

        self.assertEqual(EXPECTED_FACT_TITLES, [fact["title"] for fact in facts])

    def test_slack_blocks_report_the_environment_and_suite(self):
        fields = _slack_fields(self.notification)

        self.assertIn("*Environment*\npreprod", fields)
        self.assertIn("*Suite*\nadmin", fields)

    def test_teams_card_reports_the_same_environment_and_suite(self):
        facts = _teams_content(self.notification)["body"][1]["facts"]

        self.assertEqual("preprod", next(fact["value"] for fact in facts if fact["title"] == "Environment"))
        self.assertEqual("admin", next(fact["value"] for fact in facts if fact["title"] == "Suite"))

    def test_title_names_the_environment_and_suite(self):
        self.assertEqual("✅ UI tests on preprod — admin", self.notification.title)

    def test_counts_total_cases_from_the_merged_report_and_failures_from_the_final_run(self):
        results = RunResults(
            environment="dev",
            suite_label="admin",
            run_attempts=2,
            merged=RunTotals(passed=10, failed=3, skipped=2),
            final=RunTotals(passed=13, failed=1, skipped=1),
        )

        facts = dict(ui_test_report(results).facts)

        self.assertEqual("15", facts["Total test cases"])
        self.assertEqual("10", facts["Passed test cases"])
        self.assertEqual("1", facts["Failed test cases"])
        self.assertEqual("2", facts["Skipped test cases"])

    def test_passing_run_is_coloured_good(self):
        self.assertEqual(GOOD, ui_test_report(_results()).colour)

    def test_failing_run_is_coloured_warning(self):
        self.assertEqual(WARNING, ui_test_report(_results(failed=2)).colour)

    def test_slack_blocks_list_the_failed_suites(self):
        notification = ui_test_report(_results(failed=1, failed_suites=("tests/admin/bau/publication.robot",)))
        blocks = notification.to_slack_blocks()

        self.assertEqual("*Failed test suites* (1)", blocks[3]["text"]["text"])
        listed = blocks[4]["elements"][0]["elements"][0]["elements"][0]["text"]
        self.assertEqual("bau/publication.robot", listed)

    def test_teams_card_lists_the_same_failed_suites(self):
        notification = ui_test_report(_results(failed=1, failed_suites=("tests/admin/bau/publication.robot",)))
        body = _teams_content(notification)["body"]

        self.assertEqual("Failed test suites (1)", body[2]["text"])
        self.assertEqual("- bau/publication.robot", body[3]["text"])

    def test_omits_the_failed_suites_section_for_a_passing_run(self):
        notification = ui_test_report(_results())

        self.assertEqual(2, len(notification.to_slack_blocks()))
        self.assertEqual(2, len(_teams_content(notification)["body"]))

    def test_teams_card_links_to_the_pipeline_artifacts(self):
        actions = _teams_content(self.notification)["actions"]

        self.assertEqual(1, len(actions))
        self.assertEqual(ARTIFACTS_URL, actions[0]["url"])
        self.assertEqual("View test results", actions[0]["title"])

    def test_teams_card_has_no_action_outside_a_pipeline(self):
        self.assertNotIn("actions", _teams_content(ui_test_report(_results())))

    def test_attaches_the_results_archive_only_when_there_is_something_to_investigate(self):
        self.assertIsNone(ui_test_report(_results()).results_archive_name)
        self.assertIsNotNone(ui_test_report(_results(run_attempts=2)).results_archive_name)


class MultiLineError(Exception):
    def __repr__(self) -> str:
        return "MultiLineError(\nboom\n)"


class UiTestExceptionTests(unittest.TestCase):
    def setUp(self):
        self.notification = ui_test_exception("preprod", "admin", 2, ValueError("boom"))

    def test_title_names_the_environment_and_suite(self):
        self.assertEqual("❌ UI test run failed on preprod — admin", self.notification.title)

    def test_is_coloured_attention(self):
        self.assertEqual(ATTENTION, self.notification.colour)

    def test_slack_blocks_contain_the_error_details(self):
        section = self.notification.to_slack_blocks()[-1]

        self.assertEqual("*Error details*\nValueError('boom')", section["text"]["text"])

    def test_teams_card_contains_the_same_error_details(self):
        body = _teams_content(self.notification)["body"]

        self.assertEqual("Error details", body[2]["text"])
        self.assertEqual("ValueError('boom')", body[3]["text"])

    def test_error_details_are_flattened_onto_one_line(self):
        notification = ui_test_exception("dev", "admin", 1, MultiLineError())

        self.assertEqual("MultiLineError(boom)", notification.body)

    def test_reports_the_run_number(self):
        self.assertEqual("2", dict(self.notification.facts)["Run number"])


if __name__ == "__main__":
    unittest.main()
