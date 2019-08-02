from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.common.action_chains  import ActionChains
from selenium.common.exceptions import NoSuchElementException
sl = BuiltIn().get_library_instance('SeleniumLibrary')

def cookie_names_should_be_on_page():
  cookies = sl.driver.get_cookies()
  for cookie in cookies:
    if cookie['name'] == '_hjIncludedInSample':
      continue
    try:
      sl.wait_until_element_contains('css:table', cookie['name'])
    except:
      raise AssertionError(f"Table containing cookie names should be on page, containing cookie name {cookie['name']}")

def user_should_be_at_top_of_page():
  (x, y) = sl.get_window_position()
  if y != 0:
    raise AssertionError(f"Windows position Y is {y} not 0! User should be at the top of the page!")

def user_checks_page_contains_accordion(accordion_heading):
  try:
    sl.driver.find_element_by_xpath(f'//*[@class="govuk-accordion__section-button" and text()="{accordion_heading}"]')
  except:
    raise AssertionError(f"Accordion with heading '{accordion_heading} not found!'")

def user_checks_accordion_is_in_position(header_starts_with, position):
  try:
    # NOTE(mark): When nth-child won't do, you need to do the unholy equivalent of css .class in xpath...
    elem = sl.driver.find_element_by_xpath(f'(.//*[contains(concat(" ", normalize-space(@class), " "), " govuk-accordion__section ")])[{position}]')
  except:
    raise AssertionError(f"There are less than {position} accordion sections!")

  if not elem.text.startswith(header_starts_with):
    raise AssertionError(f'Accordion in position {position} expected start with text "{header_starts_with}". Actual found text: "{elem.text}"')

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

def user_checks_accordion_section_contains_details(accordion_section, details_component):
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

def user_checks_details_dropdown_contains_publication(details_heading, publication_name):
  try:
    sl.driver.find_element_by_xpath(f'//*[@class="govuk-details__summary-text" and text()="{details_heading}"]')
  except:
    raise AssertionError(f'Cannot find details component "{details_heading}"')

  try:
    sl.driver.find_element_by_xpath(f'//*[@class="govuk-details__summary-text" and text()="{details_heading}"]/../..//strong[text()="{publication_name}"]')
  except:
    raise AssertionError(f'Cannot find publication "{publication_name}" inside details component "{details_heading}"')

def user_checks_key_stat_tile_contents(tile_title, tile_value, tile_context):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@data-testid="key-stat-tile-title" and contains(text(), "{tile_title}")]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find key stats tile "{tile_title}"')

  try:
    elem.find_element_by_xpath(f'../p[text()="{tile_value}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find key stat tile "{tile_title}" with value "{tile_value}"')

  try:
    elem.find_element_by_xpath(f'../p[text()="{tile_context}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find key stat tile "{tile_title}" with context "{tile_context}"')

def user_checks_key_stat_bullet_exists(bullet_text):
  elem = sl.driver.find_element_by_xpath('.//*[@id="keystats-summary"]')
  try:
    elem.find_element_by_xpath(f'.//li[text()="{bullet_text}"]')
  except NoSuchElementException:
    raise AssertionError(f'Cannot find KeyStat summary bullet "{bullet_text}"')

def user_checks_number_of_previous_releases_is_correct(number):
  elems = sl.driver.find_elements_by_xpath('(.//*[@data-testid="previous-release-item"])')
  if len(elems) != int(number):
    raise AssertionError(f'Found "{len(elems)}" previous releases, not "{int(number)}"')

def user_checks_previous_release_is_shown(release_name):
  try:
    sl.driver.find_element_by_xpath(f'.//*[@data-testid="previous-release-item" and a/text()="{release_name}"]')
  except:
    raise AssertionError(f'No previous release "{release_name}" found')

def user_checks_previous_release_is_shown_in_position(release_name, position):
  try:
    sl.driver.find_element_by_xpath(f'.//*[@data-testid="previous-release-item" and a/text()="{release_name}"]')
  except:
    raise AssertionError(f'No previous release "{release_name}" found')

  try:
    elem = sl.driver.find_element_by_xpath(f'(.//a[../@data-testid="previous-release-item"])[{position}]')
  except:
    raise AssertionError(f"There are less than {position} previous releases listed!")

  if release_name != elem.text:
    raise AssertionError(f'Previous release "{release_name}" not in position {position}. Found "{elem.text}" instead!')

def user_checks_number_of_updates_is_correct(number):
  elems = sl.driver.find_elements_by_xpath('(.//*[@data-testid="last-updated-element"])')
  if len(elems) != int(number):
    raise AssertionError(f'Found "{len(elems)}" updates, not "{int(number)}"')

