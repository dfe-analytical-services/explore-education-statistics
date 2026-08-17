*** Settings ***
Library             ../../libs/admin_api.py
Library             ../../libs/public_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/public-api-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      run keywords    user closes the browser    AND    user removes ein page if exists    ui-test-page
Test Setup          fail test fast if required

Force Tags          Admin    PublicApi    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    EiN API query tile %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Academic year 3000/01
${SUBJECT_NAME}=        ${PUBLICATION_NAME} - Subject 1
${INDICATOR_LABEL}=     Authorised absence rate
${TILE_STATISTIC}=      3.0%    # 3.01996 in the source data, formatted using the indicator's unit (%) and decimal places (1)


*** Test Cases ***
Create publication and release to hold an API data set
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Upload subject for the API data set
    user uploads subject and waits until complete    ${SUBJECT_NAME}    tiny-two-filters.csv
    ...    tiny-two-filters.meta.csv    ${PUBLIC_API_FILES_DIR}

Add data guidance to subject
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user navigates to Data Guidance page and adds data guidance for subject    ${SUBJECT_NAME}
    ...    ${SUBJECT_NAME} Main guidance content

Create API data set
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user creates API data set and opens details    ${SUBJECT_NAME}
    user waits until draft API data set status contains    Ready

    ${current_url}=    get location
    ${API_DATA_SET_ID}=    fetch from right    ${current_url}    /
    check that variable is not empty    API_DATA_SET_ID    ${API_DATA_SET_ID}
    set suite variable    ${API_DATA_SET_ID}

Publish the release so the API data set has a live version
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

    user approves release for immediate publication

Build the query for the API query stat tile
    ${meta}=    wait until keyword succeeds    10x    %{WAIT_SMALL}s
    ...    user gets api data set meta via api    ${API_DATA_SET_ID}

    ${indicator_id}=    user gets API data set indicator id    ${meta}    ${INDICATOR_LABEL}
    ${colour_id}=    user gets API data set filter option id    ${meta}    Colour    Blue
    ${school_type_id}=    user gets API data set filter option id    ${meta}    School type    Total

    # An ApiQueryStatTile requires a query returning exactly one indicator and a single
    # national result for the latest time period. These filters narrow the data set down
    # to its one National row.
    ${API_QUERY}=    catenate    SEPARATOR=
    ...    {"criteria":{"and":[
    ...    {"filters":{"eq":"${colour_id}"}},
    ...    {"filters":{"eq":"${school_type_id}"}}
    ...    ]},"indicators":["${indicator_id}"]}
    set suite variable    ${API_QUERY}

Remove UI test page if exists via API
    user removes ein page if exists    ui-test-page

Go to EiN management page
    user navigates to admin dashboard
    user clicks link    Platform administration
    user waits until h1 is visible    Platform administration

    user clicks link    Manage Education in Numbers
    user waits until page contains element    testid:education-in-numbers-table

    user checks page does not contain    UI test page

Add new page "UI test page"
    user clicks link    Add new page
    user waits until h1 is visible    Create a new Education in Numbers page

    user enters text into element    css:#educationInNumbersSummaryForm-title    UI test page
    user enters text into element    css:#educationInNumbersSummaryForm-description    UI test page description

    user clicks button    Create page

    user waits until h2 is visible    Page summary

Validate page appears in EiN page table
    user clicks link    Manage Education in Numbers
    user waits until h1 is visible    Education in Numbers pages

    ${ROW}=    user gets table row    UI test page    testid:education-in-numbers-table
    user checks row cell contains text    ${ROW}    2    ui-test-page
    user checks row cell contains text    ${ROW}    3    Draft
    user checks row cell contains text    ${ROW}    4    Not yet published
    user checks row cell contains text    ${ROW}    5    0
    user checks row cell contains text    ${ROW}    6    Edit
    user checks row cell contains text    ${ROW}    6    Delete

    user clicks link    Edit    ${ROW}
    user waits until h2 is visible    Page summary

Validate page summary
    user checks summary list contains    Title    UI test page    testid:summary-list
    user checks summary list contains    Slug    ui-test-page    testid:summary-list
    user checks summary list contains    Description    UI test page description    testid:summary-list
    user checks summary list contains    Status    Draft    testid:summary-list
    user checks summary list contains    Published on    Not yet published    testid:summary-list

Edit page summary
    user clicks link    Edit summary
    user waits until h2 is visible    Edit page summary

    user clears element text    css:#educationInNumbersSummaryForm-description
    user enters text into element    css:#educationInNumbersSummaryForm-description    UI test page description updated

    user clicks button    Update page
    user waits until h2 is visible    Page summary

Validate updated page summary
    user checks summary list contains    Description    UI test page description updated    testid:summary-list

Add a content section
    user clicks link    Manage content
    user waits until h2 is visible    UI test page

    user creates new content section    1    Content section title

Add a text block
    user adds text block to editable accordion section    Content section title    testid:accordion
    user adds content to accordion section text block    Content section title    1
    ...    Some text block content    testid:accordion

Add a group block
    user clicks button    Add group block
    user clicks button    Add group heading

    user enters text into element    //input[@name="heading"]    Group tile heading
    user clicks button    Save group heading

Add free text stat tile
    user clicks button    Add new free text stat tile
    user waits until page contains element    testid:freeTextStatTile-editForm

    user enters text into element    //input[@name="title"]    Tile title
    user enters text into element    //input[@name="statistic"]    Over 9000!
    user enters text into element    //input[@name="trend"]    It's up a lot!
    user enters text into element    //input[@name="linkUrl"]    http://test.link
    user enters text into element    //input[@name="linkText"]    A link to somewhere

    user clicks button    Save
    user waits until page contains element
    ...    xpath://*[@data-testid="free-text-stat-tile-title" and text()="Tile title"]

