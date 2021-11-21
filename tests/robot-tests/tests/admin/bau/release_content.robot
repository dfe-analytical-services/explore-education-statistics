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
${TOPIC_NAME}           %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}     UI tests - release content %{RUN_IDENTIFIER}

*** Test Cases ***
Create test publication and release via API
    ${PUBLICATION_ID}    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    AY    2025
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    Academic Year 2025/26 (not Live)

Upload a subject
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
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    Academic Year 2025/26 (not Live)
    user clicks link    Content
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until h2 is visible    ${PUBLICATION_NAME}

Add summary content to release
    user clicks button    Add a summary text block
    user waits until element contains    id:releaseSummary    This section is empty
    user clicks button    Edit block    id:releaseSummary
    user presses keys    Test intro text for ${PUBLICATION_NAME}
    user clicks button    Save    id:releaseSummary
    user waits until element contains    id:releaseSummary    Test intro text for ${PUBLICATION_NAME}

# TODO: Add comment to summary content

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
    user clicks button    Create link

Add secondary statistics
    user waits until the page does not contain the secondary statistics table tab
    user clicks button    Add secondary stats
    user checks select contains x options    css:select[name="selectedDataBlock"]    5
    user checks select contains option    css:select[name="selectedDataBlock"]    Select a data block
    user checks select contains option    css:select[name="selectedDataBlock"]    Data Block 1
    user checks select contains option    css:select[name="selectedDataBlock"]    Data Block 2
    user checks select contains option    css:select[name="selectedDataBlock"]    Key Stats Data Block 1
    user checks select contains option    css:select[name="selectedDataBlock"]    Key Stats Data Block 2
    user chooses and embeds data block    Data Block 1
    user waits until the page contains the secondary statistics table tab

Check secondary statistics are included correctly
    user clicks the secondary statistics table tab
    user checks page contains    Data Block 1 title
    user checks page contains element    css:table
    user waits until page contains button    Change secondary stats
    user waits until page contains button    Remove secondary stats

Change secondary statistics
    user clicks button    Change secondary stats
    user checks select contains x options    css:select[name="selectedDataBlock"]    4
    user checks select contains option    css:select[name="selectedDataBlock"]    Select a data block
    user checks select contains option    css:select[name="selectedDataBlock"]    Data Block 2
    user checks select contains option    css:select[name="selectedDataBlock"]    Key Stats Data Block 1
    user checks select contains option    css:select[name="selectedDataBlock"]    Key Stats Data Block 2
    user chooses and embeds data block    Data Block 2
    user waits until the page contains the secondary statistics table tab
    user checks page contains    Data Block 2 title
    user waits until page contains button    Change secondary stats
    user waits until page contains button    Remove secondary stats

Remove secondary statistics
    user clicks button    Remove secondary stats
    user waits until modal is visible    Remove secondary statistics section
    user clicks button    Confirm
    user waits until modal is not visible    Remove secondary statistics section
    user waits until the page does not contain the secondary statistics table tab
    user waits until page does not contain button    Change secondary stats
    user waits until page does not contain button    Remove secondary stats
    user waits until page contains button    Add secondary stats

Add a key statistics tile
    user clicks button    Add key statistic
    user checks select contains x options    css:select[name="selectedDataBlock"]    3
    user checks select contains option    css:select[name="selectedDataBlock"]    Select a data block
    user checks select contains option    css:select[name="selectedDataBlock"]    Key Stats Data Block 1
    user checks select contains option    css:select[name="selectedDataBlock"]    Key Stats Data Block 2
    user chooses and embeds data block    Key Stats Data Block 1
    user waits until page contains    Proportion of settings open
    user waits until page contains    1%

Edit the guidance information for the key statistics tile
    user clicks element    //*[@data-testid="keyStat"][1]//button[.="Edit"]
    user enters text into element    css:input[name="dataSummary"]    Down from last year
    user enters text into element    label:Guidance title    Learn more about open settings
    # Tab into the CK Editor guidance text editor
    user presses keys    TAB
    user presses keys    Some information about about open settings
    user clicks element    //*[@data-testid="keyStat"][1]//button[.="Save"]
    user waits until page does not contain element    css:input[name="dataSummary"]

Check the guidance information for the key statistics tile
    user waits until page contains    Down from last year
    user waits until page contains    Some information about about open settings
    user checks page for details dropdown    Learn more about open settings
    user opens details dropdown    Learn more about open settings
    user checks page contains    Some information about about open settings

Add another key statistics tile
    user clicks button    Add another key statistic
    user checks select contains x options    css:select[name="selectedDataBlock"]    2
    user checks select contains option    css:select[name="selectedDataBlock"]    Select a data block
    user checks select contains option    css:select[name="selectedDataBlock"]    Key Stats Data Block 2
    user chooses and embeds data block    Key Stats Data Block 2
    user waits until page contains    Number of open settings
    user waits until page contains    22,900

Remove a key statistics tile
    # Remove the second tile
    user clicks element    //*[@data-testid="keyStat"][2]//button[.="Remove"]
    user waits until page does not contain    Number of open settings
    user waits until page does not contain    22,900
    # Make sure the first key stat tile is still there
    user checks page contains    Proportion of settings open

Add key statistics summary content to release
    user clicks button    Add a headlines text block    id:releaseHeadlines
    user waits until element contains    id:releaseHeadlines    This section is empty
    user clicks button    Edit block    id:releaseHeadlines
    user presses keys    Test key statistics summary text for ${PUBLICATION_NAME}
    user clicks button    Save    id:releaseHeadlines

    user waits until page contains    Test key statistics summary text for ${PUBLICATION_NAME}

Add accordion sections to release
    user creates new content section    1    Test section one
    user creates new content section    2    Test section two
    user creates new content section    3    Test section three

Add content blocks to Test section one
    user adds text block to editable accordion section    Test section one    id:releaseMainContent
    user adds content to accordion section text block    Test section one    1    block one test text
    ...    id:releaseMainContent

    user adds text block to editable accordion section    Test section one    id:releaseMainContent
    user adds content to accordion section text block    Test section one    2    block two test text
    ...    id:releaseMainContent

    user adds text block to editable accordion section    Test section one    id:releaseMainContent
    user adds content to accordion section text block    Test section one    3    block three test text
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
user waits until the page contains the secondary statistics table tab
    user waits until page contains link    Table

user waits until the page does not contain the secondary statistics table tab
    user waits until page does not contain element    id:releaseHeadlines-dataBlock-tables-tab

user clicks the secondary statistics table tab
    user clicks element    id:releaseHeadlines-dataBlock-tables-tab
