*** Settings ***
Library             ../libs/admin_api.py
Library             ../libs/dates_and_times.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/public-common.robot
Resource            ../libs/public-api-common.robot

Force Tags          Admin    PublicApi    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required
Test Teardown       Run Keyword If Test Failed    record test failure


*** Variables ***
${PUBLICATION_NAME}=    Public API - patch manual changes %{RUN_IDENTIFIER}
${RELEASE_1_NAME}=      Financial year 3000-01
${SUBJECT_1_NAME}=      ${PUBLICATION_NAME} - Subject 1


*** Test Cases ***
Create publication and release
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_1_NAME}

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year    3000-01    Accredited official statistics

Upload datafile
    user uploads subject and waits until complete    ${SUBJECT_1_NAME}    absence_school.csv    absence_school.meta.csv
    ...    ${PUBLIC_API_FILES_DIR}

Add data guidance to subjects
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user adds main data guidance content

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_1_NAME}

    user enters text into data guidance data file content editor    ${SUBJECT_1_NAME}
    ...    ${SUBJECT_1_NAME} Main guidance content

    user clicks button    Save guidance

Create the initial API data set version
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user chooses select option    name:releaseFileId    ${SUBJECT_1_NAME}
    user clicks button    Confirm new API data set

    user waits until page contains    Creating API data set
    user clicks link    View API data set details

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set

User waits until the initial API data set version's status changes to "Ready"
    user waits until h3 is visible    Draft version details
    user waits until draft API data set status contains    Ready

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user approves release for immediate publication

Create amendment for patch replacement
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_1_NAME}

Upload patch replacement data
    user clicks link    Data and files
    user waits until page contains data uploads table
    user uploads subject replacement    ${SUBJECT_1_NAME}
    ...    absence_school_minor_manual.csv    absence_school_minor_manual.meta.csv
    ...    ${PUBLIC_API_FILES_DIR}

    user waits until page contains element    testid:Data file replacements table
    user confirms replacement upload    ${SUBJECT_1_NAME}    Error
    user clicks link in table cell    1    4    View details    testid:Data file replacements table

    user waits until page contains element    testid:Replacement Title
    user checks table column heading contains    1    1    Original file
    user checks table column heading contains    1    2    Replacement file
    user checks headed table body row cell contains    Data file import status    2    Complete
    user checks headed table body row cell contains    API data set status    2    Action required
    ...    wait=%{WAIT_DATA_FILE_IMPORT}

Verify the pending data replacement summary
    user waits until h3 is visible    API data set Locations: ERROR
    user waits until h3 is visible    API data set Filters: ERROR
    user waits until h3 is visible    API data set has to be finalized: ERROR
    user waits until parent contains element    id:main-content    text:Cancel data replacement

Validate error summary is displayed on Api Data Set Details page
    user clicks link    go to the API data sets tab
    user waits until h2 is visible
    ...    This API data set can not be published because location or filter mappings are not yet complete.

User clicks on Map locations link
    user clicks link    Map locations
    user waits until h3 is visible    Locations not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-region"] caption
    ...    1 unmapped location    %{WAIT_LONG}

Validate the 'unmapped location' notification banner
    user waits until h2 is visible    Action required
    user waits until page contains link    There is 1 unmapped region

User edits location mapping
    user clicks button in table cell    1    4    Map option

    ${modal}=    user waits until modal is visible    Map existing location
    user clicks radio    Yorkshire
    user clicks button    Update location mapping
    user waits until modal is not visible    Map existing location

Verify location mapping changes
    user waits until element contains    css:[data-testid="mappable-table-region"] caption
    ...    1 mapped location    %{WAIT_LONG}
    user waits until h3 is visible    Locations not found in new data set
    user clicks link    Back
    user waits until h3 is visible    Draft version tasks

User clicks on Map filters link
    user clicks link    Map filters
    user waits until h3 is visible    Filter options not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-schoolType"] caption
    ...    1 unmapped filter option    %{WAIT_LONG}

Validate the 'unmapped filter option' notification banner
    user waits until h2 is visible    Action required
    user waits until page contains link    There is 1 unmapped School type filter option

User edits filter mapping
    user clicks button in table cell    1    4    Map option

    ${modal}=    user waits until modal is visible    Map existing filter option
    user clicks radio    State-funded primary and secondary
    user clicks button    Update filter option mapping
    user waits until modal is not visible    Map existing filter option

Verify filter mapping changes
    user waits until element contains    css:[data-testid="mappable-table-schoolType"] caption
    ...    1 mapped filter option    %{WAIT_LONG}

Validate the row headings and its contents in the 'filters options' section after mapping
    user waits until h3 is visible    Filter options not found in new data set
    user clicks link    Back

Confirm finalization of this API data set version
    user clicks button    Finalise this data set version
    user waits for caches to expire
    user waits until h2 is visible    Mappings finalised
    user waits until page contains    Draft API data set version is ready to be published

Verify that API summary tags have status OK and then press 'confirm data replacement'
    user clicks link    Back to API data sets
    user clicks link    Data uploads
    user clicks link containing text    View details    testId:Actions
    user waits until h3 is visible    API data set Locations: OK
    user waits until h3 is visible    API data set Filters: OK
    user waits until h3 is visible    API data set has to be finalized: OK
    user waits until parent contains element    id:main-content    text:Cancel data replacement
    user waits until parent contains element    id:main-content    text:Confirm data replacement
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete

