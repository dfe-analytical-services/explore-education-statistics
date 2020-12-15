*** Settings ***
Resource  ../../libs/admin-common.robot
Library  ../../libs/admin_api.py

Force Tags  Admin  Local  Dev  AltersData

Suite Setup       user signs in as bau1
Suite Teardown    user closes the browser

*** Variables ***
${RELEASE_NAME}  Academic Year Q1 2020/21
${PUBLICATION_NAME}  UI tests - publish release and amend 2 %{RUN_IDENTIFIER}
${SUBJECT_NAME}  Seven filters
${SECOND_SUBJECT}  upload file test
${THIRD_SUBJECT}  upload file test with filter subject
${METHODOLOGY_NAME}  UI tests - publish release and amend 2 %{RUN_IDENTIFIER}
${METHODOLOGY_TITLE}  ${METHODOLOGY_NAME} - Title

*** Test Cases ***
Navigate to manage methodoligies page
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}/methodologies
    user waits until h1 is visible  Manage methodologies

Create new methodology
    [Tags]  HappyPath
    user clicks link  Create new methodology
    user waits until h1 is visible  Create new methodology
    user enters text into element  id:createMethodologyForm-title  ${METHODOLOGY_NAME}
    user clicks button  Create methodology

Add methodology content
    [Tags]  HappyPath
    user waits until h1 is visible  ${METHODOLOGY_NAME}
    user clicks link  Manage content
    user clicks button  Add new section
    user clicks button  New section
    # NOTE: scroll to element is here to avoid selenium clicking the
    # set page view text box on the methodology page
    user scrolls to element  xpath://button[text()="Add text block"]
    user clicks button  Add text block
    user clicks button  Edit block
    user presses keys  Adding Methodology content
    user clicks button  Save
    user clicks link  Go to top
    user clicks button  Edit section title
    user enters text into element  xpath=//*[@name="heading"]  ${METHODOLOGY_NAME}
    user clicks button  Save section title

Approve methodology
    [Tags]  HappyPath
    user clicks link  Sign off
    user clicks button  Edit status
    user clicks radio  Approved for publication
    user enters text into element  xpath=//*[@name="internalReleaseNote"]  Approved by UI tests
    user clicks button  Update status

Check methodology is approved
    [Tags]  HappyPath
    user waits until page contains element  xpath://strong[text()="Approved"]
    user checks page contains element       xpath://strong[text()="Approved"]

Create publication
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user clicks link  Create new publication
    user waits until h1 is visible  Create new publication
    user waits until page contains element  id:publicationForm-title
    user enters text into element  id:publicationForm-title   ${PUBLICATION_NAME}
    user selects from list by label  id:publicationForm-methodologyId  ${METHODOLOGY_NAME} [Approved]
    user enters text into element  id:publicationForm-teamName        Attainment statistics team
    user enters text into element  id:publicationForm-teamEmail       Attainment.STATISTICS@education.gov.uk
    user enters text into element  id:publicationForm-contactName     Tingting Shu
    user enters text into element  id:publicationForm-contactTelNo    0123456789
    user clicks button   Save publication
    user waits until h1 is visible  Dashboard

Create new release
    [Tags]  HappyPath
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    user clicks link  Create new release
    user creates release for publication  ${PUBLICATION_NAME}  Academic Year Q1  2020
    user clicks link  Data and files
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user enters text into element  id:dataFileUploadForm-subjectTitle   ${SUBJECT_NAME}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}seven_filters.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}seven_filters.meta.csv
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

Add meta guidance to ${SUBJECT_NAME} subject
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document  90
    user enters text into element  id:metaGuidanceForm-content  Test meta guidance content
    user waits until page contains accordion section  ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor  ${SUBJECT_NAME}
    ...  meta guidance content

Add meta guidance to second Subject
    [Tags]  HappyPath
    user waits until h2 is visible  Public metadata guidance document
    user enters text into element  id:metaGuidanceForm-content  Test meta guidance content
    user waits until page contains accordion section  ${SECOND_SUBJECT}  15
    user enters text into meta guidance data file content editor  ${SECOND_SUBJECT}
    ...  meta guidance content
    user clicks button  Save guidance

