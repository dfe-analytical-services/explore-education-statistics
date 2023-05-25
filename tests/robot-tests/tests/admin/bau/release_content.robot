*** Settings ***
Library             Collections
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}                 UI tests - release content %{RUN_IDENTIFIER}
${SECONDARY_STATS_TABLE_TAB_ID}     releaseHeadlines-dataBlock-tables-tab


*** Test Cases ***
Create test publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2025

Upload a subject
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2025/26

    user uploads subject    Dates test subject    dates.csv    dates.meta.csv

Create 4 data blocks
    user creates data block for dates csv
    ...    Dates test subject
    ...    Data Block 1
    ...    Data Block 1 title
    user creates data block for dates csv
    ...    Dates test subject
    ...    Data Block 2
    ...    Data Block 2 title
    user creates key stats data block for dates csv
    ...    Dates test subject
    ...    Key Stats Data Block 1
    ...    Key Stats Data Block 1 title
    ...    Proportion of settings open
    ...    1%
    user creates key stats data block for dates csv
    ...    Dates test subject
    ...    Key Stats Data Block 2
    ...    Key Stats Data Block 2 title
    ...    Number of open settings
    ...    22,900

Navigate to 'Content' page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2025/26
    user clicks link    Content
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until h2 is visible    ${PUBLICATION_NAME}

Add summary content to release
    user closes Set Page View box
    user adds summary text block
    user adds content to summary text block    Test intro text for ${PUBLICATION_NAME}

Add release note to release
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note one
    user clicks button    Save note
    ${date}    get current datetime    %-d %B %Y
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) p    Test release note one

Add Useful information related page link to release
    user clicks button    Add related page link
    user enters text into element    id:relatedPageForm-description    Test link one
    user enters text into element    id:relatedPageForm-url    http://test1.example.com/test1
    user clicks element    //button[text()="Create link"]    //*[@id="relatedPageForm"]
    user checks page does not contain element    id:${SECONDARY_STATS_TABLE_TAB_ID}

Add secondary statistics
    user clicks button    Add secondary stats
    user waits until page contains element    name:selectedDataBlock    %{WAIT_MEDIUM}
    user checks select contains x options    name:selectedDataBlock    5
    user checks select contains option    name:selectedDataBlock    Select a data block
    user checks select contains option    name:selectedDataBlock    Data Block 1
    user checks select contains option    name:selectedDataBlock    Data Block 2
    user checks select contains option    name:selectedDataBlock    Key Stats Data Block 1
    user checks select contains option    name:selectedDataBlock    Key Stats Data Block 2
    user chooses and embeds data block    Data Block 1

Check secondary statistics are included correctly
    user waits until element is visible    //*[@id="${SECONDARY_STATS_TABLE_TAB_ID}"]    %{WAIT_MEDIUM}
    user scrolls to element    //*[@id="${SECONDARY_STATS_TABLE_TAB_ID}"]
    user clicks element    //*[@id="${SECONDARY_STATS_TABLE_TAB_ID}"]
    user checks page contains    Data Block 1 title
    user checks page contains element    css:table
    user checks page contains button    Change secondary stats
    user checks page contains button    Remove secondary stats

Change secondary statistics
    user clicks button    Change secondary stats
    user waits until page contains element    name:selectedDataBlock    %{WAIT_MEDIUM}
    user checks select contains x options    name:selectedDataBlock    4
    user checks select contains option    name:selectedDataBlock    Select a data block
    user checks select contains option    name:selectedDataBlock    Data Block 2
    user checks select contains option    name:selectedDataBlock    Key Stats Data Block 1
    user checks select contains option    name:selectedDataBlock    Key Stats Data Block 2
    user chooses and embeds data block    Data Block 2
    user waits until page does not contain loading spinner
    user waits until element is visible    //*[@id="${SECONDARY_STATS_TABLE_TAB_ID}"]    %{WAIT_MEDIUM}
    user checks page contains    Data Block 2 title
    user checks page contains button    Change secondary stats
    user checks page contains button    Remove secondary stats

Remove secondary statistics
    user clicks button    Remove secondary stats
    user waits until modal is visible    Remove secondary statistics section
    user clicks button    Confirm
    user waits until modal is not visible    Remove secondary statistics section    %{WAIT_MEDIUM}
    user waits until page does not contain element    //*[@id="${SECONDARY_STATS_TABLE_TAB_ID}"]
    ...    %{WAIT_MEDIUM}
    user checks page does not contain button    Change secondary stats
    user checks page does not contain button    Remove secondary stats
    user checks page contains button    Add secondary stats

