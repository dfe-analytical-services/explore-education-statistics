import os
import time
import argparse
from selenium import webdriver
import chromedriver_install as cdi

parser = argparse.ArgumentParser(prog="python get_azureadcookie.py", 
                                 description="To get the .AspNetCore.AzureADCookie")
parser.add_argument(dest="url",
                    help="URL of environment you wish to get AzureADCookie for")
parser.add_argument(dest="email",
                    help="Email address to login using")
parser.add_argument(dest="password",
                    help="Password to login with")
args = parser.parse_args()

cdi.install(file_directory="./",
            verbose=True,
            chmod=True,
            overwrite=False,
            version='75.0.3770.140')
            
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
    raise Exception(f"Timeout! Couldn't find element with xpath selector '{selector}''")
    
driver = webdriver.Chrome(executable_path=os.getcwd() + os.sep + "chromedriver")
driver.get(args.url)

wait_until_page_contains_xpath(driver, '//a[text()="Sign-in"]')
driver.find_element_by_xpath('//a[text()="Sign-in"]').click()

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

azure_ad_cookie = None
for cookie in driver.get_cookies():
    if cookie['name'] == ".AspNetCore.AzureADCookie":
        azure_ad_cookie = cookie['value']
        break
assert azure_ad_cookie is not None, "Couldn't find cookie '.AspNetCore.AzureADCookie'"

driver.close()

f = open('azure_ad_cookie', 'w')
f.write(azure_ad_cookie)
f.close()
