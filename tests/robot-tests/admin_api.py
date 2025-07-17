import os
from datetime import datetime, timezone

import requests
from scripts.get_auth_tokens import get_local_storage_identity_json
from tests.libs import local_storage_helper
from tests.libs.logger import get_logger

# TODO - there's plenty of duplication between this file and admin_api.py and setup_auth_variables.py in tests/libs.
# Would be good to attempt to consolidate.
# There is also a web of dependencies between Python scripts in tests, tests/scripts and tests/libs. Would
# be good to consolidate these into a better structure.

logger = get_logger(__name__)

test_theme_name = "UI test theme"


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

    jwt = local_storage_helper.get_access_token_from_file("ADMIN")

    return session.request(
        method,
        url=f'{os.getenv("ADMIN_URL")}{endpoint}',
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {jwt}",
        },
        stream=True,
        json=body,
        verify=False,
    )


def send_request_with_retry_on_auth_failure(session, method, endpoint, body, fail_on_reauthenticate=True):
    """
    This method makes a request to the given Admin API endpoint.

    If no prior authentication tokens are available when this method is called, they will
    be obtained using the BAU user's credentials.

    If authentication tokens exist already but are no longer valid, new tokens will be
    acquired and the requrest retried.

    If an error HTTP status code is encountered, an error will be thrown.
    """

    if not local_storage_helper.local_storage_json_file_exists("ADMIN"):
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


def create_test_theme(run_id: str):
    test_theme_id = get_test_theme_id()

    if not test_theme_id:
        timestamp_utc_str = datetime.now(timezone.utc).strftime("%Y%m%d-%H%M%S")
        test_theme_name = f"UI test theme {timestamp_utc_str} {run_id}"
        create_theme_resp = send_admin_request(
            "POST", "/api/themes", {"title": test_theme_name, "summary": "Test theme summary"}
        )
        test_theme_id = create_theme_resp.json()["id"]

    os.environ["TEST_THEME_NAME"] = test_theme_name
    os.environ["TEST_THEME_ID"] = test_theme_id

    return create_theme_resp


def get_test_theme_id():
    get_themes_resp = get_test_themes()

    for theme in get_themes_resp.json():
        if theme["title"] == test_theme_name:
            return theme["id"]

    return None


def delete_test_theme():
    if os.getenv("TEST_THEME_ID") is not None:
        send_admin_request("DELETE", f'/api/themes/{os.getenv("TEST_THEME_ID")}')


def setup_bau_authentication(clear_existing=False) -> dict:
    return setup_auth_variables(
        user="ADMIN",
        email=os.getenv("ADMIN_EMAIL"),
        password=os.getenv("ADMIN_PASSWORD"),
        clear_existing=clear_existing,
        identity_provider=os.getenv("IDENTITY_PROVIDER"),
    )


def setup_analyst_authentication(clear_existing=False) -> dict:
    return setup_auth_variables(
        user="ANALYST",
        email=os.getenv("ANALYST_EMAIL"),
        password=os.getenv("ANALYST_PASSWORD"),
        clear_existing=clear_existing,
        identity_provider=os.getenv("IDENTITY_PROVIDER"),
    )


def setup_auth_variables(user, email, password, identity_provider, clear_existing=False, driver=None) -> dict:
    assert user, "user param must be set"
    assert email, "email param must be set"
    assert password, "password param must be set"

    if clear_existing:
        local_storage_helper.clear_local_storage_file(user)

    admin_url = os.getenv("ADMIN_URL")
    assert admin_url, "ADMIN_URL env variable must be set"

    authenticated = False

    if local_storage_helper.local_storage_json_file_exists(user):
        response = send_admin_request("GET", "/api/permissions/access", fail_on_reauthenticate=False)

        if response.status_code == 200:
            authenticated = True
        else:
            authenticated = False
            logger.warning("Found invalid authentication information in local files! Attempting to reauthenticate.")

    if not authenticated:
        logger.info(f"Logging in to obtain {user} authentication information...")
        local_storage_json = get_local_storage_identity_json(
            url=admin_url, email=email, password=password, driver=driver, identity_provider=identity_provider
        )
        local_storage_helper.write_local_storage_json_to_file(local_storage_json, user)
        logger.info("Done!")

    return local_storage_helper.read_local_storage_json_from_file(user)
