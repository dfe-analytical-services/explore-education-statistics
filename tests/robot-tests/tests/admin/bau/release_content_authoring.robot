*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes all browsers
Test Setup          fail test fast if required


*** Variables ***
${RELEASE_NAME}=        Academic year Q1 2020/21
${PUBLICATION_NAME}=    UI tests - release content authoring %{RUN_IDENTIFIER}
${SECTION_1_TITLE}=     First content section
${SECTION_2_TITLE}=     Second content section
${BLOCK_1_CONTENT}=     Block 1 content
${BLOCK_2_CONTENT}=     Block 2 content
${DATABLOCK_NAME}=      Dates data block name


*** Test Cases ***
Create publication
    user selects dashboard theme if possible
    user clicks link    Create new publication
    user waits until h1 is visible    Create new publication
    user creates publication    ${PUBLICATION_NAME}

Create new release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year Q1    2020

Upload a subject
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

    user uploads subject and waits until complete    Dates test subject    dates.csv    dates.meta.csv

Create a data block
    user creates data block for dates csv
    ...    Dates test subject
    ...    ${DATABLOCK_NAME}
    ...    Data Block 1 title

Give analyst1 publication owner permissions to work on release
    user gives analyst publication owner access    ${PUBLICATION_NAME}

Switch to analyst1 to work on release content blocks
    user signs in as analyst1
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Navigate to content section as analyst1
    user clicks link    Content
    user waits until h1 is visible    ${PUBLICATION_NAME}    %{WAIT_SMALL}

Add first content section
    user scrolls to element    xpath://button[text()="Add new section"]
    user waits until button is enabled    Add new section
    user clicks button    Add new section
    user changes accordion section title    1    ${SECTION_1_TITLE}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add first text block
    user adds text block to editable accordion section    ${SECTION_1_TITLE}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    ${SECTION_1_TITLE}    1
    ...    ${BLOCK_1_CONTENT}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add second text block
    user adds text block to editable accordion section    ${SECTION_1_TITLE}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user adds content to autosaving accordion section text block    ${SECTION_1_TITLE}    2
    ...    ${BLOCK_2_CONTENT}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}    save=False

Switch to bau1 to view release
    user switches to bau1 browser
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}    %{WAIT_SMALL}

Check second text block is locked as bau1
    ${block}=    get accordion section block    First content section    2    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user checks element contains    ${block}
    ...    Analyst1 User1 (ees-test.analyst1@education.gov.uk) is currently editing this block
    user checks element contains    ${block}    Analyst1 User1 is editing

Switch to analyst1 to save second text block
    user switches to analyst1 browser
    ${block}=    get accordion section block    First content section    2    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user saves autosaving text block    ${block}

Switch to bau1 to add review comments for first text block
    user switches to bau1 browser
    user opens accordion section    ${SECTION_1_TITLE}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${block}=    user starts editing accordion section text block    First content section    1
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user presses keys    CTRL+a
    user adds comment to selected text    ${block}    Test comment 1

    user checks list has x items    testid:comments-unresolved    1    ${block}
    user checks list item contains    testid:comments-unresolved    1    Test comment 1    ${block}

    ${editor}=    get editor    ${block}
    user sets focus to element    ${editor}

    # Selects the rest of the line
    sleep    1    # Prevent intermittent failure where hitting END does not move to end
    user presses keys    END    END    END    RETURN    Block 1 another sentence    SHIFT+HOME
    sleep    0.1    # Prevent intermittent failure where toolbar button is disabled
    user adds comment to selected text    ${block}    Test comment 2

    user checks list has x items    testid:comments-unresolved    2    ${block}
    user checks list item contains    testid:comments-unresolved    1    Test comment 1    ${block}
    user checks list item contains    testid:comments-unresolved    2    Test comment 2    ${block}

    user saves autosaving text block    ${block}

