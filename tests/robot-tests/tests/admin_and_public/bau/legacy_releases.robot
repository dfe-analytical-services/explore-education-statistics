*** Settings ***
Resource            ../../libs/admin-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=        UI tests - legacy releases %{RUN_IDENTIFIER}
${DESCRIPTION}=             legacy release description
${UPDATED_DESCRIPTION}=     updated legacy release description


*** Test Cases ***
Create new publication and release via api
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2020

Create legacy release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases
    user clicks button    Create legacy release

    ${modal}=    user waits until modal is visible    Create legacy release
    user clicks button    OK    ${modal}

    user waits until page contains element    id:legacyReleaseForm-description
    user enters text into element    id:legacyReleaseForm-description    ${DESCRIPTION}
    user enters text into element    id:legacyReleaseForm-url    http://test.com
    user clicks button    Save legacy release

Validate created legacy release
    user waits until h2 is visible    Legacy releases
    user checks element count is x    css:tbody tr    1
    user checks table cell contains    1    1    1
    user checks table cell contains    1    2    ${DESCRIPTION}
    user checks table cell contains    1    3    http://test.com

Navigate to admin dashboard to create new release
    user navigates to admin dashboard    Bau1

Navigate to release in admin
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2020/21

Approve release
    user clicks link    Sign off
    user approves original release for immediate publication
    user waits until page contains element    testid:public-release-url
    ${PUBLIC_RELEASE_LINK}=    Get Value    xpath://*[@data-testid="public-release-url"]
    check that variable is not empty    PUBLIC_RELEASE_LINK    ${PUBLIC_RELEASE_LINK}
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Check legacy release appears on public frontend
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user opens details dropdown    View releases (1)

    ${other_releases}=    user gets details content element    View releases (1)

    user checks list has x items    css:ul    1    ${other_releases}

    ${other_release_1}=    user gets list item element    css:ul    1    ${other_releases}
    ${other_release_1_link}=    get child element    ${other_release_1}    link:${DESCRIPTION}
    user checks element attribute value should be    ${other_release_1_link}    href    http://test.com/

Navigate to publication to update legacy releases
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases

Update legacy release
    user clicks element    xpath://tr[1]//*[text()="Edit"]
    ${modal}=    user waits until modal is visible    Edit legacy release
    user clicks button    OK    ${modal}

    user waits until page contains element    id:legacyReleaseForm-description
    user enters text into element    id:legacyReleaseForm-description    ${UPDATED_DESCRIPTION}
    user enters text into element    id:legacyReleaseForm-url    http://test2.com
    user clicks button    Save legacy release
    sleep    %{WAIT_MEMORY_CACHE_EXPIRY}

Validate updated legacy release
    user waits until h2 is visible    Legacy releases
    user checks element count is x    css:tbody tr    1
    user checks table cell contains    1    1    1
    user checks table cell contains    1    2    ${UPDATED_DESCRIPTION}
    user checks table cell contains    1    3    http://test2.com

Validate public frontend shows changes made to legacy release after saving publication
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}

    user opens details dropdown    View releases (1)

    ${other_releases}=    user gets details content element    View releases (1)

    user checks list has x items    css:ul    1    ${other_releases}

    ${other_release_1}=    user gets list item element    css:ul    1    ${other_releases}

    ${other_release_1_link}=    get child element    ${other_release_1}    link:${UPDATED_DESCRIPTION}
    user checks element attribute value should be    ${other_release_1_link}    href    http://test2.com/

Navigate to publication to update legacy releases again
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user clicks link    Legacy releases
    user waits until h2 is visible    Legacy releases

Delete legacy release
    ${ROW}=    user gets table row    1
    user clicks element    xpath://*[text()="Delete"]    ${ROW}

    ${modal}=    user waits until modal is visible    Delete legacy release
    user clicks button    Confirm    ${modal}

    user waits until page does not contain element    css:table

Create multiple legacy releases
    user creates legacy release    Test collection 1    http://test-1.com
    user creates legacy release    Test collection 2    http://test-2.com
    user creates legacy release    Test collection 3    http://test-3.com

Validate legacy release order
    user checks element count is x    css:tbody tr    3

    user checks table cell contains    1    1    3
    user checks table cell contains    1    2    Test collection 3
    user checks table cell contains    1    3    http://test-3.com

    user checks table cell contains    2    1    2
    user checks table cell contains    2    2    Test collection 2
    user checks table cell contains    2    3    http://test-2.com

    user checks table cell contains    3    1    1
    user checks table cell contains    3    2    Test collection 1
    user checks table cell contains    3    3    http://test-1.com

Reorder legacy releases
    user clicks button    Reorder legacy releases
    user waits until modal is visible    Reorder legacy releases
    user clicks button    OK
    user waits until modal is not visible    Reorder legacy releases
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

    user checks table cell contains    1    1    3
    user checks table cell contains    1    2    Test collection 2
    user checks table cell contains    1    3    http://test-2.com

    user checks table cell contains    2    1    2
    user checks table cell contains    2    2    Test collection 1
    user checks table cell contains    2    3    http://test-1.com

    user checks table cell contains    3    1    1
    user checks table cell contains    3    2    Test collection 3
    user checks table cell contains    3    3    http://test-3.com


*** Keywords ***
user creates legacy release
    [Arguments]    ${description}    ${url}
    user clicks button    Create legacy release

    ${modal}=    user waits until modal is visible    Create legacy release
    user clicks button    OK    ${modal}

    user waits until page contains element    id:legacyReleaseForm-description
    user enters text into element    id:legacyReleaseForm-description    ${description}
    user enters text into element    id:legacyReleaseForm-url    ${url}
    user clicks button    Save legacy release
    user waits until h2 is visible    Legacy releases

    user waits until page contains    ${description}
    user checks page contains    ${url}
