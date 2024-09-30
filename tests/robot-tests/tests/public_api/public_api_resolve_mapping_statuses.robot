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



*** Variables ***
${PUBLICATION_NAME}=    UI tests - public api resolve mapping statuses %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial year 3000-01
${SUBJECT_NAME_1}=      UI test subject 1
${SUBJECT_NAME_2}=      UI test subject 2



*** Test Cases ***
Create publication and release
   ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year    3000-01    Accredited official statistics

Upload datafile
    user uploads subject and waits until complete    ${SUBJECT_NAME_1}    absence_school.csv    absence_school.meta.csv    ${PUBLIC_API_FILES_DIR}

Add data guidance to subjects
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_1}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_1}
    ...    ${SUBJECT_NAME_1} Main guidance content

Save data guidance
    user clicks button    Save guidance

Create 1st API dataset
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_1}
    user clicks button    Confirm new API data set

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set

User waits until the 1st API dataset status changes to 'Ready'
    user waits until h3 is visible    Draft version details
    wait until keyword succeeds    10x    %{WAIT_SMALL}s    Verify status of API Datasets    Ready

Add headline text block to Content page
    user clicks link    Back to API data sets
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user clicks link    Sign off
    user approves release for immediate publication

Create a second draft release via api
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year    3010

Upload subject to second release
   user uploads subject and waits until complete    ${SUBJECT_NAME_2}    absence_school_major_manual.csv    absence_school_major_manual.meta.csv    ${PUBLIC_API_FILES_DIR}

Add data guidance to second release
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_2}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_2}
    ...    ${SUBJECT_NAME_2} Main guidance content

Save data guidance
    user clicks button    Save guidance

Create a different version of an API dataset(Major version)
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until h3 is visible    Current live API data sets

    user checks table column heading contains    1    1    Version    xpath://table[@data-testid="live-api-data-sets"]
    user clicks button in table cell    1    3    Create new version    xpath://table[@data-testid="live-api-data-sets"]

    ${modal}=    user waits until modal is visible    Create a new API data set version
    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_2}
    user clicks button    Confirm new data set version

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set version

Validate the summary contents inside the 'draft version details' table
    user waits until h3 is visible    Draft version details
    user waits until element contains    css:dl[data-testid="draft-version-summary"] > div:nth-of-type(1) > dt + dd     v2.0    %{WAIT_LONG}
    user waits until element contains    css:dl[data-testid="draft-version-summary"] > div:nth-of-type(2) > dt + dd     Action required    %{WAIT_LONG}
    ${mapping_status}=    get text    css:dl[data-testid="draft-version-summary"] > div:nth-of-type(2) > dt + dd
    should be equal as strings    ${mapping_status}    Action required

Validate the version task statuses inside the 'Draft version task' section
    user waits until h3 is visible    Draft version tasks
    user waits until element contains    css:div[data-testid="draft-version-tasks"] li:nth-child(1) a    Map locations    %{WAIT_LONG}
    user waits until element contains    css:div[data-testid="draft-version-tasks"] li:nth-child(2) a    Map filters    %{WAIT_LONG}

    user waits until element contains    css:div[data-testid="draft-version-tasks"] li:nth-child(1) div[id="map-locations-task-status"]    Incomplete    %{WAIT_LONG}
    user waits until element contains    css:div[data-testid="draft-version-tasks"] li:nth-child(2) div[id="map-filters-task-status"]    Incomplete    %{WAIT_LONG}

User clicks on Map locations link
    user clicks link    Map locations
    user waits until h3 is visible        Locations not found in new data set
    user waits until element contains     xpath://table[@data-testid='mappable-table-region']/caption//strong[1]    1 unmapped location     %{WAIT_LONG}

Validate the 'unmapped location' notification banner
    user waits until h2 is visible    Action required
    user waits until page contains link    There is 1 unmapped region

Validate the row headings and its contents in the 'Regions' section
    user checks table column heading contains    1    1    Current data set
    user checks table column heading contains    1    2    New data set

    user checks table column heading contains    1    3    Type
    user checks table column heading contains   1    4    Actions
    
    user checks table cell contains    1    1    Yorkshire and The Humber
    user checks table cell contains    1    2    Unmapped
    user checks table cell contains    1    3    N/A

User edits location mapping
    user clicks button in table cell     1    4    Edit

    ${modal}=    user waits until modal is visible    Map existing location
    user clicks radio        Yorkshire
    user clicks button    Update location mapping
    user waits until modal is not visible    Map existing location

Verify mapping changes
    user waits until element contains    xpath://table[@data-testid='mappable-table-region']/caption//strong[1]    1 mapped location     %{WAIT_LONG}

Validate the row headings and its contents in the 'Regions' section(after mapping)

    user waits until h3 is visible    Locations not found in new data set
    user checks table column heading contains    1    1    Current data set
    user checks table column heading contains    1    2    New data set

    user checks table column heading contains    1    3    Type
    user checks table column heading contains   1    4    Actions

    user checks table cell contains    1    1    Yorkshire and The Humber
    user checks table cell contains    1    2    Yorkshire
    user checks table cell contains    1    3    Minor

    user clicks link    Back

