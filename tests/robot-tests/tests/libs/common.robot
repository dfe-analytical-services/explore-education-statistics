*** Settings ***
Library     SeleniumLibrary    timeout=%{TIMEOUT}    implicit_wait=%{IMPLICIT_WAIT}    run_on_failure=do this on failure
Library     OperatingSystem
Library     Collections
Library     file_operations.py
Library     utilities.py
Library     fail_fast.py
Library     visual.py
Resource    ./tables-common.robot
Resource    ./table_tool.robot


*** Variables ***
${browser}=                             chrome
${headless}=                            1
${FILES_DIR}=                           ${EXECDIR}${/}tests${/}files${/}
${DOWNLOADS_DIR}=                       ${EXECDIR}${/}test-results${/}downloads${/}
${timeout}=                             %{TIMEOUT}
${implicit_wait}=                       %{IMPLICIT_WAIT}
${prompt_to_continue_on_failure}=       0
${FAIL_TEST_SUITES_FAST}=               %{FAIL_TEST_SUITES_FAST}
${DATE_FORMAT_MEDIUM}=                  %-d %B %Y


*** Keywords ***
do this on failure
    # See if the currently executing Test Suite is failing fast and if not, take a screenshot and HTML grab of the
    # failing page.
    ${currently_failing_fast}=    current test suite failing fast

    IF    "${currently_failing_fast}" == "${FALSE}"
        capture large screenshot and html

        # Additionally, mark the current Test Suite as failing if the "FAIL_TEST_SUITES_FAST" option is enabled, and
        # this will cause subsequent tests within this same Test Suite to fail immediately (by virtue of their "Test
        # Setup" steps checking to see if their owning Test Suite is currently failing fast).
        IF    ${FAIL_TEST_SUITES_FAST} == 1
            record failing test suite
        END
    END

    IF    ${prompt_to_continue_on_failure} == 1
        prompt to continue
    END

user opens the browser
    [Arguments]    ${alias}=main
    IF    "${browser}" == "chrome"
        user opens chrome    ${alias}
    END
    IF    "${browser}" == "firefox"
        user opens firefox    ${alias}
    END
    IF    "${browser}" == "ie"
        user opens ie    ${alias}
    END
    go to    about:blank

user opens chrome
    [Arguments]    ${alias}=chrome
    IF    ${headless} == 1
        user opens chrome headlessly    ${alias}
    END
    IF    ${headless} == 0
        user opens chrome visually    ${alias}
    END

user opens firefox
    [Arguments]    ${alias}=firefox
    IF    ${headless} == 1
        user opens firefox headlessly    ${alias}
    END
    IF    ${headless} == 0
        user opens firefox visually    ${alias}
    END

user opens ie
    [Arguments]    ${alias}=ie
    open browser    about:blank    ie    alias=${alias}
    maximize browser window

user opens chrome headlessly
    [Arguments]    ${alias}=headless_chrome
    ${c_opts}=    Evaluate    sys.modules['selenium.webdriver'].ChromeOptions()    sys, selenium.webdriver
    Call Method    ${c_opts}    add_argument    headless
    Call Method    ${c_opts}    add_argument    start-maximized
    Call Method    ${c_opts}    add_argument    disable-extensions
    Call Method    ${c_opts}    add_argument    disable-infobars
    Call Method    ${c_opts}    add_argument    disable-gpu
    Call Method    ${c_opts}    add_argument    window-size\=1920,1080
    Call Method    ${c_opts}    add_argument    no-first-run
    Call Method    ${c_opts}    add_argument    no-default-browser-check
    Call Method    ${c_opts}    add_argument    ignore-certificate-errors
    Call Method    ${c_opts}    add_argument    log-level\=3
    Call Method    ${c_opts}    add_argument    disable-logging

    ${prefs}=    Create Dictionary    download.default_directory=${DOWNLOADS_DIR}
    Call Method    ${c_opts}    add_experimental_option    prefs    ${prefs}
    Create Webdriver    Chrome    ${alias}    chrome_options=${c_opts}

    ${all_opts}=    Call Method    ${c_opts}    to_capabilities

user opens chrome visually
    [Arguments]    ${alias}=chrome
    ${c_opts}=    Evaluate    sys.modules['selenium.webdriver'].ChromeOptions()    sys, selenium.webdriver
    Call Method    ${c_opts}    add_argument    no-sandbox
    Call Method    ${c_opts}    add_argument    disable-gpu
    Call Method    ${c_opts}    add_argument    disable-extensions
    Call Method    ${c_opts}    add_argument    window-size\=1920,1080
    Call Method    ${c_opts}    add_argument    ignore-certificate-errors

    ${prefs}=    Create Dictionary    download.default_directory=${DOWNLOADS_DIR}
    Call Method    ${c_opts}    add_experimental_option    prefs    ${prefs}
    Create Webdriver    Chrome    ${alias}    chrome_options=${c_opts}
    ${all_opts}=    Call Method    ${c_opts}    to_capabilities

    maximize browser window

