*** Settings ***
Resource            ../../libs/admin-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

*** Variables ***
${RELEASE_NAME}         Academic Year Q1 2020/21
${PUBLICATION_NAME}     UI tests - publish release and amend 2 %{RUN_IDENTIFIER}
${SUBJECT_NAME}         Seven filters
${SECOND_SUBJECT}       upload file test
${THIRD_SUBJECT}        upload file test with filter subject

*** Test Cases ***
Create publication
    [Tags]    HappyPath
    user navigates to admin dashboard
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    %{TEST_TOPIC_NAME}
    user clicks link    Create new publication
    user waits until h1 is visible    Create new publication
    user creates publication    ${PUBLICATION_NAME}

Create new methodology
    [Tags]    HappyPath
    user creates methodology for publication    ${PUBLICATION_NAME}

Add methodology content
    [Tags]    HappyPath
    user clicks link    Manage content
    user clicks button    Add new section
    user clicks button    New section
    # NOTE: scroll to element is here to avoid selenium clicking the
    # set page view text box on the methodology page
    user scrolls to element    xpath://button[text()="Add text block"]
    user clicks button    Add text block
    user clicks button    Edit block
    user presses keys    Adding Methodology content
    user clicks button    Save
    user clicks link    Go to top
    user clicks button    Edit section title
    user enters text into element    xpath=//*[@name="heading"]    ${PUBLICATION_NAME}
    user clicks button    Save section title

Approve methodology
    [Tags]    HappyPath
    user clicks link    Sign off
    user clicks button    Edit status
    user clicks radio    Approved for publication
    user enters text into element    xpath=//*[@name="latestInternalReleaseNote"]    Approved by UI tests
    user clicks button    Update status

Check methodology is approved
    [Tags]    HappyPath
    user waits until page contains element    xpath://strong[text()="Approved"]

Create new release
    [Tags]    HappyPath
    user opens publication on the admin dashboard    ${PUBLICATION_NAME}
    user clicks link    Create new release
    user creates release for publication    ${PUBLICATION_NAME}    Academic Year Q1    2020
    user clicks link    Data and files
    user uploads subject    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Upload another subject (for deletion later)
    [Tags]    HappyPath
    user waits until page contains element    id:dataFileUploadForm-subjectTitle
    user uploads subject    ${SECOND_SUBJECT}    upload-file-test.csv    upload-file-test.meta.csv

Add meta guidance to subject
    [Tags]    HappyPath
    user clicks link    Metadata guidance
    user waits until h2 is visible    Public metadata guidance document    90
    user enters text into element    id:metaGuidanceForm-content    Test meta guidance content
    user waits until page contains accordion section    ${SUBJECT_NAME}
    user enters text into meta guidance data file content editor    ${SUBJECT_NAME}
    ...    meta guidance content

Add meta guidance to second Subject
    [Tags]    HappyPath
    user waits until h2 is visible    Public metadata guidance document
    user enters text into element    id:metaGuidanceForm-content    Test meta guidance content
    user waits until page contains accordion section    ${SECOND_SUBJECT}    15
    user enters text into meta guidance data file content editor    ${SECOND_SUBJECT}
    ...    meta guidance content
    user clicks button    Save guidance

Navigate to 'Footnotes' page
    [Tags]    HappyPath
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add footnote to second Subject
    [Tags]    HappyPath
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote radio    ${SECOND_SUBJECT}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    Footnote 1 ${SECOND_SUBJECT}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add second footnote to second Subject
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote radio    ${SECOND_SUBJECT}    Applies to specific data
    user opens details dropdown    Indicators    testid:footnote-subject ${SECOND_SUBJECT}
    user clicks footnote checkbox    Admission Numbers
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    Footnote 2 ${SECOND_SUBJECT}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add footnote to subject
    [Tags]    HappyPath
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote radio    ${SUBJECT_NAME}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    Footnote 1 ${SUBJECT_NAME}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add second footnote to subject
    [Tags]    HappyPath
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote radio    ${SUBJECT_NAME}    Applies to specific data
    user opens details dropdown    Cheese    testid:footnote-subject ${SUBJECT_NAME}
    user clicks footnote checkbox    Stilton
    user clicks footnote checkbox    Feta
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    Footnote 2 ${SUBJECT_NAME}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add public prerelease access list
    [Tags]    HappyPath
    user clicks link    Pre-release access
    user creates public prerelease access list    Test public access list

