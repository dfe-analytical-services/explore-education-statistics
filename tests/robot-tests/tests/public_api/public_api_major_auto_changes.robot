*** Settings ***
Library             ../libs/admin_api.py
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
${PUBLICATION_NAME}=    Public API - major auto changes %{RUN_IDENTIFIER}
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

User waits until the initial data set version's status changes to "Ready"
    user waits until h3 is visible    Draft version details
    user waits until draft API data set status contains    Ready

Add headline text block to Content page
    user clicks link    Back to API data sets
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user approves release for immediate publication

Create a second draft release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year    3010

# In this new subject, the "School type" filter has been removed entirely.

Upload subject to second release
    user uploads subject and waits until complete    ${SUBJECT_2_NAME}    absence_school_major_auto.csv
    ...    absence_school_major_auto.meta.csv    ${PUBLIC_API_FILES_DIR}

Add data guidance to second release
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_2_NAME}

    user enters text into data guidance data file content editor    ${SUBJECT_2_NAME}
    ...    ${SUBJECT_2_NAME} Main guidance content

    user clicks button    Save guidance

Create a different version of an API data set with major changes
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until h3 is visible    Current live API data sets

    user checks table column heading contains    1    1    Version    testid:live-api-data-sets
    user clicks button in table cell    1    3    Create new version
    ...    testid:live-api-data-sets

    ${modal}=    user waits until modal is visible    Create a new API data set version
    user chooses select option    name:releaseFileId    ${SUBJECT_2_NAME}
    user clicks button    Confirm new data set version

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set version

Validate the summary contents inside the 'Latest live version details' table
    user waits until h3 is visible    Draft version details
    user checks summary list contains    Version    v1.0    id:live-version-summary
    user checks summary list contains    Status    Published    id:live-version-summary
    user checks summary list contains    Release    ${RELEASE_1_NAME}    id:live-version-summary
    user checks summary list contains    Data set file    ${SUBJECT_1_NAME}    id:live-version-summary
    user checks summary list contains    Geographic levels    Local authority, National, Regional, School
    ...    id:live-version-summary
    user checks summary list contains    Time periods    2020/21 to 2022/23    id:live-version-summary
    user checks summary list contains    Indicators    Enrolments    id:live-version-summary
    user checks summary list contains    Indicators    more indicators    id:live-version-summary
    user checks summary list contains    Filters    Academy type    id:live-version-summary
    user checks summary list contains    Filters    School type    id:live-version-summary
    user checks summary list contains    Actions    View live data set (opens in new tab)
    ...    id:live-version-summary

Validate the summary contents inside the 'draft version details' table
    user waits until h3 is visible    Draft version details
    user checks summary list contains    Version    v2.0    id:draft-version-summary    wait=%{WAIT_LONG}
    user checks summary list contains    Status    Action required    id:draft-version-summary
    ...    wait=%{WAIT_LONG}
    user checks summary list contains    Release    ${RELEASE_2_NAME}    id:draft-version-summary
    user checks summary list contains    Data set file    ${SUBJECT_2_NAME}    id:draft-version-summary
    user checks summary list contains    Geographic levels    Local authority, National, Regional, School
    ...    id:draft-version-summary
    user checks summary list contains    Time periods    2020/21 to 2022/23    id:draft-version-summary
    user checks summary list contains    Indicators    Enrolments    id:draft-version-summary
    user checks summary list contains    Indicators    more indicators    id:draft-version-summary
    user checks summary list contains    Filters    Academy type    id:draft-version-summary
    user checks summary list does not contain    Filters    School type    id:draft-version-summary
    user checks summary list contains    Actions    Remove draft version    id:draft-version-summary

Validate that location and filter mappings are complete
    user waits until h3 is visible    Draft version tasks
    user waits until parent contains element    testid:map-locations-task    link:Map locations
    user waits until parent contains element    id:map-locations-task-status    text:Complete
    user waits until parent contains element    testid:map-filters-task    link:Map filters
    user waits until parent contains element    id:map-filters-task-status    text:Complete

Confirm finalization of this API data set version
    user clicks button    Finalise this data set version
    user waits for caches to expire
    user waits until h2 is visible    Mappings finalised
    user waits until page contains    Draft API data set version is ready to be published

User navigates to 'changelog and guidance notes' page and update relevant details in it
    user clicks link    View changelog and guidance notes    id:draft-version-summary
    user waits until page contains    API data set changelog

    user enters text into element    name:notes
    ...    Content for the public guidance notes
    user clicks button    Save public guidance notes

    user waits until page contains    Content for the public guidance notes
    user clicks link    Back to API data set details

User clicks on 'View preview token log' link inside the 'Draft version details' section
    user clicks link    View changelog and guidance notes    id:draft-version-summary

Validate the contents in the 'API data set changelog' page.
    user waits until page contains    API data set changelog

    user waits until page contains    Content for the public guidance notes
    user clicks link    Back to API data set details

Add headline text block to Content page for the second release
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve second release
    user approves release for immediate publication

Get public second release link
    ${PUBLIC_RELEASE_2_LINK}=    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_2_LINK}

Verify newly published release is public
    user navigates to public release page    ${PUBLIC_RELEASE_2_LINK}    ${PUBLICATION_NAME}    ${RELEASE_2_NAME}

User navigates to data catalogue page
    user navigates to data catalogue page on public frontend

Search for the new data set version
    user clicks element    name:search
    user presses keys    ${PUBLICATION_NAME}
    user clicks radio    API data sets only

    user waits until page finishes loading
    user clicks radio    Newest

    user checks summary list contains    Status    This is the latest data
    ...    testid:data-set-file-summary-${SUBJECT_2_NAME}
    user checks summary list contains    Status    Available by API
    ...    testid:data-set-file-summary-${SUBJECT_2_NAME}
    user checks page contains link    ${SUBJECT_2_NAME}

User clicks on the new data set version link
    user clicks link containing text    ${SUBJECT_2_NAME}
    user waits until page finishes loading
    user waits until h1 is visible    ${SUBJECT_2_NAME}

User checks relevant headings exist on API data set details page
    user waits until h2 is visible    Data set details
    user waits until h2 is visible    Data set preview
    user waits until h2 is visible    Variables in this data set
    user waits until h2 is visible    Using this data
    user waits until h2 is visible    Using the API
    user waits until h2 is visible    API data set version history
    user waits until h2 is visible    API data set changelog

User verifies the public data guidance in the 'API data set changelog' section
    user waits until element contains    testid:public-guidance-notes    Content for the public guidance notes

User verifies major changes in the 'API data set changelog' section
    user waits until h3 is visible    Major changes for version 2.0
    ${major_changes_Section}=    get child element    id:apiChangelog    testid:major-changes

    ${school_types_filter}=    user checks changelog section contains deleted filter    ${major_changes_section}
    ...    School type

    user checks page does not contain element    testid:minor-changes


*** Keywords ***
user checks changelog section contains deleted filter
    [Arguments]
    ...    ${changes_section}
    ...    ${filter_label}
    ${deleted_filters}=    user checks element contains child element    ${changes_section}
    ...    testid:deleted-filters
    user checks element contains child element    ${deleted_filters}
    ...    xpath://li[@data-testid="deleted-item" and contains(., "${filter_label}")]