Add API query stat tile
    user clicks button    Add new API query stat tile
    user waits until page contains element    testid:apiQueryStatTile-editForm

    user enters text into element    //input[@name="title"]    ${INDICATOR_LABEL}
    user enters text into element    //input[@name="dataSetId"]    ${API_DATA_SET_ID}
    user enters text into element    //input[@name="version"]    1.0.0
    user enters text into element    //textarea[@name="query"]    ${API_QUERY}

    user clicks button    Save
    user waits until page does not contain element    testid:apiQueryStatTile-editForm    %{WAIT_LONG}

Validate API query stat tile in editing mode
    user checks API query stat tile is displayed

    # The tile was queried against the data set's only live version, so it should not
    # be flagged as out of date.
    user checks page does not contain element    testid:api-query-stat-tile-not-latest-tag
    user checks page does not contain element    testid:apiQueryStatTile-notLatestVersionWarning

Validate content preview
    user clicks element    id:editingMode-preview
    user waits until page does not contain    Remove this section

    user waits until page contains    Content section title
    user checks page contains    Some text block content

    user checks page contains    Group tile heading
    user checks page contains    Tile title
    user checks page contains    Over 9000!
    user checks page contains    It's up a lot!
    user checks page contains link with text and url    A link to somewhere    http://test.link

    user checks API query stat tile is displayed

Publish page
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    ${test_page_url}=    get value    testid:public-page-url
    set suite variable    ${test_page_url}
    should be equal    %{PUBLIC_URL}/education-in-numbers/ui-test-page    ${test_page_url}

    user checks summary list contains    Title    UI test page    testid:page-list
    user checks summary list contains    Slug    ui-test-page    testid:page-list
    user checks summary list contains    Description    UI test page description updated    testid:page-list
    user checks summary list contains    Published on    Not yet published    testid:page-list

    user clicks button    Publish
    user waits until h2 is visible    Are you sure you want to publish UI test page?

    user clicks button    Confirm

    user checks summary list contains    Status    Published    testid:summary-list

Check page appears on public site
    user navigates to    ${test_page_url}

    user waits until h1 is visible    UI test page

    user checks page contains link with text and url    UI test page    /education-in-numbers/ui-test-page

    user checks page contains    Content section title
    user checks page contains    Some text block content

    user checks page contains    Group tile heading
    user checks page contains    Tile title
    user checks page contains    Over 9000!
    user checks page contains    It's up a lot!
    user checks page contains link with text and url    A link to somewhere    http://test.link

    user checks API query stat tile is displayed

Validate Manage Education in Numbers entry
    user navigates to    %{ADMIN_URL}/education-in-numbers

    user waits until h1 is visible    Education in Numbers pages

    ${ROW}=    user gets table row    UI test page    testid:education-in-numbers-table
    user checks row cell contains text    ${ROW}    2    ui-test-page
    user checks row cell contains text    ${ROW}    3    Published
    # 4 is the published date
    user checks row cell contains text    ${ROW}    5    0
    user checks row cell contains text    ${ROW}    6    View
    user checks row cell contains text    ${ROW}    6    Create amendment

    user clicks button    Create amendment    ${ROW}
    user waits until h2 is visible    Page summary

Validate amendment summary
    user checks summary list contains    Title    UI test page    testid:summary-list
    user checks summary list contains    Slug    ui-test-page    testid:summary-list
    user checks summary list contains    Description    UI test page description updated    testid:summary-list
    user checks summary list contains    Status    Draft amendment    testid:summary-list
    user checks summary list contains    Published on    Not yet published    testid:summary-list

Add new content section
    user clicks link    Manage content
    user waits until h2 is visible    UI test page

    user creates new content section    2    Second section

Add a text block to new section
    user adds text block to editable accordion section    Second section    testid:accordion
    user adds content to accordion section text block    Second section    1
    ...    More text block content    testid:accordion

Publish amendment
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    user clicks button    Publish
    user waits until h2 is visible    Are you sure you want to publish UI test page?

    user clicks button    Confirm

    user checks summary list contains    Status    Published    testid:summary-list

Check amendment on public site
    user waits for caches to expire
    user navigates to    ${test_page_url}

    user waits until h1 is visible    UI test page

    user checks page contains link with text and url    UI test page    /education-in-numbers/ui-test-page

    user checks page contains    Content section title
    user checks page contains    Some text block content

    user checks page contains    Group tile heading
    user checks page contains    Tile title
    user checks page contains    Over 9000!
    user checks page contains    It's up a lot!
    user checks page contains link with text and url    A link to somewhere    http://test.link

    user checks API query stat tile is displayed

    user checks page contains    Second section
    user checks page contains    More text block content


*** Keywords ***
user checks API query stat tile is displayed
    user waits until page contains element
    ...    xpath://*[@data-testid="api-query-stat-tile-title" and normalize-space()="${INDICATOR_LABEL}"]
    user checks page contains element
    ...    xpath://*[@data-testid="api-query-stat-tile-statistic" and normalize-space()="${TILE_STATISTIC}"]

    user checks page contains element
    ...    xpath://*[@data-testid="api-query-stat-tile-link-release" and normalize-space()="${RELEASE_NAME}"]

    ${link_xpath}=    catenate    SEPARATOR=${SPACE}
    ...    xpath://a[@data-testid="api-query-stat-tile-link"
    ...    and contains(@href, "/find-statistics/")
    ...    and normalize-space()="${PUBLICATION_NAME}"]
    user checks page contains element    ${link_xpath}