user opens firefox headlessly
    [Arguments]    ${alias}=headless_firefox
    ${f_opts}=    Evaluate    sys.modules['selenium.webdriver'].firefox.options.Options()    sys, selenium.webdriver
    Call Method    ${f_opts}    add_argument    -headless
    Create Webdriver    Firefox    ${alias}    firefox_options=${f_opts}

user opens firefox visually
    [Arguments]    ${alias}=firefox
    open browser    about:blank    firefox    alias=${alias}
    maximize browser window

user switches browser
    [Arguments]    ${alias}
    switch browser    ${alias}

user closes the browser
    close browser

user closes all browsers
    close all browsers

user gets url
    ${url}=    get location
    [Return]    ${url}

user goes back
    go back

user reloads page
    reload page

user scrolls to the top of the page
    execute javascript    window.scrollTo(0, 0);

user scrolls to the bottom of the page
    user scrolls to element    css:.govuk-footer__licence-description

user scrolls down
    [Arguments]    ${px}
    execute javascript    window.scrollBy(0, ${px});

user scrolls up
    [Arguments]    ${px}
    execute javascript    window.scrollBy(0, -${px});

user scrolls to element
    [Arguments]    ${element}
    user waits until page contains element    ${element}
    scroll element into view    ${element}

user mouses over element
    [Arguments]    ${element}
    mouse over    ${element}

user gets css property value
    [Arguments]    ${locator}    ${property}
    ${element}=    get webelement    ${locator}
    ${value}=    call method    ${element}    value_of_css_property    ${property}
    [Return]    ${value}

user checks css property value
    [Arguments]    ${locator}    ${property}    ${value}
    ${actual_value}=    user gets css property value    ${locator}    ${property}
    should be equal    ${value}    ${actual_value}

user waits for page to finish loading
    # This is required because despite the DOM being loaded, and even a button being enabled, React/NextJS
    # hasn't finished processing the page, and so click are intermittently ignored. I'm wrapping
    # this sleep in a keyword such that if we find a way to check whether the JS processing has finished in the
    # future, we can change it here.
    sleep    1

user waits until page does not contain loading spinner
    # NOTE: The wait below is to prevent a transient error in CI ('Element 'css:[class^="LoadingSpinner"]' did not
    # disappear in 30 seconds.')

    # Also, we're only interested in loading spinners that aren't lazy loaders that are waiting for user interaction
    # prior to loading their content.
    user waits until page does not contain element    //*[@class!="lazyload-wrapper"]/*[@data-testid="loadingSpinner"]
    ...    %{WAIT_MEDIUM}

user sets focus to element
    [Arguments]    ${selector}    ${parent}=css:body
    ${element}=    lookup or return webelement    ${selector}    ${parent}
    set focus to element    ${element}

user waits until page contains
    [Arguments]    ${pageText}    ${wait}=${timeout}
    wait until page contains    ${pageText}    timeout=${wait}

user waits until page contains element
    [Arguments]    ${element}    ${wait}=${timeout}    ${limit}=None
    wait until page contains element    ${element}    timeout=${wait}    limit=${limit}

user waits until page does not contain
    [Arguments]    ${pageText}    ${wait}=${timeout}
    wait until page does not contain    ${pageText}    timeout=${wait}

user waits until page does not contain element
    [Arguments]    ${element}    ${wait}=${timeout}
    wait until page does not contain element    ${element}    timeout=${wait}

user waits until element contains
    [Arguments]    ${element}    ${text}    ${wait}=${timeout}
    wait until element contains    ${element}    ${text}    timeout=${wait}

user waits until page contains link
    [Arguments]    ${link_text}    ${wait}=${timeout}
    wait until page contains element    xpath://a[.="${link_text}"]    timeout=${wait}

user waits until page does not contain link
    [Arguments]    ${link_text}    ${wait}=${timeout}
    wait until page does not contain element    xpath://a[.="${link_text}"]    timeout=${wait}

user checks page does not contain link
    [Arguments]    ${link_text}
    user checks page does not contain element    xpath://a[.="${link_text}"]

user waits until element contains link
    [Arguments]    ${element}    ${link_text}    ${wait}=${timeout}
    user waits until parent contains element    ${element}    link:${link_text}    timeout=${wait}
    ${link}=    get child element    ${element}    xpath:.//a[.="${link_text}"]
    [Return]    ${link}

user waits until element contains testid
    [Arguments]    ${element}    ${testid}    ${wait}=${timeout}
    user waits until parent contains element    ${element}    css:[data-testid="${testid}"]    timeout=${wait}

