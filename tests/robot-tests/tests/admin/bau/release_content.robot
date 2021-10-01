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

# TODO: Add Secondary Stats
# TODO: Add key statistics

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
