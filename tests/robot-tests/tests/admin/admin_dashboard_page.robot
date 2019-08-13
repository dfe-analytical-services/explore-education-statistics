*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify admin index page loads
    [Tags]  HappyPath
    environment variable should be set   ADMIN_URL
    user goes to url  %{ADMIN_URL}
    user waits until page contains heading     Sign-in

Verify user can sign in
    [Tags]   HappyPath
    user clicks link   Sign-in

    environment variable should be set   ADMIN_EMAIL
    environment variable should be set   ADMIN_PASSWORD
    user logs into microsoft online  %{ADMIN_EMAIL}   %{ADMIN_PASSWORD}

    user checks url contains  %{ADMIN_URL}
    user waits until page contains heading   User1 EESADMIN
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(1)     Home
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(2)     Administrator dashboard

Heading is present on tab
    [Tags]  HappyPath
    user checks element contains  css:#my-publications-tab  Manage publications and releases
    
Correct information is shown on tabs
    [Tags]   HappyPath
    user clicks element   css:#draft-releases-tab
    user checks element contains  css:#draft-releases-tab  View draft releases
    user clicks element   css:#scheduled-releases-tab
    user checks element contains  css:#scheduled-releases-tab  View scheduled releases

Verify correct data is shown when theme and topic is shown
    [Tags]   HappyPath
    user clicks element   css:#my-publications-tab
    select from list by label  css:#selectTheme  Finance and funding
    select from list by label  css:#selectTopic  Local authority and school finance
    user checks page contains accordion  Income and expenditure in academies in England
    user checks accordion section contains text  Income and expenditure in academies in England    Methodology
    user checks accordion section contains text  Income and expenditure in academies in England    Releases
        
Validate accordion sections order
    [Tags]  HappyPath
    user checks accordion is in position  Income and expenditure in academies in England            1
    user checks accordion is in position  LA and school expenditure                                 2
    user checks accordion is in position  Planned LA and school expenditure                         3