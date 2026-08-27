import os
from typing import Any

import requests

# To prevent InsecureRequestWarning
requests.packages.urllib3.disable_warnings()


class PublicApiClient:
    ROBOT_AUTO_KEYWORDS = False

    @staticmethod
    def __request(method: str, url: str, params: dict[str, Any] | None = None):
        assert method and url
        assert os.getenv("PUBLIC_API_URL") is not None

        requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
        session = requests.Session()

        return session.request(
            method,
            url=f'{os.getenv("PUBLIC_API_URL")}{url}',
            params=params,
            headers={"Content-Type": "application/json"},
            stream=True,
            verify=False,
        )

    def get(self, url: str, params: dict[str, Any] | None = None):
        return self.__request("GET", url, params)


public_api_client = PublicApiClient()


def user_gets_api_data_set_meta_via_api(data_set_id: str) -> dict[str, Any]:
    assert data_set_id

    resp = public_api_client.get(f"/data-sets/{data_set_id}/meta")

    assert (
        resp.status_code == 200
    ), f"Could not get API data set meta! Responded with {resp.status_code} and {resp.text}"

    return resp.json()
