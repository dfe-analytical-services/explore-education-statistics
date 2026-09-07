*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/public-api-common.robot

Force Tags          Admin    PublicApi    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes all browsers
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=    Public API - standard user permissions %{RUN_IDENTIFIER}
${RELEASE_1_NAME}=      Financial year 3000-01
${RELEASE_2_NAME}=      Academic year 3010/11
${SUBJECT_1_NAME}=      ${PUBLICATION_NAME} - Subject 1
${SUBJECT_2_NAME}=      ${PUBLICATION_NAME} - Subject 2


*** Test Cases ***
Create publication and release as bau1
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_1_NAME}

Give the Standard User drafter access to the publication
    user gives analyst publication drafter access    ${PUBLICATION_NAME}

Upload subject and add data guidance to release 1
    # Granting drafter access above navigates away to the "Manage user" admin
    # page, so we need to return to the release before continuing as bau1.
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_1_NAME}

    user uploads subject and waits until complete    ${SUBJECT_1_NAME}    absence_school.csv
    ...    absence_school.meta.csv    ${PUBLIC_API_FILES_DIR}

    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user navigates to Data Guidance page and adds data guidance for subject    ${SUBJECT_1_NAME}
    ...    ${SUBJECT_1_NAME} Main guidance content

Create the initial API data set version as bau1
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user creates API data set and opens details    ${SUBJECT_1_NAME}
    user waits until draft API data set status contains    Ready

As bau1, check the "Cannot delete files" modal offers to remove the API data set
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks button in table cell    1    4    Delete files    testid:Data files table
    ${modal}=    user waits until modal is visible    Cannot delete files
    user checks element contains    ${modal}
    ...    This data file has an API data set linked to it. Please remove the API data set before deleting.
    user checks element contains link    ${modal}    Go to API data set
    user clicks button    Close    ${modal}
    user waits until modal is not visible    Cannot delete files

Sign in as the Standard User
    user signs in as analyst1
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_1_NAME}

As a Standard User, check the "API data sets" tab is visible but has no create controls
    # Navigating to a release always lands on its Summary page, so "API data
    # sets" (a tab within the Data and files page) isn't reachable until we
    # click through to Data and files first.
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until page contains
    ...    To create, replace or remove API data sets, please contact
    user checks page contains link with text and url    explore.statistics@education.gov.uk
    ...    mailto:explore.statistics@education.gov.uk

    user checks page does not contain button    Create API data set

As a Standard User, check the draft API data set is visible but cannot be removed
    user waits until h3 is visible    Draft API data sets
    user checks table cell contains    1    2    ${SUBJECT_1_NAME}    testid:draft-api-data-sets

    user checks page does not contain button    Remove draft

    user clicks link in table cell    1    4    View details    testid:draft-api-data-sets
    user waits until h3 is visible    Draft version details
    user checks page does not contain button    Remove draft version

As a Standard User, check the "Cannot delete files" modal instead asks them to contact the EES team
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks button in table cell    1    4    Delete files    testid:Data files table
    ${modal}=    user waits until modal is visible    Cannot delete files
    user checks element contains    ${modal}
    ...    This data file has an API data set linked to it. It will need removing before the data file can be deleted. You do not have the required role to resolve the issue, but you can contact the EES team for support at
    user checks element contains link    ${modal}    explore.statistics@education.gov.uk
    user checks element contains link    ${modal}    Go to API data set
    user clicks button    Close    ${modal}
    user waits until modal is not visible    Cannot delete files

Switch to bau1 and publish release 1
    user switches to bau1 browser
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

    user approves release for immediate publication

Create release 2 as bau1
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year    3010

Upload subject and add data guidance to release 2
    user uploads subject and waits until complete    ${SUBJECT_2_NAME}    absence_school_minor_manual.csv
    ...    absence_school_minor_manual.meta.csv    ${PUBLIC_API_FILES_DIR}

    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user navigates to Data Guidance page and adds data guidance for subject    ${SUBJECT_2_NAME}
    ...    ${SUBJECT_2_NAME} Main guidance content

As a Standard User, check the "Create new version" action is hidden on the live API data sets table
    user switches to analyst1 browser
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_2_NAME}

    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until h3 is visible    Current live API data sets
    user checks table cell contains    1    1    v1.0    testid:live-api-data-sets
    user checks page does not contain button    Create new version

