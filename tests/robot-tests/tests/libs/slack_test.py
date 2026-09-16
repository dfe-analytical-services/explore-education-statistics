import unittest
from pathlib import Path
from unittest.mock import MagicMock, patch

from tests.libs.slack import DEFAULT_RESULTS_DIRECTORY, SlackService
from tests.libs.ui_test_notification import UiTestNotification

UI_TESTS_CHANNEL = "C070FUXS3GC"


@patch("tests.libs.slack.WebClient")
class SlackServiceTests(unittest.TestCase):
    def setUp(self):
        self.notification = UiTestNotification(title="✅ UI tests on dev — admin", facts=(("Environment", "dev"),))
        self.environment = patch.dict("os.environ", {"SLACK_APP_TOKEN": "token"})
        self.environment.start()
        self.addCleanup(self.environment.stop)

    def _response(self, status_code: int = 200) -> MagicMock:
        return MagicMock(status_code=status_code, data={"ts": "1700000000.000100"})

    def test_posts_the_notification_to_the_ui_test_reports_channel(self, web_client_mock):
        web_client_mock.return_value.chat_postMessage.return_value = self._response()

        self.assertTrue(SlackService().send_notification(self.notification))

        web_client_mock.return_value.chat_postMessage.assert_called_once_with(
            channel=UI_TESTS_CHANNEL,
            text=self.notification.title,
            blocks=self.notification.to_slack_blocks(),
        )

    def test_returns_false_for_an_unsuccessful_response(self, web_client_mock):
        web_client_mock.return_value.chat_postMessage.return_value = self._response(status_code=500)

        self.assertFalse(SlackService().send_notification(self.notification))

    def test_does_not_upload_an_archive_when_the_notification_has_no_name(self, web_client_mock):
        web_client_mock.return_value.chat_postMessage.return_value = self._response()

        SlackService().send_notification(self.notification)

        web_client_mock.return_value.files_upload_v2.assert_not_called()

    @patch("tests.libs.slack.os.remove")
    @patch("tests.libs.slack.shutil.make_archive")
    def test_uploads_the_archive_into_the_message_thread(self, make_archive_mock, remove_mock, web_client_mock):
        web_client_mock.return_value.chat_postMessage.return_value = self._response()
        notification = UiTestNotification(title="⚠️ UI tests on dev — admin", results_archive_name="report.zip")
        results_directory = Path("some-results-directory")

        SlackService(results_directory=results_directory).send_notification(notification)

        make_archive_mock.assert_called_once_with("report", "zip", results_directory)
        web_client_mock.return_value.files_upload_v2.assert_called_once_with(
            channel=UI_TESTS_CHANNEL, file="report.zip", title="report.zip", thread_ts="1700000000.000100"
        )
        remove_mock.assert_called_once_with("report.zip")

    def test_raises_when_the_token_is_missing(self, _web_client_mock):
        with patch.dict("os.environ", {}, clear=True):
            with self.assertRaises(AssertionError):
                SlackService()


class ResultsDirectoryTests(unittest.TestCase):
    def test_results_directory_does_not_depend_on_the_working_directory(self, *_):
        """
        run_tests.py imports this module before changing directory into tests/robot-tests,
        so the results directory must be resolved from the module's own location.
        """
        self.assertTrue(DEFAULT_RESULTS_DIRECTORY.is_absolute())
        self.assertEqual("test-results", DEFAULT_RESULTS_DIRECTORY.name)
        self.assertEqual("robot-tests", DEFAULT_RESULTS_DIRECTORY.parent.name)


if __name__ == "__main__":
    unittest.main()
