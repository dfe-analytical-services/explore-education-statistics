*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Go to Table Tool page
    [Tags]  HappyPath
    user goes to url  ${url}/table-tool
    user waits until page contains   Create your own tables online

Select "Pupil absence" publication
    [Tags]  HappyPath
    user opens details dropdown    Pupils and schools
    user opens details dropdown    Pupil absence
    user selects radio      Pupil absence in schools in England
    user clicks element    css:#publicationForm-submit
    user waits until page contains    Choose a subject
    user checks previous table tool step contains  1   Publication   Pupil absence in schools in England

Select subject "Absence by characteristic"
    [Tags]  HappyPath
    user selects radio   Absence by characteristic
    user clicks element   css:#publicationSubjectForm-submit
    user waits until page contains    Choose locations
    user checks previous table tool step contains  2    Subject     Absence by characteristic

Select Location Country, England
    [Tags]  HappyPath
    user opens details dropdown     Country
    user clicks checkbox    England
    user clicks element     css:#locationFiltersForm-submit
    user waits until page contains  Choose time period
    user checks previous table tool step contains  3    Country    England

Select Start date and End date
    [Tags]  HappyPath
    user selects start date     2012/13
    user selects end date       2015/16
    user clicks element     css:#timePeriodForm-submit
    user waits until page contains   Choose your filters
    user checks previous table tool step contains  4    Start date    2012/13
    user checks previous table tool step contains  4    End date      2015/16

Select Indicators
    [Tags]  HappyPath
    user clicks indicator checkbox   Absence fields        Authorised absence rate
    user clicks indicator checkbox   Absence fields        Overall absence rate
    user clicks indicator checkbox   Absence fields        Unauthorised absence rate

Select Characteristics
    [Tags]   HappyPath
    user opens details dropdown     Characteristic
    user clicks category checkbox   Characteristic   Gender female
    user clicks category checkbox   Characteristic   Gender male

Select School types
    [Tags]  HappyPath
    user opens details dropdown       School type
    user clicks category checkbox     School type    Total

Select Year breakdown
    [Tags]  HappyPath
    user opens details dropdown     Year breakdown
    user clicks category checkbox   Year breakdown     six half terms

Create table
    [Tags]  HappyPath
    user clicks element     css:#filtersForm-submit
    user waits until results table appears

Validate results table column headings
    [Tags]  HappyPath
    user checks results table column heading contains  1   1   Total

    user checks results table column heading contains  2   1   six half terms

    user checks results table column heading contains  3   1   2012/13
    user checks results table column heading contains  3   2   2013/14
    user checks results table column heading contains  3   3   2014/15
    user checks results table column heading contains  3   4   2015/16

Validate results table row headings
    [Tags]  HappyPath
    user checks results table row heading contains  1    1    England

    user checks results table row heading contains  1    2    Gender female
    user checks results table row heading contains  1    3    Authorised absence rate
    user checks results table row heading contains  2    1    Overall absence rate
    user checks results table row heading contains  3    1    Unauthorised absence rate

    user checks results table row heading contains  4    1    Gender male
    user checks results table row heading contains  4    2    Authorised absence rate
    user checks results table row heading contains  5    1    Overall absence rate
    user checks results table row heading contains  6    1    Unauthorised absence rate

Validate Gender female Authorised absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  1    1     4.2%
    user checks results table cell contains  1    2     3.5%
    user checks results table cell contains  1    3     3.5%
    user checks results table cell contains  1    4     3.4%

Validate Gender female Unauthorised absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  2    1     5.3%
    user checks results table cell contains  2    2     4.5%
    user checks results table cell contains  2    3     4.6%
    user checks results table cell contains  2    4     4.5%

Validate Gender female Overall absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  3    1     1.1%
    user checks results table cell contains  3    2     1.1%
    user checks results table cell contains  3    3     1.1%
    user checks results table cell contains  3    4     1.1%

Validate Gender male Authorised absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  4    1     4.2%
    user checks results table cell contains  4    2     3.4%
    user checks results table cell contains  4    3     3.6%
    user checks results table cell contains  4    4     3.5%

Validate Gender male Unauthorised absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  5    1     5.2%
    user checks results table cell contains  5    2     4.5%
    user checks results table cell contains  5    3     4.6%
    user checks results table cell contains  5    4     4.6%

Validate Gender male Overall absence rate row
    [Tags]  HappyPath
    user checks results table cell contains  6    1     1.1%
    user checks results table cell contains  6    2     1.1%
    user checks results table cell contains  6    3     1.1%
    user checks results table cell contains  6    4     1.1%

Reorder results
    [Tags]  HappyPath   Failing
    user opens details dropdown     Re-order table headers

    user reorders table headers  xpath://legend[text()="Row group 1"]   xpath://legend[text()="Column groups"]  # England
    user waits until page contains element   xpath://legend[text()="Column group 2"]
    user reorders table headers  xpath://legend[text()="Row group 1"]   xpath://legend[text()="Column groups"]  # Gender female, Gender male
    user waits until page contains element   xpath://legend[text()="Column group 3"]
    user reorders table headers  xpath://legend[text()="Column group 3"]    xpath://div[text()="Add groups by dragging them here"]

    user reorders table headers  xpath://strong[text()="Gender male"]/..  xpath://strong[text()="Gender female"]/..
    user reorders table headers  xpath://strong[text()="Authorised absence rate"]   xpath://strong[text()="Unauthorised absence rate"]
    user reorders table headers  xpath://strong[text()="2012/13"]   xpath://strong[text()="2015/16"]
    
    sleep    10

    user clicks element     xpath://button[text()="Re-order table"]

Validate results table column headings after reordering
    [Tags]  HappyPath     Failing
    user checks results table column heading contains  1   1   Gender female
    user checks results table column heading contains  1   2   Gender male

    user checks results table column heading contains  2   1   England
    user checks results table column heading contains  2   2   England

    user checks results table column heading contains  3   1   2013/14
    user checks results table column heading contains  3   2   2014/15
    user checks results table column heading contains  3   3   2015/16
    user checks results table column heading contains  3   4   2012/13

    user checks results table column heading contains  3   5   2013/14
    user checks results table column heading contains  3   6   2014/15
    user checks results table column heading contains  3   7   2015/16
    user checks results table column heading contains  3   8   2012/13

Validate results table row headings after reordering
    [Tags]  HappyPath     Failing
    user checks results table row heading contains  1    1      Total
#    user checks results table row heading contains  1    2      Authorised absence rate
#    user checks results table row heading contains  2    1      Overall absence rate
#    user checks results table row heading contains  3    1      Unauthorised absence rate

Validate Total Overall absence rate row after reordering
    [Tags]  HappyPath    Failing
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
    [Tags]  HappyPath    Failing
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
    [Tags]  HappyPath    Failing
    user checks results table cell contains  5    1     Authorised absence rate

    user checks results table cell contains  5    2     3.5%
    user checks results table cell contains  5    3     3.5%
    user checks results table cell contains  5    4     3.4%
    user checks results table cell contains  5    5     4.2%

    user checks results table cell contains  5    6     3.4%
    user checks results table cell contains  5    7     3.6%
    user checks results table cell contains  5    8     3.5%
    user checks results table cell contains  5    9     4.2%