Validate the version status of location task
    user waits until h3 is visible    Draft version tasks

    user waits until element contains    css:div[data-testid="draft-version-tasks"] li:nth-child(1) a    Map locations    %{WAIT_LONG}
    user waits until element contains    css:div[data-testid="draft-version-tasks"] li:nth-child(2) a    Map filters    %{WAIT_LONG}

    user waits until element contains    css:div[data-testid="draft-version-tasks"] li:nth-child(1) div[id="map-locations-task-status"]    Complete    %{WAIT_LONG}

User clicks on Map filters link
    user clicks link    Map filters
    user waits until h3 is visible        Filter options not found in new data set
    user waits until element contains     xpath://table[@data-testid='mappable-table-school_type']/caption//strong[1]    1 unmapped filter option     %{WAIT_LONG}

Validate the 'unmapped filter option' notification banner
    user waits until h2 is visible    Action required
    user waits until page contains link    There is 1 unmapped School type filter option

Validate the row headings and its contents in the 'filter options' section
    user checks table column heading contains    1    1    Current data set
    user checks table column heading contains    1    2    New data set

    user checks table column heading contains    1    3    Type
    user checks table column heading contains   1    4    Actions

    user checks table cell contains    1    1    Total
    user checks table cell contains    1    2    Unmapped
    user checks table cell contains    1    3    N/A

User edits filter mapping
    user clicks button in table cell     1    4    Edit

    ${modal}=    user waits until modal is visible    Map existing filter option
    user clicks radio        State-funded primary and secondary
    user clicks button    Update filter option mapping
    user waits until modal is not visible    Map existing location

Verify mapping changes
    user waits until element contains    xpath://table[@data-testid='mappable-table-school_type']/caption//strong[1]    1 mapped filter option     %{WAIT_LONG}

Validate the row headings and its contents in the 'filters options' section(after mapping)
    user waits until h3 is visible    Filter options not found in new data set
    user checks table column heading contains    1    1    Current data set
    user checks table column heading contains    1    2    New data set

    user checks table column heading contains    1    3    Type
    user checks table column heading contains   1    4    Actions

    user checks table cell contains    1    1    Total
    user checks table cell contains    1    2    State-funded primary and secondary
    user checks table cell contains    1    3    Minor

    user clicks link    Back

Confirm finalization of this API data set version
    user clicks button    Finalise this data set version
    user waits for caches to expire
    user waits until h2 is visible    Mappings finalised
    user waits until page contains    Draft API data set version is ready to be published

User navigates to 'changelog and guidance notes' page and update relevant details in it
    user clicks link by index    View changelog and guidance notes    1
    user waits until page contains     API data set changelog
    
    user enters text into element    css:textarea[id="guidanceNotesForm-notes"]    public guidance notes
    user clicks button    Save public guidance notes

    user waits until page contains    public guidance notes
    user clicks link    Back to API data set details

User clicks on 'View preview token log' link inside the 'Draft version details' section
    user clicks link by index    View changelog and guidance notes    2

Validate the contents in the 'API dataset changelog' page.
    user waits until page contains     API data set changelog

    user waits until page contains    public guidance notes
    user clicks link    Back to API data set details

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve second release
    user clicks link    Sign off
    user approves release for immediate publication

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

User navigates to data catalogue page
    user navigates to data catalogue page on public frontend

Search with 2nd API dataset
    user clicks element    id:searchForm-search
    user presses keys    ${PUBLICATION_NAME}
    user clicks radio    API data sets only

    user waits until page finishes loading
    user clicks radio    Newest

    ${API_DATASET_STATUS_VALUE}=  set variable  li[data-testid="data-set-file-summary-UI test subject 2"]:nth-of-type(1) [data-testid="Status-value"] strong:nth-of-type(1)
    user checks contents inside the cell value     This is the latest data     css:${API_DATASET_STATUS_VALUE}
    user checks page contains link    ${SUBJECT_NAME_2}

    user checks list item contains    testid:data-set-file-list    1    ${SUBJECT_NAME_2}

User clicks on 2nd API dataset link
    user clicks link by index    ${SUBJECT_NAME_2}
    user waits until page finishes loading

    user waits until h1 is visible    ${SUBJECT_NAME_2}

User checks relevant headings exist on API dataset details page
    user waits until h2 is visible    Data set details
    user waits until h2 is visible    Data set preview
    user waits until h2 is visible    Variables in this data set
    user waits until h2 is visible    Using this data
    user waits until h2 is visible    API data set quick start
    user waits until h2 is visible    API data set version history

User verifies the headings and contents in 'API version history' section
    user checks table column heading contains    1    1    Version    css:section[id="apiVersionHistory"]
    user checks table column heading contains    1    2    Release    css:section[id="apiVersionHistory"]
    user checks table column heading contains    1    3    Status     css:section[id="apiVersionHistory"]

    user checks table cell contains              1    1    1.1 (current)                xpath://section[@id="apiVersionHistory"]
    user checks table cell contains              1    2    Academic year 3010/11        xpath://section[@id="apiVersionHistory"]
    user checks table cell contains              1    3    Published                    xpath://section[@id="apiVersionHistory"]

    user checks table cell contains              2    1    1.0                          xpath://section[@id="apiVersionHistory"]
    user checks table cell contains              2    2    Financial year 3000-01       xpath://section[@id="apiVersionHistory"]
    user checks table cell contains              2    3    Published                    xpath://section[@id="apiVersionHistory"]