Add review comment for second text block as bau1
    user opens accordion section    ${SECTION_1_TITLE}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${block}=    user starts editing accordion section text block    First content section    2
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user presses keys    CTRL+a
    user adds comment to selected text    ${block}    Test comment 3

    user checks list has x items    testid:comments-unresolved    1    ${block}
    user checks list item contains    testid:comments-unresolved    1    Test comment 3    ${block}

Switch to analyst1 to check second text block is locked
    user switches to analyst1 browser
    ${block}=    get accordion section block    First content section    2    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user waits until element contains    ${block}
    ...    Bau1 User1 (ees-test.bau1@education.gov.uk) is currently editing this block
    user waits until element contains    ${block}    Bau1 User1 is editing

Switch to bau1 to save second text block
    user switches to bau1 browser
    ${block}=    get accordion section block    First content section    2    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user saves autosaving text block    ${block}

Switch to analyst1 to start resolving comments
    user switches to analyst1 browser
    ${block}=    get accordion section block    First content section    1    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user checks element does not contain    ${block}
    ...    Bau1 User1 (ees-test.bau1@education.gov.uk) is currently editing this block
    user checks element does not contain    ${block}    Bau1 User1 is editing

    # Start resolving comments
    user clicks button    View comments    ${block}

    user checks element does not contain    ${block}
    ...    Analyst1 User1 (ees-test.analyst1@education.gov.uk) is currently editing this block
    user checks element does not contain    ${block}    Analyst1 User1 is editing

Switch to bau1 to check first text block is locked
    user switches to bau1 browser
    ${block}=    get accordion section block    First content section    1    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user waits until element contains    ${block}
    ...    Analyst1 User1 (ees-test.analyst1@education.gov.uk) is currently editing this block
    user waits until element contains    ${block}    Analyst1 User1 is editing

Switch to analyst1 to resolve comment for first text block
    user switches to analyst1 browser
    ${block}=    get accordion section block    First content section    1    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user checks list has x items    testid:comments-unresolved    2    ${block}
    user checks list item contains    testid:comments-unresolved    1    Test comment 1    ${block}
    user checks list item contains    testid:comments-unresolved    2    Test comment 2    ${block}

    ${comment}=    user gets unresolved comment    Test comment 1    ${block}
    ${author}=    get child element    ${comment}    testid:comment-author
    user waits until element contains    ${author}    Bau1 User1

    # resolve the comment left by bau1
    user clicks button    Resolve    ${comment}

    user waits until parent contains element
    ...    ${block}
    ...    testid:Expand Details Section Resolved comments (1)    10

    user checks list has x items    testid:comments-unresolved    1    ${block}
    user checks list item contains    testid:comments-unresolved    1    Test comment 2    ${block}

    user opens details dropdown    Resolved comments (1)    ${block}

    user checks list has x items    testid:comments-resolved    1    ${block}
    user checks list item contains    testid:comments-resolved    1    Test comment 1    ${block}

    ${comment}=    user gets resolved comment    Test comment 1    ${block}
    user waits until element contains    ${comment}    Resolved by Analyst1 User1

    user saves autosaving text block    ${block}

Switch to bau1 to check first text block is unlocked
    user switches to bau1 browser
    ${block}=    get accordion section block    First content section    1    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user checks element does not contain    ${block}
    ...    Analyst1 User1 (ees-test.analyst1@education.gov.uk) is currently editing this block
    user checks element does not contain    ${block}    Analyst1 User1 is editing

Switch back to analyst1 to resolve second text block
    user switches to analyst1 browser
    ${block}=    get accordion section block    First content section    2    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user clicks button    View comments    ${block}

    user checks list has x items    testid:comments-unresolved    1    ${block}
    user checks list item contains    testid:comments-unresolved    1    Test comment 3    ${block}

    ${comment}=    user gets unresolved comment    Test comment 3    ${block}
    ${author}=    get child element    ${comment}    testid:comment-author
    user waits until element contains    ${author}    Bau1 User1

    # resolve the comment left by bau1
    user sets focus to element    xpath://button[text()="Resolve"]    ${block}
    user clicks button    Resolve

    user waits until parent contains element
    ...    ${block}
    ...    testid:Expand Details Section Resolved comments (1)    10

    user waits until parent does not contain element    ${block}    testid:comments-unresolved
    user opens details dropdown    Resolved comments (1)    ${block}

    user checks list has x items    testid:comments-resolved    1    ${block}
    user checks list item contains    testid:comments-resolved    1    Test comment 3    ${block}

    ${comment}=    user gets resolved comment    Test comment 3    ${block}
    user waits until element contains    ${comment}    Resolved by Analyst1 User1

    user saves autosaving text block    ${block}

