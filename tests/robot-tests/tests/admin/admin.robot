*** Settings ***
Resource    ../libs/library.robot

Force Tags  Admin

Suite Setup       user opens the browser
Suite Teardown    user closes the browser

*** Test Cases ***
Verify Admin Page Loads
    [Tags]  HappyPath
    user goes to url  ${urlAdmin}
    user waits until page contains   Administrator dashboard

Edit current release of Pupil absence statistics
    [Tags]  HappyPath
    user clicks button  Absence and exclusions
    element attribute value should be  css:#schools-heading-1   aria-expanded   true
    user clicks element  xpath://div[@id='schools-content-1']//li[1]//a[contains(text(), 'Edit current release')]
    user waits until page contains  Edit pupil absence statistics