Navigate to 'Footnotes' page
    [Tags]  HappyPath
    user clicks link  Footnotes
    user waits until h2 is visible  Footnotes

Add footnote to ${SECOND_SUBJECT}
    [Tags]  HappyPath
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote
    user clicks footnote checkbox  Select all indicators and filters for this subject
    user enters text into element  id:footnoteForm-content  Footnote 1 ${SECOND_SUBJECT}
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Add second footnote to ${SECOND_SUBJECT}
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote
    user opens details dropdown  Indicators
    user clicks footnote checkbox  Admission Numbers
    user enters text into element  id:footnoteForm-content  Footnote 2 ${SECOND_SUBJECT}
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Add footnote to ${SUBJECT_NAME} subject
    [Tags]  HappyPath
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote
    user clicks element  //*[@data-testid="footnote-subject ${SUBJECT_NAME}"]//input[@type="checkbox"]
    user enters text into element  id:footnoteForm-content  Footnote 1 ${SUBJECT_NAME}
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Add second footnote to ${SUBJECT_NAME} subject
    [Tags]  HappyPath
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote
    user opens details dropdown  Cheese
    user clicks footnote checkbox  Stilton
    user clicks footnote checkbox  Feta
    user enters text into element  id:footnoteForm-content  Footnote 2 ${SUBJECT_NAME}
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

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

Select "${SUBJECT_NAME}" subject
    [Tags]  HappyPath
    user clicks radio   ${SUBJECT_NAME}
    user clicks element   id:publicationSubjectForm-submit
    user waits until h2 is visible  Choose locations
    user checks previous table tool step contains  2    Subject     ${SUBJECT_NAME}

Select National location
    [Tags]  HappyPath
    user opens details dropdown  National
    user clicks checkbox  England

Click next step button
    [Tags]  HappyPath
    user clicks element     id:locationFiltersForm-submit
    user waits until h2 is visible  Choose time period

Select start date and end date
    [Tags]  HappyPath
    user selects from list by label  id:timePeriodForm-start   2012/13
    user selects from list by label  id:timePeriodForm-end     2012/13
    user clicks element     id:timePeriodForm-submit
    user waits until h2 is visible  Choose your filters
    user waits until page contains element   id:filtersForm-indicators
    user checks previous table tool step contains  4    Start date    2012/13
    user checks previous table tool step contains  4    End date      2012/13

Select Indicators
    [Tags]  HappyPath
    user clicks indicator checkbox  Lower quartile annualised earnings
    user checks indicator checkbox is checked  Lower quartile annualised earnings

Select cheese filter
    [Tags]  HappyPath
    user opens details dropdown  Cheese
    user clicks select all for category  Cheese

Select Number of years after achievement of learning aim filter
    [Tags]  HappyPath
    user opens details dropdown  Number of years after achievement of learning aim
    user clicks select all for category  Number of years after achievement of learning aim

Select ethnicity group filter
    [Tags]  HappyPath
    user opens details dropdown  Ethnicity group
    user clicks select all for category  Ethnicity group

Select Provision filter
    [Tags]  HappyPath
    user opens details dropdown  Provision
    user clicks select all for category  Provision

Click submit button
    [Tags]  HappyPath
    user clicks element     id:filtersForm-submit

Wait until table is generated
    [Tags]  HappyPath
    user waits until page contains button  Generate permanent link

Wait until new footnote is visible
    [Tags]  HappyPath
    user checks page contains  Footnote 1 ${SUBJECT_NAME}

Validate results table column headings
    [Tags]  HappyPath
    user checks results table row heading contains  1  1  Asian/Asian British
    user checks results table row heading contains  2  1  Black/African/Caribbean/Black British
    user checks results table row heading contains  3  1  Mixed/Multiple ethnic group

    user checks results table row heading contains  4  1  Not Known/Not Provided
    user checks results table row heading contains  5  1  Other Ethnic Group
    user checks results table row heading contains  6  1  Total
    user checks results table row heading contains  7  1  White

Validate row headings
    [Tags]  HappyPath
    user checks table column heading contains  1  1  1 year after study
    user checks table column heading contains  1  2  2 years after study
    user checks table column heading contains  1  3  3 years after study
    user checks table column heading contains  1  4  4 years after study
    user checks table column heading contains  1  5  5 years after study

