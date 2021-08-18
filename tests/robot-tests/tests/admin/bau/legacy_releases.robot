*** Settings ***
Resource            ../../libs/admin-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${PUBLICATION_NAME}     UI tests - legacy releases %{RUN_IDENTIFIER}

*** Test Cases ***
Create new publication for topic
    environment variable should be set    RUN_IDENTIFIER
    user creates test publication via api    ${PUBLICATION_NAME}

Verify new publication
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user waits until page contains button    ${PUBLICATION_NAME}

Go to legacy releases page
    user opens accordion section    ${PUBLICATION_NAME}
    user clicks element    testid:Edit publication link for ${PUBLICATION_NAME}
    user waits until page contains title caption    ${PUBLICATION_NAME}
    user waits until h1 is visible    Manage publication
    user clicks element    testid:Legacy releases link for ${PUBLICATION_NAME}
    user waits until h1 is visible    Legacy releases
    user waits until page contains title caption    ${PUBLICATION_NAME}
    user checks page does not contain element    css:table

Create legacy release
    user clicks link    Create legacy release
    user waits until h1 is visible    Create legacy release
    user enters text into element    id:legacyReleaseForm-description    Test collection
    user enters text into element    id:legacyReleaseForm-url    http://test.com
    user clicks button    Save legacy release

Validate created legacy release
    user waits until h1 is visible    Legacy releases
    user checks element count is x    css:tbody tr    1
    user checks results table cell contains    1    1    1
    user checks results table cell contains    1    2    Test collection
    user checks results table cell contains    1    3    http://test.com

Update legacy release
    user clicks element    xpath://tr[1]//*[text()="Edit release"]
    user waits until h1 is visible    Edit legacy release
    user enters text into element    id:legacyReleaseForm-description    Test collection 2
    user enters text into element    id:legacyReleaseForm-url    http://test-2.com
    user enters text into element    id:legacyReleaseForm-order    2
    user clicks button    Save legacy release

Validate updated legacy release
    user waits until h1 is visible    Legacy releases
    user checks element count is x    css:tbody tr    1
    # Changing order to 2 should not actually change it to 2
    # as there isn't another release to swap the order with.
    user checks results table cell contains    1    1    1
    user checks results table cell contains    1    2    Test collection 2
    user checks results table cell contains    1    3    http://test-2.com

Delete legacy release
    user clicks element    xpath://tr[1]//*[text()="Delete release"]
    user clicks button    Confirm
    user waits until page does not contain element    css:table

Create multiple legacy releases
    user creates legacy release    Test collection 1    http://test-1.com
    user creates legacy release    Test collection 2    http://test-2.com
    user creates legacy release    Test collection 3    http://test-3.com

Validate legacy release order
    user checks element count is x    css:tbody tr    3

    user checks results table cell contains    1    1    3
    user checks results table cell contains    1    2    Test collection 3
    user checks results table cell contains    1    3    http://test-3.com

    user checks results table cell contains    2    1    2
    user checks results table cell contains    2    2    Test collection 2
    user checks results table cell contains    2    3    http://test-2.com

    user checks results table cell contains    3    1    1
    user checks results table cell contains    3    2    Test collection 1
    user checks results table cell contains    3    3    http://test-1.com

Reorder legacy releases
    user clicks button    Reorder legacy releases
    user waits until page contains button    Confirm order
    user sets focus to element    css:tbody tr:first-child
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}
    user clicks button    Confirm order

Validate reordered legacy releases
    user waits until page contains button    Reorder legacy releases
    user checks element count is x    css:tbody tr    3

    user checks results table cell contains    1    1    3
    user checks results table cell contains    1    2    Test collection 2
    user checks results table cell contains    1    3    http://test-2.com

    user checks results table cell contains    2    1    2
    user checks results table cell contains    2    2    Test collection 1
    user checks results table cell contains    2    3    http://test-1.com

    user checks results table cell contains    3    1    1
    user checks results table cell contains    3    2    Test collection 3
    user checks results table cell contains    3    3    http://test-3.com

*** Keywords ***
user creates legacy release
    [Arguments]    ${description}    ${url}
    user clicks link    Create legacy release
    user waits until h1 is visible    Create legacy release
    user enters text into element    id:legacyReleaseForm-description    ${description}
    user enters text into element    id:legacyReleaseForm-url    ${url}
    user clicks button    Save legacy release
    user waits until h1 is visible    Legacy releases