Switch back to bau1 to update unresolved comment
    user switches to bau1 browser
    ${block}=    get accordion section block    First content section    1    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

    user clicks button    View comments    ${block}
    ${comment}=    user gets unresolved comment    Test comment 2    ${block}

    user clicks button    Edit    ${comment}
    user waits until parent contains element    ${comment}    label:Comment
    ${textarea}=    get child element    ${comment}    label:Comment
    user enters text into element    ${textarea}    Test updated comment 2
    user clicks button    Update    ${comment}

Delete unresolved comment as bau1
    ${block}=    get accordion section block    First content section    1    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    ${comment}=    user gets unresolved comment    Test updated comment 2    ${block}
    user clicks button    Delete    ${comment}
    user waits until parent does not contain element    ${block}    testid:comments-unresolved

    user saves autosaving text block    ${block}

Add second content section as analyst1
    user switches to analyst1 browser
    user scrolls to element    xpath://button[text()="Add new section"]
    user waits until button is enabled    Add new section
    user clicks button    Add new section
    user changes accordion section title    2    ${SECTION_2_TITLE}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add data block
    user adds data block to editable accordion section    ${SECTION_2_TITLE}    ${DATABLOCK_NAME}
    ...    ${RELEASE_CONTENT_EDITABLE_ACCORDION}

Add review comment for data block as bau1
    user switches to bau1 browser
    user reloads page
    user opens accordion section    ${SECTION_2_TITLE}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user scrolls down    400
    ${block}=    set variable    xpath://*[@data-testid="data-block-comments-${DATABLOCK_NAME}"]
    user adds comment to data block    ${block}    Test data block comment

    user checks list has x items    testid:comments-unresolved    1    ${block}
    user checks list item contains    testid:comments-unresolved    1    Test data block comment    ${block}

Resolve comment for data block as analyst1
    user switches to analyst1 browser
    user reloads page
    user opens accordion section    ${SECTION_2_TITLE}    ${RELEASE_CONTENT_EDITABLE_ACCORDION}
    user scrolls down    400
    ${block}=    set variable    xpath://*[@data-testid="data-block-comments-${DATABLOCK_NAME}"]
    ${comment}=    user gets unresolved comment    Test data block comment    ${block}

    user clicks button    Resolve    ${comment}

    user waits until parent contains element
    ...    ${block}
    ...    testid:Expand Details Section Resolved comments (1)    10

    user opens details dropdown    Resolved comments (1)    ${block}
    user checks list has x items    testid:comments-resolved    1    ${block}
    user checks list item contains    testid:comments-resolved    1    Test data block comment    ${block}


*** Keywords ***
user adds comment to selected text
    [Arguments]    ${block}    ${text}
    ${toolbar}=    get editor toolbar    ${block}
    ${button}=    user gets button element    Add comment    ${toolbar}
    user checks element does not have class    ${button}    ck-disabled
    user clicks element    ${button}

    ${comments}=    get comments sidebar    ${block}
    user waits until parent contains element    ${comments}    label:Comment
    ${textarea}=    get child element    ${comments}    label:Comment
    user enters text into element    ${textarea}    ${text}
    user clicks button    Add comment    ${comments}

user adds comment to data block
    [Arguments]    ${block}    ${text}
    user clicks button    Add comment    ${block}
    user enters text into element    label:Comment    ${text}
    user clicks button    Add comment    ${block}
