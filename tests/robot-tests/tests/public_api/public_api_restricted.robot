
*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=    UI tests - public api restricted %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial year 3000-01
${SUBJECT_NAME_1}=      UI test subject 1
${SUBJECT_NAME_2}=      UI test subject 2
${SUBJECT_NAME_3}=      UI test subject 3
${SUBJECT_NAME_4}=      UI test subject 4
${STATUS_XPATH}    xpath=(//div[@data-testid="Status"]//dd[@data-testid="Status-value"]//strong)[2]


*** Test Cases ***
Create publication
   ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year    3000-01    Accredited official statistics

Create new release with data files

    user uploads subject    ${SUBJECT_NAME_1}    seven_filters.csv    seven_filters.meta.csv
    user uploads subject    ${SUBJECT_NAME_2}    tiny-two-filters.csv    tiny-two-filters.meta.csv

Add data guidance to subjects
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_1}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_1}
    ...    ${SUBJECT_NAME_1} Main guidance content

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_2}
    ...    ${SUBJECT_NAME_2} Main guidance content

Save data guidance
    user clicks button    Save guidance

Add 1st API dataset
    User Scrolls To The Top Of The Page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets


    user clicks button    Create API data set
    ${modal}=    User Waits Until Modal Is Visible    Create a new API data set
    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_1}
    user clicks button    Confirm new API data set

    user waits until page finishes loading

    user waits until modal is not visible    Create a new API data set    %{WAIT_LONG}

User waits until the 1st API dataset status changes to 'Ready'
    user waits until h3 is visible    Draft version details
    wait until keyword succeeds    10x    5s    Check Status Is Ready

Add 2nd API dataset
    
    user clicks link    Back to API data sets
    user clicks button    Create API data set
    ${modal}=    User Waits Until Modal Is Visible    Create a new API data set
    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_2}
    user clicks button    Confirm new API data set

    user waits until page finishes loading

    user waits until modal is not visible    Create a new API data set    %{WAIT_LONG}

User waits until the 2nd API dataset status changes to 'Ready'
    user waits until h3 is visible    Draft version details
    wait until keyword succeeds    10x    5s    Check Status Is Ready

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user clicks link    Sign off
    user approves release for immediate publication





*** Keywords ***
user edits release status
    user clicks link    Sign off
    user waits until h2 is visible    Sign off    %{WAIT_SMALL}

    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}


user checks checklist errors contains
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until element contains    testid:releaseChecklist-errors    ${text}

user checks checklist errors contains link
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until parent contains element    testid:releaseChecklist-errors    link:${text}

Check Status Is Ready
    ${status_value}=    Get Text    ${STATUS_XPATH}
    Should Be Equal As Strings    ${status_value}    Ready
