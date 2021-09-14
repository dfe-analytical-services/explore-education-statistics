*** Settings ***
Library             ../../libs/admin_api.py
Resource            ../../libs/admin-common.robot
Resource            ../../libs/public-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser

Force Tags          Admin    Local    Dev    AltersData

*** Variables ***
${TOPIC_NAME}=                          %{TEST_TOPIC_NAME}
${PUBLICATION_NAME}=                    UI tests - publish data %{RUN_IDENTIFIER}
${RELEASE_1_NAME}=                      Financial Year 3000-01
${RELEASE_2_NAME}=                      Financial Year 3001-02
${SUBJECT_1_NAME}=                      UI test subject 1
${SUBJECT_2_NAME}=                      UI test subject 2
${FOOTNOTE_ALL}=                        Footnote for all subjects
${FOOTNOTE_ALL_INDICATOR}=              Footnote for all subjects - indicator
${FOOTNOTE_ALL_INDICATOR_UPDATED}=      Footnote for all subjects - updated indicator
${FOOTNOTE_ALL_FILTER}=                 Footnote for all subjects - filters
${FOOTNOTE_SUBJECT_1}=                  Footnote for subject 1
${FOOTNOTE_SUBJECT_1_INDICATOR}=        Footnote for subject 1 - indicator
${FOOTNOTE_SUBJECT_1_FILTER}=           Footnote for subject 1 - filter
${FOOTNOTE_SUBJECT_1_FILTER_GROUP}=     Footnote for subject 1 - filter group
${FOOTNOTE_SUBJECT_1_FILTER_ITEM}=      Footnote for subject 1 - filter item
${FOOTNOTE_SUBJECT_1_MIXTURE}=          Footnote for subject 1 - mixture of all

*** Test Cases ***
Create new publication and release via API
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user create test release via api    ${PUBLICATION_ID}    FY    3000

Navigate to release
    user navigates to editable release summary from admin dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_1_NAME} (not Live)

Add public prerelease access list
    user clicks link    Pre-release access
    user waits until h2 is visible    Manage pre-release user access

    user clicks link    Public access list
    user waits until h2 is visible    Public pre-release access list
    user clicks button    Create public pre-release access list
    user presses keys    CTRL+a+BACKSPACE
    user presses keys    Test public access list
    user clicks button    Save access list
    user waits until element contains    css:[data-testid="publicPreReleaseAccessListPreview"]
    ...    Test public access list

Go to "Sign off" page and approve release
    user clicks link    Sign off
    user approves release for immediate publication

Create another release for the same publication
    user selects theme and topic from admin dashboard    %{TEST_THEME_NAME}    ${TOPIC_NAME}
    user waits until page contains link    Create new publication
    user opens accordion section    ${PUBLICATION_NAME}
    user clicks element    testid:Create new release link for ${PUBLICATION_NAME}
    user creates release for publication    ${PUBLICATION_NAME}    Financial Year    3001

Verify new release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user waits until h2 is visible    Release summary
    user checks summary list contains    Publication title    ${PUBLICATION_NAME}

Upload subjects to release
    user clicks link    Data and files
    user uploads subject    ${SUBJECT_1_NAME}    tiny-two-filters.csv    tiny-two-filters.meta.csv
    user uploads subject    ${SUBJECT_2_NAME}    upload-file-test.csv    upload-file-test-with-filter.meta.csv

Navigate to Footnotes page
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes

