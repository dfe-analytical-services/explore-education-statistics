from tests.libs.setup_auth_variables import setup_auth_variables
import os
import requests 
import json 
from tests.libs.logger import get_logger


get_logger(__name__)

def setup_authentication(args, clear_existing=False):
    # Don't need BAU user if running general_public tests
    if "general_public" not in args.tests:
        setup_auth_variables(
            user="ADMIN",
            email=os.getenv("ADMIN_EMAIL"),
            password=os.getenv("ADMIN_PASSWORD"),
            clear_existing=clear_existing,
            identity_provider=os.getenv("IDENTITY_PROVIDER"),
        )

    # Don't need analyst user if running admin/bau or admin_and_public/bau tests
    if f"{os.sep}bau" not in args.tests:
        setup_auth_variables(
            user="ANALYST",
            email=os.getenv("ANALYST_EMAIL"),
            password=os.getenv("ANALYST_PASSWORD"),
            clear_existing=clear_existing,
            identity_provider=os.getenv("IDENTITY_PROVIDER"),
        )


def admin_request(method, endpoint, body=None):
    assert method and endpoint
    assert os.getenv("ADMIN_URL") is not None
    assert os.getenv("IDENTITY_LOCAL_STORAGE_ADMIN") is not None

    if method == "POST":
        assert body is not None, "POST requests require a body"

    requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
    session = requests.Session()

    # To prevent InsecureRequestWarning
    requests.packages.urllib3.disable_warnings()

    jwt_token = json.loads(os.getenv("IDENTITY_LOCAL_STORAGE_ADMIN"))["access_token"]
    headers = {
        "Content-Type": "application/json",
        "Authorization": f"Bearer {jwt_token}",
    }
    response = session.request(
        method, url=f'{os.getenv("ADMIN_URL")}{endpoint}', headers=headers, stream=True, json=body, verify=False
    )

    if response.status_code in {401, 403}:
        logger.info("Attempting re-authentication...")

        # Delete identify files and re-attempt to fetch them
        setup_authentication(clear_existing=True)
        jwt_token = json.loads(os.environ["IDENTITY_LOCAL_STORAGE_ADMIN"])["access_token"]
        response = session.request(
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

        assert response.status_code not in {401, 403}, "Failed to reauthenticate."

    assert response.status_code < 300, f"Admin request responded with {response.status_code} and {response.text}"
    return response


def get_test_themes():
    return admin_request("GET", "/api/themes")


def create_test_theme():
    return admin_request("POST", "/api/themes", {"title": "Test theme", "summary": "Test theme summary"})


def create_test_topic():
    assert os.getenv("TEST_THEME_ID") is not None

    topic_name = f'UI test topic {os.getenv("RUN_IDENTIFIER")}'
    resp = admin_request("POST", "/api/topics", {"title": topic_name, "themeId": os.getenv("TEST_THEME_ID")})

    os.environ["TEST_TOPIC_NAME"] = topic_name
    os.environ["TEST_TOPIC_ID"] = resp.json()["id"]


def delete_test_topic():
    if os.getenv("TEST_TOPIC_ID") is not None:
        admin_request("DELETE", f'/api/topics/{os.getenv("TEST_TOPIC_ID")}')
