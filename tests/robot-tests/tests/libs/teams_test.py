import unittest
from unittest.mock import MagicMock, patch
from urllib.error import HTTPError, URLError

from tests.libs.teams import TeamsService


class TeamsServiceTests(unittest.TestCase):
    def setUp(self):
        self.card = {"type": "message"}

    @patch("tests.libs.teams.urlopen")
    def test_returns_true_for_successful_webhook_response(self, urlopen_mock):
        response = MagicMock()
        response.status = 200
        urlopen_mock.return_value.__enter__.return_value = response

        result = TeamsService("https://example.com/webhook").send_test_report(self.card)

        self.assertTrue(result)
        request = urlopen_mock.call_args.args[0]
        self.assertEqual("POST", request.method)
        self.assertEqual("application/json", request.headers["Content-type"])

    @patch("tests.libs.teams.urlopen", side_effect=TimeoutError("timed out"))
    def test_returns_false_for_webhook_timeout(self, _):
        self.assertFalse(TeamsService("https://example.com/webhook").send_test_report(self.card))

    @patch("tests.libs.teams.urlopen", side_effect=URLError("unavailable"))
    def test_returns_false_for_webhook_connection_error(self, _):
        self.assertFalse(TeamsService("https://example.com/webhook").send_test_report(self.card))

    @patch("tests.libs.teams.urlopen")
    def test_returns_false_for_webhook_http_error(self, urlopen_mock):
        urlopen_mock.side_effect = HTTPError("https://example.com", 500, "failure", {}, None)

        self.assertFalse(TeamsService("https://example.com/webhook").send_test_report(self.card))

    def test_returns_false_for_malformed_webhook_url(self):
        self.assertFalse(TeamsService("://bad-url").send_test_report(self.card))

    def test_returns_false_when_webhook_is_missing(self):
        with patch.dict("os.environ", {}, clear=True):
            self.assertFalse(TeamsService().send_test_report(self.card))

    def test_uses_the_webhook_url_from_the_environment(self):
        with patch.dict("os.environ", {"TEAMS_UI_TESTS_WEBHOOK_URL": "https://example.com/webhook"}, clear=True):
            self.assertEqual("https://example.com/webhook", TeamsService().webhook_url)


if __name__ == "__main__":
    unittest.main()