Create footnote for both subjects
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to all data
    user clicks footnote subject radio    ${SUBJECT_2_NAME}    Applies to all data
    user enters text into element    label:Footnote    ${FOOTNOTE_ALL}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Create footnote for both subject indicators
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to specific data
    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    Indicators
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Indicators    Authorised absence rate
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Indicators    Number of persistent absentees

    user clicks footnote subject radio    ${SUBJECT_2_NAME}    Applies to specific data
    user opens footnote subject dropdown    ${SUBJECT_2_NAME}    Indicators
    user clicks footnote subject checkbox    ${SUBJECT_2_NAME}    Indicators    Admission Numbers

    user enters text into element    label:Footnote    ${FOOTNOTE_ALL_INDICATOR}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Create footnote for both subject filters
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to specific data
    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    School type
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    Select all

    user clicks footnote subject radio    ${SUBJECT_2_NAME}    Applies to specific data
    user opens footnote subject dropdown    ${SUBJECT_2_NAME}    Random Filter
    user clicks footnote subject checkbox    ${SUBJECT_2_NAME}    Random Filter    Select all

    user enters text into element    label:Footnote    ${FOOTNOTE_ALL_FILTER}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Create footnote for subject 1
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to all data

    user enters text into element    label:Footnote    ${FOOTNOTE_SUBJECT_1}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Create footnote for subject 1 indicators
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to specific data
    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    Indicators
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Indicators    Authorised absence rate
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Indicators    Number of persistent absentees

    user enters text into element    label:Footnote    ${FOOTNOTE_SUBJECT_1_INDICATOR}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Create footnote for subject 1 filters
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to specific data

    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    School type
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    Select all

    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    Colour
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Colour    Select all

    user enters text into element    label:Footnote    ${FOOTNOTE_SUBJECT_1_FILTER}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Create footnote for subject 1 filter groups
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to specific data

    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    School type
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    Combined
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    Individual

    user enters text into element    label:Footnote    ${FOOTNOTE_SUBJECT_1_FILTER_GROUP}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Create footnote for subject 1 filter items
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to specific data

    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    School type
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    State-funded primary and secondary
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    State-funded primary
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    Total

    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    Colour
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Colour    Blue
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Colour    Orange

    user enters text into element    label:Footnote    ${FOOTNOTE_SUBJECT_1_FILTER_ITEM}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Create footnote for subject 1 with mixture of indicators and filters
    user clicks link    Create footnote
    user waits until h2 is visible    Create footnote

    user clicks footnote subject radio    ${SUBJECT_1_NAME}    Applies to specific data

    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    Indicators
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Indicators    Percentage of persistent absentees
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Indicators    Unauthorised absence rate

    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    School type
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    Combined
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    State-funded primary
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    State-funded secondary
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    School type    Total

    user opens footnote subject dropdown    ${SUBJECT_1_NAME}    Colour
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Colour    Blue
    user clicks footnote subject checkbox    ${SUBJECT_1_NAME}    Colour    Orange

    user enters text into element    label:Footnote    ${FOOTNOTE_SUBJECT_1_MIXTURE}
    user clicks button    Save footnote
    user waits until h2 is visible    Footnotes

Confirm created footnotes
    user waits until h2 is visible    Footnotes
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_ALL}
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_ALL_INDICATOR}
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_ALL_FILTER}
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_SUBJECT_1}
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_SUBJECT_1_INDICATOR}
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_SUBJECT_1_FILTER}
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_SUBJECT_1_FILTER_GROUP}
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_SUBJECT_1_FILTER_ITEM}
    user waits until page contains element    testid:Footnote - ${FOOTNOTE_SUBJECT_1_MIXTURE}

Add data guidance to subjects
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_1_NAME}
    user enters text into data guidance data file content editor    ${SUBJECT_1_NAME}
    ...    ${SUBJECT_1_NAME} data guidance content

    user waits until page contains accordion section    ${SUBJECT_2_NAME}
    user enters text into data guidance data file content editor    ${SUBJECT_2_NAME}
    ...    ${SUBJECT_2_NAME} data guidance content

Validate data guidance variables and descriptions
    ${subject_1_content}=    user gets accordion section content element    ${SUBJECT_1_NAME}
    ...    id:dataGuidance-dataFiles
    user opens details dropdown    Variable names and descriptions    ${subject_1_content}

    ${subject_1_variables}=    get child element    ${subject_1_content}    testid:Variables
    user checks table body has x rows    9    ${subject_1_variables}

    user checks table column heading contains    1    1    Variable name    ${subject_1_variables}
    user checks table column heading contains    1    2    Variable description    ${subject_1_variables}

    user checks results table cell contains    1    1    colour    ${subject_1_variables}
    user checks results table cell contains    1    2    Colour    ${subject_1_variables}

    user checks results table cell contains    2    1    enrolments    ${subject_1_variables}
    user checks results table cell contains    2    2    Number of pupil enrolments    ${subject_1_variables}

    user checks results table cell contains    4    1    enrolments_pa_10_exact_percent    ${subject_1_variables}
    user checks results table cell contains    4    2    Percentage of persistent absentees    ${subject_1_variables}

    user checks results table cell contains    8    1    sess_overall_percent    ${subject_1_variables}
    user checks results table cell contains    8    2    Overall absence rate    ${subject_1_variables}

    user checks results table cell contains    9    1    sess_unauthorised_percent    ${subject_1_variables}
    user checks results table cell contains    9    2    Unauthorised absence rate    ${subject_1_variables}

    ${subject_2_content}=    user gets accordion section content element    ${SUBJECT_2_NAME}
    ...    id:dataGuidance-dataFiles
    user opens details dropdown    Variable names and descriptions    ${subject_2_content}

    ${subject_2_variables}=    get child element    ${subject_2_content}    testid:Variables
    user checks table body has x rows    2    ${subject_2_variables}

    user checks table column heading contains    1    1    Variable name    ${subject_2_variables}
    user checks table column heading contains    1    2    Variable description    ${subject_2_variables}

    user checks results table cell contains    1    1    admission_numbers    ${subject_2_variables}
    user checks results table cell contains    1    2    Admission Numbers    ${subject_2_variables}

    user checks results table cell contains    2    1    some_filter    ${subject_2_variables}
    user checks results table cell contains    2    2    Random Filter    ${subject_2_variables}

