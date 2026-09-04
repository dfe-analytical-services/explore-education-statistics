*** Settings ***
Library             DateTime
Library             ../libs/admin_api.py
Library             ../libs/public_api.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/public-api-common.robot
Resource            ../libs/public-common.robot

Force Tags          Admin    PublicApi    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${PUBLICATION_NAME}=        Public API - preview token %{RUN_IDENTIFIER}
${RELEASE_NAME}=            Academic year Q1 3000/01
${SUBJECT_NAME}=            ${PUBLICATION_NAME} - Subject 1
${PREVIEW_TOKEN_NAME}=      Test token


*** Test Cases ***
Create publication and release
    user selects dashboard theme if possible
    user clicks link    Create new publication
    user waits until h1 is visible    Create new publication
    user creates publication    ${PUBLICATION_NAME}
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year Q1    3000

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Academic year Q1    3000/01    Accredited official statistics

Upload data files
    user uploads subject and waits until complete    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv
    ...    ${PUBLIC_API_FILES_DIR}

Add data guidance to subjects
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user navigates to Data Guidance page and adds data guidance for subject    ${SUBJECT_NAME}
    ...    ${SUBJECT_NAME} Main guidance content

Create API data set
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user creates API data set and opens details    ${SUBJECT_NAME}

User waits until the API data set status changes to 'Ready'
    user waits until h3 is visible    Draft version details
    user waits until draft API data set status contains    Ready

Verify the contents inside 'Draft API data sets' table
    user clicks link    Back to API data sets
    user waits until h3 is visible    Draft API data sets

    user checks table column heading contains    1    1    Draft version    testid:draft-api-data-sets
    user checks table column heading contains    1    2    Name    testid:draft-api-data-sets
    user checks table column heading contains    1    3    Status    testid:draft-api-data-sets
    user checks table column heading contains    1    4    Actions    testid:draft-api-data-sets

    user checks table body has x rows    1    testid:draft-api-data-sets
    user checks table cell contains    1    1    v1.0    testid:draft-api-data-sets
    user checks table cell contains    1    3    Ready    testid:draft-api-data-sets

Click on 'View Details' link for API data set
    user clicks link in table cell    1    4    View details    testid:draft-api-data-sets
    user waits until h3 is visible    Draft version details

    ${current_url}=    get location
    ${API_DATA_SET_ID}=    fetch from right    ${current_url}    /
    check that variable is not empty    API_DATA_SET_ID    ${API_DATA_SET_ID}
    set suite variable    ${API_DATA_SET_ID}

User verifies the 'Draft API data set' summary list
    user checks summary list contains    Version    v1.0
    user checks summary list contains    Status    Ready
    user checks summary list contains    Release    ${RELEASE_NAME}
    user checks summary list contains    Data set file    ${SUBJECT_NAME}
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

User clicks on 'Preview API data set' link
    user clicks link containing text    Preview API data set

User clicks on 'Generate preview token'
    user clicks button    Generate preview token

User creates preview token through 'Generate preview token' modal window
    ${modal}=    user waits until modal is visible    Generate preview token
    user enters text into element    name:label    ${PREVIEW_TOKEN_NAME}
    user clicks element    name:agreeTerms
    user clicks button    Continue

    user waits until page finishes loading
    user waits until modal is not visible    Generate preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME}

User revokes preview token
    user clicks button    Revoke preview token
    ${modal}=    user waits until modal is visible    Revoke preview token
    user clicks button    Confirm
    user waits until page finishes loading
    user waits until modal is not visible    Revoke preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token log

User again clicks on 'Generate preview token' to pick pre set days as expiry
    user clicks link    Generate preview token
    user clicks button    Generate preview token

User creates preview token through 'Generate preview token' modal window selecting the preview token to be active for 7 days
    ${modal}=    user waits until modal is visible    Generate preview token
    user enters text into element    name:label    ${PREVIEW_TOKEN_NAME}

    user clicks element    id:apiDataSetTokenCreateForm-selectionMethod-presetDays
    user clicks element    id:apiDataSetTokenCreateForm-datePresetSpan-7
    user clicks element    name:agreeTerms
    user clicks button    Continue

    user waits until page finishes loading
    user waits until modal is not visible    Generate preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME}

