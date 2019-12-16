import os
import time
import argparse
import json
from selenium import webdriver
import chromedriver_install as cdi

def wait_until_page_contains_xpath(context, selector):
    timeout = 10
    max_time = time.time() + timeout
    elem = None
    while time.time() < max_time:
        try:
            elem = context.find_element_by_xpath(selector)
        except:
            pass
        if elem is not None:
            return
    raise Exception(f"Timeout! Couldn't find element with xpath selector '{selector}'")

def get_identity_info(url, email, password, chromedriver_version='78.0.3904.70'):
    cdi.install(file_directory="./webdriver/",
                verbose=False,
                chmod=True,
                overwrite=False,
                version=chromedriver_version)
    os.environ["PATH"] += os.pathsep + os.getcwd() + os.sep + 'webdriver'

    chrome_options = webdriver.ChromeOptions()
    chrome_options.add_argument('--no-sandbox')
    if __name__ == '__main__':
        chrome_options.add_argument('--headless')
    chrome_options.add_argument('--disable-gpu')
    driver = webdriver.Chrome(options=chrome_options)
    driver.get(url)

    wait_until_page_contains_xpath(driver, '//button[contains(text(), "Sign-in")]')
    driver.find_element_by_xpath('//button[contains(text(), "Sign-in")]').click()

    try:
        wait_until_page_contains_xpath(driver, '//div[text()="Sign in"]')
        time.sleep(1)
    except:
        print('Sign in page didn\'t appear?')
        driver.close()

    try:
        driver.find_element_by_xpath('//input[@type="email"]').send_keys(email)
        time.sleep(1)
        driver.find_element_by_xpath('//input[@value="Next"]').click()

        wait_until_page_contains_xpath(driver, '//div[text()="Enter password"]')
        time.sleep(1)
    except:
        print('Error when entering/submitting email!')
        driver.close()

    try:
        driver.find_element_by_xpath('//input[@type="password"]').send_keys(password)
        time.sleep(1)
        driver.find_element_by_xpath('//input[@value="Sign in"]').click()

        wait_until_page_contains_xpath(driver, '//div[text()="Stay signed in?"]')
        wait_until_page_contains_xpath(driver, '//input[@value="No"]')
    except:
        print('Error when entering/submitting password')
        driver.close()

    time.sleep(1)
    driver.find_element_by_xpath('//input[@value="No"]').click()

    # Register user if necessary
    try:
        wait_until_page_contains_xpath(driver, '//span[contains(text(),"Register")]')
        driver.find_element_by_css_selector('#Input_FirstName').clear()
        driver.find_element_by_css_selector('#Input_FirstName').send_keys('User1')
        driver.find_element_by_css_selector('#Input_LastName').clear()
        driver.find_element_by_css_selector('#Input_LastName').send_keys('EESADMIN')
        driver.find_element_by_css_selector('#Input_Email').clear()
        driver.find_element_by_css_selector('#Input_Email').send_keys(args.email)
        driver.find_element_by_xpath('//button[contains(text(), "Register")]').click()
    except:
        pass

    wait_until_page_contains_xpath(driver, '//span[text()="Welcome"]')

    local_storage_json = driver.execute_script(f"return window.localStorage.getItem('GovUk.Education.ExploreEducationStatistics.Adminuser:{url}:GovUk.Education.ExploreEducationStatistics.Admin')")
    assert local_storage_json is not None, f"Couldn't find 'GovUk.Education.ExploreEducationStatistics.Adminuser:{url}:GovUk.Education.ExploreEducationStatistics.Admin' in Local Storage!"

    identity_cookie_dict = driver.get_cookie('.AspNetCore.Identity.Application')
    assert identity_cookie_dict is not None, "Couldn't get cookie '.AspNetCore.Identity.Application'"
    identity_cookie = json.dumps(identity_cookie_dict)

    driver.close()
    return (local_storage_json, identity_cookie)

if __name__ == "__main__":
    parser = argparse.ArgumentParser(prog="python get_auth_tokens.py",
                                     description="To get the authentication tokens for using the admin service")
    parser.add_argument(dest="url",
                        help="URL of environment you wish to get authentication tokens for")
    parser.add_argument(dest="email",
                        help="Email address to login using")
    parser.add_argument(dest="password",
                        help="Password to login with")
    args = parser.parse_args()

    (local_storage_json, _) = get_identity_info(args.url, args.email, args.password)
    jwt = json.loads(local_storage_json)['access_token']
    assert jwt is not None, "Couldn't get 'access_token' from local storage!"

    f = open('jwt', 'w')
    f.write(jwt)
    f.close()
