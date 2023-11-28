import json
import os
from pathlib import Path
from typing import Tuple

import requests
from scripts.get_auth_tokens import get_identity_info
from tests.libs.logger import get_logger

# TODO - there's plenty of duplication between this file and admin_api.py and setup_auth_variables.py in tests/libs.
# Would be good to attempt to consolidate.
# There is also a web of dependencies between Python scripts in tests, tests/scripts and tests/libs. Would
# be good to consolidate these into a better structure.

logger = get_logger(__name__)

test_theme_name = "Test theme"


def send_admin_request(method, endpoint, body=None, fail_on_reauthenticate=False):
    """
    This method makes an authenticated request to the Admin API.

    If no prior authentication tokens are available when this method is called, they will
    be obtained using the BAU user's credentials.

    If authentication tokens exist already but are no longer valid, new tokens will be
    acquired and the requrest retried.

    If an error HTTP status code is encountered, an error will be thrown.
    """

    assert method and endpoint
    assert os.getenv("ADMIN_URL") is not None

    if method == "POST":
        assert body is not None, "POST requests require a body"

    requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
    session = requests.Session()

    # To prevent InsecureRequestWarning
    requests.packages.urllib3.disable_warnings()

    response = send_request_with_retry_on_auth_failure(session, method, endpoint, body, fail_on_reauthenticate)

    if response.status_code == 400 and response.text.find("SlugNotUnique") != -1:
        raise Exception(f"SlugNotUnique for {body}")
    else:
        assert response.status_code < 300, f"Admin request responded with {response.status_code} and {response.text}"
    return response


def send_authenticated_api_request(session, method, endpoint, body):
    """
    This method makes an request to the Admin API, and requires that authentication tokens
    have been fetched prior to being called.
    """

    jwt_token = json.loads(os.getenv("IDENTITY_LOCAL_STORAGE_ADMIN"))["access_token"]
    return session.request(
        method,
        url=f'{os.getenv("ADMIN_URL")}{endpoint}',
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {jwt_token}",
        },
        stream=True,
        json=body,
        verify=False,
    )


def send_request_with_retry_on_auth_failure(session, method, endpoint, body, fail_on_reauthenticate=True):
    """
    This method makes an request to the given Admin API endpoint.

    If no prior authentication tokens are available when this method is called, they will
    be obtained using the BAU user's credentials.

    If authentication tokens exist already but are no longer valid, new tokens will be
    acquired and the requrest retried.

    If an error HTTP status code is encountered, an error will be thrown.
    """

    if os.getenv("IDENTITY_LOCAL_STORAGE_ADMIN") is None:
        setup_bau_authentication(clear_existing=True)

    response = send_authenticated_api_request(session, method, endpoint, body)

    if response.status_code not in {401, 403}:
        return response

    logger.info("Attempting re-authentication...")

    # Delete identify files and re-attempt to fetch them
    setup_bau_authentication(clear_existing=True)
    response = send_authenticated_api_request(session, method, endpoint, body)

    if fail_on_reauthenticate:
        assert response.status_code not in {401, 403}, "Failed to reauthenticate."

    return response


def get_test_themes():
    return send_admin_request("GET", "/api/themes")


def create_test_theme():
    return send_admin_request("POST", "/api/themes", {"title": test_theme_name, "summary": "Test theme summary"})


def get_test_theme_id():
    get_themes_resp = get_test_themes()

    for theme in get_themes_resp.json():
        if theme["title"] == test_theme_name:
            return theme["id"]

    return None


def create_test_topic(run_id: str):
    test_theme_id = get_test_theme_id()

    if not test_theme_id:
        create_theme_resp = create_test_theme()
        test_theme_id = create_theme_resp.json()["id"]

    os.environ["TEST_THEME_NAME"] = test_theme_name
    os.environ["TEST_THEME_ID"] = test_theme_id

    topic_name = f"UI test topic {run_id}"
    response = send_admin_request("POST", "/api/topics", {"title": topic_name, "themeId": os.getenv("TEST_THEME_ID")})

    os.environ["TEST_TOPIC_NAME"] = topic_name
    os.environ["TEST_TOPIC_ID"] = response.json()["id"]


def delete_test_topic():
    if os.getenv("TEST_TOPIC_ID") is not None:
        send_admin_request("DELETE", f'/api/topics/{os.getenv("TEST_TOPIC_ID")}')


def setup_bau_authentication(clear_existing=False):
    setup_auth_variables(
        user="ADMIN",
        email=os.getenv("ADMIN_EMAIL"),
        password=os.getenv("ADMIN_PASSWORD"),
        clear_existing=clear_existing,
        identity_provider=os.getenv("IDENTITY_PROVIDER"),
    )


def setup_analyst_authentication(clear_existing=False):
    setup_auth_variables(
        user="ANALYST",
        email=os.getenv("ANALYST_EMAIL"),
        password=os.getenv("ANALYST_PASSWORD"),
        clear_existing=clear_existing,
        identity_provider=os.getenv("IDENTITY_PROVIDER"),
    )


def setup_auth_variables(
    user, email, password, identity_provider, clear_existing=False, driver=None
) -> Tuple[str, str]:
    assert user, "user param must be set"
    assert email, "email param must be set"
    assert password, "password param must be set"

    local_storage_name = f"IDENTITY_LOCAL_STORAGE_{user}"
    cookie_name = f"IDENTITY_COOKIE_{user}"

    local_storage_file = Path(f"{local_storage_name}.json")
    cookie_file = Path(f"{cookie_name}.json")

    if clear_existing:
        local_storage_file.unlink(True)
        cookie_file.unlink(True)

    admin_url = os.getenv("ADMIN_URL")
    assert admin_url, "ADMIN_URL env variable must be set"

    authenticated = False

    if local_storage_file.exists() and cookie_file.exists():
        os.environ[local_storage_name] = local_storage_file.read_text()
        os.environ[cookie_name] = cookie_file.read_text()

        response = send_admin_request("GET", "/api/permissions/access", fail_on_reauthenticate=False)

        if response.status_code == 200:
            authenticated = True
        else:
            authenticated = False
            logger.warn("Found invalid authentication information in local files! Attempting to reauthenticate.")

    if not authenticated:
        logger.info(f"Logging in to obtain {user} authentication information...")

        os.environ[local_storage_name], os.environ[cookie_name] = get_identity_info(
            url=admin_url, email=email, password=password, driver=driver, identity_provider=identity_provider
        )

        # Cache auth info to files for efficiency
        local_storage_file.write_text(os.environ[local_storage_name])
        cookie_file.write_text(os.environ[cookie_name])

        logger.info("Done!")

    local_storage_token = os.getenv(local_storage_name)
    cookie_token = os.getenv(cookie_name)

    assert local_storage_token, f"{local_storage_name} env variable was not set"
    assert cookie_token, f"{cookie_name} env variable was not set"

    return local_storage_token, cookie_token
