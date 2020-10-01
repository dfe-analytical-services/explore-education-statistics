*** Settings ***
Resource    ../libs/public-common.robot

Force Tags  GeneralPublic  Local  Dev  Test  Preprod

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Go to Table Tool page
    [Tags]  HappyPath
    environment variable should be set  PUBLIC_URL
    user goes to url  %{PUBLIC_URL}/data-tables
    user waits until h1 is visible  Create your own tables online
    user waits for page to finish loading

Select "Pupil absence" publication
    [Tags]  HappyPath
    user opens details dropdown    Pupils and schools
    user opens details dropdown    Pupil absence
    user clicks radio      Pupil absence in schools in England
    user clicks element    css:#publicationForm-submit
    user waits until h2 is visible  Choose a subject
    user checks previous table tool step contains  1   Publication   Pupil absence in schools in England

Select subject "Absence in prus"
    [Tags]  HappyPath
    user clicks radio   Absence in prus
    user clicks element   css:#publicationSubjectForm-submit
    user waits until h2 is visible  Choose locations
    user checks previous table tool step contains  2    Subject     Absence in prus

Select Location Country, England
    [Tags]  HappyPath
    user opens details dropdown     National
    user clicks checkbox    England
    user clicks element     css:#locationFiltersForm-submit
    user waits until h2 is visible  Choose time period
    user checks previous table tool step contains  3   National     England

Select Start date and End date
    [Tags]  HappyPath
    user selects from list by label  id:timePeriodForm-start   2013/14
    user selects from list by label  id:timePeriodForm-end     2016/17
    user clicks element     css:#timePeriodForm-submit
    user waits until h2 is visible  Choose your filters
    user waits until page contains element   id:filtersForm-indicators
    user checks previous table tool step contains  4    Start date    2013/14
    user checks previous table tool step contains  4    End date      2016/17

Select Indicators
    [Tags]  HappyPath
    user clicks subheaded indicator checkbox   Absence fields        Number of schools
    user checks subheaded indicator checkbox is checked   Absence fields        Number of schools

Create table
    [Tags]  HappyPath
    user clicks element     css:#filtersForm-submit
    user waits until results table appears     60
    user waits until page contains element   xpath://*[@id="dataTableCaption" and text()="Table showing Number of schools for 'Absence in prus' from 'Pupil absence in schools in England' in England between 2013/14 and 2016/17"]

Validate results table column headings
    [Tags]  HappyPath
    user checks table column heading contains  1   1   2013/14
    user checks table column heading contains  1   2   2014/15
    user checks table column heading contains  1   3   2015/16
    user checks table column heading contains  1   4   2016/17

Validate results table row headings
    [Tags]   HappyPath
    user checks results table row heading contains  1     1     Number of schools

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
    user checks page contains element  xpath://*[@class="govuk-error-message" and text()="Select at least one location"]
    user closes details dropdown    National

Select locations LAs Barnet, Barnsley, Bedford
    [Tags]  HappyPath
    user opens details dropdown     Local Authority
    user clicks checkbox            Barnet
    user clicks checkbox            Barnsley
    user clicks checkbox            Bedford

    user clicks element   css:#locationFiltersForm-submit
    user waits until h2 is visible  Choose time period
    user checks previous table tool step contains  3    Local Authority    Barnet
    user checks previous table tool step contains  3    Local Authority    Barnsley
    user checks previous table tool step contains  3    Local Authority    Bedford

Select new start and end date
    [Tags]   HappyPath
    user selects from list by label  id:timePeriodForm-start   2014/15
    user selects from list by label  id:timePeriodForm-end     2015/16
    user clicks element     css:#timePeriodForm-submit
    user waits until h2 is visible  Choose your filters
    user waits until page contains element   id:filtersForm-indicators
    user checks previous table tool step contains  4    Start date    2014/15
    user checks previous table tool step contains  4    End date      2015/16

Select indicator Number of pupil enrolments
    [Tags]   HappyPath
    user clicks subheaded indicator checkbox  Absence fields   Number of pupil enrolments
    user checks subheaded indicator checkbox is checked  Absence fields  Number of pupil enrolments

Select indicator Number of sessions available
    [Tags]   HappyPath
    user clicks subheaded indicator checkbox  Absence fields   Number of sessions possible
    user checks subheaded indicator checkbox is checked  Absence fields  Number of sessions possible

Verify indicator Number of schools is still selected
    [Tags]   HappyPath
    user checks subheaded indicator checkbox is checked  Absence fields  Number of schools

Create table again
    [Tags]   HappyPath
    user clicks element    css:#filtersForm-submit
    user waits until results table appears    60
    user waits until page contains element   xpath://*[@id="dataTableCaption" and text()="Table showing 'Absence in prus' from 'Pupil absence in schools in England' in Barnet, Barnsley and Bedford between 2014/15 and 2015/16"]

Validate new table column headings
    [Tags]   HappyPath
    user checks table column heading contains  1   1   2014/15
    user checks table column heading contains  1   2   2015/16

Validate Barnet Number of pupil enrolments row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Barnet   Number of pupil enrolments
    user checks row contains heading  ${row}  Number of pupil enrolments
    user checks row cell contains text  ${row}    1     224
    user checks row cell contains text  ${row}    2     210

Validate Barnet Number of sessions possible row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Barnet   Number of sessions possible
    user checks row contains heading  ${row}  Number of sessions possible
    user checks row cell contains text  ${row}    1     38,345
    user checks row cell contains text  ${row}    2     36,820

Validate Barnsley Number of pupil enrolments row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Barnsley   Number of pupil enrolments
    user checks row contains heading  ${row}  Number of pupil enrolments
    user checks row cell contains text  ${row}    1     149
    user checks row cell contains text  ${row}    2     146

Validate Barnsley Number of sessions possible row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Barnsley   Number of sessions possible
    user checks row contains heading  ${row}  Number of sessions possible
    user checks row cell contains text  ${row}    1     31,938
    user checks row cell contains text  ${row}    2     36,250

Validate Bedford Number of pupil enrolments row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Bedford   Number of pupil enrolments
    user checks row contains heading  ${row}  Number of pupil enrolments
    user checks row cell contains text  ${row}    1     176
    user checks row cell contains text  ${row}    2     178

Validate Bedford Number of sessions possible row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   Bedford   Number of sessions possible
    user checks row contains heading  ${row}  Number of sessions possible
    user checks row cell contains text  ${row}    1     17,687
    user checks row cell contains text  ${row}    2     21,847