User verifies created preview token details expires in 7 days
    user checks page contains    Reference: ${PREVIEW_TOKEN_NAME}

    ${date_in_7_days}=    get local browser date and time    offset_days=7    format_string=%-d %B %Y
    user checks page contains
    ...    The token expires: ${date_in_7_days} (UK time)

    user checks page contains button    Copy preview token
    user checks page contains button    Revoke preview token
    user checks page contains link    View preview token log
    user checks page contains    API data set endpoints quick start

User revokes preview token that expires in 7 days
    user clicks button    Revoke preview token
    ${modal}=    user waits until modal is visible    Revoke preview token
    user clicks button    Confirm
    user waits until page finishes loading
    user waits until modal is not visible    Revoke preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token log

User again clicks on 'Generate preview token' to pick custom activation and expiration dates
    user clicks link    Generate preview token
    user clicks button    Generate preview token

User creates another preview token through 'Generate preview token' modal window to set custom activation and expiration dates
    ${modal}=    user waits until modal is visible    Generate preview token
    user enters text into element    name:label    ${PREVIEW_TOKEN_NAME}
    user clicks element    id:apiDataSetTokenCreateForm-selectionMethod-customDates
    ${today}=    Get Current Date    result_format=%d
    ${current_month}=    Get Current Date    result_format=%m
    ${current_year}=    Get Current Date    result_format=%Y
    ${tomorrow}=    Add Time To Date    ${current_year}-${current_month}-${today}    1 day    result_format=%d
    ${tomorrow_month}=    Add Time To Date    ${current_year}-${current_month}-${today}    1 day    result_format=%m
    ${tomorrow_year}=    Add Time To Date    ${current_year}-${current_month}-${today}    1 day    result_format=%Y
    ${after_tomorrow}=    Add Time To Date    ${current_year}-${current_month}-${today}    2 day    result_format=%d
    ${after_tomorrow_month}=    Add Time To Date    ${current_year}-${current_month}-${today}    2 day
    ...    result_format=%m
    ${after_tomorrow_year}=    Add Time To Date    ${current_year}-${current_month}-${today}    2 day
    ...    result_format=%Y

    user enters text into element    id:apiDataSetTokenCreateForm-activates-day    ${tomorrow}
    user enters text into element    id:apiDataSetTokenCreateForm-activates-month    ${tomorrow_month}
    user enters text into element    id:apiDataSetTokenCreateForm-activates-year    ${tomorrow_year}

    user enters text into element    id:apiDataSetTokenCreateForm-expires-day    ${after_tomorrow}
    user enters text into element    id:apiDataSetTokenCreateForm-expires-month    ${after_tomorrow_month}
    user enters text into element    id:apiDataSetTokenCreateForm-expires-year    ${tomorrow_year}
    user clicks element    name:agreeTerms
    user clicks button    Continue

    user waits until page finishes loading
    user waits until modal is not visible    Generate preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME}

User verifies created preview token details for custom dates
    user checks page contains    Reference: ${PREVIEW_TOKEN_NAME}

    ${date_tomorrow}=    get local browser date and time    offset_days=1    format_string=%-d %B %Y
    ${date_after_tomorrow}=    get local browser date and time    offset_days=2    format_string=%-d %B %Y
    user checks page contains
    ...    The token is active from: ${date_tomorrow} (UK time) and expires: ${date_after_tomorrow} (UK time)

    user checks page contains button    Copy preview token
    user checks page contains button    Revoke preview token
    user checks page contains link    View preview token log
    user checks page contains    API data set endpoints quick start

User revokes custom expiry preview token
    user clicks button    Revoke preview token
    ${modal}=    user waits until modal is visible    Revoke preview token
    user clicks button    Confirm
    user waits until page finishes loading
    user waits until modal is not visible    Revoke preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token log

