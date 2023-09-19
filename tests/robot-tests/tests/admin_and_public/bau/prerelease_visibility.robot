*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${RELEASE_NAME}=        Calendar year 2000
${PUBLICATION_NAME}=    UI tests - public release visibility %{RUN_IDENTIFIER}


*** Test Cases ***
Create test publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    CY    2000

Verify release summary
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}
    user verifies release summary    Calendar year    2000
    ...    National statistics

Upload subject
    user uploads subject    UI test subject    upload-file-test.csv    upload-file-test.meta.csv

Check release isn't publically visible
    user clicks link    Sign off
    user waits until page finishes loading
    user waits until page contains element    testid:public-release-url
    ${PUBLIC_RELEASE_LINK}=    Get Value    testid:public-release-url
    check that variable is not empty    PUBLIC_RELEASE_LINK    ${PUBLIC_RELEASE_LINK}
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user waits until page contains    Page not found
    user checks page does not contain    ${RELEASE_NAME}

Return to admin
    user navigates to admin dashboard    Bau1

Create methodology for release
    user creates methodology for publication    ${PUBLICATION_NAME}

    user clicks link    Manage content
    user creates new content section    1    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds text block to editable accordion section    Methodology content section 1
    ...    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}
    user adds content to accordion section text block    Methodology content section 1    1
    ...    Content 1    ${METHODOLOGY_CONTENT_EDITABLE_ACCORDION}

Set methodology to be published alongside release
    approve methodology from methodology view    WithRelease    ${PUBLICATION_NAME} - ${RELEASE_NAME}

Check methodology isn't publically visible
    user waits until page contains element    testid:public-methodology-url
    ${PUBLIC_METHODOLOGY_LINK}=    Get Value    testid:public-methodology-url
    check that variable is not empty    PUBLIC_METHODOLOGY_LINK    ${PUBLIC_METHODOLOGY_LINK}
    Set Suite Variable    ${PUBLIC_METHODOLOGY_LINK}

    user navigates to public frontend    ${PUBLIC_METHODOLOGY_LINK}
    user waits until page contains    Page not found

Return to admin again
    user navigates to admin dashboard    Bau1

Add public prerelease access list
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

    user clicks link    Pre-release access
    user creates public prerelease access list    Initial test public access list

Update public prerelease access list
    user updates public prerelease access list    Updated test public access list

Add data guidance to subject
    user clicks link    Data and files
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user waits until page contains element    id:dataGuidanceForm-content
    user waits until page contains element    id:dataGuidance-dataFiles
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    UI test subject
    user enters text into data guidance data file content editor    UI test subject    dataguidance content
    user clicks button    Save guidance

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Go to "Sign off" page
    user clicks link    Sign off
    user waits until h2 is visible    Sign off    %{WAIT_MEDIUM}
    user waits until page contains button    Edit release status    %{WAIT_SMALL}

Approve release and wait for it to be Scheduled
    ${day}=    get current datetime    %-d    2
    ${month}=    get current datetime    %-m    2
    ${month_word}=    get current datetime    %B    2
    ${year}=    get current datetime    %Y    2

    user clicks button    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by UI tests

    user waits until page contains element    xpath://label[text()="On a specific date"]/../input    %{WAIT_SMALL}
    user clicks radio    On a specific date

    user waits until page contains    Publish date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${month}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${year}
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    1
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    2001
    user clicks button    Update status
    user waits until h2 is visible    Confirm publish date
    user clicks button    Confirm

    # the below fails on dev
    user checks summary list contains    Scheduled release    ${day} ${month_word} ${year}
    user checks summary list contains    Next release expected    January 2001
    user checks summary list contains    Current status    Approved

    user waits for release process status to be    Scheduled    %{WAIT_SMALL}

Check scheduled release isn't visible on public Table Tool
    user navigates to data tables page on public frontend
    user checks page does not contain    ${PUBLICATION_NAME}

Go to public release URL and check release isn't visible
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user waits until page contains    Page not found
    user waits until page does not contain    ${PUBLICATION_NAME}

Check methodology isn't accessible via URL
    user navigates to public frontend    ${PUBLIC_METHODOLOGY_LINK}
    user waits until page contains    Page not found

Go to admin release summary
    user navigates to admin dashboard    Bau1
    user navigates to scheduled release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Approve release for immediate publication
    user approves original release for immediate publication

Check release has been published
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user waits until page contains title caption    Calendar year 2000
    user waits until h1 is visible    ${PUBLICATION_NAME}

Check methodology is visible on public Methodologies page
    user navigates to public methodologies page
    user waits until page contains    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until page contains    ${PUBLICATION_NAME}

Check methodology has been published
    user clicks link    ${PUBLICATION_NAME}

    user waits until page contains title caption    Methodology
    user waits until h1 is visible    ${PUBLICATION_NAME}
