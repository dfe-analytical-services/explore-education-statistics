import os
import time
import argparse
from selenium import webdriver
import chromedriver_install as cdi

parser = argparse.ArgumentParser(prog="python get_auth_tokens.py",
                                 description="To get the authentication tokens for using the admin service")
parser.add_argument(dest="url",
                    help="URL of environment you wish to get authentication tokens for")
parser.add_argument(dest="email",
                    help="Email address to login using")
parser.add_argument(dest="password",
                    help="Password to login with")
args = parser.parse_args()

print("args.url: ", args.url)

cdi.install(file_directory="./",
            verbose=True,
            chmod=True,
            overwrite=False,
            version='78.0.3904.70')
os.environ["PATH"] += os.pathsep + os.getcwd()
            
timeout = 10

def wait_until_page_contains_xpath(context, selector):
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
    
chrome_options = webdriver.ChromeOptions()
chrome_options.add_argument('--no-sandbox')
chrome_options.add_argument('--headless')
chrome_options.add_argument('--disable-gpu')
driver = webdriver.Chrome(options=chrome_options)
driver.get(args.url)

wait_until_page_contains_xpath(driver, '//button[contains(text(), "Sign-in")]')
driver.find_element_by_xpath('//button[contains(text(), "Sign-in")]').click()

wait_until_page_contains_xpath(driver, '//div[text()="Sign in"]')
time.sleep(1)
driver.find_element_by_xpath('//input[@type="email"]').send_keys(args.email)
time.sleep(1)
driver.find_element_by_xpath('//input[@value="Next"]').click()

wait_until_page_contains_xpath(driver, '//div[text()="Enter password"]')
time.sleep(1)
driver.find_element_by_xpath('//input[@type="password"]').send_keys(args.password)
time.sleep(1)
driver.find_element_by_xpath('//input[@value="Sign in"]').click()

wait_until_page_contains_xpath(driver, '//div[text()="Stay signed in?"]')
wait_until_page_contains_xpath(driver, '//input[@value="No"]')
time.sleep(1)
driver.find_element_by_xpath('//input[@value="No"]').click()

wait_until_page_contains_xpath(driver, '//span[text()="Welcome"]')

jwt = driver.execute_script(f"return JSON.parse(window.localStorage.getItem('GovUk.Education.ExploreEducationStatistics.Adminuser:{args.url}:GovUk.Education.ExploreEducationStatistics.Admin')).access_token")
assert jwt is not None, f"Couldn't find JWT 'GovUk.Education.ExploreEducationStatistics.Adminuser:{args.url}:GovUk.Education.ExploreEducationStatistics.Admin'"

driver.close()

f = open('jwt', 'w')
f.write(jwt)
f.close()
