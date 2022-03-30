*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

*** Variables ***
${RELEASE_NAME}=                Academic Year Q1 2020/21
${PUBLICATION_NAME}=            UI tests - comments %{RUN_IDENTIFIER}
${CONTENT_BLOCK_TITLE_1}=       First content section
${CONTENT_BLOCK_BODY_1}=        Adding some content that is incorrect

*** Test Cases ***
Create publication
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}
    ...    UI test topic %{RUN_IDENTIFIER}
    user clicks link    Create new publication
    user waits until h1 is visible    Create new publication
    user creates publication    ${PUBLICATION_NAME}

Create new release
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user clicks link    Create new release
    user creates release for publication    ${PUBLICATION_NAME}    Academic Year Q1    2020

Give analyst1 publication owner permissions to work on release
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Switch to analyst1 to work on release content blocks
    user changes to analyst1
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}

    user opens details dropdown    ${RELEASE_NAME}    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} (not Live)    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="Edit release"]
    user clicks link    Edit release

Navigate to content section as analyst1
    user clicks link    Content
    user waits until h1 is visible    ${PUBLICATION_NAME}

Add first content section
    user closes Set Page View box
    user scrolls down    400
    user scrolls to element    xpath://button[text()="Add new section"]
    user waits until button is enabled    Add new section
    user clicks button    Add new section
    user changes accordion section title    1    ${CONTENT_BLOCK_TITLE_1}    css:#releaseMainContent

Add first text block
    user adds text block to editable accordion section    ${CONTENT_BLOCK_TITLE_1}    css:#releaseMainContent
    user adds content to autosaving accordion section text block    ${CONTENT_BLOCK_TITLE_1}    1
    ...    ${CONTENT_BLOCK_BODY_1}    css:#releaseMainContent

Switch to bau1 to add review comments
    user changes to bau1
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user opens details dropdown    ${RELEASE_NAME}    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} (not Live)    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="Edit release"]
    user clicks link    Edit release

Navigate to content section as bau1
    user clicks link    Content
    user closes Set Page View box

Add review comment for first content block
    user clicks button    First content section
    user clicks button    Edit block
    # select the text to enable the add comment button
    user presses keys    CTRL+a
    user clicks element    xpath://*[@aria-label="Editor toolbar"]//button[9]    # CKEditor comment button
    user waits until page contains element
    ...    testid:comment-textarea    5
    user presses keys    This section needs fixing    testid:comment-textarea
    # ensure 'Set page view' box doesn't intercept button click
    user scrolls down    100
    user clicks button    Add comment
    user clicks button    Save & close

Switch to analyst1 and navigate to release
    user changes to analyst1
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user opens details dropdown    ${RELEASE_NAME}    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} (not Live)    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="Edit release"]
    user clicks link    Edit release

Navigate to content section to resolve comments
    user clicks link    Content
    user waits until h1 is visible    ${PUBLICATION_NAME}

Resolve comments
    user closes Set Page View box
    user scrolls to element    testid:accordionSection
    user clicks button    First content section
    user clicks element    testid:view-comments

    # avoid set page view box getting in the way
    user scrolls down    600

    # resolve the comment left by bau1
    ${comment}=    user gets comment    This section needs fixing
    user clicks button    Resolve    ${comment}
    user waits until page contains element
    ...    testid:Expand Details Section Resolved comments (1)    10
