*** Settings ***
Resource            ../../libs/admin-common.robot
Resource            ../../libs/admin/manage-content-common.robot
Library             ../../libs/admin_api.py

Force Tags          Admin    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required


*** Variables ***
${RELEASE_NAME}         Academic year Q1 2020/21
${PUBLICATION_NAME}     UI tests - publish release and amend 2 %{RUN_IDENTIFIER}
${SUBJECT_NAME}         Seven filters
${SECOND_SUBJECT}       upload file test
${THIRD_SUBJECT}        upload file test with filter subject


*** Test Cases ***
Create publication
    user selects dashboard theme and topic if possible
    user clicks link    Create new publication
    user waits until h1 is visible    Create new publication
    user creates publication    ${PUBLICATION_NAME}

Create new release
    user navigates to publication page from dashboard    ${PUBLICATION_NAME}
    user creates release from publication page    ${PUBLICATION_NAME}    Academic year Q1    2020
    user uploads subject    ${SUBJECT_NAME}    seven_filters.csv    seven_filters.meta.csv

Upload another subject (for deletion later)
    user waits until page contains element    id:dataFileUploadForm-subjectTitle
    user uploads subject    ${SECOND_SUBJECT}    upload-file-test.csv    upload-file-test.meta.csv

Add data guidance to subject
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance    %{WAIT_MEDIUM}
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    ${SUBJECT_NAME}
    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    data guidance content

Add data guidance to second Subject
    user waits until h2 is visible    Public data guidance
    user enters text into element    id:dataGuidanceForm-content    Test data guidance content
    user waits until page contains accordion section    ${SECOND_SUBJECT}    15
    user enters text into data guidance data file content editor    ${SECOND_SUBJECT}
    ...    data guidance content
    user clicks button    Save guidance
    user waits until page contains button    ${SUBJECT_NAME}
    user waits until page contains button    ${SECOND_SUBJECT}

Navigate to 'Footnotes' page
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add footnote to second Subject
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SECOND_SUBJECT}    Applies to all data
    user enters text into element    label:Footnote    Footnote 1 ${SECOND_SUBJECT}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add second footnote to second Subject
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SECOND_SUBJECT}    Applies to specific data
    user opens footnote subject dropdown    ${SECOND_SUBJECT}    Indicators
    user clicks footnote subject checkbox    ${SECOND_SUBJECT}    Indicators    Admission Numbers
    user enters text into element    label:Footnote    Footnote 2 ${SECOND_SUBJECT}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add footnote to subject
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to all data
    user enters text into element    label:Footnote    Footnote 1 ${SUBJECT_NAME}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add second footnote to subject
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${SUBJECT_NAME}    Applies to specific data
    user opens footnote subject dropdown    ${SUBJECT_NAME}    Cheese
    user clicks footnote subject checkbox    ${SUBJECT_NAME}    Cheese    Stilton
    user clicks footnote subject checkbox    ${SUBJECT_NAME}    Cheese    Feta
    user enters text into element    label:Footnote    Footnote 2 ${SUBJECT_NAME}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Add public prerelease access list
    user clicks link    Pre-release access
    user creates public prerelease access list    Test public access list

Approve release
    user clicks link    Sign off
    user approves original release for immediate publication

Go to public Table Tool page
    user navigates to data tables page on public frontend

Select "Test Topic" publication
    environment variable should be set    TEST_THEME_NAME
    environment variable should be set    TEST_TOPIC_NAME
    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}

Select subject
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Subject    ${SUBJECT_NAME}

Select National location
    user checks location checkbox is checked    England

Click next step button
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period

Select start date and end date
    user chooses select option    id:timePeriodForm-start    2012/13
    user chooses select option    id:timePeriodForm-end    2012/13
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2012/13

Select Indicators
    user clicks indicator checkbox    Lower quartile annualised earnings
    user checks indicator checkbox is checked    Lower quartile annualised earnings

