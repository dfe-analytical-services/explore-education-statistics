*** Settings ***
Library             ../libs/admin_api.py
Library             ../libs/dates_and_times.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/public-common.robot
Resource            ../libs/public-api-common.robot

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required
Test Teardown       Run Keyword If Test Failed    record test failure


*** Variables ***
${PUBLICATION_NAME}=    Public API - minor manual changes %{RUN_IDENTIFIER}
${RELEASE_1_NAME}=      Financial year 3000-01
${RELEASE_2_NAME}=      Academic year 3010/11
${SUBJECT_1_NAME}=      ${PUBLICATION_NAME} - Subject 1
${SUBJECT_2_NAME}=      ${PUBLICATION_NAME} - Subject 2


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

Check modal that blocks replacing a draft patch data set version is displayed
    user clicks link    Back to API data sets
    user clicks link    Data uploads
    user reloads page
    user waits until page contains data uploads table
    user clicks button    Replace data    testId:Actions
    ${modal}=    user waits until modal is visible    Cannot replace data
    user clicks button    Close
    user waits until modal is not visible    Create a new API data set

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user approves release for immediate publication

Create amendment for patch replacement
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_1_NAME}

Navigate to data replacement page
    user clicks link    Data and files
    user waits until page contains data uploads table
    user clicks link    Replace data    testId:Actions

Upload replacement data
    user waits until h2 is visible    Upload replacement data    %{WAIT_MEDIUM}
    user chooses file    id:dataFileUploadForm-dataFile    ${PUBLIC_API_FILES_DIR}absence_school_minor_manual.csv
    user chooses file    id:dataFileUploadForm-metadataFile
    ...    ${PUBLIC_API_FILES_DIR}absence_school_minor_manual.meta.csv
    user clicks button    Upload data files

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
    ...    This API data set can not be published because it has incomplete location or filter manual mapping.
    user checks element is visible    testid:cancel-replacement-link

Validate the summary contents inside the 'Latest live version details' table
    user waits until h3 is visible    Latest live version details
    user checks summary list contains    Version    v1.0    id:live-version-summary
    user checks summary list contains    Status    Published    id:live-version-summary
    user checks summary list contains    Release    ${RELEASE_1_NAME}    id:live-version-summary
    user checks summary list contains    Data set file    ${SUBJECT_1_NAME}    id:live-version-summary
    user checks summary list contains    Geographic levels    Local authority, National, Regional, School
    ...    id:live-version-summary
    user checks summary list contains    Time periods    2020/21 to 2022/23    id:live-version-summary
    user checks summary list contains    Indicators    Enrolments    id:live-version-summary
    user checks summary list contains    Indicators    Number of authorised sessions    id:live-version-summary
    user checks summary list contains    Indicators    Number of possible sessions    id:live-version-summary
    user checks summary list contains    Filters    Academy type    id:live-version-summary
    user checks summary list contains    Filters    National Curriculum year    id:live-version-summary
    user checks summary list contains    Actions    View live data set (opens in new tab)
    ...    id:live-version-summary

Validate the summary contents inside the 'draft version details' table
    user waits until h3 is visible    Draft version details
    user checks summary list contains    Version    v1.0.1    id:draft-version-summary    wait=%{WAIT_LONG}
    user checks summary list contains    Status    Action required    id:draft-version-summary
    ...    wait=%{WAIT_LONG}
    user checks summary list contains    Release    ${RELEASE_1_NAME}    id:draft-version-summary
    user checks summary list contains    Data set file    ${SUBJECT_1_NAME}    id:draft-version-summary
    user checks summary list contains    Geographic levels    Local authority, National, Regional, School
    ...    id:draft-version-summary
    user checks summary list contains    Time periods    2020/21 to 2022/23    id:draft-version-summary
    user checks summary list contains    Indicators    Enrolments    id:draft-version-summary
    user checks summary list contains    Indicators    Number of authorised sessions    id:draft-version-summary
    user checks summary list contains    Indicators    Number of possible sessions    id:draft-version-summary
    user checks summary list contains    Filters    Academy type    id:draft-version-summary
    user checks summary list contains    Filters    National Curriculum year    id:draft-version-summary

Validate the version task statuses inside the 'Draft version task' section
    user waits until h3 is visible    Draft version tasks
    user waits until parent contains element    testid:map-locations-task    link:Map locations
    user waits until parent contains element    id:map-locations-task-status    text:Incomplete
    user waits until parent contains element    testid:map-filters-task    link:Map filters
    user waits until parent contains element    id:map-filters-task-status    text:Incomplete

User clicks on Map locations link
    user clicks link    Map locations
    user waits until h3 is visible    Locations not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-region"] caption
    ...    1 unmapped location    %{WAIT_LONG}

