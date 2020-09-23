*** Settings ***
Resource    ../../libs/admin-common.robot

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - prerelease %{RUN_IDENTIFIER}
${RELEASE_URL}

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
    user clicks testid element  Create new release link for ${PUBLICATION_NAME}
    user creates release for publication  ${PUBLICATION_NAME}  Calendar Year  2000

Verify release summary
    [Tags]  HappyPath
    user checks page contains element   xpath://li/a[text()="Release summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible  Release summary
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Add basic release content
    [Tags]  HappyPath
    user clicks link  Manage content
    user waits until h1 is visible  ${PUBLICATION_NAME}
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user adds basic release content  ${PUBLICATION_NAME}

Go to "Release status" tab
    [Tags]  HappyPath
    user clicks link   Release status
    user waits until h2 is visible  Release status
    user waits until page contains button  Edit release status

Approve release and wait for it to be Scheduled
    [Tags]  HappyPath
    ${day}=         get current datetime  %-d  2
    ${month}=       get current datetime  %-m  2
    ${month_word}=  get current datetime  %B  2
    ${year}=        get current datetime  %Y  2

    user clicks button  Edit release status
    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote     Approved by prerelease UI tests
    user waits until page contains element   xpath://label[text()="On a specific date"]/../input
    user clicks radio   On a specific date
    user waits until page contains   Publish date
    user enters text into element  id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element  id:releaseStatusForm-publishScheduled-month  ${month}
    user enters text into element  id:releaseStatusForm-publishScheduled-year   ${year}
    user enters text into element  id:releaseStatusForm-nextReleaseDate-month   1
    user enters text into element  id:releaseStatusForm-nextReleaseDate-year    2001
    user clicks button   Update status

    user waits until h2 is visible  Release status
    user checks summary list contains  Current status  Approved
    user checks summary list contains  Scheduled release  ${day} ${month_word} ${year}
    user checks summary list contains  Next release expected  January 2001
    user waits for release process status to be  Scheduled  90

Navigate to prerelease page
    [Tags]  HappyPath
    ${current_url}=  get location
    ${RELEASE_URL}=  remove substring from right of string  ${current_url}  /status
    set suite variable  ${RELEASE_URL}
    user goes to url   ${RELEASE_URL}/prerelease

Validate prerelease has not started
    [Tags]  HappyPath
    user waits until h1 is visible  Pre-release access is not yet available
    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    ${day}=         get current datetime  %d   1
    ${month}=       get current datetime  %m   1
    ${year}=        get current datetime  %Y   1
    ${time_start}=  format uk to local datetime  ${year}-${month}-${day}T00:00:00  %-d %B %Y at %H:%M
    ${time_end}=    format uk to local datetime  ${year}-${month}-${day}T23:59:00  %-d %B %Y at %H:%M
    user checks page contains   Pre-release access will be available from ${time_start} until ${time_end}.

Go to prerelease access page
    [Tags]  HappyPath
    user goes to url   ${RELEASE_URL}/prerelease-access
    user waits until h2 is visible  Manage pre-release user access

Invite users to prerelease for scheduled release
    [Tags]  HappyPath
    # This is GOV.UK Notify's test email address
    user enters text into element  css:input[name="email"]  simulate-delivered@notifications.service.gov.uk
    user clicks button  Invite new user

    user checks table column heading contains  css:table  1  1  User email

    user checks results table cell contains  1  1  simulate-delivered@notifications.service.gov.uk

    user enters text into element  css:input[name="email"]  analyst1@example.com
    user clicks button  Invite new user
    user checks results table cell contains  2  1  analyst1@example.com

Add public prerelease access list
    [Tags]  HappyPath
    user clicks link  Public access list
    user waits until h2 is visible  Public pre-release access list
    user clicks button   Create public pre-release access list
    user presses keys  CTRL+a+BACKSPACE
    user presses keys  Initial test public access list
    user clicks button  Save access list
    user waits until element contains  css:[data-testid="publicPreReleaseAccessListPreview"]  Initial test public access list

Update public prerelease access list
    [Tags]  HappyPath
    user clicks button   Edit public pre-release access list
    user presses keys  CTRL+a+BACKSPACE
    user presses keys  Updated test public access list
    user clicks button  Save access list
    user waits until element contains  css:[data-testid="publicPreReleaseAccessListPreview"]  Updated test public access list

Validate prerelease has not started for Analyst user
    [Tags]  HappyPath
    user changes to analyst1
    user goes to url   ${RELEASE_URL}/prerelease

    user waits until h1 is visible  Pre-release access is not yet available
    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    ${day}=         get current datetime  %d   1
    ${month}=       get current datetime  %m   1
    ${year}=        get current datetime  %Y   1
    ${time_start}=  format uk to local datetime  ${year}-${month}-${day}T00:00:00  %-d %B %Y at %H:%M
    ${time_end}=    format uk to local datetime  ${year}-${month}-${day}T23:59:00  %-d %B %Y at %H:%M
    user checks page contains   Pre-release access will be available from ${time_start} until ${time_end}.

Start prerelease
    [Tags]  HappyPath
    user changes to bau1
    ${day}=         get current datetime  %-d  1
    ${month}=       get current datetime  %-m  1
    ${month_word}=  get current datetime  %B  1
    ${year}=        get current datetime  %Y  1
    user goes to url  ${RELEASE_URL}/status
    user clicks button  Edit release status
    user enters text into element  id:releaseStatusForm-publishScheduled-day    ${day}
    user enters text into element  id:releaseStatusForm-publishScheduled-month  ${month}
    user enters text into element  id:releaseStatusForm-publishScheduled-year   ${year}
    user clicks button   Update status

    user waits until h2 is visible  Release status
    user checks summary list contains  Current status  Approved
    user checks summary list contains  Scheduled release  ${day} ${month_word} ${year}
    user waits for release process status to be  Scheduled  90

Validate prerelease has started
    [Tags]  HappyPath
    ${current_url}=  get location
    ${RELEASE_URL}=  remove substring from right of string  ${current_url}  /status
    set suite variable  ${RELEASE_URL}
    user goes to url   ${RELEASE_URL}/prerelease

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until element contains  id:releaseSummary  Test summary text for ${PUBLICATION_NAME}
    user waits until element contains  id:releaseHeadlines  Test headlines summary text for ${PUBLICATION_NAME}

Validate public prerelease access list
    [Tags]  HappyPath
    user opens details dropdown  Download associated files
    user clicks link  Pre-release access list

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access list

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Pre-release access list
    user waits until page contains  Updated test public access list

Go back to prerelease page
    [Tags]  HappyPath
    user clicks link  Back

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

Validate prerelease has started for Analyst user
    [Tags]  HappyPath
    user changes to analyst1
    user goes to url   ${RELEASE_URL}/prerelease

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until element contains  id:releaseSummary  Test summary text for ${PUBLICATION_NAME}
    user waits until element contains  id:releaseHeadlines  Test headlines summary text for ${PUBLICATION_NAME}

Validate public prerelease access list for Analyst user
    [Tags]  HappyPath
    user opens details dropdown  Download associated files
    user clicks link  Pre-release access list

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access list

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Pre-release access list
    user waits until page contains  Updated test public access list

Go back to prerelease page for Analyst user
    [Tags]  HappyPath
    user clicks link  Back

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}
