*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${RELEASE_NAME}=                Academic Year Q1 2020/21
${PUBLICATION_NAME}=            UI tests - comments %{RUN_IDENTIFIER}
${SUBJECT_NAME}=                Seven filters
${CONTENT_BLOCK_TITLE_1}=       First content section
${CONTENT_BLOCK_BODY_1}=        Adding some content that is incorrect

*** Test Cases ***
Create publication
    [Tags]    HappyPath
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}
    ...    UI test topic %{RUN_IDENTIFIER}
    user clicks link    Create new publication
    user waits until h1 is visible    Create new publication
    user creates publication    ${PUBLICATION_NAME}

Create new methodology
    [Tags]    HappyPath
    user creates methodology for publication    ${PUBLICATION_NAME}

Add methodology content
    [Tags]    HappyPath
    user clicks link    Manage content
    user creates new content section    1    Methodology content section 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    1
    ...    Adding Methodology content    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

Approve methodology
    [Tags]    HappyPath
    user approves methodology for publication    ${PUBLICATION_NAME}

Create new release
    [Tags]    HappyPath
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user clicks link    Create new release
    user creates release for publication    ${PUBLICATION_NAME}    Academic Year Q1    2020
    user clicks link    Data and files
    user uploads subject    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Add meta guidance to subject
    [Tags]    HappyPath
    user clicks link    Metadata guidance
    user waits until h2 is visible    Public metadata guidance document    90
    user enters text into element    id:metaGuidanceForm-content    Test meta guidance content

    user waits until page contains accordion section    ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor    ${SUBJECT_NAME}
    ...    meta guidance content
    user clicks button    Save guidance

Navigate to 'Footnotes' page
    [Tags]    HappyPath
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add footnote to second Subject
    [Tags]    HappyPath
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user enters text into element    label:Footnote    Footnote 1 ${SUBJECT_NAME}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Give analyst1 publication owner permissions to work on release
    [Tags]    HappyPath
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Switch to analyst1 to work on release content blocks
    [Tags]    HappyPath
    user changes to analyst1
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}

    user opens details dropdown    ${RELEASE_NAME}    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} (not Live)    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="Edit this release"]
    user clicks link    Edit this release

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

add first text block
    user adds text block to editable accordion section    ${CONTENT_BLOCK_TITLE_1}    css:#releaseMainContent
    user adds content to accordion section text block    ${CONTENT_BLOCK_TITLE_1}    1    ${CONTENT_BLOCK_BODY_1}
    ...    css:#releaseMainContent

Switch to bau1 to add review comments
    [Tags]    HappyPath
    user changes to bau1
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user opens details dropdown    ${RELEASE_NAME}    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} (not Live)    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="Edit this release"]
    user clicks link    Edit this release

Navigate to content section as bau1
    [Tags]    HappyPath
    user clicks link    Content
    user closes Set Page View box

Add review comment for first content block
    [Tags]    HappyPath
    user scrolls to element    testid:accordionSection
    user clicks button    First content section

    user scrolls to element    testid:Expand Details Section Add / View comments (0 unresolved)
    user clicks element    testid:Expand Details Section Add / View comments (0 unresolved)

    # avoid set page view box getting in the way
    user scrolls down    400
    user presses keys    This section needs fixing    testid:comment-textarea
    user clicks button    Add comment

    user waits until page contains element
    ...    //*[@data-testid="comment-content" and text()="This section needs fixing"]    5
    user clicks element    testid:Expand Details Section Add / View comments (1 unresolved)

Switch to analyst1 and navigate to release
    [Tags]    HappyPath
    user changes to analyst1
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user opens details dropdown    ${RELEASE_NAME}    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} (not Live)    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="Edit this release"]
    user clicks link    Edit this release

Navigate to content section to resolve comments
    [Tags]    HappyPath
    user clicks link    Content
    user waits until h1 is visible    ${PUBLICATION_NAME}

Resolve comments
    [Tags]    HappyPath
    user closes Set Page View box
    user scrolls to element    testid:accordionSection
    user clicks button    First content section
    user clicks element    testid:Expand Details Section Add / View comments (1 unresolved)

    # avoid set page view box getting in the way
    user scrolls down    600

    # resolve the comment left by bau1
    ${comment}=    user gets comment    This section needs fixing
    user clicks button    Resolve    ${comment}

    user clicks element    testid:comment-textarea
    user presses keys    Fixed the problem    testid:comment-textarea

    # add another comment letting bau user know they've fixed the problem (and pressing resolve on own comment)
    user clicks button    Add comment

    user waits until page contains element
    ...    //*[@data-testid="Expand Details Section Add / View comments (1 unresolved)"]    10

    ${comment_2}=    user gets comment    Fixed the problem
    user clicks button    Resolve    ${comment_2}

Move publication into 'Higher Review'
    [Tags]    HappyPath
    user clicks link    Sign off
    user clicks button    Edit release status
    user clicks element    id:releaseStatusForm-approvalStatus-HigherLevelReview

Switch to bau1 to approve release for immediate publication
    [Tags]    HappyPath
    user changes to bau1
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user opens details dropdown    ${RELEASE_NAME}    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} (not Live)    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="Edit this release"]
    user clicks link    Edit this release
    user approves original release for immediate publication