Approve release
    [Tags]    HappyPath
    user clicks link    Sign off
    user approves release for immediate publication

Go to public Table Tool page
    [Tags]    HappyPath
    user navigates to data tables page on public frontend

Select "Test Topic" publication
    [Tags]    HappyPath
    environment variable should be set    TEST_THEME_NAME
    environment variable should be set    TEST_TOPIC_NAME
    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}

Select subject
    [Tags]    HappyPath
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    Choose locations
    user checks previous table tool step contains    2    Subject    ${SUBJECT_NAME}

Select National location
    [Tags]    HappyPath
    user opens details dropdown    National
    user clicks checkbox    England

Click next step button
    [Tags]    HappyPath
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    Choose time period

Select start date and end date
    [Tags]    HappyPath
    user selects from list by label    id:timePeriodForm-start    2012/13
    user selects from list by label    id:timePeriodForm-end    2012/13
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2012/13 to 2012/13

Select Indicators
    [Tags]    HappyPath
    user clicks indicator checkbox    Lower quartile annualised earnings
    user checks indicator checkbox is checked    Lower quartile annualised earnings

Select cheese filter
    [Tags]    HappyPath
    user opens details dropdown    Cheese
    user clicks select all for category    Cheese

Select Number of years after achievement of learning aim filter
    [Tags]    HappyPath
    user opens details dropdown    Number of years after achievement of learning aim
    user clicks select all for category    Number of years after achievement of learning aim

Select ethnicity group filter
    [Tags]    HappyPath
    user opens details dropdown    Ethnicity group
    user clicks select all for category    Ethnicity group

Select Provision filter
    [Tags]    HappyPath
    user opens details dropdown    Provision
    user clicks select all for category    Provision

Click submit button
    [Tags]    HappyPath
    user clicks element    id:filtersForm-submit

Wait until table is generated
    [Tags]    HappyPath
    user waits until page contains button    Generate shareable link

Wait until new footnote is visible
    [Tags]    HappyPath
    user checks page contains    Footnote 1 ${SUBJECT_NAME}

Validate results table column headings
    [Tags]    HappyPath
    user checks results table row heading contains    1    1    Asian/Asian British
    user checks results table row heading contains    2    1    Black/African/Caribbean/Black British
    user checks results table row heading contains    3    1    Mixed/Multiple ethnic group

    user checks results table row heading contains    4    1    Not Known/Not Provided
    user checks results table row heading contains    5    1    Other Ethnic Group
    user checks results table row heading contains    6    1    Total
    user checks results table row heading contains    7    1    White

Validate row headings
    [Tags]    HappyPath
    user checks table column heading contains    1    1    1 year after study
    user checks table column heading contains    1    2    2 years after study
    user checks table column heading contains    1    3    3 years after study
    user checks table column heading contains    1    4    4 years after study
    user checks table column heading contains    1    5    5 years after study