Validate data guidance footnotes
    ${subject_1_content}=    user gets accordion section content element    ${SUBJECT_1_NAME}
    ...    id:dataGuidance-dataFiles
    user opens details dropdown    Footnotes    ${subject_1_content}

    user checks list has x items    testid:Footnotes    9    ${subject_1_content}
    user checks list item contains    testid:Footnotes    1    ${FOOTNOTE_ALL}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    2    ${FOOTNOTE_ALL_INDICATOR}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    3    ${FOOTNOTE_ALL_FILTER}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    4    ${FOOTNOTE_SUBJECT_1}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    5    ${FOOTNOTE_SUBJECT_1_INDICATOR}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    6    ${FOOTNOTE_SUBJECT_1_FILTER}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    7    ${FOOTNOTE_SUBJECT_1_FILTER_GROUP}
    ...    ${subject_1_content}
    user checks list item contains    testid:Footnotes    8    ${FOOTNOTE_SUBJECT_1_FILTER_ITEM}
    ...    ${subject_1_content}
    user checks list item contains    testid:Footnotes    9    ${FOOTNOTE_SUBJECT_1_MIXTURE}    ${subject_1_content}

    ${subject_2_content}=    user gets accordion section content element    ${SUBJECT_2_NAME}
    ...    id:dataGuidance-dataFiles
    user opens details dropdown    Footnotes    ${subject_2_content}

    user checks list has x items    testid:Footnotes    3    ${subject_2_content}
    user checks list item contains    testid:Footnotes    1    ${FOOTNOTE_ALL}    ${subject_2_content}
    user checks list item contains    testid:Footnotes    2    ${FOOTNOTE_ALL_INDICATOR}    ${subject_2_content}
    user checks list item contains    testid:Footnotes    3    ${FOOTNOTE_ALL_FILTER}    ${subject_2_content}

Save data guidance
    user clicks button    Save guidance

Navigate to Data blocks page
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

Create new data block
    user clicks link    Create data block
    user waits until h2 is visible    Create data block
    user waits until table tool wizard step is available    Choose a subject

Select subject "${SUBJECT_2_NAME}"
    user waits until page contains    ${SUBJECT_2_NAME}
    user clicks radio    ${SUBJECT_2_NAME}
    user clicks element    id:publicationSubjectForm-submit

Select locations
    user waits until table tool wizard step is available    Choose locations
    user opens details dropdown    Opportunity Area
    user clicks checkbox    Bolton 001 (E02000984)
    user clicks checkbox    Bolton 001 (E05000364)
    user clicks checkbox    Bolton 004 (E02000987)
    user clicks checkbox    Bolton 004 (E05010450)
    user opens details dropdown    Ward
    user clicks checkbox    Nailsea Youngwood
    user clicks checkbox    Syon
    user clicks element    id:locationFiltersForm-submit

Select time period
    user waits until table tool wizard step is available    Choose time period
    user chooses select option    id:timePeriodForm-start    2005
    user chooses select option    id:timePeriodForm-end    2020
    user clicks element    id:timePeriodForm-submit

Select indicators and filters
    user waits until table tool wizard step is available    Choose your filters
    user clicks indicator checkbox    Admission Numbers

    user opens details dropdown    Random Filter
    user clicks category checkbox    Random Filter    Not specified

