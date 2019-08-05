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

Select subject "Absence in prus"
    [Tags]  HappyPath
    user selects radio   Absence in prus
    user clicks element   css:#publicationSubjectForm-submit
    user waits until page contains    Choose locations
    user checks previous table tool step contains  2    Subject     Absence in prus

Select Location Country, England
    [Tags]  HappyPath
    user opens details dropdown     Country
    user clicks checkbox    England
    user clicks element     css:#locationFiltersForm-submit
    user waits until page contains  Choose time period
    user checks previous table tool step contains  3    Country    England

Select Start date and End date
    [Tags]  HappyPath
    user selects start date     2013/14
    user selects end date       2016/17
    user clicks element     css:#timePeriodForm-submit
    user waits until page contains   Choose your filters
    user checks previous table tool step contains  4    Start date    2013/14
    user checks previous table tool step contains  4    End date      2016/17

Select Indicators
    [Tags]  HappyPath
    user clicks indicator checkbox   Absence fields        Number of schools

Create table
    [Tags]  HappyPath
    user clicks element     css:#filtersForm-submit
    user waits until results table appears

Validate results table column headings
    [Tags]  HappyPath
    user checks results table column heading contains  1   1   Pupil Referral Unit

    user checks results table column heading contains  2   1   2013/14
    user checks results table column heading contains  2   2   2014/15
    user checks results table column heading contains  2   3   2015/16
    user checks results table column heading contains  2   4   2016/17

Validate results table row headings
    [Tags]   HappyPath
    user checks results table row heading contains  1     1     England
    user checks results table row heading contains  1     2     Number of schools

Validate Number of schools row results
    [Tags]  HappyPath
    user checks results table cell contains     1     1     361
    user checks results table cell contains     1     2     363
    user checks results table cell contains     1     3     350
    user checks results table cell contains     1     4     349

Go back to Locations step
    [Tags]  HappyPath   UnderConstruction
    user clicks element  xpath://button[contains(text(), "Choose locations")]
    user waits until page contains element  xpath://h1[text()="Go back to previous step"]
    user clicks element  xpath://button[text()="Confirm"]

Unselect England as a location
    [Documentation]  DFE-1142
    [Tags]  HappyPath    UnderConstruction
    user opens details dropdown     Country
    user clicks checkbox            England
    user closes details dropdown    Country

    #user checks page does not contain element   css:#locationFiltersForm-levels-error

Select locations LAs Barnet, Barnsley, Bedford
    [Tags]  HappyPath   UnderConstruction
    user opens details dropdown     Local Authority
    user clicks checkbox            Barnet
    user clicks checkbox            Barnsley
    user clicks checkbox            Bedford

    user clicks element   css:#locationFiltersForm-submit
