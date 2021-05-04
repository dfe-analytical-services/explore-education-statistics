*** Settings ***
Resource  ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${RELEASE_NAME}  Calendar Year 2000
${PUBLICATION_NAME}  UI tests - public release visibility %{RUN_IDENTIFIER}

*** Test Cases ***
Create test publication and release via API
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api   ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}   CY    2000
    
Verify release summary
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}   ${RELEASE_NAME} (not Live)
    user waits until h2 is visible  Release summary
    user checks page contains element   xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user checks summary list contains  Publication title  ${PUBLICATION_NAME}

Upload subject
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until h2 is visible  Add data file to release
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user enters text into element  id:dataFileUploadForm-subjectTitle   UI test subject
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}upload-file-test.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}upload-file-test.meta.csv
    user waits for page to finish loading
    user clicks button  Upload data files


    user waits until h2 is visible  Uploaded data files
    user waits until element contains  id:dataFileUploadForm-subjectTitle   ${EMPTY}
    user waits until page contains accordion section   UI test subject  60
    user opens accordion section   UI test subject

    ${section}=  user gets accordion section content element  UI test subject
    user checks headed table body row contains  Subject title    UI test subject  ${section}
    user checks headed table body row contains  Data file        upload-file-test.csv  ${section}
    user checks headed table body row contains  Metadata file    upload-file-test.meta.csv  ${section}
    user checks headed table body row contains  Number of rows   159  ${section}
    user checks headed table body row contains  Data file size   15 Kb  ${section}
    user checks headed table body row contains  Status           Complete  ${section}  360

Go to 'Sign Off' page
    [Tags]  HappyPath
    user clicks link  Sign off
    user waits for page to finish loading
    user waits until page contains testid  public-release-url
    ${PUBLIC_RELEASE_LINK}=  Get Text  xpath://*[@data-testid="public-release-url"]
    Set Suite Variable  ${PUBLIC_RELEASE_LINK}

Go to Public Release Link
    [Tags]  HappyPath  NotAgainstLocal
    # To get around basic auth on public frontend
    user goes to url  %{PUBLIC_URL}
    user waits until h1 is visible  Explore our statistics and data
    user goes to url  ${PUBLIC_RELEASE_LINK}
    user waits until page contains  Page not found
    user checks page does not contain  ${RELEASE_NAME}

Return to admin
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}

Select release from admin dashboard
    [Tags]  HappyPath
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown  ${RELEASE_NAME} (not Live)  ${accordion}
    ${details}=  user gets details content element  ${RELEASE_NAME} (not Live)  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="Edit this release"]
    user clicks link  Edit this release

Add public prerelease access list
    [Tags]  HappyPath
    user clicks link  Pre-release access
    user waits until h2 is visible  Manage pre-release user access
    user creates public prerelease access list  Initial test public access list

Update public prerelease access list
    [Tags]  HappyPath
    user updates public prerelease access list  Updated test public access list

Add meta guidance to ${PUBLICATION_NAME} subject
    [Tags]  HappyPath
    user clicks link  Data and files
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document  90
    user enters text into element  id:metaGuidanceForm-content  Test meta guidance content
    user waits until page contains accordion section  UI test subject
    user enters text into meta guidance data file content editor  UI test subject  metaguidance content
    user clicks button  Save guidance

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
    user enters text into element  id:releaseStatusForm-internalReleaseNote     Approved by UI tests
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

Go to Public release page and ensure release isn't visible
    [Tags]  HappyPath
    # To get around basic auth on public frontend
    user goes to url  %{PUBLIC_URL}
    user goes to url  ${PUBLIC_RELEASE_LINK}

Go to Table Tool page
    [Tags]  HappyPath
    user goes to url  %{PUBLIC_URL}/data-tables
    user waits for page to finish loading
    user waits until h1 is visible  Create your own tables online

Check scheduled release isn't visible
    [Tags]  HappyPath
    environment variable should be set  TEST_THEME_NAME
    user checks page does not contain  ${PUBLICATION_NAME}

Go to release URL and check release isn't visible
    [Tags]  HappyPath  NotAgainstLocal
    user goes to url  ${PUBLIC_RELEASE_LINK}
    user waits until page contains  Page not found

Return to admin dashboard
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}

Go to release from admin dashboard
    [Tags]  HappyPath
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown  ${RELEASE_NAME} (not Live)  ${accordion}
    ${details}=  user gets details content element  ${RELEASE_NAME} (not Live)  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="Edit this release"]
    user clicks link  Edit this release

Approve release for immediate publication
    [Tags]  HappyPath
    user clicks link  Sign off
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status
    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests
    user clicks radio   As soon as possible
    user clicks button   Update status

Check release is approved
    [Tags]  HappyPath
    # EES-1007 - Release process status doesn't automatically update
    user waits until h2 is visible  Sign off
    user checks summary list contains  Current status  Approved
    user reloads page  # EES-1448
    user checks page does not contain button  Edit release status

Go to public release URL and check release isn't visible
    [Tags]  HappyPath  NotAgainstLocal
    user goes to url  ${PUBLIC_RELEASE_LINK}
    user waits until page contains  Page not found