Create a new minor version of the API data set as bau1
    user switches to bau1 browser
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until h3 is visible    Current live API data sets
    user clicks button in table cell    1    3    Create new version
    ...    testid:live-api-data-sets

    ${modal}=    user waits until modal is visible    Create a new API data set version
    user chooses select option    name:releaseFileId    ${SUBJECT_2_NAME}
    user clicks button    Confirm new data set version

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set version

Confirm the new draft version requires manual mapping
    user waits until h3 is visible    Draft version details
    user checks summary list contains    Version    v2.0    id:draft-version-summary    wait=%{WAIT_LONG}
    user checks summary list contains    Status    Action required    id:draft-version-summary
    ...    wait=%{WAIT_LONG}

    user waits until h3 is visible    Draft version tasks
    user waits until parent contains element    testid:map-locations-task    link:Map locations
    user waits until parent contains element    id:map-locations-task-status    text:Incomplete

Switch to the Standard User and navigate to release 2
    user switches to analyst1 browser
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_2_NAME}

As a Standard User, check the draft version link text and actions in the "Draft API data sets" table
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until h3 is visible    Draft API data sets
    # This table now has an extra "Live version" column compared to the
    # release 1 table above, since Subject 1's data set has a live v1.0 as
    # well as this draft - shifting Name to column 3 and Actions to column 5.
    # The data set's name doesn't change between versions - it's still named
    # after Subject 1's file, even though this v2.0 draft is based on Subject 2.
    user waits until table cell contains    1    3    ${SUBJECT_1_NAME}    testid:draft-api-data-sets
    ...    wait=%{WAIT_LONG}
    user checks table cell contains    1    5    View details    testid:draft-api-data-sets
    user checks table cell does not contain    1    5    View details / edit draft
    ...    testid:draft-api-data-sets
    user checks page does not contain button    Remove draft

As a Standard User, check the draft version details page shows a read-only "action required" banner
    user clicks link in table cell    1    5    View details    testid:draft-api-data-sets
    user waits until h3 is visible    Draft version details

    user waits until h2 is visible    Action required
    user waits until page contains
    ...    This API data set version has mapping actions that need to be completed before it can be published, but you do not have the required role to resolve this.
    user checks page contains link with text and url    explore.statistics@education.gov.uk
    ...    mailto:explore.statistics@education.gov.uk

    user checks page does not contain button    Finalise this data set version
    user checks page does not contain button    Remove draft version

As a Standard User, check none of the location mapping tables have an Actions column
    user clicks link    Map locations
    user waits until h3 is visible    Locations not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-ward"] caption
    ...    1 unmapped location    %{WAIT_LONG}
    user waits until h3 is visible    Auto mapped locations

    # Covers every mappable and auto-mapped table on the page (one per location
    # level, e.g. Ward, Region, National), not just the single Ward table that
    # has an unmapped location - none of them should show an Actions column.
    user checks page does not contain element
    ...    xpath://table//th[text()="Actions"]

As a Standard User, check none of the filter mapping tables have an Actions column
    user clicks link    Back
    user clicks link    Map filters
    user waits until h3 is visible    Filter options not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-schoolType"] caption
    ...    1 unmapped filter option    %{WAIT_LONG}
    user waits until h3 is visible    Filter options found in both

    user checks page does not contain element
    ...    xpath://table//th[text()="Actions"]

As a Standard User, check none of the indicator mapping tables have an Actions column
    user clicks link    Back
    user clicks link    Map indicators
    user waits until h3 is visible    Indicators not found in new data set (1)
    user waits until element contains    css:[data-testid="mappable-table-default"] caption
    ...    1 unmapped indicator    %{WAIT_LONG}
    user waits until h3 is visible    Indicators found in both

    user checks page does not contain element
    ...    xpath://table//th[text()="Actions"]

Switch to bau1 and complete the location mapping
    user switches to bau1 browser
    user clicks link    Map locations
    user waits until h3 is visible    Locations not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-ward"] caption
    ...    1 unmapped location    %{WAIT_LONG}

    user clicks button in table cell    1    4    Map location
    ${modal}=    user waits until modal is visible    Map existing location
    user clicks radio    St Agnes
    user clicks button    Update location mapping
    user waits until modal is not visible    Map existing location

    user waits until element contains    css:[data-testid="mappable-table-ward"] caption
    ...    1 mapped location    %{WAIT_LONG}
    user clicks link    Back