Navigate to 'Content' page for amendment
    user clicks link    Back
    user navigates to content page    ${PUBLICATION_NAME}

Add release note for amendment
    User clicks button    Add note    id:release-notes
    user enters text into element    id:create-release-note-form-reason    Test release note
    user clicks button    Save note
    ${date}=    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note

Approve amendment release
    user approves release for immediate publication

Create second amendment for patch replacement
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_1_NAME}

Upload second patch replacement data
    user clicks link    Data and files
    user waits until page contains data uploads table
    user uploads subject replacement    ${SUBJECT_1_NAME}
    ...    absence_school_patch_manual.csv    absence_school_patch_manual.meta.csv
    ...    ${PUBLIC_API_FILES_DIR}
    user waits until page contains element    testid:Data file replacements table
    user confirms replacement upload    ${SUBJECT_1_NAME}    Error
    user clicks link in table cell    1    4    View details    testid:Data file replacements table

    user waits until page contains element    testid:Replacement Title
    user checks table column heading contains    1    1    Original file
    user checks table column heading contains    1    2    Replacement file
    user checks headed table body row cell contains    Data file import status    2    Complete
    user checks headed table body row cell contains    API data set status    2    Action required
    ...    wait=%{WAIT_DATA_FILE_IMPORT}

Verify the pending data replacement summary for second patch replacement
    user waits until h3 is visible    API data set Locations: ERROR
    user waits until h3 is visible    API data set Filters: OK
    user waits until h3 is visible    API data set has to be finalized: ERROR
    user waits until parent contains element    id:main-content    text:Cancel data replacement

Validate error summary is displayed on Api Data Set Details page for second patch replacement
    user clicks link    go to the API data sets tab
    user waits until h2 is visible
    ...    This API data set can not be published because location or filter mappings are not yet complete.
    user checks element is visible    testid:cancel-replacement-link

Validate the summary contents inside the 'draft version details' table contains version 1.0.2
    user waits until h3 is visible    Draft version details
    user checks summary list contains    Version    v1.0.2    id:draft-version-summary    wait=%{WAIT_LONG}

User clicks on Map locations link for second patch replacement
    user clicks link    Map locations
    user waits until h3 is visible    Locations not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-localAuthority"] caption
    ...    1 unmapped location    %{WAIT_LONG}

User edits location mapping for second patch replacement
    user clicks button in table cell    1    4    Map option

    ${modal}=    user waits until modal is visible    Map existing location
    user clicks radio    Hull
    user clicks button    Update location mapping
    user waits until modal is not visible    Map existing location

Verify location mapping changes for second patch replacement
    user waits until element contains    css:[data-testid="mappable-table-localAuthority"] caption
    ...    1 mapped location    %{WAIT_LONG}
    user waits until h3 is visible    Locations not found in new data set
    user clicks link    Back
    user waits until h3 is visible    Draft version tasks

Confirm finalization of this API data set version for second patch replacement
    user clicks button    Finalise this data set version
    user waits for caches to expire
    user waits until h2 is visible    Mappings finalised
    user waits until page contains    Draft API data set version is ready to be published

Verify that API summary tags have status OK and then press 'confirm data replacement' for second patch replacement
    user clicks link    Back to API data sets
    user clicks link    Data uploads
    user clicks link containing text    View details    testId:Actions
    user waits until h3 is visible    API data set Locations: OK
    user waits until h3 is visible    API data set Filters: OK
    user waits until h3 is visible    API data set has to be finalized: OK
    user waits until parent contains element    id:main-content    text:Cancel data replacement
    user waits until parent contains element    id:main-content    text:Confirm data replacement
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete

Navigate to 'Content' page for amendment for second patch replacement
    user clicks link    Back
    user navigates to content page    ${PUBLICATION_NAME}

Add release note for amendment for second patch replacement
    User clicks button    Add note    id:release-notes
    user enters text into element    id:create-release-note-form-reason    Test release note
    user clicks button    Save note
    ${date}=    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note

Approve amendment release for second patch replacement
    user approves release for immediate publication

Get public release link
    ${PUBLIC_RELEASE_LINK}=    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Verify newly published release is public
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_1_NAME}

Navigate to api data set on public frontend website
    user clicks link    Data catalogue
    user waits until h1 is visible    Data catalogue
    user clicks link    ${PUBLICATION_NAME} - Subject 1
    user waits until h2 is visible    Data set details

Verify the highest patch version is displayed in the data set public frontend page
    user waits until element is visible    xpath://div[span[text()='API data set version'] and contains(., '1.0.2')]

Verify that the two patch versions are collated on a single page
    user clicks link    API data set changelog
    user waits until h3 is visible    Patch changes for version 1.0.2
    user waits until h3 is visible    Patch changes for version 1.0.1
    # This change log is from 1.0.2
    user waits until li is visible    label changed to: Hull
    # This change log is from 1.0.1
    user waits until li is visible    label changed to: State-funded primary and secondary
    user waits until li is visible    label changed to: Yorkshire


*** Keywords ***
user waits until li is visible
    [Arguments]    ${text}    ${wait}=${timeout}    ${exact_match}=${False}
    ${text_matcher}=    get xpath text matcher    ${text}    ${exact_match}
    user waits until element is visible    xpath://li[${text_matcher}]    ${wait}
