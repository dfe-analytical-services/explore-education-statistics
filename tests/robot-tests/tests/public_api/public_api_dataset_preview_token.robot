*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot
Resource            ../libs/admin/manage-content-common.robot
Resource            ../libs/public-api-common.robot
Resource            ../libs/public-common.robot

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser



*** Variables ***
${PUBLICATION_NAME}=    UI tests - public api restricted %{RUN_IDENTIFIER}
${RELEASE_NAME}=        Financial year 3000-01
${SUBJECT_NAME_1}=      UI test subject 1
${SUBJECT_NAME_2}=      UI test subject 2
${SUBJECT_NAME_3}=      UI test subject 3
${SUBJECT_NAME_4}=      UI test subject 4
${SUBJECT_NAME_5}=      UI test subject 5



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

Create 1st API dataset
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_1}
    user clicks button    Confirm new API data set

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set    %{WAIT_LONG}

User waits until the 1st API dataset status changes to 'Ready'
    user waits until h3 is visible    Draft version details
    wait until keyword succeeds    10x    5s    Verify status of API Datasets    Ready

Create 2nd API dataset
    user clicks link    Back to API data sets
    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_2}
    user clicks button    Confirm new API data set

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set    %{WAIT_LONG}

User waits until the 2nd API dataset status changes to 'Ready'
    user waits until h3 is visible    Draft version details
    wait until keyword succeeds    10x    5s    Verify status of API Datasets    Ready

Verify the contents inside the 'Draft API datasets' table
    user clicks link    Back to API data sets
    user waits until h3 is visible    Draft API data sets


    user checks table column heading contains    1    1    Draft version    xpath://table[@data-testid='draft-api-data-sets']
    user checks table column heading contains    1    2    Name             xpath://table[@data-testid='draft-api-data-sets']
    user checks table column heading contains    1    3    Status           xpath://table[@data-testid='draft-api-data-sets']
    user checks table column heading contains    1    4    Actions          xpath://table[@data-testid='draft-api-data-sets']


    user checks table cell contains    1    1    v1.0    xpath://table[@data-testid='draft-api-data-sets']
    user checks table cell contains    1    3    Ready    xpath://table[@data-testid='draft-api-data-sets']

    user checks table cell contains    2    1    v1.0    xpath://table[@data-testid='draft-api-data-sets']
    user checks table cell contains    2    3    Ready    xpath://table[@data-testid='draft-api-data-sets']


Click on 'View Details' link(First API dataset)
    user clicks link in table cell    1    4    View details    xpath://table[@data-testid='draft-api-data-sets']
    user waits until h3 is visible    Draft version details
    user checks row data heading in the Draft version details table