Validate table cells
    [Tags]  HappyPath
    user checks results table cell contains  1   1  8
    user checks results table cell contains  2   1  2
    user checks results table cell contains  3   1  5
    user checks results table cell contains  4   1  8
    user checks results table cell contains  5   1  8
    user checks results table cell contains  6   1  2
    user checks results table cell contains  7   1  3

    user checks results table cell contains  1   2  10
    user checks results table cell contains  2   2  4
    user checks results table cell contains  3   2  8
    user checks results table cell contains  4   2  5
    user checks results table cell contains  5   2  5
    user checks results table cell contains  6   2  2
    user checks results table cell contains  7   2  6

    user checks results table cell contains  1   3  3
    user checks results table cell contains  2   3  0
    user checks results table cell contains  3   3  6
    user checks results table cell contains  4   3  3
    user checks results table cell contains  5   3  2
    user checks results table cell contains  6   3  8
    user checks results table cell contains  7   3  0

    user checks results table cell contains  1   4  3
    user checks results table cell contains  2   4  9
    user checks results table cell contains  3   4  4
    user checks results table cell contains  4   4  7
    user checks results table cell contains  5   4  4
    user checks results table cell contains  6   4  2
    user checks results table cell contains  7   4  8

    user checks results table cell contains  1   5  0
    user checks results table cell contains  2   5  6
    user checks results table cell contains  3   5  8
    user checks results table cell contains  4   5  1
    user checks results table cell contains  5   5  1
    user checks results table cell contains  6   5  9
    user checks results table cell contains  7   5  1

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
    user checks page contains  Footnote 1 ${SUBJECT_NAME}

Return to Admin
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user waits until h1 is visible   Dashboard
    user waits until page contains title caption  Welcome Bau1

Edit methodology
    [Tags]  HappyPath
    user clicks link  manage methodologies
    user clicks element  //*[@id="approved-methodologies-tab"]
    user clicks link  ${METHODOLOGY_NAME}
    user clicks link  Manage content
    user clicks button  ${METHODOLOGY_NAME}
    # NOTE: scrolls to element is here to avoid selenium clicking the
    # set page view text box on the methodology page
    user scrolls to element  xpath://button[text()="Add text block"]
    user waits until button is enabled  Add text block
    user clicks button  Add text block
    user clicks button  Edit block
    user presses keys  New & Updated content -
    user clicks button  Save
    user clicks button  Edit section title
    user enters text into element  xpath=//*[@name="heading"]  ${METHODOLOGY_NAME} New and Updated Title -
    user clicks button  Save section title

Create amendment
    [Tags]  HappyPath
    user clicks link  Home
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown   ${RELEASE_NAME} (Live - Latest release)  ${accordion}
    ${details}=  user gets details content element  ${RELEASE_NAME} (Live - Latest release)  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]
    user clicks button  Amend this release  ${details}
    user clicks button  Confirm

Replace data files for amendment
    [Tags]  HappyPath
    [Documentation]  EES-1442
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
    user opens details dropdown  Footnote 2 ${SUBJECT_NAME}
    user clicks button  Delete footnote
    user clicks button  Confirm
    #EES-1442: Bug when Confirm data replacement button doesn't show
    user reloads page
#    Sleep  10000
#    user scrolls down  800
    user waits until page contains  Footnotes: OK
    user waits until page contains  Data blocks: OK
    user waits until button is enabled  Confirm data replacement

Confirm data replacement
    [Tags]  HappyPath
    user clicks button  Confirm data replacement
    # potentially a bug here

Delete second subject file
    [Tags]  HappyPath
    user clicks link  Footnotes
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
    [Documentation]  EES-1448
    user waits for release process status to be  Complete    ${release_complete_wait}
    user reloads page  # EES-1448
    user checks page does not contain button  Edit release status

Go to permalink page & check for error element to be present
    [Tags]  HappyPath
    user goes to url  ${PERMA_LOCATION_URL}
    user waits until page contains  WARNING - The data used in this permalink may be out-of-date.

