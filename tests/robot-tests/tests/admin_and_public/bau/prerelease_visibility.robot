*** Settings ***
Resource            ../../libs/admin-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${RELEASE_NAME}=        Calendar Year 2000
${PUBLICATION_NAME}=    UI tests - public release visibility %{RUN_IDENTIFIER}

*** Test Cases ***
Create test publication and release via API
    [Tags]    HappyPath
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    CY    2000

Verify release summary
    [Tags]    HappyPath
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME} (not Live)
    user verifies release summary    ${PUBLICATION_NAME}    Calendar Year    2000    UI test contact name
    ...    National Statistics

Upload subject
    [Tags]    HappyPath
    user clicks link    Data and files
    user uploads subject    UI test subject    upload-file-test.csv    upload-file-test.meta.csv

Go to 'Sign Off' page
    [Tags]    HappyPath
    user clicks link    Sign off
    user waits for page to finish loading
    user waits until page contains element    testid:public-release-url
    ${PUBLIC_RELEASE_LINK}=    Get Value    xpath://*[@data-testid="public-release-url"]
    check that variable is not empty    PUBLIC_RELEASE_LINK    ${PUBLIC_RELEASE_LINK}
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Go to Public Release Link
    [Tags]    HappyPath
    # To get around basic auth on public frontend
    user goes to url    %{PUBLIC_URL}
    user waits until h1 is visible    Explore our statistics and data
    user goes to url    ${PUBLIC_RELEASE_LINK}
    user waits until page contains    Page not found
    user checks page does not contain    ${RELEASE_NAME}

Return to admin
    [Tags]    HappyPath
    user navigates to admin dashboard    Bau1

Select release from admin dashboard
    [Tags]    HappyPath
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    ${accordion}=    user gets accordion section content element    ${PUBLICATION_NAME}
    user opens details dropdown    ${RELEASE_NAME} (not Live)    ${accordion}
    ${details}=    user gets details content element    ${RELEASE_NAME} (not Live)    ${accordion}
    user waits until parent contains element    ${details}    xpath:.//a[text()="Edit this release"]
    user clicks link    Edit this release

Add public prerelease access list
    [Tags]    HappyPath
    user clicks link    Pre-release access
    user creates public prerelease access list    Initial test public access list

Update public prerelease access list
    [Tags]    HappyPath
    user updates public prerelease access list    Updated test public access list

Add meta guidance to subject
    [Tags]    HappyPath
    user clicks link    Data and files
    user clicks link    Metadata guidance
    user waits until h2 is visible    Public metadata guidance document
    user waits until page contains element    id:metaGuidanceForm-content
    user waits until page contains element    id:metaGuidance-dataFiles
    user enters text into element    id:metaGuidanceForm-content    Test meta guidance content
    user waits until page contains accordion section    UI test subject
    user enters text into meta guidance data file content editor    UI test subject    metaguidance content
    user clicks button    Save guidance

Go to "Sign off" page
    [Tags]    HappyPath
    user clicks link    Sign off
    user waits until h2 is visible    Sign off    90
    user waits until page contains button    Edit release status    60

Approve release and wait for it to be Scheduled
    [Tags]    HappyPath    NotAgainstDev
    ${day}=    get current datetime    %-d    2
    ${month}=    get current datetime    %-m    2
    ${month_word}=    get current datetime    %B    2
    ${year}=    get current datetime    %Y    2

    user clicks button    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote    Approved by UI tests

    user waits until page contains element    xpath://label[text()="On a specific date"]/../input    60
    user clicks radio    On a specific date

    user waits until page contains    Publish date
    user enters text into element    id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element    id:releaseStatusForm-publishScheduled-month    ${month}
    user enters text into element    id:releaseStatusForm-publishScheduled-year    ${year}
    user enters text into element    id:releaseStatusForm-nextReleaseDate-month    1
    user enters text into element    id:releaseStatusForm-nextReleaseDate-year    2001
    user clicks button    Update status
    user waits until h1 is visible    Confirm publish date
    user clicks button    Confirm
    # the below fails on dev
    user checks summary list contains    Scheduled release    ${day} ${month_word} ${year}
    user checks summary list contains    Next release expected    January 2001
    user checks summary list contains    Current status    Approved

    user waits for release process status to be    Scheduled    60

Check scheduled release isn't visible on public Table Tool
    [Tags]    HappyPath
    user navigates to data tables page on public frontend
    user checks page does not contain    ${PUBLICATION_NAME}

Go to public release URL and check release isn't visible
    [Tags]    HappyPath
    user goes to url    ${PUBLIC_RELEASE_LINK}
    user waits until page does not contain    ${PUBLICATION_NAME}

Check "Page not found" appears
    [Tags]    HappyPath
    user waits until page contains    Page not found

Go to admin release summary
    [Tags]    HappyPath
    user navigates to admin dashboard    Bau1
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME} (not Live)

Approve release for immediate publication but don't wait to finish
    [Tags]    HappyPath
    user clicks link    Sign off
    user waits until h2 is visible    Sign off
    user waits until page contains button    Edit release status
    user clicks button    Edit release status
    user waits until h2 is visible    Edit release status
    user clicks radio    Approved for publication
    user enters text into element    id:releaseStatusForm-latestInternalReleaseNote    Approved by UI tests
    user clicks radio    Immediately
    user clicks button    Update status
    user waits until h2 is visible    Sign off
    user checks summary list contains    Current status    Approved

Go to public release URL and check release isn't visible
    [Tags]    HappyPath
    user goes to url    ${PUBLIC_RELEASE_LINK}
    user waits until page contains    Page not found
