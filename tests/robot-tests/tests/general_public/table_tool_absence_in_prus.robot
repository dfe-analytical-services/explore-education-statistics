*** Settings ***
Resource            ../libs/public-common.robot
Resource            ../seed_data/seed_data_theme_1_constants.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev    Preprod


*** Test Cases ***
Go to Table Tool page
    user navigates to data tables page on public frontend

Select "Seed Data Theme 1 Publication 1" publication
    user clicks radio    ${PUPILS_AND_SCHOOLS_THEME_TITLE}
    user clicks radio    ${PUPIL_ABSENCE_PUBLICATION_TITLE}
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Select a data set
    user checks previous table tool step contains    1    Publication    ${PUPIL_ABSENCE_PUBLICATION_TITLE}

Validate "Absence in PRUs" subject details
    user clicks radio    Absence in PRUs
    user checks summary list contains    Geographic levels    Local authority; National; Regional
    user checks summary list contains    Time period    2013/14 to 2016/17

Select subject "Absence in PRUs"
    user clicks element    id:publicationDataStepForm-submit
    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Data set    Absence in PRUs

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
    ...    xpath://*[@data-testid="dataTableCaption" and text()="Number of schools for 'Absence in PRUs' in England between 2013/14 and 2016/17"]

Validate results table column headings
    user checks table column heading contains    1    1    2016/17
    user checks table column heading contains    1    2    2015/16
    user checks table column heading contains    1    3    2014/15
    user checks table column heading contains    1    4    2013/14

Validate results table row headings
    user checks table row heading contains    1    1    Number of schools

Validate Number of schools row results
    user checks table cell contains    1    1    349
    user checks table cell contains    1    2    350
    user checks table cell contains    1    3    363
    user checks table cell contains    1    4    361

Go back to Locations step
    user clicks button    Edit locations
    user waits until table tool wizard step is available    3    Choose locations

Unselect England as a location
    [Documentation]    DFE-1142    EES-231
    user opens details dropdown    National
    user clicks checkbox    England
    user checks page contains element
    ...    xpath://*[@class="govuk-error-message" and text()="Select at least one location"]

    # Workaround to avoid error summary stealing focus when closing dropdown
    user sets focus to element    id:locationFiltersForm-submit
    user waits until page contains    Select at least one location

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
    ...    xpath://*[@data-testid="dataTableCaption" and text()="'Absence in PRUs' in Barnet, Barnsley and Bedford between 2014/15 and 2015/16"]

Validate new table column headings
    user checks table column heading contains    1    1    2015/16
    user checks table column heading contains    1    2    2014/15

Validate Barnet Number of pupil enrolments row
    ${row}=    user gets row with group and indicator    Barnet    Number of pupil enrolments
    user checks row contains heading    ${row}    Number of pupil enrolments
    user checks row cell contains text    ${row}    1    210
    user checks row cell contains text    ${row}    2    224

Validate Barnet Number of sessions possible row
    ${row}=    user gets row with group and indicator    Barnet    Number of sessions possible
    user checks row contains heading    ${row}    Number of sessions possible
    user checks row cell contains text    ${row}    1    36,820
    user checks row cell contains text    ${row}    2    38,345

Validate Barnsley Number of pupil enrolments row
    ${row}=    user gets row with group and indicator    Barnsley    Number of pupil enrolments
    user checks row contains heading    ${row}    Number of pupil enrolments
    user checks row cell contains text    ${row}    1    146
    user checks row cell contains text    ${row}    2    149

Validate Barnsley Number of sessions possible row
    ${row}=    user gets row with group and indicator    Barnsley    Number of sessions possible
    user checks row contains heading    ${row}    Number of sessions possible
    user checks row cell contains text    ${row}    1    36,250
    user checks row cell contains text    ${row}    2    31,938

Validate Bedford Number of pupil enrolments row
    ${row}=    user gets row with group and indicator    Bedford    Number of pupil enrolments
    user checks row contains heading    ${row}    Number of pupil enrolments
    user checks row cell contains text    ${row}    1    178
    user checks row cell contains text    ${row}    2    176

Validate Bedford Number of sessions possible row
    ${row}=    user gets row with group and indicator    Bedford    Number of sessions possible
    user checks row contains heading    ${row}    Number of sessions possible
    user checks row cell contains text    ${row}    1    21,847
    user checks row cell contains text    ${row}    2    17,687
