*** Settings ***
Resource    ../libs/public-common.robot

Force Tags  GeneralPublic  Local  Dev  Test

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Go to Table Tool page
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}/data-tables
    user waits until page contains heading  Create your own tables online

Select "Pupil absence" publication
    [Tags]  HappyPath
    user opens details dropdown    Pupils and schools
    user opens details dropdown    Pupil absence
    user selects radio      Pupil absence in schools in England
    user clicks element    css:#publicationForm-submit
    user waits until element is visible  xpath://h2[text()="Choose a subject"]
    user checks previous table tool step contains  1   Publication   Pupil absence in schools in England

Select subject "Absence by characteristic"
    [Tags]  HappyPath
    user selects radio   Absence by characteristic
    user clicks element   css:#publicationSubjectForm-submit
    user waits until element is visible  xpath://h2[text()="Choose locations"]
    user checks previous table tool step contains  2    Subject     Absence by characteristic

Select Location Country, England
    [Tags]  HappyPath
    user opens details dropdown     National
    user clicks checkbox    England
    user clicks element     css:#locationFiltersForm-submit
    # Extra timeout until EES-315/316
    user waits until element is visible  xpath://h2[text()="Choose time period"]   90
    user checks previous table tool step contains  3    National    England

Select Start date and End date
    [Tags]  HappyPath
    user selects start date     2012/13
    user selects end date       2015/16
    user clicks element     css:#timePeriodForm-submit
    user waits until element is visible  xpath://h2[text()="Choose your filters"]
    user checks previous table tool step contains  4    Start date    2012/13
    user checks previous table tool step contains  4    End date      2015/16

Select Indicators
    [Tags]  HappyPath
    user clicks subheaded indicator checkbox   Absence fields        Authorised absence rate
    user clicks subheaded indicator checkbox   Absence fields        Overall absence rate
    user clicks subheaded indicator checkbox   Absence fields        Unauthorised absence rate

Select Characteristics
    [Tags]   HappyPath
    user opens details dropdown     Characteristic
    user clicks category checkbox   Characteristic   Gender female
    user clicks category checkbox   Characteristic   Gender male

# EES-706
Select School type Total
    [Tags]  HappyPath
    user opens details dropdown   School type
    user clicks category checkbox  School type   Total

Create table
    [Tags]  HappyPath
    user clicks element     css:#filtersForm-submit
    # Extra timeout until EES-234
    user waits until results table appears

Validate results table column headings
    [Tags]  HappyPath
    user checks results table column heading contains  1   1   England
    user checks results table column heading contains  2   1   2012/13
    user checks results table column heading contains  2   2   2013/14
    user checks results table column heading contains  2   3   2014/15
    user checks results table column heading contains  2   4   2015/16

Validate Gender male Authorised absence rate row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Gender male   Authorised absence rate
    user checks row contains heading  ${row}  Authorised absence rate
    user checks row cell contains text  ${row}    1     4.2%
    user checks row cell contains text  ${row}    2     3.4%
    user checks row cell contains text  ${row}    3     3.6%
    user checks row cell contains text  ${row}    4     3.5%

Validate Gender male Overall absence rate row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Gender male   Overall absence rate
    user checks row contains heading  ${row}  Overall absence rate
    user checks row cell contains text  ${row}    1     5.2%
    user checks row cell contains text  ${row}    2     4.5%
    user checks row cell contains text  ${row}    3     4.6%
    user checks row cell contains text  ${row}    4     4.6%

Validate Gender male Unauthorised absence rate row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Gender male   Unauthorised absence rate
    user checks row contains heading  ${row}  Unauthorised absence rate
    user checks row cell contains text  ${row}    1     1.1%
    user checks row cell contains text  ${row}    2     1.1%
    user checks row cell contains text  ${row}    3     1.1%
    user checks row cell contains text  ${row}    4     1.1%

Validate Gender female Authorised absence rate row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Gender female   Authorised absence rate
    user checks row contains heading  ${row}  Authorised absence rate
    user checks row cell contains text  ${row}    1     4.2%
    user checks row cell contains text  ${row}    2     3.5%
    user checks row cell contains text  ${row}    3     3.5%
    user checks row cell contains text  ${row}    4     3.4%

