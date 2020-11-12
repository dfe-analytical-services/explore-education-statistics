*** Settings ***
Resource  ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${DETAILS_HEADING}  Academic Year 2020/21
${PUBLICATION_NAME}  Publish Release and Amend 2 Publication %{RUN_IDENTIFIER}
${SUBJECT_NAME}  multiple filters test subject
${SECOND_SUBJECT}  Upload file test subject

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
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}multiple_filters.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}multiple_filters.meta.csv
    user clicks button  Upload data files
    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   ${SUBJECT_NAME}
    user opens accordion section   ${SUBJECT_NAME}
    ${section}=  user gets accordion section content element  ${SUBJECT_NAME}
    user checks headed table body row contains  Status           Complete  ${section}  180

Upload another subject (for deletion later)
    [Tags]  HappyPath
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user enters text into element  id:dataFileUploadForm-subjectTitle   ${SECOND_SUBJECT}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}upload-file-test.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}upload-file-test.meta.csv
    user clicks button  Upload data files
    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   ${SECOND_SUBJECT}
    user opens accordion section   ${SECOND_SUBJECT}
    ${section}=  user gets accordion section content element  ${SECOND_SUBJECT}
    user checks headed table body row contains  Status           Complete  ${section}  180

Add meta guidance to filter subject
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document
    user waits until page contains accordion section  ${SUBJECT_NAME}
    user waits until element is visible  css:#metaGuidance-dataFiles
    user enters text into meta guidance data file content editor  ${SUBJECT_NAME}
    ...  ${SUBJECT_NAME} meta guidance content

Add meta guidance to Second Subject
    [Tags]  HappyPath
    user clicks element  css:body
    user waits until page contains accordion section  ${SECOND_SUBJECT}  15
    user enters text into meta guidance data file content editor  ${SECOND_SUBJECT}
    ...  ${SECOND_SUBJECT} meta guidance content
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

Select "Test Topic" publication
    [Tags]  HappyPath
    environment variable should be set  TEST_THEME_NAME
    environment variable should be set  TEST_TOPIC_NAME
    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user clicks radio      ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until h2 is visible  Choose a subject
    user checks previous table tool step contains  1   Publication   ${PUBLICATION_NAME}

Select "filter subject" subject
    [Tags]  HappyPath
    user clicks radio   ${SUBJECT_NAME}
    user clicks element   id:publicationSubjectForm-submit
    user waits until h2 is visible  Choose locations
    user checks previous table tool step contains  2    Subject     ${SUBJECT_NAME}

Select Local Authority
    [Tags]  HappyPath
    user opens details dropdown     Local Authority
    user clicks select all for category  Local Authority

Select National
    [Tags]  HappyPath
    user opens details dropdown  National
    user clicks checkbox  England

Select Regional
    [Tags]  HappyPath
    user opens details dropdown  Regional
    user clicks select all for category  Regional

Click next step button
    user clicks element     id:locationFiltersForm-submit
    user waits until h2 is visible  Choose time period

Select Start date and End date
    [Tags]  HappyPath
    user selects from list by label  id:timePeriodForm-start   2017/18
    user selects from list by label  id:timePeriodForm-end     2017/18
    user clicks element     id:timePeriodForm-submit
    user waits until h2 is visible  Choose your filters
    user waits until page contains element   id:filtersForm-indicators
    user checks previous table tool step contains  4    Start date    2017/18
    user checks previous table tool step contains  4    End date      2017/18

Select Indicators
    [Tags]  HappyPath
    user clicks indicator checkbox   Authorised absence rate
    user checks indicator checkbox is checked  Authorised absence rate
    user clicks indicator checkbox   Number of persistent absentees
    user checks indicator checkbox is checked  Number of persistent absentees
    user clicks indicator checkbox   Number of pupil enrolments
    user checks indicator checkbox is checked  Number of pupil enrolments
    user clicks indicator checkbox   Number of schools
    user checks indicator checkbox is checked  Number of schools
    user clicks indicator checkbox   Overall absence rate
    user checks indicator checkbox is checked  Overall absence rate
    user clicks indicator checkbox   Percentage of persistent absentees
    user checks indicator checkbox is checked  Percentage of persistent absentees
    user clicks indicator checkbox   Unauthorised absence rate
    user checks indicator checkbox is checked  Unauthorised absence rate

Select gender filter
    [Tags]   HappyPath
    user opens details dropdown  Gender
    user clicks select all for category  Gender

Select school type filter
    [Tags]  HappyPath
    user opens details dropdown  School type
    user clicks select all for category  School type

Create table
    [Tags]  HappyPath
    user clicks element     id:filtersForm-submit
    user waits until results table appears     60

Validate results table column headings
    [Tags]  HappyPath
    user checks table column heading contains  1   1  Female

Generate the permalink
    [Tags]  HappyPath
    [Documentation]  EES-214
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
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown   ${DETAILS_HEADING} (Live - Latest release)  ${accordion}
    ${details}=  user gets details content element  ${DETAILS_HEADING} (Live - Latest release)  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]
    user clicks button  Amend this release  ${details}
    user clicks button  Confirm

Replace data files for amendment
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   ${SUBJECT_NAME}
    user opens accordion section   ${SUBJECT_NAME}
    ${section}=  user gets accordion section content element  ${SUBJECT_NAME}
    user clicks link  Replace data  ${section}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}dates.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}dates.meta.csv
    user clicks button  Upload data files
    user checks headed table body row cell contains  Status          2  Complete   wait=180

Confirm data replacement
    [Tags]  HappyPath
    user waits until page contains  Data blocks: OK
    user waits until page contains  Footnotes: OK
    user clicks button  Confirm data replacement
    user waits until h2 is visible  Data replacement complete

Delete second subject file
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until page contains accordion section  ${SECOND_SUBJECT}
    user opens accordion section  ${SECOND_SUBJECT}
    user scrolls to accordion section content  ${SECOND_SUBJECT}
    ${accordion}=  user gets accordion section content element  ${SECOND_SUBJECT}
    ${button}=  user gets button element  Delete files  ${accordion}
    user clicks element  ${button}
    user clicks button  Confirm

Go to "Sign off" page and approve amendment
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

Check amended release doesn't contain deleted subject
    [Tags]  HappyPath
    user goes to url  %{PUBLIC_URL}/data-tables
    user waits until h1 is visible  Create your own tables online
    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user clicks radio      ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until h2 is visible  Choose a subject
    user checks previous table tool step contains  1   Publication   ${PUBLICATION_NAME}
    user checks page does not contain  ${SECOND_SUBJECT}