Validate table cells
    [Tags]    HappyPath
    user checks results table cell contains    1    1    8
    user checks results table cell contains    2    1    2
    user checks results table cell contains    3    1    5
    user checks results table cell contains    4    1    8
    user checks results table cell contains    5    1    8
    user checks results table cell contains    6    1    2
    user checks results table cell contains    7    1    3

    user checks results table cell contains    1    2    10
    user checks results table cell contains    2    2    4
    user checks results table cell contains    3    2    8
    user checks results table cell contains    4    2    5
    user checks results table cell contains    5    2    5
    user checks results table cell contains    6    2    2
    user checks results table cell contains    7    2    6

    user checks results table cell contains    1    3    3
    user checks results table cell contains    2    3    0
    user checks results table cell contains    3    3    6
    user checks results table cell contains    4    3    3
    user checks results table cell contains    5    3    2
    user checks results table cell contains    6    3    8
    user checks results table cell contains    7    3    0

    user checks results table cell contains    1    4    3
    user checks results table cell contains    2    4    9
    user checks results table cell contains    3    4    4
    user checks results table cell contains    4    4    7
    user checks results table cell contains    5    4    4
    user checks results table cell contains    6    4    2
    user checks results table cell contains    7    4    8

    user checks results table cell contains    1    5    0
    user checks results table cell contains    2    5    6
    user checks results table cell contains    3    5    8
    user checks results table cell contains    4    5    1
    user checks results table cell contains    5    5    1
    user checks results table cell contains    6    5    9
    user checks results table cell contains    7    5    1

Generate the permalink
    [Documentation]    EES-214
    [Tags]    HappyPath
    user waits until page contains button    Generate shareable link    60
    user clicks button    Generate shareable link
    user waits until page contains testid    permalink-generated-url
    ${PERMA_LOCATION_URL}    Get Value    testid:permalink-generated-url
    Set Suite Variable    ${PERMA_LOCATION_URL}

Go to permalink
    [Tags]    HappyPath
    user goes to url    ${PERMA_LOCATION_URL}
    user waits until h1 is visible    '${SUBJECT_NAME}' from '${PUBLICATION_NAME}'
    user checks page does not contain    WARNING - The data used in this permalink may be out-of-date.
    user checks page contains    Footnote 1 ${SUBJECT_NAME}

Return to Admin
    [Tags]    HappyPath
    user navigates to admin dashboard    Bau1

Change methodology status to Draft
    [Tags]    HappyPath
    user views methodology for publication    ${PUBLICATION_NAME}
    user clicks link    Sign off
    user clicks button    Edit status
    user clicks element    id:methodologyStatusForm-status-Draft
    user clicks button    Update status

Edit methodology content
    [Tags]    HappyPath
    user clicks link    Manage content
    user clicks button    ${PUBLICATION_NAME}

    user scrolls to element    xpath://button[text()="Add text block"]
    user waits until button is enabled    Add text block
    user clicks button    Add text block
    user clicks button    Edit block
    user presses keys    New & Updated content -
    user clicks button    Save
    user clicks button    Edit section title
    user enters text into element    xpath=//*[@name="heading"]    ${PUBLICATION_NAME} New and Updated Title -
    user clicks button    Save section title

Change methodology status to Approved
    [Tags]    HappyPath
    user clicks link    Sign off
    user changes methodology status to Approved

Create amendment
    [Tags]    HappyPath
    user clicks link    Home
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}    (Live - Latest release)

Replace subject data
    [Tags]    HappyPath
    user clicks link    Data and files
    user waits until page contains element    id:dataFileUploadForm-subjectTitle
    user waits until h2 is visible    Uploaded data files

    user waits until page contains accordion section    ${SUBJECT_NAME}
    user opens accordion section    ${SUBJECT_NAME}
    ${section}    user gets accordion section content element    ${SUBJECT_NAME}
    user clicks link    Replace data    ${section}
    user chooses file    id:dataFileUploadForm-dataFile    ${FILES_DIR}dates.csv
    user chooses file    id:dataFileUploadForm-metadataFile    ${FILES_DIR}dates.meta.csv
    user clicks button    Upload data files

    user waits until page contains    Footnotes: ERROR    %{WAIT_MEDIUM}
    user opens details dropdown    Footnote 2 ${SUBJECT_NAME}
    user clicks button    Delete footnote
    user clicks button    Confirm

    #EES-1442: Bug when Confirm data replacement button doesn't show
    user reloads page
    user waits until page contains    Footnotes: OK
    user waits until page contains    Data blocks: OK
    user waits until button is enabled    Confirm data replacement