Validate Gender female Overall absence rate row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Gender female   Overall absence rate
    user checks row contains heading  ${row}  Overall absence rate
    user checks row cell contains text  ${row}    1     5.3%
    user checks row cell contains text  ${row}    2     4.5%
    user checks row cell contains text  ${row}    3     4.6%
    user checks row cell contains text  ${row}    4     4.5%

Validate Gender female Unauthorised absence rate row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Gender female   Unauthorised absence rate
    user checks row contains heading  ${row}  Unauthorised absence rate
    user checks row cell contains text  ${row}    1     1.1%
    user checks row cell contains text  ${row}    2     1.1%
    user checks row cell contains text  ${row}    3     1.1%
    user checks row cell contains text  ${row}    4     1.1%

Reorder Gender to be column
    [Tags]  HappyPath
    user opens details dropdown     Re-order table headers
    user sets focus to element  xpath://legend[text()="Row group 1"]/../../..
    user presses keys    SPACE
    user presses keys    ARROW_DOWN
    user presses keys    SPACE

Reorder England to be row
    [Tags]  HappyPath
    user waits until page contains element   xpath://legend[text()="Column group 2"]
    user checks page does not contain element   xpath://legend[text()="Row group 1"]
    user sets focus to element   xpath://legend[text()="Column group 1"]/../../..
    user presses keys    SPACE
    user presses keys    ARROW_UP
    user presses keys    SPACE
    user waits until page contains element   xpath://legend[text()="Row group 1"]
    user checks page does not contain element   xpath://legend[text()="Column group 2"]

Reorder Gender male to be second
    [Tags]  HappyPath
    user sets focus to element   xpath://strong[text()="Gender male"]/../..  # The /../.. to get to a focusable element
    user presses keys    SPACE
    user presses keys    ARROW_DOWN
    user presses keys    SPACE

Reorder Authorised absence rate to be last
    [Tags]  HappyPath
    user sets focus to element  xpath://strong[text()="Authorised absence rate"]/../..  # The /../.. to get to a focusable element
    user presses keys    SPACE
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN  # Three time to ensure
    user presses keys    SPACE

Reorder Overall absence rate to be first
    [Tags]  HappyPath
    user sets focus to element  xpath://strong[text()="Overall absence rate"]/../..  # The /../.. to get to a focusable element
    user presses keys    SPACE
    user presses keys    ARROW_UP
    user presses keys    SPACE

Reorder 2012/13 to be last
    [Tags]  HappyPath
    user sets focus to element  xpath://strong[text()="2012/13"]/../..  # The /../.. to get to a focusable element
    user presses keys    SPACE
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    ARROW_DOWN
    user presses keys    SPACE

Click Re-order table button
    [Tags]  HappyPath
    user clicks element     xpath://button[text()="Re-order table"]

Validate results table column headings after reordering
    [Tags]  HappyPath
    user checks results table column heading contains  1   1   Gender female
    user checks results table column heading contains  1   2   Gender male
    user checks results table column heading contains  2   1   2013/14
    user checks results table column heading contains  2   2   2014/15
    user checks results table column heading contains  2   3   2015/16
    user checks results table column heading contains  2   4   2012/13
    user checks results table column heading contains  2   5   2013/14
    user checks results table column heading contains  2   6   2014/15
    user checks results table column heading contains  2   7   2015/16
    user checks results table column heading contains  2   8   2012/13

Validate results table row headings after reordering
    [Tags]  HappyPath
    user checks results table row heading contains  1    1      England
    user checks results table row heading contains  1    2      Overall absence rate
    user checks results table row heading contains  2    1      Unauthorised absence rate
    user checks results table row heading contains  3    1      Authorised absence rate

