import os

import requests
from scripts.get_auth_tokens import get_local_storage_identity_json
from tests.libs import local_storage_helper
from tests.libs.logger import get_logger

logger = get_logger(__name__)


# TODO - this method is nearly line-for-line the same as robot-tests/admin_api.py setup_auth_variables().
# Can one of them go?
def setup_auth_variables(user, email, password, identity_provider, clear_existing=False, driver=None) -> str:
    assert user, "user param must be set"
    assert email, "email param must be set"
    assert password, "password param must be set"

    if clear_existing:
        local_storage_helper.clear_local_storage_file(user)

    admin_url = os.getenv("ADMIN_URL")
    assert admin_url, "ADMIN_URL env variable must be set"

    authenticated = False

    if local_storage_helper.local_storage_json_file_exists(user):
        logger.info(f"Getting {user} authentication information from local files... ")

        requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
        session = requests.Session()
        requests.packages.urllib3.disable_warnings()

        # Checks that the stored authentication information is actually valid.
        # If not, we want to be able to try and authenticate again.
        jwt_token = local_storage_helper.get_access_token_from_file(user)
        response = session.request(
            "GET",
            url=f'{os.getenv("ADMIN_URL")}/api/permissions/access',
            headers={
                "Content-Type": "application/json",
                "Authorization": f"Bearer {jwt_token}",
            },
            stream=True,
            verify=False,
        )

        if response.status_code == 200:
            authenticated = True
        else:
            authenticated = False
            logger.warning("Found invalid authentication information in local files!")

    if not authenticated:
        logger.info(f"Logging in to obtain {user} authentication information...")
        local_storage_json = get_local_storage_identity_json(
            url=admin_url, email=email, password=password, driver=driver, identity_provider=identity_provider
        )
        local_storage_helper.write_local_storage_json_to_file(local_storage_json, user)
        logger.info("Done!")

    return local_storage_helper.read_local_storage_json_from_file(user)