Confirm data replacement
    [Tags]    HappyPath
    user clicks button    Confirm data replacement

Delete second subject file
    [Tags]    HappyPath
    user clicks link    Footnotes    # to avoid focus issues
    user clicks link    Data and files
    user deletes subject file    ${SECOND_SUBJECT}

Navigate to 'Content' page for amendment
    [Tags]    HappyPath
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block

Add release note to amendment
    [Tags]    HappyPath
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note one
    user clicks button    Save note
    ${date}    get current datetime    %-d %B %Y
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) p    Test release note one

Go to "Sign off" page and approve amendment
    [Tags]    HappyPath
    user clicks link    Sign off
    user approves release for immediate publication

Go to permalink page & check for error element to be present
    [Tags]    HappyPath
    user goes to url    ${PERMA_LOCATION_URL}
    user waits until page contains    WARNING - The data used in this permalink may be out-of-date.

Check the table has the same results as original table
    [Tags]    HappyPath
    user checks results table row heading contains    1    1    Asian/Asian British
    user checks results table row heading contains    2    1    Black/African/Caribbean/Black British
    user checks results table row heading contains    3    1    Mixed/Multiple ethnic group

    user checks results table row heading contains    4    1    Not Known/Not Provided
    user checks results table row heading contains    5    1    Other Ethnic Group
    user checks results table row heading contains    6    1    Total
    user checks results table row heading contains    7    1    White

    user checks table column heading contains    1    1    1 year after study
    user checks table column heading contains    1    2    2 years after study
    user checks table column heading contains    1    3    3 years after study
    user checks table column heading contains    1    4    4 years after study
    user checks table column heading contains    1    5    5 years after study

    user checks results table cell contains    1    1    8
    user checks results table cell contains    2    1    2
    user checks results table cell contains    3    1    5
    user checks results table cell contains    4    1    8
    user checks results table cell contains    5    1    8
    user checks results table cell contains    6    1    2
    user checks results table cell contains    7    1    3

    user checks results table cell contains    1    2    10
    user checks results table cell contains    2    2    4
    user checks results table cell contains    3    2    8
    user checks results table cell contains    4    2    5
    user checks results table cell contains    5    2    5
    user checks results table cell contains    6    2    2
    user checks results table cell contains    7    2    6

    user checks results table cell contains    1    3    3
    user checks results table cell contains    2    3    0
    user checks results table cell contains    3    3    6
    user checks results table cell contains    4    3    3
    user checks results table cell contains    5    3    2
    user checks results table cell contains    6    3    8
    user checks results table cell contains    7    3    0

    user checks results table cell contains    1    4    3
    user checks results table cell contains    2    4    9
    user checks results table cell contains    3    4    4
    user checks results table cell contains    4    4    7
    user checks results table cell contains    5    4    4
    user checks results table cell contains    6    4    2
    user checks results table cell contains    7    4    8

    user checks results table cell contains    1    5    0
    user checks results table cell contains    2    5    6
    user checks results table cell contains    3    5    8
    user checks results table cell contains    4    5    1
    user checks results table cell contains    5    5    1
    user checks results table cell contains    6    5    9
    user checks results table cell contains    7    5    1

Check amended release doesn't contain deleted subject
    [Tags]    HappyPath
    user goes to url    %{PUBLIC_URL}/data-tables
    user waits until h1 is visible    Create your own tables
    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}
    user checks page does not contain    ${SECOND_SUBJECT}

Create amendment to modify release
    [Tags]    HappyPath
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}    (Live - Latest release)

Add subject to release
    [Tags]    HappyPath
    user clicks link    Data and files
    user uploads subject    ${THIRD_SUBJECT}    upload-file-test-with-filter.csv
    ...    upload-file-test-with-filter.meta.csv

