import os
import sys
import time
from selenium import webdriver
from selenium.webdriver.chrome.options import Options
from selenium.common.exceptions import NoSuchElementException

import chromedriver_install as cdi

def wait_for_server(url):
  path = cdi.install(file_directory='./lib/', verbose=False, chmod=True, overwrite=False, version=None)
  os.environ["PATH"] += os.pathsep + os.getcwd() + os.sep + 'lib'
  chrome_options = Options()
  chrome_options.add_argument("--headless")
  driver = webdriver.Chrome(options=chrome_options)
  driver.get(url + '/themes')
  timeWaited = 0
  print('Waiting for APIs...', end='')
  while not __check_elem_exists(driver, '[data-testid="content-item-list--schoolss"]'):
    print('.', end='')
    sys.stdout.flush()
    time.sleep(3)
    timeWaited += 5
    if timeWaited > 300:
      driver.close()
      print(' I CANNE DO IT CAPTAIN!')
      raise Exception('APIs not spun up after five minutes!')
    driver.refresh()
  print(' IT\'S ALIVE!')
  driver.close()


def __check_elem_exists(driver, css):
  try:
    driver.find_element_by_css_selector(css)
  except NoSuchElementException:
    return False
  return True

if __name__ == '__main__':
  warm_up_server("https://educationstatisticstest.z6.web.core.windows.net/themes")