*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    Preprod


*** Test Cases ***
Go to Table Tool page
    user navigates to data tables page on public frontend

Select "Pupil absence" publication
    user clicks radio    Pupils and schools
    user clicks radio    Pupil absence in schools in England
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication    Pupil absence in schools in England

Validate "Absence by characteristic" subject details
    user opens details dropdown    More details    css:[data-testid="Radio item for Absence by characteristic"]
    ${details}=    user gets details content element    More details
    ...    css:[data-testid="Radio item for Absence by characteristic"]
    user checks summary list contains    Geographic levels    Local authority; Local authority district; National
    ...    ${details}
    user checks summary list contains    Time period    2012/13 to 2016/17    ${details}

Validate back takes you to step 1
    user goes back
    user waits until table tool wizard step is available    1    Choose a publication
    user clicks element    id:publicationForm-submit

Select subject "Absence by characteristic"
    user clicks radio    Absence by characteristic
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Subject    Absence by characteristic

Validate back takes you to step 2
    user goes back
    user waits until table tool wizard step is available    2    Choose a subject
    user clicks element    id:publicationSubjectForm-submit

Select Location Country, England
    user opens details dropdown    National
    user clicks checkbox    England
    user clicks element    id:locationFiltersForm-submit
    # Extra timeout until EES-315/316
    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    National    England

Select Start date and End date
    user chooses select option    id:timePeriodForm-start    2012/13
    user chooses select option    id:timePeriodForm-end    2015/16
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2012/13 to 2015/16

Select Indicators - Authorised absence rate
    user clicks subheaded indicator checkbox    Absence fields    Authorised absence rate
    user checks subheaded indicator checkbox is checked    Absence fields    Authorised absence rate

Select Indicators - Overall absence rate
    user clicks subheaded indicator checkbox    Absence fields    Overall absence rate
    user checks subheaded indicator checkbox is checked    Absence fields    Overall absence rate

Select Indicators - Unauthorised absence rate
    user clicks subheaded indicator checkbox    Absence fields    Unauthorised absence rate
    user checks subheaded indicator checkbox is checked    Absence fields    Unauthorised absence rate

Select Characteristics
    user opens details dropdown    Characteristic
    user clicks category checkbox    Characteristic    Gender female
    user clicks category checkbox    Characteristic    Gender male

Create table
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_SMALL}

Validate results table column headings
    user checks table column heading contains    1    1    2012/13
    user checks table column heading contains    1    2    2013/14
    user checks table column heading contains    1    3    2014/15
    user checks table column heading contains    1    4    2015/16

Validate Gender male Authorised absence rate row
    ${row}=    user gets row with group and indicator    Gender male    Authorised absence rate
    user checks row contains heading    ${row}    Authorised absence rate
    user checks row cell contains text    ${row}    1    4.2%
    user checks row cell contains text    ${row}    2    3.4%
    user checks row cell contains text    ${row}    3    3.6%
    user checks row cell contains text    ${row}    4    3.5%

Validate Gender male Overall absence rate row
    ${row}=    user gets row with group and indicator    Gender male    Overall absence rate
    user checks row contains heading    ${row}    Overall absence rate
    user checks row cell contains text    ${row}    1    5.2%
    user checks row cell contains text    ${row}    2    4.5%
    user checks row cell contains text    ${row}    3    4.6%
    user checks row cell contains text    ${row}    4    4.6%

Validate Gender male Unauthorised absence rate row
    ${row}=    user gets row with group and indicator    Gender male    Unauthorised absence rate
    user checks row contains heading    ${row}    Unauthorised absence rate
    user checks row cell contains text    ${row}    1    1.1%
    user checks row cell contains text    ${row}    2    1.1%
    user checks row cell contains text    ${row}    3    1.1%
    user checks row cell contains text    ${row}    4    1.1%

Validate Gender female Authorised absence rate row
    ${row}=    user gets row with group and indicator    Gender female    Authorised absence rate
    user checks row contains heading    ${row}    Authorised absence rate
    user checks row cell contains text    ${row}    1    4.2%
    user checks row cell contains text    ${row}    2    3.5%
    user checks row cell contains text    ${row}    3    3.5%
    user checks row cell contains text    ${row}    4    3.4%

Validate Gender female Overall absence rate row
    ${row}=    user gets row with group and indicator    Gender female    Overall absence rate
    user checks row contains heading    ${row}    Overall absence rate
    user checks row cell contains text    ${row}    1    5.3%
    user checks row cell contains text    ${row}    2    4.5%
    user checks row cell contains text    ${row}    3    4.6%
    user checks row cell contains text    ${row}    4    4.5%

Validate Gender female Unauthorised absence rate row
    ${row}=    user gets row with group and indicator    Gender female    Unauthorised absence rate
    user checks row contains heading    ${row}    Unauthorised absence rate
    user checks row cell contains text    ${row}    1    1.1%
    user checks row cell contains text    ${row}    2    1.1%
    user checks row cell contains text    ${row}    3    1.1%
    user checks row cell contains text    ${row}    4    1.1%

Reorder Gender to be column group
    user clicks button    Move and reorder table headers
    # Column group needs to be inside the viewport
    user scrolls to element    xpath://button[text()="Update and view reordered table"]
    user clicks button    Move    testId:rowGroups-0

Move Gender to be first column group
    # The /.. to get to a focusable element
    user sets focus to element    xpath://h4[text()="Characteristic"]/..
    user presses keys    ${SPACE}
    user presses keys    ARROW_LEFT
    user presses keys    ${SPACE}