user waits until page contains accordion section
    [Arguments]    ${section_title}    ${wait}=${timeout}
    user waits until page contains element
    ...    xpath://*[contains(@class,"govuk-accordion__section-button") and text()="${section_title}"]    ${wait}

user waits until page does not contain accordion section
    [Arguments]    ${section_title}    ${wait}=${timeout}
    user waits until page does not contain element
    ...    xpath://*[contains(@class,"govuk-accordion__section-button") and text()="${section_title}"]    ${wait}

user verifies accordion is open
    [Arguments]    ${section_text}
    user waits until page contains element
    ...    xpath://*[@class="govuk-accordion__section-button" and text()="${section_text}" and @aria-expanded="true"]

user verifies accordion is closed
    [Arguments]    ${section_text}
    user waits until page contains element
    ...    xpath://*[@class="govuk-accordion__section-button" and text()="${section_text}" and @aria-expanded="false"]

user checks there are x accordion sections
    [Arguments]    ${count}    ${parent}=css:body
    user waits until parent contains element    ${parent}    css:[data-testid="accordionSection"]    count=${count}

user checks accordion is in position
    [Arguments]    ${section_text}    ${position}    ${parent}=css:[data-testid="accordion"]
    user waits until parent contains element    ${parent}
    ...    xpath:(.//*[@data-testid="accordionSection"])[${position}]//button[starts-with(text(), "${section_text}")]

user waits until accordion section contains text
    [Arguments]    ${section_text}    ${text}    ${wait}=${timeout}
    ${section}=    user gets accordion section content element    ${section_text}
    user waits until parent contains element    ${section}    xpath:.//*[text()="${text}"]    timeout=${wait}

user gets accordion header button element
    [Arguments]    ${heading_text}    ${parent}=css:[data-testid="accordion"]
    ${button}=    get child element    ${parent}    xpath:.//button[@aria-expanded and contains(., "${heading_text}")]
    [Return]    ${button}

user opens accordion section
    [Arguments]
    ...    ${heading_text}
    ...    ${parent}=css:[data-testid="accordion"]

    ${header_button}=    user gets accordion header button element    ${heading_text}    ${parent}
    ${accordion}=    user opens accordion section with accordion header    ${header_button}    ${parent}
    [Return]    ${accordion}

user opens accordion section with id
    [Arguments]
    ...    ${id}
    ...    ${parent}=css:[data-testid="accordion"]

    ${header_button}=    get child element    ${parent}    id:${id}-heading
    ${accordion}=    user opens accordion section with accordion header    ${header_button}    ${parent}
    [Return]    ${accordion}

user opens accordion section with accordion header
    [Arguments]
    ...    ${header_button}
    ...    ${parent}=css:[data-testid="accordion"]

    ${is_expanded}=    get element attribute    ${header_button}    aria-expanded
    IF    '${is_expanded}' != 'true'
        user clicks element    ${header_button}
    END
    user checks element attribute value should be    ${header_button}    aria-expanded    true
    ${accordion}=    user gets accordion section content element from heading element    ${header_button}    ${parent}
    [Return]    ${accordion}

user closes accordion section
    [Arguments]    ${heading_text}    ${parent}=css:[data-testid="accordion"]
    ${header_button}=    user gets accordion header button element    ${heading_text}    ${parent}
    user closes accordion section with accordion header    ${header_button}    ${parent}

user closes accordion section with id
    [Arguments]
    ...    ${id}
    ...    ${parent}=css:[data-testid="accordion"]

    ${header_button}=    get child element    ${parent}    id:${id}-heading
    user closes accordion section with accordion header    ${header_button}    ${parent}

user closes accordion section with accordion header
    [Arguments]
    ...    ${header_button}
    ...    ${parent}=css:[data-testid="accordion"]

    ${is_expanded}=    get element attribute    ${header_button}    aria-expanded
    IF    '${is_expanded}' != 'false'
        user clicks element    ${header_button}
    END
    user checks element attribute value should be    ${header_button}    aria-expanded    false

user gets accordion section content element
    [Arguments]    ${heading_text}    ${parent}=css:[data-testid="accordion"]
    ${header_button}=    user gets accordion header button element    ${heading_text}    ${parent}
    ${content_id}=    get element attribute    ${header_button}    id
    ${content}=    get child element    ${parent}    css:[aria-labelledby="${content_id}"]
    [Return]    ${content}

user gets accordion section content element from heading element
    [Arguments]    ${heading_element}    ${parent}=css:[data-testid="accordion"]
    ${content_id}=    get element attribute    ${heading_element}    id
    ${content}=    get child element    ${parent}    css:[aria-labelledby="${content_id}"]
    [Return]    ${content}

user scrolls to accordion section content
    [Arguments]    ${heading_text}    ${parent}=css:[data-testid="accordion"]
    ${content}=    user gets accordion section content element    ${heading_text}    ${parent}
    user scrolls to element    ${content}
    # Workaround to get lazy loaded data blocks to render
    user scrolls down    1

user waits until page contains testid
    [Arguments]    ${id}    ${wait}=${timeout}
    user waits until page contains element    css:[data-testid="${id}"]    ${wait}

user checks page does not contain testid
    [Arguments]    ${id}
    user checks page does not contain element    css:[data-testid="${id}"]

user checks testid element contains
    [Arguments]    ${id}    ${text}
    user waits until element contains    css:[data-testid="${id}"]    ${text}

user gets testid element
    [Arguments]    ${id}    ${wait}=${timeout}    ${parent}=css:body
    user waits until parent contains element    ${parent}    css:[data-testid="${id}"]
    ${element}=    get child element    ${parent}    css:[data-testid="${id}"]
    [Return]    ${element}

user checks element does not contain
    [Arguments]    ${element}    ${text}
    element should not contain    ${element}    ${text}

user checks element contains child element
    [Arguments]
    ...    ${element}
    ...    ${child_element}
    user waits until parent contains element    ${element}    ${child_element}

user checks element does not contain child element
    [Arguments]
    ...    ${element}
    ...    ${child_element}
    user waits until parent does not contain element    ${element}    ${child_element}

user checks element contains
    [Arguments]    ${element}    ${text}
    user waits until parent contains element    ${element}    xpath://*[contains(.,"${text}")]

user checks element contains button
    [Arguments]
    ...    ${element}
    ...    ${button_text}
    user waits until parent contains element    ${element}    xpath://button[text()="${button_text}"]

user checks element does not contain button
    [Arguments]
    ...    ${element}
    ...    ${button_text}
    user waits until parent does not contain element    ${element}    xpath://button[text()="${button_text}"]

user checks element contains link
    [Arguments]
    ...    ${element}
    ...    ${link_text}
    user waits until parent contains element    ${element}    xpath://a[text()="${link_text}"]

user checks element does not contain link
    [Arguments]
    ...    ${element}
    ...    ${link_text}
    user waits until parent does not contain element    ${element}    xpath://a[text()="${link_text}"]

user waits until element is visible
    [Arguments]    ${selector}    ${wait}=${timeout}
    wait until element is visible    ${selector}    timeout=${wait}

user checks element is visible
    [Arguments]    ${element}
    element should be visible    ${element}

user waits until element is not visible
    [Arguments]    ${selector}    ${wait}=${timeout}
    wait until element is not visible    ${selector}    timeout=${wait}

user checks element is not visible
    [Arguments]    ${element}    ${wait}=${timeout}
    element should not be visible    ${element}    ${wait}

user checks element is visually hidden
    [Arguments]    ${selector}    ${parent}=css:body
    user checks element has class
    ...    ${selector}
    ...    govuk-visually-hidden
    ...    ${parent}

user checks element has class
    [Arguments]    ${selector}    ${class}    ${parent}=css:body
    ${element}=    lookup or return webelement    ${selector}    ${parent}
    ${classes}=    get element attribute    ${element}    class
    should contain    ${classes}    ${class}

user checks element does not have class
    [Arguments]    ${selector}    ${class}    ${parent}=css:body
    ${element}=    lookup or return webelement    ${selector}    ${parent}
    ${classes}=    get element attribute    ${element}    class
    should not contain    ${classes}    ${class}

user waits until element is enabled
    [Arguments]    ${element}    ${wait}=${timeout}
    wait until element is enabled    ${element}    ${wait}

user checks element is enabled
    [Arguments]    ${element}
    element should be enabled    ${element}

user checks element is disabled
    [Arguments]    ${element}
    element should be disabled    ${element}

user checks element should contain
    [Arguments]    ${element}    ${text}    ${wait}=${timeout}
    element should contain    ${element}    ${text}

user checks element should not contain
    [Arguments]    ${element}    ${text}
    element should not contain    ${element}    ${text}

user checks input field contains
    [Arguments]    ${element}    ${text}
    page should contain textfield    ${element}
    textfield value should be    ${element}    ${text}

user checks page contains
    [Arguments]    ${text}
    page should contain    ${text}

user checks page does not contain
    [Arguments]    ${text}
    page should not contain    ${text}

user checks page contains element
    [Arguments]    ${element}
    page should contain element    ${element}

user checks page does not contain element
    [Arguments]    ${element}
    page should not contain element    ${element}

user clicks element
    [Arguments]
    ...    ${selector}
    ...    ${parent}=css:body
    ${element}=    lookup or return webelement    ${selector}    ${parent}
    user scrolls to element    ${element}
    wait until element is enabled    ${element}
    click element    ${element}

user clicks link
    [Arguments]    ${text}    ${parent}=css:body
    user clicks element    link:${text}    ${parent}

user clicks link by visible text
    [Arguments]    ${text}    ${parent}=css:body
    user clicks element    xpath:.//a[text()="${text}"]    ${parent}

user clicks button
    [Arguments]    ${text}    ${parent}=css:body
    ${button}=    user gets button element    ${text}    ${parent}
    user clicks element    ${button}

user waits until page contains button
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until page contains element    xpath://button[text()="${text}"]    ${wait}

user checks page contains button
    [Arguments]    ${text}
    user checks page contains element    xpath://button[text()="${text}"]

user checks page does not contain button
    [Arguments]    ${text}
    user checks page does not contain element    xpath://button[text()="${text}"]

user waits until page does not contain button
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until page does not contain element    xpath://button[text()="${text}"]    ${wait}

user waits until button is enabled
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until element is enabled    xpath://button[text()="${text}"]    ${wait}

user waits until parent does not contain button
    [Arguments]    ${parent}    ${text}    ${wait}=${timeout}
    user waits until parent does not contain element    ${parent}    xpath://button[text()="${text}"]    ${wait}

user gets button element
    [Arguments]    ${text}    ${parent}=css:body
    user waits until parent contains element    ${parent}
    ...    xpath:.//button[text()="${text}" or .//*[text()="${text}"]]
    ${button}=    get child element    ${parent}    xpath:.//button[text()="${text}" or .//*[text()="${text}"]]
    [Return]    ${button}

user checks page contains tag
    [Arguments]    ${text}
    user checks page contains element    xpath://*[contains(@class, "govuk-tag")][text()="${text}"]

user waits until h1 is visible
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until element is visible    xpath://h1[text()="${text}"]    ${wait}

user waits until h1 is not visible
    [Arguments]    ${text}    ${wait}=%{WAIT_SMALL}
    user waits until element is not visible    xpath://h1[text()="${text}"]    ${wait}

user waits until h2 is visible
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until element is visible    xpath://h2[text()="${text}"]    ${wait}

user waits until h2 is not visible
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until element is not visible    xpath://h2[text()="${text}"]    ${wait}

user waits until h3 is visible
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until element is visible    xpath://h3[text()="${text}"]    ${wait}

user waits until h3 is not visible
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until element is not visible    xpath://h3[text()="${text}"]    ${wait}

user waits until legend is visible
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until element is visible    xpath://legend[text()="${text}"]    ${wait}

user waits until page contains title
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until page contains element    xpath://h1[@data-testid="page-title" and text()="${text}"]    ${wait}

user waits until page contains title caption
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until page contains element    xpath://span[@data-testid="page-title-caption" and text()="${text}"]
    ...    ${wait}

user selects newly opened window
    switch window    locator=NEW

user checks element attribute value should be
    [Arguments]    ${locator}    ${attribute}    ${expected}
    element attribute value should be    ${locator}    ${attribute}    ${expected}

user checks element value should be
    [Arguments]    ${locator}    ${value}    ${wait}=${timeout}
    element attribute value should be    ${locator}    value    ${value}    ${wait}

user checks textarea contains
    [Arguments]    ${selector}    ${text}
    textarea should contain    ${selector}    ${text}

user checks summary list contains
    [Arguments]    ${term}    ${description}    ${parent}=css:body    ${wait}=${timeout}
    user waits until parent contains element    ${parent}
    ...    xpath:.//dl//dt[contains(text(), "${term}")]/following-sibling::dd[contains(., "${description}")]
    ...    %{WAIT_MEDIUM}
    ${element}=    get child element    ${parent}
    ...    xpath:.//dl//dt[contains(text(), "${term}")]/following-sibling::dd[contains(., "${description}")]
    user waits until element is visible    ${element}    %{WAIT_LONG}

user checks select contains x options
    [Arguments]    ${locator}    ${num}
    ${options}=    get list items    ${locator}
    length should be    ${options}    ${num}

user checks select contains at least x options
    [Arguments]    ${locator}    ${num}
    ${options}=    get list items    ${locator}
    ${length}=    get length    ${options}
    should be true    ${options} > ${num}

user checks select contains option
    [Arguments]    ${locator}    ${label}
    ${options}=    get list items    ${locator}
    list should contain value    ${options}    ${label}

user checks select does not contain option
    [Arguments]    ${locator}    ${label}
    ${options}=    get list items    ${locator}
    list should not contain value    ${options}    ${label}

user checks selected option label
    [Arguments]    ${locator}    ${label}
    ${selected_label}=    get selected list label    ${locator}
    should be equal    ${selected_label}    ${label}

user chooses select option
    [Arguments]    ${locator}    ${label}
    user waits until page contains element    ${locator}
    user waits until parent contains element    ${locator}    xpath:.//option
    select from list by label    ${locator}    ${label}

user chooses select option at index
    [Arguments]    ${locator}    ${index}
    user waits until page contains element    ${locator}
    user waits until parent contains element    ${locator}    xpath:.//option
    select from list by index    ${locator}    ${index}

user chooses file
    [Arguments]    ${locator}    ${file_path}
    user waits until element is visible    ${locator}
    choose file    ${locator}    ${file_path}

user clears element text
    [Arguments]    ${selector}
    user clicks element    ${selector}
    press keys    ${selector}    CTRL+a+BACKSPACE
    sleep    0.3

user presses keys
    [Arguments]    @{keys}
    press keys    ${NONE}    @{keys}    # No selector as sometimes leads to text not being input
    sleep    0.1

user enters text into element
    [Arguments]    ${selector}    ${text}
    user sets focus to element    ${selector}
    user waits until element is visible    ${selector}    %{WAIT_SMALL}
    user clears element text    ${selector}
    press keys    ${selector}    ${text}
    sleep    0.3

user checks element count is x
    [Arguments]    ${locator}    ${count}
    page should contain element    ${locator}    count=${count}

user checks url contains
    [Arguments]    ${text}
    ${current_url}=    get location
    should contain    ${current_url}    ${text}

user checks page contains link with text and url
    [Arguments]
    ...    ${text}
    ...    ${href}
    ...    ${parent}=css:body
    user waits until parent contains element    ${parent}
    ...    xpath:.//a[@href="${href}" and contains(text(), "${text}")]

user opens details dropdown
    [Arguments]    ${text}    ${parent}=css:body
    user waits until parent contains element    ${parent}
    ...    xpath:.//details/summary[contains(., "${text}") and @aria-expanded]    %{WAIT_SMALL}
    ${details}=    get child element    ${parent}    xpath:.//details[summary[contains(., "${text}")]]
    ${summary}=    get child element    ${parent}    xpath:.//details/summary[contains(., "${text}")]
    user waits until element is visible    ${summary}    %{WAIT_SMALL}
    ${is_expanded}=    get element attribute    ${summary}    aria-expanded
    IF    '${is_expanded}' != 'true'
        user clicks element    ${summary}
    END
    user checks element attribute value should be    ${summary}    aria-expanded    true
    [Return]    ${details}

user closes details dropdown
    [Arguments]    ${text}    ${parent}=css:body
    user waits until parent contains element    ${parent}
    ...    xpath:.//details/summary[contains(., "${text}") and @aria-expanded]
    ${summary}=    get child element    ${parent}    xpath:.//details/summary[contains(., "${text}")]
    user waits until element is visible    ${summary}
    ${is_expanded}=    get element attribute    ${summary}    aria-expanded
    IF    '${is_expanded}' != 'false'
        user clicks element    ${summary}
    END
    user checks element attribute value should be    ${summary}    aria-expanded    false

user gets details content element
    [Arguments]    ${text}    ${parent}=css:body    ${wait}=${timeout}
    user waits until parent contains element    ${parent}    xpath:.//details/summary[contains(., "${text}")]
    ...    timeout=${wait}
    ${summary}=    get child element    ${parent}    xpath:.//details/summary[contains(., "${text}")]
    ${content_id}=    get element attribute    ${summary}    aria-controls
    ${content}=    get child element    ${parent}    id:${content_id}
    [Return]    ${content}

user waits until page contains details dropdown
    [Arguments]    ${text}    ${wait}=${timeout}
    user waits until page contains element    xpath:.//details/summary[contains(., "${text}")]    ${wait}

user checks page for details dropdown
    [Arguments]    ${text}
    user checks page contains element    xpath:.//details/summary[contains(., "${text}")]

user scrolls to details dropdown
    [Arguments]    ${text}    ${wait}=${timeout}
    user scrolls to element    xpath:.//details/summary[contains(., "${text}")]

user checks publication bullet contains link
    [Arguments]    ${publication}    ${link}
    user checks page contains element    xpath://details[@open]//*[text()="${publication}"]/..//a[text()="${link}"]

user checks publication bullet does not contain link
    [Arguments]    ${publication}    ${link}
    user checks page does not contain element
    ...    xpath://details[@open]//*[text()="${publication}"]/..//a[text()="${link}"]

user checks key stat contents
    [Arguments]    ${tile}    ${title}    ${statistic}    ${trend}    ${wait}=${timeout}
    user waits until element is visible
    ...    css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-title"]    ${wait}
    user waits until element contains    css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-title"]
    ...    ${title}
    user waits until element contains
    ...    css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-statistic"]    ${statistic}
    user waits until element contains
    ...    css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-trend"]    ${trend}

user checks key stat guidance
    [Arguments]    ${tile}    ${guidance_title}    ${guidance_text}
    user opens details dropdown    ${guidance_title}    css:[data-testid="keyStat"]:nth-of-type(${tile})
    user waits until element is visible
    ...    css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-guidanceText"]
    user checks element should contain
    ...    css:[data-testid="keyStat"]:nth-of-type(${tile}) [data-testid="keyStat-guidanceText"]    ${guidance_text}

    # The guidance text dropdown can hide the key stat Edit and Remove buttons
    user closes details dropdown    ${guidance_title}    css:[data-testid="keyStat"]:nth-of-type(${tile})

user checks page contains radio
    [Arguments]    ${label}
    page should contain radio button    xpath://label[text()="${label}"]/../input[@type="radio"]

user clicks radio
    [Arguments]    ${label}
    user clicks element    xpath://label[text()="${label}"]/../input[@type="radio"]

user clicks radio if exists
    [Arguments]    ${label}
    user clicks element if exists    xpath://label[text()="${label}"]/../input[@type="radio"]

user checks radio is checked
    [Arguments]    ${label}
    user checks page contains element    xpath://label[text()="${label}"]/../input[@type="radio" and @checked]

user checks radio in position has label
    [Arguments]    ${position}    ${label}
    user checks page contains element
    ...    xpath://*[contains(@data-testid, "Radio item for ")][${position}]//label[contains(text(), "${label}")]

user clicks checkbox
    [Arguments]    ${label}
    user scrolls to element    xpath://label[text()="${label}" or strong[text()="${label}"]]/../input[@type="checkbox"]
    user clicks element    xpath://label[text()="${label}" or strong[text()="${label}"]]/../input[@type="checkbox"]

user checks checkbox is checked
    [Arguments]    ${label}
    user checks checkbox input is checked
    ...    xpath://label[text()="${label}" or strong[text()="${label}"]]/../input[@type="checkbox"]

user checks checkbox is not checked
    [Arguments]    ${label}
    user checks checkbox input is not checked
    ...    xpath://label[text()="${label}" or strong[text()="${label}"]]/../input[@type="checkbox"]

user checks checkbox input is checked
    [Arguments]    ${selector}
    user waits until page contains element    ${selector}
    checkbox should be selected    ${selector}

user checks checkbox input is not checked
    [Arguments]    ${selector}
    user waits until page contains element    ${selector}
    checkbox should not be selected    ${selector}

user checks checkbox in position has label
    [Arguments]    ${position}    ${label}
    user checks page contains element
    ...    xpath://*[contains(@data-testid,"Checkbox item for ")][${position}]//label[contains(text(), "${label}")]

user checks list has x items
    [Arguments]    ${locator}    ${count}    ${parent}=css:body
    user waits until parent contains element    ${parent}    ${locator}
    ${list}=    get child element    ${parent}    ${locator}
    user waits until parent contains element    ${list}    css:li    count=${count}

user gets list item element
    [Arguments]    ${locator}    ${item_num}    ${parent}=css:body
    user waits until parent contains element    ${parent}    ${locator}
    ${list}=    get child element    ${parent}    ${locator}
    ${item}=    get child element    ${list}    css:li:nth-child(${item_num})
    [Return]    ${item}

user checks list item contains
    [Arguments]    ${locator}    ${item_num}    ${content}    ${parent}=css:body
    ${item}=    user gets list item element    ${locator}    ${item_num}    ${parent}
    user checks element should contain    ${item}    ${content}

user checks list item is visually hidden
    [Arguments]    ${locator}    ${item_num}    ${parent}=css:body
    ${item}=    user gets list item element    ${locator}    ${item_num}    ${parent}
    user checks element is visually hidden    ${item}

user checks list contains exact items in order
    [Arguments]    ${locator}    ${expected_items}    ${parent}=css:body
    user waits until parent contains element    ${parent}    ${locator}
    ${list}=    get child element    ${parent}    ${locator}
    ${actual}=    get child elements    ${list}    css:li
    ${num_items}=    Get Length    ${expected_items}
    length should be    ${actual}    ${num_items}
    FOR    ${index}    ${content}    IN ENUMERATE    @{expected_items}
        user checks element should contain    ${actual}[${index}]    ${content}
    END

user checks items matching locator contain exact items in order
    [Arguments]    @{expected_items}    ${locator}
    ${actual}=    Get WebElements    ${locator}
    ${num_items}=    Get Length    ${expected_items}
    length should be    ${actual}    ${num_items}
    FOR    ${index}    ${content}    IN ENUMERATE    @{expected_items}
        user checks element should contain    ${actual}[${index}]    ${content}
    END

user checks breadcrumb count should be
    [Arguments]    ${count}
    user waits until page contains element    css:[data-testid="breadcrumbs--list"] li    limit=${count}

user checks nth breadcrumb contains
    [Arguments]    ${num}    ${text}
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(${num})    ${text}

user waits until page contains other release
    [Arguments]    ${other_release_title}    ${wait}=${timeout}
    user waits until page contains element
    ...    xpath://li[@data-testid="other-release-item"]/a[text()="${other_release_title}"]    ${wait}

user checks page does not contain other release
    [Arguments]    ${other_release_title}
    user checks page does not contain element
    ...    xpath://li[@data-testid="other-release-item"]/a[text()="${other_release_title}"]

user navigates to admin frontend
    [Arguments]    ${URL}=%{ADMIN_URL}
    disable basic auth headers
    go to    ${URL}

user navigates to public frontend
    [Arguments]    ${URL}=%{PUBLIC_URL}
    enable basic auth headers
    go to    ${URL}

user navigates to find statistics page on public frontend
    environment variable should be set    PUBLIC_URL
    user navigates to public frontend    %{PUBLIC_URL}/find-statistics
    user waits until h1 is visible    Find statistics and data

user navigates to data tables page on public frontend
    environment variable should be set    PUBLIC_URL
    user navigates to public frontend    %{PUBLIC_URL}/data-tables
    user waits until h1 is visible    Create your own tables

user navigates to data catalogue page on public frontend
    environment variable should be set    PUBLIC_URL
    user navigates to public frontend    %{PUBLIC_URL}/data-catalogue
    user waits until page contains title caption    Data catalogue
    user waits until h1 is visible    Browse our open data
    user waits until page contains    View all of the open data available and choose files to download.

check that variable is not empty
    [Arguments]    ${variable_name}    ${variable_value}
    IF    '${variable_value}'=='${EMPTY}'
        Variable "${variable_name}" is empty.
    END

user waits until table tool wizard step is available
    [Arguments]    ${step_number}    ${table_tool_step_title}    ${wait}=%{WAIT_MEDIUM}
    user waits until page contains element    xpath://*[@data-testid="wizardStep-${step_number}"]    ${wait}
    user waits until page does not contain element    xpath://*[@data-testid="wizardStep-${step_number}" and @hidden]
    ...    ${wait}
    # this visible check passes when it should fail?!
    user waits until element is visible    xpath://h2|h3//*[contains(text(),"${table_tool_step_title}")]
    ...    %{WAIT_SMALL}
    user waits until page does not contain loading spinner

user gets data block from parent
    [Arguments]    ${data_block_name}    ${parent}
    ${data_block_test_id}=    set variable    testid:Data block - ${data_block_name}
    user waits until parent contains element    ${parent}    ${data_block_test_id}
    ${data_block}=    get child element    ${parent}    ${data_block_test_id}
    [Return]    ${data_block}

user gets data block table from parent
    [Arguments]    ${data_block_name}    ${parent}
    ${data_block}=    user gets data block from parent    ${data_block_name}    ${parent}
    user clicks link by visible text    Table    ${data_block}
    ${data_block_id}=    get element attribute    ${data_block}    id
    ${data_block_table}=    get child element    ${data_block}    id:${data_block_id}-tables
    [Return]    ${data_block_table}

user gets data block chart from parent
    [Arguments]    ${data_block_name}    ${parent}
    ${data_block}=    user gets data block from parent    ${data_block_name}    ${parent}
    user clicks link by visible text    Chart    ${data_block}
    ${data_block_id}=    get element attribute    ${data_block}    id
    ${data_block_chart}=    get child element    ${data_block}    id:${data_block_id}-chart
    [Return]    ${data_block_chart}

lookup or return webelement
    [Arguments]
    ...    ${selector_or_webelement}
    ...    ${parent}=css:body

    ${is_webelement}=    is webelement    ${selector_or_webelement}
    IF    ${is_webelement} is ${TRUE}
        ${element}=    set variable    ${selector_or_webelement}
    ELSE
        user waits until parent contains element    ${parent}    ${selector_or_webelement}
        ${element}=    get child element    ${parent}    ${selector_or_webelement}
    END
    [Return]    ${element}

user takes screenshot of element
    [Arguments]
    ...    ${selector_or_webelement}
    ...    ${filename}
    ...    ${wait}=${timeout}
    ...    ${limit}=None
    ${element}=    lookup or return webelement    ${selector_or_webelement}
    ${filepath}=    take screenshot of element    ${element}    ${filename}
    [Return]    ${filepath}

user takes html snapshot of element
    [Arguments]
    ...    ${selector_or_webelement}
    ...    ${filename}
    ...    ${wait}=${timeout}
    ...    ${limit}=None
    ${element}=    lookup or return webelement    ${selector_or_webelement}
    ${filepath}=    take html snapshot of element    ${element}    ${filename}
    [Return]    ${filepath}
