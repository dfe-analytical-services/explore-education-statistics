import json
import os.path


def get_local_storage_json_from_browser(driver) -> dict:
    local_storage_string = driver.execute_script(
        "return JSON.stringify(Object.entries(window.localStorage)"
        ".reduce((acc, [key, value]) => { acc[key] = JSON.parse(value); return acc; }, {}))"
    )

    return json.loads(local_storage_string)


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


def get_access_token_from_local_storage_json(local_storage_json: dict) -> str:
    for entry in local_storage_json.values():
        if isinstance(entry, dict) and "credentialType" in entry and entry["credentialType"] == "AccessToken":
            return entry["secret"]

    raise RuntimeError("Unable to find access token from local storage JSON")


def get_access_token_from_file(user: str) -> str:
    local_storage_json = read_local_storage_json_from_file(user)
    return get_access_token_from_local_storage_json(local_storage_json)
