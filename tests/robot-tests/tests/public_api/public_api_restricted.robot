
*** Settings ***
Library             ../libs/admin_api.py
Resource            ../libs/admin-common.robot

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
Create publication
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year    3000-01    Accredited official statistics

Create new release with data files
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
    

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Approve first release
    user clicks link    Sign off
    user approves release for immediate publication

Navigate to admin and create an amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Change the Release type
    user waits until page contains link    Edit release summary
    user clicks link    Edit release summary
    user waits until page finishes loading
    user waits until h2 is visible    Edit release summary
    user checks page contains radio    Official statistics in development
    user clicks radio    Official statistics in development
    user clicks button    Update release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year
    ...    3000-01
    ...    Official statistics in development

Upload third subject(large data file)
    user waits until large data upload is completed
    ...    ${SUBJECT_NAME_3}
    ...    data-upload-import.csv
    ...    data-upload-import.meta.csv

Add data guidance to third subject
    user clicks link    Data guidance
    user enters text into data guidance data file content editor    ${SUBJECT_NAME_3}    meta content
    user clicks button    Save guidance

Create an API dataset through the amendment
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user clicks button    Create API data set
    ${modal}=    user waits until modal is visible    Create a new API data set
    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_3}
    user clicks button    Confirm new API data set

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set    %{WAIT_LONG}

User waits until the 2nd API dataset status changes to 'Failed'
    user waits until h3 is visible    Draft version details
    wait until keyword succeeds    20x    5s    Verify status of API Datasets    Failed

Verify the contents inside the 'Draft API datasets' table
    user clicks link    Back to API data sets
    user waits until h3 is visible    Draft API data sets

    user checks table column heading contains    1    1    Draft version    xpath://table[@data-testid='draft-api-data-sets']
    user checks table column heading contains    1    2    Name             xpath://table[@data-testid='draft-api-data-sets']
    user checks table column heading contains    1    3    Status           xpath://table[@data-testid='draft-api-data-sets']
    user checks table column heading contains    1    4    Actions          xpath://table[@data-testid='draft-api-data-sets']


    user checks table cell contains    1    1    v1.0    xpath://table[@data-testid='draft-api-data-sets']
    user checks table cell contains    1    3    Failed    xpath://table[@data-testid='draft-api-data-sets']

Add release note for new release amendment
    user clicks link    Content
    user adds a release note    Test release note two

    ${date}    get current datetime    %-d %B %Y
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note two

Validate checklist error while API data set is still processing or being failed
    user edits release status
    user checks checklist errors contains
    ...    1 issue that must be resolved before this release can be published.
    user checks checklist errors contains either link
    ...    All public API data set processing must be completed
    ...    All failed public API data sets must be retried or removed

Create a second draft release via api
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year    3010

Upload subject to second release
    user uploads subject    ${SUBJECT_NAME_4}    seven_filters_minor_update.csv    seven_filters_minor_update.meta.csv

Add data guidance to second release
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_4}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_4}
    ...    ${SUBJECT_NAME_4} Main guidance content

Save data guidance
    user clicks button    Save guidance

Create a different version of an API dataset(Minor version)
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets
    
    user waits until h3 is visible    Current live API data sets

    user checks table column heading contains    1    1    Version    xpath://table[@data-testid="live-api-data-sets"]
    user clicks button in table cell    1    3    Create new version    xpath://table[@data-testid="live-api-data-sets"]

    ${modal}=    user waits until modal is visible    Create a new API data set version
    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_4}
    user clicks button    Confirm new data set version

    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set version    %{WAIT_LONG}

Validate the summary contents inside the 'draft version details' table
    user waits until h3 is visible    Draft version details
    user waits until element contains    css:dl[data-testid="draft-version-summary"] > div:nth-of-type(1) > dt + dd     v1.1    %{WAIT_LONG}
    user waits until element contains    css=dl[data-testid="draft-version-summary"] > div:nth-of-type(2) > dt + dd     Action required    %{WAIT_LONG}
    ${mapping_status}=    get text    css:dl[data-testid="draft-version-summary"] > div:nth-of-type(2) > dt + dd
    should be equal as strings    ${mapping_status}    Action required

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Validate checklist error for a draft API dataset which shows mapping error
    user edits release status
    user checks checklist errors contains
    ...    1 issue that must be resolved before this release can be published.
    user checks checklist errors contains link
    ...    All public API data set mappings must be completed