User checks row data contents inside the 'Draft API datasets' summary table
    user checks contents inside the cell value    v1.0                                       xpath://dl[@data-testid="draft-version-summary"]/div/dd[@data-testid='Version-value']/strong
    user checks contents inside the cell value    Ready                                      xpath:(//div[@data-testid="Status"]//dd[@data-testid="Status-value"]//strong)[2]
    user checks contents inside the cell value    Financial year 3000-01                     xpath:(//div[@data-testid="Release"]//dd[@data-testid="Release-value"]//a)[1]
    user checks contents inside the cell value    ${SUBJECT_NAME_1}                          xpath://div[@data-testid="Data set file"]//dd[@data-testid="Data set file-value"]
    user checks contents inside the cell value    National                                   xpath://div[@data-testid="Geographic levels"]//dd[@data-testid="Geographic levels-value"]
    user checks contents inside the cell value    2012/13                                    xpath://div[@data-testid="Time periods"]//dd[@data-testid="Time periods-value"]


    user checks contents inside the cell value    Lower quartile annualised earnings         xpath://div[@data-testid="Indicators"]//dd[@data-testid="Indicators-value"]/ul/li[1]
    user checks contents inside the cell value    Median annualised earnings                 xpath://div[@data-testid="Indicators"]//dd[@data-testid="Indicators-value"]/ul/li[2]
    user checks contents inside the cell value    Number of learners with earnings           xpath://div[@data-testid="Indicators"]//dd[@data-testid="Indicators-value"]/ul/li[3]

    user clicks button                              Show 1 more indicator                    xpath://div[@data-testid="Indicators"]//dd[@data-testid="Indicators-value"]

    user checks contents inside the cell value      Upper quartile annualised earnings       xpath://div[@data-testid="Indicators"]//dd[@data-testid="Indicators-value"]/ul/li[4]

    user checks contents inside the cell value    	Cheese                                   xpath://div[@data-testid="Filters"]//dd[@data-testid="Filters-value"]/ul/li[1]
    user checks contents inside the cell value    	Colour                                   xpath://div[@data-testid="Filters"]//dd[@data-testid="Filters-value"]/ul/li[2]
    user checks contents inside the cell value    	Ethnicity group                          xpath://div[@data-testid="Filters"]//dd[@data-testid="Filters-value"]/ul/li[3]

    user clicks button                              Show 4 more filters                      xpath://div[@data-testid="Filters"]//dd[@data-testid="Filters-value"]

    user checks contents inside the cell value    	Gender                                    xpath://div[@data-testid="Filters"]//dd[@data-testid="Filters-value"]/ul/li[4]
    user checks contents inside the cell value    	Level of learning                         xpath://div[@data-testid="Filters"]//dd[@data-testid="Filters-value"]/ul/li[5]
    user checks contents inside the cell value      Number of years after achievement of learning aim    xpath://div[@data-testid="Filters"]//dd[@data-testid="Filters-value"]/ul/li[6]
    user checks contents inside the cell value      Provision                                 xpath://div[@data-testid="Filters"]//dd[@data-testid="Filters-value"]/ul/li[7]


    user checks contents inside the cell value      Preview API data set                      xpath://div[@data-testid="Actions"]//dd[@data-testid="Actions-value"]/ul/li[1]/a
    user checks contents inside the cell value      View preview token log                    xpath://div[@data-testid="Actions"]//dd[@data-testid="Actions-value"]/ul/li[2]/a
    user checks contents inside the cell value      Remove draft version                      xpath://div[@data-testid="Actions"]//dd[@data-testid="Actions-value"]/ul/li[3]/button


User clicks on 'Preview API dataset' link
    user clicks link containing text    Preview API data set
    
User click on 'Generate Token'
    user clicks button     Generate token

User creates API token through 'Generate API token' modal window
    ${modal}=    user waits until modal is visible    Generate API token
    user enters text into element    css:input[id="apiDataSetTokenCreateForm-label"]    API Token
    user clicks checkbox by selector        css:input[id="apiDataSetTokenCreateForm-agreeTerms"]
    user clicks button    Continue

    user waits until page finishes loading
    user waits until modal is not visible    Generate API token    %{WAIT_LONG}
    user waits until page contains    API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME_1}

User revokes created API token
    user clicks button    Revoke this token
    ${modal}=    user waits until modal is visible    Revoke this token
    user clicks button    Confirm
    user waits until page finishes loading
    user waits until modal is not visible    Revoke this token    %{WAIT_LONG}
    user waits until page contains    Generate API data set preview token

Again,user click on 'Generate Token'
    user clicks button     Generate token

User creates API token through 'Generate API token' modal window
    ${modal}=    user waits until modal is visible    Generate API token
    user enters text into element    css:input[id="apiDataSetTokenCreateForm-label"]    API Token
    user clicks checkbox by selector        css:input[id="apiDataSetTokenCreateForm-agreeTerms"]
    user clicks button    Continue

    user waits until page finishes loading
    user waits until modal is not visible    Generate API token    %{WAIT_LONG}
    user waits until page contains    API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME_1}

User cancells to revoke created API token
    user clicks button    Revoke this token
    ${modal}=    user waits until modal is visible    Revoke this token
    user clicks button    Cancel
    user waits until page finishes loading
    user waits until modal is not visible    Revoke this token    %{WAIT_LONG}
    user waits until page contains    API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME_1}

User cancells to create API token
    user clicks link    Back to API data set details
    user clicks link containing text    Preview API data set
    user clicks button     Generate token

    ${modal}=    user waits until modal is visible    Generate API token
    user enters text into element    css:input[id="apiDataSetTokenCreateForm-label"]    API Token
    user clicks checkbox by selector        css:input[id="apiDataSetTokenCreateForm-agreeTerms"]
    user clicks button    Cancel

    user waits until page finishes loading
    user waits until modal is not visible    Generate API token    %{WAIT_LONG}
    user waits until page contains    Generate API data set preview token
    user waits until h2 is visible    ${SUBJECT_NAME_1}

Verify the 'Active tokens' and 'Expired tokens' on preview token log page
    user clicks link    Back to API data set details
    user clicks link    View preview token log
    user waits until page contains    API data set preview token log
    user waits until h2 is visible    ${SUBJECT_NAME_1}

    user checks table column heading contains    1    1    Reference
    user checks table column heading contains    1    2    User
    user checks table column heading contains    1    3    Date generated
    user checks table column heading contains    1    4    Status
    user checks table column heading contains    1    5    Expiry
    user checks table column heading contains    1    6    Action

    user checks table cell contains    1    4    Active
    user checks table cell contains    2    4    Expired

    user clicks button in table cell    1    6    Revoke
    ${modal}=    user waits until modal is visible    Revoke this token
    user clicks button    Confirm    ${modal}
    user waits until page finishes loading
    user checks table cell contains    1    4    Expired

Add headline text block to Content page
    user clicks link    Back to API data set details
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user clicks link    Sign off
    user approves release for immediate publication

Verify newly published release is on Find Statistics page
    user checks publication is on find statistics page    ${PUBLICATION_NAME}

User navigates to data catalogue page
    user navigates to data catalogue page on public frontend

Search with 1st API dataset
    user clicks element    id:searchForm-search
    user presses keys    ${PUBLICATION_NAME}
    user clicks radio    API data sets only

    user waits until page finishes loading
    user clicks radio    Newest
    user checks page contains link    ${SUBJECT_NAME_1}
    user checks list item contains    testid:data-set-file-list    1    ${SUBJECT_NAME_1}
