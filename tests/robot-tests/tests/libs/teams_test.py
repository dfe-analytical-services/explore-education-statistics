import tempfile
import unittest
from pathlib import Path
from unittest.mock import MagicMock, patch
from urllib.error import HTTPError, URLError

from scripts.send_teams_pipeline_report import SUITES, send_pipeline_report
from tests.libs.teams import (
    FAILED,
    FLAKY,
    PASSED,
    UNAVAILABLE,
    SuiteDefinition,
    SuiteResult,
    TeamsService,
    build_pipeline_report_card,
    collect_suite_result,
    get_pipeline_artifacts_url,
)


def _write_report(path: Path, passed: int, failed: int, skipped: int, flaky_tests: list[str] = None):
    path.parent.mkdir(parents=True, exist_ok=True)
    tests = "".join(f'<test name="{test_name}"><tag>Flaky</tag></test>' for test_name in flaky_tests or [])
    path.write_text(
        f"<robot><suite>{tests}</suite><statistics><total>"
        f'<stat pass="{passed}" fail="{failed}" skip="{skipped}" />'
        "</total></statistics></robot>",
        encoding="utf-8",
    )


class CollectSuiteResultTests(unittest.TestCase):
    def setUp(self):
        self.temporary_directory = tempfile.TemporaryDirectory()
        self.artifacts_directory = Path(self.temporary_directory.name)

    def tearDown(self):
        self.temporary_directory.cleanup()

    def create_suite(self, dependency_status: str = "Succeeded") -> SuiteDefinition:
        return SuiteDefinition(
            name="Public & API",
            artifact_name="test-results-public-api",
            dependency_status=dependency_status,
        )

    def write_suite_reports(
        self,
        merged: tuple[int, int, int],
        final: tuple[int, int, int],
        run_attempts: int = 1,
        flaky_tests: list[str] = None,
    ):
        artifact_directory = self.artifacts_directory / "test-results-public-api"
        _write_report(artifact_directory / "output.xml", *merged, flaky_tests=flaky_tests)

        for run_attempt in range(1, run_attempts + 1):
            run_totals = final if run_attempt == run_attempts else merged
            _write_report(artifact_directory / f"run-{run_attempt}" / "output.xml", *run_totals)

    def test_classifies_passing_suite(self):
        self.write_suite_reports(merged=(8, 0, 1), final=(8, 0, 1))

        result = collect_suite_result(self.artifacts_directory, self.create_suite())

        self.assertEqual(PASSED, result.classification)
        self.assertEqual((8, 0, 1, 1), (result.passed, result.failed, result.skipped, result.run_attempts))

    def test_classifies_final_failures(self):
        self.write_suite_reports(merged=(6, 2, 1), final=(0, 2, 0), run_attempts=3)

        result = collect_suite_result(self.artifacts_directory, self.create_suite("Failed"))

        self.assertEqual(FAILED, result.classification)
        self.assertEqual(2, result.final_failures)
        self.assertEqual(3, result.run_attempts)

    def test_classifies_suite_that_passes_on_rerun_as_flaky(self):
        self.write_suite_reports(merged=(8, 0, 1), final=(2, 0, 0), run_attempts=2)

        result = collect_suite_result(self.artifacts_directory, self.create_suite())

        self.assertEqual(FLAKY, result.classification)
        self.assertEqual(2, result.run_attempts)

    def test_reports_tests_that_were_rescued_by_a_rerun_as_flaky(self):
        self.write_suite_reports(merged=(8, 0, 1), final=(8, 0, 1), flaky_tests=["Upload datafile", "Approve release"])

        result = collect_suite_result(self.artifacts_directory, self.create_suite())

        self.assertEqual(FLAKY, result.classification)
        self.assertEqual(("Approve release", "Upload datafile"), result.flaky_tests)

    def test_reports_no_flaky_tests_when_none_are_tagged(self):
        self.write_suite_reports(merged=(8, 0, 1), final=(8, 0, 1))

        result = collect_suite_result(self.artifacts_directory, self.create_suite())

        self.assertEqual(PASSED, result.classification)
        self.assertEqual((), result.flaky_tests)

    def test_classifies_failed_job_even_when_report_has_no_final_failures(self):
        self.write_suite_reports(merged=(8, 0, 1), final=(8, 0, 1))

        result = collect_suite_result(self.artifacts_directory, self.create_suite("Failed"))

        self.assertEqual(FAILED, result.classification)

        card = build_pipeline_report_card("dev", [result], None)
        self.assertIn("job status: Failed", card["attachments"][0]["content"]["body"][3]["text"])

    def test_classifies_canceled_job_with_a_report_as_unavailable(self):
        self.write_suite_reports(merged=(8, 0, 1), final=(8, 0, 1))

        result = collect_suite_result(self.artifacts_directory, self.create_suite("Canceled"))

        self.assertEqual(UNAVAILABLE, result.classification)

    def test_classifies_missing_report_as_unavailable(self):
        result = collect_suite_result(self.artifacts_directory, self.create_suite("Canceled"))

        self.assertEqual(UNAVAILABLE, result.classification)
        self.assertEqual("test-results-public-api", result.artifact_name)

    def test_classifies_malformed_report_as_unavailable(self):
        report_path = self.artifacts_directory / "test-results-public-api" / "output.xml"
        report_path.parent.mkdir(parents=True)
        report_path.write_text("not xml", encoding="utf-8")

        result = collect_suite_result(self.artifacts_directory, self.create_suite())

        self.assertEqual(UNAVAILABLE, result.classification)


