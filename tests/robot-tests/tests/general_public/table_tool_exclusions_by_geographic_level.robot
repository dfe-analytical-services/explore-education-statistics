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

Select Exclusions publication
    [Tags]  HappyPath
    user opens details dropdown    Pupils and schools
    user opens details dropdown    Exclusions
    user selects radio      Permanent and fixed-period exclusions in England
    user clicks element    css:#publicationForm-submit
    user waits until element is visible  xpath://h2[text()="Choose a subject"]
    user checks previous table tool step contains  1   Publication   Permanent and fixed-period exclusions in England

Select subject "Exclusions by geographic level"
    [Tags]  HappyPath
    user selects radio   Exclusions by geographic level
    user clicks element   css:#publicationSubjectForm-submit
    user waits until element is visible  xpath://h2[text()="Choose locations"]
    user checks previous table tool step contains  2    Subject     Exclusions by geographic level

Select Locations LA, Bury, Sheffield, York
    [Tags]  HappyPath
    user opens details dropdown     Local Authority
    user clicks checkbox    Bury
    user clicks checkbox    Sheffield
    user clicks checkbox    York
    user clicks element     css:#locationFiltersForm-submit
    # Extra timeout until EES-315/316
    user waits until element is visible  xpath://h2[text()="Choose time period"]   90
    user checks previous table tool step contains  3    Local Authority    Bury
    user checks previous table tool step contains  3    Local Authority    Sheffield
    user checks previous table tool step contains  3    Local Authority    York

Select Start date and End date
    [Tags]  HappyPath
    user selects start date     2006/07
    user selects end date       2008/09
    user clicks element     css:#timePeriodForm-submit
    user waits until element is visible  xpath://h2[text()="Choose your filters"]
    user checks previous table tool step contains  4    Start date    2006/07
    user checks previous table tool step contains  4    End date      2008/09

Select Indicators
    [Tags]  HappyPath
    user clicks indicator checkbox  Number of pupils
    user clicks indicator checkbox  Number of permanent exclusions
    user clicks indicator checkbox  Number of fixed period exclusions

Select Characteristics
    [Tags]   HappyPath
    user opens details dropdown     School type
    user clicks category checkbox   School type   State-funded primary
    user clicks category checkbox   School type   State-funded secondary

Create table
    [Tags]  HappyPath
    user clicks element     css:#filtersForm-submit
    # Extra timeout until EES-234
    user waits until results table appears

Validate results table column headings
    [Tags]  HappyPath
    user checks results table column heading contains  css:table  1   1   State-funded secondary
    user checks results table column heading contains  css:table  1   2   State-funded primary
    user checks results table column heading contains  css:table  2   1   2006/07
    user checks results table column heading contains  css:table  2   2   2007/08
    user checks results table column heading contains  css:table  2   3   2008/09
    user checks results table column heading contains  css:table  2   4   2006/07
    user checks results table column heading contains  css:table  2   5   2007/08
    user checks results table column heading contains  css:table  2   6   2008/09

Validate Bury Number of fixed period exclusions row
    [Tags]  HappyPath
    ${row}=  user gets row with group and indicator   xpath://table  Bury   Number of fixed period exclusions
    user checks row contains heading  ${row}  Number of fixed period exclusions
    user checks row cell contains text  ${row}    1     1,539
    user checks row cell contains text  ${row}    2     1,469
    user checks row cell contains text  ${row}    3     1,298
    user checks row cell contains text  ${row}    4     95
    user checks row cell contains text  ${row}    5     106
    user checks row cell contains text  ${row}    6     111

User generates a permanent link
    [Tags]   HappyPath
    user clicks element    xpath://*[text()="Generate permanent link"]
    user waits until page contains element   xpath://a[text()="View permanent link"]   60
    user checks generated permalink is valid

User validates permanent link works correctly
    [Tags]   HappyPath
    user clicks link   View permanent link
    select window    NEW
    user waits until page contains heading  'Exclusions by geographic level' from 'Permanent and fixed-period exclusions in England'

User validates permalink contains correct date
    ${date}=  get datetime  %d %B %Y
    user checks page contains element   xpath://*[@data-testid="created-date"]//time[text()="${date}"]

User validates permalink table headers
    [Tags]   HappyPath
    user checks results table column heading contains  css:table  1   1   State-funded secondary
    user checks results table column heading contains  css:table  1   2   State-funded primary
    user checks results table column heading contains  css:table  2   1   2006/07
    user checks results table column heading contains  css:table  2   2   2007/08
    user checks results table column heading contains  css:table  2   3   2008/09
    user checks results table column heading contains  css:table  2   4   2006/07
    user checks results table column heading contains  css:table  2   5   2007/08
    user checks results table column heading contains  css:table  2   6   2008/09