User again clicks on 'Generate preview token'
    user clicks link    Generate preview token
    user clicks button    Generate preview token

User creates another preview token through 'Generate preview token' modal window
    ${modal}=    user waits until modal is visible    Generate preview token
    user enters text into element    name:label    ${PREVIEW_TOKEN_NAME}
    user clicks element    name:agreeTerms
    user clicks button    Continue

    user waits until page finishes loading
    user waits until modal is not visible    Generate preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME}

User cancels revoking preview token
    user clicks button    Revoke preview token
    ${modal}=    user waits until modal is visible    Revoke preview token
    user clicks button    Cancel
    user waits until page finishes loading
    user waits until modal is not visible    Revoke preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME}

User cancels creating preview token
    user clicks link    Back to API data set details
    user clicks link containing text    Preview API data set
    user clicks button    Generate preview token

    ${modal}=    user waits until modal is visible    Generate preview token
    user enters text into element    name:label    ${PREVIEW_TOKEN_NAME}
    user clicks element    name:agreeTerms
    user clicks button    Cancel

    user waits until page finishes loading
    user waits until modal is not visible    Generate preview token    %{WAIT_LONG}
    user waits until page contains    Generate API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME}

Verify the 'Active tokens' and 'Expired tokens' on preview token log page
    user clicks link    Back to API data set details
    user clicks link    View preview token log
    user waits until page contains    API data set preview token log
    user waits until h2 is visible    ${SUBJECT_NAME}

    user checks table column heading contains    1    1    Reference
    user checks table column heading contains    1    2    User
    user checks table column heading contains    1    3    Activates
    user checks table column heading contains    1    4    Status
    user checks table column heading contains    1    5    Expires
    user checks table column heading contains    1    6    Action

    user checks table cell contains    1    4    Active
    user checks table cell contains    2    4    Expired

    user clicks button in table cell    1    6    Revoke
    ${modal}=    user waits until modal is visible    Revoke preview token
    user clicks button    Confirm    ${modal}
    user waits until page finishes loading
    user checks table cell contains    1    4    Expired

User verifies the relevant fields on the active preview token page
    user clicks link    Generate preview token
    user clicks button    Generate preview token

    ${modal}=    user waits until modal is visible    Generate preview token
    user enters text into element    name:label    ${PREVIEW_TOKEN_NAME}
    user clicks element    name:agreeTerms
    user clicks button    Continue

    user waits until page finishes loading
    user waits until modal is not visible    Generate preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token

    user waits until h2 is visible    ${SUBJECT_NAME}
    user checks page contains    Reference: ${PREVIEW_TOKEN_NAME}

    user checks page contains
    ...    The token expires: tomorrow at 11:59 PM (UK time)

    user checks page contains button    Copy preview token
    user checks page contains button    Revoke preview token
    user checks page contains link    View preview token log
    user checks page contains    API data set endpoints quick start

User copies the preview token for use with the API
    ${PREVIEW_TOKEN}=    get value    id:copy-preview-token
    check that variable is not empty    PREVIEW_TOKEN    ${PREVIEW_TOKEN}
    set suite variable    ${PREVIEW_TOKEN}

User cannot access the draft API data set via the API without the preview token
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}
    user checks response status code    ${response}    403

    &{params}=    create dictionary    dataSetVersion=1.0
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}/meta    params=${params}
    user checks response status code    ${response}    403

User accesses the draft API data set via the API using the preview token
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}
    ...    preview_token=${PREVIEW_TOKEN}
    user checks response status code    ${response}    200
    ${json}=    response should be json    ${response}
    should be equal    ${json['title']}    ${SUBJECT_NAME}

User gets the draft API data set meta via the API using the preview token
    &{params}=    create dictionary    dataSetVersion=1.0
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}/meta
    ...    params=${params}
    ...    preview_token=${PREVIEW_TOKEN}
    user checks response status code    ${response}    200
    ${meta}=    response should be json    ${response}

    ${MEDIAN_INDICATOR_ID}=    user gets API data set indicator id    ${meta}    Median annualised earnings
    set suite variable    ${MEDIAN_INDICATOR_ID}

