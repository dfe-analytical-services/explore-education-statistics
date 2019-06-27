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
    user clicks element    css:[data-testid="Pupil absence in schools in England"]
    user clicks element    css:#publicationForm-submit
    user waits until page contains    Choose a subject

Select "Geographic levels" area of interest
    [Tags]  HappyPath   UnderConstruction
    user clicks element   css:[data-testid="Absence by geographic level"]
    user clicks element   css:#publicationSubjectForm-submit
    user waits until page contains    Choose locations

Select Location "National"
    [Tags]  HappyPath  UnderConstruction
    user clicks element   css:#filter-locationLevel-national

Select start and end years
    [Tags]  HappyPath  UnderConstruction
    user selects from list by label   css:#filter-timePeriodStart   2012/13
    user selects from list by label   css:#filter-timePeriodEnd   2015/16

Select a few Characteristics
    [Tags]  HappyPath   UnderConstruction
    user clicks element child containing text   css:#filter-characteristics     Total
    user clicks element   css:[id="categoricalFilters.characteristics-total-Total"]

Select a few School types
    [Tags]  HappyPath   UnderConstruction
    user clicks element     css:#schoolTypes-State_Funded_Primary
    user clicks element     css:#schoolTypes-State_Funded_Secondary

Select a few Indicators
    [Tags]  HappyPath   UnderConstruction
    user clicks element child containing text  css:#filter-indicators    Absence fields
    user clicks element     css:#indicators-absenceFields-sess_authorised_percent
    user clicks element     css:#indicators-absenceFields-sess_unauthorised_percent
    user clicks element     css:#indicators-absenceFields-sess_overall_percent

Create table
    [Tags]  HappyPath   UnderConstruction
    user clicks element     css:#submit-button
    user waits until page contains  4. Explore data for 'Pupil absence'

Validate results table column headings
    [Tags]  HappyPath   UnderConstruction
    table cell should contain   css:table:nth-child(4)  1   1   ${EMPTY}
    table cell should contain   css:table:nth-child(4)  1   2   Primary schools
    table cell should contain   css:table:nth-child(4)  1   3   Secondary school
    table cell should contain   css:table:nth-child(4)  2   1   ${EMPTY}
    table cell should contain   css:table:nth-child(4)  2   2   2012/13
    table cell should contain   css:table:nth-child(4)  2   3   2013/14
    table cell should contain   css:table:nth-child(4)  2   4   2014/15
    table cell should contain   css:table:nth-child(4)  2   5   2015/16
    table cell should contain   css:table:nth-child(4)  2   6   2012/13
    table cell should contain   css:table:nth-child(4)  2   7   2013/14
    table cell should contain   css:table:nth-child(4)  2   8   2014/15
    table cell should contain   css:table:nth-child(4)  2   9   2015/16

Validate Authorised absence rate row
    [Tags]  HappyPath   UnderConstruction
    table cell should contain    css:table:nth-child(4)  3    1     All pupils
    table cell should contain    css:table:nth-child(4)  3    2     Authorised absence rate
    table cell should contain    css:table:nth-child(4)  3    3     3.9%
    table cell should contain    css:table:nth-child(4)  3    4     3%
    table cell should contain    css:table:nth-child(4)  3    5     3.1%
    table cell should contain    css:table:nth-child(4)  3    6     3.1%
    table cell should contain    css:table:nth-child(4)  3    7     4.5%
    table cell should contain    css:table:nth-child(4)  3    8     3.9%
    table cell should contain    css:table:nth-child(4)  3    9     4%
    table cell should contain    css:table:nth-child(4)  3    10    3.8%

Validate Unauthorised absence rate row
    [Tags]  HappyPath   UnderConstruction
    table cell should contain    css:table:nth-child(4)  4    1     Unauthorised absence rate
    table cell should contain    css:table:nth-child(4)  4    2     0.8%
    table cell should contain    css:table:nth-child(4)  4    3     0.8%
    table cell should contain    css:table:nth-child(4)  4    4     0.9%
    table cell should contain    css:table:nth-child(4)  4    5     0.9%
    table cell should contain    css:table:nth-child(4)  4    6     1.4%
    table cell should contain    css:table:nth-child(4)  4    7     1.3%
    table cell should contain    css:table:nth-child(4)  4    8     1.3%
    table cell should contain    css:table:nth-child(4)  4    9     1.4%