class BuildPipelineReportCardTests(unittest.TestCase):
    def test_builds_aggregate_card_with_only_attention_suites(self):
        results = [
            SuiteResult("Public", "public-artifact", "Succeeded", 10, 0, 1, 1, 0, PASSED),
            SuiteResult("Admin & public", "admin & artifact", "Failed", 8, 2, 3, 3, 2, FAILED),
            SuiteResult("Public API", "api-artifact", "Succeeded", 7, 0, 0, 2, 0, FLAKY, ("Choose locations",)),
            SuiteResult("Seed data", "seed-artifact", "Canceled"),
        ]
        artifacts_url = "https://dev.azure.com/example/project/_build/results?buildId=123&view=artifacts"

        card = build_pipeline_report_card("dev", results, artifacts_url)

        content = card["attachments"][0]["content"]
        facts = content["body"][1]["facts"]
        details = content["body"][3]["text"]
        self.assertEqual("31", next(fact["value"] for fact in facts if fact["title"] == "Total test cases"))
        self.assertEqual("1", next(fact["value"] for fact in facts if fact["title"] == "Flaky test cases"))
        self.assertIn("flaky tests: Choose locations", details)
        self.assertNotIn("**Public**", details)
        self.assertIn("**Admin & public**", details)
        self.assertIn("`admin & artifact`", details)
        self.assertIn("**Public API**", details)
        self.assertIn("**Seed data**", details)
        self.assertEqual(artifacts_url, content["actions"][0]["url"])
        self.assertEqual(1, len(content["actions"]))

    def test_builds_success_card_without_attention_section(self):
        results = [
            SuiteResult(f"Suite {index}", f"artifact-{index}", "Succeeded", 10, 0, 0, 1, 0, PASSED)
            for index in range(1, 7)
        ]

        card = build_pipeline_report_card("dev", results, None)

        content = card["attachments"][0]["content"]
        self.assertEqual(2, len(content["body"]))
        self.assertNotIn("actions", content)
        self.assertIn("pipeline passed", content["body"][0]["text"])
        self.assertEqual("6", content["body"][1]["facts"][1]["value"])

    def test_encodes_project_and_build_id_in_artifacts_url(self):
        url = get_pipeline_artifacts_url(
            "https://dev.azure.com/example/",
            "Education & Statistics",
            "build/123",
        )

        self.assertEqual(
            "https://dev.azure.com/example/Education%20%26%20Statistics/_build/results"
            "?buildId=build%2F123&view=artifacts&pathAsName=false&type=publishedArtifacts",
            url,
        )

    def test_returns_no_artifacts_url_when_build_context_is_missing(self):
        self.assertIsNone(get_pipeline_artifacts_url("", "project", "123"))
        self.assertIsNone(get_pipeline_artifacts_url("https://dev.azure.com/example", "", "123"))
        self.assertIsNone(get_pipeline_artifacts_url("https://dev.azure.com/example", "project", ""))