Check the table has the same results as original table
    [Tags]  HappyPath
    user checks results table row heading contains  1  1  Asian/Asian British
    user checks results table row heading contains  2  1  Black/African/Caribbean/Black British
    user checks results table row heading contains  3  1  Mixed/Multiple ethnic group

    user checks results table row heading contains  4  1  Not Known/Not Provided
    user checks results table row heading contains  5  1  Other Ethnic Group
    user checks results table row heading contains  6  1  Total
    user checks results table row heading contains  7  1  White

    user checks table column heading contains  1  1  1 year after study
    user checks table column heading contains  1  2  2 years after study
    user checks table column heading contains  1  3  3 years after study
    user checks table column heading contains  1  4  4 years after study
    user checks table column heading contains  1  5  5 years after study

    user checks results table cell contains  1   1  8
    user checks results table cell contains  2   1  2
    user checks results table cell contains  3   1  5
    user checks results table cell contains  4   1  8
    user checks results table cell contains  5   1  8
    user checks results table cell contains  6   1  2
    user checks results table cell contains  7   1  3

    user checks results table cell contains  1   2  10
    user checks results table cell contains  2   2  4
    user checks results table cell contains  3   2  8
    user checks results table cell contains  4   2  5
    user checks results table cell contains  5   2  5
    user checks results table cell contains  6   2  2
    user checks results table cell contains  7   2  6

    user checks results table cell contains  1   3  3
    user checks results table cell contains  2   3  0
    user checks results table cell contains  3   3  6
    user checks results table cell contains  4   3  3
    user checks results table cell contains  5   3  2
    user checks results table cell contains  6   3  8
    user checks results table cell contains  7   3  0

    user checks results table cell contains  1   4  3
    user checks results table cell contains  2   4  9
    user checks results table cell contains  3   4  4
    user checks results table cell contains  4   4  7
    user checks results table cell contains  5   4  4
    user checks results table cell contains  6   4  2
    user checks results table cell contains  7   4  8

    user checks results table cell contains  1   5  0
    user checks results table cell contains  2   5  6
    user checks results table cell contains  3   5  8
    user checks results table cell contains  4   5  1
    user checks results table cell contains  5   5  1
    user checks results table cell contains  6   5  9
    user checks results table cell contains  7   5  1

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

Return to admin to modify footnotes
    [Tags]  HappyPath
    user goes to url  %{ADMIN_URL}
    user waits until h1 is visible   Dashboard
    user waits until page contains title caption  Welcome Bau1

Create amendment to add and modify footnotes
    [Tags]  HappyPath
    user clicks link  Home
    user selects theme and topic from admin dashboard  %{TEST_THEME_NAME}  %{TEST_TOPIC_NAME}
    user waits until page contains accordion section   ${PUBLICATION_NAME}
    user opens accordion section  ${PUBLICATION_NAME}
    ${accordion}=  user gets accordion section content element  ${PUBLICATION_NAME}
    user opens details dropdown   ${RELEASE_NAME} (Live - Latest release)  ${accordion}
    ${details}=  user gets details content element  ${RELEASE_NAME} (Live - Latest release)  ${accordion}
    user waits until parent contains element   ${details}   xpath:.//a[text()="View this release"]
    user clicks button  Amend this release  ${details}
    user clicks button  Confirm

Add "upload file test with filter" subject file
    [Tags]  HappyPath
    user clicks link  Data and files
    user waits until page contains element  id:dataFileUploadForm-subjectTitle
    user enters text into element  id:dataFileUploadForm-subjectTitle   ${THIRD_SUBJECT}
    user chooses file   id:dataFileUploadForm-dataFile       ${FILES_DIR}upload-file-test-with-filter.csv
    user chooses file   id:dataFileUploadForm-metadataFile   ${FILES_DIR}upload-file-test-with-filter.meta.csv
    user clicks button  Upload data files
    user waits until h2 is visible  Uploaded data files
    user waits until page contains accordion section   ${THIRD_SUBJECT}
    user opens accordion section   ${THIRD_SUBJECT}
    ${section}=  user gets accordion section content element  ${THIRD_SUBJECT}
    user checks headed table body row contains  Status           Complete  ${section}  180

Add meta guidance to ${THIRD_SUBJECT} subject
    [Tags]  HappyPath
    user clicks link  Metadata guidance
    user waits until h2 is visible  Public metadata guidance document  90
    user enters text into element  id:metaGuidanceForm-content  Test meta guidance content
    user waits until page contains accordion section  ${THIRD_SUBJECT}
    user enters text into meta guidance data file content editor  ${THIRD_SUBJECT}
    ...  meta guidance content