Validate Overall absence rate row
    [Tags]  HappyPath   UnderConstruction
    table cell should contain    css:table:nth-child(4)  5    1     Overall absence rate
    table cell should contain    css:table:nth-child(4)  5    2     4.7%
    table cell should contain    css:table:nth-child(4)  5    3     3.9%
    table cell should contain    css:table:nth-child(4)  5    4     4%
    table cell should contain    css:table:nth-child(4)  5    5     4%
    table cell should contain    css:table:nth-child(4)  5    6     5.9%
    table cell should contain    css:table:nth-child(4)  5    7     5.2%
    table cell should contain    css:table:nth-child(4)  5    8     5.3%
    table cell should contain    css:table:nth-child(4)  5    9     5.2%

Reorder results table
    [Tags]  HappyPath   UnderConstruction
    user clicks element containing text     Re-order table headers
    user waits until page contains element   css:[data-testid="Re-order table headers"][open]
    user drags and drops   css:#sort-columnGroups > div > div:nth-child(1) strong  css:#sort-columnGroups > div > div:nth-child(2) strong
    user drags and drops   css:#sort-rows > div > div:nth-child(3) strong  css:#sort-rows > div > div:nth-child(1) strong
    user waits until element contains  css:#sort-rows > div > div:nth-child(1)  Overall absence rate
    user clicks element child containing text  css:form:nth-child(2)  Re-order table

Validate results table headings have reordered
    [Tags]  HappyPath   UnderConstruction
    table cell should contain   css:table:nth-child(4)  1   1   ${EMPTY}
    table cell should contain   css:table:nth-child(4)  1   2   Secondary schools
    table cell should contain   css:table:nth-child(4)  1   3   Primary school
    table cell should contain   css:table:nth-child(4)  2   1   ${EMPTY}
    table cell should contain   css:table:nth-child(4)  2   2   2012/13
    table cell should contain   css:table:nth-child(4)  2   3   2013/14
    table cell should contain   css:table:nth-child(4)  2   4   2014/15
    table cell should contain   css:table:nth-child(4)  2   5   2015/16
    table cell should contain   css:table:nth-child(4)  2   6   2012/13
    table cell should contain   css:table:nth-child(4)  2   7   2013/14
    table cell should contain   css:table:nth-child(4)  2   8   2014/15
    table cell should contain   css:table:nth-child(4)  2   9   2015/16

Validate Overall absence rate row has reordered
    [Tags]  HappyPath   UnderConstruction
    table cell should contain    css:table:nth-child(4)  3    1     All pupils
    table cell should contain    css:table:nth-child(4)  3    2     Overall absence rate
    table cell should contain    css:table:nth-child(4)  3    3     5.9%
    table cell should contain    css:table:nth-child(4)  3    4     5.2%
    table cell should contain    css:table:nth-child(4)  3    5     5.3%
    table cell should contain    css:table:nth-child(4)  3    6     5.2%
    table cell should contain    css:table:nth-child(4)  3    7     4.7%
    table cell should contain    css:table:nth-child(4)  3    8     3.9%
    table cell should contain    css:table:nth-child(4)  3    9     4%
    table cell should contain    css:table:nth-child(4)  3    10    4%

Validate Authorised absence rate row has reordered
    [Tags]  HappyPath   UnderConstruction
    table cell should contain    css:table:nth-child(4)  4    1     Authorised absence rate
    table cell should contain    css:table:nth-child(4)  4    2     4.5%
    table cell should contain    css:table:nth-child(4)  4    3     3.9%
    table cell should contain    css:table:nth-child(4)  4    4     4%
    table cell should contain    css:table:nth-child(4)  4    5     3.8%
    table cell should contain    css:table:nth-child(4)  4    6     3.9%
    table cell should contain    css:table:nth-child(4)  4    7     3%
    table cell should contain    css:table:nth-child(4)  4    8     3.1%
    table cell should contain    css:table:nth-child(4)  4    9     3.1%

Validate Unauthorised absence rate row has reordered
    [Tags]  HappyPath   UnderConstruction
    table cell should contain    css:table:nth-child(4)  5    1     Unauthorised absence rate
    table cell should contain    css:table:nth-child(4)  5    2     1.4%
    table cell should contain    css:table:nth-child(4)  5    3     1.3%
    table cell should contain    css:table:nth-child(4)  5    4     1.3%
    table cell should contain    css:table:nth-child(4)  5    5     1.4%
    table cell should contain    css:table:nth-child(4)  5    6     0.8%
    table cell should contain    css:table:nth-child(4)  5    7     0.8%
    table cell should contain    css:table:nth-child(4)  5    8     0.9%
    table cell should contain    css:table:nth-child(4)  5    9     0.9%

