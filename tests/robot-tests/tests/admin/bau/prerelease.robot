*** Settings ***
Resource    ../../libs/admin-common.robot
Resource    ../../libs/tables.robot
Library     ../../libs/admin_api.py
Library     Collections

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${TOPIC_NAME}        %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}  UI tests - prerelease %{RUN_IDENTIFIER}
${RELEASE_URL}

*** Keywords ***
user creates table
    user waits until page contains   UI test subject
    user clicks radio    UI test subject
    user clicks element   id:publicationSubjectForm-submit
    user waits until h2 is visible  Choose locations    90
    user checks previous table tool step contains  1    Subject     UI test subject

    user opens details dropdown   Ward
    user clicks checkbox   Nailsea Youngwood
    user clicks checkbox   Syon
    user clicks element     id:locationFiltersForm-submit
    user waits until h2 is visible  Choose time period  90

    ${timePeriodStartList}=   get list items  id:timePeriodForm-start
    ${timePeriodEndList}=   get list items  id:timePeriodForm-end
    ${expectedList}=   create list   Please select  2005  2007  2008  2010  2011  2012  2016  2017
    lists should be equal  ${timePeriodStartList}   ${expectedList}
    lists should be equal  ${timePeriodEndList}   ${expectedList}

    user selects from list by label  id:timePeriodForm-start  2005
    user selects from list by label  id:timePeriodForm-end  2017
    user clicks element     id:timePeriodForm-submit
    user waits until h2 is visible  Choose your filters
    user checks previous table tool step contains  3    Start date    2005
    user checks previous table tool step contains  3    End date      2017

    user clicks indicator checkbox    Admission Numbers

    user clicks element   id:filtersForm-submit
    user waits until results table appears     180
    user waits until element contains  css:[data-testid="dataTableCaption"]
    ...  Table showing Admission Numbers for 'UI test subject' from '${PUBLICATION_NAME}' in Nailsea Youngwood and Syon between 2005 and 2017

user validates table rows
    user checks table column heading contains  1  1  Admission Numbers

    ${row}=  user gets row number with heading  Nailsea Youngwood
    user checks table heading in offset row contains  ${row}  0  2  2005
    user checks table heading in offset row contains  ${row}  1  1  2010
    user checks table heading in offset row contains  ${row}  2  1  2011
    user checks table heading in offset row contains  ${row}  3  1  2012
    user checks table heading in offset row contains  ${row}  4  1  2016

    user checks table cell in offset row contains  ${row}  0  1  3,612
    user checks table cell in offset row contains  ${row}  1  1  9,304
    user checks table cell in offset row contains  ${row}  2  1  9,603
    user checks table cell in offset row contains  ${row}  3  1  8,150
    user checks table cell in offset row contains  ${row}  4  1  4,198

    ${row}=  user gets row number with heading  Syon
    user checks table heading in offset row contains  ${row}  0  2  2007
    user checks table heading in offset row contains  ${row}  1  1  2008
    user checks table heading in offset row contains  ${row}  2  1  2010
    user checks table heading in offset row contains  ${row}  3  1  2012
    user checks table heading in offset row contains  ${row}  4  1  2017

    user checks table cell in offset row contains  ${row}  0  1  9,914
    user checks table cell in offset row contains  ${row}  1  1  5,505
    user checks table cell in offset row contains  ${row}  2  1  6,060
    user checks table cell in offset row contains  ${row}  3  1  1,109
    user checks table cell in offset row contains  ${row}  4  1  1,959

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

Add public prerelease access list
    [Tags]  HappyPath
    user clicks link  Pre-release access
    user waits until h2 is visible  Manage pre-release user access
    user creates public prerelease access list  Initial test public access list

Update public prerelease access list
    [Tags]  HappyPath
    user updates public prerelease access list  Updated test public access list

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
    user goes to url   ${RELEASE_URL}/prerelease/content

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

    user enters text into element  css:input[name="email"]  ees-analyst1@education.gov.uk
    user clicks button  Invite new user
    user checks results table cell contains  2  1  ees-analyst1@education.gov.uk

Validate prerelease has not started for Analyst user
    [Tags]  HappyPath
    user changes to analyst1
    user goes to url   ${RELEASE_URL}/prerelease/content

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
    user goes to url   ${RELEASE_URL}/prerelease/content

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

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Metadata guidance document
    user waits until page contains  Test metadata guidance content

    user waits until page contains accordion section  UI test subject
    user checks there are x accordion sections  1

    user opens accordion section  UI test subject
    user checks summary list contains  Filename             upload-file-test.csv
    user checks summary list contains  Geographic levels
    ...  Local Authority; Local Authority District; Local Enterprise Partnership; Opportunity Area; Parliamentary Constituency; RSC Region; Regional; Ward
    user checks summary list contains  Time period          2005 to 2020
    user checks summary list contains  Content              Test file guidance content

    user opens details dropdown  Variable names and descriptions

    user checks table column heading contains  1  1  Variable name
    user checks table column heading contains  1  2  Variable description

    user checks results table cell contains  1  1   admission_numbers
    user checks results table cell contains  1  2   Admission Numbers

Go back to prerelease content page
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

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Pre-release access list
    user waits until page contains  Updated test public access list

Go back to prerelease content page again
    [Tags]  HappyPath
    user clicks link  Back
    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

Go to prerelease table tool page
    [Tags]  HappyPath
    user clicks link  Table tool

    user waits until h1 is visible  Create your own tables online
    user waits until h2 is visible  Choose a subject

Create and validate table
    [Tags]  HappyPath
    user creates table
    user validates table rows


Validate prerelease has started for Analyst user
    [Tags]  HappyPath
    user changes to analyst1
    user goes to url   ${RELEASE_URL}/prerelease/content

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

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Metadata guidance document
    user waits until page contains  Test metadata guidance content

    user waits until page contains accordion section  UI test subject
    user checks there are x accordion sections  1

    user opens accordion section  UI test subject
    user checks summary list contains  Filename             upload-file-test.csv
    user checks summary list contains  Geographic levels
    ...  Local Authority; Local Authority District; Local Enterprise Partnership; Opportunity Area; Parliamentary Constituency; RSC Region; Regional; Ward
    user checks summary list contains  Time period          2005 to 2020
    user checks summary list contains  Content              Test file guidance content

    user opens details dropdown  Variable names and descriptions

    user checks table column heading contains  1  1  Variable name
    user checks table column heading contains  1  2  Variable description

    user checks results table cell contains  1  1   admission_numbers
    user checks results table cell contains  1  2   Admission Numbers

Go back to prerelease content page as Analyst user
    [Tags]  HappyPath
    user clicks link  Back
    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

Validate public prerelease access list as Analyst user
    [Tags]  HappyPath
    user opens details dropdown  Download associated files
    user clicks link  Pre-release access list

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

    user waits until h2 is visible  Pre-release access list
    user waits until page contains  Updated test public access list

Go back to prerelease content page again as Analyst user
    [Tags]  HappyPath
    user clicks link  Back

    user checks breadcrumb count should be   2
    user checks nth breadcrumb contains   1    Home
    user checks nth breadcrumb contains   2    Pre-release access

    user waits until page contains title caption  Calendar Year 2000
    user waits until h1 is visible  ${PUBLICATION_NAME}

Go to prerelease table tool page as Analyst user
    [Tags]  HappyPath
    user clicks link  Table tool

    user waits until h1 is visible  Create your own tables online
    user waits until h2 is visible  Choose a subject

Create and validate table as Analyst user
    [Tags]  HappyPath
    user creates table
    user validates table rows
