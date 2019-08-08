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
    user waits until page contains element    xpath://h1[text()="Sign-in"]

Verify user can sign in
    [Tags]   HappyPath
    user clicks link   Sign-in

    user waits until page contains element  xpath://div[text()="Sign in"]
    environment variable should be set   ADMIN_EMAIL
    user presses keys     %{ADMIN_EMAIL}
    user waits until page contains element    css:input[value="Next"]
    user clicks element   css:input[value="Next"]

    user waits until page contains element  xpath://div[text()="Enter password"]
    environment variable should be set   ADMIN_PASSWORD
    user presses keys     %{ADMIN_PASSWORD}
    user waits until page contains element    css:input[value="Sign in"]
    user clicks element   css:input[value="Sign in"]

    user waits until page contains element  xpath://div[text()="Stay signed in?"]
    user waits until page contains element    css:input[value="No"]
    user clicks element   css:input[value="No"]

    user checks url contains  %{ADMIN_URL}
    user checks page contains element  xpath://h1[text()="User1 EESADMIN"]
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(1)     Home
    user checks element should contain    css:[data-testid="breadcrumbs--list"] li:nth-child(2)     Administrator dashboard

Go to edit page for current release of Pupil absence statistics
    [Tags]  HappyPath     UnderConstruction
    user clicks accordion section   Pupil absence statistics and data for schools in England
    user clicks element child containing text  css:#schools-content-1 li:nth-child(1)    Edit current release
    user waits until page contains  Edit pupil absence statistics

Validate "Click to edit" tags are appearing
    [Tags]  HappyPath     UnderConstruction
    elements containing text should match x times  Click to edit      15

Edit headline
    [Tags]  HappyPath     UnderConstruction
    user clicks element containing text  Pupil absence statistics and data for schools in England
    user deletes text from element until block is empty  css:h2  Pupil absence statistics and data for schools in England
    user presses keys    abcdefghijklmopqrstuvwxyz
    italic x characters before cursor  5
    insert image
    
    
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
        user checks accordion section contains details  Income and expenditure in academies in England    Methodology
        user checks accordion section contains details  Income and expenditure in academies in England    Releases
        
Validate accordion sections order
            [Tags]  HappyPath
            user checks accordion is in position  Income and expenditure in academies in England            1
            user checks accordion is in position  LA and school expenditure                                 2
            user checks accordion is in position  Planned LA and school expenditure                         3