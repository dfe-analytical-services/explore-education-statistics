from robot.libraries.BuiltIn import BuiltIn
from selenium.webdriver.common.action_chains import ActionChains
from selenium.common.exceptions import NoSuchElementException

sl = BuiltIn().get_library_instance('SeleniumLibrary')
import os
import re


def raise_assertion_error(err_msg):
    sl.capture_page_screenshot()
    raise AssertionError(err_msg)


def cookie_should_not_exist(name):
    for cookie in sl.driver.get_cookies():
        if cookie['name'] == name:
            raise_assertion_error(f'Cookie {name} exists when it shouldn\'t!')


def cookie_should_have_value(name, value):
    for cookie in sl.driver.get_cookies():
        if cookie['name'] == name and cookie['value'] == value:
            return
    raise_assertion_error(f"Couldn't find cookie {name} with value {value}")


def cookie_names_should_be_on_page():
    cookies = sl.driver.get_cookies()
    for cookie in cookies:
        if cookie['name'] in ['_hjIncludedInSample', '_hjid']:
            continue
        try:
            sl.page_should_contain(cookie['name'])
        except:
            raise_assertion_error(f"Page should contain text \"{cookie['name']}\"!")


def user_checks_key_stat_tile_contents(tile_title, tile_value, tile_context):
    try:
        elem = sl.driver.find_element_by_xpath(
            f'.//*[@data-testid="keyStatTile-title" and contains(text(), "{tile_title}")]')
    except NoSuchElementException:
        raise_assertion_error(f'Cannot find key stats tile "{tile_title}"')

    try:
        elem.find_element_by_xpath(f'../p[text()="{tile_value}"]')
    except NoSuchElementException:
        raise_assertion_error(f'Cannot find key stat tile "{tile_title}" with value "{tile_value}"')

    try:
        elem.find_element_by_xpath(f'../p[text()="{tile_context}"]')
    except NoSuchElementException:
        raise_assertion_error(
            f'Cannot find key stat tile "{tile_title}" with context "{tile_context}"')


def user_checks_key_stat_bullet_exists(bullet_text):
    elem = sl.driver.find_element_by_xpath('.//*[@id="releaseHeadlines-dataBlock-1"]')
    try:
        elem.find_element_by_xpath(f'.//li[text()="{bullet_text}"]')
    except NoSuchElementException:
        raise_assertion_error(f'Cannot find KeyStat summary bullet "{bullet_text}"')


def user_checks_number_of_other_releases_is_correct(number):
    elems = sl.driver.find_elements_by_xpath('(.//*[@data-testid="other-release-item"])')
    if len(elems) != int(number):
        raise_assertion_error(f'Found "{len(elems)}" other releases, not "{int(number)}"')


def user_checks_other_release_is_shown_in_position(release_name, position):
    try:
        sl.driver.find_element_by_xpath(
            f'.//*[@data-testid="other-release-item" and a/text()="{release_name}"]')
    except:
        raise_assertion_error(f'No other release "{release_name}" found')

    try:
        elem = sl.driver.find_element_by_xpath(
            f'(.//a[../@data-testid="other-release-item"])[{position}]')
    except:
        raise_assertion_error(f"There are less than {position} other releases listed!")

    if release_name != elem.text:
        raise_assertion_error(
            f'Other release "{release_name}" not in position {position}. Found "{elem.text}" instead!')


def user_checks_number_of_updates_is_correct(number):
    elems = sl.driver.find_elements_by_xpath('(.//*[@data-testid="last-updated-element"])')
    if len(elems) != int(number):
        raise_assertion_error(f'Found "{len(elems)}" updates, not "{int(number)}"')


def user_checks_update_exists(date, text_starts_with):
    try:
        elem = sl.driver.find_element_by_xpath(
            f'.//*[@data-testid="last-updated-element" and time/text()="{date}"]')
    except NoSuchElementException:
        raise_assertion_error(f'No update with date "{date}" found')

    try:
        elem.find_element_by_xpath(f'./p[starts-with(text(), "{text_starts_with}")]')
    except NoSuchElementException:
        raise_assertion_error(f'No update on "{date}" found starting with text "{text_starts_with}"')


# Methodology
def user_checks_page_contains_methodology_link(topic, methodology, link_url):
    try:
        sl.driver.find_element_by_xpath(f'//summary/span[text()="{topic}"]')
    except:
        raise_assertion_error(f'Cannot find theme "{topic}" on page')

    try:
        sl.driver.find_element_by_xpath(
            f'//summary/span[text()="{topic}"]/../..//h3[text()="{methodology}"]')
    except:
        raise_assertion_error(f'Topic "{topic}" doesn\'t contain methodology "{methodology}"!')

    try:
        sl.driver.find_element_by_xpath(
            f'//h3[text()="{methodology}"]/..//a[text()="View methodology" and @href="{link_url}"]')
    except:
        raise_assertion_error(
            f'View methodology link for "{methodology}" should be linking to "{link_url}"!')


def user_clicks_methodology_link(topic, methodology):
    try:
        elem = sl.driver.find_element_by_xpath(f'//summary/span[text()="{topic}"]')
    except:
        raise_assertion_error(f'Cannot find theme "{topic}" on page')

    try:
        elem.find_element_by_xpath(f'./../..//h3[text()="{methodology}"]')
    except:
        raise_assertion_error(f'Topic "{topic}" doesn\'t contain methodology "{methodology}"!')

    try:
        elem.find_element_by_xpath(
            f'./../..//h3[text()="{methodology}"]/..//a[text()="View methodology"]').click()
    except:
        raise_assertion_error(f'Cannot click "View methodology" link for "{methodology}"!')


# Table tool
def user_checks_generated_permalink_is_valid():
    elem = sl.driver.find_element_by_css_selector('[data-testid="permalink-generated-url"]')
    url_without_basic_auth = re.sub(r'.*@', '', os.environ['PUBLIC_URL'])
    url_without_http = re.sub(r'^https?:\/\/', '', url_without_basic_auth)
    current_url_without_http = re.sub(r'^https?:\/\/', '', elem.text)
    if not current_url_without_http.startswith(f"{url_without_http}/data-tables/permalink/"):
        raise_assertion_error(
            f'Generated permalink "{current_url_without_http}" is invalid! Should start with "{url_without_http}/data-tables/permalink/"')


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