Create table
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_LONG}

Check created table has footnotes
    user checks list has x items    testid:footnotes    3
    user checks list item contains    testid:footnotes    1    ${FOOTNOTE_ALL}
    user checks list item contains    testid:footnotes    2    ${FOOTNOTE_ALL_INDICATOR}
    user checks list item contains    testid:footnotes    3    ${FOOTNOTE_ALL_FILTER}

Save data block as a featured table
    user enters text into element    id:dataBlockDetailsForm-name    UI Test data block name
    user enters text into element    id:dataBlockDetailsForm-heading    UI Test table title
    user enters text into element    id:dataBlockDetailsForm-source    UI Test source

    user clicks checkbox    Set as a featured table for this publication
    user waits until page contains element    id:dataBlockDetailsForm-highlightName
    user enters text into element    id:dataBlockDetailsForm-highlightName    Test highlight name
    user enters text into element    id:dataBlockDetailsForm-highlightDescription    Test highlight description

    user clicks button    Save data block
    user waits until page contains    Delete this data block

Edit footnote
    user clicks link    Footnotes
    user waits until h2 is visible    Footnotes
    user clicks link    Edit footnote    testid:Footnote - ${FOOTNOTE_ALL_INDICATOR}

    user waits until h2 is visible    Edit footnote
    user enters text into element    label:Footnote    ${FOOTNOTE_ALL_INDICATOR_UPDATED}
    user clicks button    Save footnote
    user waits until page contains    ${FOOTNOTE_ALL_INDICATOR_UPDATED}
    user checks page does not contain    ${FOOTNOTE_ALL_INDICATOR}

Check footnote was updated on data block
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Edit block    css:tbody > tr:first-child
    user waits until table is visible

    user checks list has x items    testid:footnotes    3
    user checks list item contains    testid:footnotes    1    ${FOOTNOTE_ALL}
    user checks list item contains    testid:footnotes    2    ${FOOTNOTE_ALL_INDICATOR_UPDATED}
    user checks list item contains    testid:footnotes    3    ${FOOTNOTE_ALL_FILTER}

Add public prerelease access list for release
    user clicks link    Pre-release access
    user creates public prerelease access list    Test public access list

Approve release
    user clicks link    Sign off
    user approves release for immediate publication

User goes to public Find Statistics page
    user navigates to find statistics page on public frontend

Verify newly published release is on Find Statistics page
    user waits until page contains accordion section    %{TEST_THEME_NAME}
    user opens accordion section    %{TEST_THEME_NAME}
    user waits until accordion section contains text    %{TEST_THEME_NAME}    ${TOPIC_NAME}

    user opens details dropdown    ${TOPIC_NAME}
    user waits until details dropdown contains publication    ${TOPIC_NAME}    ${PUBLICATION_NAME}
    user checks publication bullet contains link    ${PUBLICATION_NAME}    View statistics and data
    user checks publication bullet contains link    ${PUBLICATION_NAME}    Create your own tables
    user checks publication bullet does not contain link    ${PUBLICATION_NAME}    Statistics at DfE

Navigate to published release page
    user clicks element    testid:View stats link for ${PUBLICATION_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}    90

Check latest release is correct
    user waits until page contains title caption    ${RELEASE_2_NAME}    90
    user checks page contains    This is the latest data
    user checks page contains    See other releases (1)

    user opens details dropdown    See other releases (1)
    user checks page contains other release    ${RELEASE_1_NAME}
    user checks page does not contain other release    ${RELEASE_2_NAME}

    user clicks link    ${RELEASE_1_NAME}

Check other release is correct
    user waits until page contains title caption    ${RELEASE_1_NAME}

    user waits until page contains link    View latest data: ${RELEASE_2_NAME}
    user checks page contains    See other releases (1)
    user checks page contains other release    ${RELEASE_2_NAME}
    user checks page does not contain other release    ${RELEASE_1_NAME}

Go to Table Tool page
    user navigates to data tables page on public frontend

Select publication in table tool
    user opens details dropdown    %{TEST_THEME_NAME}
    user opens details dropdown    ${TOPIC_NAME}
    user clicks radio    ${PUBLICATION_NAME}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    Choose a subject
    user checks previous table tool step contains    1    Publication    ${PUBLICATION_NAME}

