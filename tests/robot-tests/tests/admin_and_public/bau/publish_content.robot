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
${RELEASE_NAME}=        Calendar Year 2001

*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    CY    2001

Navigate to release content
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME} (not Live)
    user clicks link    Content
    user waits until page contains button    Add new section

Add an accordion section to release content
    user clicks button    Add new section
    user changes accordion section title    1    Test section

Add text block with link to absence glossary entry to accordion section
    user adds text block to editable accordion section    Test section    css:#releaseMainContent
    #user adds content to accordion section text block    Test section    1    Test. <a href=%{PUBLIC_URL}/glossary#absence>Absence</a>. Test.    css:#releaseMainContent
    ${block}=    user edits accordion section text block    Test section    1    css:#releaseMainContent
    user presses keys    Absence
    user presses keys    CTRL+a
    user clicks element    xpath://*[@aria-label="Editor toolbar"]//button[3]    # CKEditor link button
    user waits until page contains element    css:.ck-link-form
    user presses keys    %{PUBLIC_URL}/glossary#absence
    user clicks element    css:.ck-button-save
    user clicks button    Save
    user waits until element contains    ${block}    Absence

Check glossary info icon appears on release preview
    user clicks radio    Preview release page
    user opens accordion section    Test section    css:#releaseMainContent
    user waits until page contains link    Absence
    user waits until page contains testid    glossary-info-icon

Click glossary info icon and validate glossary entry
    user clicks element    testid:glossary-info-icon
    user waits until h1 is visible    Absence
    user checks page contains    When a pupil misses (or is absent from) at least 1 possible school session.
    user clicks link    Close
    user waits until page does not contain element    xpath://h1[text()="Absence"]
    user checks page does not contain    When a pupil misses (or is absent from) at least 1 possible school session.

Approve release
    user clicks link    Sign off
    user approves original release for immediate publication

User goes to public Find Statistics page
    user navigates to find statistics page on public frontend

Verify newly published release is on Find Statistics page
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until accordion section contains text    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}

    user opens details dropdown    %{TEST_TOPIC_NAME}
    user waits until details dropdown contains publication    %{TEST_TOPIC_NAME}    ${PUBLICATION_NAME}
    user checks publication bullet contains link    ${PUBLICATION_NAME}    View statistics and data
    user checks publication bullet contains link    ${PUBLICATION_NAME}    Create your own tables
    user checks publication bullet does not contain link    ${PUBLICATION_NAME}    Statistics at DfE

Navigate to published release page
    user clicks element    testid:View stats link for ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}    90

Check latest release contains glossary info icon
    user waits until page contains title caption    ${RELEASE_NAME}
    user checks page contains    This is the latest data
    user opens accordion section    Test section    css:#content
    user waits until page contains link    Absence
    user waits until page contains element    testid:glossary-info-icon

Click glossary info icon and verify entry is correct
    user clicks element    testid:glossary-info-icon
    user waits until h1 is visible    Absence
    user checks page contains    When a pupil misses (or is absent from) at least 1 possible school session.
    user clicks link    Close
    user waits until page does not contain element    xpath://h1[text()="Absence"]
    user checks page does not contain    When a pupil misses (or is absent from) at least 1 possible school session.