Reorder Gender male to be first
    user clicks button    Reorder    testId:columnGroups-0
    # The /../.. to get to a focusable element
    user sets focus to element    xpath://*[text()="Gender male"]/../..    testId:columnGroups-0
    user presses keys    ${SPACE}
    user presses keys    ARROW_UP
    user presses keys    ${SPACE}
    user clicks button    Done

Reorder Authorised absence rate to be last
    user clicks button    Reorder    testId:rowGroups-0
    # The /../.. to get to a focusable element
    user sets focus to element    xpath://*[text()="Authorised absence rate"]/../..    testId:rowGroups-0
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN    # Three times to ensure
    user presses keys    ${SPACE}

Reorder Overall absence rate to be first
    # The /../.. to get to a focusable element
    user sets focus to element    xpath://*[text()="Overall absence rate"]/../..    testId:rowGroups-0
    user presses keys    ${SPACE}
    user presses keys    ${SPACE}
    user presses keys    ARROW_UP
    user presses keys    ARROW_UP    # Twice to ensure
    user presses keys    ${SPACE}
    user clicks button    Done

Reorder 2012/13 to be last
    user clicks button    Reorder    testId:columnGroups-1
    # The /../.. to get to a focusable element
    user sets focus to element    xpath://*[text()="2012/13"]/../..    testId:columnGroups-1
    user presses keys    ${SPACE}
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ${SPACE}
    user clicks button    Done

Click Update and view reordered table button
    user clicks button    Update and view reordered table

Validate results table column headings after reordering
    user checks table column heading contains    1    1    Gender male
    user checks table column heading contains    1    2    Gender female
    user checks table column heading contains    2    1    2013/14
    user checks table column heading contains    2    2    2014/15
    user checks table column heading contains    2    3    2015/16
    user checks table column heading contains    2    4    2012/13
    user checks table column heading contains    2    5    2013/14
    user checks table column heading contains    2    6    2014/15
    user checks table column heading contains    2    7    2015/16
    user checks table column heading contains    2    8    2012/13

Validate results table row headings after reordering
    user checks table row heading contains    1    1    Overall absence rate
    user checks table row heading contains    2    1    Unauthorised absence rate
    user checks table row heading contains    3    1    Authorised absence rate

Validate rows after reordering
    # Overall absence rate
    user checks table cell contains    1    1    4.5%
    user checks table cell contains    1    2    4.6%
    user checks table cell contains    1    3    4.6%
    user checks table cell contains    1    4    5.2%
    user checks table cell contains    1    5    4.5%
    user checks table cell contains    1    6    4.6%
    user checks table cell contains    1    7    4.5%
    user checks table cell contains    1    8    5.3%

    # Unauthorised absence rate
    user checks table cell contains    2    1    1.1%
    user checks table cell contains    2    2    1.1%
    user checks table cell contains    2    3    1.1%
    user checks table cell contains    2    4    1.1%
    user checks table cell contains    2    5    1.1%
    user checks table cell contains    2    6    1.1%
    user checks table cell contains    2    7    1.1%
    user checks table cell contains    2    8    1.1%

    # Authorised absence rate
    user checks table cell contains    3    1    3.4%
    user checks table cell contains    3    2    3.6%
    user checks table cell contains    3    3    3.5%
    user checks table cell contains    3    4    4.2%
    user checks table cell contains    3    5    3.5%
    user checks table cell contains    3    6    3.5%
    user checks table cell contains    3    7    3.4%
    user checks table cell contains    3    8    4.2%

User generates a permanent link
    user waits until page contains button    Generate shareable link
    user clicks button    Generate shareable link
    user waits until page contains link    View share link
    user checks generated permalink is valid

User validates permanent link works correctly
    [Documentation]    EES-2892
    user clicks link    View share link
    user waits until h1 is visible    'Absence by characteristic' from 'Pupil absence in schools in England'

User validates permalink table
    user checks table column heading contains    1    1    Gender male
    user checks table column heading contains    1    2    Gender female
    user checks table column heading contains    2    1    2013/14
    user checks table column heading contains    2    2    2014/15
    user checks table column heading contains    2    3    2015/16
    user checks table column heading contains    2    4    2012/13
    user checks table column heading contains    2    5    2013/14
    user checks table column heading contains    2    6    2014/15
    user checks table column heading contains    2    7    2015/16
    user checks table column heading contains    2    8    2012/13

    user checks table row heading contains    1    1    Overall absence rate
    user checks table row heading contains    2    1    Unauthorised absence rate
    user checks table row heading contains    3    1    Authorised absence rate

    # Overall absence rate
    user checks table cell contains    1    1    4.5%
    user checks table cell contains    1    2    4.6%
    user checks table cell contains    1    3    4.6%
    user checks table cell contains    1    4    5.2%
    user checks table cell contains    1    5    4.5%
    user checks table cell contains    1    6    4.6%
    user checks table cell contains    1    7    4.5%
    user checks table cell contains    1    8    5.3%

    # Unauthorised absence rate
    user checks table cell contains    2    1    1.1%
    user checks table cell contains    2    2    1.1%
    user checks table cell contains    2    3    1.1%
    user checks table cell contains    2    4    1.1%
    user checks table cell contains    2    5    1.1%
    user checks table cell contains    2    6    1.1%
    user checks table cell contains    2    7    1.1%
    user checks table cell contains    2    8    1.1%

    # Authorised absence rate
    user checks table cell contains    3    1    3.4%
    user checks table cell contains    3    2    3.6%
    user checks table cell contains    3    3    3.5%
    user checks table cell contains    3    4    4.2%
    user checks table cell contains    3    5    3.5%
    user checks table cell contains    3    6    3.5%
    user checks table cell contains    3    7    3.4%
    user checks table cell contains    3    8    4.2%