Navigate to 'Footnotes' Tab
    [Tags]  HappyPath
    user clicks link  Footnotes
    user waits until h2 is visible  Footnotes

Add footnote to "upload file test filter" subject file
    [Tags]  HappyPath
    user clicks link  Create footnote
    user waits until h2 is visible  Create footnote
    user clicks element  //*[@data-testid="footnote-subject ${THIRD_SUBJECT}"]//input
    user enters text into element  id:footnoteForm-content  upload file test filter footnote
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Update ${SUBJECT_NAME} footnote
    [Tags]  HappyPath
    user clicks element  xpath://*[@data-testid="footnote Footnote 1 Seven filters"]//div//div[2]//a
    # seven filters
    user enters text into element  id:footnoteForm-content  Updating ${SUBJECT_NAME} footnote
    user clicks button  Save footnote
    user waits until h2 is visible  Footnotes

Go to "Sign off" page for amendment
    [Tags]  HappyPath
    user clicks link   Sign off
    user waits until h2 is visible  Sign off
    user waits until page contains button  Edit release status

Approve the release for amendment
    [Tags]  HappyPath
    user clicks button  Edit release status
    user waits until h2 is visible  Edit release status
    user clicks radio   Approved for publication
    user enters text into element  id:releaseStatusForm-internalReleaseNote  Approved by UI tests
    user clicks radio   As soon as possible
    user clicks button   Update status

Go to Table Tool page for amendment
    [Tags]  HappyPath
    user goes to url  %{PUBLIC_URL}/data-tables
    user waits until h1 is visible  Create your own tables online

Go to amended release & create table
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

Select ${SUBJECT_NAME} subject
    user clicks radio   ${SUBJECT_NAME}
    user clicks element   id:publicationSubjectForm-submit
    user waits until h2 is visible  Choose locations
    user checks previous table tool step contains  2    Subject     ${SUBJECT_NAME}

Select National location filter
    [Tags]  HappyPath
    user opens details dropdown  National
    user clicks checkbox  England

Click the next step button
    [Tags]  HappyPath
    user clicks element     id:locationFiltersForm-submit
    user waits until h2 is visible  Choose time period

Select start date + end date
    [Tags]  HappyPath
    user selects from list by label  id:timePeriodForm-start   2020 Week 13
    user selects from list by label  id:timePeriodForm-end     2021 Week 24
    user clicks element     id:timePeriodForm-submit
    user waits until h2 is visible  Choose your filters
    user waits until page contains element   id:filtersForm-indicators

Select four indicators
    [Tags]  HappyPath
    user clicks indicator checkbox  Number of open settings
    user checks indicator checkbox is checked  Number of open settings
    user clicks indicator checkbox  Number of children attending
    user checks indicator checkbox is checked  Number of children attending
    user clicks indicator checkbox  Number of children of critical workers attending
    user checks indicator checkbox is checked  Number of children of critical workers attending
    user clicks indicator checkbox  Response rate
    user checks indicator checkbox is checked  Response rate

Select the date cateogory
    [Tags]  HappyPath
    user opens details dropdown  Date
    user clicks select all for category  Date

Generate table
    [Tags]  HappyPath
    user clicks element     id:filtersForm-submit

Wait until the table is generated
    [Tags]  HappyPath
    user waits until page contains button  Generate permanent link

Generate the new permalink
    [Tags]  HappyPath
    [Documentation]  EES-214
    user clicks button  Generate permanent link
    user waits until page contains testid  permalink-generated-url
    ${PERMA_LOCATION_URL_TWO}=  Get Text  xpath://*[@data-testid="permalink-generated-url"]
    Set Suite Variable  ${PERMA_LOCATION_URL_TWO}

Go to new permalink
    [Tags]  HappyPath
    user goes to url  ${PERMA_LOCATION_URL_TWO}
    user waits until h1 is visible  '${SUBJECT_NAME}' from '${PUBLICATION_NAME}'
    user checks page does not contain   WARNING - The data used in this permalink may be out-of-date.
    user checks page contains  Updating ${SUBJECT_NAME} footnote