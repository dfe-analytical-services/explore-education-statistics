*** Settings ***
Resource    ../libs/public-common.robot

Force Tags  GeneralPublic  Dev  Test

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
    user opens details dropdown     National
    user clicks checkbox    England
    user clicks element     css:#locationFiltersForm-submit
    user waits until page contains  Choose time period
    user checks previous table tool step contains  3   National     England

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
    [Tags]  HappyPath
    user clicks element  xpath://button[contains(text(), "Choose locations")]
    user waits until page contains element  xpath://h1[text()="Go back to previous step"]
    user clicks element  xpath://button[text()="Confirm"]

Unselect England as a location
    [Documentation]  DFE-1142  EES-231
    [Tags]  HappyPath
    user opens details dropdown     National
    user clicks checkbox            England
    # EES-231
    #user checks page does not contain element  xpath://h2[text()="There is a problem"]
    user closes details dropdown    National

Select locations LAs Barnet, Barnsley, Bedford
    [Tags]  HappyPath
    user opens details dropdown     Local Authority
    user clicks checkbox            Barnet
    user clicks checkbox            Barnsley
    user clicks checkbox            Bedford

    user clicks element   css:#locationFiltersForm-submit
    user waits until page contains  Choose time period
    user checks previous table tool step contains  3    Local Authority    Barnet
    user checks previous table tool step contains  3    Local Authority    Barnsley
    user checks previous table tool step contains  3    Local Authority    Bedford

Select start and end date again
    [Tags]   HappyPath
    user selects start date     2014/15
    user selects end date       2015/16
    user clicks element     css:#timePeriodForm-submit
    user waits until page contains   Choose your filters
    user checks previous table tool step contains  4    Start date    2014/15
    user checks previous table tool step contains  4    End date      2015/16

Select indicators again
    [Tags]   HappyPath
    user clicks indicator checkbox  Absence fields   Number of pupil enrolments
    user clicks indicator checkbox  Absence fields   Number of sessions possible

Create table again
    [Tags]   HappyPath
    user clicks element    css:#filtersForm-submit
    user waits until results table appears

Validate table's column headings
    [Tags]   HappyPath
    user checks results table column heading contains  1   1   Pupil Referral Unit

    user checks results table column heading contains  2   1   2014/15
    user checks results table column heading contains  2   2   2015/16

Validate table's row headings
    [Tags]   HappyPath
    user checks results table row heading contains  1   1   Barnet
    user checks results table row heading contains  1   2   Number of sessions possible
    user checks results table row heading contains  2   1   Number of pupil enrolments

    user checks results table row heading contains  3   1   Barnsley
    user checks results table row heading contains  3   2   Number of sessions possible
    user checks results table row heading contains  4   1   Number of pupil enrolments

    user checks results table row heading contains  5   1   Bedford
    user checks results table row heading contains  5   2   Number of sessions possible
    user checks results table row heading contains  6   1   Number of pupil enrolments

Validate table results
    [Tags]    HappyPath
    # Barnet
    user checks results table cell contains   1     1     38,345
    user checks results table cell contains   1     2     36,820
    user checks results table cell contains   2     1     224
    user checks results table cell contains   2     2     210

    # Barnsley
    user checks results table cell contains   3     1     31,938
    user checks results table cell contains   3     2     36,250
    user checks results table cell contains   4     1     149
    user checks results table cell contains   4     2     146

    # Bedford
    user checks results table cell contains   5     1     17,687
    user checks results table cell contains   5     2     21,847
    user checks results table cell contains   6     1     176
    user checks results table cell contains   6     2     178