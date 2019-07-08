*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify admin index page loads
    [Tags]  HappyPath
    user goes to url  ${urlAdmin}
    user waits until page contains  Index page for administrative application

Go to Admin dashboard
    [Tags]  HappyPath   UnderConstruction
    user clicks link       Administrators dashboard page

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