Add a key statistics data block tile
    user clicks button    Add key statistic from data block
    user waits until page contains element    name:selectedDataBlock    %{WAIT_MEDIUM}
    user checks select contains x options    name:selectedDataBlock    3
    user checks select contains option    name:selectedDataBlock    Select a data block
    user checks select contains option    name:selectedDataBlock    Key Stats Data Block 1
    user checks select contains option    name:selectedDataBlock    Key Stats Data Block 2
    user chooses and embeds data block    Key Stats Data Block 1
    user waits until page contains    Proportion of settings open    %{WAIT_MEDIUM}
    user checks page contains    1%

Edit the guidance information for the key statistics tile
    user clicks the nth key stats tile button    1    Edit
    user enters text into element    css:input[name="trend"]    Down from last year
    user enters text into element    label:Guidance title    Learn more about open settings
    # Tab into the CK Editor guidance text editor
    user presses keys    TAB
    user presses keys    Some information about about open settings
    user clicks the nth key stats tile button    1    Save
    user waits until page does not contain element    css:input[name="trend"]    %{WAIT_MEDIUM}

Check the guidance information for the key statistics tile
    user waits until page contains    Down from last year    %{WAIT_MEDIUM}
    user checks page contains    Some information about about open settings
    user checks page for details dropdown    Learn more about open settings
    user opens details dropdown    Learn more about open settings
    user checks page contains    Some information about about open settings

Add another key statistics data block tile
    user clicks button    Add key statistic from data block
    user waits until page contains element    name:selectedDataBlock    %{WAIT_MEDIUM}
    user checks select contains x options    name:selectedDataBlock    2
    user checks select contains option    name:selectedDataBlock    Select a data block
    user checks select contains option    name:selectedDataBlock    Key Stats Data Block 2
    user chooses and embeds data block    Key Stats Data Block 2
    user checks page contains    Number of open settings
    user checks page contains    22,900

Remove newly added key statistics data block tile
    user clicks the nth key stats tile button    2    Remove
    user waits until page does not contain    Number of open settings    %{WAIT_MEDIUM}
    user checks page does not contain    22,900

    # Make sure the first key stat tile is still there
    user checks page contains    Proportion of settings open

Add free text key stat
    user adds free text key stat    Free text key stat title    9001%    Trend    Guidance title    Guidance text

    user checks element count is x    testid:keyStat    2
    user checks key stat contents    1    Proportion of settings open    1%    Down from last year
    user checks key stat guidance    1    Learn more about open settings    Some information about about open settings
    user checks key stat contents    2    Free text key stat title    9001%    Trend
    user checks key stat guidance    2    Guidance title    Guidance text

Update free text key stat
    user updates free text key stat    2    Updated title    9002%    Updated trend    Updated guidance title
    ...    Updated guidance text

    user checks element count is x    testid:keyStat    2
    user checks key stat contents    1    Proportion of settings open    1%    Down from last year
    user checks key stat guidance    1    Learn more about open settings    Some information about about open settings
    user checks key stat contents    2    Updated title    9002%    Updated trend
    user checks key stat guidance    2    Updated guidance title    Updated guidance text

Remove free text key stat
    user removes key stat    2
    user waits until page does not contain    Updated title

    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Proportion of settings open    1%    Down from last year
    user checks key stat guidance    1    Learn more about open settings    Some information about about open settings

Add key statistics summary content to release
    user adds headlines text block
    user adds content to headlines text block    Test key statistics summary text for ${PUBLICATION_NAME}

Add accordion sections to release
    user creates new content section    1    Test section one
    user creates new content section    2    Test section two
    user creates new content section    3    Test section three

Add content blocks to Test section one
    user adds text block to editable accordion section    Test section one    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test section one    1    block one test text
    ...    id:releaseMainContent

    user adds text block to editable accordion section    Test section one    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test section one    2    block two test text
    ...    id:releaseMainContent

    user adds text block to editable accordion section    Test section one    id:releaseMainContent
    user adds content to autosaving accordion section text block    Test section one    3    block three test text
    ...    id:releaseMainContent

    user checks accordion section contains X blocks    Test section one    3    id:releaseMainContent

Delete second content block
    user deletes editable accordion section content block    Test section one    2    id:releaseMainContent
    user waits until page does not contain    block two test text
    user checks accordion section contains X blocks    Test section one    2    id:releaseMainContent

Validate two remaining content blocks
    user checks accordion section text block contains    Test section one    1    block one test text
    ...    id:releaseMainContent
    user checks accordion section text block contains    Test section one    2    block three test text
    ...    id:releaseMainContent


*** Keywords ***
user clicks the nth key stats tile button
    [Arguments]
    ...    ${tile_number}
    ...    ${button_text}
    user clicks element    //*[@data-testid="keyStat"][${tile_number}]//button[contains(., "${button_text}")]
