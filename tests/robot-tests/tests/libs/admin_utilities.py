import json
import os
import time

import requests
from robot.libraries.BuiltIn import BuiltIn
from selenium.common import NoSuchElementException
from selenium.webdriver.common.by import By
from tests.libs.logger import get_logger
from tests.libs.setup_auth_variables import setup_auth_variables
from tests.libs.utilities import set_cookie_from_json, set_to_local_storage

sl = BuiltIn().get_library_instance("SeleniumLibrary")

logger = get_logger(__name__)


def raise_assertion_error(err_msg):
    sl.failure_occurred()
    raise AssertionError(err_msg)


def user_signs_in_as(user: str):
    try:
        (local_storage_token, cookie_token) = setup_auth_variables(
            user,
            email=os.getenv(f"{user}_EMAIL"),
            password=os.getenv(f"{user}_PASSWORD"),
            driver=sl.driver,
            identity_provider=os.getenv("IDENTITY_PROVIDER"),
        )

        admin_url = os.getenv("ADMIN_URL")
        assert admin_url

        set_to_local_storage(
            f"GovUk.Education.ExploreEducationStatistics.Adminuser:{admin_url}:GovUk.Education"
            f".ExploreEducationStatistics.Admin",
            local_storage_token,
        )
        set_cookie_from_json(cookie_token)

        sl.go_to(admin_url)
    except Exception as e:
        raise_assertion_error(e)


def get_theme_id_from_url():
    url = sl.get_location()
    assert "/themes/" in url, "URL does not contain /themes"
    result = url[len(os.getenv("ADMIN_URL")) :].lstrip("/").split("/")
    assert result[0] == "themes", 'String "themes" should be 1st element in list'
    return result[1]


def get_release_guid_from_release_status_page_url(url):
    assert url.endswith("/status")
    url_components = url.split("/")
    return url_components[-2]


def data_csv_number_contains_xpath(num, xpath):
    try:
        elem = sl.driver.find_element(By.XPATH, f'//*[@id="dataFileUploadForm"]/dl[{num}]')
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num}"')
    try:
        elem.find_element(By.XPATH, xpath)
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num} with xpath {xpath}')


def data_file_number_contains_xpath(num, xpath):
    try:
        elem = sl.driver.find_element(By.XPATH, f'//*[@id="fileUploadForm"]/dl[{num}]')
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num}"')
    try:
        elem.find_element(By.XPATH, xpath)
    except BaseException:
        raise_assertion_error(f'Cannot find data file number "{num} with xpath {xpath}')


def user_waits_for_release_process_status_to_be(status, timeout):
    max_time = time.time() + int(timeout)
    while time.time() < max_time:
        try:
            sl.driver.find_element(By.ID, f"release-process-status-Failed")
            raise_assertion_error("Release process status FAILED!")
        except BaseException:
            pass
        try:
            sl.driver.find_element(By.ID, f"release-process-status-{status}")
            return
        except BaseException:
            sl.reload_page()  # Necessary if release previously scheduled
            time.sleep(3)
    raise_assertion_error(f"Release process status wasn't {status} after {timeout} seconds!")


def user_checks_dashboard_theme_topic_dropdowns_exist():
    try:
        sl.driver.find_element(By.CSS_SELECTOR, "#publicationsReleases-themeTopic-themeId")
        sl.driver.find_element(By.CSS_SELECTOR, "#publicationsReleases-themeTopic-topicId")
    except NoSuchElementException:
        return False

    return True


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