Complete the filter mapping as bau1
    user clicks link    Map filters
    user waits until h3 is visible    Filter options not found in new data set
    user waits until element contains    css:[data-testid="mappable-table-schoolType"] caption
    ...    1 unmapped filter option    %{WAIT_LONG}

    user clicks button in table cell    1    4    Map filter option
    ${modal}=    user waits until modal is visible    Map existing filter option
    user clicks radio    State-funded primary and secondary
    user clicks button    Update filter option mapping
    user waits until modal is not visible    Map existing filter option

    user waits until element contains    css:[data-testid="mappable-table-schoolType"] caption
    ...    1 mapped filter option    %{WAIT_LONG}
    user clicks link    Back

Complete the indicator mapping as bau1
    user clicks link    Map indicators
    user waits until h3 is visible    Indicators not found in new data set (1)
    user waits until element contains    css:[data-testid="mappable-table-default"] caption
    ...    1 unmapped indicator    %{WAIT_LONG}

    user clicks button in table cell    1    4    Map indicator    testid:mappable-table-default
    ${modal}=    user waits until modal is visible    Map existing indicator
    user clicks radio    Number of enrolments
    user clicks button    Update indicator mapping
    user waits until modal is not visible    Map existing indicator

    user waits until element contains    css:[data-testid="mappable-table-default"] caption
    ...    1 mapped indicator    %{WAIT_LONG}
    user clicks link    Back

Confirm all mapping tasks are now complete
    user waits until h3 is visible    Draft version tasks
    user waits until parent contains element    testid:map-locations-task    link:Map locations
    user waits until parent contains element    id:map-locations-task-status    text:Complete
    user waits until parent contains element    testid:map-filters-task    link:Map filters
    user waits until parent contains element    id:map-filters-task-status    text:Complete
    user waits until parent contains element    testid:map-indicators-task    link:Map indicators
    user waits until parent contains element    id:map-indicators-task-status    text:Complete

As bau1, check the finalise banner is actionable now mapping is complete
    user waits until h2 is visible    Action required
    user waits until page contains    Draft API data set version is ready to be finalised
    user waits until page contains
    ...    The mapping changes need to be finalised before the draft API data set version can be published.

    user checks page contains button    Finalise this data set version

As a Standard User, check the same finalise banner instead asks them to contact the EES team
    user switches to analyst1 browser
    user clicks link    Back
    # Force a fresh fetch of the draft version so this browser picks up the
    # mapping changes bau1 just made, rather than relying on cached data.
    user reloads page
    user waits until h3 is visible    Draft version details

    user waits until h2 is visible    Action required
    user waits until page contains    Draft API data set version is ready to be finalised
    user waits until page contains
    ...    This API data set version is ready to be finalised, but you do not have the required role to do this.
    user checks page contains link with text and url    explore.statistics@education.gov.uk
    ...    mailto:explore.statistics@education.gov.uk

    user checks page does not contain button    Finalise this data set version

Switch to bau1 and finalise the data set version
    user switches to bau1 browser
    user clicks button    Finalise this data set version
    user waits for caches to expire
    user waits until h2 is visible    Mappings finalised
    user waits until page contains    Draft API data set version is ready to be published

As bau1, check the changelog page allows editing guidance notes
    user clicks link    View changelog and guidance notes    id:draft-version-summary
    user waits until page contains    API data set changelog

    user enters text into element    name:notes
    ...    Content for the public guidance notes
    user clicks button    Save public guidance notes

    user waits until element contains    testid:public-guidance-notes
    ...    Content for the public guidance notes
    user checks page contains button    Edit public guidance notes

As a Standard User, check the changelog page shows the guidance notes read-only
    user switches to analyst1 browser
    # Force a fresh fetch so this browser picks up bau1's finalisation above.
    user reloads page
    user waits until h3 is visible    Draft version details

    user clicks link    View changelog and guidance notes    id:draft-version-summary
    user waits until page contains    API data set changelog

    user waits until element contains    testid:public-guidance-notes
    ...    Content for the public guidance notes
    user checks page does not contain button    Edit public guidance notes
    # Unlike every other Standard User restriction in this suite, this page
    # gives no explanation of why editing isn't available and no contact link.
    user checks page does not contain    contact the EES team
