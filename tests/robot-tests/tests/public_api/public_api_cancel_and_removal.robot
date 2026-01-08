*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/public-common.robot
Resource            ../libs/public-api-common.robot

Force Tags          Admin    PublicApi    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=    Public API - cancel and removal %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial year 3000-01
${SUBJECT_NAME_1}=      ${PUBLICATION_NAME} - Subject 1
${SUBJECT_NAME_2}=      ${PUBLICATION_NAME} - Subject 2


*** Test Cases ***
Create publication and release
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year    3000-01    Accredited official statistics

Upload datafiles
    user uploads subject and waits until complete    ${SUBJECT_NAME_1}    seven_filters.csv    seven_filters.meta.csv
    ...    ${PUBLIC_API_FILES_DIR}
    user uploads subject and waits until complete    ${SUBJECT_NAME_2}    tiny-two-filters.csv
    ...    tiny-two-filters.meta.csv    ${PUBLIC_API_FILES_DIR}

Add data guidance to subjects
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user adds main data guidance content

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_1}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_1}
    ...    ${SUBJECT_NAME_1} Main guidance content

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_2}
    ...    ${SUBJECT_NAME_2} Main guidance content

Save data guidance
    user clicks button    Save guidance

User clicks on 'Cancel' button while after selecting 1st API data set
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user clicks button    Cancel    ${modal}

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set    %{WAIT_LONG}

User creates 2nd API data set
    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user chooses select option    name:releaseFileId    ${SUBJECT_NAME_2}
    user clicks button    Confirm new API data set

    user waits until page contains    Creating API data set
    user clicks link    View API data set details

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set    %{WAIT_LONG}

User waits until the 2nd API data set status changes to 'Ready'
    user waits until h3 is visible    Draft version details
    user waits until draft API data set status contains    Ready

Verify the contents inside the 'Draft API data sets' table
    user clicks link    Back to API data sets
    user waits until h3 is visible    Draft API data sets

    user checks table column heading contains    1    1    Draft version    testid:draft-api-data-sets
    user checks table column heading contains    1    2    Name    testid:draft-api-data-sets
    user checks table column heading contains    1    3    Status    testid:draft-api-data-sets
    user checks table column heading contains    1    4    Actions    testid:draft-api-data-sets

    user checks table cell contains    1    1    v1.0    testid:draft-api-data-sets
    user checks table cell contains    1    3    Ready    testid:draft-api-data-sets

Click on 'cancel' button while attempting to remove draft API data set
    user clicks button in table cell    1    4    Remove draft    testid:draft-api-data-sets

    ${modal}=    user waits until modal is visible    Remove this draft API data set version
    user clicks button    Cancel    ${modal}
    user waits until h2 is visible    API data sets

Verify the contents inside the 'Draft API data sets' table again
    user waits until h3 is visible    Draft API data sets

    user checks table column heading contains    1    1    Draft version    testid:draft-api-data-sets
    user checks table column heading contains    1    2    Name    testid:draft-api-data-sets
    user checks table column heading contains    1    3    Status    testid:draft-api-data-sets
    user checks table column heading contains    1    4    Actions    testid:draft-api-data-sets

    user checks table cell contains    1    1    v1.0    testid:draft-api-data-sets
    user checks table cell contains    1    3    Ready    testid:draft-api-data-sets

Remove draft API data set
    user clicks button in table cell    1    4    Remove draft    testid:draft-api-data-sets

    ${modal}=    user waits until modal is visible    Remove this draft API data set version
    user clicks button    Remove this API data set version

    user waits until modal is not visible    Remove this draft API data set version
    user waits until h2 is visible    API data sets

User creates API data set again
    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user chooses select option    name:releaseFileId    ${SUBJECT_NAME_1}
    user clicks button    Confirm new API data set

    user waits until page contains    Creating API data set
    user clicks link    View API data set details

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set    %{WAIT_LONG}

User waits until the 1st API data set status changes to 'Ready'
    user waits until h3 is visible    Draft version details
    user waits until draft API data set status contains    Ready

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user approves release for immediate publication

Get public release link
    ${PUBLIC_RELEASE_LINK}=    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Verify newly published release is public
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_NAME}

User navigates to data catalogue page
    user navigates to data catalogue page on public frontend

Search for API data set
    user clicks element    id:searchForm-search
    user presses keys    ${PUBLICATION_NAME}
    user clicks radio    API data sets only

    user waits until page finishes loading
    user clicks radio    Newest
    user checks page contains link    ${SUBJECT_NAME_1}
    user checks list item contains    testid:data-set-file-list    1    ${SUBJECT_NAME_1}
