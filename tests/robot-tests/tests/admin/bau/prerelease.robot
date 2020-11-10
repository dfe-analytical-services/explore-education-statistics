*** Settings ***
Resource    ../../libs/admin-common.robot
Resource    ../../libs/tables.robot
Library     ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - prerelease %{RUN_IDENTIFIER}
${RELEASE_URL}

*** Test Cases ***
Create test publication and release via API
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   CY    2000

Verify release summary
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}  Calendar Year 2000 (not Live)
    user waits until h2 is visible  Release summary
    user checks page contains element   xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Upload subject
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until h2 is visible  Add data file to release
    user enters text into element  id:dataFileUploadForm-subjectTitle   UI test subject
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}upload-file-test.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}upload-file-test.meta.csv
    user clicks button  Upload data files

    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   UI test subject
    user opens accordion section   UI test subject

    ${section}=  user gets accordion section content element  UI test subject
    user checks headed table body row contains  Subject title    UI test subject  ${section}
    user checks headed table body row contains  Data file        upload-file-test.csv  ${section}
    user checks headed table body row contains  Metadata file    upload-file-test.meta.csv  ${section}
    user checks headed table body row contains  Number of rows   159  ${section}
    user checks headed table body row contains  Data file size   15 Kb  ${section}
    user checks headed table body row contains  Status           Complete  ${section}  180

Add metadata guidance
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document

    user waits until page contains accordion section  UI test subject
    user opens accordion section  UI test subject
    user checks summary list contains  Filename             upload-file-test.csv
    user checks summary list contains  Geographic levels
    ...  Local Authority; Local Authority District; Local Enterprise Partnership; Opportunity Area; Parliamentary Constituency; RSC Region; Regional; Ward
    user checks summary list contains  Time period          2005 to 2020

    user opens details dropdown  Variable names and descriptions

    user checks table column heading contains  1  1  Variable name
    user checks table column heading contains  1  2  Variable description

    user checks results table cell contains  1  1   admission_numbers   id:metaGuidance-dataFiles
    user checks results table cell contains  1  2   Admission Numbers   id:metaGuidance-dataFiles

    user enters text into element  id:metaGuidanceForm-content  Test metadata guidance content
    user enters text into element  id:metaGuidanceForm-subjects0Content  Test file guidance content

    user clicks button  Save guidance

    user waits until page contains button  Edit guidance

Add basic release content
    [Tags]  HappyPath
    user clicks link  Content
    user waits until h1 is visible  ${PUBLICATION_NAME}
    user waits until h2 is visible  ${PUBLICATION_NAME}
    user adds basic release content  ${PUBLICATION_NAME}

Go to "Sign off" page
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until h2 is visible  Sign off
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

    user waits until h2 is visible  Sign off
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

    user checks table column heading contains  1  1  User email

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

    user waits until h2 is visible  Sign off
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

Validate metadata guidance page
    [Tags]  HappyPath
    user opens details dropdown  Download associated files
    user clicks link  Metadata guidance

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Metadata guidance document

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Metadata guidance document
    user waits until page contains  Test metadata guidance content

    user waits until page contains accordion section  UI test subject
    user checks there are x accordion sections  1

    user opens accordion section  UI test subject
    user checks summary list contains  Filename             upload-file-test.csv
    user checks summary list contains  Geographic levels
    ...  Local Authority; Local Authority District; Local Enterprise Partnership; Opportunity Area; Parliamentary Constituency; Regional; RSC Region; Ward
    user checks summary list contains  Time period          2005 to 2020
    user checks summary list contains  Content              Test file guidance content

    user opens details dropdown  Variable names and descriptions

    user checks table column heading contains  1  1  Variable name
    user checks table column heading contains  1  2  Variable description

    user checks results table cell contains  1  1   admission_numbers
    user checks results table cell contains  1  2   Admission Numbers

Go back to prerelease page
    [Tags]  HappyPath
    user clicks link  Back
    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

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

Go back to prerelease page again
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

Validate public metdata guidance for Analyst user
    [Tags]  HappyPath
    user opens details dropdown  Download associated files
    user clicks link  Metadata guidance

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Metadata guidance document

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Metadata guidance document
    user waits until page contains  Test metadata guidance content

    user waits until page contains accordion section  UI test subject
    user checks there are x accordion sections  1

    user opens accordion section  UI test subject
    user checks summary list contains  Filename             upload-file-test.csv
    user checks summary list contains  Geographic levels
    ...  Local Authority; Local Authority District; Local Enterprise Partnership; Opportunity Area; Parliamentary Constituency; Regional; RSC Region; Ward
    user checks summary list contains  Time period          2005 to 2020
    user checks summary list contains  Content              Test file guidance content

    user opens details dropdown  Variable names and descriptions

    user checks table column heading contains  1  1  Variable name
    user checks table column heading contains  1  2  Variable description

    user checks results table cell contains  1  1   admission_numbers
    user checks results table cell contains  1  2   Admission Numbers

Go back to prerelease page as Analyst user
    [Tags]  HappyPath
    user clicks link  Back
    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

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

Go back to prerelease page again as Analyst user
    [Tags]  HappyPath
    user clicks link  Back

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}
