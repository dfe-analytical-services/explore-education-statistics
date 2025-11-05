*** Settings ***
Resource            ../../libs/admin/manage-content-common.robot
Resource            ../../libs/public-common.robot

Force Tags          Admin    PublicApi    Local    Dev    AltersData

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required
Test Teardown       Run Keyword If Test Failed    record test failure


*** Variables ***
${PUBLICATION_NAME}=    Filter groups ignored by table tool %{RUN_IDENTIFIER}
${RELEASE_1_NAME}=      Financial year 3000-01
${SUBJECT_1_NAME}=      ${PUBLICATION_NAME} - Subject 1


*** Test Cases ***
Create publication and release
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_1_NAME}

Upload subject
    user uploads subject and waits until complete    ${SUBJECT_1_NAME}    filter_hierarchy_student_enrolment.csv
    ...    filter_hierarchy_student_enrolment.meta.csv

Add data guidance to subjects and verify the expected variables are there
    user clicks link    Data and files
    user waits until h2 is visible    Add data file to release

    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance

    user waits until page contains element    id:dataGuidance-dataFiles

    user opens details dropdown    Variable names and descriptions

    user checks table body has x rows    2    css:table[data-testid="Variables"]
    user checks table column heading contains    1    1    Variable name    css:table[data-testid="Variables"]
    user checks table column heading contains    1    2    Variable description    css:table[data-testid="Variables"]

    user checks table cell contains    1    1    course_title    id:dataGuidance-dataFiles
    user checks table cell contains    1    2    Name of course being studied    id:dataGuidance-dataFiles
    user checks table cell contains    2    1    enrollment_count    id:dataGuidance-dataFiles
    user checks table cell contains    2    2    Number of students enrolled    id:dataGuidance-dataFiles

    user enters text into element    id:dataGuidanceForm-content    Test metadata guidance content
    user enters text into element    id:dataGuidanceForm-dataSets-0-content    Test file guidance content

    user waits until page contains accordion section    ${SUBJECT_1_NAME}

    user enters text into data guidance data file content editor    ${SUBJECT_1_NAME}
    ...    ${SUBJECT_1_NAME} Main guidance content

    user clicks button    Save guidance

Add headline text block to Content page
    user navigates to content page    ${PUBLICATION_NAME}
    user adds headlines text block
    user adds content to headlines text block    Headline text block text

Start creating a data block
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks
    user waits until page contains    No data blocks have been created.

    user clicks link    Create data block
    user waits until table tool wizard step is available    1    Select a data set

Select subject for data block
    user waits until page contains    ${SUBJECT_1_NAME}    %{WAIT_SMALL}
    user clicks radio    ${SUBJECT_1_NAME}
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    2    Choose locations    %{WAIT_MEDIUM}
    user checks previous table tool step contains    1    Data set    ${SUBJECT_1_NAME}

Select locations
    user clicks element    testid:Expand Details Section Local authority
    user clicks checkbox    Barnsley
    user clicks checkbox    Birmingham

    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    3    Choose time period    %{WAIT_MEDIUM}

Select time period
    user waits until page contains element    id:timePeriodForm-start

    user chooses select option    id:timePeriodForm-start    2018
    user chooses select option    id:timePeriodForm-end    2018
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    4    Choose your filters

Verify that only one filter is shown (Filter groups ignored by table tool)
    @{filter_item_details_sections}=    Get WebElements    //*[starts-with(@data-testid, 'Expand Details Section')]
    Length Should Be    ${filter_item_details_sections}    1

Approve first release
    user approves release for immediate publication

Get public release link
    ${PUBLIC_RELEASE_LINK}=    user gets url public release will be accessible at
    Set Suite Variable    ${PUBLIC_RELEASE_LINK}

Verify newly published release is public
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_1_NAME}
    user waits until page finishes loading
    user clicks link    Data guidance
    user waits until h1 is visible    ${PUBLICATION_NAME}

Verify the variables and descriptions only contain one filter.
    user clicks button    Show all sections
    ${subject_1_content}=    user gets accordion section content element    ${SUBJECT_1_NAME}
    ...    testid:accordionSection
    user opens details dropdown    Variable names and descriptions    ${subject_1_content}

    ${subject_1_variables}=    get child element    ${subject_1_content}    testid:Variables
    user checks table body has x rows    2    ${subject_1_variables}

    user checks table column heading contains    1    1    Variable name    ${subject_1_variables}
    user checks table column heading contains    1    2    Variable description    ${subject_1_variables}

    user checks table cell contains    1    1    course_title    ${subject_1_variables}
    user checks table cell contains    1    2    Name of course being studied    ${subject_1_variables}
    # Below is an indicator not a filter
    user checks table cell contains    2    1    enrollment_count    ${subject_1_variables}
    user checks table cell contains    2    2    Number of students enrolled    ${subject_1_variables}

Create a new table using the table tool
    user navigates to public release page    ${PUBLIC_RELEASE_LINK}    ${PUBLICATION_NAME}    ${RELEASE_1_NAME}
    user waits until page finishes loading
    user clicks link    View or create your own tables
    user waits until h1 is visible    Create your own tables

    user waits until table tool wizard step is available    2    Select a data set
    user clicks radio    ${SUBJECT_1_NAME}
    user clicks element    id:publicationDataStepForm-submit

    user waits until table tool wizard step is available    3    Choose locations
    user clicks element    testid:Expand Details Section Local authority
    user clicks checkbox    Barnsley
    user clicks checkbox    Birmingham
    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    4    Choose time period    %{WAIT_MEDIUM}
    user waits until page contains element    id:timePeriodForm-start
    user chooses select option    id:timePeriodForm-start    2018
    user chooses select option    id:timePeriodForm-end    2018
    user clicks element    id:timePeriodForm-submit

Verify filter used in filter_grouping_column is ignored in the table tool
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until element is visible
    ...    xpath://*[@data-testid="Expand Details Section Name of course being studied"]
    @{filter_item_details_sections}=    Get WebElements    //*[starts-with(@data-testid, 'Expand Details Section')]
    Length Should Be    ${filter_item_details_sections}    1