User queries the draft API data set via the API using the preview token
    &{params}=    create dictionary    dataSetVersion=1.0
    ${response}=    user makes post request to public api    /data-sets/${API_DATA_SET_ID}/query
    ...    body={"indicators":["${MEDIAN_INDICATOR_ID}"]}
    ...    params=${params}
    ...    preview_token=${PREVIEW_TOKEN}
    user checks response status code    ${response}    200
    ${json}=    response should be json    ${response}
    paginated response should contain results    ${json}    3270

User downloads the draft API data set CSV via the API using the preview token
    &{params}=    create dictionary    dataSetVersion=1.0
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}/csv
    ...    params=${params}
    ...    preview_token=${PREVIEW_TOKEN}
    user checks response status code    ${response}    200
    ${row_count}=    evaluate    len($response.text.strip().splitlines())
    should be equal as integers    ${row_count}    3271

User revokes the preview token used for API access
    user clicks button    Revoke preview token
    ${modal}=    user waits until modal is visible    Revoke preview token
    user clicks button    Confirm
    user waits until page finishes loading
    user waits until modal is not visible    Revoke preview token    %{WAIT_LONG}
    user waits until page contains    API data set preview token log

User cannot access the draft API data set via the API with a revoked preview token
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}
    ...    preview_token=${PREVIEW_TOKEN}
    user checks response status code    ${response}    403

Add headline text block to Content page
    user clicks link    Back to API data set details
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve release
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
    user checks page contains link    ${SUBJECT_NAME}
    user checks list item contains    testid:data-set-file-list    1    ${SUBJECT_NAME}

User clicks on API data set link
    user clicks link containing text    ${SUBJECT_NAME}
    user waits until page finishes loading

    user waits until h1 is visible    ${SUBJECT_NAME}

User checks relevant headings exist on API data set details page
    user waits until h2 is visible    Data set details
    user waits until h2 is visible    Data set preview
    user waits until h2 is visible    Variables in this data set
    user waits until h2 is visible    Using this data
    user waits until h2 is visible    Using the API
    user waits until h2 is visible    API data set version history

User verifies 'Get email alerts' link
    user checks page contains link    Get email alerts

User verifies 'Data set details' section
    user checks summary list contains    Theme    %{TEST_THEME_NAME}    id:dataSetDetails
    user checks summary list contains    Publication    ${PUBLICATION_NAME}    id:dataSetDetails
    user checks summary list contains    Release    ${RELEASE_NAME}    id:dataSetDetails
    user checks summary list contains    Release type    Accredited official statistics    id:dataSetDetails
    user checks summary list contains    Geographic levels    National    id:dataSetDetails

    user checks list contains exact items in order    id:indicators
    ...    Lower quartile annualised earnings
    ...    Median annualised earnings
    ...    Number of learners with earnings
    ...    parent=id:dataSetDetails

    user clicks button    Show 1 more indicator    css:#dataSetDetails [data-testid="Indicators"]

    user checks list contains exact items in order    id:indicators
    ...    Lower quartile annualised earnings
    ...    Median annualised earnings
    ...    Number of learners with earnings
    ...    Upper quartile annualised earnings
    ...    parent=id:dataSetDetails

    user checks list contains exact items in order    id:filters
    ...    Cheese
    ...    Colour
    ...    Ethnicity group
    ...    parent=id:dataSetDetails

    user clicks button    Show 4 more filters    css:#dataSetDetails [data-testid="Filters"]

    user checks list contains exact items in order    id:filters
    ...    Cheese
    ...    Colour
    ...    Ethnicity group
    ...    Gender
    ...    Level of learning
    ...    Number of years after achievement of learning aim
    ...    Provision
    ...    parent=id:dataSetDetails

    user checks summary list contains    Time period    2012/13    id:dataSetDetails

