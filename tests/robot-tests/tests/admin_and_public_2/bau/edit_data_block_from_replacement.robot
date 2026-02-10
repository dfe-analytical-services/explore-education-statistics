*** Settings ***
Library             DateTime
Library             String
Library             XML

Resource            ../../libs/admin-common.robot
Resource            ../../libs/table_tool.robot
Resource            ../../libs/admin/manage-content-common.robot

Suite Setup         user signs in as bau1
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          Admin    Local    Dev    AltersData


*** Variables ***
${PUBLICATION_NAME}=    edit data block from replacement %{RUN_IDENTIFIER}
${RELEASE_LABEL}=       test release
${RELEASE_NAME}=        Financial year 3000-01 ${RELEASE_LABEL}
${DATABLOCK_NAME}=      edit data block from replacement data block name
${SUBJECT_NAME}=        edit data block from replacement subject


*** Test Cases ***
Create new publication for "UI tests theme" theme
    ${PUBLICATION_ID}=    user creates test publication via api    ${PUBLICATION_NAME}
    user creates test release via api    ${PUBLICATION_ID}    FY    3000    label=${RELEASE_LABEL}

Go to "Release summary" page
    user navigates to draft release page from dashboard    ${PUBLICATION_NAME}
    ...    ${RELEASE_NAME}

Verify release summary
    user checks page contains element    xpath://li/a[text()="Summary" and contains(@aria-current, 'page')]
    user verifies release summary    Financial year
    ...    3000-01    Accredited official statistics    ${RELEASE_LABEL}

Upload original file and add data guidance
    user uploads subject and waits until complete    ${SUBJECT_NAME}    tiny-one-filter.csv    tiny-one-filter.meta.csv
    user clicks link    Data guidance
    user waits until h2 is visible    Public data guidance
    user adds main data guidance content

    user waits until page contains element    id:dataGuidance-dataFiles
    user waits until page contains accordion section    ${SUBJECT_NAME}

    user checks summary list contains    Filename    tiny-one-filter.csv
    user checks summary list contains    Geographic levels    National
    user checks summary list contains    Time period    2017/18

    user enters text into data guidance data file content editor    ${SUBJECT_NAME}
    ...    ${SUBJECT_NAME} test data guidance content
    user clicks button    Save guidance

Create data block table
    user clicks link    Data blocks
    user waits until h2 is visible    Data blocks

    user clicks link    Create data block
    user waits until h2 is visible    Create data block

    user waits until table tool wizard step is available    1    Select a data set
    user waits until page contains    ${SUBJECT_NAME}
    user clicks radio    ${SUBJECT_NAME}
    user clicks element    id:publicationDataStepForm-submit

    user waits until table tool wizard step is available    2    Choose locations
    user clicks button    National

    user clicks checkbox    England
    user checks location checkbox is checked    England

    user clicks element    id:locationFiltersForm-submit

    user waits until table tool wizard step is available    3    Choose time period
    user chooses select option    id:timePeriodForm-start    2017/18
    user chooses select option    id:timePeriodForm-end    2017/18
    user clicks element    id:timePeriodForm-submit

    user waits until table tool wizard step is available    4    Choose your filters
    user clicks checkbox    Authorised absence rate
    user checks checkbox is checked    Authorised absence rate
    user clicks button    Colour
    user clicks checkbox    Blue
    user checks checkbox is checked    Blue

    user clicks element    id:filtersForm-submit

    user enters text into element    label:Data block name    ${DATABLOCK_NAME}

    user clicks button    Save data block

Navigate to 'Content' page
    user clicks link    Content
    user waits until h2 is visible    ${PUBLICATION_NAME}
    user waits until page contains button    Add a summary text block    %{WAIT_SMALL}

Add summary text block and key free text key statistic
    user adds summary text block
    user adds content to summary text block    Test intro text for ${PUBLICATION_NAME}
    user adds free text key stat    Free text key stat title    9001%    Trend    Guidance title    Guidance text

    user checks element count is x    testid:keyStat    1
    user checks key stat contents    1    Free text key stat title    9001%    Trend
    user checks key stat guidance    1    Guidance title    Guidance text

Approve release for immediate publication
    user approves original release for immediate publication

Return to Admin and create first amendment
    user navigates to admin dashboard    Bau1
    user creates amendment for release    ${PUBLICATION_NAME}    ${RELEASE_NAME}

Upload a replacement data set and verify replacement details and data block error
    user uploads subject replacement    ${SUBJECT_NAME}    tiny-two-filters.csv    tiny-two-filters.meta.csv
    user waits until page contains element    testid:Data file replacements table
    user confirms replacement upload    ${SUBJECT_NAME}    Error
    user clicks link in table cell    1    4    View details    testid:Data file replacements table

    user waits until page contains element    testid:Replacement Title
    user checks table column heading contains    1    1    Original file
    user checks table column heading contains    1    2    Replacement file
    user checks headed table body row cell contains    Data file import status    2    Complete
    user waits until h3 is visible    Data blocks: ERROR

Edit data block from the pending data replacement page
    user opens details dropdown    edit data block from replacement data block name

    user clicks link containing text    Edit data block
    user waits until h2 is visible    Edit data block
    user clicks link    Back to data replacement page
    user waits until h2 is visible    Pending data replacement
