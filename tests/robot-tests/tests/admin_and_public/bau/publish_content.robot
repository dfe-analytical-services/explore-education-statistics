*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    UI tests - publish content %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Calendar year 2001


*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2001    AdHocStatistics

Navigate to release content
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    user clicks link    Content
    user waits until page contains button    Add dashboards section
    user waits until page contains button    Add new section

Add headline text block to release content
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Add Related dashboards section to release content
    user clicks button    Add dashboards section
    user waits until page contains accordion section    View related dashboard(s)

    user opens accordion section    View related dashboard(s)    id:data-accordion
    user adds content to related dashboards text block    Related dashboards test text

Add an accordion section to release content
    user clicks button    Add new section
    user changes accordion section title    1    Test section

Add text block with link to absence glossary entry to accordion section
    user adds text block to editable accordion section    Test section    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${block}=    user starts editing accordion section text block    Test section    1
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${toolbar}=    get editor toolbar    ${block}
    ${insert}=    get child element    parent_locator=${toolbar}
    ...    child_locator=css:[data-cke-tooltip-text="Insert"]
    user clicks element    ${insert}
    ${button}=    user gets button element    Insert glossary link    ${toolbar}
    user clicks element    ${button}
    ${modal}=    user waits until modal is visible    Insert glossary link
    user enters text into element    id:glossarySearch-input    abs
    user waits until page contains element    id:glossarySearch-options
    user clicks element    id:glossarySearch-option-0
    user clicks button    Insert    ${modal}
    user waits until modal is not visible    Insert glossary link
    user clicks button    Save & close    ${block}
    user waits until parent contains button    ${block}    Absence

Check glossary info icon appears on release preview
    user clicks radio    Preview release page
    user opens accordion section    Test section    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user waits until page contains button    Absence

Click glossary info icon and validate glossary entry
    user closes admin feedback banner if needed

    user clicks button    Absence
    ${modal}=    user waits until modal is visible    Absence
    user checks page contains    When a pupil misses (or is absent from) at least 1 possible school session.
    user clicks button    Close    ${modal}
    user waits until page does not contain element    xpath://h2[text()="Absence"]
    user checks page does not contain    When a pupil misses (or is absent from) at least 1 possible school session.

Approve release
    user approves original release for immediate publication

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

Navigate to published release page
    user clicks link    ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_MEDIUM}

    user waits until page contains title caption    ${RELEASE_NAME}    %{WAIT_MEDIUM}
    user checks page contains    This is the latest data

Check latest release contains related dashboards section
    user checks there are x accordion sections    1    id:data-accordion
    user checks accordion is in position    View related dashboard(s)    1    id:data-accordion
    user checks element contains    id:related-dashboards-content    Related dashboards test text

Check quick links navigation contains link to related dashboards
    user checks element contains link    testid:quick-links    View related dashboard(s)

Check related dashboard link opens accordion section
    user verifies accordion is closed    View related dashboard(s)
    user clicks link    View related dashboard(s)    testid:quick-links
    user verifies accordion is open    View related dashboard(s)
    user closes accordion section    View related dashboard(s)    id:data-accordion

Check latest release contains glossary info icon
    user opens accordion section    Test section    css:#content
    user waits until page contains button    Absence

Click glossary info icon and verify entry is correct
    user clicks button    Absence
    ${modal}=    user waits until modal is visible    Absence
    user waits until h2 is visible    Absence
    user checks page contains    When a pupil misses (or is absent from) at least 1 possible school session.
    user clicks button    Close    ${modal}
    user waits until page does not contain element    xpath://h2[text()="Absence"]
    user checks page does not contain    When a pupil misses (or is absent from) at least 1 possible school session.
