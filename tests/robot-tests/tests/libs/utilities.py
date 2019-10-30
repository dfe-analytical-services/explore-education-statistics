from logging import warn
from robot.libraries.BuiltIn import BuiltIn
from selenium.common.exceptions import NoSuchElementException
sl = BuiltIn().get_library_instance('SeleniumLibrary')
from datetime import datetime

def get_datetime(strf):
  now = datetime.now()
  return now.strftime(strf)

def user_should_be_at_top_of_page():
  (x, y) = sl.get_window_position()
  if y != 0:
    raise AssertionError(f"Windows position Y is {y} not 0! User should be at the top of the page!")

def user_checks_page_contains_accordion(accordion_heading):
  try:
    sl.wait_until_page_contains_element(f'xpath://*[@class="govuk-accordion__section-button" and text()="{accordion_heading}"]')
    # sl.driver.find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{accordion_heading}"]')
  except:
    raise AssertionError(f"Accordion with heading '{accordion_heading}' not found!'")

def user_checks_accordion_is_in_position(header_starts_with, position):
  try:
    # NOTE(mark): When nth-child won't do, you need to do the unholy equivalent of css .class in xpath...
    elem = sl.driver.find_element_by_xpath(f'(.//*[contains(concat(" ", normalize-space(@class), " "), " govuk-accordion__section ")])[{position}]')
  except:
    raise AssertionError(f"There are less than {position} accordion sections!")

  if not elem.text.strip().startswith(header_starts_with):
    raise AssertionError(f'Accordion in position {position} expected start with text "{header_starts_with}". Actual found text: "{elem.text}"')

def user_verifies_accordion_is_open(section_text):
  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{section_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{section_text}" and @aria-expanded="true"]')
  except NoSuchElementException:
    raise AssertionError(f'Accordion section "{section_text}" should have attribute aria-expanded="true"')

def user_verifies_accordion_is_closed(section_text):
  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{section_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{section_text}" and @aria-expanded="false"]')
  except NoSuchElementException:
    raise AssertionError(f'Accordion section "{section_text}" should have attribute aria-expanded="false"')

def user_opens_accordion_section(exact_section_text):
  try:
    sl.driver\
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}"]')\
      .click()
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {exact_section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}" and @aria-expanded="true"]')
  except NoSuchElementException:
    raise AssertionError(f'Accordion "{exact_section_text} not expanded!')

def user_closes_accordion_section(exact_section_text):
  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}"]') \
      .click()
  except NoSuchElementException:
    raise AssertionError(f'Cannot find accordion with header {exact_section_text}')

  try:
    sl.driver \
      .find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{exact_section_text}" and @aria-expanded="false"]')
  except NoSuchElementException:
    raise AssertionError(f'Accordion "{exact_section_text}" not collapsed!')

def user_checks_accordion_section_contains_text(accordion_section, details_component):
  try:
    sl.driver.find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{accordion_section}"]')
  except:
    raise AssertionError(f'Cannot find accordion section "{accordion_section}"')

  try:
    sl.driver.find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{accordion_section}"]/../../..//*[text()="{details_component}"]')
  except:
    raise AssertionError(f'Details component "{details_component}" not found in accordion section "{accordion_section}"')

def user_opens_details_dropdown(exact_details_text):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@class="govuk-details__summary-text" and text()="{exact_details_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'No such detail component "{exact_details_text}" found')

  elem.click()
  if elem.find_element_by_xpath('..').get_attribute("aria-expanded") == "false":
    raise AssertionError(f'Details component "{exact_details_text}" not expanded!')

def user_closes_details_dropdown(exact_details_text):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@class="govuk-details__summary-text" and text()="{exact_details_text}"]')
  except:
    raise AssertionError(f'Cannot find details component "{exact_details_text}"')
  elem.click()
  if elem.find_element_by_xpath('..').get_attribute("aria-expanded") == "true":
    raise AssertionError(f'Details component "{exact_details_text}" is still expanded!')

def user_selects_radio(radio_label):
  sl.driver.find_element_by_xpath(f'//label[text()="{radio_label}"]').click()

def user_checks_radio_is_checked(radio_label):
  try:
    sl.driver.find_element_by_css_selector(f'input[data-testid="{radio_label}"][checked]')
  except NoSuchElementException:
    raise AssertionError('Unable to find checked radio!')

def user_clicks_checkbox(checkbox_label):
  sl.driver.find_element_by_xpath(f'//label[text()="{checkbox_label}"]').click()

def capture_large_screenshot():
  currentWindow = sl.get_window_size()
  page_height = sl._current_browser().execute_script("return document.documentElement.scrollHeight;")

  page_width = currentWindow[0]
  original_height = currentWindow[1]

  sl.set_window_size(page_width, page_height)
  warn("Capturing a screenshot at URL " + sl.get_location())
  sl.capture_page_screenshot()
  sl.set_window_size(page_width, original_height)
