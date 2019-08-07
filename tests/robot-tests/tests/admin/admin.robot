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
    [Tags]   HappyPath     UnderConstruction
    user clicks link   Sign-in

    user waits until page contains element  xpath://div[text()="Sign in"]
    environment variable should be set   ADMIN_EMAIL
    user presses keys     %{ADMIN_EMAIL}
    user clicks element   css:input[value="Next"]

    user waits until page contains element  xpath://div[text()="Enter password"]
    environment variable should be set   ADMIN_PASSWORD
    user presses keys     %{ADMIN_PASSWORD}
    user clicks element   css:input[value="Sign in"]

    user waits until page contains element  xpath://div[text()="Stay signed in?"]
    user clicks element   css:input[value="No"]

    user checks url contains  %{ADMIN_URL}
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