class TeamsServiceTests(unittest.TestCase):
    def setUp(self):
        self.card = {"type": "message"}

    @patch("tests.libs.teams.urlopen")
    def test_returns_true_for_successful_webhook_response(self, urlopen_mock):
        response = MagicMock()
        response.status = 200
        urlopen_mock.return_value.__enter__.return_value = response

        result = TeamsService("https://example.com/webhook").send_pipeline_report(self.card)

        self.assertTrue(result)
        request = urlopen_mock.call_args.args[0]
        self.assertEqual("POST", request.method)
        self.assertEqual("application/json", request.headers["Content-type"])

    @patch("tests.libs.teams.urlopen", side_effect=TimeoutError("timed out"))
    def test_returns_false_for_webhook_timeout(self, _):
        self.assertFalse(TeamsService("https://example.com/webhook").send_pipeline_report(self.card))

    @patch("tests.libs.teams.urlopen", side_effect=URLError("unavailable"))
    def test_returns_false_for_webhook_connection_error(self, _):
        self.assertFalse(TeamsService("https://example.com/webhook").send_pipeline_report(self.card))

    @patch("tests.libs.teams.urlopen")
    def test_returns_false_for_webhook_http_error(self, urlopen_mock):
        urlopen_mock.side_effect = HTTPError("https://example.com", 500, "failure", {}, None)

        self.assertFalse(TeamsService("https://example.com/webhook").send_pipeline_report(self.card))

    def test_returns_false_for_malformed_webhook_url(self):
        self.assertFalse(TeamsService("://bad-url").send_pipeline_report(self.card))

    def test_returns_false_when_webhook_is_missing(self):
        with patch.dict("os.environ", {}, clear=True):
            self.assertFalse(TeamsService().send_pipeline_report(self.card))


class SendPipelineReportTests(unittest.TestCase):
    @patch("scripts.send_teams_pipeline_report.TeamsService")
    def test_sends_one_card_containing_all_six_suites(self, teams_service_mock):
        teams_service_mock.return_value.send_pipeline_report.return_value = True

        with tempfile.TemporaryDirectory() as temporary_directory:
            artifacts_directory = Path(temporary_directory)
            for _, artifact_name, _ in SUITES:
                _write_report(artifacts_directory / artifact_name / "output.xml", 10, 0, 0)
                _write_report(artifacts_directory / artifact_name / "run-1" / "output.xml", 10, 0, 0)

            environment = {status_environment_variable: "Succeeded" for _, _, status_environment_variable in SUITES}
            environment.update(
                {
                    "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI": "https://dev.azure.com/example/",
                    "SYSTEM_TEAMPROJECT": "Education Statistics",
                    "BUILD_BUILDID": "123",
                    "TEAMS_UI_TESTS_WEBHOOK_URL": "https://example.com/webhook",
                }
            )

            with patch.dict("os.environ", environment, clear=True):
                result = send_pipeline_report(artifacts_directory, "dev")

        self.assertTrue(result)
        teams_service_mock.return_value.send_pipeline_report.assert_called_once()
        card = teams_service_mock.return_value.send_pipeline_report.call_args.args[0]
        facts = card["attachments"][0]["content"]["body"][1]["facts"]
        self.assertEqual("6", next(fact["value"] for fact in facts if fact["title"] == "Test suites"))
        self.assertEqual("60", next(fact["value"] for fact in facts if fact["title"] == "Total test cases"))


if __name__ == "__main__":
    unittest.main()