User verifies 'API version history' section
    user checks table column heading contains    1    1    Version    id:apiVersionHistory
    user checks table column heading contains    1    2    Release    id:apiVersionHistory
    user checks table column heading contains    1    3    Status    id:apiVersionHistory

    user checks table cell contains    1    1    1.0 (current)    id:apiVersionHistory
    user checks table cell contains    1    2    ${RELEASE_NAME}    id:apiVersionHistory
    user checks table cell contains    1    3    Published    id:apiVersionHistory

User checks the published API data set via the API
    ${json}=    wait until keyword succeeds    10x    %{WAIT_SMALL}s
    ...    user checks API data set is published    ${API_DATA_SET_ID}
    should be equal    ${json['title']}    ${SUBJECT_NAME}
    should be equal    ${json['latestVersion']['version']}    1.0

User lists the published API data set versions via the API
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}/versions
    user checks response status code    ${response}    200
    ${json}=    response should be json    ${response}
    paginated response should contain results    ${json}    1
    should be equal    ${json['results'][0]['version']}    1.0
    should be equal    ${json['results'][0]['status']}    Published

User gets the published API data set version via the API
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}/versions/1.0
    user checks response status code    ${response}    200
    ${json}=    response should be json    ${response}
    should be equal    ${json['version']}    1.0
    should be equal    ${json['status']}    Published

User queries the published API data set via the API for a known result
    ${meta}=    user gets api data set meta via api    ${API_DATA_SET_ID}
    ${median_id}=    user gets API data set indicator id    ${meta}    Median annualised earnings
    ${gender_id}=    user gets API data set filter option id    ${meta}    Gender    Total
    ${ethnicity_id}=    user gets API data set filter option id    ${meta}    Ethnicity group    Total
    ${provision_id}=    user gets API data set filter option id    ${meta}    Provision    Total
    ${level_id}=    user gets API data set filter option id    ${meta}    Level of learning    Total
    ${years_id}=    user gets API data set filter option id    ${meta}
    ...    Number of years after achievement of learning aim    1 year after study
    ${colour_id}=    user gets API data set filter option id    ${meta}    Colour    Orange
    ${cheese_id}=    user gets API data set filter option id    ${meta}    Cheese    Feta

    # These filters narrow the data set down to a single row of the source CSV
    # (the first data row of seven_filters.csv, where Median is 3).
    ${body}=    catenate    SEPARATOR=
    ...    {"criteria":{"and":[
    ...    {"filters":{"eq":"${gender_id}"}},
    ...    {"filters":{"eq":"${ethnicity_id}"}},
    ...    {"filters":{"eq":"${provision_id}"}},
    ...    {"filters":{"eq":"${level_id}"}},
    ...    {"filters":{"eq":"${years_id}"}},
    ...    {"filters":{"eq":"${colour_id}"}},
    ...    {"filters":{"eq":"${cheese_id}"}}
    ...    ]},"indicators":["${median_id}"]}

    ${response}=    user makes post request to public api    /data-sets/${API_DATA_SET_ID}/query
    ...    body=${body}
    user checks response status code    ${response}    200
    ${json}=    response should be json    ${response}
    paginated response should contain results    ${json}    1
    should be equal    ${json['results'][0]['values']['${median_id}']}    3

User queries the published API data set via the API using a GET query
    &{params}=    create dictionary    indicators=${MEDIAN_INDICATOR_ID}
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}/query
    ...    params=${params}
    user checks response status code    ${response}    200
    ${json}=    response should be json    ${response}
    paginated response should contain results    ${json}    3270

User downloads the published API data set CSV via the API
    ${response}=    user makes get request to public api    /data-sets/${API_DATA_SET_ID}/csv
    user checks response status code    ${response}    200
    should contain    ${response.headers['Content-Type']}    text/csv
    ${row_count}=    evaluate    len($response.text.strip().splitlines())
    should be equal as integers    ${row_count}    3271


*** Keywords ***
user checks API data set is published
    [Arguments]    ${data_set_id}
    ${response}=    user makes get request to public api    /data-sets/${data_set_id}
    user checks response status code    ${response}    200
    ${json}=    response should be json    ${response}
    should be equal    ${json['status']}    Published
    RETURN    ${json}
