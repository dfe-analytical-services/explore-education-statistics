*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

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

Add Related dashboards section to release content
    user clicks button    Add dashboards section
    user waits until page contains accordion section    View related dashboard(s)

    user opens accordion section    View related dashboard(s)    id:data-accordion
    user starts editing text block    id:related-dashboards-content
    user presses keys    Related dashboards test text
    user clicks button    Save & close
    user waits until element contains    id:related-dashboards-content    Edit block

Add an accordion section to release content
    user clicks button    Add new section
    user changes accordion section title    1    Test section

Add text block with link to absence glossary entry to accordion section
    user adds text block to editable accordion section    Test section    id:releaseMainContent
    ${block}=    user starts editing accordion section text block    Test section    1    id:releaseMainContent
    user presses keys    Absence
    user presses keys    CTRL+a
    user clicks element    xpath://*[@aria-label="Editor toolbar"]//button[3]    # CKEditor link button
    user waits until page contains element    css:.ck-link-form
    user presses keys    %{PUBLIC_URL}/glossary#absence
    user clicks element    css:.ck-button-save
    user clicks button    Save & close
    user waits until element contains    ${block}    Absence

Check glossary info icon appears on release preview
    user clicks radio    Preview release page
    user opens accordion section    Test section    id:releaseMainContent
    user waits until page contains button    Absence

Click glossary info icon and validate glossary entry
    user clicks button    Absence
    user waits until h2 is visible    Absence
    user checks page contains    When a pupil misses (or is absent from) at least 1 possible school session.
    user clicks button    Close
    user waits until page does not contain element    xpath://h2[text()="Absence"]
    user checks page does not contain    When a pupil misses (or is absent from) at least 1 possible school session.

Approve release
    user clicks link    Sign off
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
    user waits until h2 is visible    Absence
    user checks page contains    When a pupil misses (or is absent from) at least 1 possible school session.
    user clicks button    Close
    user waits until page does not contain element    xpath://h2[text()="Absence"]
    user checks page does not contain    When a pupil misses (or is absent from) at least 1 possible school session.