Select cheese filter
    user opens details dropdown    Cheese
    user clicks select all for category    Cheese

Select Number of years after achievement of learning aim filter
    user opens details dropdown    Number of years after achievement of learning aim
    user clicks select all for category    Number of years after achievement of learning aim

Select ethnicity group filter
    user opens details dropdown    Ethnicity group
    user clicks select all for category    Ethnicity group

Select Provision filter
    user opens details dropdown    Provision
    user clicks select all for category    Provision

Click submit button
    user clicks element    id:filtersForm-submit

Wait until table is generated
    user waits until page contains button    Generate shareable link

Wait until new footnote is visible
    user checks page contains    Footnote 1 ${SUBJECT_NAME}

Validate results table column headings
    user checks table row heading contains    1    1    Total
    user checks table row heading contains    2    1    Asian/Asian British
    user checks table row heading contains    3    1    Black/African/Caribbean/Black British
    user checks table row heading contains    4    1    Mixed/Multiple ethnic group
    user checks table row heading contains    5    1    Not Known/Not Provided
    user checks table row heading contains    6    1    Other Ethnic Group
    user checks table row heading contains    7    1    White

Validate row headings
    user checks table column heading contains    1    1    1 year after study
    user checks table column heading contains    1    2    2 years after study
    user checks table column heading contains    1    3    3 years after study
    user checks table column heading contains    1    4    4 years after study
    user checks table column heading contains    1    5    5 years after study

Validate table cells
    user checks table cell contains    1    1    2
    user checks table cell contains    2    1    8
    user checks table cell contains    3    1    2
    user checks table cell contains    4    1    5
    user checks table cell contains    5    1    8
    user checks table cell contains    6    1    8
    user checks table cell contains    7    1    3

    user checks table cell contains    1    2    2
    user checks table cell contains    2    2    10
    user checks table cell contains    3    2    4
    user checks table cell contains    4    2    8
    user checks table cell contains    5    2    5
    user checks table cell contains    6    2    5
    user checks table cell contains    7    2    6

    user checks table cell contains    1    3    8
    user checks table cell contains    2    3    3
    user checks table cell contains    3    3    0
    user checks table cell contains    4    3    6
    user checks table cell contains    5    3    3
    user checks table cell contains    6    3    2
    user checks table cell contains    7    3    0

    user checks table cell contains    1    4    2
    user checks table cell contains    2    4    3
    user checks table cell contains    3    4    9
    user checks table cell contains    4    4    4
    user checks table cell contains    5    4    7
    user checks table cell contains    6    4    4
    user checks table cell contains    7    4    8

    user checks table cell contains    1    5    9
    user checks table cell contains    2    5    0
    user checks table cell contains    3    5    6
    user checks table cell contains    4    5    8
    user checks table cell contains    5    5    1
    user checks table cell contains    6    5    1
    user checks table cell contains    7    5    1

Generate the permalink
    [Documentation]    EES-214
    user waits until page contains button    Generate shareable link    %{WAIT_SMALL}
    user clicks button    Generate shareable link
    user waits until page contains testid    permalink-generated-url
    ${PERMA_LOCATION_URL}    Get Value    testid:permalink-generated-url
    Set Suite Variable    ${PERMA_LOCATION_URL}

Go to permalink
    user navigates to public frontend    ${PERMA_LOCATION_URL}
    user waits until h1 is visible    '${SUBJECT_NAME}' from '${PUBLICATION_NAME}'
    user checks page does not contain    WARNING
    user checks page contains    Footnote 1 ${SUBJECT_NAME}

Return to Admin
    user navigates to admin dashboard    Bau1

Create release amendment
    user clicks link    Home
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Replace subject data
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
    user clicks button    Confirm data replacement
    user waits until h2 is visible    Data replacement complete    %{WAIT_MEDIUM}

Delete second subject file
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release
    user deletes subject file    ${SECOND_SUBJECT}

Navigate to 'Content' page for release amendment
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block

