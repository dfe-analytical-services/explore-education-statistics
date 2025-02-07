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
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    AY    2025

Upload a subject
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    Academic year 2025/26

    user uploads subject and waits until complete    Dates test subject    dates.csv    dates.meta.csv

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
    user adds summary text block
    user adds content to summary text block    Test intro text for ${PUBLICATION_NAME}

Add release note to release
    user adds a release note    Test release note one
    ${date}=    get london date
    user waits until element contains    css:#release-notes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#release-notes li:nth-of-type(1) p    Test release note one

Add Useful information related page link to release
    user clicks button    Add related page
    user waits until modal is visible    Add related page link
    user enters text into element    id:relatedPageForm-description    Test link one
    user enters text into element    id:relatedPageForm-url    http://test1.example.com/test1
    user clicks button    Save    id:relatedPageForm
    user waits until modal is not visible    Add related page link

Edit related page link
    user clicks button    Edit pages
    user clicks button    Edit Test link one
    user enters text into element    id:relatedPageForm-description    Test link one - edited
    user clicks button    Save    id:relatedPageForm
    user clicks button    Close
    user checks page contains    Test link one - edited

Reorder related pages
    user clicks button    Add related page
    user waits until modal is visible    Add related page link
    user enters text into element    id:relatedPageForm-description    Test link two
    user enters text into element    id:relatedPageForm-url    http://test2.example.com/test2
    user clicks button    Save    id:relatedPageForm
    user waits until modal is not visible    Add related page link
    user clicks button    Edit pages
    user clicks button    Reorder pages
    user moves item of draggable list down    testid:reorder-related-pages    1
    user checks list contains exact items in order    testid:reorder-related-pages
    ...    Test link two
    ...    Test link one - edited
    user clicks button    Confirm order
    user clicks button    Close

Add secondary statistics
    ${expected_select_options}=    create list    Data Block 1    Data Block 2    Key Stats Data Block 1
    ...    Key Stats Data Block 2
    user adds secondary stats data block
    ...    Data Block 1
    ...    ${expected_select_options}

Check secondary statistics are included correctly
    user waits until element is visible    id:${SECONDARY_STATS_TABLE_TAB_ID}    %{WAIT_MEDIUM}
    user clicks element    id:${SECONDARY_STATS_TABLE_TAB_ID}
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
    # Fix for bug whereby occasionally the secondary stats block fails to lazy-load until
    # scrolling is performed.    EES-5856 is raised to try and address this.
    user scrolls down    20
    user waits until element is visible    id:${SECONDARY_STATS_TABLE_TAB_ID}    %{WAIT_MEDIUM}
    user checks page contains    Data Block 2 title
    user checks page contains button    Change secondary stats
    user checks page contains button    Remove secondary stats

Remove secondary statistics
    user clicks button    Remove secondary stats
    user waits until modal is visible    Remove secondary statistics section
    user clicks button    Confirm
    user waits until modal is not visible    Remove secondary statistics section    %{WAIT_MEDIUM}
    user waits until page does not contain element    id:${SECONDARY_STATS_TABLE_TAB_ID}
    ...    %{WAIT_MEDIUM}
    user checks page does not contain button    Change secondary stats
    user checks page does not contain button    Remove secondary stats
    user checks page contains button    Add secondary stats

Add a key statistics data block tile
    ${expected_select_options}=    create list    Key Stats Data Block 1    Key Stats Data Block 2
    user adds key statistic from data block
    ...    Key Stats Data Block 1
    ...    Down from last year
    ...    Learn more about open settings
    ...    Some information about about open settings
    ...    ${expected_select_options}
    ...    Proportion of settings open
    ...    1%

Check the guidance information for the key statistics tile
    user waits until page contains    Down from last year    %{WAIT_MEDIUM}
    user checks page contains    Some information about about open settings
    user checks page for details dropdown    Learn more about open settings
    user opens details dropdown    Learn more about open settings
    user checks page contains    Some information about about open settings

Add another key statistics data block tile
    ${expected_select_options}=    create list    Key Stats Data Block 2
    user adds key statistic from data block
    ...    Key Stats Data Block 2
    ...    ${EMPTY}
    ...    ${EMPTY}
    ...    ${EMPTY}
    ...    ${expected_select_options}
    ...    Number of open settings
    ...    22,900

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
    user adds text block to editable accordion section    Test section one    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test section one    1    block one test text
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user scrolls up    100
    user adds text block to editable accordion section    Test section one    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test section one    2    block two test text
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user adds text block to editable accordion section    Test section one    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    Test section one    3    block three test text
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user checks accordion section contains X blocks    Test section one    3    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Delete second content block
    user deletes editable accordion section content block    Test section one    2
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user waits until page does not contain    block two test text
    user checks accordion section contains X blocks    Test section one    2    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Validate two remaining content blocks
    user checks accordion section text block contains    Test section one    1    block one test text
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user checks accordion section text block contains    Test section one    2    block three test text
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Verify that validation prevents adding an image without alt text
    user scrolls up    100
    user adds image without alt text to accordion section text block    Test section one    1
    ...    test-infographic.png    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user checks page contains    1 image does not have alternative text.

    user clicks element    xpath://img    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user clicks button    Change image text alternative
    user enters text into element    label:Text alternative    Alt text for the uploaded content image
    user clicks element    css:button.ck-button-save

    user checks accordion section text block contains image with alt text    Test section one    1
    ...    Alt text for the uploaded content image    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user clicks button    Save & close

    user checks page does not contain    1 image does not have alternative text.

Verify that validation prevents adding an invalid link
    user adds link to accordion section text block    Test section one    1    https://gov
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user checks page contains    1 link has an invalid URL.

    user clicks element    xpath://a[text()='Link text']
    user clicks button    Edit link
    user enters text into element    label:Link URL    gov.uk
    user presses keys    ENTER

    user clicks button    Save & close

    user checks page does not contain    1 link has an invalid URL.
