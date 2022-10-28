import json
import os
from pathlib import Path
from typing import Tuple

import requests
from scripts.get_auth_tokens import get_identity_info
from tests.libs.logger import get_logger

logger = get_logger(__name__)


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
        logger.info(f"Getting {user} authentication information from local files... ")

        os.environ[local_storage_name] = local_storage_file.read_text()
        os.environ[cookie_name] = cookie_file.read_text()

        requests.sessions.HTTPAdapter(pool_connections=50, pool_maxsize=50, max_retries=3)
        session = requests.Session()
        requests.packages.urllib3.disable_warnings()

        # Checks that the stored authentication information is actually valid.
        # If not, we want to be able to try and authenticate again.
        jwt_token = json.loads(os.environ[local_storage_name])["access_token"]
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
            logger.warn("Found invalid authentication information in local files!")

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