Add release note to release amendment
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note one
    user clicks button    Save note
    ${date}    get current datetime    %-d %B %Y
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) p    Test release note one

Go to "Sign off" page
    user clicks link    Sign off
    user waits until h3 is visible    Release status history

Validate Release status table row is correct
    user waits until page contains element    css:table
    user checks element count is x    xpath://table/tbody/tr    1
    ${datetime}    get current datetime    %-d %B %Y
    table cell should contain    css:table    2    1    ${datetime}    # Date
    table cell should contain    css:table    2    2    Approved    # Status
    table cell should contain    css:table    2    3    Approved by UI tests    # Internal note
    table cell should contain    css:table    2    4    1    # Release version
    table cell should contain    css:table    2    5    ees-test.bau1@education.gov.uk    # By user

Approve release amendment
    user approves amended release for immediate publication

Check new release status history entry is present
    user waits until h3 is visible    Release status history    10
    table cell should contain    testid:release-status-history    2    4    2    # Release version 2

Go to permalink page & check for error element to be present
    user navigates to public frontend    ${PERMA_LOCATION_URL}
    user waits until page contains
    ...    WARNING - The data used in this table may be invalid as the subject file has been amended or removed since its creation.

Check the table has the same results as original table
    user checks table row heading contains    1    1    Total
    user checks table row heading contains    2    1    Asian/Asian British
    user checks table row heading contains    3    1    Black/African/Caribbean/Black British
    user checks table row heading contains    4    1    Mixed/Multiple ethnic group
    user checks table row heading contains    5    1    Not Known/Not Provided
    user checks table row heading contains    6    1    Other Ethnic Group
    user checks table row heading contains    7    1    White

    user checks table column heading contains    1    1    1 year after study
    user checks table column heading contains    1    2    2 years after study
    user checks table column heading contains    1    3    3 years after study
    user checks table column heading contains    1    4    4 years after study
    user checks table column heading contains    1    5    5 years after study

    user checks table cell contains    1    1    2
    user checks table cell contains    2    1    8
    user checks table cell contains    3    1    2
    user checks table cell contains    4    1    5
    user checks table cell contains    5    1    8
    user checks table cell contains    6    1    8
    user checks table cell contains    7    1    3

    user checks table cell contains    1    2    2
    user checks table cell contains    2    2    10
    user checks table cell contains    3    2    4
    user checks table cell contains    4    2    8
    user checks table cell contains    5    2    5
    user checks table cell contains    6    2    5
    user checks table cell contains    7    2    6

    user checks table cell contains    1    3    8
    user checks table cell contains    2    3    3
    user checks table cell contains    3    3    0
    user checks table cell contains    4    3    6
    user checks table cell contains    5    3    3
    user checks table cell contains    6    3    2
    user checks table cell contains    7    3    0

    user checks table cell contains    1    4    2
    user checks table cell contains    2    4    3
    user checks table cell contains    3    4    9
    user checks table cell contains    4    4    4
    user checks table cell contains    5    4    7
    user checks table cell contains    6    4    4
    user checks table cell contains    7    4    8

    user checks table cell contains    1    5    9
    user checks table cell contains    2    5    0
    user checks table cell contains    3    5    6
    user checks table cell contains    4    5    8
    user checks table cell contains    5    5    1
    user checks table cell contains    6    5    1
    user checks table cell contains    7    5    1

Check amended release doesn't contain deleted subject
    user navigates to public frontend    %{PUBLIC_URL}/data-tables
    user waits until h1 is visible    Create your own tables
    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}
    user checks page does not contain    ${SECOND_SUBJECT}

Create amendment to modify release
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Add subject to release
    user uploads subject    ${THIRD_SUBJECT}    upload-file-test-with-filter.csv
    ...    upload-file-test-with-filter.meta.csv

Add data guidance to third subject
    user clicks link    Data guidance
    user enters text into data guidance data file content editor    ${THIRD_SUBJECT}    meta content
    user clicks button    Save guidance

