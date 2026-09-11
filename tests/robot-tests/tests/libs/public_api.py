import json
import os
from typing import Any

import requests

# To prevent InsecureRequestWarning
requests.packages.urllib3.disable_warnings()


class PublicApiClient:
    ROBOT_AUTO_KEYWORDS = False

    @staticmethod
    def __request(
        method: str,
        url: str,
        params: dict[str, Any] | None = None,
        body: dict[str, Any] | None = None,
        preview_token: str | None = None,
    ):
        assert method and url
        assert os.getenv("PUBLIC_API_URL") is not None

        headers = {"Content-Type": "application/json"}
        if preview_token:
            headers["Preview-Token"] = preview_token

        requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
        session = requests.Session()

        return session.request(
            method,
            url=f'{os.getenv("PUBLIC_API_URL")}{url}',
            params=params,
            json=body,
            headers=headers,
            stream=True,
            verify=False,
        )

    def get(self, url: str, params: dict[str, Any] | None = None, preview_token: str | None = None):
        return self.__request("GET", url, params=params, preview_token=preview_token)

    def post(
        self,
        url: str,
        body: dict[str, Any] | None = None,
        params: dict[str, Any] | None = None,
        preview_token: str | None = None,
    ):
        return self.__request("POST", url, params=params, body=body, preview_token=preview_token)


public_api_client = PublicApiClient()


def user_makes_get_request_to_public_api(
    url: str, params: dict[str, Any] | None = None, preview_token: str | None = None
):
    return public_api_client.get(url, params=params, preview_token=preview_token)


def user_makes_post_request_to_public_api(
    url: str,
    body: str | dict[str, Any] | None = None,
    params: dict[str, Any] | None = None,
    preview_token: str | None = None,
):
    if isinstance(body, str):
        body = json.loads(body)

    return public_api_client.post(url, body=body, params=params, preview_token=preview_token)


def user_gets_api_data_set_meta_via_api(data_set_id: str) -> dict[str, Any]:
    assert data_set_id

    resp = public_api_client.get(f"/data-sets/{data_set_id}/meta")

    assert (
        resp.status_code == 200
    ), f"Could not get API data set meta! Responded with {resp.status_code} and {resp.text}"

    return resp.json()