Select subject "${SUBJECT_2_NAME}" in table tool
    user clicks link    Create your own table
    user waits until table tool wizard step is available    Choose a subject
    user waits until page contains    ${SUBJECT_2_NAME}
    user clicks radio    ${SUBJECT_2_NAME}
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    Choose locations
    user checks previous table tool step contains    2    Subject    ${SUBJECT_2_NAME}

Select locations in table tool
    user opens details dropdown    Local Authority
    user clicks checkbox    Barnsley
    user clicks checkbox    Birmingham
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    Choose time period
    user checks previous table tool step contains    3    Local Authority    Barnsley
    user checks previous table tool step contains    3    Local Authority    Birmingham

Select time period in table tool
    user chooses select option    id:timePeriodForm-start    2014
    user chooses select option    id:timePeriodForm-end    2018
    user clicks element    id:timePeriodForm-submit

Select indicators and filters in table tool
    user waits until table tool wizard step is available    Choose your filters
    user clicks indicator checkbox    Admission Numbers

    user opens details dropdown    Random Filter
    user clicks category checkbox    Random Filter    Not specified
    user clicks element    id:filtersForm-submit

Validate table
    user waits until results table appears    %{WAIT_LONG}
    user checks table column heading contains    1    1    2014
    user checks table column heading contains    1    2    2015
    user checks table column heading contains    1    3    2016
    user checks table column heading contains    1    4    2017
    user checks table column heading contains    1    5    2018

    ${row}=    user gets row number with heading    Barnsley
    user checks table cell in offset row contains    ${row}    0    1    9,854
    user checks table cell in offset row contains    ${row}    0    2    1,134
    user checks table cell in offset row contains    ${row}    0    3    7,419
    user checks table cell in offset row contains    ${row}    0    4    5,032
    user checks table cell in offset row contains    ${row}    0    5    8,123

    ${row}=    user gets row number with heading    Birmingham
    user checks table cell in offset row contains    ${row}    0    1    3,708
    user checks table cell in offset row contains    ${row}    0    2    9,303
    user checks table cell in offset row contains    ${row}    0    3    8,856
    user checks table cell in offset row contains    ${row}    0    4    8,530
    user checks table cell in offset row contains    ${row}    0    5    3,962

Validate table has footnotes
    user checks list has x items    testid:footnotes    3
    user checks list item contains    testid:footnotes    1    ${FOOTNOTE_ALL}
    user checks list item contains    testid:footnotes    2    ${FOOTNOTE_ALL_INDICATOR_UPDATED}
    user checks list item contains    testid:footnotes    3    ${FOOTNOTE_ALL_FILTER}

Select table featured table from subjects step
    user clicks element    testid:wizardStep-2-goToButton
    user waits until h1 is visible    Go back to previous step
    user clicks button    Confirm

    user waits until table tool wizard step is available    Choose a subject

    user clicks link    Featured tables
    user waits until table tool wizard step is available    Choose a table

    user checks element count is x    css:#featuredTables li    1
    user checks element should contain    css:#featuredTables li:first-child a    Test highlight name
    user checks element should contain    css:#featuredTables li:first-child [id^="highlight-description"]
    ...    Test highlight description

    user clicks link    Test highlight name
    user waits until results table appears    %{WAIT_LONG}
    user waits until page contains element
    ...    xpath://*[@data-testid="dataTableCaption" and text()="Admission Numbers for '${SUBJECT_2_NAME}' for Not specified in Bolton 001 (E02000984), Bolton 001 (E05000364), Bolton 004 (E02000987), Bolton 004 (E05010450), Nailsea Youngwood and Syon between 2005 and 2020"]

Validate table column headings for featured table
    user checks table column heading contains    1    1    Admission Numbers

