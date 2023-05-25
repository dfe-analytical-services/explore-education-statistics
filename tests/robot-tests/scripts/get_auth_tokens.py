import argparse
import json
import time
import traceback
from typing import Union

from selenium import webdriver
from selenium.webdriver.common.by import By
from selenium.webdriver.common.desired_capabilities import DesiredCapabilities

from .get_webdriver import get_webdriver


def wait_until_page_contains_xpath(context, selector):
    timeout = 10
    max_time = time.time() + timeout
    elem = None
    while time.time() < max_time:
        try:
            elem = context.find_element(By.XPATH, selector)
        except BaseException:
            pass
        if elem is not None:
            return
    raise Exception(f"Timeout! Couldn't find element with xpath selector '{selector}'")


def login_with_keycloak(url, email, password, driver):
    try:
        driver.get(url)
        try:
            wait_until_page_contains_xpath(driver, '//button[contains(text(), "Sign in")]')
            driver.find_element(By.XPATH, '//button[contains(text(), "Sign in")]').click()
            time.sleep(1)
        except BaseException:
            raise AssertionError("Sign in page didn't appear")

        driver.find_element(By.ID, "username").send_keys(email)
        driver.find_element(By.ID, "password").send_keys(password)
        driver.find_element(By.ID, "kc-login").click()
        try:
            wait_until_page_contains_xpath(driver, '//h1[text()="Dashboard"]')  # Should be Admin dashboard for user
        except BaseException:
            raise AssertionError(
                f"Couldn't find '//h1[text()=\"Dashboard\"]' on page. Incorrect user details used? Found page source: \n{driver.page_source}"
            )

    except BaseException:
        raise AssertionError(f"Couldn't login with keycloak. Error: {traceback.format_exc()}")

    return get_local_storage_json(driver, url), get_identity_cookie(driver)


def login_with_azure(url, email, password, first_name, last_name, driver):
    driver.get(url)

    wait_until_page_contains_xpath(driver, '//button[contains(text(), "Sign in")]')
    driver.find_element(By.XPATH, '//button[contains(text(), "Sign in")]').click()

    try:
        wait_until_page_contains_xpath(driver, '//div[text()="Sign in"]')
        time.sleep(1)
    except BaseException:
        raise AssertionError("Sign in page didn't appear?")

    try:
        driver.find_element(By.XPATH, '//input[@type="email"]').send_keys(email)
        time.sleep(1)
        driver.find_element(By.XPATH, '//input[@value="Next"]').click()

        wait_until_page_contains_xpath(driver, '//div[text()="Enter password"]')
        time.sleep(1)

    except BaseException:
        raise AssertionError("Error when entering/submitting email!")

    try:
        driver.find_element(By.XPATH, '//input[@type="password"]').send_keys(password)
        time.sleep(1)
        driver.find_element(By.XPATH, '//input[@value="Sign in"]').click()

        wait_until_page_contains_xpath(driver, '//div[text()="Stay signed in?"]')
        wait_until_page_contains_xpath(driver, '//input[@value="No"]')
    except BaseException:
        raise AssertionError("Error when entering/submitting password")

    time.sleep(1)
    driver.find_element(By.XPATH, '//input[@value="No"]').click()

    # Register user if necessary
    try:
        wait_until_page_contains_xpath(driver, '//span[contains(text(),"Register")]')
        driver.find_element(By.CSS_SELECTOR, "#Input_FirstName").clear()
        driver.find_element(By.CSS_SELECTOR, "#Input_FirstName").send_keys(first_name)
        driver.find_element(By.CSS_SELECTOR, "#Input_LastName").clear()
        driver.find_element(By.CSS_SELECTOR, "#Input_LastName").send_keys(last_name)
        driver.find_element(By.CSS_SELECTOR, "#Input_Email").clear()
        driver.find_element(By.CSS_SELECTOR, "#Input_Email").send_keys(email)
        driver.find_element(By.XPATH, '//button[contains(text(), "Register")]').click()
    except Exception:
        pass

    try:
        wait_until_page_contains_xpath(driver, '//h1[text()="Dashboard"]')  # Should be Admin dashboard for user
    except BaseException:
        raise AssertionError(
            f"Couldn't find '//h1[text()=\"Dashboard\"]' on page. Incorrect user details used? Found page source: \n{driver.page_source}"
        )

    return get_local_storage_json(driver, url), get_identity_cookie(driver)


def get_local_storage_json(driver, url) -> str:
    local_storage_json = driver.execute_script(
        f"return window.localStorage.getItem('GovUk.Education.ExploreEducationStatistics.Adminuser:{url}:GovUk.Education.ExploreEducationStatistics.Admin')"
    )
    assert (
        local_storage_json is not None
    ), f"Couldn't find 'GovUk.Education.ExploreEducationStatistics.Adminuser:{url}:GovUk.Education.ExploreEducationStatistics.Admin' in Local Storage!"
    return local_storage_json


def get_identity_cookie(driver) -> str:
    identity_cookie_dict = driver.get_cookie(".AspNetCore.Identity.Application")
    assert identity_cookie_dict is not None, "Couldn't get cookie '.AspNetCore.Identity.Application'"
    identity_cookie = json.dumps(identity_cookie_dict)

    return identity_cookie


def get_identity_info(
    identity_provider, url, email, password, first_name="Bau1", last_name="EESADMIN", driver=None
) -> Union[str, str]:

    using_existing_driver = driver is not None

    if not driver:
        get_webdriver("latest")

    chrome_options = webdriver.ChromeOptions()
    chrome_options.add_argument("--no-sandbox")
    chrome_options.add_argument("--headless")
    chrome_options.add_argument("--disable-gpu")
    chrome_options.add_argument("--ignore-certificate-errors")
    chrome_options.add_argument("--disable-logging")
    chrome_options.add_argument("--log-level=\3")
    driver = webdriver.Chrome(options=chrome_options, desired_capabilities=DesiredCapabilities.CHROME)

    if identity_provider == "KEYCLOAK":
        try:
            return login_with_keycloak(url, email, password, driver)
        finally:
            if not using_existing_driver:
                driver.close()

    elif identity_provider == "AZURE":
        try:
            return login_with_azure(url, email, password, first_name, last_name, driver)
        finally:
            if not using_existing_driver:
                driver.close()
    else:
        raise AssertionError(f"Unknown identity provider: {identity_provider}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(
        prog="python get_auth_tokens.py", description="To get the authentication tokens for using the admin service"
    )
    parser.add_argument(dest="url", help="URL of environment you wish to get authentication tokens for")
    parser.add_argument(dest="email", help="Email address to login using")
    parser.add_argument(dest="password", help="Password to login with")
    parser.add_argument(dest="identity_provider", help="Identity provider to use"),
    args = parser.parse_args()

    (local_storage_token, _) = get_identity_info(args.url, args.email, args.password, args.identity_provider)
    jwt = json.loads(local_storage_token)["access_token"]
    assert jwt is not None, "Couldn't get 'access_token' from local storage!"

    f = open("jwt", "w")
    f.write(jwt)
    f.close()
