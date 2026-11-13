import base64
import datetime
import json
import os.path
import re
import time
import urllib.parse

from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.ciphers.aead import AESGCM
from cryptography.hazmat.primitives.kdf.hkdf import HKDF

MSAL_TOKEN_KEYS_PATTERN = r"^msal\.\d+\.token\.keys\.(.+)$"


def wait_until_access_token_in_browser_local_storage(driver, timeout_seconds: int = 30):
    # MSAL can write the access token to local storage after the Dashboard has rendered.
    # The token itself is encrypted, but MSAL's index of token keys ("msal.<schema>.token.keys.<clientId>")
    # isn't, so wait until that lists an access token before capturing local storage.
    max_time = time.time() + timeout_seconds
    while time.time() < max_time:
        has_access_token = driver.execute_script(
            """
            const pattern = new RegExp(arguments[0]);
            return Object.keys(window.localStorage)
                .filter(key => pattern.test(key))
                .some(key => {
                    try {
                        return JSON.parse(window.localStorage.getItem(key)).accessToken?.length > 0;
                    } catch (e) {
                        return false;
                    }
                });
            """,
            MSAL_TOKEN_KEYS_PATTERN,
        )
        if has_access_token:
            return
        time.sleep(0.5)
    raise RuntimeError(f"Timed out after {timeout_seconds}s waiting for an MSAL access token in browser local storage")


def get_local_storage_json_from_browser(driver) -> dict:
    wait_until_access_token_in_browser_local_storage(driver)

    local_storage_string = driver.execute_script("""
         return JSON.stringify(Object.entries(window.localStorage).reduce((acc, [key, value]) => {
             try {
                 acc[key] = JSON.parse(value);
             } catch (e) {
                 acc[key] = value;
             }
             return acc;
         }, {}));
         """)

    local_storage_json = json.loads(local_storage_string)
    encryption_cookie = driver.get_cookie("msal.cache.encryption")
    expires = datetime.datetime.now() + datetime.timedelta(minutes=15)
    local_storage_json["ROBOT_ENCRYPTION_COOKIE_VALUE"] = encryption_cookie["value"]
    local_storage_json["ROBOT_ENCRYPTION_COOKIE_VALUE_EXPIRES"] = expires.isoformat()

    return local_storage_json


def clear_browser_local_storage(driver):
    driver.execute_script("""
        window.localStorage.clear();
    """)


def get_local_storage_filename_for_user(user: str) -> str:
    local_storage_name = f"IDENTITY_LOCAL_STORAGE_{user}"
    return f"{local_storage_name}.json"


def local_storage_json_file_exists(user: str) -> bool:
    return os.path.exists(get_local_storage_filename_for_user(user))


def read_local_storage_json_from_file(user: str) -> dict:
    local_storage_filename = get_local_storage_filename_for_user(user)

    if not local_storage_json_file_exists(user):
        raise f"Unable to find local storage file {local_storage_filename}"

    with open(local_storage_filename, "r") as local_storage_file:
        return json.loads(local_storage_file.read())


def write_to_local_storage(driver, key: str, value: str):
    driver.execute_script(f"localStorage.setItem('{key}', '{value}');")


def write_local_storage_json_to_local_storage(local_storage_json: dict, driver):
    for key in local_storage_json.keys():
        write_to_local_storage(driver, key, json.dumps(local_storage_json[key]))


def write_local_storage_json_to_file(local_storage_json: dict, user: str) -> dict:
    local_storage_filename = get_local_storage_filename_for_user(user)

    with open(local_storage_filename, "w") as local_storage_file:
        local_storage_file.write(json.dumps(local_storage_json))


def clear_local_storage_file(user: str):
    local_storage_filename = get_local_storage_filename_for_user(user)

    if os.path.exists(local_storage_filename):
        os.remove(local_storage_filename)


def _base64url_decode(value: str) -> bytes:
    return base64.urlsafe_b64decode(value + "=" * (-len(value) % 4))


def _get_msal_token_keys(local_storage_json: dict) -> tuple[str, dict]:
    # MSAL indexes its token keys under "msal.<schema>.token.keys.<clientId>"
    for key, value in local_storage_json.items():
        match = re.match(MSAL_TOKEN_KEYS_PATTERN, key)
        if match:
            return match.group(1), value

    raise RuntimeError("Unable to find MSAL token keys in local storage JSON")


def _decrypt_msal_cache_entry(key: str, entry: dict, base_key: bytes, client_id: str) -> dict:
    """
    Mirrors msal-browser's (v4+) LocalStorage decryption: an AES-256-GCM key is derived per
    entry using HKDF-SHA256 with the entry's nonce as salt and the client id as context.
    """
    context = client_id if client_id in key else ""
    derived_key = HKDF(
        algorithm=hashes.SHA256(),
        length=32,
        salt=_base64url_decode(entry["nonce"]),
        info=context.encode(),
    ).derive(base_key)

    plaintext = AESGCM(derived_key).decrypt(bytes(12), _base64url_decode(entry["data"]), None)
    return json.loads(plaintext)


def get_access_token_from_local_storage_json(local_storage_json: dict) -> str:
    encryption_cookie = local_storage_json.get("ROBOT_ENCRYPTION_COOKIE_VALUE")

    if not encryption_cookie:
        raise RuntimeError("Unable to find MSAL encryption cookie in local storage JSON")

    cookie = json.loads(urllib.parse.unquote(encryption_cookie))
    base_key = _base64url_decode(cookie["key"])
    client_id, token_keys = _get_msal_token_keys(local_storage_json)

    for key in token_keys.get("accessToken", []):
        entry = local_storage_json.get(key)

        # Entries encrypted with a different key are from a previous session
        if entry and entry.get("id") == cookie["id"]:
            return _decrypt_msal_cache_entry(key, entry, base_key, client_id)["secret"]

    raise RuntimeError("Unable to find access token from local storage JSON")


def get_access_token_from_file(user: str) -> str:
    local_storage_json = read_local_storage_json_from_file(user)
    return get_access_token_from_local_storage_json(local_storage_json)


def has_encryption_cookie_expired(usr: str) -> bool:
    local_storage_json = read_local_storage_json_from_file(usr)
    expires = local_storage_json.get("ROBOT_ENCRYPTION_COOKIE_VALUE_EXPIRES")
    if expires:
        return datetime.datetime.now() > datetime.datetime.fromisoformat(expires)
    return True


def write_auth_encryption_to_cookie(local_storage_json: dict, driver):
    encryption_cookie = local_storage_json.get("ROBOT_ENCRYPTION_COOKIE_VALUE")

    driver.add_cookie({"name": "msal.cache.encryption", "value": encryption_cookie})