Add meta guidance to third subject
    [Tags]    HappyPath
    user clicks link    Metadata guidance
    user enters text into meta guidance data file content editor    ${THIRD_SUBJECT}    meta content
    user clicks button    Save guidance

Navigate to 'Footnotes' Tab
    [Tags]    HappyPath
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add footnote to "upload file test filter" subject file
    [Tags]    HappyPath
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote radio    ${THIRD_SUBJECT}    Applies to all data
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    upload file test filter footnote
    user clicks button    Save footnote
    user waits until page contains testid    footnote upload file test filter footnote
    user waits until h2 is visible    Footnotes

Update Seven filters footnote
    [Tags]    HappyPath
    user clicks link    Edit footnote    testid:footnote Footnote 1 ${SUBJECT_NAME}
    user clicks element    id:footnoteForm-content
    user enters text into element    id:footnoteForm-content    Updating ${SUBJECT_NAME} footnote
    textarea should contain    id:footnoteForm-content    Updating ${SUBJECT_NAME} footnote
    user clicks button    Save footnote
    user waits until page contains testid    footnote Updating ${SUBJECT_NAME} footnote

Go to "Sign off" to approve release for immedate publication
    [Tags]    HappyPath
    user clicks link    Sign off
    user approves release for immediate publication

Go to public Table Tool page for amendment
    [Tags]    HappyPath
    user goes to url    %{PUBLIC_URL}/data-tables
    user waits until h1 is visible    Create your own tables

Select publication
    [Tags]    HappyPath
    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    %{TEST_TOPIC_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}
    #user checks page does not contain    ${SECOND_SUBJECT}    # EES-1360

Select subject again
    [Tags]    HappyPath
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    Choose locations
    user checks previous table tool step contains    2    Subject    ${SUBJECT_NAME}

Select National location filter
    [Tags]    HappyPath
    user opens details dropdown    National
    user clicks checkbox    England

Click the next step button
    [Tags]    HappyPath
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    Choose time period

Select start date + end date
    [Tags]    HappyPath
    user selects from list by label    id:timePeriodForm-start    2020 Week 13
    user selects from list by label    id:timePeriodForm-end    2021 Week 24
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    Choose your filters
    user waits until page contains element    id:filtersForm-indicators

Select four indicators
    [Tags]    HappyPath
    user clicks indicator checkbox    Number of open settings
    user checks indicator checkbox is checked    Number of open settings
    user clicks indicator checkbox    Number of children attending
    user checks indicator checkbox is checked    Number of children attending
    user clicks indicator checkbox    Number of children of critical workers attending
    user checks indicator checkbox is checked    Number of children of critical workers attending
    user clicks indicator checkbox    Response rate
    user checks indicator checkbox is checked    Response rate

Select the date cateogory
    [Tags]    HappyPath
    user opens details dropdown    Date
    user clicks select all for category    Date

Generate table
    [Tags]    HappyPath
    user clicks element    id:filtersForm-submit
    user waits until page contains    Generate shareable link    60

Validate generated table
    [Tags]    HappyPath
    user checks page contains    Updating ${SUBJECT_NAME} footnote

Generate the new permalink
    [Documentation]    EES-214
    [Tags]    HappyPath
    user clicks button    Generate shareable link
    user waits until page contains testid    permalink-generated-url
    ${PERMA_LOCATION_URL_TWO}    Get Value    testid:permalink-generated-url
    Set Suite Variable    ${PERMA_LOCATION_URL_TWO}

Go to new permalink
    [Tags]    HappyPath
    user goes to url    ${PERMA_LOCATION_URL_TWO}
    user waits until h1 is visible    '${SUBJECT_NAME}' from '${PUBLICATION_NAME}'
    user checks page does not contain    WARNING - The data used in this permalink may be out-of-date.
    user checks page contains    Updating ${SUBJECT_NAME} footnote