Validate table rows for featured table
    ${row}=    user gets row number with heading    Bolton 001 (E02000984)
    user checks table heading in offset row contains    ${row}    0    2    2019

    user checks table cell in offset row contains    ${row}    0    1    8,533

    ${row}=    user gets row number with heading    Bolton 001 (E05000364)
    user checks table heading in offset row contains    ${row}    0    2    2009
    user checks table heading in offset row contains    ${row}    1    1    2010
    user checks table heading in offset row contains    ${row}    2    1    2017

    user checks table cell in offset row contains    ${row}    0    1    5,815
    user checks table cell in offset row contains    ${row}    1    1    5,595
    user checks table cell in offset row contains    ${row}    2    1    6,373

    ${row}=    user gets row number with heading    Bolton 004 (E02000987)
    user checks table heading in offset row contains    ${row}    0    2    2020

    user checks table cell in offset row contains    ${row}    0    1    6,031

    ${row}=    user gets row number with heading    Bolton 004 (E05010450)
    user checks table heading in offset row contains    ${row}    0    2    2005
    user checks table heading in offset row contains    ${row}    1    1    2017
    user checks table heading in offset row contains    ${row}    2    1    2018

    user checks table cell in offset row contains    ${row}    0    1    8,557
    user checks table cell in offset row contains    ${row}    1    1    3,481
    user checks table cell in offset row contains    ${row}    2    1    8,630

    ${row}=    user gets row number with heading    Nailsea Youngwood
    user checks table heading in offset row contains    ${row}    0    2    2005
    user checks table heading in offset row contains    ${row}    1    1    2010
    user checks table heading in offset row contains    ${row}    2    1    2011
    user checks table heading in offset row contains    ${row}    3    1    2012
    user checks table heading in offset row contains    ${row}    4    1    2016

    user checks table cell in offset row contains    ${row}    0    1    3,612
    user checks table cell in offset row contains    ${row}    1    1    9,304
    user checks table cell in offset row contains    ${row}    2    1    9,603
    user checks table cell in offset row contains    ${row}    3    1    8,150
    user checks table cell in offset row contains    ${row}    4    1    4,198

    ${row}=    user gets row number with heading    Syon
    user checks table heading in offset row contains    ${row}    0    2    2007
    user checks table heading in offset row contains    ${row}    1    1    2008
    user checks table heading in offset row contains    ${row}    2    1    2010
    user checks table heading in offset row contains    ${row}    3    1    2012
    user checks table heading in offset row contains    ${row}    4    1    2017

    user checks table cell in offset row contains    ${row}    0    1    9,914
    user checks table cell in offset row contains    ${row}    1    1    5,505
    user checks table cell in offset row contains    ${row}    2    1    6,060
    user checks table cell in offset row contains    ${row}    3    1    1,109
    user checks table cell in offset row contains    ${row}    4    1    1,959

Validate featured table has footnotes
    user checks list has x items    testid:footnotes    3
    user checks list item contains    testid:footnotes    1    ${FOOTNOTE_ALL}
    user checks list item contains    testid:footnotes    2    ${FOOTNOTE_ALL_INDICATOR_UPDATED}
    user checks list item contains    testid:footnotes    3    ${FOOTNOTE_ALL_FILTER}

Go to release page
    user opens accordion section    Related information
    user clicks link    ${PUBLICATION_NAME}, ${RELEASE_2_NAME}

    user waits until h1 is visible    ${PUBLICATION_NAME}    90
    user waits until page contains title caption    ${RELEASE_2_NAME}

Go to data guidance document
    user opens accordion section    Explore data and files
    user waits until h3 is visible    Open data
    user clicks link    data files guide

    user waits until page contains title caption    ${RELEASE_2_NAME}
    user waits until h1 is visible    ${PUBLICATION_NAME}
    user waits until h2 is visible    Data guidance

Validate data guidance document file details
    user waits until page contains accordion section    ${SUBJECT_1_NAME}
    user waits until page contains accordion section    ${SUBJECT_2_NAME}
    user checks there are x accordion sections    2

    user opens accordion section    ${SUBJECT_1_NAME}

    ${subject_1_content}=    user gets accordion section content element    ${SUBJECT_1_NAME}
    user checks summary list contains    Filename    tiny-two-filters.csv    ${subject_1_content}
    user checks summary list contains    Geographic levels    National    ${subject_1_content}
    user checks summary list contains    Time period    2017/18    ${subject_1_content}
    user checks summary list contains    Content    UI test subject 1 data guidance content    ${subject_1_content}

    user opens accordion section    ${SUBJECT_2_NAME}

    ${subject_2_content}=    user gets accordion section content element    ${SUBJECT_2_NAME}
    user checks summary list contains    Filename    upload-file-test.csv    ${subject_2_content}
    user checks summary list contains    Geographic levels
    ...    Local Authority; Local Authority District; Local Enterprise Partnership; Opportunity Area; Parliamentary Constituency; RSC Region; Regional; Ward
    ...    ${subject_2_content}
    user checks summary list contains    Time period    2005 to 2020    ${subject_2_content}
    user checks summary list contains    Content    UI test subject 2 data guidance content    ${subject_2_content}

