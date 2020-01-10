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
    user checks results table column heading contains  1   1   2012/13
    user checks results table column heading contains  1   2   2013/14
    user checks results table column heading contains  1   3   2014/15
    user checks results table column heading contains  1   4   2015/16

Validate results table row headings
    [Tags]  HappyPath
    user checks results table row heading contains  1    1    Gender
    user checks results table row heading contains  1    2    Gender male
    user checks results table row heading contains  1    3    Authorised absence rate
    user checks results table row heading contains  2    1    Unauthorised absence rate
    user checks results table row heading contains  3    1    Overall absence rate

    user checks results table row heading contains  4    1    Gender female
    user checks results table row heading contains  4    2    Authorised absence rate
    user checks results table row heading contains  5    1    Unauthorised absence rate
    user checks results table row heading contains  6    1    Overall absence rate

Validate Gender male Authorised absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  1    1     4.2%
    user checks results table cell contains  1    2     3.4%
    user checks results table cell contains  1    3     3.6%
    user checks results table cell contains  1    4     3.5%

Validate Gender male Unauthorised absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  2    1     1.1%
    user checks results table cell contains  2    2     1.1%
    user checks results table cell contains  2    3     1.1%
    user checks results table cell contains  2    4     1.1%

Validate Gender male Overall absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  3    1     5.2%
    user checks results table cell contains  3    2     4.5%
    user checks results table cell contains  3    3     4.6%
    user checks results table cell contains  3    4     4.6%

Validate Gender female Authorised absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  4    1     4.2%
    user checks results table cell contains  4    2     3.5%
    user checks results table cell contains  4    3     3.5%
    user checks results table cell contains  4    4     3.4%

Validate Gender female Unauthorised absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  5    1     1.1%
    user checks results table cell contains  5    2     1.1%
    user checks results table cell contains  5    3     1.1%
    user checks results table cell contains  5    4     1.1%

Validate Gender female Overall absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  6    1     5.3%
    user checks results table cell contains  6    2     4.5%
    user checks results table cell contains  6    3     4.6%
    user checks results table cell contains  6    4     4.5%

Reorder results
    [Tags]  HappyPath
    user opens details dropdown     Re-order table headers
    user reorders table headers  xpath://legend[text()="Row group 1"]   xpath://legend[text()="Column groups"]  # England to Column groups

Reorder results 2
    [Tags]  HappyPath
    user waits until page contains element   xpath://legend[text()="Column group 2"]
    user checks page does not contain element   xpath://legend[text()="Row group 1"]
    user reorders table headers  xpath://legend[text()="Column group 1"]   xpath://*[text()="Add groups by dragging them here"]  # Gender female, Gender male to Row groups
    user waits until page contains element   xpath://legend[text()="Row group 1"]
    user checks page does not contain element   xpath://legend[text()="Column group 2"]

Reorder results 3
    [Tags]  HappyPath
    user sets focus to element   //strong[text()="Gender male"]/../..  # The /../.. to get to a focusable element
    press keys    ${NONE}  SPACE
    press keys    ${NONE}  ARROW_DOWN
    press keys    ${NONE}  SPACE

    user sets focus to element  //strong[text()="Authorised absence rate"]/../..  # The /../.. to get to a focusable element
    press keys    ${NONE}  SPACE
    press keys    ${NONE}  ARROW_DOWN
    press keys    ${NONE}  ARROW_DOWN
    sleep  1
    press keys    ${NONE}  SPACE

    user sets focus to element  //strong[text()="2012/13"]/../..  # The /../.. to get to a focusable element
    press keys    ${NONE}  SPACE
    press keys    ${NONE}  ARROW_DOWN
    press keys    ${NONE}  ARROW_DOWN
    press keys    ${NONE}  ARROW_DOWN
    sleep  1
    press keys    ${NONE}  SPACE

    user clicks element     xpath://button[text()="Re-order table"]

Validate results table column headings after reordering
    [Tags]  HappyPath
    user checks results table column heading contains  1   1   2013/14
    user checks results table column heading contains  1   2   2014/15
    user checks results table column heading contains  1   3   2015/16
    user checks results table column heading contains  1   4   2012/13

Validate results table row headings after reordering
    [Tags]  HappyPath
    user checks results table row heading contains  1    1      Gender

    user checks results table row heading contains  1    2      Gender female
    user checks results table row heading contains  1    3      Unauthorised absence rate
    user checks results table row heading contains  2    1      Overall absence rate
    user checks results table row heading contains  3    1      Authorised absence rate

    user checks results table row heading contains  4    1      Gender male
    user checks results table row heading contains  4    2      Unauthorised absence rate
    user checks results table row heading contains  5    1      Overall absence rate
    user checks results table row heading contains  6    1      Authorised absence rate

