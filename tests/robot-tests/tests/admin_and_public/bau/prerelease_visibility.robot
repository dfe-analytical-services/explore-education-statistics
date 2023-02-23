*** Settings ***
Resource            ../../libs/admin-common.robot
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
    user verifies release summary    ${PUBLICATION_NAME}    ${PUBLICATION_NAME} summary    Calendar year    2000
    ...    UI test contact name    National statistics

Upload subject
    user uploads subject    UI test subject    upload-file-test.csv    upload-file-test.meta.csv

Go to 'Sign Off' page
    user clicks link    Sign off
    user waits for page to finish loading
    user waits until page contains element    testid:public-release-url
    ${PUBLIC_RELEASE_LINK}=    Get Value    xpath://*[@data-testid="public-release-url"]
    check that variable is not empty    PUBLIC_RELEASE_LINK    ${PUBLIC_RELEASE_LINK}
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Go to Public Release Link
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user waits until page contains    Page not found
    user checks page does not contain    ${RELEASE_NAME}

Return to admin
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
    user waits until page does not contain    ${PUBLICATION_NAME}
    user waits until page contains    Page not found

Go to admin release summary
    user navigates to admin dashboard    Bau1
    user navigates to scheduled release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Approve release for immediate publication but don't wait to finish
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user waits until page contains button    Edit release status
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-internalReleaseNote    Approved by UI tests
    user clicks radio    Immediately
    user clicks button    Update status
    user waits until h2 is visible    Sign off
    user checks summary list contains    Current status    Approved

Go to public release URL again and check release isn't visible
    user navigates to public frontend    ${PUBLIC_RELEASE_LINK}
    user waits until page contains    Page not found
