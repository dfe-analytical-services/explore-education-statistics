import json
import os
from datetime import datetime

import requests
from robot.libraries.BuiltIn import BuiltIn

# To prevent InsecureRequestWarning
requests.packages.urllib3.disable_warnings()


class AdminClient:
    ROBOT_AUTO_KEYWORDS = False

    @staticmethod
    def __request(method: str, url: str, body: object = None):
        assert method and url
        assert os.getenv("IDENTITY_LOCAL_STORAGE_ADMIN") is not None
        assert os.getenv("ADMIN_URL") is not None

        requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
        session = requests.Session()

        jwt_token = json.loads(os.getenv("IDENTITY_LOCAL_STORAGE_ADMIN"))["access_token"]
        headers = {
            "Content-Type": "application/json",
            "Authorization": f"Bearer {jwt_token}",
        }

        return session.request(
            method, url=f'{os.getenv("ADMIN_URL")}{url}', headers=headers, stream=True, json=body, verify=False
        )

    def get(self, url: str):
        return self.__request("GET", url)

    def post(self, url: str, body: object = None):
        return self.__request("POST", url, body)

    def put(self, url: str, body: object = None):
        return self.__request("PUT", url, body)

    def patch(self, url: str, body: object = None):
        return self.__request("PATCH", url, body)

    def delete(self, url: str):
        return self.__request("DELETE", url)


admin_client = AdminClient()


def user_creates_theme_via_api(title: str, summary: str = "") -> str:
    assert title

    resp = admin_client.post(f"/api/themes", {"title": title, "summary": summary})

    assert resp.status_code == 200, f"Could not create theme! Responded with {resp.status_code} and {resp.text}"

    return resp.json()["id"]


def user_deletes_theme_via_api(theme_id: str):
    assert theme_id

    resp = admin_client.delete(f"/api/themes/{theme_id}")

    assert resp.status_code == 204, f"Could not delete theme! Responded with {resp.status_code} and {resp.text}"


def user_creates_topic_via_api(title: str, theme_id: str) -> str:
    assert title

    resp = admin_client.post(f"/api/topics", {"title": title, "themeId": theme_id})

    assert resp.status_code == 200, f"Could not create topic! Responded with {resp.status_code} and {resp.text}"

    return resp.json()["id"]


def user_creates_test_publication_via_api(publication_name: str, topic_id: str = None):
    if topic_id is not None:
        chosen_topic_id = topic_id
    else:
        assert os.getenv("TEST_TOPIC_ID") is not None
        chosen_topic_id = os.getenv("TEST_TOPIC_ID")

    response = admin_client.post(
        "/api/publications",
        {
            "title": publication_name,
            "summary": f"{publication_name} summary",
            "topicId": chosen_topic_id,
            "contact": {
                "contactName": "UI test contact name",
                "contactTelNo": "1234 1234",
                "teamEmail": "ui_test@test.com",
                "teamName": "UI test team name",
            },
        },
    )
    assert (
        response.status_code < 300
    ), f"Creating publication API request failed with {response.status_code} and {response.text}"

    return response.json()["id"]


def user_adds_user_invite_via_api(user_email: str, role_name: str, created_date: str = None):
    response = admin_client.post(
        f"/api/user-management/invites",
        {
            "email": user_email,
            "roleId": _get_global_role_id(role_name),
            "createdDate": created_date or datetime.today().strftime("%Y-%m-%d %H:%M:%S"),
        },
    )
    assert (
        response.status_code < 300
    ), f"Adding release role to user API request failed with {response.status_code} and {response.text}"


def user_adds_release_role_to_user_via_api(user_email: str, release_id: str, role_name: str = None):
    user_id = _get_user_details_via_api(user_email)["id"]

    response = admin_client.post(
        f"/api/user-management/users/{user_id}/release-role",
        {
            "releaseId": release_id,
            "releaseRole": role_name,
        },
    )
    assert (
        response.status_code < 300
    ), f"Adding release role to user API request failed with {response.status_code} and {response.text}"