Validate Total Overall absence rate row after reordering
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  3    1     Total
    user checks results table cell contains  3    2     Overall absence rate

    user checks results table cell contains  3    3     4.5%
    user checks results table cell contains  3    4     4.6%
    user checks results table cell contains  3    5     4.5%
    user checks results table cell contains  3    6     5.3%

    user checks results table cell contains  3    7     4.5%
    user checks results table cell contains  3    8     4.6%
    user checks results table cell contains  3    9     4.6%
    user checks results table cell contains  3    10    5.2%

Validate Total Unauthorised absence rate row after reordering
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  4    1     Unauthorised absence rate

    user checks results table cell contains  4    2     1.1%
    user checks results table cell contains  4    3     1.1%
    user checks results table cell contains  4    4     1.1%
    user checks results table cell contains  4    5     1.1%

    user checks results table cell contains  4    6     1.1%
    user checks results table cell contains  4    7     1.1%
    user checks results table cell contains  4    8     1.1%
    user checks results table cell contains  4    9     1.1%

Validate Total Authorised absence rate row after reordering
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  5    1     Authorised absence rate

    user checks results table cell contains  5    2     3.5%
    user checks results table cell contains  5    3     3.5%
    user checks results table cell contains  5    4     3.4%
    user checks results table cell contains  5    5     4.2%

    user checks results table cell contains  5    6     3.4%
    user checks results table cell contains  5    7     3.6%
    user checks results table cell contains  5    8     3.5%
    user checks results table cell contains  5    9     4.2%

User generates a permanent link
    [Tags]   HappyPath    UnderConstruction
    user clicks element    xpath://*[text()="Generate permanent link"]
    user waits until page contains element   xpath://a[text()="View permanent link"]   60
    user checks generated permalink is valid

User validates permanent link works correctly
    [Tags]   HappyPath    UnderConstruction
    user clicks link   View permanent link
    select window    NEW
    user waits until page contains heading  'Absence by characteristic' from 'Pupil absence in schools in England'

User validates permalink table columns
    [Tags]   HappyPath    UnderConstruction
    user checks results table column heading contains  1   1   2012/13
    user checks results table column heading contains  1   2   2013/14
    user checks results table column heading contains  1   3   2014/15
    user checks results table column heading contains  1   4   2015/16

User validates permalink table rows
    [Tags]   HappyPath    UnderConstruction
    user checks results table row heading contains  1    1    Gender
    user checks results table row heading contains  1    2    Gender male
    user checks results table row heading contains  1    3    Authorised absence rate
    user checks results table row heading contains  2    1    Unauthorised absence rate
    user checks results table row heading contains  3    1    Overall absence rate

    user checks results table row heading contains  4    1    Gender female
    user checks results table row heading contains  4    2    Authorised absence rate
    user checks results table row heading contains  5    1    Unauthorised absence rate
    user checks results table row heading contains  6    1    Overall absence rate

Validate Permalink Gender male Authorised absence rate row
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  1    1     4.2%
    user checks results table cell contains  1    2     3.4%
    user checks results table cell contains  1    3     3.6%
    user checks results table cell contains  1    4     3.5%

Validate Permalink Gender male Unauthorised absence rate row
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  2    1     1.1%
    user checks results table cell contains  2    2     1.1%
    user checks results table cell contains  2    3     1.1%
    user checks results table cell contains  2    4     1.1%

Validate Permalink Gender male Overall absence rate row
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  3    1     5.2%
    user checks results table cell contains  3    2     4.5%
    user checks results table cell contains  3    3     4.6%
    user checks results table cell contains  3    4     4.6%

Validate Permalink Gender female Authorised absence rate row
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  4    1     4.2%
    user checks results table cell contains  4    2     3.5%
    user checks results table cell contains  4    3     3.5%
    user checks results table cell contains  4    4     3.4%

Validate Permalink Gender female Unauthorised absence rate row
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  5    1     1.1%
    user checks results table cell contains  5    2     1.1%
    user checks results table cell contains  5    3     1.1%
    user checks results table cell contains  5    4     1.1%

Validate Permalink Gender female Overall absence rate row
    [Tags]  HappyPath    UnderConstruction
    user checks results table cell contains  6    1     5.3%
    user checks results table cell contains  6    2     4.5%
    user checks results table cell contains  6    3     4.6%
    user checks results table cell contains  6    4     4.5%