User validates permalink table rows for Bury
    ${row}=  user gets row with group and indicator   xpath://table  Bury   Number of fixed period exclusions
    user checks row contains heading  ${row}  Number of fixed period exclusions
    user checks row cell contains text  ${row}    1     1,539
    user checks row cell contains text  ${row}    2     1,469
    user checks row cell contains text  ${row}    3     1,298
    user checks row cell contains text  ${row}    4     95
    user checks row cell contains text  ${row}    5     106
    user checks row cell contains text  ${row}    6     111

    ${row}=  user gets row with group and indicator   xpath://table  Bury   Number of permanent exclusions
    user checks row contains heading  ${row}  Number of permanent exclusions
    user checks row cell contains text  ${row}    1     74
    user checks row cell contains text  ${row}    2     75
    user checks row cell contains text  ${row}    3     65
    user checks row cell contains text  ${row}    4     4
    user checks row cell contains text  ${row}    5     3
    user checks row cell contains text  ${row}    6     x

    ${row}=  user gets row with group and indicator   xpath://table  Bury   Number of pupils
    user checks row contains heading  ${row}  Number of pupils
    user checks row cell contains text  ${row}    1     11,618
    user checks row cell contains text  ${row}    2     11,389
    user checks row cell contains text  ${row}    3     11,217
    user checks row cell contains text  ${row}    4     16,058
    user checks row cell contains text  ${row}    5     15,978
    user checks row cell contains text  ${row}    6     16,088

User validates permalink table rows for Sheffield
    ${row}=  user gets row with group and indicator   xpath://table  Sheffield   Number of fixed period exclusions
    user checks row contains heading  ${row}  Number of fixed period exclusions
    user checks row cell contains text  ${row}    1     5,351
    user checks row cell contains text  ${row}    2     3,869
    user checks row cell contains text  ${row}    3     3,374
    user checks row cell contains text  ${row}    4     539
    user checks row cell contains text  ${row}    5     425
    user checks row cell contains text  ${row}    6     362

    ${row}=  user gets row with group and indicator   xpath://table  Sheffield   Number of permanent exclusions
    user checks row contains heading  ${row}  Number of permanent exclusions
    user checks row cell contains text  ${row}    1     12
    user checks row cell contains text  ${row}    2     8
    user checks row cell contains text  ${row}    3     4
    user checks row cell contains text  ${row}    4     0
    user checks row cell contains text  ${row}    5     x
    user checks row cell contains text  ${row}    6     0

    ${row}=  user gets row with group and indicator   xpath://table  Sheffield   Number of pupils
    user checks row contains heading  ${row}  Number of pupils
    user checks row cell contains text  ${row}    1     31,261
    user checks row cell contains text  ${row}    2     31,105
    user checks row cell contains text  ${row}    3     30,948
    user checks row cell contains text  ${row}    4     42,091
    user checks row cell contains text  ${row}    5     41,843
    user checks row cell contains text  ${row}    6     41,650

User validates permalink table rows for York
    ${row}=  user gets row with group and indicator   xpath://table  York   Number of fixed period exclusions
    user checks row contains heading  ${row}  Number of fixed period exclusions
    user checks row cell contains text  ${row}    1     1,073
    user checks row cell contains text  ${row}    2     1,214
    user checks row cell contains text  ${row}    3     892
    user checks row cell contains text  ${row}    4     190
    user checks row cell contains text  ${row}    5     147
    user checks row cell contains text  ${row}    6     126

    ${row}=  user gets row with group and indicator   xpath://table  York   Number of permanent exclusions
    user checks row contains heading  ${row}  Number of permanent exclusions
    user checks row cell contains text  ${row}    1     55
    user checks row cell contains text  ${row}    2     25
    user checks row cell contains text  ${row}    3     3
    user checks row cell contains text  ${row}    4     3
    user checks row cell contains text  ${row}    5     x
    user checks row cell contains text  ${row}    6     0

    ${row}=  user gets row with group and indicator   xpath://table  York   Number of pupils
    user checks row contains heading  ${row}  Number of pupils
    user checks row cell contains text  ${row}    1     10,179
    user checks row cell contains text  ${row}    2     9,955
    user checks row cell contains text  ${row}    3     9,870
    user checks row cell contains text  ${row}    4     13,145
    user checks row cell contains text  ${row}    5     13,009
    user checks row cell contains text  ${row}    6     12,884


