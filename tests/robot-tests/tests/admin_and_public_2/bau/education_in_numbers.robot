*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Test Cases ***
Remove UI test page if exists via API
    user removes ein page if exists    ui-test-page

Go to EiN management page
    user clicks link    Platform administration
    user waits until h1 is visible    Platform administration

    user clicks link    Manage Education in Numbers
    user waits until page contains element    testid:education-in-numbers-table

    user checks page does not contain    UI test page

Add new page "UI test page"
    user clicks link    Add new page
    user waits until h1 is visible    Create a new Education in Numbers page

    user enters text into element    css:#educationInNumbersSummaryForm-title    UI test page
    user enters text into element    css:#educationInNumbersSummaryForm-description    UI test page description

    user clicks button    Create page

    user waits until h2 is visible    Page summary

Validate page appears in EiN page table
    user clicks link    Manage Education in Numbers
    user waits until h1 is visible    Education in Numbers pages

    ${ROW}=    user gets table row    UI test page    testid:education-in-numbers-table
    user checks row cell contains text    ${ROW}    2    ui-test-page
    user checks row cell contains text    ${ROW}    3    Draft
    user checks row cell contains text    ${ROW}    4    Not yet published
    user checks row cell contains text    ${ROW}    5    0
    user checks row cell contains text    ${ROW}    6    Edit
    user checks row cell contains text    ${ROW}    6    Delete

    user clicks link    Edit    ${ROW}
    user waits until h2 is visible    Page summary

Validate page summary
    user checks summary list contains    Title    UI test page    testid:summary-list
    user checks summary list contains    Slug    ui-test-page    testid:summary-list
    user checks summary list contains    Description    UI test page description    testid:summary-list
    user checks summary list contains    Status    Draft    testid:summary-list
    user checks summary list contains    Published on    Not yet published    testid:summary-list

Edit page summary
    user clicks link    Edit summary
    user waits until h2 is visible    Edit page summary

    user clears element text    css:#educationInNumbersSummaryForm-description
    user enters text into element    css:#educationInNumbersSummaryForm-description    UI test page description updated

    user clicks button    Update page
    user waits until h2 is visible    Page summary

Validate updated page summary
    user checks summary list contains    Description    UI test page description updated    testid:summary-list

Add a content section
    user clicks link    Manage content
    user waits until h2 is visible    UI test page

    user creates new content section    1    Content section title

Add a text block
    user adds text block to editable accordion section    Content section title    testid:accordion
    user adds content to accordion section text block    Content section title    1
    ...    Some text block content    testid:accordion

Add a group block
    user clicks button    Add group block
    user clicks button    Add group heading

    user enters text into element    //input[@name="heading"]    Group tile heading
    user clicks button    Save group heading

Add free text stat tile
    user clicks button    Add new tile
    user waits until page contains element    testid:freeTextStatTile-editForm

    user enters text into element    //input[@name="title"]    Tile title
    user enters text into element    //input[@name="statistic"]    Over 9000!
    user enters text into element    //input[@name="trend"]    It's up a lot!
    user enters text into element    //input[@name="linkUrl"]    http://test.link
    user enters text into element    //input[@name="linkText"]    A link to somewhere

    user clicks button    Save
    user waits until page contains element
    ...    xpath://*[@data-testid="free-text-stat-tile-title" and text()="Tile title"]

Validate content preview
    user clicks element    id:editingMode-preview
    user waits until page does not contain    Remove this section

    user waits until page contains    Content section title
    user checks page contains    Some text block content

    user checks page contains    Group tile heading
    user checks page contains    Tile title
    user checks page contains    Over 9000!
    user checks page contains    It's up a lot!
    user checks page contains link with text and url    A link to somewhere    http://test.link

Publish page
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    ${test_page_url}=    get value    testid:public-page-url
    set suite variable    ${test_page_url}
    should be equal    %{PUBLIC_URL}/education-in-numbers/ui-test-page    ${test_page_url}

    user checks summary list contains    Title    UI test page    testid:page-list
    user checks summary list contains    Slug    ui-test-page    testid:page-list
    user checks summary list contains    Description    UI test page description updated    testid:page-list
    user checks summary list contains    Published on    Not yet published    testid:page-list

    user clicks button    Publish
    user waits until h2 is visible    Are you sure you want to publish UI test page?

    user clicks button    Confirm

    user checks summary list contains    Status    Published    testid:summary-list

Check page appears on public site
    go to    ${test_page_url}

    user waits until h1 is visible    UI test page

    user checks page contains link with text and url    Education in numbers    /education-in-numbers
    user checks page contains link with text and url    UI test page    /education-in-numbers/ui-test-page

    user checks page contains    Content section title
    user checks page contains    Some text block content

    user checks page contains    Group tile heading
    user checks page contains    Tile title
    user checks page contains    Over 9000!
    user checks page contains    It's up a lot!
    user checks page contains link with text and url    A link to somewhere    http://test.link

Validate Manage Education in Numbers entry
    go to    %{ADMIN_URL}/education-in-numbers

    user waits until h1 is visible    Education in Numbers pages

    ${ROW}=    user gets table row    UI test page    testid:education-in-numbers-table
    user checks row cell contains text    ${ROW}    2    ui-test-page
    user checks row cell contains text    ${ROW}    3    Published
    # 4 is the published date
    user checks row cell contains text    ${ROW}    5    0
    user checks row cell contains text    ${ROW}    6    View
    user checks row cell contains text    ${ROW}    6    Create amendment

    user clicks button    Create amendment    ${ROW}
    user waits until h2 is visible    Page summary

Validate amendment summary
    user checks summary list contains    Title    UI test page    testid:summary-list
    user checks summary list contains    Slug    ui-test-page    testid:summary-list
    user checks summary list contains    Description    UI test page description updated    testid:summary-list
    user checks summary list contains    Status    Draft amendment    testid:summary-list
    user checks summary list contains    Published on    Not yet published    testid:summary-list

Add new content section
    user clicks link    Manage content
    user waits until h2 is visible    UI test page

    user creates new content section    2    Second section

Add a text block to new section
    user adds text block to editable accordion section    Second section    testid:accordion
    user adds content to accordion section text block    Second section    1
    ...    More text block content    testid:accordion

Publish amendment
    user clicks link    Sign off
    user waits until h2 is visible    Sign off

    user clicks button    Publish
    user waits until h2 is visible    Are you sure you want to publish UI test page?

    user clicks button    Confirm

    user checks summary list contains    Status    Published    testid:summary-list

Check amendment on public site
    go to    ${test_page_url}

    user waits until h1 is visible    UI test page

    user checks page contains link with text and url    Education in numbers    /education-in-numbers
    user checks page contains link with text and url    UI test page    /education-in-numbers/ui-test-page

    user checks page contains    Content section title
    user checks page contains    Some text block content

    user checks page contains    Group tile heading
    user checks page contains    Tile title
    user checks page contains    Over 9000!
    user checks page contains    It's up a lot!
    user checks page contains link with text and url    A link to somewhere    http://test.link

    user checks page contains    Second section
    user checks page contains    More text block content