def user_checks_update_exists(date, text_starts_with):
  try:
    elem = sl.driver.find_element_by_xpath(f'.//*[@data-testid="last-updated-element" and time/text()="{date}"]')
  except NoSuchElementException:
    raise AssertionError(f'No update with date "{date}" found')

  try:
    elem.find_element_by_xpath(f'./p[starts-with(text(), "{text_starts_with}")]')
  except NoSuchElementException:
    raise AssertionError(f'No update on "{date}" found starting with text "{text_starts_with}"')

def user_selects_radio(radio_label):
  sl.driver.find_element_by_xpath(f'//label[text()="{radio_label}"]').click()

def user_checks_radio_is_checked(radio_label):
  try:
    sl.driver.find_element_by_css_selector(f'input[data-testid="{radio_label}"][checked]')
  except NoSuchElementException:
    raise AssertionError('Unable to find checked radio!')

def user_clicks_checkbox(checkbox_label):
  sl.driver.find_element_by_xpath(f'//label[text()="{checkbox_label}"]').click()

def user_selects_start_date(start_date):
  sl.select_from_list_by_label('css:#timePeriodForm-start', start_date)

def user_selects_end_date(end_date):
  sl.select_from_list_by_label('css:#timePeriodForm-end', end_date)

def user_clicks_indicator_checkbox(subheading_label, indicator_label):
  sl.driver.find_element_by_xpath(f'//*[@id="filtersForm-indicators"]//legend[text()="{subheading_label}"]/..//label[text()="{indicator_label}"]').click()

def user_clicks_category_checkbox(subheading_label, category_label):
  sl.driver.find_element_by_xpath(f'//legend[text()="{subheading_label}"]/..//label[text()="{category_label}"]').click()

def user_clicks_select_all_for_category(category_label):
  sl.driver.find_element_by_xpath(f'//legend[text()="{category_label}"]/..//button[contains(text(),"Select")]').click()

def user_checks_results_table_column_heading_contains(row, column, expected):
  elem = sl.driver.find_element_by_xpath(f'//table/thead/tr[{row}]/th[{column}]')
  if expected not in elem.text:
    raise AssertionError(f'"{expected} not found in th tag in results table thead row {row}, column {column}')

def user_checks_results_table_row_heading_contains(row, column, expected):
  elem = sl.driver.find_element_by_xpath(f'//table/tbody/tr[{row}]/th[{column}]')
  if expected not in elem.text:
    raise AssertionError(f'"{expected} not found in th tag in results table tbody row {row}, column {column}')

def user_checks_results_table_cell_contains(row, column, expected):
  elem = sl.driver.find_element_by_xpath(f'//table/tbody/tr[{row}]/td[{column}]')
  if expected not in elem.text:
    raise AssertionError(f'"{expected} not found in td tag in results table tbody row {row}, column {column}')

def user_checks_previous_table_tool_step_contains(step, key, value):
  try:
    sl.wait_until_page_contains_element(f'xpath://*[@id="tableTool-steps-step-{step}"]//*[text()="Go to this step"]')
    elem = sl.driver.find_element_by_css_selector(f"#tableTool-steps-step-{step}")
  except:
    raise AssertionError(f'Element "#tableTool-steps-step-{step}" isn\'t the previous step!')

  try:
    elem.find_element_by_xpath(f'.//dt[text()="{key}"]/../dd[text()="{value}"]')
  except:
    sl.capture_page_screenshot()
    raise AssertionError(f'Element "#tableTool-steps-step-{step}" containing "{key}" and "{value}" not found!')

def user_reorders_table_headers(drag_selector, drop_selector):
  drag_elem = None
  drop_elem = None
  if drag_selector.startswith('css:'):
    drag_selector = drag_selector[4:]
    sl.wait_until_page_contains_element(f'css:{drag_selector}')
    drag_elem = sl.driver.find_element_by_css_selector(drag_selector)
  if drop_selector.startswith('css:'):
    drop_selector = drop_selector[4:]
    sl.wait_until_page_contains_element(f'css:{drop_selector}')
    drop_elem = sl.driver.find_element_by_css_selector(drop_selector)
  if drag_selector.startswith('xpath:'):
    drag_selector = drag_selector[6:]
    sl.wait_until_page_contains_element(f'xpath:{drag_selector}')
    drag_elem = sl.driver.find_element_by_xpath(drag_selector)
  if drop_selector.startswith('xpath:'):
    drop_selector = drop_selector[6:]
    sl.wait_until_page_contains_element(f'xpath:{drop_selector}')
    drop_elem = sl.driver.find_element_by_xpath(drop_selector)

  # https://github.com/react-dnd/react-dnd/issues/1195#issuecomment-456370983
  action = ActionChains(sl.driver)
  action.click_and_hold(drag_elem).perform()
  action.move_to_element(drop_elem).perform()
  action.move_by_offset(0, 0).pause(0.01).perform()
  action.release().perform()