Validate data guidance document variables
    ${subject_1_content}=    user gets accordion section content element    ${SUBJECT_1_NAME}
    user opens details dropdown    Variable names and descriptions    ${subject_1_content}

    ${subject_1_variables}=    get child element    ${subject_1_content}    testid:Variables
    user checks table body has x rows    9    ${subject_1_variables}

    user checks table column heading contains    1    1    Variable name    ${subject_1_variables}
    user checks table column heading contains    1    2    Variable description    ${subject_1_variables}

    user checks results table cell contains    1    1    colour    ${subject_1_variables}
    user checks results table cell contains    1    2    Colour    ${subject_1_variables}

    user checks results table cell contains    2    1    enrolments    ${subject_1_variables}
    user checks results table cell contains    2    2    Number of pupil enrolments    ${subject_1_variables}

    user checks results table cell contains    4    1    enrolments_pa_10_exact_percent    ${subject_1_variables}
    user checks results table cell contains    4    2    Percentage of persistent absentees    ${subject_1_variables}

    user checks results table cell contains    8    1    sess_overall_percent    ${subject_1_variables}
    user checks results table cell contains    8    2    Overall absence rate    ${subject_1_variables}

    user checks results table cell contains    9    1    sess_unauthorised_percent    ${subject_1_variables}
    user checks results table cell contains    9    2    Unauthorised absence rate    ${subject_1_variables}

    ${subject_2_content}=    user gets accordion section content element    ${SUBJECT_2_NAME}
    user opens details dropdown    Variable names and descriptions    ${subject_2_content}

    ${subject_2_variables}=    get child element    ${subject_2_content}    testid:Variables
    user checks table body has x rows    2    ${subject_2_variables}

    user checks table column heading contains    1    1    Variable name    ${subject_2_variables}
    user checks table column heading contains    1    2    Variable description    ${subject_2_variables}

    user checks results table cell contains    1    1    admission_numbers    ${subject_2_variables}
    user checks results table cell contains    1    2    Admission Numbers    ${subject_2_variables}

    user checks results table cell contains    2    1    some_filter    ${subject_2_variables}
    user checks results table cell contains    2    2    Random Filter    ${subject_2_variables}

Validate data guidance document footnotes
    ${subject_1_content}=    user gets accordion section content element    ${SUBJECT_1_NAME}
    user opens details dropdown    Footnotes    ${subject_1_content}

    user checks list has x items    testid:Footnotes    9    ${subject_1_content}
    user checks list item contains    testid:Footnotes    1    ${FOOTNOTE_ALL}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    2    ${FOOTNOTE_ALL_INDICATOR_UPDATED}
    ...    ${subject_1_content}
    user checks list item contains    testid:Footnotes    3    ${FOOTNOTE_ALL_FILTER}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    4    ${FOOTNOTE_SUBJECT_1}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    5    ${FOOTNOTE_SUBJECT_1_INDICATOR}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    6    ${FOOTNOTE_SUBJECT_1_FILTER}    ${subject_1_content}
    user checks list item contains    testid:Footnotes    7    ${FOOTNOTE_SUBJECT_1_FILTER_GROUP}
    ...    ${subject_1_content}
    user checks list item contains    testid:Footnotes    8    ${FOOTNOTE_SUBJECT_1_FILTER_ITEM}
    ...    ${subject_1_content}
    user checks list item contains    testid:Footnotes    9    ${FOOTNOTE_SUBJECT_1_MIXTURE}    ${subject_1_content}

    ${subject_2_content}=    user gets accordion section content element    ${SUBJECT_2_NAME}
    user opens details dropdown    Footnotes    ${subject_2_content}

    user checks list has x items    testid:Footnotes    3    ${subject_2_content}
    user checks list item contains    testid:Footnotes    1    ${FOOTNOTE_ALL}    ${subject_2_content}
    user checks list item contains    testid:Footnotes    2    ${FOOTNOTE_ALL_INDICATOR_UPDATED}
    ...    ${subject_2_content}
    user checks list item contains    testid:Footnotes    3    ${FOOTNOTE_ALL_FILTER}    ${subject_2_content}