Navigate to 'Footnotes' Tab
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Add footnote to "upload file test filter" subject file
    user waits until page contains link    Create footnote
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote
    user clicks footnote subject radio    ${THIRD_SUBJECT}    Applies to all data
    user clicks element    label:Footnote
    user enters text into element    label:Footnote    upload file test filter footnote
    user clicks button    Save footnote
    user waits until page contains element    testid:Footnote - upload file test filter footnote
    user waits until h2 is visible    Footnotes

Update Seven filters footnote
    user clicks link    Edit footnote    testid:Footnote - Footnote 1 ${SUBJECT_NAME}
    user clicks element    label:Footnote
    user enters text into element    label:Footnote    Updating ${SUBJECT_NAME} footnote
    user clicks button    Save footnote
    user waits until page contains element    testid:Footnote - Updating ${SUBJECT_NAME} footnote

Add release note for new release amendment
    user clicks link    Content
    user clicks button    Add note
    user enters text into element    id:createReleaseNoteForm-reason    Test release note two
    user clicks button    Save note
    ${date}    get current datetime    %-d %B %Y
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) time    ${date}
    user waits until element contains    css:#releaseNotes li:nth-of-type(1) p    Test release note two

Go to "Sign off" to approve amended release for immediate publication
    user clicks link    Sign off
    user approves amended release for immediate publication

Go to public Table Tool page for amendment
    user navigates to public frontend    %{PUBLIC_URL}/data-tables
    user waits until h1 is visible    Create your own tables

Select publication
    user clicks radio    %{TEST_THEME_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}
    #user checks page does not contain    ${SECOND_SUBJECT}    # EES-1360

Select subject again
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Subject    ${SUBJECT_NAME}

Select National location filter
    user checks location checkbox is checked    England

Click the next step button
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period

Select start date + end date
    user chooses select option    id:timePeriodForm-start    2020 Week 13
    user chooses select option    id:timePeriodForm-end    2021 Week 24
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators

Select four indicators
    user clicks indicator checkbox    Number of open settings
    user checks indicator checkbox is checked    Number of open settings
    user clicks indicator checkbox    Number of children attending
    user checks indicator checkbox is checked    Number of children attending
    user clicks indicator checkbox    Number of children of critical workers attending
    user checks indicator checkbox is checked    Number of children of critical workers attending
    user clicks indicator checkbox    Response rate
    user checks indicator checkbox is checked    Response rate

Select the date cateogory
    user opens details dropdown    Date
    user clicks select all for category    Date

Attempt to generate a table that is too large
    user clicks element    id:filtersForm-submit
    user waits until page contains
    ...    Could not create table as the filters chosen may exceed the maximum allowed table size.
    user waits until page contains    Select different filters or download the subject data.
    user waits until page contains button    Download Seven filters (csv, 17 Kb)    %{WAIT_MEDIUM}

Reduce the number of selected Dates and generate a smaller table
    user clicks unselect all for category    Date
    user clicks category checkbox    Date    23/03/2020
    user clicks category checkbox    Date    24/03/2020
    user clicks category checkbox    Date    25/03/2020
    user clicks element    id:filtersForm-submit
    user waits until page contains    Generate shareable link    %{WAIT_SMALL}

Validate generated table
    user checks page contains    Updating ${SUBJECT_NAME} footnote

Generate the new permalink
    [Documentation]    EES-214
    user clicks button    Generate shareable link
    user waits until page contains testid    permalink-generated-url
    ${PERMA_LOCATION_URL_TWO}    Get Value    testid:permalink-generated-url
    Set Suite Variable    ${PERMA_LOCATION_URL_TWO}

Go to new permalink
    user navigates to public frontend    ${PERMA_LOCATION_URL_TWO}
    user waits until h1 is visible    '${SUBJECT_NAME}' from '${PUBLICATION_NAME}'
    user checks page does not contain    WARNING
    user checks page contains    Updating ${SUBJECT_NAME} footnote