Create a third draft release via api
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year    3020

Upload subject to third release
    user uploads subject    ${SUBJECT_NAME_5}    institution_and_provider.csv    institution_and_provider.meta.csv

Add data guidance to third release
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME_5}

    user enters text into data guidance data file content editor    ${SUBJECT_NAME_5}
    ...    ${SUBJECT_NAME_5} Main guidance content


Save data guidance
    user clicks button    Save guidance

Create a different version of API dataset (major version)
    user scrolls to the top of the page
    user clicks link    API data sets
    user waits until h2 is visible    API data sets

    user waits until h3 is visible    Current live API data sets

    user checks table column heading contains    1    1    Version    xpath://table[@data-testid="live-api-data-sets"]
    user clicks button in table cell    1    3    Create new version    xpath://table[@data-testid="live-api-data-sets"]

    ${modal}=    user waits until modal is visible    Create a new API data set version

    user chooses select option    id:apiDataSetCreateForm-releaseFileId   ${SUBJECT_NAME_5}
    user clicks button    Confirm new data set version


    user waits until page finishes loading
    user waits until modal is not visible    Create a new API data set version    %{WAIT_LONG}

Validate the summary contents inside the 'draft version details' table
    user waits until h3 is visible    Draft version details
    user waits until element contains    css:dl[data-testid="draft-version-summary"] > div:nth-of-type(1) > dt + dd     v2.0    %{WAIT_LONG}
    user waits until element contains    css:dl[data-testid="draft-version-summary"] > div:nth-of-type(2) > dt + dd     Action required    %{WAIT_LONG}

    ${mapping_status}=    get text    css:dl[data-testid="draft-version-summary"] > div:nth-of-type(2) > dt + dd
    should be equal as strings    ${mapping_status}    Action required

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Validate checklist error for a draft API dataset which shows mapping error
    user edits release status
    user checks checklist errors contains
    ...    1 issue that must be resolved before this release can be published.
    user checks checklist errors contains link
    ...    All public API data set mappings must be completed









*** Keywords ***
user edits release status
    user clicks link    Sign off
    user waits until h2 is visible    Sign off    %{WAIT_SMALL}

    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status    %{WAIT_SMALL}


user checks checklist errors contains
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until element contains    testid:releaseChecklist-errors    ${text}

user checks checklist errors contains link
    [Arguments]    ${text}
    user waits until page contains testid    releaseChecklist-errors
    user waits until parent contains element    testid:releaseChecklist-errors    link:${text}

User Checks Checklist Errors Contains Either Link
    [Arguments]    ${text1}    ${text2}
    user waits until page contains testid    releaseChecklist-errors
    ${status}=    Run Keyword And Return Status    Wait Until Keyword Succeeds    10s    1s    Check Either Link Exists    ${text1}    ${text2}

    Run Keyword If    ${status} == False    Fail    Neither of the expected links (${text1}, ${text2}) was found. Failing fast as required.
    Log    One of the expected links (${text1}, ${text2}) is present.

Check Either Link Exists
    [Arguments]    ${text1}    ${text2}
    ${condition1}=    Run Keyword And Return Status    user waits until parent contains element without retries    testid:releaseChecklist-errors    link:${text1}    timeout=5s
    Run Keyword If    ${condition1}    Set Test Variable    ${link_found}    True

    ${condition2}=    Run Keyword And Return Status    user waits until parent contains element without retries    testid:releaseChecklist-errors    link:${text2}    timeout=5s
    Run Keyword If    ${condition1} == False    Set Test Variable    ${link_found}    ${condition2}

    ${link_found}=    Evaluate    ${condition1} or ${condition2}
    Run Keyword If    ${link_found} == False    Log    Neither link '${text1}' nor '${text2}' was found after checking both. Continuing to check...

    [Return]    ${link_found}


Verify status of API Datasets
    [Arguments]    ${expected_status}
    user waits for caches to expire
    ${status_value}=    get text    xpath:(//div[@data-testid="Status"]//dd[@data-testid="Status-value"]//strong)[2]
    should be equal as strings    ${status_value}    ${expected_status}
