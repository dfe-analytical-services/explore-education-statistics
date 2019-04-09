*** Settings ***
Resource    ../libs/library.robot

Force Tags  GeneralPublic

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Go to Table Tool page
    [Tags]  HappyPath
    user goes to url  ${url}
    user checks element contains  css:body   Explore education statistics
    user clicks link   Create your own charts and tables online
    user waits until page contains   Create your own table

Select Pupil absence publication
    [Tags]  HappyPath
    user clicks element    css:[data-testid="Early years and schools"]
    user clicks element    css:[data-testid="Absence and exclusions"]
    user clicks element    css:[data-testid="Pupil absence"]
    user waits until page contains    2. Filter statistics from

Select start and end years
    [Tags]  HappyPath
    user selects from list by label   css:#filter-startYear   2012/13
    user selects from list by label   css:#filter-endYear   2015/16

Select School type Total
    [Tags]  HappyPath
    user clicks element  css:#filter-schoolTypes-total

Select a few Indicators
    [Tags]  HappyPath
    user clicks element   css:[data-testid="Absence fields"]

    user clicks element   css:#sess_authorised_percent
    user clicks element   css:#sess_overall_percent
    user clicks element   css:#sess_unauthorised_percent

Select a few Characteristics
    [Tags]  HappyPath
    user clicks element   css:[data-testid="Total"]
    user clicks element   css:input#Total

Generate a table
    [Tags]  HappyPath  Failing
    user clicks element   css:#submit-button
    user waits until page contains  Explore statistics from
    user checks element should contain  css:table   2012/13
    user checks element should contain  css:table   2013/14
    user checks element should contain  css:table   2013/14
    user checks element should not contain  css:table   2016/17
