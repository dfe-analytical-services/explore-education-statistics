import unittest
from unittest.mock import patch

from tests.libs.azure_pipelines import current_artifacts_url, get_pipeline_artifacts_url

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


class CurrentArtifactsUrlTests(unittest.TestCase):
    def test_builds_the_url_from_the_pipeline_variables(self):
        with patch.dict("os.environ", BUILD_ENVIRONMENT, clear=True):
            self.assertIn("buildId=123", current_artifacts_url())

    def test_returns_none_outside_azure_devops(self):
        with patch.dict("os.environ", {}, clear=True):
            self.assertIsNone(current_artifacts_url())


if __name__ == "__main__":
    unittest.main()