Validate the 'unmapped location' notification banner
    user waits until h2 is visible    Action required
    user waits until page contains link    There is 1 unmapped region

Validate the row headings and its contents in the 'Regions' section
    user checks table column heading contains    1    1    Current data set
    user checks table column heading contains    1    2    New data set

    user checks table column heading contains    1    3    Type
    user checks table column heading contains    1    4    Actions

    user checks table cell contains    1    1    Yorkshire and The Humber
    user checks table cell contains    1    2    Unmapped
    user checks table cell contains    1    3    N/A

User edits location mapping
    user clicks button in table cell    1    4    Map option

    ${modal}=    user waits until modal is visible    Map existing location
    user clicks radio    Yorkshire
    user clicks button    Update location mapping
    user waits until modal is not visible    Map existing location

Verify location mapping changes
    user waits until element contains    css:[data-testid="mappable-table-region"] caption
    ...    1 mapped location    %{WAIT_LONG}

Validate the row headings and its contents in the 'Regions' section after mapping
    user waits until h3 is visible    Locations not found in new data set
    user checks table column heading contains    1    1    Current data set
    user checks table column heading contains    1    2    New data set

    user checks table column heading contains    1    3    Type
    user checks table column heading contains    1    4    Actions

    user checks table cell contains    1    1    Yorkshire and The Humber
    user checks table cell contains    1    2    Yorkshire
    user checks table cell contains    1    3    Minor

    user clicks link    Back

Validate the version status of location task is now complete
    user waits until h3 is visible    Draft version tasks
    user waits until parent contains element    testid:map-locations-task    link:Map locations
    user waits until parent contains element    id:map-locations-task-status    text:Complete
    user waits until parent contains element    testid:map-filters-task    link:Map filters
    user waits until parent contains element    id:map-filters-task-status    text:Incomplete

User clicks on Map filters link
    user clicks link    Map filters
    user waits until h3 is visible    Filter options not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-schoolType"] caption
    ...    1 unmapped filter option    %{WAIT_LONG}

Validate the 'unmapped filter option' notification banner
    user waits until h2 is visible    Action required
    user waits until page contains link    There is 1 unmapped School type filter option

Validate the row headings and its contents in the 'filter options' section
    user checks table column heading contains    1    1    Current data set
    user checks table column heading contains    1    2    New data set

    user checks table column heading contains    1    3    Type
    user checks table column heading contains    1    4    Actions

    user checks table cell contains    1    1    Total
    user checks table cell contains    1    2    Unmapped
    user checks table cell contains    1    3    N/A

User edits filter mapping
    user clicks button in table cell    1    4    Map option

    ${modal}=    user waits until modal is visible    Map existing filter option
    user clicks radio    State-funded primary and secondary
    user clicks button    Update filter option mapping
    user waits until modal is not visible    Map existing location

Verify filter mapping changes
    user waits until element contains    css:[data-testid="mappable-table-schoolType"] caption
    ...    1 mapped filter option    %{WAIT_LONG}

Validate the row headings and its contents in the 'filters options' section after mapping
    user waits until h3 is visible    Filter options not found in new data set
    user checks table column heading contains    1    1    Current data set
    user checks table column heading contains    1    2    New data set

    user checks table column heading contains    1    3    Type
    user checks table column heading contains    1    4    Actions

    user checks table cell contains    1    1    Total
    user checks table cell contains    1    2    State-funded primary and secondary
    user checks table cell contains    1    3    Minor

    user clicks link    Back

Confirm finalization of this API data set version
    user clicks button    Finalise this data set version
    user waits for caches to expire
    user waits until h2 is visible    Mappings finalised
    user waits until page contains    Draft API data set version is ready to be published

Complete replacement by verifying and confirming the PUBLICATION_NAME
    user clicks link    Back to API data sets
    user clicks link    Data uploads
    user clicks link    View details    testId:Actions
    user waits until h3 is visible    API data set Locations: OK
    user waits until h3 is visible    API data set Filters: OK
    user waits until h3 is visible    API data set has to be finalized: OK
    user waits until parent contains element    id:main-content    text:Cancel data replacement
    user waits until parent contains element    id:main-content    text:Confirm data replacement
    user clicks button    Confirm data replacement

Navigate to 'Content' page for amendment
    user clicks link    Back
    user navigates to content page    ${PUBLICATION_NAME}

Update free text key stat
    User clicks button    Add note    id:release-notes
    user enters text into element    id:create-release-note-form-reason    Test release note
    user clicks button    Save note
    ${date}=    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note

Approve amendment release
    user approves release for immediate publication
