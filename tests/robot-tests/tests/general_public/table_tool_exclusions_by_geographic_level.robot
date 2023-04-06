*** Settings ***
Resource            ../libs/public-common.robot

Suite Setup         user opens the browser
Suite Teardown      user closes the browser
Test Setup          fail test fast if required

Force Tags          GeneralPublic    Local    Dev


*** Test Cases ***
Go to Table Tool page
    user navigates to data tables page on public frontend

Select Exclusions publication
    user clicks radio    Pupils and schools
    user clicks radio    Permanent and fixed-period exclusions in England
    user clicks element    id:publicationForm-submit
    user waits until table tool wizard step is available    2    Choose a subject
    user checks previous table tool step contains    1    Publication
    ...    Permanent and fixed-period exclusions in England

Validate "Exclusions by geographic level" subject details
    user opens details dropdown    More details    css:[data-testid="Radio item for Exclusions by geographic level"]
    ${details}=    user gets details content element    More details
    ...    css:[data-testid="Radio item for Exclusions by geographic level"]
    user checks summary list contains    Geographic levels    Local authority; National; Regional    ${details}
    user checks summary list contains    Time period    2006/07 to 2016/17    ${details}

Select subject "Exclusions by geographic level"
    user clicks radio    Exclusions by geographic level
    user clicks element    id:publicationSubjectForm-submit
    user waits until table tool wizard step is available    3    Choose locations
    user checks previous table tool step contains    2    Subject    Exclusions by geographic level

Select all LA Locations
    user opens details dropdown    Local authority
    user clicks button    Select all 156 options
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    Local authority    Barking and Dagenham
    user checks previous table tool step contains    3    Local authority    Barnet
    user checks previous table tool step contains    3    Local authority    Barnsley
    user checks previous table tool step contains    3    Local authority    Bath and North East Somerset
    user checks previous table tool step contains    3    Local authority    Bedford
    user checks previous table tool step contains    3    Local authority    Show 151 more locations

Select all available Time periods
    user chooses select option    id:timePeriodForm-start    2006/07
    user chooses select option    id:timePeriodForm-end    2016/17
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2006/07 to 2016/17

Select Indicator - Number of pupils
    user clicks indicator checkbox    Number of pupils
    user checks indicator checkbox is checked    Number of pupils

Select Indicator - Number of permanent exclusions
    user clicks indicator checkbox    Number of permanent exclusions
    user checks indicator checkbox is checked    Number of permanent exclusions

Select Indicator - Number of fixed period exclusions
    user clicks indicator checkbox    Number of fixed period exclusions
    user checks indicator checkbox is checked    Number of fixed period exclusions

Select Indicator - Number of schools
    user clicks indicator checkbox    Number of schools
    user checks indicator checkbox is checked    Number of schools

Select Indicator - Permanent exclusion rate
    user clicks indicator checkbox    Permanent exclusion rate
    user checks indicator checkbox is checked    Permanent exclusion rate

Select Characteristic School types
    user opens details dropdown    School type
    user clicks category checkbox    School type    State-funded primary
    user clicks category checkbox    School type    State-funded secondary
    user clicks category checkbox    School type    Special

User clicks Create table button
    user clicks element    id:filtersForm-submit

Validate the query could exceed the maximum allowable table size
    user waits until page contains
    ...    Could not create table as the filters chosen may exceed the maximum allowed table size.
    user waits until page contains    Select different filters or download the subject data.
    user waits until page contains button    Download Exclusions by geographic level (csv, 512 B)    %{WAIT_MEDIUM}

Go back to Locations step
    user clicks button    Edit locations
    user waits until table tool wizard step is available    3    Choose locations

Unselect all LA Locations
    user opens details dropdown    Local authority
    user clicks button    Unselect all 156 options
    user checks page contains element
    ...    xpath://*[@class="govuk-error-message" and text()="Select at least one location"]

Select Locations LA, Bury, Sheffield, York
    user clicks checkbox    Bury
    user clicks checkbox    Sheffield
    user clicks checkbox    York
    user clicks element    id:locationFiltersForm-submit
    user waits until table tool wizard step is available    4    Choose time period
    user checks previous table tool step contains    3    Local authority    Bury
    user checks previous table tool step contains    3    Local authority    Sheffield
    user checks previous table tool step contains    3    Local authority    York

Select new start and end date
    user chooses select option    id:timePeriodForm-start    2006/07
    user chooses select option    id:timePeriodForm-end    2008/09
    user clicks element    id:timePeriodForm-submit
    user waits until table tool wizard step is available    5    Choose your filters
    user waits until page contains element    id:filtersForm-indicators
    user checks previous table tool step contains    4    Time period    2006/07 to 2008/09

Select Indicator again - Number of pupils
    user clicks indicator checkbox    Number of pupils
    user checks indicator checkbox is checked    Number of pupils

Select Indicator again - Number of permanent exclusions
    user clicks indicator checkbox    Number of permanent exclusions
    user checks indicator checkbox is checked    Number of permanent exclusions