def user_adds_publication_role_to_user_via_api(user_email: str, publication_id: str, role_name: str = None):
    user_id = _get_user_details_via_api(user_email)["id"]

    response = admin_client.post(
        f"/api/user-management/users/{user_id}/publication-role",
        {
            "publicationId": publication_id,
            "publicationRole": role_name,
        },
    )
    assert (
        response.status_code < 300
    ), f"Adding publication role to user API request failed with {response.status_code} and {response.text}"


def user_removes_all_release_and_publication_roles_from_user(user_id: str) -> None:
    response = admin_client.delete(f"/api/user-management/user/{user_id}/resource-roles/all")
    assert (
        response.status_code < 300
    ), f"Removing release role from user API request failed with {response.status_code} and {response.text}"


def user_resets_user_roles_via_api_if_required(user_emails: list) -> None:
    allowed_users = [
        "ees-prerelease1@education.gov.uk",
        "ees-prerelease2@education.gov.uk",
    ]

    for user_email in user_emails:
        if user_email not in allowed_users:
            raise AssertionError(f"`User emails` must contain only allowed users: {allowed_users}")
        try:
            user_ids = [_get_prerelease_user_details_via_api(user_email)["id"]]
            _ = [user_removes_all_release_and_publication_roles_from_user(user_id) for user_id in user_ids]
            BuiltIn().log(f"All userReleaseRoles & userPublicationRoles reset for user: {user_email}")
        except TypeError or IndexError:
            BuiltIn().log(f"User with email {user_email} does not exist in pre-release user list", "WARN")

            try:
                user_ids = [_get_user_details_via_api(user_email)["id"]]
                _ = [user_removes_all_release_and_publication_roles_from_user(user_id) for user_id in user_ids]
                BuiltIn().log(f"All userReleaseRoles & userPublicationRoles reset for user: {user_email}")
            except TypeError or IndexError:
                BuiltIn().log(f"User with email {user_email} does not exist in user list", "WARN")


def user_creates_test_release_via_api(
    publication_id: str, time_period: str, year: str, type: str = "NationalStatistics"
):
    response = admin_client.post(
        f"/api/publications/{publication_id}/releases",
        {
            "publicationId": publication_id,
            "timePeriodCoverage": {
                "value": time_period,
            },
            "year": int(year),
            "type": type,
            "templateReleaseId": "",
        },
    )
    assert (
        response.status_code < 300
    ), f"Creating release API request failed with {response.status_code} and {response.text}"

    return response.json()["id"]


def user_updates_release_published_date_via_api(release_id: str, published: datetime) -> None:
    response = admin_client.patch(f"/api/releases/{release_id}/published", {"published": published.isoformat()})
    assert (
        response.status_code < 300
    ), f"Updating release published date failed with {response.status_code} and {response.text}"


def _get_user_details_via_api(user_email: str):
    users = admin_client.get("/api/user-management/users").json()

    matching_users = list(filter(lambda user: user["email"] == user_email, users))

    assert matching_users, f"Could not find user with email {user_email}"

    return matching_users[0]


def _get_prerelease_user_details_via_api(user_email: str):
    users = admin_client.get("/api/user-management/pre-release").json()

    matching_users = list(filter(lambda user: user["email"] == user_email, users))

    assert matching_users, f"Could not find user with email {user_email}"

    return matching_users[0]


def _get_global_role_id(role_name: str):
    response = admin_client.get("/api/user-management/roles")
    assert (
        response.status_code < 300
    ), f"Getting list of global roles failed with {response.status_code} and {response.text}"

    roles = response.json()
    matching_roles = list(filter(lambda role: role["name"] == role_name, roles))

    assert matching_roles, f"Could not find global role matching name {role_name}"

    return matching_roles[0]["id"]


def _get_user_invite(user_email: str):
    response = admin_client.get("/api/user-management/invites")
    assert response.status_code < 300, f"Getting list of invites failed with {response.status_code} and {response.text}"

    invites = response.json()
    matching_invites = list(filter(lambda invite: invite["email"] == user_email, invites))
    return next(iter(matching_invites), None)
