import unittest
from unittest.mock import patch

from tests.libs.azure_pipelines import current_results_url, get_pipeline_artifacts_url, get_release_url

RELEASE_WEB_URL = "https://dev.azure.com/example/project/_release?releaseId=6355&_a=release-summary"

BUILD_ENVIRONMENT = {
    "SYSTEM_TEAMFOUNDATIONCOLLECTIONURI": "https://dev.azure.com/example/",
    "SYSTEM_TEAMPROJECT": "Education Statistics",
    "BUILD_BUILDID": "123",
}


class GetPipelineArtifactsUrlTests(unittest.TestCase):
    def test_encodes_the_project_and_build_id(self):
        url = get_pipeline_artifacts_url("https://dev.azure.com/example/", "Education Statistics", "123")

        self.assertEqual(
            "https://dev.azure.com/example/Education%20Statistics/_build/results"
            "?buildId=123&view=artifacts&pathAsName=false&type=publishedArtifacts",
            url,
        )

    def test_returns_none_when_any_part_is_missing(self):
        self.assertIsNone(get_pipeline_artifacts_url("", "Education Statistics", "123"))
        self.assertIsNone(get_pipeline_artifacts_url("https://dev.azure.com/example/", "", "123"))
        self.assertIsNone(get_pipeline_artifacts_url("https://dev.azure.com/example/", "Education Statistics", ""))


class GetReleaseUrlTests(unittest.TestCase):
    def test_encodes_the_project_and_release_id(self):
        url = get_release_url("https://dev.azure.com/example/", "Education Statistics", "6355")

        self.assertEqual(
            "https://dev.azure.com/example/Education%20Statistics/_release" "?releaseId=6355&_a=release-summary",
            url,
        )

    def test_returns_none_when_any_part_is_missing(self):
        self.assertIsNone(get_release_url("", "Education Statistics", "6355"))
        self.assertIsNone(get_release_url("https://dev.azure.com/example/", "Education Statistics", ""))


class CurrentResultsUrlTests(unittest.TestCase):
    def test_links_to_the_build_artifacts_in_a_build_pipeline(self):
        with patch.dict("os.environ", BUILD_ENVIRONMENT, clear=True):
            self.assertIn("buildId=123", current_results_url())

    def test_prefers_the_release_url_that_azure_devops_provides(self):
        """
        A release deploys a build, so its BUILD_BUILDID is the application build being
        deployed and its artifacts are the application's, not this test run's.
        """
        environment = {**BUILD_ENVIRONMENT, "RELEASE_RELEASEWEBURL": RELEASE_WEB_URL}

        with patch.dict("os.environ", environment, clear=True):
            self.assertEqual(RELEASE_WEB_URL, current_results_url())

    def test_builds_the_release_url_when_only_the_release_id_is_exposed(self):
        environment = {**BUILD_ENVIRONMENT, "RELEASE_RELEASEID": "6355"}

        with patch.dict("os.environ", environment, clear=True):
            url = current_results_url()

        self.assertIn("releaseId=6355", url)
        self.assertNotIn("buildId", url)

    def test_reports_the_release_variables_it_found_when_it_cannot_build_a_release_url(self):
        """
        A release that falls through to the build artifacts was not recognised as one, so
        name the variables it did have to show which to use instead.
        """
        environment = {**BUILD_ENVIRONMENT, "RELEASE_DEPLOYMENTID": "42"}

        with patch.dict("os.environ", environment, clear=True):
            with self.assertLogs("tests.libs.azure_pipelines", level="WARNING") as logs:
                current_results_url()

        self.assertIn("RELEASE_DEPLOYMENTID", logs.records[0].getMessage())

    def test_returns_none_outside_azure_devops(self):
        with patch.dict("os.environ", {}, clear=True):
            self.assertIsNone(current_results_url())


if __name__ == "__main__":
    unittest.main()
