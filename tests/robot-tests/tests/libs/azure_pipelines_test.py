import unittest
from unittest.mock import patch

from tests.libs.azure_pipelines import current_results_url, get_pipeline_artifacts_url

RELEASE_URL = "https://dev.azure.com/example/Education%20Statistics/_release?releaseId=6355&_a=release-summary"

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


class CurrentResultsUrlTests(unittest.TestCase):
    def test_links_to_the_build_artifacts_in_a_build_pipeline(self):
        with patch.dict("os.environ", BUILD_ENVIRONMENT, clear=True):
            self.assertIn("buildId=123", current_results_url())

    def test_links_to_the_release_in_a_release_pipeline(self):
        """
        A release deploys a build, so its BUILD_BUILDID is the application build being
        deployed and its artifacts are the application's, not this test run's.
        """
        environment = {**BUILD_ENVIRONMENT, "RELEASE_RELEASEWEBURL": RELEASE_URL}

        with patch.dict("os.environ", environment, clear=True):
            self.assertEqual(RELEASE_URL, current_results_url())

    def test_returns_none_outside_azure_devops(self):
        with patch.dict("os.environ", {}, clear=True):
            self.assertIsNone(current_results_url())


if __name__ == "__main__":
    unittest.main()
