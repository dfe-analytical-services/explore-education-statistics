*** Settings ***
Resource  ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${DETAILS_HEADING}  Academic Year 2020/21
${PUBLICATION_NAME}  Permalink Test Publication %{RUN_IDENTIFIER}
${TOPIC_NAME}  %{TEST_TOPIC_NAME}
${SUBJECT_NAME}  Dates test subject

*** Test Cases ***
Create publication & release
    [Tags]  HappyPath
    ${PUBLICATION_ID}=  user creates test publication via api  ${PUBLICATION_NAME}
    user create test release via api  ${PUBLICATION_ID}  AY  2020

Upload a subject
    [Tags]  HappyPath
    user navigates to release summary from admin dashboard  ${PUBLICATION_NAME}  ${DETAILS_HEADING} (not Live)
    user clicks link  Data and files
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user enters text into element  id:dataFileUploadForm-subjectTitle   ${SUBJECT_NAME}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}dates.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}dates.meta.csv
    user clicks button  Upload data files
    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   ${SUBJECT_NAME}
    user opens accordion section   ${SUBJECT_NAME}
    ${section}=  user gets accordion section content element  ${SUBJECT_NAME}
    user checks headed table body row contains  Status           Complete  ${section}  180

Add meta guidance to subject
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document

    user waits until page contains accordion section  ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor  ${SUBJECT_NAME}
    ...  ${SUBJECT_NAME} meta guidance content
    user clicks button  Save guidance

Go to "Sign off" page
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until h2 is visible  Sign off
    user waits until page contains button  Edit release status

Approve release
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status
    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests
    user clicks radio   As soon as possible
    user clicks button   Update status

Wait for release process status to be Complete
    [Tags]  HappyPath
    user waits for release process status to be  Complete    ${release_complete_wait}
    user reloads page  # EES-1448
    user checks page does not contain button  Edit release status

Go to Table Tool page
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}/data-tables
    user waits until h1 is visible  Create your own tables online

Select "Pupil Exclusions" publication
    [Tags]  HappyPath
    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    ${TOPIC_NAME}
    user clicks radio      ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until h2 is visible  Choose a subject
    user checks previous table tool step contains  1   Publication   ${PUBLICATION_NAME}

Select subject "Duration of fixed exclusions"
    [Tags]  HappyPath
    user clicks radio   ${SUBJECT_NAME}
    user clicks element   id:publicationSubjectForm-submit
    user waits until h2 is visible  Choose locations
    user checks previous table tool step contains  2    Subject     ${SUBJECT_NAME}

Select Location Country, England
    [Tags]  HappyPath
    user opens details dropdown     National
    user clicks checkbox    England
    user clicks element     id:locationFiltersForm-submit
    user waits until h2 is visible  Choose time period
    user checks previous table tool step contains  3   National     England

Select Start date and End date
    [Tags]  HappyPath
    user selects from list by label  id:timePeriodForm-start   2020 Week 14
    user selects from list by label  id:timePeriodForm-end     2020 Week 14
    user clicks element     id:timePeriodForm-submit
    user waits until h2 is visible  Choose your filters
    user waits until page contains element   id:filtersForm-indicators
    user checks previous table tool step contains  4    Start date    2020 Week 14
    user checks previous table tool step contains  4    End date      2020 Week 14

Select Indicators
    [Tags]  HappyPath
    user clicks indicator checkbox   Number of open settings
    user checks indicator checkbox is checked  Number of open settings

Select Filters
    [Tags]   HappyPath
    user opens details dropdown   Date
    user clicks category checkbox   Date   30/03/2020
    user checks category checkbox is checked   Date   30/03/2020

Create table
    [Tags]  HappyPath
    user clicks element     id:filtersForm-submit
    user waits until results table appears     60

Validate results table column headings
    [Tags]  HappyPath
    user checks table column heading contains  1   1   2020 Week 14

Generate the peramlink
    # EES-214
    [Tags]  HappyPath
    user clicks button  Generate permanent link
    user waits until page contains testid  permalink-generated-url
    ${PERMA_LOCATION_URL}=  Get Text  xpath://*[@data-testid="permalink-generated-url"]
    Set Suite Variable  ${PERMA_LOCATION_URL}

Go to permalink
    [Tags]  HappyPath
    user goes to url  ${PERMA_LOCATION_URL}
    user waits until h1 is visible  '${SUBJECT_NAME}' from '${PUBLICATION_NAME}'
    user checks page does not contain   WARNING - The data used in this permalink may be out-of-date.

Return to Admin to start creating an amendment
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user waits until h1 is visible   Dashboard
    user waits until page contains title caption  Welcome Bau1

Create amendment
    [Tags]  HappyPath
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  ${TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown   ${DETAILS_HEADING} (Live - Latest release)  ${accordion}
    ${details}=  user gets details content element  ${DETAILS_HEADING} (Live - Latest release)  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]
    user clicks button  Amend this release  ${details}
    user clicks button  Confirm

replace data files for amendment
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   Dates test subject
    user opens accordion section   Dates test subject
    ${section}=  user gets accordion section content element  Dates test subject
    user clicks link  Replace data  ${section}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}upload-file-test.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}upload-file-test.meta.csv
    user clicks button  Upload data files
    user checks headed table body row cell contains  Status          2  Complete   wait=180

Confirm data replacement
    [Tags]  HappyPath
    user waits until page contains  Data blocks: OK
    user waits until page contains  Footnotes: OK
    user clicks button  Confirm data replacement
    user waits until h2 is visible  Data replacement complete

Go to "Sign off" page for amendment
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until h2 is visible  Sign off
    user waits until page contains button  Edit release status

Approve release for amendment
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status
    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests
    user clicks radio   As soon as possible
    user clicks button   Update status

Wait for release amendment process status to be Complete
    [Tags]  HappyPath
    user waits for release process status to be  Complete    ${release_complete_wait}
    user reloads page  # EES-1448
    user checks page does not contain button  Edit release status

Go to permalink page & check for error element to be present
    [Tags]  HappyPath
    user goes to url  ${PERMA_LOCATION_URL}
    user waits until page contains  WARNING - The data used in this permalink may be out-of-date.

# EES-1303
Return to Admin to start deleting subject
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user waits until h1 is visible  Dashboard
    user waits until page contains title caption  Welcome Bau1

go to admin dashboard
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}

Create amendment with empty files
    [Tags]  HappyPath
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  ${TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown   ${DETAILS_HEADING} (Live - Latest release)  ${accordion}
    ${details}=  user gets details content element  ${DETAILS_HEADING} (Live - Latest release)  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]
    user clicks button  Amend this release  ${details}
    user clicks button  Confirm

Delete subject files
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until h2 is visible  Add data file to release
    user scrolls to accordion section content  Dates test subject
    user opens accordion section  Dates test subject
    user clicks button  Delete files
    user clicks button  Confirm
