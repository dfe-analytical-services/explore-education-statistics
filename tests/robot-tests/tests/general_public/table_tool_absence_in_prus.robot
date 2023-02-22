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

Validate "Absence in prus" subject details
    user opens details dropdown    More details    css:[data-testid="Radio item for Absence in prus"]
    ${details}=    user gets details content element    More details
    ...    css:[data-testid="Radio item for Absence in prus"]
    user checks summary list contains    Geographic levels    Local authority; National; Regional    ${details}
    user checks summary list contains    Time period    2013/14 to 2016/17    ${details}

Select subject "Absence in prus"
    user clicks radio    Absence in prus
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Subject    Absence in prus

Select Location Country, England
    user opens details dropdown    National
    user clicks checkbox    England
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    National    England

Select Start date and End date
    user chooses select option    id:timePeriodForm-start    2013/14
    user chooses select option    id:timePeriodForm-end    2016/17
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2013/14 to 2016/17

Select Indicators
    user clicks subheaded indicator checkbox    Absence fields    Number of schools
    user checks subheaded indicator checkbox is checked    Absence fields    Number of schools

Create table
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_SMALL}
    user waits until page contains element
    ...    xpath://*[@data-testid="dataTableCaption" and text()="Number of schools for 'Absence in prus' in England between 2013/14 and 2016/17"]

Validate results table column headings
    user checks table column heading contains    1    1    2013/14
    user checks table column heading contains    1    2    2014/15
    user checks table column heading contains    1    3    2015/16
    user checks table column heading contains    1    4    2016/17

Validate results table row headings
    user checks table row heading contains    1    1    Number of schools

Validate Number of schools row results
    user checks table cell contains    1    1    361
    user checks table cell contains    1    2    363
    user checks table cell contains    1    3    350
    user checks table cell contains    1    4    349

Go back to Locations step
    user clicks element    xpath://button[contains(text(), "Edit locations")]
    user waits until page contains element    xpath://h2[text()="Go back to previous step"]
    user clicks element    xpath://button[text()="Confirm"]

Unselect England as a location
    [Documentation]    DFE-1142    EES-231
    user opens details dropdown    National
    user clicks checkbox    England
    user checks page contains element
    ...    xpath://*[@class="govuk-error-message" and text()="Select at least one location"]

    # Workaround to avoid error summary stealing focus when closing dropdown
    user sets focus to element    id:locationFiltersForm-submit
    user waits until h2 is visible    There is a problem

    user closes details dropdown    National

Select locations LAs Barnet, Barnsley, Bedford
    user opens details dropdown    Local authority
    user clicks checkbox    Barnet
    user clicks checkbox    Barnsley
    user clicks checkbox    Bedford

    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    Local authority    Barnet
    user checks previous table tool step contains    3    Local authority    Barnsley
    user checks previous table tool step contains    3    Local authority    Bedford

Select new start and end date
    user chooses select option    id:timePeriodForm-start    2014/15
    user chooses select option    id:timePeriodForm-end    2015/16
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2014/15 to 2015/16

Select indicator Number of pupil enrolments
    user clicks subheaded indicator checkbox    Absence fields    Number of pupil enrolments
    user checks subheaded indicator checkbox is checked    Absence fields    Number of pupil enrolments

Select indicator Number of sessions available
    user clicks subheaded indicator checkbox    Absence fields    Number of sessions possible
    user checks subheaded indicator checkbox is checked    Absence fields    Number of sessions possible

Verify indicator Number of schools is still selected
    user checks subheaded indicator checkbox is checked    Absence fields    Number of schools

Create table again
    user clicks element    id:filtersForm-submit
    user waits until results table appears    %{WAIT_SMALL}
    user waits until page contains element
    ...    xpath://*[@data-testid="dataTableCaption" and text()="'Absence in prus' in Barnet, Barnsley and Bedford between 2014/15 and 2015/16"]

Validate new table column headings
    user checks table column heading contains    1    1    2014/15
    user checks table column heading contains    1    2    2015/16

Validate Barnet Number of pupil enrolments row
    ${row}=    user gets row with group and indicator    Barnet    Number of pupil enrolments
    user checks row contains heading    ${row}    Number of pupil enrolments
    user checks row cell contains text    ${row}    1    224
    user checks row cell contains text    ${row}    2    210

Validate Barnet Number of sessions possible row
    ${row}=    user gets row with group and indicator    Barnet    Number of sessions possible
    user checks row contains heading    ${row}    Number of sessions possible
    user checks row cell contains text    ${row}    1    38,345
    user checks row cell contains text    ${row}    2    36,820

Validate Barnsley Number of pupil enrolments row
    ${row}=    user gets row with group and indicator    Barnsley    Number of pupil enrolments
    user checks row contains heading    ${row}    Number of pupil enrolments
    user checks row cell contains text    ${row}    1    149
    user checks row cell contains text    ${row}    2    146

Validate Barnsley Number of sessions possible row
    ${row}=    user gets row with group and indicator    Barnsley    Number of sessions possible
    user checks row contains heading    ${row}    Number of sessions possible
    user checks row cell contains text    ${row}    1    31,938
    user checks row cell contains text    ${row}    2    36,250

Validate Bedford Number of pupil enrolments row
    ${row}=    user gets row with group and indicator    Bedford    Number of pupil enrolments
    user checks row contains heading    ${row}    Number of pupil enrolments
    user checks row cell contains text    ${row}    1    176
    user checks row cell contains text    ${row}    2    178

Validate Bedford Number of sessions possible row
    ${row}=    user gets row with group and indicator    Bedford    Number of sessions possible
    user checks row contains heading    ${row}    Number of sessions possible
    user checks row cell contains text    ${row}    1    17,687
    user checks row cell contains text    ${row}    2    21,847
