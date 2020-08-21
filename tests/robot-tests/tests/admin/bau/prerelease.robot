*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - prerelease %{RUN_IDENTIFIER}

*** Test Cases ***
Create new publication for "UI tests topic" topic
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains link     Create new publication
    user checks page does not contain button   ${PUBLICATION_NAME}
    user clicks link  Create new publication
    user creates publication    ${PUBLICATION_NAME}

Verify new publication
    [Tags]  HappyPath
    user selects theme "Test theme" and topic "${TOPIC_NAME}" from the admin dashboard
    user waits until page contains button  ${PUBLICATION_NAME}

Create new release
    [Tags]  HappyPath
    user opens accordion section  ${PUBLICATION_NAME}
    user clicks element  css:[data-testid="Create new release link for ${PUBLICATION_NAME}"]
    user creates release for publication  ${PUBLICATION_NAME}  Calendar Year  2000

Verify release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until page contains heading 2  Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Go to "Release status" tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until page contains heading 2  Release status
    user waits until page contains button  Edit release status

Approve release
    [Tags]  HappyPath
    ${PUBLISH_DATE_DAY}=         get datetime  %d  2
    ${PUBLISH_DATE_MONTH}=       get datetime  %m  2
    ${PUBLISH_DATE_MONTH_WORD}=  get datetime  %B  2
    ${PUBLISH_DATE_YEAR}=        get datetime  %Y  2
    set suite variable  ${PUBLISH_DATE_DAY}
    set suite variable  ${PUBLISH_DATE_MONTH}
    set suite variable  ${PUBLISH_DATE_MONTH_WORD}
    set suite variable  ${PUBLISH_DATE_YEAR}

    user clicks button  Edit release status
    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote     Approved by prerelease UI tests
    user waits until page contains element   xpath://label[text()="On a specific date"]/../input
    user clicks radio   On a specific date
    user waits until page contains   Publish date
    user enters text into element  id:releaseStatusForm-publishScheduled-day    ${PUBLISH_DATE_DAY}
    user enters text into element  id:releaseStatusForm-publishScheduled-month  ${PUBLISH_DATE_MONTH}
    user enters text into element  id:releaseStatusForm-publishScheduled-year   ${PUBLISH_DATE_YEAR}
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   1
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    2001
    user clicks button   Update status

Wait for release process status to be Scheduled
    [Tags]  HappyPath
    user waits until page contains heading 2  Release status
    user checks summary list contains  Current status  Approved
    user checks summary list contains  Scheduled release  ${PUBLISH_DATE_DAY} ${PUBLISH_DATE_MONTH_WORD} ${PUBLISH_DATE_YEAR}
    user checks summary list contains  Next release expected  January 2001
    user waits for release process status to be  Scheduled  90

Navigate to Prerelease page
    [Tags]  HappyPath
    ${current_url}=  get location
    ${release_url}=  remove substring from right of string  ${current_url}  /status
    ${prerelease_url}=   evaluate  "${release_url}" + "/prerelease"
    user goes to url   ${prerelease_url}

Validate Prerelease page is displayed
    [Tags]  HappyPath
    user waits until page contains heading 1  Pre Release access is not yet available
    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre Release access

Validate Prerelease page displays correct message
    [Tags]  HappyPath   NotAgainstLocal
    ${PREREL_DATE_DAY}=         get datetime  %d  1
    ${PREREL_DATE_MONTH_WORD}=  get datetime  %B  1
    ${PREREL_DATE_YEAR}=        get datetime  %Y  1
    user checks page contains   Pre Release access will be available from ${PREREL_DATE_DAY} ${PREREL_DATE_MONTH_WORD} ${PREREL_DATE_YEAR} at 09:30 until ${PREREL_DATE_DAY} ${PREREL_DATE_MONTH_WORD} ${PREREL_DATE_YEAR} at 23:59.

Navigate to Admin dashboard
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user waits until page contains heading 1  Dashboard

Go to View scheduled releases tab
    [Tags]  HappyPath
    user waits until page contains element  id:scheduled-releases-tab    60
    user clicks element   id:scheduled-releases-tab
    user waits until page contains element   xpath://*[@id="scheduled-releases-tab" and @aria-selected="true"]

Invite a new user to prerelease for scheduled release
    [Tags]  HappyPath
    set test variable  ${details_selector}  xpath://*[@data-testid="releaseByStatusTab ${PUBLICATION_NAME}"]/details/summary//*[text()="Calendar Year 2000 (not Live)"]
    user waits until page contains element  ${details_selector}
    user waits until element is visible  ${details_selector}
    user clicks element   ${details_selector}

    ${details_dropdown}=   get webelement   xpath://*[@data-testid="releaseByStatusTab ${PUBLICATION_NAME}"]/details
    user waits until parent contains element   ${details_dropdown}   xpath:.//*[text()="Manage pre release access"]
    ${invite_input}=  get child element   ${details_dropdown}   xpath:.//label[text()="Invite a new user"]/../input

    user waits until element is visible   ${invite_input}
    user clicks element  ${invite_input}
    user presses keys  mark@hiveit.co.uk

    ${invite_button}=  get child element  ${details_dropdown}   xpath:.//button[text()="Invite new user"]
    user waits until element is visible  ${invite_button}
    user clicks element   ${invite_button}

    user waits until parent contains element  ${details_dropdown}  xpath:.//dt[.="Pre release access"]/../dd[.="mark@hiveit.co.uk (invited)"]