Select Indicator again - Number of fixed period exclusions
    user clicks indicator checkbox    Number of fixed period exclusions
    user checks indicator checkbox is checked    Number of fixed period exclusions

Select Characteristic School type - State-funded secondary
    user opens details dropdown    School type
    user clicks category checkbox    School type    State-funded secondary

Create table again
    user clicks element    id:filtersForm-submit
    # Extra timeout until EES-234
    user waits until results table appears    %{WAIT_LONG}
    user waits until page contains element
    ...    xpath://*[@data-testid="dataTableCaption" and text()="'Exclusions by geographic level' for State-funded secondary in Bury, Sheffield and York between 2006/07 and 2008/09"]

Validate results table column headings
    user checks table column heading contains    1    1    2006/07
    user checks table column heading contains    1    2    2007/08
    user checks table column heading contains    1    3    2008/09

Validate Bury Number of fixed period exclusions row
    ${row}=    user gets row with group and indicator    Bury    Number of fixed period exclusions
    user checks row contains heading    ${row}    Number of fixed period exclusions
    user checks row cell contains text    ${row}    1    1,539
    user checks row cell contains text    ${row}    2    1,469
    user checks row cell contains text    ${row}    3    1,298

User generates a permanent link
    user waits until page contains button    Generate shareable link    %{WAIT_MEDIUM}
    user clicks button    Generate shareable link
    user waits until page contains testid    permalink-generated-url    %{WAIT_MEDIUM}
    user checks generated permalink is valid

User validates permanent link works correctly
    user clicks link    View share link
    user waits until h1 is visible
    ...    'Exclusions by geographic level' from 'Permanent and fixed-period exclusions in England'

User validates permalink contains correct date
    ${date}=    get current datetime    %-d %B %Y
    user checks page contains element    xpath://*[@data-testid="created-date"]//strong//time[text()="${date}"]

User validates permalink table headers
    user checks table column heading contains    1    1    2006/07
    user checks table column heading contains    1    2    2007/08
    user checks table column heading contains    1    3    2008/09

User validates permalink table rows for Bury
    ${row}=    user gets row with group and indicator    Bury    Number of fixed period exclusions
    user checks row contains heading    ${row}    Number of fixed period exclusions
    user checks row cell contains text    ${row}    1    1,539
    user checks row cell contains text    ${row}    2    1,469
    user checks row cell contains text    ${row}    3    1,298

    ${row}=    user gets row with group and indicator    Bury    Number of permanent exclusions
    user checks row contains heading    ${row}    Number of permanent exclusions
    user checks row cell contains text    ${row}    1    74
    user checks row cell contains text    ${row}    2    75
    user checks row cell contains text    ${row}    3    65

    ${row}=    user gets row with group and indicator    Bury    Number of pupils
    user checks row contains heading    ${row}    Number of pupils
    user checks row cell contains text    ${row}    1    11,618
    user checks row cell contains text    ${row}    2    11,389
    user checks row cell contains text    ${row}    3    11,217

User validates permalink table rows for Sheffield
    ${row}=    user gets row with group and indicator    Sheffield    Number of fixed period exclusions
    user checks row contains heading    ${row}    Number of fixed period exclusions
    user checks row cell contains text    ${row}    1    5,351
    user checks row cell contains text    ${row}    2    3,869
    user checks row cell contains text    ${row}    3    3,374

    ${row}=    user gets row with group and indicator    Sheffield    Number of permanent exclusions
    user checks row contains heading    ${row}    Number of permanent exclusions
    user checks row cell contains text    ${row}    1    12
    user checks row cell contains text    ${row}    2    8
    user checks row cell contains text    ${row}    3    4

    ${row}=    user gets row with group and indicator    Sheffield    Number of pupils
    user checks row contains heading    ${row}    Number of pupils
    user checks row cell contains text    ${row}    1    31,261
    user checks row cell contains text    ${row}    2    31,105
    user checks row cell contains text    ${row}    3    30,948

User validates permalink table rows for York
    ${row}=    user gets row with group and indicator    York    Number of fixed period exclusions
    user checks row contains heading    ${row}    Number of fixed period exclusions
    user checks row cell contains text    ${row}    1    1,073
    user checks row cell contains text    ${row}    2    1,214
    user checks row cell contains text    ${row}    3    892

    ${row}=    user gets row with group and indicator    York    Number of permanent exclusions
    user checks row contains heading    ${row}    Number of permanent exclusions
    user checks row cell contains text    ${row}    1    55
    user checks row cell contains text    ${row}    2    25
    user checks row cell contains text    ${row}    3    3

    ${row}=    user gets row with group and indicator    York    Number of pupils
    user checks row contains heading    ${row}    Number of pupils
    user checks row cell contains text    ${row}    1    10,179
    user checks row cell contains text    ${row}    2    9,955
    user checks row cell contains text    ${row}    3    9,870