Validate rows after reordering
    [Tags]  HappyPath
    # Overall absence rate
    user checks results table cell contains  1    1     4.5%
    user checks results table cell contains  1    2     4.6%
    user checks results table cell contains  1    3     4.5%
    user checks results table cell contains  1    4     5.3%
    user checks results table cell contains  1    5     4.5%
    user checks results table cell contains  1    6     4.6%
    user checks results table cell contains  1    7     4.6%
    user checks results table cell contains  1    8     5.2%

    # Unauthorised absence rate
    user checks results table cell contains  2    1     1.1%
    user checks results table cell contains  2    2     1.1%
    user checks results table cell contains  2    3     1.1%
    user checks results table cell contains  2    4     1.1%
    user checks results table cell contains  2    5     1.1%
    user checks results table cell contains  2    6     1.1%
    user checks results table cell contains  2    7     1.1%
    user checks results table cell contains  2    8     1.1%

    # Authorised absence rate
    user checks results table cell contains  3    1     3.5%
    user checks results table cell contains  3    2     3.5%
    user checks results table cell contains  3    3     3.4%
    user checks results table cell contains  3    4     4.2%
    user checks results table cell contains  3    5     3.4%
    user checks results table cell contains  3    6     3.6%
    user checks results table cell contains  3    7     3.5%
    user checks results table cell contains  3    8     4.2%

User generates a permanent link
    [Tags]   HappyPath
    user clicks element    xpath://*[text()="Generate permanent link"]
    user waits until page contains element   xpath://a[text()="View permanent link"]   60
    user checks generated permalink is valid

User validates permanent link works correctly
    [Tags]   HappyPath
    user clicks link   View permanent link
    select window    NEW
    user waits until page contains heading  'Absence by characteristic' from 'Pupil absence in schools in England'

User validates permalink table
    [Tags]   HappyPath
    user checks results table column heading contains  1   1   England
    user checks results table column heading contains  2   1   2012/13
    user checks results table column heading contains  2   2   2013/14
    user checks results table column heading contains  2   3   2014/15
    user checks results table column heading contains  2   4   2015/16

    ${row}=  user gets row with group and indicator   Gender male   Authorised absence rate
    user checks row contains heading  ${row}  Authorised absence rate
    user checks row cell contains text  ${row}    1     4.2%
    user checks row cell contains text  ${row}    2     3.4%
    user checks row cell contains text  ${row}    3     3.6%
    user checks row cell contains text  ${row}    4     3.5%

    ${row}=  user gets row with group and indicator   Gender male   Overall absence rate
    user checks row contains heading  ${row}  Overall absence rate
    user checks row cell contains text  ${row}    1     5.2%
    user checks row cell contains text  ${row}    2     4.5%
    user checks row cell contains text  ${row}    3     4.6%
    user checks row cell contains text  ${row}    4     4.6%

    ${row}=  user gets row with group and indicator   Gender male   Unauthorised absence rate
    user checks row contains heading  ${row}  Unauthorised absence rate
    user checks row cell contains text  ${row}    1     1.1%
    user checks row cell contains text  ${row}    2     1.1%
    user checks row cell contains text  ${row}    3     1.1%
    user checks row cell contains text  ${row}    4     1.1%

    ${row}=  user gets row with group and indicator   Gender female   Authorised absence rate
    user checks row contains heading  ${row}  Authorised absence rate
    user checks row cell contains text  ${row}    1     4.2%
    user checks row cell contains text  ${row}    2     3.5%
    user checks row cell contains text  ${row}    3     3.5%
    user checks row cell contains text  ${row}    4     3.4%

    ${row}=  user gets row with group and indicator   Gender female   Overall absence rate
    user checks row contains heading  ${row}  Overall absence rate
    user checks row cell contains text  ${row}    1     5.3%
    user checks row cell contains text  ${row}    2     4.5%
    user checks row cell contains text  ${row}    3     4.6%
    user checks row cell contains text  ${row}    4     4.5%

    ${row}=  user gets row with group and indicator   Gender female   Unauthorised absence rate
    user checks row contains heading  ${row}  Unauthorised absence rate
    user checks row cell contains text  ${row}    1     1.1%
    user checks row cell contains text  ${row}    2     1.1%
    user checks row cell contains text  ${row}    3     1.1%
    user checks row cell contains text  ${row}    4     1.1%
