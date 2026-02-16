*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/public-api-common.robot

Force Tags          Admin    PublicApi    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}     Public API - restricted %{RUN_IDENTIFIER}
${RELEASE_NAME}         Financial year 3000-01
${SUBJECT_NAME_1}       ${PUBLICATION_NAME} - Subject 1
${SUBJECT_NAME_2}       ${PUBLICATION_NAME} - Subject 2
${SUBJECT_NAME_3}       ${PUBLICATION_NAME} - Subject 3
${SUBJECT_NAME_4}       ${PUBLICATION_NAME} - Subject 4
${SUBJECT_NAME_5}       ${PUBLICATION_NAME} - Subject 5


*** Test Cases ***
Create publication and release
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year    3000-01    Accredited official statistics

Upload data files
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

Create 1st API data set
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

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

Create 2nd API data set
    user clicks link    Back to API data sets
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

    user checks table cell contains    2    1    v1.0    testid:draft-api-data-sets
    user checks table cell contains    2    3    Ready    testid:draft-api-data-sets

Click on 'View Details' link
    user clicks link in table cell    1    4    View details    testid:draft-api-data-sets
    user waits until h3 is visible    Draft version details

User checks row data contents inside the 'Draft API data sets' summary table
    user checks summary list contains    Version    v1.0
    user checks summary list contains    Status    Ready
    user checks summary list contains    Release    ${RELEASE_NAME}
    user checks summary list contains    Data set file    ${SUBJECT_NAME_1}
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time periods    2012/13
    user checks list contains exact items in order    id:draft-version-summary-indicators
    ...    Lower quartile annualised earnings
    ...    Median annualised earnings
    ...    Number of learners with earnings

    user clicks button    Show 1 more indicator    testid:Indicators

    user checks list contains exact items in order    id:draft-version-summary-indicators
    ...    Lower quartile annualised earnings
    ...    Median annualised earnings
    ...    Number of learners with earnings
    ...    Upper quartile annualised earnings

    user checks list contains exact items in order    id:draft-version-summary-filters
    ...    Cheese
    ...    Colour
    ...    Ethnicity group

    user clicks button    Show 4 more filters    testid:Filters

    user checks list contains exact items in order    id:draft-version-summary-filters
    ...    Cheese
    ...    Colour
    ...    Ethnicity group
    ...    Gender
    ...    Level of learning
    ...    Number of years after achievement of learning aim
    ...    Provision

    user checks list contains exact items in order    testid:Actions
    ...    Preview API data set
    ...    View preview token log
    ...    Remove draft version

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user approves release for immediate publication

Navigate to admin and create an amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Create a second draft release via API
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year    3010

Upload subject to second release
    user uploads subject and waits until complete    ${SUBJECT_NAME_4}    seven_filters_minor_update.csv
    ...    seven_filters_minor_update.meta.csv    ${PUBLIC_API_FILES_DIR}

Validate checklist warning for an API data set which has not been updated
    user clicks link    Publishing checklist
    user checks checklist warnings contains
    ...    Public API data sets associated with this publication have not been updated as part of this release. This will create breaking changes and be confusing for end users. Please set up new versions of API data sets where appropriate

Add data guidance to second release
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user adds main data guidance content

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_4}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_4}
    ...    ${SUBJECT_NAME_4} Main guidance content

Save data guidance for the second release
    user clicks button    Save guidance

Create a different version of an API data set (minor version)
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until h3 is visible    Current live API data sets

    user checks table column heading contains    1    1    Version    testid:live-api-data-sets
    user clicks button in table cell    1    3    Create new version    testid:live-api-data-sets

    ${modal}=    user waits until modal is visible    Create a new API data set version
    user chooses select option    name:releaseFileId    ${SUBJECT_NAME_4}
    user clicks button    Confirm new data set version

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set version    %{WAIT_LONG}

Validate the summary contents inside the 'draft version details' table
    user waits until h3 is visible    Draft version details
    user waits until draft api data set status contains    Action required
    user checks summary list contains    Version    v1.1

Add headline text block to Content page for the second release
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Validate checklist error for a draft API data set which shows mapping error
    user clicks link    Publishing checklist
    user checks checklist errors contains
    ...    1 issue that must be resolved before this release can be published.
    user checks checklist errors contains link
    ...    All public API data set mappings must be completed

Create a third draft release via API
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year    3020

Upload subject to the third release
    user uploads subject and waits until complete    ${SUBJECT_NAME_5}    grouped-filters-and-indicators.csv
    ...    grouped-filters-and-indicators.meta.csv    ${PUBLIC_API_FILES_DIR}

Add data guidance to the third release
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user adds main data guidance content

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_5}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_5}
    ...    ${SUBJECT_NAME_5} Main guidance content

Save data guidance for the third release
    user clicks button    Save guidance

Create a different version of API data set (major version) for the third release
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until h3 is visible    Current live API data sets

    user checks table column heading contains    1    1    Version    testid:live-api-data-sets
    user clicks button in table cell    1    3    Create new version    testid:live-api-data-sets

    ${modal}=    user waits until modal is visible    Create a new API data set version

    user chooses select option    name:releaseFileId    ${SUBJECT_NAME_5}
    user clicks button    Confirm new data set version

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set version    %{WAIT_LONG}

Validate the summary contents inside the 'draft version details' table for the third release
    user waits until h3 is visible    Draft version details
    user waits until draft api data set status contains    Action required
    user checks summary list contains    Version    v2.0

# Adding this headline text block is optional, but I chose to include it to focus specifically on the errors I'm targeting.
# Without this, I might be inclined to add a checklist for headline-specific text block errors, which isn't necessary.

Add headline text block to Content page for the third release
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Validate checklist error for a draft API data set which shows mapping error for the third release
    user clicks link    Publishing checklist
    user checks checklist errors contains
    ...    1 issue that must be resolved before this release can be published.
    user checks checklist errors contains link
    ...    All public API data set mappings must be completed
